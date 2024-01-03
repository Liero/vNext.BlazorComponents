var vNext;
(function (vNext) {
    class SimpleGrid {
        constructor(elementRef, dotNetRef) {
            this.elementRef = elementRef;
            this.dotNetRef = dotNetRef;
            elementRef.addEventListener('mousedown', evt => {
                /** @type HTMLElement */
                var target = evt.target;
                if (target.matches('.sg-header-cell-resize')) {
                    evt.stopPropagation();
                    this.startResize(evt);
                }
                if (evt.shiftKey) {
                    if (target.matches('input')) {
                        target.focus();
                    }
                    const cancelSelection = (evt2) => {
                        evt2.preventDefault();
                    };
                    elementRef.addEventListener('selectstart', cancelSelection);
                    setTimeout(() => elementRef.removeEventListener('selectstart', cancelSelection));
                }
                if (target.matches('.sg-header-cell') && evt.shiftKey) {
                    evt.preventDefault();
                }
            });
            //workaround to fix https://github.com/dotnet/aspnetcore/issues/34060
            elementRef.addEventListener('scroll', evt => {
                elementRef.style.height = elementRef.offsetHeight + 'px';
            });
            this.customEvent = elementRef.querySelector('input[type=\'hidden\']');
            this.gridElement = elementRef.querySelector('.simple-grid');
            this.gridElement.addEventListener('focusout', evt => {
                var target = evt.target;
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
                                const newCell = this.gridElement.querySelector(`.sg-row[data-row-index=\'${pos.rowIndex}\']>:nth-child(${pos.colIndex + 1})`)
                                    || this.gridElement.querySelector(`.sg-row[data-row-index]:nth-child(${rowChildIndex})>:nth-child(${pos.colIndex + 1})`)
                                    || this.gridElement.querySelector(`.sg-row[data-row-index]>:first-child`);
                                (newCell || this.gridElement).focus({ preventScroll: true });
                            }
                        }, 0);
                    }
                }
            });
            this.gridElement.addEventListener('focusin', evt => {
                var target = evt.target;
                if (this.isCell(target)) {
                    target.setAttribute('tabindex', '0');
                }
            });
            this.gridElement.addEventListener('click', event => {
                var target = event.target;
                if (!this.isCell(target))
                    return;
                target.focus();
            });
            this.gridElement.addEventListener('keypress', event => {
                var target = event.target;
                if (!this.isCell(target) || event.shiftKey || event.ctrlKey || event.altKey)
                    return;
                switch (event.key) {
                    case "Delete":
                        event.preventDefault();
                        break;
                }
            }, { capture: true });
            this.gridElement.addEventListener('keydown', event => {
                var target = event.target;
                if (event.shiftKey || event.ctrlKey || event.altKey)
                    return;
                if (!this.isCell(target) && target != this.gridElement)
                    return;
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
                        this.customEvent.value = JSON.stringify(Object.assign({ name: 'Delete' }, SimpleGrid.getCellIndex(target)));
                        this.customEvent.dispatchEvent(new CustomEvent('change', { bubbles: true }));
                        break;
                    default:
                        return;
                }
                event.stopPropagation();
                event.preventDefault();
            });
        }
        isVisible() {
            return this.gridElement.clientHeight > 0;
        }
        startResize(evt) {
            /** @type HTMLElement */
            var dragHandle = evt.target;
            const x = evt.clientX;
            /** @type HTMLElement */
            const colElem = dragHandle.closest('.sg-header-cell');
            var columns = Array.from(colElem.parentElement.children);
            const colIndex = columns.indexOf(colElem);
            const initialWidth = colElem.offsetWidth;
            let columnWidths = columns.map(c => c.offsetWidth);
            /**@param {MouseEvent} e  */
            let move = e => {
                e.preventDefault();
                var diff = e.clientX - x;
                columnWidths[colIndex] = initialWidth + diff;
                this.gridElement.style['grid-template-columns'] = columnWidths.map(c => `${c}px`).join(' ');
            };
            let stop = (e) => {
                document.removeEventListener('mousemove', move);
                this.dotNetRef.invokeMethodAsync('OnResizeInterop', colIndex, columnWidths);
            };
            document.addEventListener('mousemove', move);
            document.addEventListener('mouseup', stop, { once: true });
            document.addEventListener('click', e => { e.stopPropagation(); e.preventDefault(); }, { once: true, capture: true });
        }
        scrollToIndex(index, behavior) {
            var rowHeight = this.elementRef.querySelector('.sg-cell').offsetHeight;
            this.elementRef.querySelector('.simple-grid').scrollTo({
                behavior: behavior || 'smooth',
                top: index * rowHeight,
            });
        }
        moveFocus(horizontal, vertical) {
            if (!this.isCell(document.activeElement)) {
                return;
            }
            var pos = SimpleGrid.getCellIndex(document.activeElement);
            pos.colIndex += horizontal;
            pos.rowIndex += vertical;
            var newCell = this.gridElement.querySelector(`.sg-row[data-row-index=\'${pos.rowIndex}\']>:nth-child(${pos.colIndex + 1})`);
            newCell === null || newCell === void 0 ? void 0 : newCell.focus();
        }
        isCell(element) {
            return element.matches('.sg-cell') || (element === null || element === void 0 ? void 0 : element.parentElement.parentElement) == this.gridElement;
        }
        static init(elementRef, dotNetRef) {
            return new SimpleGrid(elementRef, dotNetRef);
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
        static getCellIndex(cell) {
            const colIndex = Array.prototype.indexOf.call(cell.parentNode.children, cell);
            const rowIndex = +cell.parentElement.getAttribute('data-row-index');
            return { colIndex, rowIndex };
        }
    }
    vNext.SimpleGrid = SimpleGrid;
})(vNext || (vNext = {}));
//# sourceMappingURL=SimpleGrid.js.map