import * as alt from 'alt';
import * as game from 'natives';
import { Survival } from 'client/player/Survival';
import { VoiceChat } from 'client/Voice/VoiceChat';

export class Hud {
    private _hide: boolean;
    public get Hide(): boolean { return this._hide; }
    private setHide(val: boolean) {
        this._hide = val;
        game.displayRadar(!val);
        game.displayHud(!val);
    }

    ChatIsOpen: boolean = false;
    private _money: number;
    public get Money(): number { return this._money; }
    private setMoney(val: number) {
        this._money = val;
    }

    public Browser: alt.WebView;

    constructor(money: number) {
        this._money = Math.round(money);
        this.Browser = new alt.WebView("http://resource/client/cef/hud/index.html");

        alt.onServer("UpdateMoneyHUD", (val: number) => {
            this._money = Math.round(val);
        });

        alt.onServer("LaunchProgressBar", (val: number) => {
            this.Browser.emit("MakeProgressBar", val)
        });

        alt.onServer("StopProgressBar", (val: number) => {
            this.Browser.emit("FinishCB");
        });

        alt.on("update", () => {

            if (!game.hasStreamedTextureDictLoaded("resurrection_images"))
                game.requestStreamedTextureDict("resurrection_images", true);
            if (!game.hasStreamedTextureDictLoaded("srange_gen"))
                game.requestStreamedTextureDict("srange_gen", true);

            if (this.Hide)
                return;
            
            if (this.Browser != null)
            {
                let range = "Parler";
                if (alt.Player.local.getSyncedMeta("Voice_VoiceRange") != null)
                    range = alt.Player.local.getSyncedMeta("Voice_VoiceRange");

                this.Browser.emit("setHUD", Survival.Hunger, Survival.Thirst, VoiceChat.isTalking, range, this.Money, VoiceChat.isMicrophoneMuted);
            }
        });
    }
}