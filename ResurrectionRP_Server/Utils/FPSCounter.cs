using AltV.Net;
using ResurrectionRP_Server.Entities.Vehicles;
using System;

namespace ResurrectionRP_Server.Utils
{
    public static class FPSCounter
    {
        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;
        private static string title;

        public static void OnTick()
        {
            if (string.IsNullOrEmpty(title))
                title = Console.Title;

            var players = Alt.GetAllPlayers();
            var vehicles = Alt.GetAllVehicles();

            int joueurCount = 0;
            int vehicleCount = 0;

            lock (players)
            {
                joueurCount = players.Count;
            }

            lock (vehicles)
            {
                vehicleCount = vehicles.Count;
            }

            Console.Title = title + $" FPS: {CalculateFrameRate()} Joueurs: {joueurCount} Véhicules: {vehicleCount}";
        }

        public static int CalculateFrameRate()
        {
            if (Environment.TickCount - lastTick >= 1000)
            {
                lastFrameRate = frameRate;
                frameRate = 0;
                lastTick = Environment.TickCount;
            }

            frameRate++;
            return lastFrameRate;
        }
    }
}