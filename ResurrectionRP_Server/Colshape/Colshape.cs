using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System.Collections.Generic;

namespace ResurrectionRP_Server.Colshape
{
    public abstract class Colshape : IColshape
    {
        #region Fields
        public uint Id { get; }

        public short Dimension { get; }

        public Position Position { get; }

        public float Radius { get; }

        private readonly HashSet<IEntity> _entities;

        public ICollection<IEntity> Entities => _entities;
        #endregion

        #region Events
        public event ColshapePlayerEventHandler OnPlayerEnterColshape;
        public event ColshapePlayerEventHandler OnPlayerLeaveColshape;
        public event ColshapePlayerEventHandler OnPlayerInteractInColshape;
        public event ColshapeVehicleEventHandler OnVehicleEnterColshape;
        public event ColshapeVehicleEventHandler OnVehicleLeaveColshape;
        #endregion

        #region Constructor
        public Colshape(uint id, Position position, float radius, short dimension)
        {
            Id = id;
            Position = position;
            Radius = radius;
            Dimension = dimension;
            _entities = new HashSet<IEntity>();
        }
        #endregion

        #region Public methods
        public void AddEntity(IEntity entity)
        {
            bool entityAdded;

            lock (_entities)
            {
                entityAdded = _entities.Add(entity);
            }

            if (entityAdded)
            {
                if (entity.Type == BaseObjectType.Player)
                    OnPlayerEnterColshape?.Invoke(this, (IPlayer)entity);
                else if (entity.Type == BaseObjectType.Vehicle)
                    OnVehicleEnterColshape?.Invoke(this, (IVehicle)entity);
            }
        }

        public void Delete()
        {
            ColshapeManager.DeleteColshape(this);
        }

        public bool IsEntityIn(IEntity entity)
        {
            lock (_entities)
            {
                return _entities.Contains(entity);
            }
        }

        public abstract bool IsEntityInside(IEntity entity);

        public abstract bool IsPositionInside(Position position);

        public void RemoveEntity(IEntity entity)
        {
            bool entityRemoved;

            lock (_entities)
            {
                entityRemoved = _entities.Remove(entity);
            }

            if (entityRemoved)
            {
                if (entity.Type == BaseObjectType.Player)
                    OnPlayerLeaveColshape?.Invoke(this, (IPlayer)entity);
                else if (entity.Type == BaseObjectType.Vehicle)
                    OnVehicleLeaveColshape?.Invoke(this, (IVehicle)entity);
            }
        }
        #endregion
    }
}
