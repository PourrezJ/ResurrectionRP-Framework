using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Address = ResurrectionRP_Server.Phone.Data.Address;
using PhoneState = ResurrectionRP_Server.Phone.Data.PhoneState;
using PhoneSettings = ResurrectionRP_Server.Phone.Data.PhoneSettings;
using Conversation = ResurrectionRP_Server.Phone.Data.Conversation;
using SMS = ResurrectionRP_Server.Phone.Data.SMS;

namespace ResurrectionRP_Server.Phone
{
    public class PhoneManager
    {
        #region Private static properties
        private static Dictionary<IPlayer, Phone> _ClientPhoneMenu = new Dictionary<IPlayer, Phone>();
        public Dictionary<IPlayer, List<Phone>> PhoneClientList = new Dictionary<IPlayer, List<Phone>>();

        private static Vector3 femaleUsingPhonePosition = new Vector3(0.14f, 0, -0.021f);
        private static Vector3 femaleUsingPhoneRotation = new Vector3(130, 115, 0);
        private static Vector3 femaleCallingPhonePosition = new Vector3(0.1f, 0, -0.021f);
        private static Vector3 femaleCallingPhoneRotation = new Vector3(90, 100, 0);

        private static Vector3 maleUsingPhoneRotation = new Vector3(130, 100, 0);
        private static Vector3 maleUsingPhonePosition = new Vector3(0.17f, 0.021f, -0.009f);
        private static Vector3 maleCallingPhoneRotation = new Vector3(120, 100, 0);
        private static Vector3 maleCallingPhonePosition = new Vector3(0.16f, 0.02f, -0.01f);
        #endregion

        #region Public Variables
        public List<string> PhoneNumberList = new List<string>();
        #endregion

        #region Constructor
        public PhoneManager()
        {
            PhoneClientList = new Dictionary<IPlayer, List<Phone>>();
            AltAsync.OnClient("PhoneMenuCallBack", PhoneMenuCallBack);
            AltAsync.OnClient("CallOpenPhone", EventTrigered);
            AltAsync.OnClient("ClosePhone", EventTrigered);
        }
        #endregion

        #region Open Menu
        public static async Task<bool> OpenPhone(IPlayer client, Phone phone, bool incomingCall = false, string contactNumber = null, string contactName = null)
        {
            try
            {
                Phone oldMenu = null;
                _ClientPhoneMenu.Remove(client, out oldMenu);

                if (oldMenu != null)
                {
                    // Callback fermeture telephone
                }

                if (_ClientPhoneMenu.TryAdd(client, phone))
                {
                    // phone.props = await ObjectHandlerManager.CreateObject(MP.Utility.Joaat("prop_npc_phone_02"), client.Position, client.Rotation);
                    /*
                     if (phone.props == null)
                         return false;
     */
                    long newMessagesCount = GetNewMessagesOnConversationsCount(phone.PhoneNumber);
                    await client.EmitAsync("OpenPhone", newMessagesCount, JsonConvert.SerializeObject(phone.Settings), incomingCall, contactNumber, contactName);
                    await client.EmitAsync("showCursor", true);

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
            GameMode.Instance.PhoneManager.PhoneNumberList.Add(phoneNumber);

            phone.PhoneNumber = phoneNumber;
            phone.AddressBook.Add(new Address() { contactName = "Votre Numéro", phoneNumber = phone.PhoneNumber });

            return phone;
        }

        private static string GeneratePhoneNumber()
        {

            string phoneNumber = $"{Utils.Utils.RandomNumber(1000, 9999)}-{Utils.Utils.RandomNumber(1000, 9999)}";
            while (GameMode.Instance.PhoneManager.PhoneNumberList.Contains(phoneNumber))
            {
                phoneNumber = $"{Utils.Utils.RandomNumber(1000, 9999)}-{Utils.Utils.RandomNumber(1000, 9999)}";
            }
            return phoneNumber;
        }

        public static bool HasOpenMenu(IPlayer client, out Phone phone)
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

        private async Task EventTrigered(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();
            if (ph == null)
                return;
            if (args[0] == null)
            {
                Alt.Server.LogError("EventTrigered in PhoneManager, not argument to tell if CallOpen / Close... maybe client side problem ?");
                return;
            }
            switch (args[0])
            {
                case "CallOpenPhone":
                    if (ph.PhoneSelected != null)
                        await OpenPhone(client, ph.PhoneSelected);
                    break;

                case "ClosePhone":
                    if (ph.PhoneSelected != null)
                        await ph.PhoneSelected.ClosePhone();
                    break;
            }
        }

        private async Task PhoneMenuCallBack(IPlayer client, object[] args)
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
                    await phone?.SendContactListToClient(client);
                    break;

                case "AddOrEditContact":
                    Address contact = JsonConvert.DeserializeObject<Address>(args[1].ToString());

                    if (string.IsNullOrEmpty(contact.originalNumber) && await phone.TryAddNewContact(client, contact.contactName, contact.phoneNumber))
                        await client.SendNotificationSuccess($"Vous avez ajouté le contact {contact.contactName}");
                    else if (await phone.TryEditContact(client, contact.contactName, contact.phoneNumber, contact.originalNumber))
                        await client.SendNotificationSuccess($"Vous avez édité le contact {contact.contactName}");

                    await ph.Update();
                    break;

                case "RemoveContact":
                    if (await phone.RemoveContactFromAddressBook(args[1].ToString()))
                        await client.SendNotificationSuccess("Contact Supprimé!!");

                    await ph.Update();
                    break;

                case "getConversationsV2":
                    await phone.LoadConversations(client);
                    break;

                case "DeleteConversation":
                    var filter = Builders<Conversation>.Filter.And(
                        Builders<Conversation>.Filter.Eq(p => p.receiver, (string)args[1]),
                        Builders<Conversation>.Filter.Eq(p => p.sender, phone.PhoneNumber)
                    );

                    if (Database.MongoDB.GetCollectionSafe<Conversation>("conversations").DeleteOne(filter).DeletedCount > 0)
                    {
                        await client.NotifyAsync("Conversation supprimée");
                        await client.EmitAsync("deletedConversation");
                    }
                    break;

                case "getMessages":
                    await phone.GetMessages(client, (string)args[1]);
                    break;

                case "SendMessage":
                    await phone.SendSMS(client, (string)args[1], (string)args[2]);
                    break;

                case "initiateCall":
                    await phone.InitiateCall(client, args[1].ToString());
                    break;

                case "cancelCall":
                    await phone.CancelCall(client, args[1].ToString());
                    break;

                case "endCall":
                    await phone.EndCall(client, args[1].ToString());
                    break;

                case "acceptCall":
                    await phone.StartCall(client, args[1].ToString());
                    break;
                default:
                    Alt.Server.LogError("PhoneManager  PhoneMenuCallback, is arg[0] the event name ?");
                    break;
            }
        }

        public async static void ClosePhone(IPlayer client)
        {
            await client.EmitAsync("ClosePhone");
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
