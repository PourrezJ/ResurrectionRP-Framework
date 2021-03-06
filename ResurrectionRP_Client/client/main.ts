﻿import * as alt from 'alt-client';
import * as game from 'natives';
import * as chat from './Chat/Chat';
import * as speedometer from './Vehicle/Vehicle';
import * as xtreamMenu from './Menus/xtreamMenu/xtreamMenuManager';
import * as login from './Login/Login';
import * as nightClub from './Env/NightClub';
import * as trains from './Env/Trains';
import * as PlayerCustomization from './Player/PlayerCustomization';
import { Game } from './Player/Game';
import { OpenCharCreator } from './Creator/Creator';
import { NetworkingEntityClient } from './Streamer/NetworkingEntityClient';
import { Notify } from './Notify/Notify';
import menuManager from './MenuManager/MenuManager';
import { Admin } from './Utils/Admin';
import { LSPDManager } from './LSPDCall';
import { Doors } from './Env/Doors';
import * as CustomEvents from './Utils/CustomEvents';
import * as apiext from './Utils/ApiExtends';
import { Loading } from './Env/Loading';
import { Subtitle } from './Env/Subtitle';
import { HelpText } from './Env/HelpText';

var GameClass: Game;

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
            IsDebug: boolean,
            Position: Vector3
        ) => {
            PlayerCustomization.init();
            GameClass = new Game(StaffRank, IdentiteName, Money, Thirst, Hunger, Time, Weather, WeatherWind, WeatherWindDirection, IsDebug, Position);
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

            if (Loading.loading != null)
                Loading.loading.Draw();
            if (Subtitle.subtitle != null)
                Subtitle.subtitle.Draw();
            if (HelpText.helpText != null)
                HelpText.helpText.Draw();
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

            // Unfreeze Player
            game.freezeEntityPosition(alt.Player.local.scriptID, false);

            // Destroy All Cameras
            game.renderScriptCams(false, false, 0, false, false, 0);
            game.destroyAllCams(true);

            // Turn off Screen Fades
            game.doScreenFadeIn(1);
            game.triggerScreenblurFadeOut(1);
        });

        alt.log("Chargement des controlleurs");

        chat.initialize();
        speedometer.initialize();
        login.init();
        xtreamMenu.init();
        nightClub.initialize();
        apiext.initialize();
        await trains.initialize();
        new LSPDManager();
        new Notify();       
        new Admin();
        menuManager();
        CustomEvents.initialize();

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