using System;
using System.Timers;
using AltV.Net;
using OpenWeatherAPI;
using WeatherType = ResurrectionRP_Server.Weather.Data.WeatherType;

namespace ResurrectionRP_Server.Weather
{
    public static class WeatherManager
    {
        public static WeatherType Actual_weather { get; set; } = WeatherType.Extrasunny;
        public static double Wind { get; set; } = 0;
        public static double WindDirection { get; set; } = 0;
        public static float WeatherTransition { get; set; }
        public static bool Forced { get; set; }

        public static void InitWeather()
        {
            try
            {
                if (Config.GetSetting<bool>("Winter"))
                {
                    Actual_weather = WeatherType.Xmas;
                    Forced = true;
                    UpdatePlayersWeather();
                }
                var apikey = Config.GetSetting<string>("OpenWeatherAPIKey");
                if (string.IsNullOrEmpty(apikey)) throw new ArgumentException("Vous devez renseigner l'api key de OpenWeather");
                var client = new OpenWeatherAPI.OpenWeatherAPI(apikey);

                var results = client.Query(Config.GetSetting<string>("OpenWeatherCity"));
                WeatherDataReceived(results.Weathers[0], results.Wind);

                Utils.Util.Delay((int)TimeSpan.FromMinutes(5).TotalMilliseconds, () =>
                {
                    results = client.Query(Config.GetSetting<string>("OpenWeatherCity"));
                    WeatherDataReceived(results?.Weathers[0], results?.Wind);
                });
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("InitWeather" + ex.ToString());
            }
        }

        public static void UpdatePlayersWeather()
        {
            Alt.EmitAllClients("WeatherChange", Actual_weather.ToString(), Wind, WindDirection, WeatherTransition);
        }

        private static Timer timer = null;
        public static void ChangeWeather(WeatherType weather, double wind, double winddirection)
        {
            if (Forced)
                return;

            Wind = wind;
            WindDirection = winddirection;

            if (timer != null)
            {
                timer.Close();
                timer = null;
            }

            if (Actual_weather != weather)
            {
                Actual_weather = weather;
                WeatherTransition = 0.0f;
                timer = Utils.Util.SetInterval(() =>
                {
                    WeatherTransition += 0.02f;

                    if (WeatherTransition >= 1.0)
                    {
                        WeatherTransition = 1;
                        timer.Close();
                    }
                    UpdatePlayersWeather();
                }, 2500);
                Alt.Server.LogInfo("[WEATHER] Weather Update to " + weather.ToString());
            }
            else if (WeatherTransition > 1.0)
            {
                UpdatePlayersWeather();
            }
        }

        private static void WeatherDataReceived(OpenWeatherAPI.Weather weather, Wind wind)
        {
            if (wind == null || weather == null) return;
            switch (weather.ID)
            {
                case 200:
                    ChangeWeather(WeatherType.Overcast, wind.SpeedMetersPerSecond, wind.Degree);
                    break;
                case 201:
                    ChangeWeather(WeatherType.Rain, wind.SpeedMetersPerSecond, wind.Degree);
                    break;
                case 202:
                case 210:
                case 211:
                case 212:
                case 221:
                case 230:
                case 231:
                case 232:
                    ChangeWeather(WeatherType.Thunder, wind.SpeedMetersPerSecond, wind.Degree);
                    break;


                case 800:
                    ChangeWeather(WeatherType.Clear, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 801:
                case 802:
                case 803:
                case 804:
                    ChangeWeather(WeatherType.Clouds, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 500:
                    ChangeWeather(WeatherType.Clearing, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 501:
                    ChangeWeather(WeatherType.Overcast, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 502:
                case 503:
                case 504:
                case 511:
                case 520:
                case 521:
                case 522:
                case 531:
                    ChangeWeather(WeatherType.Rain, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 300:
                case 301:
                    ChangeWeather(WeatherType.Smog, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 302:
                case 310:
                case 311:
                case 312:
                case 313:
                case 314:
                case 321:
                    ChangeWeather(WeatherType.Foggy, wind.SpeedMetersPerSecond, wind.Degree);
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
                    ChangeWeather(WeatherType.Foggy, wind.SpeedMetersPerSecond, wind.Degree);
                    break;

                case 600:
                    ChangeWeather(WeatherType.Snowlight, wind.SpeedMetersPerSecond, wind.Degree);
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
                    ChangeWeather(WeatherType.Snow, wind.SpeedMetersPerSecond, wind.Degree);
                    break;
                default:
                    ChangeWeather(WeatherType.Clear, 0, 0);
                    break;
            }
        }
    }
}
