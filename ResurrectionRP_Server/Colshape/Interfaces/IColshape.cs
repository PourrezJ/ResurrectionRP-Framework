using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using System.Collections.Generic;

namespace ResurrectionRP_Server.Colshape
{
    public interface IColshape
    {
        #region Properties
        uint Id { get; }

        short Dimension { get; }

        Position Position { get; }

        float Radius { get; }

        IEnumerable<IEntity> Entities { get; }

        IDictionary<IEntity, bool> LastChecked { get; }
        #endregion

        #region Events
        event ColshapePlayerEventHandler OnPlayerEnterColshape;
        event ColshapePlayerEventHandler OnPlayerLeaveColshape;
        event ColshapePlayerEventHandler OnPlayerInteractInColshape;
        event ColshapeVehicleEventHandler OnVehicleEnterColshape;
        event ColshapeVehicleEventHandler OnVehicleLeaveColshape;
        #endregion

        #region Methods
        void AddEntity(IEntity entity);

        void Delete();

        bool IsEntityIn(IEntity entity);

        bool IsEntityInside(IEntity entity);

        bool IsPositionInside(Position position);

        void RemoveEntity(IEntity entity);
        #endregion
    }
}
