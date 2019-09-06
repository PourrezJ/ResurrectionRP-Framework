var today = moment().add(20, 'years');

let app = angular.module("charCreator", ['cp.ngConfirm']);

app.config(['$compileProvider', function ($compileProvider) {
    $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|package):/);
    $compileProvider.imgSrcSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|package):/);
}]);

$(() => {
    if ('alt' in window) {
        alt.on("CharCreatorLoad", loadChar);
    }
});


function loadChar() {
    var $scope = angular.element($("body")).scope();
    $scope.$apply(function () {
        $scope.setGender(0);
        $scope.$emit('charLoaded');
    });
}

function setHairColor() {
    let hairColorIndex = parseInt(document.getElementById("hairColorId").value);
    let hairColorHighlightIndex = parseInt(document.getElementById("hairColorHighlightId").value);
    alt.emit("setHairColor", hairColorIndex, hairColorHighlightIndex);
}

function setHeadOverlayColor(type, color) {
    alt.emit("setHeadOverlayColor", type, color);
}

function setHeadOverlay(type, index) {
    alt.emit("setHeadOverlay", type, index);
}

function setComponentVariation(type, index) {
    alt.emit("setComponentVariation", type, index);
}

$(function () {
    $("#componentId, #componentIndex").change(function () {
        let index = parseInt($('#componentId').val());
        let val = parseInt($('#componentIndex').val());
        setComponentVariation(index, val);
    });

    $("#hairColorId, #hairColorHighlightId").change(function () {
        setHairColor();
    });

    $("#eyeColorId").change(function () {
        setEyeColor();
    });
    /*
    $("input[data-type='headOverlayColor']").change(function () {
        let index = parseInt($(this).data('index'));
        let val = parseInt($(this).val());
        setHeadOverlayColor(index, val);
    });

    $("input[data-type='headOverlay']").change(function () {
        let index = parseInt($(this).data('index'));
        let val = parseInt($(this).val());
        setHeadOverlay(index, val);
    });*/
});

