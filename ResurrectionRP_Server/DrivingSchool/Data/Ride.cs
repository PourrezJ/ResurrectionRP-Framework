using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using ResurrectionRP_Server.Colshape;

namespace ResurrectionRP_Server.DrivingSchool
{
    public struct Ride
    {
        public Vector3 Position;
        public int Speed;
        public IColshape Colshape;
    }
}
