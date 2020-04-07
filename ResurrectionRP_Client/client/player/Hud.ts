import * as alt from 'alt-client';
import * as game from 'natives';
import * as ui from '../Helpers/UiHelper';
import * as veh from '../Vehicle/Vehicle';
import * as chat from '../Chat/Chat'
import { Survival } from '../Player/Survival';
import { VoiceChat } from '../Voice/VoiceChat';

export class Hud {

    private _advert: number = 0;

    private _hide: boolean;
    public get Hide(): boolean { return this._hide; }
    public setHide(val: boolean) {
        this._hide = val;
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
        this.Browser = new alt.WebView("http://resource/client/cef/hud/index.html", true);

        alt.on('hideHud', (value: boolean) => {
            this.setHide(value);
        });

        alt.on('keyup', (key) => {
            if (key == 0x76) {
                this._hide = !this._hide;
                game.displayHud(!this._hide);
                game.displayRadar(!this._hide);
                chat.hide(this._hide);
                veh.hideHud(this._hide);

                if (this.Browser != null)
                    this.Browser.emit("showHide", this._hide);
            }
        });

        alt.onServer("UpdateMoneyHUD", (val: number) => {
            this._money = Math.round(val);
        });

        alt.onServer("LaunchProgressBar", (val: number) => {
            this.Browser.emit("MakeProgressBar", val)
        });

        alt.onServer("StopProgressBar", (val: number) => {
            this.Browser.emit("FinishCB");
        });

        alt.everyTick(() => {

            if (alt.Player.local.getMeta("IsConnected") == null)
                return;

            if (!alt.Player.local.getMeta("IsConnected"))
                return;

            if (!VoiceChat.isConnected && !alt.Player.local.getMeta("IsDebug") && alt.Player.local.getMeta("LevelRank") != null) {
                if (alt.Player.local.getMeta("LevelRank") > 0)
                    return;

                this._advert++;
                if (this._advert >= 2000) {
                    let screenRes = game.getScreenResolution(0, 0);
                    let screenX = screenRes[1];
                    let screenY = screenRes[2];

                    game.drawRect(1, 1, screenX, screenY, 0, 0, 0, 255, false);
                    ui.DrawText2d("Veuillez-être connecté sur le Teamspeak: ~r~address.ts ", 0.5, 0.5, 0.5, 4, 255, 255, 255, 255, true, true, 99);
                    ui.DrawText2d("~w~\n et avoir votre ~r~plugin activé~w~.", 0.5, 0.5, 0.5, 4, 255, 255, 255, 255, true, true, 99);
                }
                return;
            }

            if (VoiceChat.isRadioTalking && VoiceChat.radioChannel != "")
                ui.DrawText2d("Transmission en cours (" + VoiceChat.radioChannel + "MHz)", 0.1, 0.75, 0.4, 4, 255, 255, 255, 255, true, true, 99);

            this._advert = 0;
        });

        alt.setInterval(() => {
            if (this.Browser != null) {
                let range = "Parler";
                if (alt.Player.local.getSyncedMeta("Voice_VoiceRange") != null)
                    range = alt.Player.local.getSyncedMeta("Voice_VoiceRange");
                this.Browser.emit("setHUD", Survival.Hunger, Survival.Thirst, VoiceChat.isTalking, range, this.Money, VoiceChat.isMicrophoneMuted);
            }
        }, 250);
    }
}