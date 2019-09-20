app.controller("CallCtrl", function ($scope) {
    let urlParams = (new URL(document.location)).searchParams;
    $scope.inputNumber = (urlParams.get("number") == null ? "" : urlParams.get("number"));

    $scope.addNumber = function (number) {
        $scope.inputNumber += number.toString();
        $scope.checkFormat();
    };

    $scope.remove = function () {
        $scope.inputNumber = $scope.inputNumber.slice(0, -1);
    };

    $scope.checkFormat = function () {
        var formatChecker = /^[0-9]{0,4}-?[0-9]{0,4}$/;
        if (!$scope.inputNumber.match(formatChecker)) {
            $scope.inputNumber = $scope.inputNumber.slice(0, -1);
        }

        if ($scope.inputNumber.length === 4) {
            $scope.inputNumber += "-";
        }
    };

    $scope.keyDown = function (e) {
        if (e.keyCode === 8) {
            e.preventDefault();
            $scope.inputNumber = $scope.inputNumber.slice(0, -1);
        }
    };

    $scope.callNumber = function () {
        alt.emit("initiateCall", $scope.inputNumber);
    };
});

app.controller("OnCallCtrl", function ($scope, $interval, $timeout) {
    let urlParams = (new URL(document.location)).searchParams;
    
    $scope.calledNumber = urlParams.get("number");
    $scope.contactName = (urlParams.get("name") == null ? "Inconnu" : urlParams.get("name"));
    $scope.incomingCall = (urlParams.get("incomingCall"));
    console.log($scope.incomingCall);
    $scope.callStatus = ($scope.incomingCall ? "Appel entrant..." : "Appel...");

    $scope.callStart = null;
    $scope.timerInterval = null;

    $scope.callStarted = function () {
        $scope.callStart = moment();
        $scope.callStatus = "00:00";
        $scope.timerInterval = $interval($scope.callTimerUpdate, 1000);
    };

    $scope.callTimerUpdate = function () {
        let diffSec = moment().diff($scope.callStart, 'seconds');
        let diffMinutes = 0;

        while (diffSec >= 60) {
            diffMinutes++;
            diffSec -= 60;
            if ($scope.timerInterval == null)
                return;
        }

        $scope.callStatus = addZeros(diffMinutes) + ":" + addZeros(diffSec);
    };

    $scope.callEnded = function () {
        if ($scope.timerInterval != null) {
            $interval.cancel($scope.timerInterval);
            $scope.timerInterval = null;
        }

        $scope.$apply(() => {
            $scope.callStatus = "Fin d'appel...";
        });

        $timeout(() => {
            window.location.href = "call.html?number=" + $scope.calledNumber;
        }, 3000);
    };

    $scope.acceptCall = function () {
        alt.emit("acceptCall", $scope.calledNumber);
    };

    $scope.cancelCall = function () {
        alt.emit("cancelCall", $scope.calledNumber);
    };

    $scope.endCall = function () {
        alt.emit("endCall", $scope.calledNumber);
    };

    window.addEventListener("OnCallEvent", ev => {
        if (ev.detail == "started") {
            $scope.callStarted();
        } else if (ev.detail == "canceled") {
            $scope.callEnded();
        } else if (ev.detail == "ended") {
            $scope.callEnded();
        }
    });
});

let addZeros = number => (parseInt(number) < 10 ? "0" + number : number);

window.addEventListener('load', function () {
    if ('alt' in window) {
        alt.on("callEvent", callEvent);
    }
});
function callEvent(event) {
    event = new CustomEvent("OnCallEvent", { "detail": event });
    window.dispatchEvent(event);
};