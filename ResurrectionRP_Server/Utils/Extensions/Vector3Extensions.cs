using System;
using System.Numerics;

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

        public static float DistanceTo(this Vector3 point, Vector3 position) => (position - point).Length();

        public static float DistanceTo2D(this Vector3 point, Vector3 position)
        {
            Vector3 lhs = new Vector3(point.X, point.Y, 0.0f);
            Vector3 rhs = new Vector3(position.X, position.Y, 0.0f);

            return Distance(lhs, rhs);
        }

        public static float Distance(Vector3 position1, Vector3 position2) => (position1 - position2).Length();

        public static Vector3 Subtract(this Vector3 left, Vector3 right) => new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        public static Entity.Position ConvertToEntityPosition(this Vector3 pos)
        {
            return new Entity.Position { X = pos.X, Y=pos.Y, Z=pos.Z };
        }

    }
}
