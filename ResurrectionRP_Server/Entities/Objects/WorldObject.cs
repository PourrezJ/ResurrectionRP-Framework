using System.Collections.Generic;
using ResurrectionRP_Server.Streamer.Data;
using Newtonsoft.Json;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using System.Collections.Concurrent;
using System.Numerics;
using AltV.Net;
using AltV.Net.NetworkingEntity.Elements.Entities;

namespace ResurrectionRP_Server.Entities.Objects
{
    public class WorldObject
    {
        public static ConcurrentDictionary<ulong, WorldObject> ListObject = new ConcurrentDictionary<ulong, WorldObject>();

        private ulong id;
        public ulong ID { 
            get
            {
                if (INetworkEntity != null)
                    return INetworkEntity.Id;
                return id;
            }
            private set
            {
                id = value;
            }
        }
        public int Model { get; private set; }
        public bool Exists { get; private set; }

        private Position _position;
        public Position Position
        {
            get => _position;
            set
            {
                _position = value;
                if (Exists)
                    Streamer.Streamer.UpdateEntityObject(this);
               
            }
        }

        private Rotation _rotation;
        public Rotation Rotation
        {         
            set
            {
                _rotation = value;
                if (Exists)
                    Streamer.Streamer.UpdateEntityObject(this);
            }
            get => _rotation;
        }

        public short Dimension { get; private set; }
        public bool Freeze { get; private set; }
        public Models.Attachment Attachment { get; private set; }
        public string Pickup { get; private set; }
        private ConcurrentDictionary<string, object> Datas;
        public INetworkingEntity INetworkEntity;

        public WorldObject(int model, Position position, Rotation rotation, bool freeze = false, short dimension = GameMode.GlobalDimension)
        {
            Exists = false;
            Model = model;
            Freeze = freeze;
            Position = position;
            Rotation = rotation;
            Dimension = dimension;
            Datas = new ConcurrentDictionary<string, object>();
            INetworkEntity = Streamer.Streamer.AddEntityObject(this);
        }
        public WorldObject(int model, Position position, Rotation rotation, Models.Attachment attachment = null, bool freeze = false, short dimension = GameMode.GlobalDimension)
        {
            Exists = false;
            Model = model;
            Freeze = freeze;
            Position = position;
            Rotation = rotation;
            Dimension = dimension;
            Attachment = attachment;
            Datas = new ConcurrentDictionary<string, object>();
            INetworkEntity = Streamer.Streamer.AddEntityObject(this);
        }

        public bool SetAttachToEntity(IEntity target, string bone, Position positionOffset, Rotation rotationOffset)
        {
            AttachToEntity(target, this, bone, positionOffset, rotationOffset);
            return true;
        }

        public bool DetachAttach()
        {
            Attachment = null;
            Streamer.Streamer.UpdateEntityObject(this);
            return true;
        }

        public void Destroy()
        {
            if (Exists)
                Streamer.Streamer.DeleteEntityObject(this);
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
            data["rotation"] = JsonConvert.SerializeObject(Rotation);
            data["freeze"] = Freeze;
            data["attach"] = JsonConvert.SerializeObject(Attachment);
            data["dimension"] = Dimension;
            return data;
        }

        public static WorldObject CreateObject(int model, Vector3 position, Vector3 rotation, bool freeze = false, bool dynamic = false, short dimension = GameMode.GlobalDimension)
        {
            var resuobject = new WorldObject
            (
                model,
                position.ConvertToPosition(),
                rotation.ConvertToEntityRotation(),
                freeze,
                dimension
            );

            ListObject.TryAdd(resuobject.ID, resuobject);        
            resuobject.Exists = true;
            return resuobject;
        }
        public static WorldObject CreateObject(int model, Vector3 position, Vector3 rotation, Models.Attachment attachment = null, bool freeze = false, bool dynamic = false, short dimension = GameMode.GlobalDimension)
        {
            var resuobject = new WorldObject
            (
                model,
                position.ConvertToPosition(),
                rotation.ConvertToEntityRotation(),
                attachment,
                freeze,
                dimension
            );

            ListObject.TryAdd(resuobject.ID, resuobject);
            Streamer.Streamer.AddEntityObject(resuobject);
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
            Streamer.Streamer.UpdateEntityObject(target);
        }

        public static void AttachToEntity(IEntity entity, WorldObject target, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            var attach = new Models.Attachment()
            {
                Bone = bone,
                PositionOffset = positionOffset,
                RotationOffset = rotationOffset,
                RemoteID = entity.Id,
            };

            switch (entity.Type)
            {
                case BaseObjectType.Vehicle:
                    attach.Type = EntityType.Vehicle;
                    break;

                case BaseObjectType.Player:
                    attach.Type = EntityType.Ped;
                    break;
            }

            target.Attachment = attach;
            Streamer.Streamer.UpdateEntityObject(target);
        }

        public static void DetachFromEntity(WorldObject entity)
        {
            entity.Attachment = null;
            Streamer.Streamer.UpdateEntityObject(entity);
        }
    }
}
