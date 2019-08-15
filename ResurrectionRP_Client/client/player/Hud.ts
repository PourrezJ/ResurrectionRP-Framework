import * as alt from 'alt';
import * as game from 'natives';
import { Game } from 'client/player/Game';

export class Hud {
    private _hide: boolean;
    public get Hide(): boolean { return this._hide; }
    private setHide(val: boolean) {
        this._hide = val;
        game.displayRadar(!val);
        game.displayHud(!val);
    }

    private playerId: number = alt.Player.local.scriptID;

    ChatIsOpen: boolean = false;
    private _money: number;
    public get Money(): number { return this._money; }
    private setMoney(val: number) {
        game.statSetInt(game.getHashKey("SP0_TOTAL_CASH"), val, false);
        game.setPedMoney(this.playerId, val);
        this._money = val;
    }

    public Browser: alt.WebView;

    constructor(money: number) {
        this._money = Math.round(money);
        this.Browser = new alt.WebView("http://resources/resurrectionrp/client/cef/hud/index.html");

        alt.onServer("UpdateMoneyHUD", (val: number) => {
            this._money = Math.round(val);
        });
        alt.onServer("LaunchProgressBar", (val: number) => {
            this.Browser.execJS("MakeProgressBar(" + val + ");")
        });
        alt.onServer("StopProgressBar", (val: number) => {
            this.Browser.execJS("FinishCB();");
        });

        alt.on("update", () => {
            if (!game.hasStreamedTextureDictLoaded("resurrection_images"))
                game.requestStreamedTextureDict("resurrection_images", true);
            if (!game.hasStreamedTextureDictLoaded("srange_gen"))
                game.requestStreamedTextureDict("srange_gen", true);

            
        });
    }
}