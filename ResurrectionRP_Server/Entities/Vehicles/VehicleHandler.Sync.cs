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

    public enum DoorID
    {
        DoorFrontLeft,
        DoorFrontRight,
        DoorRearLeft,
        DoorRearRight,
        DoorHood,
        DoorTrunk
    }

    public enum DoorState
    {
        DoorClosed,
        DoorOpen,
        DoorBroken,
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

    public partial class VehicleHandler
    {

    }
}
