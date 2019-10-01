using AltV.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Utils.Extensions
{
    public static class PositionExtensions
    {
        public static float Distance2D(this Position point, Position position)
        {
            Position lhs = new Position(point.X, point.Y, 0.0f);
            Position rhs = new Position(position.X, position.Y, 0.0f);

            return lhs.Distance(rhs);
        }
    }
}
