namespace vNext {
    export class SimpleGrid {
        gridElement: HTMLElement;
        customEvent: HTMLInputElement;

        constructor(
            public elementRef: HTMLDivElement,
            private dotNetRef: any) {

            elementRef.addEventListener('mousedown', evt => {
                /** @type HTMLElement */
                var target = evt.target as HTMLElement;
                if (target.matches('.sg-header-cell-resize')) {
                    evt.stopPropagation();
                    this.startResize(evt);
                }
                if (evt.shiftKey) {
                    if (target.matches('input')) {
                        target.focus();
                    }
                    const cancelSelection = (evt2: Event) => {
                        evt2.preventDefault();
                    }
                    elementRef.addEventListener('selectstart', cancelSelection);
                    setTimeout(() => elementRef.removeEventListener('selectstart', cancelSelection));
                }

                if (target.matches('.sg-header-cell') && evt.shiftKey) {
                    evt.preventDefault()
                }
            });

            //workaround to fix https://github.com/dotnet/aspnetcore/issues/34060
            elementRef.addEventListener('scroll', evt => {
                elementRef.style.height = elementRef.offsetHeight + 'px';
            });

            this.customEvent = elementRef.querySelector('input[type=\'hidden\']');
            this.gridElement = elementRef.querySelector('.simple-grid') as HTMLElement;

            this.gridElement.addEventListener('focusout', evt => {
                var target = evt.target as HTMLElement;
                if (this.isCell(target)) {
                    target.setAttribute('tabindex', '-1');

                    //try to maintain focus on virtual scrolling
                    if (document.activeElement == document.body) {
                        const pos = SimpleGrid.getCellIndex(target);
                        const rowChildIndex = Array.prototype.indexOf.call(target.parentElement.children, target);
                        setTimeout(() => {
                            if (document.activeElement != document.body || !this.isVisible()) {
                                return;
                            }
                            if (target.isConnected) {
                                target.focus({ preventScroll: true });
                            }
                            else {
                                const newCell = this.gridElement.querySelector<HTMLElement>(`.sg-row[data-row-index=\'${pos.rowIndex}\']>:nth-child(${pos.colIndex + 1})`)
                                    || this.gridElement.querySelector<HTMLElement>(`.sg-row[data-row-index]:nth-child(${rowChildIndex})>:nth-child(${pos.colIndex + 1})`)
                                    || this.gridElement.querySelector<HTMLElement>(`.sg-row[data-row-index]>:first-child`);

                                (newCell || this.gridElement).focus({ preventScroll: true });
                            }
                        }, 0);
                    }
                }
            });
            this.gridElement.addEventListener('focusin', evt => {
                var target = evt.target as Element;
                if (this.isCell(target)) {
                    target.setAttribute('tabindex', '0');
                }
            });
            this.gridElement.addEventListener('click', event => {
                var target = event.target as HTMLElement;
                if (!this.isCell(target)) return;
                target.focus();
            });
            this.gridElement.addEventListener('keypress', event => {
                var target = event.target as HTMLElement;
                if (!this.isCell(target) || event.shiftKey || event.ctrlKey || event.altKey) return;
                switch (event.key) {
                    case "Delete":
                        event.preventDefault();
                        break;
                }
            }, { capture: true });
            this.gridElement.addEventListener('keydown', event => {
                var target = event.target as HTMLElement;
                if (event.shiftKey || event.ctrlKey || event.altKey) return;
                if (!this.isCell(target) && target != this.gridElement) return;

                switch (event.key) {
                    case "ArrowRight":
                        this.moveFocus(1, 0);
                        break;
                    case "ArrowLeft":
                        this.moveFocus(-1, 0);
                        break;
                    case "ArrowDown":
                        this.moveFocus(0, 1);
                        break;
                    case "ArrowUp":
                        this.moveFocus(0, -1);
                        break;
                    case " ":
                        target.click();
                        break;
                    case "Enter":
                        (target.firstElementChild instanceof HTMLElement ? target.firstElementChild : target).click();
                        break;
                    case "Delete":
                        this.customEvent.value = JSON.stringify({ name: 'Delete', ...SimpleGrid.getCellIndex(target) });
                        this.customEvent.dispatchEvent(new CustomEvent<Event>('change', { bubbles: true }));
                        break;
                    default:
                        return;
                }
                event.stopPropagation();
                event.preventDefault();
            })
        }

        private isVisible(): boolean {
            return this.gridElement.clientHeight > 0;
        }

        private startResize(evt: MouseEvent) {
            /** @type HTMLElement */
            var dragHandle = evt.target as HTMLElement;
            const x = evt.clientX;
            /** @type HTMLElement */
            const colElem = dragHandle.closest<HTMLElement>('.sg-header-cell');
            var columns = Array.from(colElem.parentElement.children) as HTMLElement[];
            const colIndex = columns.indexOf(colElem);
            const initialWidth = colElem.offsetWidth;
            let columnWidths = columns.map(c => c.offsetWidth);

            /**@param {MouseEvent} e  */
            let move = e => {
                e.preventDefault();
                var diff = e.clientX - x;
                columnWidths[colIndex] = initialWidth + diff;
                this.gridElement.style['grid-template-columns'] = columnWidths.map(c => `${c}px`).join(' ');
            }

            let stop = (e) => {
                document.removeEventListener('mousemove', move);
                this.dotNetRef.invokeMethodAsync('OnResizeInterop', colIndex, columnWidths);
            }
            document.addEventListener('mousemove', move);
            document.addEventListener('mouseup', stop, { once: true });
            document.addEventListener('click', e => { e.stopPropagation(); e.preventDefault(); }, { once: true, capture: true });
        }

        scrollToIndex(index, behavior) {
            var rowHeight = this.elementRef.querySelector<HTMLElement>('.sg-cell').offsetHeight;
            this.elementRef.querySelector('.simple-grid').scrollTo({
                behavior: behavior || 'smooth',
                top: index * rowHeight,
            });
        }

        moveFocus(horizontal: number, vertical: number) {
            if (!this.isCell(document.activeElement)) {
                return;
            }
            var pos = SimpleGrid.getCellIndex(document.activeElement);
            pos.colIndex += horizontal;
            pos.rowIndex += vertical;
            var newCell = this.gridElement.querySelector<HTMLElement>(`.sg-row[data-row-index=\'${pos.rowIndex}\']>:nth-child(${pos.colIndex + 1})`);
            newCell?.focus();
        }

        isCell(element: Element) {
            return element.matches('.sg-cell') || element?.parentElement.parentElement == this.gridElement;
        }

        static init(elementRef, dotNetRef) {
            return new SimpleGrid(elementRef, dotNetRef)
        }

        /**
         * get colIndex and rowIndex from client coordinates.
         * @param {Object} args - Typically a MouseEvent.
         * @param {number} args.clientX
         * @param {number} args.clientY
         */
        static getCellFromPoint({ clientX, clientY }) {
            const cell = document.elementsFromPoint(clientX, clientY).find(e => e.matches('.sg-cell'));
            if (!cell) {
                return null;
            }
            var { colIndex, rowIndex } = SimpleGrid.getCellIndex(cell);
            return [colIndex, rowIndex];
        }

        /**
         * @param cell must be  '.sg-cell'
         */
        private static getCellIndex(cell: Element)
            : { colIndex: number, rowIndex: number } {
            const colIndex: number = Array.prototype.indexOf.call(cell.parentNode.children, cell);
            const rowIndex: number = +cell.parentElement.getAttribute('data-row-index');
            return { colIndex, rowIndex };
        }
    }
}