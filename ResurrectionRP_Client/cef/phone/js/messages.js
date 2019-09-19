app.controller("MessagesCtrl", function ($scope) {
    $scope.conversations = [];

    window.addEventListener("ConversationsReturned", ev => {
        $scope.$apply(function () {
            //console.log(ev.detail);
            $scope.conversations = JSON.parse(ev.detail);
        });
    });
});

$(() => {
    if ('alt' in window) {
        alt.on("loadConversations", loadConversations);
        alt.on("loadMessages", loadMessages);
        alt.emit("getConversations");
    }
});

function loadConversations(conversations) {
    var event = new CustomEvent("ConversationsReturned", { "detail": conversations });
    window.dispatchEvent(event);
}

app.controller("MessageViewCtrl", function ($scope) {

    $scope.messages = [];

    window.addEventListener("MessagesReturned", ev => {
        $scope.$apply(function () {
            $scope.conversationId = ev.detail.convId;
            $scope.messages = ev.detail.messages;
            $("#messages-container").animate({ scrollTop: $(document).height() }, 'slow');
        });
    });

    let urlParams = (new URL(document.location)).searchParams;

    $scope.conversationName = urlParams.get("convName");
    $scope.conversationId = urlParams.get("convId");
    $scope.receiverNumber = urlParams.get("number");
    $scope.knownContact = parseInt(urlParams.get("knownContact"));

    $scope.originalNumber = $scope.phoneNumber;

    $scope.sendMessage = function () {
        alt.emit("sendMessage", $scope.receiverNumber, $scope.messageBox);
        $scope.messageBox = null;
    };

    canClose = () => {
        alt.emit("CanClose", false);
    };

    $(function () {
        alt.emit("getMessages", $scope.conversationId, $scope.receiverNumber);
    });
    
    window.onload = function () {
        var objDiv = document.getElementById("messages-container");
        objDiv.scrollTop = objDiv.scrollHeight;
    };
});

function loadMessages(response) {
    response = JSON.parse(response);
    var event = new CustomEvent("MessagesReturned", { "detail": { conversationId: response.convId, messages: response.messages}});
    window.dispatchEvent(event);
}

app.controller("MsgOptionsCtrl", function ($scope) {
    let urlParams = (new URL(document.location)).searchParams;
    $scope.contactName = urlParams.get("contactName");
    $scope.contactNumber = urlParams.get("contactNumber");
    $scope.conversationId = urlParams.get("convId");

    if ($scope.contactName != "" && $scope.contactName != null) {
        $scope.knownContact = true;
    } else {
        $scope.knownContact = false;
    }

    $scope.addContact = function () {
        let contact = {
            name: $scope.contactName,
            phone: $scope.contactNumber
        };

        alt.emit("addOrEditContact", JSON.stringify(contact));
    };

    $scope.deleteConversation = function (id) {
        alt.emit("deleteConversation", $scope.contactNumber);
    };
});