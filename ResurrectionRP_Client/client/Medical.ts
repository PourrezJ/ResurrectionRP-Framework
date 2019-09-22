﻿import * as alt from 'alt';
import * as game from 'natives';
import * as Utils from './Utils/Utils';
import Scaleforms from './Helpers/Scaleform';

export class Blesse {

    public ID: string;
    public BlessePlayer: alt.Player;
    public Position: alt.Vector3;
    public Traited: boolean;
    public Blip: alt.Blip;
    public Marker: alt.Entity;

    constructor(player: alt.Player, id: string, position: alt.Vector3) {
        this.ID = id;
        this.BlessePlayer = player;
        this.Position = position;

        this.Blip = new alt.PointBlip(parseFloat(position.x + ""), parseFloat(position.x + "") + 100, parseFloat(position.x + ""))
        this.Blip.sprite = 280;
        this.Blip.color = 1;
        this.Blip.name = id;
        this.Blip.scale = 1;
        this.Blip.shrinked = true;
        this.Blip.shortRange = false;
         
        if (!Medical.isInMission)
        {
            alt.on('keydown', this.KeyHandler);
        }
    }

    private RemoveBindKey = () =>
    {
        alt.off('keydown', this.KeyHandler)
        alt.off('keydown', this.CallAcceptHandler)
    }

    private KeyHandler = (key) => {
        if (key == 'Y'.charCodeAt(0)) {
            alt.emitServer("ONU_ImAccept", this.BlessePlayer.id);
        }
        else if (key == 'N'.charCodeAt(0)) {
            this.CallRefuse();
        }
    }

    public CallAccept = () =>
    {
        this.Traited = true;
        this.Blip.color = 2;
        this.RemoveBindKey();

        alt.emit("successNotify", "APPEL D'URGENCE", "Vous avez accepté l'appel d'urgence du numéro: " + this.ID);
        alt.emit("notify", "APPEL D'URGENCE", "Vous ne recevrez plus d'appel, appuyez sur Y pour vous rendre disponible de nouveau.");
        alt.on('keydown', this.CallAcceptHandler);
    }

    public CallAcceptHandler = (key) => {
        if (key == 'Y'.charCodeAt(0)) {
            alt.emit("ONU_Available");
            alt.off('keydown', this.CallAcceptHandler)
        }
    }

    public CallTaken = () => {
        this.RemoveBindKey();
    }

    private CallRefuse = () => {
        alt.emit("notify", "APPEL D'URGENCE", "Vous avez refusé l'appel du numéro: " + this.ID);
        this.Destroy();
    }

    public Destroy = (keepWaypoint : boolean = false) => {
        this.Traited = false;
        this.Blip.destroy();
        this.RemoveBindKey();
        if (keepWaypoint)
            game.setWaypointOff();
    }
}

export class Medical {
    public static scaleForm: Scaleforms;
    public static BlesseList: Blesse[] = [];
    public static isInMission: boolean = false;
    public static deathMessage: string;
    public static RequestedTimeMedic: number = 0;
    public static everyTick: number;

