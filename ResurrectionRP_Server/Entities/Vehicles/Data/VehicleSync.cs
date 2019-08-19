using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using System.Drawing;

namespace ResurrectionRP_Server.Entities.Vehicles.Data
{
    //Enums for ease of use
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

    public enum WheelID
    {
        Wheel0,
        Wheel1,
        Wheel2,
        Wheel3,
        Wheel4,
        Wheel5,
        Wheel6,
        Wheel7,
        Wheel8,
        Wheel9
    }

    public enum WheelState
    {
        WheelFixed,
        WheelBurst,
        WheelOnRim,
    }
}
