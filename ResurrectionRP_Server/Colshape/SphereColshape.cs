using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace ResurrectionRP_Server.Colshape
{
    public class SphereColshape : Colshape
    {
        #region Constructor
        public SphereColshape(uint id, Position position, float radius, short dimension) : base(id, position, radius, dimension)
        {
        }
        #endregion

        #region Public methods
        public override bool IsEntityInside(IEntity entity)
        {
            if (entity.Dimension != Dimension)
                return false;

            return IsPositionInside(entity.Position);
        }

        public override bool IsPositionInside(Position position)
        {
            return Position.Distance(position) <= Radius;
        }
        #endregion
    }
}
