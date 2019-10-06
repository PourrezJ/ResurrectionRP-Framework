using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils.Extensions;
using System;

namespace ResurrectionRP_Server.Colshape
{
    public class CylinderColshape : Colshape
    {
        #region Fields
        public float Height { get; }
        #endregion

        #region Constructor
        public CylinderColshape(uint id, Position position, float radius, float height, short dimension) : base(id, position, radius, dimension)
        {
            Height = height;
        }
        #endregion

        #region Public methods
        public override bool IsEntityInside(IEntity entity)
        {
            try
            {
                if (!entity.Exists)
                    return false;
                else if (entity.Dimension != Dimension)
                    return false;
                else if (entity.Position.Z < Position.Z || entity.Position.Z > Position.Z + Height)
                    return false;

                return IsPositionInside(entity.Position);
            }
            catch(AccessViolationException)
            {
                Alt.Server.LogError($"CylinderColshape.IsEntityInside() - AccessViolationException with entity {entity.Id}");
                return false;
            }
        }

        public override bool IsPositionInside(Position position)
        {
            return Position.Distance2D(position) <= Radius;
        }
        #endregion
    }
}
