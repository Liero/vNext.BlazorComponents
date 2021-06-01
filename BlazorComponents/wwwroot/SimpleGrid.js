"use strict";
var vNext;
(function (vNext) {
    class SimpleGrid {
        /** 
         *  @param { HTMLDivElement } elementRef
         */
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
                    function cancelSelection(evt2) {
                        evt2.preventDefault();
                    }
                    elementRef.addEventListener('selectstart', cancelSelection);
                    setTimeout(() => elementRef.removeEventListener('selectstart', cancelSelection));
                }

                if (target.matches('.sg-header-cell') && evt.shiftKey) {
                    evt.preventDefault()
                }
            });
            /** @type HTMLElement */
            this.gridElement = elementRef.firstChild
        }

        /**
         * @param {MouseEvent} evt
         */
        startResize(evt) {
            /** @type HTMLElement */
            var dragHandle = evt.target;
            const x = evt.clientX;
            /** @type HTMLElement */
            const colElem = dragHandle.closest('.sg-header-cell');
            var columns = [...colElem.parentElement.children];
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
                return -1;
            }
            const colIndex = Array.prototype.indexOf.call(cell.parentNode.children, cell);
            const rowIndex = +cell.parentElement.getAttribute('data-row-index');
            return [colIndex, rowIndex];
        }
    }

    vNext.SimpleGrid = SimpleGrid;
})(vNext || (vNext = {}));