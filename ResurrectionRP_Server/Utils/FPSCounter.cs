﻿using System;

namespace ResurrectionRP_Server.Utils
{
    public class FPSCounter
    {
        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;
        private static string title;

        public static void OnTick()
        {
            if (string.IsNullOrEmpty(title)) title = Console.Title;
            Console.Title = title + $" FPS: {CalculateFrameRate()} Joueurs: {AltV.Net.Alt.GetAllPlayers().Count} Véhicules: {AltV.Net.Alt.GetAllVehicles().Count}";
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