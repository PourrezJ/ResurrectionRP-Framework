import * as alt from 'alt-client';
import * as game from 'natives';
import * as chat from './chat/chat';
import * as speedometer from './vehicle/vehicle';
import * as xtreamMenu from './menus/xtreamMenu/xtreamMenuManager';
import * as utils from './Utils/utils';
import * as login from './login/Login';
import * as nightClub from './Env/NightClub';
import * as trains from './Env/Trains';
import * as PlayerCustomization from './player/PlayerCustomization';
import { Game } from './player/Game';
import { OpenCharCreator } from './Creator/Creator';
import { NetworkingEntityClient } from './Streamer/NetworkingEntityClient';
import { Notify } from './Notify/Notify';
import menuManager from './MenuManager/MenuManager';
import { Admin } from './Utils/Admin';
import { LSPDManager } from './LSPDCall';
import { Doors } from './Env/Doors';
import * as CustomEvents from './Utils/CustomEvents';


var GameClass: Game;

const handlingproperties = [
    'mass',
    'initialDragCoeff',
    'percentSubmerged',
    'centreOfMassOffset',
    'inertiaMultiplier',
    'driveBiasFront',
    'initialDriveGears',
    'initialDriveForce',
    'driveInertia',
    'clutchChangeRateScaleUpShift',
    'clutchChangeRateScaleDownShift',
    'initialDriveMaxFlatVel',
    'breakForce',
    'brakeBiasFront',
    'handBrakeForce',
    'steeringLock',
    'tractionCurveMax',
    'tractionCurveMin',
    'tractionCurveLateral',
    'tractionSpringDeltaMax',
    'lowSpeedTractionLossMult',
    'camberStiffnesss',
    'tractionBiasFront',
    'tractionLossMult',
    'suspensionForce',
    'suspensionCompDamp',
    'suspensionReboundDamp',
    'suspensionUpperLimit',
    'suspensionLowerLimit',
    'suspensionRaise',
    'suspensionBiasFront',
    'antiRollBarForce',
    'antiRollBarBiasFront',
    'rollCentreHeightFront',
    'rollCentreHeightRear',
    'collisionDamageMult',
    'weaponDamageMult',
    'deformationDamageMult',
    'engineDamageMult',
    'petrolTankVolume',
    'oilVolume',
    'seatOffsetDistX',
    'seatOffsetDistY',
    'seatOffsetDistZ',
    'monetaryValue',
    'modelFlags',
    'handlingFlags',
    'damageFlags'
];

const init = async () => {
    try {
        alt.log('Chargement des events.');

        new Doors();

        alt.onServer("PlayerInitialised", (
            StaffRank: number,
            IdentiteName: string,
            Money: number,
            Thirst: number,
            Hunger: number,
            Time: string,
            Weather: string,
            WeatherWind: number,
            WeatherWindDirection: number,
            isDebug: boolean,
            Position: Vector3
        ) => {
            PlayerCustomization.init();
            GameClass = new Game(StaffRank, IdentiteName, Money, Thirst, Hunger, Time, Weather, WeatherWind, WeatherWindDirection, isDebug, Position);
            game.freezeEntityPosition(alt.Player.local.scriptID, false);
        });

        alt.onServer('OpenCreator', () => {
            OpenCharCreator();
        });

        alt.onServer("togglePlayerControl", (value: boolean) => {
            alt.toggleGameControls(value);
        });

        alt.everyTick(() => {
            game.drawRect(0, 0, 0, 0, 0, 0, 0, 0, false);
        });

        alt.on("disconnect", () => {
            alt.log("disconnect detected.");

            game.animpostfxStop("DeathFailMPIn")
            game.setCamEffect(0);

            game.setFadeInAfterDeathArrest(false);
            game.setFadeOutAfterArrest(false);
            game.pauseDeathArrestRestart(true);
            game.setFadeInAfterLoad(false);
            game.setFadeOutAfterDeath(false);
        });

        alt.onServer("GetHandling", (model: number) => {
            let handling : any = alt.HandlingData.getForModel(model);

            let data = {};

            for (const key in handlingproperties) {
                const property = handlingproperties[key];
                
                if (property === 'centreOfMassOffset' || property === 'inertiaMultiplier') {
                    const value: alt.Vector3 = handling[property];

                    data[property] = value;

                } else if (property === 'initialDriveGears' || property === 'modelFlags' || property === 'monetaryValue' || property === 'modelFlags' || property === 'handlingFlags' || property === 'damageFlags') {
                    data[property] = parseInt(handling[property]);
                } else {
                    data[property] = parseFloat(handling[property]).toFixed(6);
                }
            }

            alt.emitServer("CallbackGetHandling", model.toString(), JSON.stringify(data));
        });

        alt.log("Chargement des controlleurs");

        chat.initialize();
        speedometer.initialize();
        utils.initialize();
        login.init();
        xtreamMenu.init();
        nightClub.initialize();
        await trains.initialize();
        new LSPDManager();
        new Notify();       
        new Admin();
        menuManager();
        CustomEvents.initialize();
        //alt.discordRequestOAuth2();
        //while (!alt.isDiscordInfoReady()) { }

        

        alt.log("Connection avec le social club: " + game.scGetNickname());

        alt.emitServer("Events_PlayerJoin", game.scGetNickname(), JSON.stringify(alt.Discord.currentUser));
    }
    catch (Exception) {
        alt.logError(`Failed to load scripts.\nMessage: ${Exception.Message}`);
        game.freezeEntityPosition(alt.getLocalPlayer().scriptID, true);
        game.doScreenFadeOut(0);
        alt.logError(`Erreur! Essayez de vous reconnecter. Si le problème se reproduit, veuillez contacter un helpers.`);
    }
};

new NetworkingEntityClient();
init();