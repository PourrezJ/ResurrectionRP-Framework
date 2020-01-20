using AltV.Net.Data;
using AltV.Net.NetworkingEntity;
using AltV.Net.NetworkingEntity.Elements.Entities;
using Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities
{
    public class Entity
    {
        #region Private Fields
        public INetworkingEntity NetworkEntity;
        #endregion

        #region Public Fields
        public ulong ID
        {
            get
            {
                if (NetworkEntity != null && NetworkEntity.Exists)
                    return NetworkEntity.Id;
                return 0;
            }
        }

        public AltV.Net.Data.Position Position
        {
            get
            {
                if (NetworkEntity != null && NetworkEntity.Exists)
                    return new AltV.Net.Data.Position(NetworkEntity.Position.X, NetworkEntity.Position.Y, NetworkEntity.Position.Z);
                return new AltV.Net.Data.Position();
            }
            set
            {
                if (NetworkEntity != null && NetworkEntity.Exists)
                    NetworkEntity.Position = value.ConvertToEntityPosition();
            }
        }

        public int Dimension
        {
            get
            {
                if (NetworkEntity != null && NetworkEntity.Exists)
                    return NetworkEntity.Dimension;
                return 0;
            }
            set
            {
                if (NetworkEntity != null && NetworkEntity.Exists)
                    NetworkEntity.Dimension = value;
            }
        }

        public bool Exists
        {
            get
            {
                if (NetworkEntity != null && NetworkEntity.Exists)
                    return NetworkEntity.Exists;
                return false;
            }
        }
        #endregion

        #region C4TOR
        public Entity(AltV.Net.Data.Position position, int dimension)
        {
            datas = new ConcurrentDictionary<string, object>();
        }
        #endregion

        #region Methods
        public virtual async Task Destroy()
        {
            if (NetworkEntity != null && NetworkEntity.Exists)
                await NetworkEntity.Remove();
        }

        public virtual Dictionary<string, object> Export()
        {
            return new Dictionary<string, object>();          
        }
        #endregion

        #region Datas
        private ConcurrentDictionary<string, object> datas;

        public bool SetData(string key, object data)
            => datas.TryAdd(key, data);

        public object GetData(string key)
            => datas[key] ?? null;
        #endregion
    }
}
