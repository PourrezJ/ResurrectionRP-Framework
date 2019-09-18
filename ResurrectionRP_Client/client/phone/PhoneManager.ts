import * as alt from 'alt';
import * as game from 'natives';
import * as utils from '../Utils/Utils';
import * as voice from '../Voice/VoiceChat';

var isPhoneOpen: boolean = false;

export default class PhoneManager {

    public browser: alt.WebView = null;
    public LockControls: boolean = false;
    private animStage: number = 0;

    constructor()
    {
        alt.onServer("OpenPhone", (idk0: any, idk1: any, incomingCall: boolean, contactNumber: string, contactName: string) => {
            if (game.isPauseMenuActive())
                return;

            isPhoneOpen = true;
            game.freezePedCameraRotation(alt.Player.local.scriptID);

            if (incomingCall)
                this.browser = new alt.WebView(`http://resource/client/cef/phone/oncall.html?incomingCall=true&number=${contactNumber}&name=${contactName}`);
            else
                this.browser = new alt.WebView(`http://resource/client/cef/phone/home.html?newMessages=${idk0}&phoneSettings=${idk1}`);

            alt.showCursor(true);

            alt.setTimeout(() => {
                if (this.browser != null) this.browser.emit('loadPhoneSettings', idk1);
            }, 1000);

            this.browser.focus();

            this.browser.on("SavePhoneSettings", (arg) => alt.emitServer("PhoneMenuCallBack", "SavePhoneSettings", arg));
            this.browser.on("GetContacts", (arg) => alt.emitServer("PhoneMenuCallBack", "GetContacts", arg));
            this.browser.on("AddOrEditContact", (arg) => alt.emitServer("PhoneMenuCallBack", "AddOrEditContact", arg));
            this.browser.on("RemoveContact", (arg) => alt.emitServer("PhoneMenuCallBack", "RemoveContact", arg));
            this.browser.on("getConversations", (arg) => alt.emitServer("PhoneMenuCallBack", "getConversationsV2"));

            this.browser.on("callTaxi", () => alt.emit("alertNotify", "Les services de taxi ne sont pas encore en ville !", 10000));
            this.browser.on("callUrgences", () => alt.emitServer("ONU_CallUrgenceMedic"));
            //this.browser.on("callPolice", () => alt.emitServer("ONU_CallUrgenceMedic")); NEED MENU MANAGER

            this.browser.on("getMessages", (arg, arg2) => alt.emitServer("PhoneMenuCallBack", "GetMessages", arg2));
            this.browser.on("sendMessage", (arg, arg2) => alt.emitServer("PhoneMenuCallBack", "SendMessage", arg, arg2));
            this.browser.on("deleteConversation", (arg, arg2) => { if (this.browser != null) { this.browser.url = "http://resource/client/cef/phone/messages.html" } });
            this.browser.on("deleteConversation", (arg, arg2) => alt.emitServer("PhoneMenuCallBack", "DeleteConversation", arg));
            alt.onServer("ContactEdited", (args) => { if (this.browser != null) { this.browser.url = "http://resource/client/cef/phone/contacts.html" } });
            alt.onServer("ConversationsReturnedV2", (args) => { if (this.browser != null) { this.browser.emit("loadConversations", args) } });
            alt.onServer("MessagesReturned", (args) => { if (this.browser != null) { this.browser.emit("loadMessages", args) } });
            alt.onServer("ContactReturned", (args) => { if (this.browser != null) { this.browser.emit("loadContacts", args) } });
            alt.onServer("ClosePhone", () => this.ClosePhone());
            alt.onServer("initiatedCall", (phoneNumber: string, contactName: string) => { if (this.browser != null) { this.browser.url = 'http://resource/client/cef/phone/oncall.html?incomingCall=true&number=' + phoneNumber + '&name=' + contactName }  });

            alt.on("ClosePhone", () => this.ClosePhone());

            this.browser.on("initiateCall", (arg, arg2) => alt.emitServer("PhoneMenuCallBack", "initiateCall", arg));
            this.browser.on("acceptCall", (arg, arg2) => alt.emitServer("PhoneMenuCallBack", "acceptCall", arg));

            this.browser.on("cancelCall", (arg, arg2) => alt.emitServer("PhoneMenuCallBack", "cancelCall", arg));

            this.browser.on("canceledCall", (arg, arg2) => {
                utils.playAnimation(
                    (alt.Player.local.vehicle != null) ? "cellphone@in_car@ds" : (alt.Player.local.model == 2627665880) ? "cellphone@female" : "cellphone@",
                    "cellphone_text_in",
                    8,
                    -1,
                    0
                );

                this.animStage = 0;
                if (this.browser != null)
                    this.browser.emit("callEvent", "canceled");
            });

            this.browser.on("endCall", (arg, arg2) => alt.emitServer("PhoneMenuCallBack", "endCall", arg));

            alt.onServer("endedCall", (playerName: string) => {
                if (this.browser != null)
                    this.browser.emit("callEvent", "ended");

                voice.VoiceChat.OnEndCall(playerName)
            });

            alt.onServer("PlayRingPhone", (entity: any) => {
                game.playSoundFromEntity(-1, "Beep_Green", entity, "DLC_HEIST_HACKING_SNAKE_SOUNDS", false, 0);
            });

            alt.onServer("StartedCall", (player: alt.Player) => {
                this.animStage = 3;

                if (this.browser != null)
                    this.browser.emit("callEvent", "started");

                voice.VoiceChat.OnEstablishCall(player.getSyncedMeta("Voice_TeamSpeakName").toString());
                
            });

            alt.everyTick(() => {
                if (this.browser == null)
                    return;
                // Ouverture du téléphone
                if (this.animStage == 0 && (game.isEntityPlayingAnim(alt.Player.local.scriptID, "cellphone@in_car@ds", "cellphone_text_in", 3) || game.isEntityPlayingAnim(alt.Player.local.scriptID, (alt.Player.local.model == 2627665880) ? "cellphone@female" : "cellphone@", "cellphone_text_in", 3))) {
                    if (game.getEntityAnimCurrentTime(alt.Player.local.scriptID, (alt.Player.local.vehicle != null) ? "cellphone@in_car@ds" : (alt.Player.local.model == 2627665880) ? "cellphone@female" : "cellphone@", "cellphone_text_in") >= 0.95) {
                        this.animStage = 1;
                    }
                }
                // Animation en main
                else if (this.animStage == 1) {

                    utils.playAnimation(
                        (alt.Player.local.vehicle != null) ? "cellphone@in_car@ds" : (alt.Player.local.model == 2627665880) ? "cellphone@female" : "cellphone@",
                        "cellphone_text_read_base",
                        8,
                        -1,
                        0
                    );

                    this.animStage = 2;
                }
                // Animation Appel
                else if (this.animStage == 3) {

                    utils.playAnimation(
                        (alt.Player.local.vehicle != null) ? "cellphone@in_car@ds" : (alt.Player.local.model == 2627665880) ? "cellphone@female" : "cellphone@",
                        "cellphone_call_listen_base",
                        8,
                        -1,
                        0
                    );

                    this.animStage = 4;
                }

                utils.DisEnableControls(false);
            });
        });
    }

    public ClosePhone = () => {
        if (this.browser == null)
            return;

        utils.playAnimation(
            (alt.Player.local.vehicle != null) ? "cellphone@in_car@ds" : (alt.Player.local.model == 2627665880) ? "cellphone@female" : "cellphone@",
            "cellphone_text_out",
            8,
            -1,
            0
        );

        alt.showCursor(false);
        this.browser.destroy();
        this.browser = null;
        game.freezePedCameraRotation(alt.Player.local.scriptID);
        isPhoneOpen = false;
        alt.emit("toggleChatAdminRank");
    }

    public static IsPhoneOpen() {
        return isPhoneOpen;
    }
}