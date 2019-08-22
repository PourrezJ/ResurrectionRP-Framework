using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Objects
{/*
    public class ObjectHandler
    {
        [BsonIgnore, JsonIgnore]
        public IObject IObject;

        public uint NetHandle;
        public Attachment Attachment;
        public bool Freeze;
        public bool Dynamic;

        public async Task AttachObject(IEntity ent2, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            await ObjectHandlerManager.AttachEntity(IObject, ent2, bone, positionOffset, rotationOffset);
        }

        public async Task DetachObject(IEntity ent2)
        {
            await ObjectHandlerManager.Detach(ent2);
        }

        public async Task UpdateSync()
        {
            await MP.Players.CallAsync("ObjStream_GetStreamInfo_Clt", IObject.Id, this);
        }

        public async Task Destroy()
        {
            if (IObject != null && IObject.Exists)
            {
                await MP.Players.CallAsync("ObjStream_Destroy", IObject.Id, this);

                await IObject.DestroyAsync();
            }
        }
    }

    public class ObjectHandlerManager
    {
        public ObjectHandlerManager()
        {
            MP.Events.Add("ObjStream_GetStreamInfo_Srv", ObjStream_GetStreamInfo_Srv);
        }

        public static async Task<ObjectHandler> CreateObject(uint model, Vector3 position, Vector3 rotation, bool freeze = false, bool dynamic = false, uint dimension = MP.GlobalDimension)
        {
            await Task.Delay(20);
            var obj = await MP.Objects.NewAsync(model, position, rotation, dimension);

            var resuobject = new ObjectHandler
            {
                IObject = obj,
                Freeze = freeze,
                Dynamic = dynamic
            };

            resuobject.NetHandle = resuobject.IObject.Id;

            resuobject.IObject.SetData("ObjectHandler", resuobject);

            return resuobject;
        }

        public static async Task AttachEntity(IEntity ent1, IEntity ent2, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            var attach = new Attachment()
            {
                Bone = bone,
                PositionOffset = positionOffset,
                RotationOffset = rotationOffset,
                RemoteID = ent2.Id,
                Type = (byte)ent2.Type
            };

            switch (ent1.Type)
            {
                case EntityType.Object:
                    var obj = ent1 as IObject;
                    var objhandle = GetHandlerByObject(obj);
                    if (objhandle != null)
                    {
                        objhandle.Attachment = attach;
                        await objhandle.UpdateSync();
                    }

                    break;
            }
        }

        public static async Task Detach(IEntity ent1)
        {
            switch (ent1.Type)
            {
                case EntityType.Object:
                    var obj = ent1 as IObject;
                    var objhandle = GetHandlerByObject(obj);
                    if (objhandle != null)
                    {
                        objhandle.Attachment = null;
                        await objhandle.UpdateSync();
                    }
                    break;
            }

            await MP.Players.CallAsync("ObjStream_Detach", ent1.Id);
        }

        private async Task ObjStream_GetStreamInfo_Srv(object sender, PlayerRemoteEventEventArgs arg)
        {
            if (!arg.Player.Exists)
                return;

            var @object = MP.Objects.GetAt(Convert.ToUInt16(arg.Arguments[0]));
            ObjectHandler resuobj = GetHandlerByObject(@object);

            if (resuobj != null)
            {
                await arg.Player.CallAsync("ObjStream_GetStreamInfo_Clt", @object.Id, resuobj);
            }
            else
            {
                await arg.Player.CallAsync("ObjStream_GetStreamInfo_Clt", @object.Id, null);
            }
        }

        public static ObjectHandler GetHandlerByObject(IObject mapObject)
        {
            if (mapObject != null && mapObject.Exists)
            {
                if (mapObject.TryGetData("ObjectHandler", out object data))
                {
                    return data as ObjectHandler;
                }
            }
            return null;
        }
    }*/
}
