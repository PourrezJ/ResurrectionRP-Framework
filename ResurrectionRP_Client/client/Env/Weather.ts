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

        game.setWind(this.WindSpeed);
        game.setWindDirection(this.WindDirection);

        
    }

    WeatherChange = (weatherType: string, windSpeed: number, windDirection: number, transition: number) => {
        this.WeatherType = weatherType;
        this.WindSpeed = windSpeed;
        this.WindDirection = windDirection;
        this.WeatherTransition = transition;
        alt.log("Mise à jour de la météo, nouvelle météo : " + weatherType);
        game.setWeatherTypeTransition(game.getHashKey(this.OldWeather), game.getHashKey(this.WeatherType), this.WeatherTransition);

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
        game.setWind(this.WindSpeed);
        game.setWindDirection(this.WindDirection);
    }
}