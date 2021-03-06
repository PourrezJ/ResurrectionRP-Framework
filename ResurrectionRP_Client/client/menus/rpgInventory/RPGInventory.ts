﻿import * as alt from 'alt-client';
import * as game from 'natives';
import * as chat from '../../Chat/Chat';

export class RPGInventoryManager {
    public loading: boolean;
    public view: alt.WebView;
    public pocket: object;
    public bag: object;
    public distant: object;
    public outfit: object;
    public give: boolean;
    private callbackTime: number = Date.now(); 

    constructor()
    {
        alt.offServer("InventoryManager_OpenMenu", this.InventoryManager_OpenMenu.bind(this));
        alt.offServer("InventoryManager_RefreshMenu", this.InventoryManager_RefreshMenu.bind(this));
        alt.offServer("InventoryManager_CloseMenu", this.CloseMenu.bind(this));

        alt.onServer("InventoryManager_OpenMenu", this.InventoryManager_OpenMenu.bind(this));
        alt.onServer("InventoryManager_RefreshMenu", this.InventoryManager_RefreshMenu.bind(this));
        alt.onServer("InventoryManager_CloseMenu", this.CloseMenu.bind(this));
        alt.on("InventoryManager_CloseMenu", this.CloseMenu.bind(this));
    }

    /*
     *                                  Inventory Open   
     */
    private InventoryManager_OpenMenu(pocket: string, bag: string, distant: string, outfit: string, give: boolean)
    {
        if (chat.isOpened() || game.isPauseMenuActive())
            return;

        this.pocket = JSON.parse(pocket);
        this.bag = JSON.parse(bag);
        this.distant = JSON.parse(distant);
        this.outfit = JSON.parse(outfit);
        this.give = give;

        this.loading = true;

        if (this.view == null) {
            alt.log("debug inventaire: ouverture de l'inventaire.");
            // création du webview
            this.view = new alt.WebView("http://resource/client/cef/inventory/index.html", true);

            this.view.focus();
            alt.showCursor(true);
            alt.toggleGameControls(false)

            alt.log("debug inventaire: création events.");
            this.view.on('inventoryUseItem', (arg1: any, arg2: any, arg3: any) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("RPGInventory_UseItem", /*arg1,*/ arg2, arg3);
            });

            this.view.on('inventoryDropItem', (arg1: any, arg2: any, arg3: any, arg4: any) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("RPGInventory_DropItem", arg1, /*arg2,*/ arg3, arg4);
            });

            this.view.on('inventoryChangeItemPrice', (arg1: any, arg2: any, arg3: any, arg4: any) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("RPGInventory_PriceItemInventory_SRV", arg1, arg2, arg3, arg4);
            });

            this.view.on('inventoryGiveItem', (arg1: any, arg2: any, arg3: any, arg4: any) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("RPGInventory_GiveItem", arg1, arg2, arg3, arg4);
            });  

            this.view.on('inventorySwitchItem', (arg1: any, arg2: any, arg3: any, arg4: any, arg5: any) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("RPGInventory_SwitchItemInventory_SRV", arg1, arg2, arg3, arg4, arg5);
            });

            this.view.on('inventorySplitItem', (arg1: any, arg2: any, arg3: any, arg4: any, arg5: any, arg6: any, arg7: any) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("RPGInventory_SplitItemInventory_SRV", arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            });

            // Callback une fois l'inventaire ouvert
            this.view.on('loaditem', () => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                if (!this.view)
                    return;

                alt.setTimeout(() => {
                    this.view.emit('loadInventory',
                        this.pocket,
                        this.bag,
                        this.distant,
                        this.outfit,
                        (this.give) ? "true" : "false");

                    this.loading = false;
                }, 50);
            });

            alt.emit("hideChat");
        }
    }

    private CheckMultipleCallbak() {
        const time = Date.now() - this.callbackTime;

        if (time < 100) {
            alt.logWarning('Inventory multiple callback: ' + time + 'ms');
            return true;
        }

        this.callbackTime = Date.now();
        return false;
    }

    /*
    *                                  Refresh Menu
    */
    private InventoryManager_RefreshMenu(pocket: string, bag: string, distant: string, outfit: string, give: boolean) {
        if (this.view == null)
            return;

        alt.log("debug inventaire: refresh de l'inventaire.");

        this.pocket = JSON.parse(pocket);
        this.bag = JSON.parse(bag);
        this.distant = JSON.parse(distant);
        this.outfit = JSON.parse(outfit);
        this.give = give;

        this.view.emit('loadInventory',
            this.pocket,
            this.bag,
            this.distant,
            this.outfit,
            (this.give) ? "true" : "false");
    }

    /*
    *                                  Close Menu
    */
    private CloseMenu() {
        alt.log("Fermeture de l'inventaire");
        if (this.view == null)
            return;
        this.view.destroy();
        this.view = null;
        this.loading = false;

        alt.emitServer("RPGInventory_ClosedMenu_SRV");
        alt.toggleGameControls(true);
        alt.showCursor(false);
        chat.hide(false);
    }

    public HasOpen() { this.view != null };
}