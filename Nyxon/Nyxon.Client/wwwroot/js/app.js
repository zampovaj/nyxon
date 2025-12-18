window.terminalHotKeyManager = {
    currentHandler: null,
    register: function (elementId) {
        this.dispose();

        this.currentHandler = function (e) {
            if (e.key === '/') {
                // check if user is already typing -> if yes, return
                const activeTag = document.activeElement ? document.activeElement.tagName.toLowerCase() : "";
                if (activeTag === 'input' || activeTag === 'textarea') return;

                e.preventDefault();

                const el = document.getElementById(elementId);
                if (el) el.focus();
            }
        };
        window.addEventListener('keydown', this.currentHandler);

    },
    dispose: function () {
        if (this.currentHandler) {
            window.removeEventListener('keydown', this.currentHandler);
            this.currentHandler = null;
        }
    }
};