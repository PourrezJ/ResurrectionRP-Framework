import * as alt from 'alt';
import * as game from 'natives';
import * as chat from 'client/chat/chat';
import * as speedometer from 'client/speedometer/speedometer';
import * as xtreamMenu from 'client/menus/xtreamMenu/xtreamMenuManager';
import * as utils from 'client/Utils/utils';
import * as login from 'client/login/Login';
import * as PlayerCustomization from 'client/player/PlayerCustomization';
import { Game } from 'client/player/Game';
import { OpenCharCreator } from 'client/Creator/Creator';
import { Streamer } from 'client/Streamer/Streamer';
import { Notify } from 'client/Notify/Notify';
import menuManager from 'client/MenuManager/MenuManager';

chat.initialize();
speedometer.initialize();
utils.initialize();
login.init();
xtreamMenu.init();
new Streamer();
menuManager();

var GameClass: Game;

export function Instance(): Game {
    return GameClass;
}

alt.onServer("PlayerInitialised", (
    StaffRank: number,
    IdentiteName: string,
    Money: number,
    Thirst: number,
    Hunger: number,
    AnimSettings: string,
    Time: string,
    Weather: string,
    WeatherWind: number,
    WeatherWindDirection: number,
    isDebug: boolean,
    Location: string
) => {
    PlayerCustomization.init();
    new Notify();
    GameClass = new Game(StaffRank, IdentiteName, Money, Thirst, Hunger, AnimSettings, Time, Weather, WeatherWind, WeatherWindDirection, isDebug, Location);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
});

alt.onServer('OpenCreator', () => {
    OpenCharCreator();
});

alt.onServer("togglePlayerControl", (value: boolean) => {
    alt.toggleGameControls(value);
});