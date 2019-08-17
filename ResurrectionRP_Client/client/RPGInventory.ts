import * as alt from 'alt';
import * as game from 'natives';
import * as chat from 'client/chat/chat';

export class RPGInventoryManager {
    private loading: boolean;
    private view: alt.WebView;
    private pocket: object;
    private bag: object;
    private distant: object;
    private outfit: object;
    private give: boolean;

    constructor() {
        alt.onServer("InventoryManager_OpenMenu", (pocket: object, bag: object, distant: object, outfit: object, give: boolean) => {
            if (chat.isOpened() || game.isPauseMenuActive())
                return;

            this.pocket = pocket;
            this.bag = bag;
            this.distant = distant;
            this.outfit = outfit;
            this.give = give;

            this.loading = true;

            if (this.view == null) {

                this.view = new alt.WebView("http://resources/resurrectionrp/client/cef/inventory/index.html");
                this.view.focus();
                alt.showCursor(true);
            }
            else {
                this.view.emit("loadInventory",
                    this.pocket,
                    (this.bag != null) ? this.bag : null,
                    (this.distant) ? this.distant : null,
                    (this.outfit) ? this.outfit : null,
                    (this.give) ? "true" : "false");
                this.loading = false;
            }

            alt.emit("toggleChat");

        });

        this.view.on("LoadInventoryItems", () => {
            if (!this.view)
                return;
            this.view.emit("loadInventory",
                this.pocket,
                (this.bag != null) ? this.bag : null,
                (this.distant) ? this.distant : null,
                (this.outfit) ? this.outfit : null,
                (this.give) ? "true" : "false");
            this.loading = false;
        });

        alt.onServer("InventoryManager_CloseMenu", () => this.CloseMenu());
        alt.on("InventoryManager_CloseMenu", () => this.CloseMenu());

        this.view.on("inventoryUseItem", (arg1: any, arg2: any, arg3: any) => alt.emitServer("RPGInventory_UseItem", arg1, arg2, arg3));
        this.view.on("inventoryDropItem", (arg1: any, arg2: any, arg3: any, arg4: any) => alt.emitServer("RPGInventory_DropItem", arg1, arg2, arg3, arg4));
        this.view.on("InventoryChangeItemPrice", (arg1: any, arg2: any, arg3: any, arg4: any) => alt.emitServer("RPGInventory_PriceItemInventory_SRV", arg1, arg2, arg3, arg4));
        this.view.on("inventoryGiveItem", (arg1: any, arg2: any, arg3: any, arg4: any) => alt.emitServer("RPGInventory_GiveItem", arg1, arg2, arg3, arg4));
        this.view.on("RPGInventory_SwitchItemInventory", (arg1: any, arg2: any, arg3: any, arg4: any, arg5: any) => alt.emitServer("RPGInventory_SwitchItemInventory_SRV", arg1, arg2, arg3, arg4, arg5));
        this.view.on("inventorySplitItem", (arg1: any, arg2: any, arg3: any, arg4: any, arg5: any, arg6: any, arg7: any) => alt.emitServer("RPGInventory_SplitItemInventory_SRV", arg1, arg2, arg3, arg4, arg5, arg6, arg7));

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
        alt.emit("toggleChat");

    }
    public HasOpen() { this.view != null };
}