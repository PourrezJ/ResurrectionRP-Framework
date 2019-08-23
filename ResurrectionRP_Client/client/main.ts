import * as alt from 'alt';
import * as game from 'natives';
import * as chat from 'client/chat/chat';
import * as speedometer from 'client/speedometer/speedometer';
import * as xtreamMenu from 'client/xtreamMenu/xtreamMenuManager';
import * as utils from 'client/utils';
import * as login from 'client/login/Login';
import * as PlayerCustomization from 'client/player/PlayerCustomization';
import { Game } from 'client/player/Game';
import { OpenCharCreator } from 'client/Creator/Creator';
import { Streamer } from 'client/Streamer/Streamer';

chat.initialize()
speedometer.initialize();
utils.initialize();
login.init();
xtreamMenu.init();
 new Streamer();


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
    var GameClass: Game = new Game(StaffRank, IdentiteName, Money, Thirst, Hunger, AnimSettings, Time, Weather, WeatherWind, WeatherWindDirection, isDebug, Location);

});

alt.onServer('OpenCreator', () => {
    OpenCharCreator();
});

alt.onServer("togglePlayerControl", (value: boolean) => {
    alt.toggleGameControls(value);
});