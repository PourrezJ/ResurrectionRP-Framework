﻿import * as alt from 'alt';
import * as game from 'natives';
import * as chat from 'client/chat/chat';

export class RPGInventoryManager {
    public loading: boolean;
    public view: alt.WebView;
    public pocket: object;
    public bag: object;
    public distant: object;
    public outfit: object;
    public give: boolean;

    constructor() {
        alt.onServer("InventoryManager_OpenMenu", (pocket: string, bag: string, distant: string, outfit: string, give: boolean) => {
            if (chat.isOpened() || game.isPauseMenuActive())
                return;

            this.pocket = JSON.parse(pocket);
            this.bag = JSON.parse(bag);
            this.distant = JSON.parse(distant);
            this.outfit = JSON.parse(outfit);
            this.give = give;

            this.loading = true;

            if (this.view == null) {
                this.view = new alt.WebView("http://resources/resurrectionrp/client/cef/inventory/index.html");
                this.view.on("loaditem", () => {
                    if (!this.view)
                        return;

                    alt.setTimeout(() => {
                        this.view.emit('loadInventory',
                            this.pocket,
                            (this.bag != null) ? this.bag : null,
                            (this.distant) ? this.distant : null,
                            (this.outfit) ? this.outfit : null,
                            (this.give) ? "true" : "false");
                    }, 75);

                    this.loading = false;
                });
                this.view.focus();
                alt.showCursor(true);
            }
            else {
                this.view.emit('loadInventory',
                    this.pocket,
                    (this.bag != null) ? this.bag : null,
                    (this.distant) ? this.distant : null,
                    (this.outfit) ? this.outfit : null,
                    (this.give) ? "true" : "false");
                this.loading = false;
            }

            this.view.on('inventoryUseItem', (arg1: any, arg2: any, arg3: any) => alt.emitServer("RPGInventory_UseItem", arg1, arg2, arg3));
            this.view.on('inventoryDropItem', (arg1: any, arg2: any, arg3: any, arg4: any) => alt.emitServer("RPGInventory_DropItem", arg1, arg2, arg3, arg4));
            this.view.on('inventoryChangeItemPrice', (arg1: any, arg2: any, arg3: any, arg4: any) => alt.emitServer("RPGInventory_PriceItemInventory_SRV", arg1, arg2, arg3, arg4));
            this.view.on('inventoryGiveItem', (arg1: any, arg2: any, arg3: any, arg4: any) => alt.emitServer("RPGInventory_GiveItem", arg1, arg2, arg3, arg4));
            this.view.on('inventorySwitchItem', (arg1: any, arg2: any, arg3: any, arg4: any, arg5: any) => alt.emitServer("RPGInventory_SwitchItemInventory_SRV", arg1, arg2, arg3, arg4, arg5));
            this.view.on('inventorySplitItem', (arg1: any, arg2: any, arg3: any, arg4: any, arg5: any, arg6: any, arg7: any) => alt.emitServer("RPGInventory_SplitItemInventory_SRV", arg1, arg2, arg3, arg4, arg5, arg6, arg7));


            alt.emit("hideChat");
        });

        alt.onServer("InventoryManager_CloseMenu", () => this.CloseMenu());
        alt.on("InventoryManager_CloseMenu", () => this.CloseMenu());

        alt.on("update", () => {
            if (this.view != null)
                alt.toggleGameControls(false)
        });

    }
    private CloseMenu() {
        if (this.view == null)
            return;
        this.view.destroy();
        this.view = null;
        this.loading = false;

        alt.emitServer("RPGInventory_ClosedMenu_SRV");
        alt.toggleGameControls(true);
        alt.showCursor(false);
        alt.emit("toggleChatAdminRank");

    }
    public HasOpen() { this.view != null };
}