import * as alt from 'alt';
import * as game from 'natives';
import * as utils from './Utils/Utils'
import Scaleforms from './Helpers/Scaleform';

const MEDIC_CALL_WAIT_TIME: number = 3 * 60000;

export class Medical {
    public static scaleForm: Scaleforms;
    public static isInMission: boolean = false;
    public static deathMessage: string;
    public static RequestedTimeMedic: Date;
    public static everyTick: number;
    public static IsInComa = false;
    public static ComaTime = Date.now();

    constructor()
    {
        alt.on('keydown', this.KeyHandler);
        Medical.everyTick = alt.everyTick(this.OnTick);

        alt.on("Player_SetOutOfComa", () => Medical.IsInComa = false)

        Medical.scaleForm = new Scaleforms("mp_big_message_freemode");
        Medical.RequestedTimeMedic = new Date();


        alt.onServer("ResurrectPlayer", (health: number) => {
            game.setPlayerHealthRechargeMultiplier(alt.Player.local.scriptID, 0);
            game.setEntityHealth(alt.Player.local.scriptID, health, 0);
            //game.animpostfxStop("DeathFailMPIn")
            //game.setCamEffect(0);

            game.setPedConfigFlag(alt.Player.local.scriptID, 35, false);
            game.setPedConfigFlag(alt.Player.local.scriptID, 184, false);
            game.setPedConfigFlag(alt.Player.local.scriptID, 429, true);

            game.clearPedBloodDamage(alt.Player.local.scriptID);
        });  
    }

    private KeyHandler(key)
    {
        if (game.isPlayerDead(0)) {
            if (key == 'Y'.charCodeAt(0)) {
                alt.emitServer("InteractEmergencyCall", "emit", "ONU", "Appel d'un témoin, une personne inconsciente");
            } else if (key == 'R'.charCodeAt(0)) {
                alt.log("i want a die");
                Medical.ComaTime = Date.now() + 10000;
                Medical.IsInComa = false;
                alt.emitServer("IWantToDie");
            }
        }

        //game.animpostfxPlay("DeathFailMPIn", 0, true);
        //game.setCamEffect(1);
    }

    private OnTick() {
        if (game.isPlayerDead(0))
        {
            if (!Medical.IsInComa && Medical.ComaTime < Date.now()) {
                alt.emitServer("Player_SetInComa");
                Medical.IsInComa = true;
            }
            if (Date.now() >= Medical.RequestedTimeMedic.getTime())
                Medical.deathMessage = "Appuyer sur ~g~Y~w~ pour utiliser l'appel d'urgence ou ~r~R~w~ pour en finir :(";
            else
            {
                let waitTime = Medical.RequestedTimeMedic.getTime() - Date.now();

                if (waitTime < 60000) {
                    Medical.deathMessage = `Il reste à attendre ${Math.ceil(waitTime / 1000)} secondes pour re-contacter les secours.`
                } else {
                    Medical.deathMessage = `Il vous reste à attendre ${Math.ceil(waitTime / 60000)} minutes pour re-contacter les secours.`
                }
            }

            Medical.scaleForm.call("SHOW_SHARD_WASTED_MP_MESSAGE", "~r~Vous êtes dans le Coma!", Medical.deathMessage);
            Medical.scaleForm.render2D();
        }

    }
}