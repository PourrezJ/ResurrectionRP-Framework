var app = angular.module("ResurrectionPhone", ['angularMoment']);
let urlParams = (new URL(document.location)).searchParams;

app.config(['$compileProvider', function ($compileProvider) {
    $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|package):/);
    $compileProvider.imgSrcSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|package):/);
}]);

app.run(function ($rootScope, amMoment) {
    amMoment.changeLocale('fr');
    $rootScope.phoneSettings = JSON.parse(localStorage.getItem("phoneSettings"));
    console.log($rootScope.phoneSettings);
    $rootScope.newMessagesCount = (urlParams.get("newMessages") == null ? 0 : parseInt(urlParams.get("newMessages")));
    alt.emit('CanClose', true);
});

app.controller("HomeCtrl", ['$scope', 'moment', function ($scope, moment) {

}]);

function loadPhoneSettings(settings) {
    localStorage.setItem("phoneSettings", settings);
}

app.filter("moment", () => (val, format) => moment(val).format(format));
app.filter("momentFromNow", () => val => moment(val).fromNow());

alt.on("loadPhoneSettings", loadPhoneSettings);