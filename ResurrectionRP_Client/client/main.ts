import * as alt from 'alt';
import * as game from 'natives';
import * as chat from 'client/chat/chat';
import * as speedometer from 'client/speedometer/speedometer';
import * as utils from 'client/utils';
import * as login from 'client/login/Login';
import * as PlayerCustomization from 'client/player/PlayerCustomization';
import { Game } from 'client/player/Game';
import { OpenCharCreator } from 'client/Creator/Creator';

chat.initialize()
speedometer.initialize();
utils.initialize();
login.init();

alt.onServer("PlayerInitialised", (
    StaffRank: number,
    IdentiteName: string,
    Money: number,
    Thirst: number,
    Hunger: number,
    AnimSettings: string,
    Time: string,
    Weather: number,
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