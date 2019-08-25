/**
 * ResurrectionInventory
 * @param {String} Module's name
 * @param {Array} Modules dependancies (injector)
 * @description AngularJS module binded to the auto-boostrap directive 'ngApp'
 * @doc https://code.angularjs.org/1.7.0/docs/api/ng/type/angular.Module
 */
let app = angular.module("ResurrectionInventory", ['cp.ngConfirm', 'ngDragDrop']);

app.config(['$compileProvider', ($compileProvider) => {
    $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|package):/);
    $compileProvider.imgSrcSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|package|file):/);
}]);

function loadInventory(pocket, bag, distant, outfit, give) {
    console.log("cef: loadInventory recept");
    var event = new CustomEvent("ItemsLoaded", { "detail": { pocket, bag, distant, outfit, give } });
    window.dispatchEvent(event);
}

$(() => {
    if ('alt' in window) {
        console.log("cef: Request Item Inventory Event");
        alt.on("loadInventory", loadInventory);
        alt.emit('loaditem');
    }
});

/**
 * ngRightClick
 * @description Directive to create an attribute 'ng-right-click' in view
 * @doc https://code.angularjs.org/1.7.0/docs/guide/directive
 */
app.directive('ngRightClick', ($parse) => {
    return (scope, element, attrs) => {
        let fn = $parse(attrs.ngRightClick);
        element.bind('contextmenu', (event) => {
            scope.$apply(() => {
                event.preventDefault();
                fn(scope, { $event: event });
            });
        });
    };
});

/**
 * run
 * @method angular.Module.run
 * @description Launch when the injector is done loading all modules
 * @doc https://code.angularjs.org/1.7.0/docs/api/ng/type/angular.Module#run
 */
app.run(['$ngConfirmDefaults', ($ngConfirmDefaults) => {
    $ngConfirmDefaults.useBootstrap = false;
    $ngConfirmDefaults.theme = "dark";
    $ngConfirmDefaults.boxWidth = "500px";
}]);

/**
 * controller
 * @param {String} InventoryCtrl - Factory's name
 * @param {Array} Factory ($scope - Scope of the controller binded to the view / $ngConfirm - / $q - Service for running functions asynchronously)
 * @description Adds new factory to create 'InventoryCtrl' contoller attached to 'ng-controller' directive in view
 * @doc https://code.angularjs.org/1.7.0/docs/api/ng/service/$controller
 */
