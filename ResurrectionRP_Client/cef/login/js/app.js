var app = angular.module("ResurrectionLogin", ['cp.ngConfirm']);

app.config(['$compileProvider', function ($compileProvider) {
    $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|package):/);
    $compileProvider.imgSrcSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|package):/);
}]);

app.directive('ngEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });

                event.preventDefault();
            }
        });
    };
});

app.controller("LoginCtrl", function ($scope, $http, $ngConfirm) {
    $scope.currentStep = 'login';

    $scope.login = "";
    $scope.password = "";
    $scope.socialClub = null;

    $scope.loggingIn = false;

    $scope.loginToServer = () => {
        $scope.loggingIn = true;

        if ($scope.login != "" && $scope.password != "") {

            let loginObj = {
                login: $scope.login,
                password: $scope.password
            };
            alt.emit("SendLogin", JSON.stringify(loginObj));

        } else {
            $scope.loggingIn = false;
            $scope.error("Erreur", "Vérifiez les champs de connexion avant de continuer");
        }
    };

    $scope.exitGame = () => {
        alt.emit("ExitGame");
    };

    $scope.error = (title, message) => {
        $ngConfirm({
            theme: 'dark',
            type: 'red',
            boxWidth: '30%',
            useBootstrap: false,
            title: title,
            content: message,
            scope: $scope,
            buttons: {
                abort: {
                    text: 'Compris !',
                    btnClass: 'btn-default'
                }
            }
        });
    };

    $scope.success = (title, message) => {
        $ngConfirm({
            theme: 'dark',
            type: 'green',
            boxWidth: '30%',
            useBootstrap: false,
            title: title,
            content: message,
            scope: $scope,
            buttons: {
                abort: {
                    text: 'Compris !',
                    btnClass: 'btn-default'
                }
            }
        });
    };

    window.addEventListener("OnCallEvent", ev => {
        $scope.$apply(() => {
            $scope.loggingIn = false;
            $scope.error("Erreur de connexion", "Identifiant ou mot de passe incorrect");
        });
    });
});

function callEvent() {
    var event = new CustomEvent("OnCallEvent");
    window.dispatchEvent(event);
};

// Iterate over each select element
$('select').each(function () {

    // Cache the number of options
    var $this = $(this),
        numberOfOptions = $(this).children('option').length;

    // Hides the select element
    $this.addClass('s-hidden');

    // Wrap the select element ind a div
    $this.wrap('<div class="select"></div>');

    // Insert a styled div to sit over the top of the hidden select element
    $this.after('<div class="styledSelect"></div>');

    // Cache the styled div
    var $styledSelect = $this.next('div.styledSelect');

    // Show the first select option in the styled div
    $styledSelect.text($this.children('option').eq(0).text());

    // Insert an unordered list after the styled div and also cache the list
    var $list = $('<ul />', {
        'class': 'options'
    }).insertAfter($styledSelect);

    // Insert a list item into the unordered list for each select option
    for (var i = 0; i < numberOfOptions; i++) {
        $('<li />', {
            text: $this.children('option').eq(i).text(),
            rel: $this.children('option').eq(i).val()
        }).appendTo($list);
    }

    // Cache the list items
    var $listItems = $list.children('li');

    // Show the unordered list when the styled div is clicked (also hides it if the div is clicked again)
    $styledSelect.click(function (e) {
        e.stopPropagation();
        $('div.styledSelect.active').each(function () {
            $(this).removeClass('active').next('ul.options').hide();
        });
        $(this).toggleClass('active').next('ul.options').toggle();
    });

    // Hides the unordered list when a list item is clicked and updates the styled div to show the selected list item
    // Updates the select element to have the value of the equivalent option
    $listItems.click(function (e) {
        e.stopPropagation();
        $styledSelect.text($(this).text()).removeClass('active');
        $this.val($(this).attr('rel'));
        $list.hide();
        /* alert($this.val()); Uncomment this for demonstration! */
    });

    // Hides the unordered list when clicking outside of it
    $(document).click(function () {
        $styledSelect.removeClass('active');
        $list.hide();
    });

});

jQuery(document).ready(function ($) {
    $(".identifiant-input").focusin(function () {
        $(".info").css("display", "block");
    });
});

jQuery(document).ready(function ($) {
    $(".identifiant-input").focusout(function () {
        $(".info").css("display", "none");
    });
});

$(() => {
    if ('alt' in window) {
        alt.on('error', callEvent);
    }
});
