let pluginAddress = "127.0.0.1:8088";
let isConnected = false;

// Packet Stats
let packetsSent = 0;
let packetsReceived = 0;
let lastCommand = "";

function connect() {
    try {
       window.webSocket = new WebSocket(`ws://${pluginAddress}/`);
    } catch
    {
        // no warning please!
        return;
    }
    

    webSocket.onmessage = function (evt) {
        let object = JSON.parse(evt.data);

        if (typeof object.ServerUniqueIdentifier === "string") {
            alt.emit("SaltyChat_OnMessage", evt.data);
        }
        else
        {
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
    };

    webSocket.onerror = function (error) {
        //console.error(error);
    };
}

function setWebSocketAddress(address) {
    if (typeof address === "string")
        pluginAddress = address;
}

function runCommand(command) {
    if (!isConnected || typeof command !== "string")
        return;

    //console.log(command);

    webSocket.send(command);

    packetsSent++;
    lastCommand = command;
    updateHtml();
}

function updateHtml() {
    //$("#demo").html(`Last Command: ${lastCommand}</br>Packets Sent: ${packetsSent}</br>Packets Received ${packetsReceived}`);
}

$(() => {
    if ('alt' in window) {
        alt.on('runCommand', runCommand);

        connect();
        updateHtml();
    }
});
