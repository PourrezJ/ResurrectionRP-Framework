using System.Collections.Generic;
using ResurrectionRP_Server.Streamer.Data;
using Newtonsoft.Json;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using System.Collections.Concurrent;
using System.Numerics;
using AltV.Net;

namespace ResurrectionRP_Server.Entities.Objects
{
    public class WorldObject
    {
        public static ConcurrentDictionary<int, WorldObject> ListObject = new ConcurrentDictionary<int, WorldObject>();

        public int ID { get; private set; }
        public int Model;
        public bool Exists;

        private Position _position;
        public Position Position
        {
            get => _position;
            set
            {
                _position = value;
                if (Exists)
                    GameMode.Instance.Streamer.UpdateEntityObject(this);
               
            }
        }

        private Rotation _rotation;
        public Rotation Rotation
        {         
            set
            {
                _rotation = value;
                if (Exists)
                    GameMode.Instance.Streamer.UpdateEntityObject(this);
            }
            get => _rotation;
        }
        public uint Dimension;
        public bool Freeze;
        public Models.Attachment Attachment = null;
        public string Pickup = null;
        private ConcurrentDictionary<string, object> Datas;

        public WorldObject(int model, Position position, Rotation rotation, int entityId, bool freeze = false, uint dimension = ushort.MaxValue)
        {
            Exists = false;
            Model = model;
            ID = entityId;
            Freeze = freeze;
            Position = position;
            Rotation = rotation;
            Dimension = dimension;
            Datas = new ConcurrentDictionary<string, object>();
        }

        public bool SetAttachToEntity(IVehicle target, string bone, Position positionOffset, Rotation rotationOffset)
        {
            AttachToEntity(target, this, bone, positionOffset, rotationOffset);
            return true;
        }

        public bool DetachAttach()
        {
            Attachment = null;
            GameMode.Instance.Streamer.UpdateEntityObject(this);
            return true;
        }

        public void Destroy()
        {
            DestroyObject(ID);
        }

        public bool SetData(string key, object data)
            => Datas.TryAdd(key, data);

        public object GetData(string key)
            => Datas[key] ?? null;

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = Model;
            data["entityType"] = (int)EntityType.Object;
            data["id"] = ID;
            data["position"] = JsonConvert.SerializeObject(ID);
            data["freeze"] = Freeze;
            data["attach"] = JsonConvert.SerializeObject(Attachment);
            data["dimension"] = Dimension;
            return data;
        }

        public static WorldObject CreateObject(int model, Vector3 position, Vector3 rotation, bool freeze = false, bool dynamic = false, uint dimension = ushort.MaxValue)
        {
            var resuobject = new WorldObject
            (
                model,
                position.ConvertToPosition(),
                rotation.ConvertToEntityRotation(),
                GameMode.Instance.Streamer.EntityNumber++,
                freeze,
                dimension
            );
            ListObject[resuobject.ID] = resuobject;
            GameMode.Instance.Streamer.AddEntityObject(resuobject);
            resuobject.Exists = true;
            return resuobject;
        }
        public static WorldObject CreateObject(string model, Vector3 position, Vector3 rotation, bool freeze = false, bool dynamic = false, uint dimension = ushort.MaxValue)
        {
            var resuobject = new WorldObject
            (
                (int)Alt.Hash(model),
                position.ConvertToPosition(),
                rotation.ConvertToEntityRotation(),
                GameMode.Instance.Streamer.EntityNumber++,
                freeze,
                dimension
            );
            ListObject[resuobject.ID] = resuobject;
            GameMode.Instance.Streamer.AddEntityObject(resuobject);
            resuobject.Exists = true;
            return resuobject;
        }

        public static void AttachToEntity(WorldObject ent1, WorldObject target, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            var attach = new Models.Attachment()
            {
                Bone = bone,
                PositionOffset = positionOffset,
                RotationOffset = rotationOffset,
                RemoteID = (uint)target.ID,
                Type = EntityType.Object
            };
            target.Attachment = attach;
            GameMode.Instance.Streamer.UpdateEntityObject(target);
        }
        public static void AttachToEntity(IVehicle vehicle, WorldObject target, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            var attach = new Models.Attachment()
            {
                Bone = bone,
                PositionOffset = positionOffset,
                RotationOffset = rotationOffset,
                RemoteID = (uint)vehicle.Id,
                Type = EntityType.Vehicle
            };
            target.Attachment = attach;
            GameMode.Instance.Streamer.UpdateEntityObject(target);

        }
        public static void DetachFromEntity(WorldObject entity)
        {
            entity.Attachment = null;
            GameMode.Instance.Streamer.UpdateEntityObject(entity);
        }

        public static void DestroyObject(int oid)
        {
            GameMode.Instance.Streamer.DeleteEntityObject(ListObject[oid]);
            ListObject.TryRemove(oid, out _);
        }
    }
}
