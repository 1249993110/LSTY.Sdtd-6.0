if (window.top === window.self) {
    const sdtdConsole = {
        webSocketPath: 'ws://' + window.location.host + '/ws',
        connection: null,
        timer: null,
        startTimer: function () {
            if (sdtdConsole.timer) {
                clearTimeout(sdtdConsole.timer);
            }
            sdtdConsole.timer = setTimeout(sdtdConsole.initWebsocket, 5000);
        }
    };

    if (!!top.accessToken) {
        sdtdConsole.webSocketPath += '?access-token=' + top.accessToken;
    }

    sdtdConsole.initWebsocket = function () {
        top.Vue.prototype.$EventBus.$emit('on_webSocket_connecting');

        var connection = new WebSocket(sdtdConsole.webSocketPath);
        connection.onopen = () => {
            top.Vue.prototype.$EventBus.$emit('on_webSocket_open');
        }
        connection.onmessage = (e) => {
            const message = JSON.parse(e.data);
            //console.log(message);
            switch (message.messageType) {
                // ConsoleLog
                case 0:
                    top.Vue.prototype.$EventBus.$emit('on_consoleLog', message.messageEntity);
                    break;
                // PlayerUpdate
                case 1:
                    top.Vue.prototype.$EventBus.$emit('on_playerUpdate', message.messageEntity);
                    break;
                // ChatMessage
                case 2:
                    top.Vue.prototype.$EventBus.$emit('on_chatMessage', message.messageEntity);
                    break;
            }
        }
        connection.onclose = (e) => {
            top.Vue.prototype.$EventBus.$emit('on_webSocket_close');
            sdtdConsole.startTimer();
        }
        connection.onerror = (e) => {
            top.Vue.prototype.$EventBus.$emit('on_webSocket_error');
        }

        sdtdConsole.connection = connection;
    }();

    top.sdtdConsole = sdtdConsole;
}