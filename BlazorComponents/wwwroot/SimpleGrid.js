"use strict";
var vNext;
(function (vNext) {
    function initGrid(elementRef, dotNetRef) {
        return new SimpleGrid(elementRef, dotNetRef)
    }
    vNext.initGrid = initGrid;

    class SimpleGrid {
        /** 
         *  @param { HTMLDivElement } elementRef
         */
        constructor(elementRef, dotNetRef) {
            this.elementRef = elementRef;
            this.dotNetRef = dotNetRef;

            elementRef.addEventListener('mousedown', evt => {
                /** @type Element */
                var target = evt.target;
                if (target.matches('.sg-header-cell-resize')) {
                    evt.stopPropagation();
                    this.startResize(evt);
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
                var diff = e.clientX - x;
                columnWidths[colIndex] = initialWidth + diff;
                this.gridElement.style['grid-template-columns'] = columnWidths.map(c => `${c}px`).join(' ');
            }

            let stop = (e) => {
                document.removeEventListener('mousemove', move);
                document.removeEventListener('mouseup', stop);
                this.dotNetRef.invokeMethodAsync('OnResizeInterop', colIndex, columnWidths);
            }
            document.addEventListener('mousemove', move);
            document.addEventListener('mouseup', stop);
        }
    }
})(vNext || (vNext = {}));