import * as alt from 'alt';
import * as game from 'natives';
import * as ui from 'client/Helpers/UiHelper';
import { Survival } from 'client/player/Survival';
import { VoiceChat } from 'client/Voice/VoiceChat';

export class Hud {

    private _advert: number;

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

            if (alt.Player.local.getMeta("IsConnected") == null)
                return;

            if (!alt.Player.local.getMeta("IsConnected"))
                return;

            if (!game.hasStreamedTextureDictLoaded("resurrection_images"))
                game.requestStreamedTextureDict("resurrection_images", true);
            if (!game.hasStreamedTextureDictLoaded("srange_gen"))
                game.requestStreamedTextureDict("srange_gen", true);

            if (!VoiceChat.isConnected && !alt.Player.local.getMeta("IsDebug")) {
                this._advert++;
                alt.log(this._advert);
                if (this._advert >= 2000) {
                    let screenRes = game.getScreenResolution(0, 0);
                    let screenX = screenRes[1];
                    let screenY = screenRes[2];

                    game.drawRect(1, 1, screenX, screenY, 0, 0, 0, 255);
                    ui.DrawText2d("Veuillez-être connecté sur le Teamspeak: ~r~ts.resurrectionrp.fr ", 0.5, 0.5, 0.5, 4, 255, 255, 255, 255, true, true, 99);
                    ui.DrawText2d("~w~\n et avoir votre ~r~plugin activé~w~.", 0.5, 0.5, 0.5, 4, 255, 255, 255, 255, true, true, 99);
                }
                return;
            }
            this._advert = 0;

            if (this.Hide)
                return;

            if (!game.isPedSittingInAnyVehicle(alt.Player.local.scriptID))
                game.drawSprite("srange_gen", "hits_dot", 0.5, 0.5, 0.005, 0.007, 0, 255, 255, 255, 30);

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