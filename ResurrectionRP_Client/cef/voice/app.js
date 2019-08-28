let pluginAddress = "127.0.0.1:8088";
let isConnected = false;
let serverUniqueIdentifierFilter = null;

// Packet Stats
let packetsSent = 0;
let packetsReceived = 0;
let lastCommand = "";

function connect() {
    try
    {
        window.webSocket = new WebSocket(`ws://${pluginAddress}/`);
    }
    catch (ex) {
        console.log(ex);
    }


    webSocket.onmessage = function (evt) {
        let object = JSON.parse(evt.data);

        if (typeof serverUniqueIdentifierFilter === "string") {
            if (object.ServerUniqueIdentifier === serverUniqueIdentifierFilter)
                alt.emit("SaltyChat_OnMessage", evt.data);
            else if (typeof object.ServerUniqueIdentifier === "undefined")
                alt.emit("SaltyChat_OnError", evt.data);
        }
        else {
            if (typeof object.ServerUniqueIdentifier === "string")
                alt.emit("SaltyChat_OnMessage", evt.data);
            else
                alt.emit("SaltyChat_OnError", evt.data);
        }

        packetsReceived++;
        updateHtml();
    };

    webSocket.onopen = function () {
        isConnected = true;

        alt.emit("SaltyChat_OnConnected");
    };

    webSocket.onclose = function () {
        isConnected = false;

        alt.emit("SaltyChat_OnDisconnected");

        connect();
    }

    webSocket.onerror = function (error) {
        console.error(error);
    };
}

function setWebSocketAddress(address) {
    if (typeof address === "string")
        pluginAddress = address;
}

function setServerUniqueIdentifierFilter(serverUniqueIdentifier) {
    if (typeof serverUniqueIdentifier === "string")
        serverUniqueIdentifierFilter = serverUniqueIdentifier;
}

function runCommand(command) {
    if (!isConnected || typeof command !== "string")
        return;

    webSocket.send(command);

    packetsSent++;
    lastCommand = command;
    updateHtml();
}

function updateHtml() {
    //$("#demo").html(`Last Command: ${lastCommand}</br>Packets Sent: ${packetsSent}</br>Packets Received ${packetsReceived}`);
}

document.addEventListener("DOMContentLoaded", function () {
    connect();
    updateHtml();
});