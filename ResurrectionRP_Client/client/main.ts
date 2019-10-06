﻿import * as alt from 'alt';
import * as game from 'natives';
import * as chat from './chat/chat';
import * as speedometer from './vehicle/vehicle';
import * as xtreamMenu from './menus/xtreamMenu/xtreamMenuManager';
import * as utils from './Utils/utils';
import * as login from './login/Login';
import * as nightClub from './Env/NightClub';
import * as PlayerCustomization from './player/PlayerCustomization';
import { Game } from './player/Game';
import { OpenCharCreator } from './Creator/Creator';
import { NetworkingEntityClient } from './Streamer/NetworkingEntityClient';
import { Notify } from './Notify/Notify';
import menuManager from './MenuManager/MenuManager';
import { Admin } from './Utils/Admin';
import { LSPDManager } from './LSPDCall';
import * as CustomEvents from './Utils/CustomEvents';


var GameClass: Game;

const init = async () => {
    try {
        alt.log('Chargement des events.');

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

        alt.log("Chargement des controlleurs");

        chat.initialize();
        speedometer.initialize();
        utils.initialize();
        login.init();
        xtreamMenu.init();
        nightClub.initialize();
        new LSPDManager();
        new Notify();
        new NetworkingEntityClient();
        new Admin();
        menuManager();
        CustomEvents.initialize();
        //alt.discordRequestOAuth2();
        //while (!alt.isDiscordInfoReady()) { }

        alt.log("Connection avec le social club: " + game.scGetNickname());

        alt.emitServer("Events_PlayerJoin", game.scGetNickname(), JSON.stringify(alt.discordInfo()));
    }
    catch (Exception) {
        alt.logError(`Failed to load scripts.\nMessage: ${Exception.Message}`);
        game.freezeEntityPosition(alt.getLocalPlayer().scriptID, true);
        game.doScreenFadeOut(0);
        alt.logError(`Erreur! Essayez de vous reconnecter. Si le problème se reproduit, veuillez contacter un helpers.`);
    }
};
init();


