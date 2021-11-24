/// <reference path="Virtualize6.ts" />
namespace vNext {
    export class SimpleGrid {
        gridElement: HTMLElement;

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

            this.gridElement = elementRef.firstChild as HTMLElement;
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

        static init(elementRef, dotNetRef) {
            return new SimpleGrid(elementRef, dotNetRef)
        }

        /**
         * get colIndex and rowIndex from client coordinates.
         * @param {Object} args - Typically a MouseEvent.
         * @param {number} args.clientX
         * @param {number} args.clientY
         * @returns {Array<Number>}
         */
        static getCellFromPoint({ clientX, clientY }) {
            const cell = document.elementsFromPoint(clientX, clientY).find(e => e.matches('.sg-cell'));
            if (!cell) {
                return null;
            }
            const colIndex = Array.prototype.indexOf.call(cell.parentNode.children, cell);
            const rowIndex = +cell.parentElement.getAttribute('data-row-index');
            return [colIndex, rowIndex];
        }
    }
}