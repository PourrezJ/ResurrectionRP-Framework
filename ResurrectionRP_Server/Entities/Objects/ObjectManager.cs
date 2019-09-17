using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Streamer.Data;
using System.Collections.Concurrent;

namespace ResurrectionRP_Server.Entities.Objects
{


    public class ObjectManager
    {
        public ConcurrentDictionary<int, Object> ListObject = new ConcurrentDictionary<int, Object>();
        public ObjectManager()
        {
            AltAsync.OnClient("ObjStream_GetStreamInfo_Srv", ObjStream_GetStreamInfo_Srv);
        }

        public static Object CreateObject(int model, Vector3 position, Vector3 rotation, bool freeze = false, bool dynamic = false, uint dimension = ushort.MaxValue)
        {
            var resuobject = new Object 
            (
                model,
                position.ConvertToPosition(),
                rotation.ConvertToEntityRotation(),
                GameMode.Instance.Streamer.EntityNumber++,
                freeze,
                dimension
            );
            GameMode.Instance.ObjectManager.ListObject[resuobject.id] = resuobject;
            GameMode.Instance.Streamer.AddEntityObject(resuobject);
            return resuobject;
        }
        public static Object CreateObject(string model, Vector3 position, Vector3 rotation, bool freeze = false, bool dynamic = false, uint dimension = ushort.MaxValue)
        {
            var resuobject = new Object 
            (
                (int)Alt.Hash(model),
                position.ConvertToPosition(),
                rotation.ConvertToEntityRotation(),
                GameMode.Instance.Streamer.EntityNumber++,
                freeze,
                dimension
            );
            GameMode.Instance.ObjectManager.ListObject[resuobject.id] = resuobject;
            GameMode.Instance.Streamer.AddEntityObject(resuobject);
            return resuobject;
        }

        public static void AttachToEntity(Object ent1, Object target, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            var attach = new Models.Attachment()
            {
                Bone = bone,
                PositionOffset = positionOffset,
                RotationOffset = rotationOffset,
                RemoteID = (uint)target.id,
                Type = EntityType.Object
            };
            //target.attach = attach;

            //var obj = ent1 as IObject;
            //var objhandle = GetHandlerByObject(obj);
            //if (objhandle != null)
            //{
            //     objhandle.Attachment = attach;
            //     await objhandle.UpdateSync();
            //}

        }
        public static void AttachToEntity(IVehicle vehicle, Object target, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            var attach = new Models.Attachment()
            {
                Bone = bone,
                PositionOffset = positionOffset,
                RotationOffset = rotationOffset,
                RemoteID = (uint)vehicle.Id,
                Type = EntityType.Vehicle
            };
            target.attach = attach;
            GameMode.Instance.Streamer.UpdateEntityObject(target);

        }

        public static Task DetachFromEntity(IEntity ent1, Object target)
        {
            target.attach = null;
            GameMode.Instance.Streamer.UpdateEntityObject(target);
            return Task.CompletedTask;
            //switch (ent1.Type)
            //{
            //    case EntityType.Object:
            //        var obj = ent1 as IObject;
            //        var objhandle = GetHandlerByObject(obj);
            //        if (objhandle != null)
            //        {
            //            objhandle.Attachment = null;
            //            await objhandle.UpdateSync();
            //        }
            //        break;
            //}

            //await MP.Players.CallAsync("ObjStream_Detach", ent1.Id);
        }

        private async Task ObjStream_GetStreamInfo_Srv(IPlayer client, object[] args)
        {
            //if (!arg.Player.Exists)
            //    return;

            //var @object = MP.Objects.GetAt(Convert.ToUInt16(arg.Arguments[0]));
            //ObjectHandler resuobj = GetHandlerByObject(@object);

            //if (resuobj != null)
            //{
            //    await arg.Player.CallAsync("ObjStream_GetStreamInfo_Clt", @object.Id, resuobj);
            //}
            //else
            //{
            //    await arg.Player.CallAsync("ObjStream_GetStreamInfo_Clt", @object.Id, null);
            //}
        }

        public static Object GetHandlerByObject(Object mapObject)
        {
            //if (mapObject != null && mapObject.Exists)
            //{
            //    if (mapObject.TryGetData("ObjectHandler", out object data))
            //    {
            //        return data as ObjectHandler;
            //    }
            //}
            return null;
        }

        public Task DestroyObject(int oid)
        {
            GameMode.Instance.Streamer.DeleteEntityObject(this.ListObject[oid]);
            this.ListObject[oid] = null;
            return Task.CompletedTask;
        }
    }
}
