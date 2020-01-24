using System.Collections.Generic;
using ResurrectionRP_Server.Streamer.Data;
using Newtonsoft.Json;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using System.Collections.Concurrent;
using System.Numerics;
using AltV.Net.NetworkingEntity.Elements.Entities;
using AltV.Net.NetworkingEntity;
using ResurrectionRP_Server.Models;
using Entity;

namespace ResurrectionRP_Server.Entities.Objects
{
    public class WorldObject : Entity
    {
        public static ConcurrentDictionary<ulong, WorldObject> ListObject = new ConcurrentDictionary<ulong, WorldObject>();

        public int Model;

        public Rotation Rotation;

        public bool Freeze;

        public Attachment Attachment;

        public string Pickup;

        public WorldObject(int model, AltV.Net.Data.Position position, Rotation rotation, bool freeze = false, short dimension = GameMode.GlobalDimension) : base(position, dimension)
        {
            Model = model;
            Freeze = freeze;
            Rotation = rotation;
            Dimension = dimension;
            NetworkEntity = AltNetworking.CreateEntity(position.ConvertToEntityPosition(), dimension, GameMode.StreamDistance, Export(), StreamingType.Default);
        }

        public WorldObject(int model, AltV.Net.Data.Position position, Rotation rotation, Attachment attachment = null, bool freeze = false, short dimension = GameMode.GlobalDimension) : base(position, dimension)
        {
            Model = model;
            Freeze = freeze;
            Rotation = rotation;
            Dimension = dimension;
            Attachment = attachment;
            NetworkEntity = AltNetworking.CreateEntity(position.ConvertToEntityPosition(), dimension, GameMode.StreamDistance, Export(), StreamingType.Default);
        }

        public override Dictionary<string, object> Export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = Model;
            data["entityType"] = (int)EntityType.Object;
            data["rotation"] = JsonConvert.SerializeObject(Rotation);
            data["freeze"] = Freeze;
            data["attach"] = JsonConvert.SerializeObject(Attachment);
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
            return resuobject;
        }
        
        public static void AttachToEntity(WorldObject ent1, WorldObject target, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            var attach = new Attachment()
            {
                Bone = bone,
                PositionOffset = positionOffset,
                RotationOffset = rotationOffset,
                RemoteID = (uint)target.ID,
                Type = EntityType.Object
            };

            target.Attachment = attach;
            ent1.SetData("attach", JsonConvert.SerializeObject(attach));
        }
        
        public static void AttachToEntity(IEntity entity, WorldObject target, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            Attachment attach = new Attachment()
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

            target.NetworkEntity.SetData("attach", JsonConvert.SerializeObject(attach));
        }

        public void DetachEntity()
        {
            Attachment = null;
            NetworkEntity.SetData("attach", string.Empty);
        }

        public bool AttachEntity(IEntity target, string bone, AltV.Net.Data.Position positionOffset, Rotation rotationOffset)
        {
            AttachToEntity(target, this, bone, positionOffset, rotationOffset);
            return true;
        }
    }
}
