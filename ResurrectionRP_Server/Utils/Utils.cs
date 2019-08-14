using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Utils
{
    class Utils
    {
        public static async Task SetInterval(Action action, TimeSpan timeout)
        {
            await Task.Delay(timeout).ConfigureAwait(false);

            action();

            await SetInterval(action, timeout);
        }

        public static int RandomNumber(int max) => new Random().Next(max);
        public static int RandomNumber(int min, int max) => new Random().Next(min, max);

        public static System.Timers.Timer Delay(int ms, bool onlyOnce, Action action)
        {
            if (onlyOnce)
            {
                Task.Delay(ms).ContinueWith((t) => action());
                return null;
            }
            else
            {
                var t = new System.Timers.Timer(ms);
                t.Elapsed += (s, e) => action();
                t.Start();
                return t;
            }
        }

        public static void StopTimer(System.Timers.Timer timer) => timer.Stop();

        /*
        public static async Task SendNotificationPictureToALL(string message, CharPicture picture, bool flash, int icontype, string title, string description)
        {
            await MP.Players.CallAsync("SetNotificationMessage", message, picture.ToString(), flash, icontype, title, description);
        }*/
    }
}
