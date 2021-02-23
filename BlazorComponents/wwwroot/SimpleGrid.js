"use strict";
var vNext;
(function (vNext) {
    function initGrid(elementRef, dotNetRef) {
        return new SimpleGrid(elementRef, dotNetRef)
    }
    vNext.initGrid = initGrid;

    class SimpleGrid {
        constructor(elementRef, dotNetRef) {
            this.elementRef = elementRef;
            this.dotNetRef = dotNetRef;
        }
    }
})(vNext || (vNext = {}));