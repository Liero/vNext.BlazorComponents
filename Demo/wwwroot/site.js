(function () {
    document.addEventListener('click', (evt) => {
        /** @type HTMLElement */
        var element = evt.target;
        if (element.matches && element.matches('[data-toggle=popper][data-target]')) {
            var target = document.querySelector(element.getAttribute('data-target'));
            var visible = window.getComputedStyle(target).display != 'none'
            target.classList.toggle('show', !visible);
            target.classList.toggle('d-block', !visible);

            if (visible) {
                //trigger event so that we have a change to destroy popper object
                target.dispatchEvent(new Event('popper-hide'));
            }
            else {
                //create popper
                var popover = new Popper(element, target);

                //add placement class in order to show arrow
                [...target.classList]
                    .filter(c => c.startsWith('bs-popover-'))
                    .forEach(target.classList.remove)
                target.classList.add(`bs-popover-${popover.options.placement}`);



                //cleanup when popper is hidden;
                function onPopperHidden() {
                    popover.destroy();
                    target.removeEventListener('popper-hide', onPopperHidden);
                };
                target.addEventListener('popper-hide', onPopperHidden);
            }
        }
    });
})();