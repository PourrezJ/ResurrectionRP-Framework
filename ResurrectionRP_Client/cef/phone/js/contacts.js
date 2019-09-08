app.controller("NewContactCtrl", function ($scope) {
    alt.emit('CanClose', false);
    $scope.addContact = function () {
        let contact = {
            originalNumber: $scope.originalNumber,
            contactName: $scope.contactName,
            phoneNumber: $scope.phoneNumber
        };
        alt.emit('CanClose', true);
        alt.emit("AddOrEditContact", JSON.stringify(contact));
    };

    $scope.removeContact = function () {
        alt.emit("RemoveContact", $scope.phoneNumber);
    };

    let urlParams = (new URL(document.location)).searchParams;
    $scope.contactName = urlParams.get("name");
    $scope.phoneNumber = urlParams.get("phone");

    $scope.originalNumber = $scope.phoneNumber;
});

app.controller("ContactsCtrl", function ($scope) {
    $scope.contacts = [];

    window.addEventListener("ContactsReturned", ev => {
        $scope.$apply(function () {
            //$scope.contacts = JSON.parse(ev.detail);
            $scope.contacts = ev.detail;
        });
    });
});

function loadContacts(contacts) {
    var event = new CustomEvent("ContactsReturned", { "detail": JSON.parse(contacts)});
    window.dispatchEvent(event);
}

$(() => {
    if ('alt' in window) {
        alt.on("loadContacts", loadContacts);
        alt.emit("GetContacts");        
    }
});
