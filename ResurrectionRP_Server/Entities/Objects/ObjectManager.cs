using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Streamer.Data;

namespace ResurrectionRP_Server.Entities.Objects
{


    public class ObjectManager
    {
        public ObjectManager()
        {
            AltAsync.OnClient("ObjStream_GetStreamInfo_Srv", ObjStream_GetStreamInfo_Srv);
        }

        public static Object CreateObject(string model, Vector3 position, Vector3 rotation, bool freeze = false, bool dynamic = false, uint dimension = ushort.MaxValue)
        {
            var resuobject = new Object 
            (
                model,
                position.ConvertToPosition(),
                rotation.ConvertToEntityRotation(),
                GameMode.Instance.Streamer.EntityNumber++,
                freeze
            );
            return resuobject;
        }

        public static void AttachEntity(IEntity ent1, Object target, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            var attach = new Models.Attachment()
            {
                Bone = bone,
                PositionOffset = positionOffset,
                RotationOffset = rotationOffset,
                RemoteID = (uint)target.id,
                Type = EntityType.Object
            };

            //var obj = ent1 as IObject;
            //var objhandle = GetHandlerByObject(obj);
            //if (objhandle != null)
            //{
            //     objhandle.Attachment = attach;
            //     await objhandle.UpdateSync();
            //}

        }

        public static async Task Detach(IEntity ent1)
        {
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
    }
}
