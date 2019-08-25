angular.module("ResurrectionInteractionMenu", []).controller("MenuCtrl", function ($scope, $timeout) {
    $scope.buttons = [];

    let u = new URL(document.location.href);
    let menu = JSON.parse(u.searchParams.get("params"));

    $scope.menuId = menu.Id;
    $scope.buttons = menu.Items;

    $scope.selected = {
        name: null,
        desc: null
    };

    $scope.displayAction = index => {
        if ($scope.buttons[index] === undefined) {
            $scope.emptyAction();
            return;
        }

        $scope.selected = {
            name: $scope.buttons[index].Text,
            desc: $scope.buttons[index].Description
        };
    };

    $scope.emptyAction = () => {
        $scope.selected = {
            name: null,
            desc: null
        };
    };

    $scope.sendAction = index => {
        if ($scope.buttons[index] !== undefined)
            alt.emit("XMenuManager_Callback", index);
    };
});