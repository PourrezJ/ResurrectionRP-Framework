using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public enum WindowID
    {
        WindowFrontRight,
        WindowFrontLeft,
        WindowRearRight,
        WindowRearLeft
    }

    public enum WindowState
    {
        WindowFixed,
        WindowDown,
        WindowBroken
    }

    public enum WheelState
    {
        WheelFixed,
        WheelBurst,
        WheelOnRim,
    }

    public class Wheel
    {
        public float Health;
        public bool Burst;
        public bool HasTire;

        public Wheel(float health = 100, bool burst = false, bool hasTire = true)
        {
            Health = health;
            Burst = burst;
            HasTire = hasTire;
        }
    }
}