app.controller("CharCtrl", function ($scope, $ngConfirm, $timeout) {
    $scope.char = {};
    $scope.currentMenu = null, $scope.previousGender = null, $scope.hairName = null;
    $scope.EyeColor = eyeColors;
    $scope.Hair = hairList;
    $scope.Parents = [fathers, mothers];
    $scope.ParentsName = [];
    $scope.Gender = false;
    $scope.lastUpdateField = {};
    $scope.limits = {
        hairColor: 63,
        facialHair: 28,
        maxLipstickColor: 32,
        headOverlay: {
            0: { min: -1, max: 23 },
            1: { min: -1, max: 28 },
            2: { min: -1, max: 33 },
            3: { min: -1, max: 14 },
            4: { min: -1, max: 74 },
            6: { min: -1, max: 11 },
            8: { min: -1, max: 9 },
            9: { min: -1, max: 17 }
        }
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

    $scope.passportDeliveryDate = today.format("DD/MM/YYYY");

    $scope.mouseDown = false;

    $scope.confirmGender = function () {
        if ($scope.Gender)
            $scope.char.Gender = 1;
        else
            $scope.char.Gender = 0;

        $ngConfirm({
            theme: 'dark',
            columnClass: 'col-md-5 col-md-offset-3',
            title: 'Changement de sexe',
            content: '<strong>Attention !</strong> Changer le sexe du personnage entraîne la remise à zéro de tous les paramètres !',
            scope: $scope,
            buttons: {
                confirm: {
                    text: 'C\'est mon ultime bafouille',
                    btnClass: 'btn-danger',
                    action: function (scope, button) {
                        $scope.setGender();
                        $scope.previousGender = scope.char.Gender;
                    }
                },
                abort: {
                    text: 'Bof en fait',
                    btnClass: 'btn-default',
                    action: function (scope, button) {
                        scope.$apply(function () {
                            scope.char.Gender = scope.previousGender;
                            $scope.Gender = !$scope.Gender;
                        });
                    }
                }
            }
        });
    };

    $scope.setEyeColor = function () {
        alt.emit("setHeadBlend", JSON.stringify($scope.char.Parents));
        alt.emit("setEyeColor", $scope.char.EyeColor);
    };

    $scope.setHairStyle = function () {
        alt.emit("setHeadBlend", JSON.stringify($scope.char.Parents));
        alt.emit("setHairStyle", $scope.char.Hair.Hair);
    };

    $scope.setHairColor = function () {
        alt.emit("setHeadBlend", JSON.stringify($scope.char.Parents));
        alt.emit("setHairColor", $scope.char.Hair.Color, $scope.char.Hair.HighlightColor);
    };

    $scope.setFaceFeature = function (index) {
        alt.emit("setHeadBlend", JSON.stringify($scope.char.Parents));
        alt.emit("setFaceFeature", index, $scope.char.Features[index]);
    };

    $scope.setHeadBlend = function () {
        alt.emit("setHeadBlend", JSON.stringify($scope.char.Parents));
    };

    $scope.setGender = function (gender) {
        if ($scope.char.Gender != gender) {
            $scope.char.Gender = gender;
            alt.emit("setGender", $scope.char.Gender);
            $scope.resetChar();
        }
    };

    $scope.setHeadOverlay = function () {
        if (Object.keys($scope.lastUpdateField) > 0) {
            alt.emit("setHeadBlend", JSON.stringify($scope.char.Parents));
            let ho = $scope.char[$scope.lastUpdateField.array][$scope.lastUpdateField.overlay];
            alt.emit("setHeadOverlay", $scope.lastUpdateField.overlay.toString(), JSON.stringify(ho, null, 4));
        }
    };
    
    $scope.updateHeadOverlay = function (index, type, value = null, max = null) {
        if (value !== null && value >= $scope.limits.headOverlay[index].min && value <= ((max === null) ? $scope.limits.headOverlay[index].max : max))
            $scope.char.Appearance[index][type] = value;

        alt.emit("setHeadBlend", JSON.stringify($scope.char.Parents));
        alt.emit("updateHeadOverlay", index, JSON.stringify($scope.char.Appearance[index]));
    };
    
    $scope.resetChar = function () {
        $scope.char.Identite = {
            LastName: "",
            FirstName: "",
            Nationalite: "AMERICAINE",
            BirthDate: "",
            Age: 0
        };

        $scope.getAge = () => {
            let diff = today.diff(moment($scope.char.Identite.BirthDate, "DD/MM/YYYY"), 'years');
            $scope.char.Identite.Age = diff;
            $scope.char.Identite.AgeString = isNaN(diff) ? "" : diff + " ans";
        };

        $scope.char.Parents = {};
        $scope.char.Parents.ShapeFirst = $scope.char.Gender === 0 ? 0 : 21;
        $scope.char.Parents.ShapeSecond = $scope.Parents[1][0];
        $scope.char.Parents.ShapeThird = 0;
        $scope.char.Parents.SkinFirst = 0;
        $scope.char.Parents.SkinSecond = $scope.Parents[1][0];

        $scope.char.Parents.ShapeMix = $scope.char.Gender === 0 ? 0 : 1;
        $scope.char.Parents.SkinMix = $scope.char.Gender === 0 ? 0 : 1;
        $scope.char.Parents.SkinThird = $scope.char.Gender === 0 ? 0 : 1;
        $scope.char.Parents.ThirdMix = 0;
        //$scope.char.Parents.ShapeMix = 0.5;
        //$scope.char.Parents.SkinMix = 0.5;
        //$scope.char.Parents.SkinThird = 0;
        //$scope.char.Parents.ThirdMix = 0.5;

        $scope.char.Features = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        $scope.char.Appearance =
            [
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
                { Index: -1, Opacity: 1, Color: -1, SecondaryColor: 0 },
            ];
        $scope.char.Hair = {
            Hair: $scope.Hair[$scope.char.Gender][0].ID,
            Color: 0,
            HighlightColor: 0
        };
        $scope.char.EyeColor = 0;

        $scope.ParentsName[0] = fatherNames[$scope.Parents[0].findIndex((parent) => parent === $scope.char.Parents.ShapeFirst)];
        $scope.ParentsName[1] = motherNames[$scope.Parents[1].findIndex((parent) => parent === $scope.char.Parents.ShapeSecond)];

        console.log(`debug: ${$scope.ParentsName[0]} || ${$scope.ParentsName[1]}`);
    };

    $scope.saveChar = function () {
        if ($scope.char.Identite.LastName === null || $scope.char.Identite.LastName === "") {
            $scope.error("Erreur !", "Nous avons besoin de votre nom de famille pour le passeport");
            return;
        }

        if ($scope.char.Identite.FirstName === null || $scope.char.Identite.FirstName === "") {
            $scope.error("Erreur !", "Nous avons besoin de votre prénom pour le passeport");
            return;
        }

        if ($scope.char.Identite.Nationalite === null || $scope.char.Identite.Nationalite === "") {
            $scope.error("Erreur !", "Nous avons besoin de votre nationalité pour le passeport");
            return;
        }

        if (!moment($scope.char.Identite.BirthDate, "DD/MM/YYYY").isValid()) {
            $scope.error("Erreur !", "Votre date de naissance doit être au format JOUR/MOIS/ANNEE");
            return;
        }

        if ($scope.char.Identite.Age > 90 || $scope.char.Identite.Age <= 12) {
            $scope.error("Erreur !", "Tu as " + $scope.char.Identite.Age + ($scope.char.Identite.Age === 1 ? " an" : " ans") + " ? Vraiment ?");
            return;
        }

        if ($scope.char.Identite.Age < 18 && $scope.char.Identite.Age > 12) {
            $scope.error("Erreur !", "Los Santos est une ville de violence et de débauche. Tu dois être majeur pour entrer.<br />En attendant, retourne dans les bras de tes parents !");
            return;
        }

        close_passport();

        for (var i in $scope.char.Appearance) {
            $scope.char.Appearance[i].Index = $scope.char.Appearance[i].Index === -1 ? 255 : $scope.char.Appearance[i].Index;
            $scope.char.Appearance[i].Color = $scope.char.Appearance[i].Color === -1 ? 255 : $scope.char.Appearance[i].Color; 
            $scope.char.Appearance[i].SecondaryColor = $scope.char.Appearance[i].SecondaryColor === -1 ? 255 : $scope.char.Appearance[i].SecondaryColor;
        }

        $scope.char.Identite.BirthDate = moment($scope.char.Identite.BirthDate, "DD/MM/YYYY").format("YYYY-MM-DD");

        alt.emit("saveCharacter", JSON.stringify($scope.char), JSON.stringify($scope.char.Identite));
    };

    $scope.updateValue = function (newValue, field, field2, max) {
        let previousValue = typeof field2 !== 'undefined' ? $scope.char[field][field2] : $scope.char[field];

        let maxValue = null;
        if (typeof max !== 'undefined')
            maxValue = max;
        else
            maxValue = typeof field2 !== 'undefined' ? $scope[field][field2].length - 1 : $scope[field].length - 1;

        if (newValue === previousValue || newValue < 0 || newValue > maxValue)
            return;

        if (typeof field2 !== 'undefined')
            $scope.char[field][field2] = newValue;
        else
            $scope.char[field] = newValue;

        setTimeout(function () {
            if ($scope.mouseDown) {
                let i = newValue;
                if (previousValue > newValue)
                    i--;
                else
                    i++;

                $scope.$apply(function () {
                    $scope.updateValue(i, field, field2, max);
                });
            }
        }, 200);
    };

    $scope.updateArray = function (newValue, field, iterator, field2, max) {
        $scope.lastUpdateField.array = field;
        $scope.lastUpdateField.overlay = iterator;

        let previousValue = typeof field2 !== 'undefined' ? $scope.char[field][iterator][field2] : $scope.char[field][iterator];

        let maxValue = null;
        if (typeof max !== 'undefined')
            maxValue = max;
        else
            maxValue = typeof field2 !== 'undefined' ? $scope[field][iterator][field2].length - 1 : $scope[field][iterator].length - 1;

        if (newValue === previousValue || newValue < 0 || newValue > maxValue)
            return;

        if (typeof field2 !== 'undefined')
            $scope.char[field][iterator][field2] = newValue;
        else
            $scope.char[field][iterator] = newValue;

        setTimeout(function () {
            if ($scope.mouseDown) {
                let i = newValue;
                if (previousValue > newValue)
                    i--;
                else
                    i++;

                $scope.$apply(function () {
                    $scope.updateArray(i, field, iterator, field2, max);
                });
            }
        }, 200);
    };

    $scope.hairStyleRangeCheck = function (newValue) {
        let hairList = $scope.Hair[$scope.char.Gender];

        let previousValue = $scope.char.Hair.Hair;
        let maxValue = hairList[hairList.length - 1].ID;
        if (newValue === previousValue || newValue < 0 || newValue > maxValue)
            return;

        if (typeof hairList.find(hair => hair.ID === newValue) === 'undefined') {
            let i = hairList.findIndex((hair) => hair.ID === previousValue);
            if (previousValue > newValue)
                i--;
            else
                i++;


            newValue = hairList[i].ID;
        }
        $scope.char.Hair.Hair = newValue;

        setTimeout(function () {
            if ($scope.mouseDown) {
                let i = newValue;
                if (previousValue > newValue)
                    i--;
                else
                    i++;

                $scope.$apply(function () {
                    $scope.hairStyleRangeCheck(i);
                });
            }
        }, 200);
    };

    $scope.parentsRangeCheck = function (newValue, parentId, field) {
        let parentList = $scope.Parents[parentId];

        let previousValue = $scope.char.Parents[field];
        let maxValue = parentList[parentList.length - 1];

        if (newValue === previousValue || newValue < 0 || newValue > maxValue)
            return;

        if (typeof parentList.find(parent => parent === newValue) === 'undefined') {
            let i = parentList.findIndex((parent) => parent === previousValue);
            if (previousValue > newValue)
                i--;
            else
                i++;
            newValue = parentList[i];

        }

        $scope.char.Parents[field] = newValue;


        setTimeout(function () {
            if ($scope.mouseDown) {
                let i = newValue;
                if (previousValue > newValue)
                    i--;
                else
                    i++;

                $scope.$apply(function () {
                    $scope.parentsRangeCheck(i, parentId, field);
                });
            }
        }, 200);
    };

    $scope.$on('charLoaded', function (e) {
        let navigationHeight = $("#navigation").height();

        if ($scope.char.Gender === 1)
            $scope.Gender = true;

        $scope.$watch('char.EyeColor', function () { $scope.setEyeColor(); }, true);

        $scope.$watch('char.Hair.Hair', function () {
            $scope.setHairStyle();
            $scope.hairName = $scope.Hair[$scope.char.Gender].find(hair => hair.ID === $scope.char.Hair.Hair).Name;
        }, true);

        $scope.$watch('char.Hair.Color', function () {
            $scope.setHairColor();
        }, true);

        $scope.$watch('char.Hair.HighlightColor', function () {
            $scope.setHairColor();
        }, true);

        $scope.$watch('char.Parents.ShapeFirst', function () {
            $scope.char.Parents.SkinFirst = $scope.char.Parents.ShapeFirst;
        }, true);

        $scope.$watch('char.Parents.ShapeSecond', function () {
            $scope.char.Parents.SkinSecond = $scope.char.Parents.ShapeSecond;
        }, true);

        $scope.$watch('char.Parents', function () {
            $scope.setHeadBlend();
            $scope.ParentsName[0] = fatherNames[$scope.Parents[0].findIndex((parent) => parent === $scope.char.Parents.ShapeFirst)];
            $scope.ParentsName[1] = motherNames[$scope.Parents[1].findIndex((parent) => parent === $scope.char.Parents.ShapeSecond)];
        }, true);

        $scope.$watch('currentMenu', function () {
            $timeout(() => {
                let optionsHeight = $("#options").height();
                if (navigationHeight > optionsHeight) {
                    $("#options").css("top", "442px");
                    $("#options").css("bottom", "initial");
                } else {
                    $("#options").css("top", "initial");
                    $("#options").css("bottom", "40px");
                }
            }, 1);
        }, true);
        console.log("charLoaded !");
        //$scope.$watch('char.Appearance', function () {
        //    $scope.updateHeadOverlay();
        //}, true);
    });
});

function open_gender_selection() {
    close_character_creator();
    $("#sexe").css("display", "block");
}

function close_gender_selection() {
    $("#sexe").css("display", "none");
}

function open_character_creator() {
    close_gender_selection();
    close_passport();
    $("#character_creator #navigation li").css("transform", "translateY(0%)");
    $("#character_creator .content").css("background-image", "-webkit-linear-gradient(left, rgba(255, 42, 0, 0.788235294117647), transparent 80%)");
    $("#character_creator .precedent").css("opacity", "1");
    $("#character_creator a.next.perso").css("display", "block");
}

function close_character_creator() {
    var $scope = angular.element($("body")).scope();
    $scope.$apply(() => { $scope.currentMenu = null; });
    $("#character_creator #navigation li").css("transform", "translateY(1000%)");
    $("#character_creator .content").css("background-image", "initial");
    $("#character_creator .precedent").css("opacity", "0");
    $("#character_creator a.next.perso").css("display", "none");
}

function open_passport() {
    close_character_creator();
    $("#passport .passux").css("transform", "translate(-50%, 0%)");
    $("#passport .precedent").css("opacity", "1");
    $("#passport").css("display", "block");
    $("#passport").css("z-index", "999");
    $("a.next.pass").css("display", "block");
}

function close_passport() {
    $("#passport .passux").css("transform", "translate(50%, 0%)");
    $("#passport").css("display", "none");
    $("#passport").css("z-index", "5");
    $("a.next.pass").css("display", "none");
}