﻿import * as alt from 'alt';
import * as game from 'natives';

export class Blesse {

    public ID: string;
    public BlessePlayer: alt.Player;
    public Position: alt.Vector3;
    public Traited: boolean;
    public Blip: alt.Blip;
    public Marker: alt.Entity;

    constructor(player: alt.Player, id: string, position: alt.Vector3, enMission: boolean) {
        this.ID = id;
        this.BlessePlayer = player;
        this.Position = position;

        if (!enMission)
        {
            alt.on('keydown', this.KeyHandler);
        }
    }

    private RemoveBindKey()
    {
        alt.off('keydown', this.KeyHandler)
        alt.off('keydown', this.CallAcceptHandler)
    }

    private KeyHandler(key) {
        if (key == 'Y'.charCodeAt(0)) {
            alt.emitServer("ONU_IAccept_Server", this.BlessePlayer.id);
        }
        else if (key == 'N'.charCodeAt(0)) {
            this.CallRefuse();
        }
    }

    private CallAccept()
    {
        this.Traited = true;
        this.Blip.color = 2;
        this.RemoveBindKey();

        alt.emit("successNotify", "APPEL D'URGENCE", "Vous avez accepté l'appel d'urgence du numéro: " + this.ID);
        alt.emit("notify", "APPEL D'URGENCE", "Vous ne recevrez plus d'appel, appuyez sur Y pour vous rendre disponible de nouveau.");
        alt.on('keydown', this.CallAcceptHandler);
    }

    private CallAcceptHandler(key) {
        if (key == 'Y'.charCodeAt(0)) {
            alt.emit("ONU_Available");
        }
    }

    public CallTaken() {
        this.RemoveBindKey();
    }

    private CallRefuse() {
        alt.emit("notify", "APPEL D'URGENCE", "Vous avez refusé l'appel du numéro: " + this.ID);
        this.Destroy();
    }

    public Destroy(keepWaypoint : boolean = false) {
        this.Traited = false;
        this.Blip.destroy();
        this.RemoveBindKey();
        if (keepWaypoint)
            game.setWaypointOff();
    }
}

export class Medical {
    private BlesseList: Blesse[];
    private isInMission: boolean;
    private deathMessage: string;
    private RequestedTimeMedic: Date;

    constructor()
    {
        alt.onServer("ONU_IAccept_Client", (player: alt.Player) => {
            this.isInMission = true;
            let call = this.BlesseList.find(p => p.BlessePlayer.id == player.id);
            if (call != null)
                call.CallTaken();
        });

        alt.onServer("ONU_Available", (player: alt.Player) => {
            this.isInMission = false;
            let call = this.BlesseList.find(p => p.BlessePlayer.id == player.id);
            if (call != null)
                call.CallTaken();
        });

        alt.onServer("ONU_BlesseCalled", (player: alt.Player, name: string, position: alt.Vector3) => {
            let call = this.BlesseList.find(p => p.BlessePlayer.id == player.id && p.Traited == false);
            if (call != null)
                call.Destroy();

            this.BlesseList.push(new Blesse(player, name, position, this.isInMission));
            if (!this.isInMission)
                alt.emit("notify", "APPEL D'URGENCE", "Appuyer sur Y pour accepter, N pour refuser.");
        });

        alt.onServer("ONU_BlesseCallTaken", (player: alt.Player) => {
            alt.emit("notify", "APPEL D'URGENCE", "L'appel a déjà été pris en charge par un médecin.");
            let call = this.BlesseList.find(p => p.BlessePlayer.id == player.id);
            if (call != null)
                call.CallTaken();
        });

        alt.onServer("ONU_Callback", (count: number) => {
            let msg = count == 0 ?
                "~r~Aucun médecin de disponible."
                : "~g~Appel transmit!";
            alt.emit("notify", "APPEL D'URGENCE", msg);
        });

        alt.onServer("ONU_CallbackAccept", () => {
            //Game.Instance.KeyHandler.Remove((int)ConsoleKey.R);
            this.deathMessage = "~g~Votre appel à été reçu, quelqu'un arrive ! ";
            alt.emit("notify", "APPEL D'URGENCE", this.deathMessage);
        });

        alt.onServer("ONU_BlesseCalled_Accepted", (player: alt.Player, medecin: string) => {
            let blesse = this.BlesseList.find(p => p.BlessePlayer.id == player.id);
            if (blesse != null) {
                blesse.CallTaken();
                blesse.Blip.color = 68;

                blesse.Blip.name = `${blesse.ID} (${medecin})`;
                alt.emit("notify", "APPEL D'URGENCE", `L'appel ${blesse.ID} à était pris par le médecin ${medecin}`);
            }
        });

        alt.onServer("ONU_BlesseEnd", (player: alt.Player) => {
            let blesse = this.BlesseList.find(p => p.BlessePlayer.id == player.id);
            if (blesse != null)
                blesse.Destroy();
        });

        alt.onServer("ONU_PlayerDeath", (weapon: number) => {
            alt.on('keydown', this.KeyHandler);
        });

        alt.onServer("ResurrectPlayer", () => {
            //Game.Instance.KeyHandler.Remove((int)ConsoleKey.Y); TODO
            //Game.Instance.KeyHandler.Remove((int)ConsoleKey.R);

            game.setPlayerHealthRechargeMultiplier(alt.Player.local.scriptID, 0);
            game.animpostfxStop("DeathFailMPIn")
            game.setCamEffect(0);

            game.setFadeInAfterDeathArrest(false);
            game.setFadeOutAfterArrest(false);
            game.pauseDeathArrestRestart(true);
            game.setFadeInAfterLoad(false);
            game.setFadeOutAfterDeath(false);
        });  
    }

    private KeyHandler(key)
    {
        if (key == 'Y'.charCodeAt(0)) {
            //if (Date.now(). >= this.RequestedTimeMedic) {
            //    Events.CallRemote("ONU_CallUrgenceMedic");
            //    RequestedTimeMedic = DateTime.Now.AddMinutes(3);
            //}
        }
        else if (key == 'R'.charCodeAt(0)) {

        }
    }
}