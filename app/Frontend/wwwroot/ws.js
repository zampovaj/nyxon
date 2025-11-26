window.createWebSocket = (url, dotnetHelper) => {
    const socket = new WebSocket(url);

    socket.onmessage = (event) => {
        dotnetHelper.invokeMethodAsync('ReceiveMessage', event.data);
    };

    return socket;
};

window.sendMessage = (socket, message) => {
    socket.send(message);
};