    constructor()
    {
        Medical.scaleForm = new Scaleforms("mp_big_message_freemode");
        Medical.RequestedTimeMedic = Date.now();

        alt.onServer("ONU_IAccept", (player: alt.Player) => {
            Medical.isInMission = true;
            let call = Medical.BlesseList.find(p => p.BlessePlayer.id == player.id);
            if (call != null)
                call.CallAccept();
        });

        alt.onServer("ONU_Available", (player: alt.Player) => {
            Medical.isInMission = false;
            let call = Medical.BlesseList.find(p => p.BlessePlayer.id == player.id);
            if (call != null)
                call.CallTaken();
        });

        alt.onServer("ONU_BlesseCalled", (player: alt.Player, name: string, position: any) => {
            let call = Medical.BlesseList.find(p => p.BlessePlayer.id == player.id && p.Traited == false);
            if (call != null)
                call.Destroy();
            position = JSON.parse(position);
            Medical.BlesseList.push(new Blesse(player, name, new alt.Vector3(position.X, position.Y, position.Z) ));
            if (!Medical.isInMission)
                alt.emit("notify", "APPEL D'URGENCE", "Appuyer sur Y pour accepter, N pour refuser.");
        });

        alt.onServer("ONU_BlesseCallTaken", (player: alt.Player) => {
            alt.emit("notify", "APPEL D'URGENCE", "L'appel a déjà été pris en charge par un médecin.");
            let call = Medical.BlesseList.find(p => p.BlessePlayer.id == player.id);
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
            Medical.deathMessage = "~g~Votre appel à été reçu, quelqu'un arrive ! ";
            alt.emit("notify", "APPEL D'URGENCE", Medical.deathMessage);
        });

        alt.onServer("ONU_BlesseCalled_Accepted", (player: alt.Player, medecin: string) => {
            let blesse = Medical.BlesseList.find(p => p.BlessePlayer.id == player.id);
            if (blesse != null) {
                blesse.CallTaken();
                blesse.Blip.color = 68;

                blesse.Blip.name = `${blesse.ID} (${medecin})`;
                alt.emit("notify", "APPEL D'URGENCE", `L'appel ${blesse.ID} à était pris par le médecin ${medecin}`);
            }
        });

        alt.onServer("ONU_BlesseEnd", (player: alt.Player) => {
            let blesse = Medical.BlesseList.find(p => p.BlessePlayer.id == player.id);
            if (blesse != null)
                blesse.Destroy();
        });

        alt.onServer("ONU_PlayerDeath", (weapon: number) => {
            alt.on('keydown', this.KeyHandler);
            Medical.everyTick = alt.everyTick(this.OnTick.bind(this));
        });

        alt.onServer("ResurrectPlayer", (health: number) => {
            alt.clearEveryTick(Medical.everyTick);
            alt.off('keydown', this.KeyHandler);
            game.setPlayerHealthRechargeMultiplier(alt.Player.local.scriptID, 0);

            //game.animpostfxStop("DeathFailMPIn")
            //game.setCamEffect(0);

            game.clearPedBloodDamage(alt.Player.local.scriptID);
        });  
        
    }

    private KeyHandler(key)
    {
        if (key == 'Y'.charCodeAt(0)) {
            if (new Date(Date.now()).getTime() >= new Date(Medical.RequestedTimeMedic).getTime()) // <--- Besoin d'améliorer prochainement se check.
            {
                alt.emitServer("ONU_CallUrgenceMedic");
                Medical.RequestedTimeMedic = Date.now() + new Date().setMinutes(3)
            }
        }
        else if (key == 'R'.charCodeAt(0)) {
            alt.emitServer("IWantToDie");
        }

        //game.animpostfxPlay("DeathFailMPIn", 0, true);
        //game.setCamEffect(1);
    }

    private OnTick() {
        if (game.isPlayerDead(0))
        {
            if (Date.now() >= Medical.RequestedTimeMedic) // <--- Besoin d'améliorer prochainement se check.
            {
                Medical.deathMessage = "Appuyer sur ~g~Y~w~ pour utiliser l'appel d'urgence ou ~r~R~w~ pour en finir :(";
            } 

            if (new Date(Date.now()).getTime() >= new Date(Medical.RequestedTimeMedic).getTime()) // <--- Besoin d'améliorer prochainement se check.
            {
                Medical.deathMessage = "Appuyer sur ~g~Y~w~ pour utiliser l'appel d'urgence ou ~r~R~w~ pour en finir :(";
            } 
            else {
                if (new Date(Date.now()).getTime() - new Date(Medical.RequestedTimeMedic).getTime() > -1) {
                    Medical.deathMessage = `Il vous reste à attendre ${new Date(new Date(Date.now()).getTime() - new Date(Medical.RequestedTimeMedic).getTime()).getSeconds()} secondes pour re-contacter les secours.`;
                }
                else {
                    Medical.deathMessage = `Il vous reste à attendre ${new Date(new Date(Date.now()).getTime() - new Date(Medical.RequestedTimeMedic).getTime()).getMinutes()} minutes pour re-contacter les secours.`;
                }
            }
           
            Medical.scaleForm.call("SHOW_SHARD_WASTED_MP_MESSAGE", "~r~Vous êtes dans le Coma!", Medical.deathMessage);
            Medical.scaleForm.render2D();
            
        }

        for (var i = 0; i < Medical.BlesseList.length; i++) {
            var blesse: Blesse = Medical.BlesseList[i];
            if (blesse == undefined || blesse ==  null)
                continue;

            if (game.getDistanceBetweenCoords(parseFloat(blesse.Position.x + ""), parseFloat(blesse.Position.y + ""), parseFloat(blesse.Position.z + ""), alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, false)) {
                Medical.isInMission = false;
                alt.emitServer('ONU_BlesseRemoveBlip', blesse.BlessePlayer.id);
                blesse.Destroy();
                delete Medical.BlesseList[i];
                return;
            }
        }
    }
}