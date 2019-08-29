using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using OpenWeatherAPI;
using AltV.Net.Async;
using WeatherType = ResurrectionRP_Server.Weather.Data.WeatherType;

namespace ResurrectionRP_Server.Weather
{
    public class WeatherManager
    {
        public WeatherType Actual_weather { get; set; } = WeatherType.Extrasunny;
        public double Wind { get; set; } = 0;
        public double WindDirection { get; set; } = 0;
        public float WeatherTransition { get; set; }
        public bool Forced { get; set; }

        public async Task InitWeather()
        {
            try
            {
                if (Config.GetSetting<bool>("Winter"))
                {
                    Actual_weather = WeatherType.Xmas;
                    //await MP.World.SetWeatherAsync(WeatherType.Xmas);
                    await this.UpdatePlayersWeather();
                }
                var apikey = Config.GetSetting<string>("OpenWeatherAPIKey");
                if (string.IsNullOrEmpty(apikey)) throw new ArgumentException("Vous devez renseigner l'api key de OpenWeather");
                var client = new OpenWeatherAPI.OpenWeatherAPI(apikey);

                var results = client.Query(Config.GetSetting<string>("OpenWeatherCity"));
                await WeatherDataReceived(results.Weathers[0], results.Wind);

                Utils.Utils.Delay((int)TimeSpan.FromMinutes(5).TotalMilliseconds, false, async () =>
                {
                    results = client.Query(Config.GetSetting<string>("OpenWeatherCity"));
                    await WeatherDataReceived(results?.Weathers[0], results?.Wind);
                });
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("InitWeather" + ex.ToString());
            }
        }

        public async Task UpdatePlayersWeather()
        {
            var players = Alt.GetAllPlayers();
            foreach(IPlayer player in players)
            {
                if (!player.Exists)
                    return;
                await player.EmitAsync("WeatherChange", this.Actual_weather.ToString(), this.Wind, this.WindDirection, this.WeatherTransition);
            }
        }

        private Timer timer = null;
        public async Task ChangeWeather(WeatherType weather, double wind, double winddirection)
        {
            if (Forced)
                return;

            this.Wind = wind;
            this.WindDirection = winddirection;

            if (timer != null)
            {
                timer.Close();
                timer = null;
            }

            if (Actual_weather != weather)
            {
                this.Actual_weather = weather;
                WeatherTransition = 0.0f;
#pragma warning disable CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
                timer = Utils.Utils.Delay(2500, false, async () =>
                {
                    WeatherTransition += 0.02f;

                    if (WeatherTransition >= 1.0)
                    {
                        WeatherTransition = 1;
                        timer.Close();
                    }
                    //await .CallAsync("WeatherManager_Change", weather.ToString(), wind, winddirection, WeatherTransition);
                    await this.UpdatePlayersWeather();

                    await Task.Delay(2500);
                });
#pragma warning restore CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
                Alt.Server.LogInfo("[WEATHER] Weather Update to " + weather.ToString());
            }
            else if (WeatherTransition > 1.0)
            {
                await this.UpdatePlayersWeather();
            }
        }

        private async Task WeatherDataReceived(OpenWeatherAPI.Weather weather, Wind wind)
        {
            if (wind == null || weather == null) return;
            switch (weather.ID)
            {
                case 200:
                    await ChangeWeather(WeatherType.Overcast, wind.SpeedMetersPerSecond, wind.Degree);
                    break;
                case 201:
                    await ChangeWeather(WeatherType.Rain, wind.SpeedMetersPerSecond, wind.Degree);
                    break;
                case 202:
                case 210:
                case 211:
                case 212:
                case 221:
                case 230:
                case 231:
                case 232:
                    await ChangeWeather(WeatherType.Thunder, wind.SpeedMetersPerSecond, wind.Degree);
                    break;


                case 800:
                    await ChangeWeather(WeatherType.Clear, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 801:
                case 802:
                case 803:
                case 804:
                    await ChangeWeather(WeatherType.Clouds, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 500:
                    await ChangeWeather(WeatherType.Clearing, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 501:
                    await ChangeWeather(WeatherType.Overcast, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 502:
                case 503:
                case 504:
                case 511:
                case 520:
                case 521:
                case 522:
                case 531:
                    await ChangeWeather(WeatherType.Rain, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 300:
                case 301:
                    await ChangeWeather(WeatherType.Smog, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 302:
                case 310:
                case 311:
                case 312:
                case 313:
                case 314:
                case 321:
                    await ChangeWeather(WeatherType.Foggy, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 701:
                case 711:
                case 721:
                case 732:
                case 741:
                case 751:
                case 761:
                case 762:
                case 771:
                case 781:
                    await ChangeWeather(WeatherType.Foggy, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 600:
                    await ChangeWeather(WeatherType.Snowlight, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 601:
                case 602:
                case 611:
                case 612:
                case 615:
                case 616:
                case 620:
                case 621:
                case 622:
                    await ChangeWeather(WeatherType.Snow, wind.SpeedMetersPerSecond, wind.Degree);
                    break;
                default:
                    await ChangeWeather(WeatherType.Clear, 0, 0);
                    break;
            }
        }
    }
}
