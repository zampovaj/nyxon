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

window.domUtils = {
    blurElement: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) el.blur();
    },
    
    focusElement: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) el.focus();
    }
};
window.clipboardCopy = {
    copyText: function (text) {
        // 1. Try modern API (if HTTPS or Localhost)
        if (navigator.clipboard && navigator.clipboard.writeText) {
            return navigator.clipboard.writeText(text);
        }
        // 2. Fallback for HTTP (Development)
        else {
            return new Promise((resolve, reject) => {
                var textArea = document.createElement("textarea");
                textArea.value = text;

                // Hide it but keep it part of the DOM
                textArea.style.position = "fixed";
                textArea.style.left = "-9999px";
                textArea.style.top = "0";
                document.body.appendChild(textArea);

                textArea.focus();
                textArea.select();

                try {
                    var successful = document.execCommand('copy');
                    if (successful) resolve();
                    else reject("Copy command failed");
                } catch (err) {
                    reject(err);
                } finally {
                    document.body.removeChild(textArea);
                }
            });
        }
    }
};
