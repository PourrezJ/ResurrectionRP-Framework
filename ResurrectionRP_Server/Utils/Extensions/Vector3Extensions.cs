using System;
using System.Numerics;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net;
using System.Collections.Generic;
using AltV.Net.Enums;
using System.Linq;
using AltV.Net.Async;
using System.Threading.Tasks;
using System.Diagnostics;
using ResurrectionRP_Server.Entities.Vehicles;

namespace ResurrectionRP_Server
{
    public static class Vector3Extensions
    {
        public static bool IsInArea(this Vector3 pos, Vector3[] pORect)
        {
            float distance = (pORect[0].DistanceTo2D(pORect[2]) < pORect[1].DistanceTo2D(pORect[3]) ? pORect[0].DistanceTo2D(pORect[2]) : pORect[1].DistanceTo2D(pORect[2])) / 1.7f;

            int count = 0;
            foreach (Vector3 v in pORect)
                if (pos.DistanceTo2D(v) <= distance) count++;

            return count >= 2;
        }

        public static Vector3 Forward(this Vector3 point, float rot, float dist)
        {
            var angle = rot;
            double xOff = -(Math.Sin((angle * Math.PI) / 180) * dist);
            double yOff = Math.Cos((angle * Math.PI) / 180) * dist;

            return point + new Vector3((float)xOff, (float)yOff, 0);
        }

        public static Vector3 Backward(this Vector3 point, float rot, float dist)
        {
            var angle = rot;
            double xOff = (Math.Cos((angle * Math.PI) / 180) * dist);
            double yOff = -(Math.Sin((angle * Math.PI) / 180) * dist);

            return point + new Vector3((float)xOff, (float)yOff, 0);
        }

        public static float ClampAngle(float angle)
        {
            return (float)(angle + Math.Ceiling(-angle / 360) * 360);
        }

        public static Position ToPosition(this Vector3 vector3) => new Position(vector3.X, vector3.Y, vector3.Z); 

        public static float DistanceTo(this Vector3 point, Vector3 position) => (position - point).Length();

        public static float DistanceTo2D(this Vector3 point, Vector3 position)
        {
            Vector3 lhs = new Vector3(point.X, point.Y, 0.0f);
            Vector3 rhs = new Vector3(position.X, position.Y, 0.0f);

            return Distance(lhs, rhs);
        }

        public static float Distance(Vector3 position1, Vector3 position2) => (position1 - position2).Length();

        public static Vector3 Subtract(this Vector3 left, Vector3 right) => new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        public static Position ConvertToPosition(this Vector3 pos) =>  new Position { X = pos.X, Y=pos.Y, Z=pos.Z };
        public static Entity.Position ConvertToEntityPosition(this Vector3 pos) =>  new Entity.Position { X = pos.X, Y=pos.Y, Z=pos.Z };

        public static Vector3 ConvertRotationToRadian(this Vector3 rot) 
        {
            if (rot.Z < 180.0)
                return new Vector3(0.0f, 0.0f, (float)(rot.Z * Math.PI / 180.0));
            else
                return new Vector3(0.0f, 0.0f, (float)((rot.Z - 360.0) / 180.0 * Math.PI));
        }

        public static Rotation ConvertToEntityRotation(this Vector3 pos) => new Rotation(pos.X, pos.Y, pos.Z );

        public static Task<List<IVehicle>> GetVehiclesInRangeAsync(this Vector3 pos, float range, short dimension = GameMode.GlobalDimension)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<IVehicle> vehicles = Alt.GetAllVehicles().ToList();
            List<IVehicle> end = new List<IVehicle>();

            foreach (IVehicle veh in vehicles)
            {
                if (pos.DistanceTo2D(veh.Position) <= range && veh.Dimension == dimension)
                    end.Add(veh);
            }

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            return Task.FromResult(end);
        }

        public static List<IPlayer> GetPlayersInRange(this Vector3 pos, float range)
        {
            ICollection<IPlayer> vehicles = Alt.GetAllPlayers();
            List<IPlayer> end = new List<IPlayer>();
            foreach(IPlayer veh in vehicles)
            {
                if (pos.DistanceTo2D(new Vector3(veh.Position.X, veh.Position.Y, veh.Position.Y)) <= range)
                    end.Add(veh);
            }
            return end;
        }


        public static IVehicle GetTowTruckInZone(this Position pos, float distance)
        {
            foreach(IVehicle veh in VehiclesManager.GetAllVehicles())
            {
                if (veh.Position.Distance(pos) <= distance && veh.Model == (uint)VehicleModel.Flatbed)
                    return veh;
            }
            return null;
        }
        public static Vector3 ConvertToVector3(this Position pos) => new Vector3(pos.X, pos.Y, pos.Z);

        public static Entity.Position ConvertToEntityPosition(this Position pos) => new Entity.Position { X = pos.X, Y = pos.Y, Z = pos.Z };

        public static Vector3Serialized ConvertToVector3Serialized(this Vector3 pos) => new Vector3Serialized(pos);
    }
}
