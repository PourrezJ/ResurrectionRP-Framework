using System.Collections.Generic;
using ResurrectionRP_Server.Streamer.Data;
using Newtonsoft.Json;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using System.Collections.Concurrent;

namespace ResurrectionRP_Server.Entities.Objects
{
    public class Object
    {
        public int id;
        public int model;
        public Position position;
        public Rotation rotation;
        public uint dimension;
        public bool freeze;
        public Models.Attachment attach = null;
        public string pickup = null;
        private ConcurrentDictionary<string, object> datas;

        public Object(int model, Position position, Rotation rotation, int entityId, bool freeze = false, uint dimension = ushort.MaxValue)
        {
            this.model = model;
            this.id = entityId;
            this.freeze = freeze;
            this.position = position;
            this.rotation = rotation;
            this.dimension = dimension;
            datas = new ConcurrentDictionary<string, object>();
        }

        public bool SetAttachToEntity(IVehicle target, string bone, Position positionOffset, Rotation rotationOffset)
        {
            ObjectManager.AttachToEntity(target, this, bone, positionOffset, rotationOffset);
            return true;
        }

        public bool DetachAttach()
        {
            this.attach = null;
            return true;
        }

        public void Destroy()
        {
            GameMode.Instance.Streamer.DeleteEntityObject(this);
        }

        public bool SetData(string key, object data)
            => datas.TryAdd(key, data);

        public object GetData(string key)
            => datas[key] ?? null;

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = this.model;
            data["entityType"] = (int)EntityType.Object;
            data["id"] = this.id;
            data["position"] = JsonConvert.SerializeObject(this.id);
            data["freeze"] = this.freeze;
            data["attach"] = JsonConvert.SerializeObject(this.attach);
            data["dimension"] = this.dimension;
            return data;
        }

    }
}
