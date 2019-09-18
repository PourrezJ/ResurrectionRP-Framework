using AltV.Net;
using AltV.Net.Data;
using System.Numerics;

namespace ResurrectionRP_Server
{
    public class Vector3Serialized : IWritable
    {
        private readonly double X;
        private readonly double Y;
        private readonly double Z;


        public Vector3Serialized(Vector3 vector3)
        {
            this.X = vector3.X;
            this.Y = vector3.Y;
            this.Z = vector3.Z;
        }

        public void OnWrite(IMValueWriter writer)
        {
            writer.BeginObject();
            writer.Name("x");
            writer.Value(X);
            writer.Name("y");
            writer.Value(Y);
            writer.Name("z");
            writer.Value(Z);
            writer.EndObject();
        }
    }
}