app.controller("InventoryCtrl", ['$scope', '$ngConfirm', '$q', ($scope, $ngConfirm, $q) => {
    // Constructor
    $scope.selectedItem = null;
    $scope.displayInventory = 'pocket';
    $scope.showInventoryBag = false;
    $scope.showSlotWeapons = false;
    $scope.showMainAction = false;
    $scope.itemGive = true;

    $scope.inventorySize = 20;

    // List slots
    $scope.pocketSlots = [];
    $scope.bagSlots = [];
    $scope.distantSlots = [];
    $scope.outfitSlots = [];
    // Items
    $scope.itemsPocket = null;
    $scope.itemsBag = null;
    $scope.itemsDistant = null;
    $scope.itemsOutfit = null;

    /**
     * setInventory
     */
    $scope.setInventory = inventoryType => {
        $scope.displayInventory = inventoryType;
    };

    /**
     * loadItems
     * @description Loading items for specific inventory
     */
    $scope.loadItems = () => {

        // Pocket
        if ($scope.itemsPocket !== undefined) {
            let size = 0;
            let itemInSlot = null;
            $scope.pocketSize = $scope.itemsPocket.Slots;

            for (let i = 0; i < $scope.pocketSize; i++) {
                itemInSlot = $scope.itemsPocket.RPGInventoryItems.find(it => it.inventorySlot === i);
                $scope.pocketSlots[i] = {
                    index: i,
                    item: itemInSlot,
                    type: "pocketSlots"
                };
                if (itemInSlot !== undefined) size += itemInSlot.quantity * itemInSlot.weight;
            }

            $scope.itemsPocket.CurrentSize = size;
            let value = ($scope.itemsPocket.CurrentSize * 100) / $scope.itemsPocket.MaxSize;
            $(".bar span").css("width", value.toString() + "%");
        }

        // Bag
        if ($scope.itemsBag !== null) {

            $scope.showInventoryBag = true; // Temp for waiting bag item. 

            let size = 0;
            let itemInSlot = null;
            $scope.bagSize = $scope.itemsBag.Slots;

            for (let i = 0; i < $scope.bagSize; i++) {
                itemInSlot = $scope.itemsBag.RPGInventoryItems.find(it => it.inventorySlot === i);
                $scope.bagSlots[i] = {
                    index: i,
                    item: itemInSlot,
                    type: "bagSlots"
                };
                if (itemInSlot !== undefined) size += itemInSlot.quantity * itemInSlot.weight;
            }

            $scope.itemsBag.CurrentSize = size;
            let value = ($scope.itemsBag.CurrentSize * 100) / $scope.itemsBag.MaxSize;
            $(".bagbar span").css("width", value.toString() + "%");
        }

        // Distant
        if ($scope.itemsDistant !== null) {
            let size = 0;
            let itemInSlot = null;
            $scope.distantSize = $scope.itemsDistant.Slots;

            for (let i = 0; i < $scope.distantSize; i++) {
                itemInSlot = $scope.itemsDistant.RPGInventoryItems.find(it => it.inventorySlot === i);
                $scope.distantSlots[i] = {
                    index: i,
                    item: itemInSlot,
                    type: "distantSlots"
                };
                if (itemInSlot !== undefined) size += itemInSlot.quantity * itemInSlot.weight;
            }
            console.log($scope.itemsDistant);
            $scope.itemsDistant.CurrentSize = size;
            let value = ($scope.itemsDistant.CurrentSize * 100) / $scope.itemsDistant.MaxSize;
            $(".distantBar span").css("width", value.toString() + "%");

        }

        // Outfit
        if ($scope.itemsOutfit !== undefined) {
            let itemInSlot = null;

            for (let i = 0; i < $scope.itemsOutfit.Slots; i++) {
                itemInSlot = $scope.itemsOutfit.RPGInventoryItems.find(it => it.inventorySlot === i);

                $scope.outfitSlots[i] = {
                    index: i,
                    class: $scope.itemsOutfit.NamedSlots[i].class,
                    item: itemInSlot,
                    type: "outfitSlots",
                    drop: $scope.itemsOutfit.NamedSlots[i].dataDrop
                };
            }

            // // If backpack = Show backpack inventory

            // if ($scope.outfitSlots[13].item !== undefined) {
            //     if ($scope.outfitSlots[13].item.id == 69) {
            //         $scope.showInventoryBag = true;
            //     }
            // }

        }
    };

    /**
     * onDragStart
     */
    $scope.onDragStart = (ev, helper, item) => {
        $scope.selectedItem = item;
    };

    /**
     * onbeforeDrop
     * @param {event} Event
     * @param {ui} UI
     * @param {slot} Slot
     * @description Filter based on promises
     */
    $scope.onbeforeDrop = (event, ui, slot) => {
        let deferred = $q.defer();

        if ($scope.selectedItem === null) {
            deferred.reject("selectedItem is null");
            return deferred.promise;
        }

        if (slot.class !== undefined) {
            if (slot.item != null) {
                deferred.reject();
                $scope.selectedItem = null;
                return deferred.promise;
            }

            if (slot.class !== $scope.selectedItem.class) {
                deferred.reject();
                $scope.selectedItem = null;
                return deferred.promise;
            }

            deferred.resolve();
            return deferred.promise;
        }

        if (slot.item !== undefined && (slot.item.name !== $scope.selectedItem.name || slot.item.id !== $scope.selectedItem.id)) {
            deferred.reject();
            $scope.selectedItem = null;
            return deferred.promise;
        }

        if (slot.item !== undefined && (slot.item.name === $scope.selectedItem.name || slot.item.id === $scope.selectedItem.id) && !$scope.selectedItem.stackable) {
            deferred.reject();
            $scope.selectedItem = null;
            return deferred.promise;
        }

        if (slot.type === undefined) {
            let maxSize = 0;
            switch (slot) {
                case "pocketSlots":
                    maxSize = $scope.itemsPocket.MaxSize;
                    break;
                case "bagSlots":
                    maxSize = $scope.itemsBag.MaxSize;
                    break;
                case "distantSlots":
                    maxSize = $scope.itemsDistant.MaxSize;
                    break;
                case "outfitSlots":
                    maxSize = $scope.itemsOutfit.MaxSize;
                    break;
                default:
                    return;
            }

            if ($scope.calculSize(slot) + ($scope.selectedItem.weight * $scope.selectedItem.quantity) > maxSize) {
                deferred.reject();
                $scope.selectedItem = null;
                return deferred.promise;
            }
            deferred.resolve();
            return deferred.promise;

        } else {
            // for re-stack
            if (slot.type === $scope.selectedItem.inventoryType && $scope.selectedItem.stackable) {
                deferred.resolve();
                return deferred.promise;
            }

            let maxSize = 0;
            switch (slot.type) {
                case "pocketSlots":
                    maxSize = $scope.itemsPocket.MaxSize;
                    break;
                case "bagSlots":
                    maxSize = $scope.itemsBag.MaxSize;
                    break;
                case "distantSlots":
                    maxSize = $scope.itemsDistant.MaxSize;
                    break;
                case "outfitSlots":
                    maxSize = $scope.itemsOutfit.MaxSize;
                    break;
                default:
                    return;
            }

            if ($scope.calculSize(slot.type) + ($scope.selectedItem.weight * $scope.selectedItem.quantity) > maxSize) {
                deferred.reject("to heavy!");
                $scope.selectedItem = null;
                return deferred.promise;
            }
        }

        deferred.resolve();
        return deferred.promise;
    };

    /**
     * onSlotDrop
     * @param {ev} Event
     * @param {ui} UI
     * @param {slot} Slot
     * @description Method launch on slot drop
     */
    $scope.onSlotDrop = (ev, ui, slot) => {

        if ($scope.selectedItem === undefined || $scope.selectedItem === null || $scope.selectedItem === slot.item) return;

        if (slot.item !== undefined && $scope.selectedItem.id && slot.item.id) {
            $scope.selectedItem.quantity += slot.item.quantity;
        }
        slot.item = $scope.selectedItem;

        let oldSlot = $scope[$scope.selectedItem.inventoryType][$scope.selectedItem.inventorySlot];

        if (oldSlot.item !== undefined && (oldSlot.index !== slot.index || oldSlot.item.inventoryType !== slot.type)) {
            try {
                //oldSlot.item.inventoryType = slot.type;
                alt.emit("inventorySwitchItem", slot.type, oldSlot.type, oldSlot.item.id, slot.index, oldSlot.item.inventorySlot);
                //slot.inventorySlot = slot.index;
                //oldSlot.item = undefined;
                //$scope.calculSize(oldSlot.type);
                //$scope.calculSize(slot.type);
                $scope.selectedItem.inventorySlot = slot.index;
            } catch (ex) {
                console.log("onSlotDrop: " + ex);
            }
        }

        $scope.selectedItem = null;
    };

    /**
     * onPocketDrop
     * @param {ev} Event
     * @param {helper} Helper
     * @description Dropping item into pocket inventory
     */
    $scope.onPocketDrop = (ev, helper) => {
        let availableSlot = $scope.pocketSlots.find(s => s.index === $scope.getFirstAvailableSlot("pocketSlots"));
        let itemId = $scope.selectedItem.id;

        if (availableSlot !== null && availableSlot !== undefined) {

            availableSlot.item = Object.assign({}, $scope.selectedItem);
            let selectedInventory = $scope.selectedItem.inventoryType;
            let oldSlot = $scope[selectedInventory][$scope.selectedItem.inventorySlot];
            if (oldSlot !== undefined) {
                alt.emit("inventorySwitchItem", availableSlot.type, $scope.selectedItem.inventoryType, itemId, availableSlot.index, oldSlot.item.inventorySlot);

                $scope.selectedItem.inventoryType = "pocketSlots";
                availableSlot.item.inventoryType = "pocketSlots";

                oldSlot.item = undefined;
                delete $scope.selectedItem;
                $scope.calculSize(oldSlot.type);
                $scope.calculSize(availableSlot.type);
            }

            $scope.selectedItem = null;
        }
    };

    /**
     * onBagDrop
     * @param {ev} Event
     * @param {helper} Helper
     * @description To bag
     */
    $scope.onBagDrop = (ev, helper) => {
        let availableSlot = $scope.bagSlots.find(s => s.index === $scope.getFirstAvailableSlot("bagSlots"));
        let itemId = $scope.selectedItem.id;

        if (availableSlot !== null && availableSlot !== undefined) {

            availableSlot.item = Object.assign({}, $scope.selectedItem);
            let selectedInventory = $scope.selectedItem.inventoryType;
            let oldSlot = $scope[selectedInventory][$scope.selectedItem.inventorySlot];
            if (oldSlot !== undefined) {
                alt.emit("inventorySwitchItem", availableSlot.type, $scope.selectedItem.inventoryType, itemId, availableSlot.index, oldSlot.item.inventorySlot);

                $scope.selectedItem.inventoryType = "bagSlots";
                availableSlot.item.inventoryType = "bagSlots";

                oldSlot.item = undefined;
                delete $scope.selectedItem;
                $scope.calculSize(oldSlot.type);
                $scope.calculSize(availableSlot.type);
            }

            $scope.selectedItem = null;
        }
    };



    /**
     * onUseDrop
     */
    $scope.onUseDrop = () => {
        //mp.trigger("inventoryUseItem", $scope.selectedItem.id, $scope.selectedItem.name, $scope.displayInventory);
        $scope.selectedItem = null;
    };

    /**
     * openCtxMenu
     * @param {String} item - Item to attach context menu
     * @param {String} ev - Event
     * @description Content of context menu
     */
    $scope.openCtxMenu = (item, ev) => {
        $("#contextMenu").css("display", "block").css("left", ev.clientX + 5).css("top", ev.clientY + 5);
        $scope.selectedItem = item;
    };

    /**
     * useItem
     */
    $scope.useItem = () => {
        if ($scope.selectedItem.quantity >= 1) {
            //$scope.selectedItem.quantity--;
            //$scope.calculSize($scope.selectedItem.inventoryType);

            alt.emit("inventoryUseItem", $scope.selectedItem.id, $scope.selectedItem.inventoryType, $scope.selectedItem.inventorySlot);

            // Initial inventory slot is emptied
            //if($scope.selectedItem.quantity <= 0) {
            //    $scope.clearInitialSlot();
            //}

            $scope.selectedItem = null;
        }
    };

    /**
     * equipItem
     * @description Equip item in outfit inventory
     */
    $scope.equipItem = () => {
        let item = $scope.selectedItem;
        let itemInventory = item.inventoryType;
        let outfitSlot = $scope.outfitSlots[item.outfitPosition - 1];

        if (item == undefined || !item.equipable) return;

        //console.log('equipItem : item', item);
        //console.log('equipItem : slot', outfitSlot);

        if (outfitSlot.item == undefined) {

            // If backpack = show inventory bag
            if (item.id == 69) {
                $scope.showInventoryBag = true;
            }

            // Initial inventory slot is emptied
            $scope[itemInventory][$scope.selectedItem.inventorySlot].item = undefined; // A creuser
            // Recalc inventory weight
            $scope.calculSize(itemInventory);

            // Copy item and change his options
            outfitSlot.item = item;
            outfitSlot.item.inventoryType = 'outfitSlots';
            outfitSlot.item.inventorySlot = item.outfitPosition - 1;
        }

        //mp.trigger("inventoryEquipItem", $scope.selectedItem.id, $scope.displayInventory, $scope.selectedItem.inventorySlot);

        $scope.selectedItem = null;
    }

    /**
     * dropItem
     */
    $scope.dropItem = () => {
        $ngConfirm({
            title: "Jeter " + $scope.selectedItem.name + " ?",
            contentUrl: 'tmpl/dropItem.html',
            scope: $scope,
            buttons: {
                drop: {
                    text: 'Débarrassez-moi de ça !',
                    btnClass: 'btn-red',
                    action: (scope, button) => {
                        $scope.$apply(() => {
                            let value = parseInt($scope.dropCount);
                            if (value <= $scope.selectedItem.quantity) {
                                $scope.selectedItem.quantity -= value;
                                $scope.calculSize($scope.selectedItem.inventoryType);

                                alt.emit("inventoryDropItem", $scope.selectedItem.inventoryType, $scope.selectedItem.id, $scope.selectedItem.inventorySlot, value);

                                if ($scope.selectedItem.quantity <= 0) {
                                    $scope.clearInitialSlot();
                                }
                            }
                        });
                    }
                },
                cancel: {
                    text: "Finalement, non",
                    btnClass: "btn-default"
                }
            }
        });
    };

    /**
     * giveItem
     */
    $scope.giveItem = () => {
        $ngConfirm({
            title: "Donner " + $scope.selectedItem.name + " ?",
            contentUrl: 'tmpl/dropItem.html',
            scope: $scope,
            buttons: {
                drop: {
                    text: 'Lui donner ça',
                    btnClass: 'btn-red',
                    action: (scope, button) => {
                        $scope.$apply(() => {
                            let value = parseInt($scope.dropCount);
                            if ($scope.selectedItem.quantity >= value) {
                                $scope.selectedItem.quantity -= value;
                                $scope.calculSize($scope.selectedItem.inventoryType);

                                alt.emit("inventoryGiveItem", $scope.selectedItem.inventoryType, $scope.selectedItem.id, $scope.selectedItem.inventorySlot, value);

                                if ($scope.selectedItem.quantity <= 0) {
                                    $scope.clearInitialSlot();
                                }
                            }
                        });
                    }
                },
                cancel: {
                    text: "Finalement, je garde",
                    btnClass: "btn-default"
                }
            }
        });
    };

    /**
     * closeCtxMenu
     * @description Add click listener on document to close the context-menu
     */
    $scope.closeCtxMenu = () => {
        $(document).click(() => { $("#contextMenu").css("display", "none"); });
        $scope.selectedItem = null;
    };

    /**
     * getFirstAvailableSlot
     * @method getter
     * @returns Index of freeSlot
     */
    $scope.getFirstAvailableSlot = inventory => {
        let freeSlot = $scope[inventory].find(s => s.item == null);
        if (freeSlot != null) {
            return freeSlot.index;
        }
        return null;
    };

    /**
     * getSlotAcceptation
     * @param {slot} Slot
     * @returns boolean
     */
    $scope.getSlotAcceptation = slot => {

        // if (slot.item === undefined) return true;

        return true;
    };

    /**
 * onaccept
 * @param {slot} Slot
 * @returns boolean
 */
    $scope.onaccept = slot => {
        console.log(slot.class);
        return "." + slot.class;
    };


    /**
     * clearInitialSlot
     * @description Clear the initial slot after drop, etc. (it use $scope.selectedItem)
     */
    $scope.clearInitialSlot = () => {
        return $scope[$scope.selectedItem.inventoryType][$scope.selectedItem.inventorySlot].item = undefined;
    }

    /**
     * splitItem
     * @param {slots} Slots
     * @description Splitting item
     */
    $scope.splitItem = slots => {
        $ngConfirm({
            title: "Séparer " + $scope.selectedItem.name + " ?",
            contentUrl: 'tmpl/splitItem.html',
            scope: $scope,
            buttons: {
                drop: {
                    text: 'Séparer ça',
                    btnClass: 'btn-red',
                    action: (scope, button) => {
                        $scope.$apply(() => {
                            let oldCount = $scope.selectedItem.quantity;
                            let oldSlot = $scope.selectedItem.inventorySlot;
                            let newSlot = $scope.getFirstAvailableSlot($scope.selectedItem.inventoryType);
                            if (newSlot != null) {
                                $scope.selectedItem.quantity -= parseInt($scope.splitCount);

                                let item = jQuery.extend({}, scope.selectedItem);
                                item.quantity = parseInt($scope.splitCount);

                                $scope[$scope.selectedItem.inventoryType].splice(newSlot, 1, {
                                    index: newSlot,
                                    item: item,
                                    type: $scope.selectedItem.inventoryType
                                });

                                alt.emit("inventorySplitItem", $scope.selectedItem.inventoryType, $scope.selectedItem.id, newSlot, oldSlot, oldCount, $scope.selectedItem.quantity, $scope.splitCount);
                            }
                        });
                    }
                },
                cancel: {
                    text: "Finalement, non",
                    btnClass: "btn-default"
                }
            }
        });
    };

    /**
     * changeItemPrice
     * @method
     */
    $scope.changeItemPrice = slots => {
        $ngConfirm({
            title: "Prix " + $scope.selectedItem.name + " ?",
            contentUrl: 'tmpl/priceItem.html',
            scope: $scope,
            buttons: {
                drop: {
                    text: 'Changer le prix',
                    btnClass: 'btn-red',
                    action: (scope, button) => {
                        $scope.$apply(() => {
                            alt.emit("inventoryChangeItemPrice", $scope.selectedItem.inventoryType, $scope.selectedItem.id, $scope.selectedItem.inventorySlot, $scope.priceChange);
                        });
                    }
                },
                cancel: {
                    text: "Finalement, non",
                    btnClass: "btn-default"
                }
            }
        });
    };

    /**
     * calculSize
     * @returns size
     * @param {inventoryType} Inventory type
     * @description Calcul inventory size
     */
    $scope.calculSize = inventoryType => {
        let size = 0;
        let value = 0;
        let item = null;

        for (let i = 0; i < $scope[inventoryType].length; i++) {
            if ($scope[inventoryType][i].item !== undefined) {
                item = $scope[inventoryType][i].item;
                size += item.weight * item.quantity;
            }
        }

        switch (inventoryType) {
            case "pocketSlots":
                $scope.itemsPocket.CurrentSize = size;
                value = parseFloat((size * 100) / $scope.itemsPocket.MaxSize).toFixed(2);
                if (value > 100) value = 100;
                $(".bar span").css("width", value.toString() + "%");
                break;

            case "bagSlots":
                $scope.itemsBag.CurrentSize = size;
                value = parseFloat((size * 100) / $scope.itemsBag.MaxSize).toFixed(2);
                if (value > 100) value = 100;
                $(".bagbar span").css("width", value.toString() + "%");
                break;

            case "distantSlots":
                $scope.itemsDistant.CurrentSize = size;
                value = parseFloat((size * 100) / $scope.itemsDistant.MaxSize).toFixed(2);
                if (value > 100) value = 100;
                $(".distantBar span").css("width", value.toString() + "%");
                break;
        }
        return size;
    };

    /**
     * IIFE (Immediately Invoked Function Expression)
     */
    $(() => {
        $scope.closeCtxMenu();
    });

    window.addEventListener("ItemsLoaded", ev => {
        $scope.$apply(() => {
            console.log(ev.detail);
            $scope.itemsPocket = ev.detail.pocket;
            $scope.itemsBag = ev.detail.bag;
            $scope.itemsDistant = ev.detail.distant;
            $scope.itemsOutfit = ev.detail.outfit;
            $scope.itemGive = ev.detail.give;
            $scope.loadItems();
        });
    });
}]);