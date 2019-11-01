using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Address = ResurrectionRP_Server.Phone.Data.Address;
using PhoneSettings = ResurrectionRP_Server.Phone.Data.PhoneSettings;
using Conversation = ResurrectionRP_Server.Phone.Data.Conversation;
using ResurrectionRP_Server.Utils.Enums;
using System.Collections.Concurrent;
using System.Numerics;

namespace ResurrectionRP_Server.Phone
{
    public static class PhoneManager
    {
        #region Private static properties
        private static ConcurrentDictionary<IPlayer, Phone> _ClientPhoneMenu = new ConcurrentDictionary<IPlayer, Phone>();
        public static ConcurrentDictionary<IPlayer, List<Phone>> PhoneClientList = new ConcurrentDictionary<IPlayer, List<Phone>>();
        #endregion

        #region Public Variables
        public static List<string> PhoneNumberList = new List<string>();
        #endregion

        #region Constructor
        public static void Init()
        {
            Alt.OnClient("PhoneMenuCallBack", PhoneMenuCallBack);
        }
        #endregion

        #region Open Menu
        public static bool OpenPhone(IPlayer client, Phone phone, bool incomingCall = false, string contactNumber = null, string contactName = null)
        {
            try
            {
                Phone oldMenu = null;
                _ClientPhoneMenu.Remove(client, out oldMenu);

                if (_ClientPhoneMenu.TryAdd(client, phone))
                {
/*                    phone.props = Entities.Objects.WorldObject.CreateObject("prop_npc_phone_02", client.Position, new Vector3(), false, false);
                    phone.props.SetAttachToEntity(client, "PH_L_Hand", new AltV.Net.Data.Position(), new AltV.Net.Data.Rotation());*/

/*                    var attach = new Models.Attachment()
                    {
                        Bone = "PH_R_Hand",
                        PositionOffset = new Vector3(0.04f,0,-0.03f),
                        RotationOffset = new Vector3(80,0,-80),
                        Type = (int)Streamer.Data.EntityType.Ped,
                        RemoteID = client.Id
                    };*/
                    var attach = new Models.Attachment()
                    {
                        Bone = "PH_R_Hand_PHONE",
                        PositionOffset = new Vector3(),
                        RotationOffset = new Vector3(),
                        Type = (int)Streamer.Data.EntityType.Ped,
                        RemoteID = client.Id
                    };
                    phone.props= Entities.Objects.WorldObject.CreateObject((int)Alt.Hash("prop_npc_phone_02"), client.Position.ConvertToVector3(), new System.Numerics.Vector3(), attach, false);

                    if (!phone.props.Exists)
                         return false;
     

                    client.PlayAnimation((client.IsInVehicle) ? "cellphone@in_car@ds" : (client.Model == Alt.Hash("mp_f_freemode_01")) ? "cellphone@female" : "cellphone@", "cellphone_text_read_base", 3, -1, -1, (AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.Loop | AnimationFlags.SecondaryTask));

                    long newMessagesCount = GetNewMessagesOnConversationsCount(phone.PhoneNumber);
                    client.Emit("OpenPhone", newMessagesCount, JsonConvert.SerializeObject(phone.Settings), incomingCall, contactNumber, contactName);
                    //client.EmitLocked("ShowCursor", true);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OpenPhone " +ex);
                return false;
            }
        }
        #endregion

        #region Methods
        public static Phone GeneratePhone()
        {
            Phone phone = new Phone();

            var phoneNumber = GeneratePhoneNumber();
            PhoneNumberList.Add(phoneNumber);

            phone.PhoneNumber = phoneNumber;
            phone.AddressBook.Add(new Address() { contactName = "Votre Numéro", phoneNumber = phone.PhoneNumber });
            if(GameMode.IsDebug)
                Alt.Server.LogInfo("[PhoneManager.Generatephone()] Generate new phone number: " + phoneNumber);
            return phone;
        }

        private static string GeneratePhoneNumber()
        {

            string phoneNumber = $"{Utils.Utils.RandomNumber(1000, 9999)}-{Utils.Utils.RandomNumber(1000, 9999)}";
            while (PhoneNumberList.Contains(phoneNumber))
            {
                phoneNumber = $"{Utils.Utils.RandomNumber(1000, 9999)}-{Utils.Utils.RandomNumber(1000, 9999)}";
            }
            return phoneNumber;
        }

        public static bool HasOpenPhone(IPlayer client, out Phone phone)
        {
            phone = null;
            if (_ClientPhoneMenu.ContainsKey(client))
            {
                phone = _ClientPhoneMenu[client];
                return true;
            }
            return false;
        }
        #endregion

        private static void PhoneMenuCallBack(IPlayer client, object[] args)
        {
            if (client == null || !client.Exists)
                return;

            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();

            if (!_ClientPhoneMenu.TryGetValue(client, out Phone phone))
                return;

            if (phone == null || ph == null)
                return;

            switch (args[0])
            {
                case "SavePhoneSettings":
                    phone.Settings = JsonConvert.DeserializeObject<PhoneSettings>(args[1].ToString()) ?? new PhoneSettings();
                    break;

                case "GetContacts":
                    phone.SendContactListToClient(client);
                    break;

                case "AddOrEditContact":
                    Address contact = JsonConvert.DeserializeObject<Address>(args[1].ToString());

                    if (string.IsNullOrEmpty(contact.originalNumber) && phone.TryAddNewContact(client, contact.contactName, contact.phoneNumber))
                        client.SendNotificationSuccess($"Vous avez ajouté le contact {contact.contactName}");
                    else if (phone.TryEditContact(client, contact.contactName, contact.phoneNumber, contact.originalNumber))
                        client.SendNotificationSuccess($"Vous avez édité le contact {contact.contactName}");

                    ph.UpdateFull();
                    break;

                case "RemoveContact":
                    if (phone.RemoveContactFromAddressBook(args[1].ToString()))
                        client.SendNotificationSuccess("Contact Supprimé!!");

                    ph.UpdateFull();
                    break;

                case "getConversationsV2":
                    phone.LoadConversations(client);
                    break;

                case "DeleteConversation":
                    var filter = Builders<Conversation>.Filter.And(
                        Builders<Conversation>.Filter.Eq(p => p.receiver, (string)args[1]),
                        Builders<Conversation>.Filter.Eq(p => p.sender, phone.PhoneNumber)
                    );

                    if (Database.MongoDB.GetCollectionSafe<Conversation>("conversations").DeleteOne(filter).DeletedCount > 0)
                    {
                        client.SendNotification("Conversation supprimée");
                        client.Emit("deletedConversation");
                    }
                    break;

                case "GetMessages":
                    phone.GetMessages(client, (string)args[1]);
                    break;

                case "SendMessage":
                    phone.SendSMS(client, (string)args[1], (string)args[2]);
                    break;

                case "initiateCall":
                    phone.InitiateCall(client, args[1].ToString());
                    break;

                case "cancelCall":
                    phone.CancelCall(client, args[1].ToString());
                    break;

                case "endCall":
                    phone.EndCall(client, args[1].ToString());
                    break;

                case "acceptCall":
                    phone.StartCall(client, args[1].ToString());
                    break;

                case "ClosePhone":
                    ClosePhone(client);
                    break;

                default:
                    Alt.Server.LogError("PhoneManager  PhoneMenuCallback, is arg[0] the event name ?");
                    break;
            }
        }

        public static void ClosePhone(IPlayer client)
        {
            if( _ClientPhoneMenu.TryRemove(client, out Phone value))
            {
                client.EmitLocked("ClosePhone");

                Utils.Utils.Delay(750, () =>
                {
                    AltAsync.Do(() =>
                    {
                        if (value.props != null)
                            value.props.Destroy();
                    });
                });
            }
        }

        #region Gestion des conversations (liste de messages) 
        public static async Task<Conversation> FindOrCreateConversation(string sender, string receiver, bool createConv = true)
        {
            try
            {
                var filter = Builders<Conversation>.Filter.And(
                    Builders<Conversation>.Filter.Eq(p => p.receiver, receiver),
                    Builders<Conversation>.Filter.Eq(p => p.sender, sender)
                );

                var conv = await Database.MongoDB.GetCollectionSafe<Conversation>("conversations").Find(filter).FirstOrDefaultAsync();
                if (conv != null)
                    return conv;

                if (createConv)
                {
                    var convers = new Conversation { sender = sender, receiver = receiver };
                    await Database.MongoDB.GetCollectionSafe<Conversation>("conversations").InsertOneAsync(convers);
                    return convers;
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("FindOrCreateConversation" + ex);
            }

            return null;
        }

        public static long GetNewMessagesOnConversationsCount(string phoneNumber)
        {
            try
            {
                var filter = Builders<Conversation>.Filter.And(
                    Builders<Conversation>.Filter.Eq(p => p.receiver, phoneNumber),
                    Builders<Conversation>.Filter.Eq(p => p.sender, phoneNumber)
                    );

                return Database.MongoDB.GetCollectionSafe<Conversation>("conversations").Find(filter).ToList().FindAll(x => x.lastReadDate < x.lastMessageDate).LongCount();
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("PhoneManager GetNewMessagesOnConversationsCount"  + ex);
            }
            return 0;
        }
        #endregion
    }
}
