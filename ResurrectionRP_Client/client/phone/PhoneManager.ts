import * as alt from 'alt-client';
import * as game from 'natives';
import * as utils from '../Utils/Utils';
import * as voice from '../Voice/VoiceChat';
import * as chat from '../chat/chat';
import { Interaction } from '../player/Interaction';

var isPhoneOpen: boolean = false;

export default class PhoneManager {

    public browser: alt.WebView = null;
    public animStage: number = 0;
    private onTick: number;
    private callbackTime: number = Date.now(); 

    constructor()
    {
        alt.onServer("OpenPhone", (idk0: any, idk1: any, incomingCall: boolean, contactNumber: string, contactName: string) => {
            if (game.isPauseMenuActive())
                return;

            this.animStage = 0;
            isPhoneOpen = true;
            chat.hide(true);

            if (this.browser != null)
                this.ClosePhone();

            if (incomingCall)
                this.browser = new alt.WebView(`http://resource/client/cef/phone/oncall.html?incomingCall=true&number=${contactNumber}&name=${contactName}`, true);
            else
                this.browser = new alt.WebView(`http://resource/client/cef/phone/home.html?newMessages=${idk0}&phoneSettings=${idk1}`, true);

            alt.showCursor(true);

            alt.setTimeout(() => {
                if (this.browser != null) this.browser.emit('loadPhoneSettings', idk1);
            }, 1000);

            this.browser.focus();

            this.browser.on("SavePhoneSettings", (arg) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "SavePhoneSettings", arg);
            });

            this.browser.on("GetContacts", (arg) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "GetContacts", arg);
            });

            this.browser.on("AddOrEditContact", (arg) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "AddOrEditContact", arg);
            });

            this.browser.on("RemoveContact", (arg) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "RemoveContact", arg);
            });

            this.browser.on("getConversations", (arg) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "getConversationsV2");
            });

            this.browser.on("callTaxi", () => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

               alt.emit("alertNotify", "Les services de taxi ne sont pas encore en ville !", 10000);
            });

            this.browser.on("callUrgences", () => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                this.ClosePhone();
                let inputView = new alt.WebView("http://resource/client/cef/userinput/input.html", true);
                inputView.focus();
                alt.showCursor(true);
                alt.toggleGameControls(false);

                inputView.emit('Input_Data', 999, "");

                inputView.on('Input_Submit', (text) => {
                    inputView.destroy();
                    alt.showCursor(false);
                    alt.toggleGameControls(true);
                    Interaction.SetCanClose(true);
                    alt.emitServer("InteractEmergencyCall", "emit", "EMS", ""+text);
                });
            });

            this.browser.on("callPolice", () => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                this.ClosePhone();
                let inputView = new alt.WebView("http://resource/client/cef/userinput/input.html", true);
                inputView.focus();
                alt.showCursor(true);
                alt.toggleGameControls(false);

                inputView.emit('Input_Data', 999, "");

                inputView.on('Input_Submit', (text) => {
                    inputView.destroy();
                    alt.showCursor(false);
                    alt.toggleGameControls(true);
                    Interaction.SetCanClose(true);
                    alt.emitServer("InteractEmergencyCall", "emit", "LSPD", ""+text);                   
                });
            });

            this.browser.on("getMessages", (arg, arg2) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "GetMessages", arg2);
            });

            this.browser.on("sendMessage", (arg, arg2) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "SendMessage", arg, arg2);
                Interaction.SetCanClose(true);
            });

            this.browser.on("deleteConversation", (arg, arg2) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "DeleteConversation", arg);
            });

            this.browser.on("initiateCall", (arg, arg2) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

               alt.emitServer("PhoneMenuCallBack", "initiateCall", arg);
            });

            this.browser.on("acceptCall", (arg, arg2) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "acceptCall", arg);
            });

            this.browser.on("cancelCall", (arg, arg2) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "cancelCall", arg);
            });

            this.browser.on("endCall", (arg, arg2) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.emitServer("PhoneMenuCallBack", "endCall", arg);
            });

            this.browser.on("CanClose", (canClose: boolean) => {
                Interaction.SetCanClose(canClose);
            });

            alt.onServer("ContactEdited", (args) => {
                if (this.browser != null) {
                    this.browser.url = "http://resource/client/cef/phone/contacts.html";
                }
            });

            alt.onServer("ConversationsReturnedV2", (args) => {
                args = JSON.parse(args);

                for (var key in args) {
                    args[key].lastReadDate = args[key].lastReadDate.replace("2038", "2019");
                    args[key].lastMessageDate = args[key].lastMessageDate.replace("2038", "2019");
                }

                args = JSON.stringify(args);

                if (this.browser != null) {
                    this.browser.emit("loadConversations", args);
                }
            });

            alt.onServer("MessagesReturned", (args) => {
                if (this.browser != null)
                    this.browser.emit("loadMessages", args)
            });

            alt.onServer("ContactReturned", (args) => { if (this.browser != null) { this.browser.emit("loadContacts", args) } });
            alt.onServer("ClosePhone", () => this.ClosePhone());

            alt.onServer("initiatedCall", (phoneNumber: string, contactName: string) => {
                if (this.browser != null)
                    this.browser.url = `http://resource/client/cef/phone/oncall.html?number=${phoneNumber}&name=${contactName}`;
            });


            alt.onServer("deleteConversation", (arg, arg2) => { if (this.browser != null) { this.browser.url = "http://resource/client/cef/phone/messages.html" } });

            alt.on("ClosePhone", () => this.ClosePhone());
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

            alt.onServer("CanceledCall", () => {
                if (this.browser != null)
                    this.browser.emit("callEvent", "canceled");

                this.animStage = 0;

                utils.playAnimation(
                    (alt.Player.local.vehicle != null) ? "cellphone@in_car@ds" : (alt.Player.local.model == 2627665880) ? "cellphone@female" : "cellphone@",
                    "cellphone_text_in",
                    8,
                    -1,
                    49
                );
            });

            this.onTick = alt.everyTick(() => {
                if (this.browser == null)
                    return;

                utils.DisEnableControls(false);

                if (!Interaction.GetCanClose())
                    game.disableAllControlActions(0);  

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
                        49
                    );
                    this.animStage = 4;
                }
            });
        });
    }

    private CheckMultipleCallbak() {
        const time = Date.now() - this.callbackTime;

        if (time < 100) {
            alt.logWarning('Phone multiple callback: ' + time + 'ms');
            return true;
        }

        this.callbackTime = Date.now();
        return false;
    }

    public ClosePhone = () => {
        if (this.browser == null)
            return;

        alt.clearEveryTick(this.onTick);

        game.clearPedTasks(alt.Player.local.scriptID);

        utils.playAnimation(
            (alt.Player.local.vehicle != null) ? "cellphone@in_car@ds" : "cellphone@", "cellphone_text_out", 8, -1, 0
        );


        alt.showCursor(false);
        this.browser.destroy();
        this.browser = null;    
        isPhoneOpen = false;
        this.animStage = 0;
        chat.hide(false);
    }

    public static IsPhoneOpen() {
        return isPhoneOpen;
    }
}