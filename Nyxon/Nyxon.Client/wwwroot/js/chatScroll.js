window.chatScroll = {
    isUserAtBottom: (containerId) => {
        const container = document.getElementById(containerId);
        const threshold = 50; // px from bottom
        return (container.scrollHeight - container.scrollTop - container.clientHeight) < threshold;
    },
    scrollToBottom: (containerId) => {
        const container = document.getElementById(containerId);
        container.scrollTo({
            top: container.scrollHeight,
            behavior: 'smooth'
        });
    }
};