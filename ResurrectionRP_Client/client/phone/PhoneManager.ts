import * as alt from 'alt';
import * as game from 'natives';
import * as Game from 'client/player/game';

export class PhoneManager {

    public browser: alt.WebView = null;
    public LockControls: boolean = false;

    constructor() {

        alt.onServer("OpenPhone", this.OpenPhone);


    }
    // REGION pHONE RELATIVE ON OPEN
    OpenPhone = (idk0: any, idk1: any, incomingCall: boolean, contactNumber: string, contactName: string) => {
        alt.logWarning("Opened Phone");
        if (game.isPauseMenuActive())
            return;
        //Game.Game.closeAllMenus();
        
        game.playEntityAnim(
            alt.Player.local.scriptID,
            (alt.Player.local.vehicle != null) ? "cellphone@in_car@ds" : (alt.Player.local.model == 2627665880) ? "cellphone@female" : "cellphone@",
            "cellphone_text_in",
            3,
            false,
            true,
            true,
            -1,
            -1
        );

        if (incomingCall == true)
            this.browser = new alt.WebView('http://resources/resurrectionrp/client/cef/phone/oncall.html?incomingCall=true&number=' + contactNumber + '&name=' + contactName);
        else
            this.browser = new alt.WebView('http://resources/resurrectionrp/client/cef/phone/home.html?newMessages=' + idk0 + '?phoneSettings=' + JSON.stringify(idk1));

        this.browser.focus();
        alt.setTimeout(() => {
            this.browser.emit("loadPhoneSettings", JSON.stringify(idk1));
        }, 100);

        this.browser.on("SavePhoneSettings", (arg) => alt.emitServer("SavePhoneSettings", arg));

        this.browser.on("GetContacts", (arg) => alt.emitServer("GetContacts", arg));
        alt.onServer("ContactReturned", (arg) => this.browser.emit("loadContacts", arg));
        alt.onServer("ContactEdited", (args) => { if (this.browser != null) { this.browser.url = "http://resources/resurrectionrp/client/cef/phone/contacts.html" } });
        alt.onServer("ConversationsReturnedV2", (args) => { if (this.browser != null) { this.browser.emit("loadConversations", JSON.stringify(args)) } });
        this.browser.on("AddOrEditContact", (arg) => alt.emitServer("AddOrEditContact", arg));
        this.browser.on("RemoveContact", (arg) => alt.emitServer("RemoveContact"));
        this.browser.on("getConversations", (arg) => alt.emitServer("getConversations"));


        this.browser.on("callTaxi", () => alt.emit("alertNotify", "Les services de taxi ne sont pas encore en ville !", 10000));
        this.browser.on("callUrgences", () => alt.emitServer("ONU_CallUrgenceMedic"));
        //this.browser.on("callPolice", () => alt.emitServer("ONU_CallUrgenceMedic")); NEED MENU MANAGER

        this.browser.on("getMessages", (arg, arg2) => alt.emitServer("getMessages", arg2));
        this.browser.on("sendMessage", (arg, arg2) => alt.emitServer("SendMessage", arg, arg2));
        this.browser.on("deleteConversation", (arg, arg2) => { if (this.browser != null) { this.browser.url = "http://resources/resurrectionrp/client/cef/phone/messages.html" } });
        this.browser.on("deleteConversation", (arg, arg2) => alt.emitServer("deleteConversation"));

        alt.on("ClosePhone", () => this.ClosePhone());
        alt.onServer("ClosePhone", () => this.ClosePhone());


        this.browser.on("initiateCall", (arg, arg2) => alt.emitServer("initiateCall", arg));
        this.browser.on("initiatedCall", (arg, arg2) => { if (this.browser != null) { this.browser.url = 'http://resources/resurrectionrp/client/cef/phone/oncall.html?incomingCall=true&number=' + arg + '&name=' + arg2 } } );
        this.browser.on("acceptCall", (arg, arg2) => alt.emitServer("acceptCall", arg));
        this.browser.on("cancelCall", (arg, arg2) => alt.emitServer("cancelCall", arg));
        this.browser.on("canceledCall", (arg, arg2) => {
/*            Events.CallRemote("canceledCall", args[0]);
            _animStage = 0;
            PlayerSyncManager.PlaySyncedAnimation((RAGE.Elements.Player.LocalPlayer.Vehicle != null) ? "cellphone@in_car@ds" : (RAGE.Elements.Player.LocalPlayer.Model == 2627665880) ? "cellphone@female" : "cellphone@", "cellphone_text_in", 3, -1, -1, (AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody));
            if (browser != null)
                browser.ExecuteJs("callEvent('canceled')");*/
            // CA ME CASSE LES COUILLES TOUS CES EVENTS
        });
        this.browser.on("endCall", (arg, arg2) => alt.emitServer("endCall", arg));

        /* ALLER HOP JE FINIRAI UN AUTRE JOUR ** Protocole
         * 
         *
            Events.Add("endedCall", (args) =>
            {
                if (browser != null)
                    browser.ExecuteJs("callEvent('ended')");

                SaltyClient.Voice.OnEndCall((string)args[0]);
            });

            Events.Add("PlayRingPhone", (args) =>
            {
                RAGE.Game.Audio.PlaySoundFromEntity(-1, "Beep_Green", (int)args[0], "DLC_HEIST_HACKING_SNAKE_SOUNDS", false, 0);
            });
         * 
         * */

        this.browser.on("StartedCall", this.StartedCall)

    }

    public StartedCall = (args) => {
/*        _animStage = 3; TODO
        if (browser != null)
            browser.ExecuteJs("callEvent('started')");

        if (ushort.TryParse(args[0].ToString(), out ushort id)) { SALTY TCHAT
            var client = RAGE.Elements.Entities.Players.GetAtRemote(id);
            if (client != null) {
                SaltyClient.Voice.OnEstablishCall(client.GetSharedData(SaltyShared.SharedData.Voice_TeamSpeakName).ToString());
            }
        }*/
    }

    public ClosePhone = () => {
        if (this.browser == null)
            return;
        alt.showCursor(false);
        this.browser.destroy();
        this.browser = null;

        alt.emit("toggleChatAdminRank");
    }
}