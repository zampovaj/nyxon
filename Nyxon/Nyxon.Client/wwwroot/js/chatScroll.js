window.chatScroll = {
    isAtBottom: function (elementId, threshold = 20) {
        const el = document.getElementById(elementId);
        if (!el) return false;

        return el.scrollHeight - el.scrollTop - el.clientHeight < threshold;
    },
    scrollToBottom: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) {
            el.scrollTop = el.scrollHeight;
        }
    },
    isNearTop: function (elementId, threshold = 80) {
        const el = document.getElementById(elementId);
        if (!el) return false;

        return el.scrollTop < threshold;
    },
}