﻿using AltV.Net;
using AltV.Net.Enums;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Utils
{
    class Util
    {
        // Aucune idée de sont impacte ... djoe
        //public static async Task SetInterval(Action action, TimeSpan timeout)
        //{
        //    await Task.Delay(timeout).ConfigureAwait(false);
        //    action();
        //    await SetInterval(action, timeout);
        //}

        public static bool CheckThread(string data = "")
        {
            if (Startup.MainThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                Alt.Server.LogError(data + " not in the main thread!");
                return false;
            }
            return true;
        }

        public static string GetIPAddress()
        {
            return new WebClient().DownloadString("https://ipinfo.io/ip").Replace("\n", "");

        }

        public static int RandomNumber(int max) => new Random().Next(max);
        public static int RandomNumber(int min, int max) => new Random().Next(min, max);

        public static void Delay(int ms, Action action)
        {
            Task.Delay(ms).ContinueWith((t) => action());
        }

        public static System.Timers.Timer SetInterval(Action action, int ms)
        {
            var t = new System.Timers.Timer(ms);
            t.Elapsed += (s, e) => action();
            t.Start();
            return t;
        }

        public static void StopTimer(System.Timers.Timer timer) => timer.Stop();

        public static void SendNotificationPicture(Enums.CharPicture img, string sender, string subject, string message) =>
            Alt.EmitAllClients("SetNotificationMessage", img.ToString(), sender, subject, message);

        public static WindowTint GetWindowTint(byte modValue)
        {
            if (modValue == 0)
                return WindowTint.None;
            else if (modValue == 1)
                return WindowTint.Green;
            else if (modValue == 2)
                return WindowTint.PureBlack;
            else if (modValue == 3)
                return WindowTint.DarkSmoke;
            else if (modValue == 4)
                return WindowTint.LightSmoke;

            return WindowTint.Stock;
        }
    }
}
