app.controller("PhoneSettingsCtrl", function ($scope, $rootScope) {
    $scope.saveSettings = () => {
        console.log($rootScope.phoneSettings);
        localStorage.setItem("phoneSettings", JSON.stringify($rootScope.phoneSettings));
        alt.emit("SavePhoneSettings", JSON.stringify($rootScope.phoneSettings));
    };
});