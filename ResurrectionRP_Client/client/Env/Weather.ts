import * as alt from 'alt';
import * as game from 'natives';
import * as Game from 'client/player/Game';

export class Weather {

    public OldWeather: string;
    public WeatherType: string;
    public WindSpeed: number;
    public WindDirection: number;
    public WeatherTransition: number;

    constructor(weatherType: string, windSpeed: number, windDirection: number) {
        this.OldWeather = this.WeatherType;
        this.WeatherType = weatherType;
        this.WindSpeed = windSpeed;
        this.WindDirection = windDirection;
        this.WeatherTransition = 0;

        game.setWeatherTypeNow(this.WeatherType);
        game.setWeatherTypeNowPersist(this.WeatherType);
        game.setOverrideWeather(this.WeatherType);

        game.setWindSpeed(this.WindSpeed);
        game.setWindDirection(this.WindDirection);

        alt.everyTick(this.OnUpdate);
        alt.onServer("WeatherChange", this.WeatherChange);
    }

    WeatherChange = (weatherType: string, windSpeed: number, windDirection: number, transition: number) => {
        this.WeatherType = weatherType;
        this.WindSpeed = windSpeed;
        this.WindDirection = windDirection;
        this.WeatherTransition = transition;
        game.setWeatherTypeTransition(game.getHashKey(this.OldWeather), game.getHashKey(this.WeatherType), this.WeatherTransition);
        game.setWindSpeed(this.WindSpeed);
        game.setWindDirection(this.WindDirection);
    }
    private lastcheck: number;
    OnUpdate = () => {
        if (game.getGameTimer() - this.lastcheck < 60000)
            return;
        this.lastcheck = game.getGameTimer();
        game.setWeatherTypeTransition(game.getHashKey(this.OldWeather), game.getHashKey(this.WeatherType), this.WeatherTransition);
        if (this.WeatherTransition >= 1.0) {
            game.setWeatherTypeNow(this.WeatherType);
            game.setWeatherTypeNowPersist(this.WeatherType);
            game.setOverrideWeather(this.WeatherType);
        }
        game.setWindSpeed(this.WindSpeed);
        game.setWindDirection(this.WindDirection);
    }
}