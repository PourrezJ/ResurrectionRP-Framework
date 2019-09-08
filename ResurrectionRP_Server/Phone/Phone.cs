using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Async;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Address = ResurrectionRP_Server.Phone.Data.Address;
using PhoneState = ResurrectionRP_Server.Phone.Data.PhoneState;
using PhoneSettings = ResurrectionRP_Server.Phone.Data.PhoneSettings;
using Conversation = ResurrectionRP_Server.Phone.Data.Conversation;
using SMS = ResurrectionRP_Server.Phone.Data.SMS;

namespace ResurrectionRP_Server.Phone
{
    public class Phone
    {
        public string PhoneNumber;
        public string PhoneName;
        public bool Actived;

        public List<Address> AddressBook = new List<Address>();

        private PhoneSettings _settings = new PhoneSettings();
        public PhoneSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new PhoneSettings();
                return _settings;
            }
            set => _settings = value;
        }

        [JsonIgnore, BsonIgnore]
        public Entities.Objects.ObjectHandler props;

        public Phone()
        {
            Actived = true;
        }

        #region Utils

        public string GetNameForNumber(string number)
        {
            try
            {
                return AddressBook.Single(x => x.phoneNumber.Equals(number)).contactName ?? number;
            }
            catch (Exception)
            {
                return number;
            }
        }

        public bool HasContactForNumber(String number)
        {
            return AddressBook.Any(x => x.phoneNumber.Equals(number));
        }
        #endregion

        #region Gestion des messages

        public async Task SendSMS(IPlayer client, string receiver, string message)
        { 
            try
            {
                Conversation conv1 = await PhoneManager.FindOrCreateConversation(this.PhoneNumber, receiver);
                Conversation conv2 = await PhoneManager.FindOrCreateConversation(receiver, this.PhoneNumber);

                conv1.messages.Add(new SMS() { content = message, isOwn = true, sentAt = DateTime.Now });
                conv2.messages.Add(new SMS() { content = message, isOwn = false, sentAt = DateTime.Now });

                conv2.lastMessageDate = DateTime.Now;

                await conv1.Update();
                await conv2.Update();

                await GetMessages(client, receiver);

                IPlayer _client = GetClientWithPhoneNumber(this.PhoneNumber);

                if (_client != null)
                {
                    string contactName = GetNameForNumber(receiver);
                    await _client.NotifyAsync("Message bien envoyé " + ("à ~b~~h~" + contactName ?? "au ~b~~h~" + receiver));
                    await _client.PlaySoundFrontEndFix(-1, "MP_5_SECOND_TIMER", "HUD_FRONTEND_DEFAULT_SOUNDSET");
                }

                Phone _phone = GetPhoneWithPhoneNumber(receiver);
                if (_phone != null)
                {
                    await _phone.NewSMS(this.PhoneNumber);
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(this.PhoneNumber + " " + receiver + ex);
            }
        }

        public async Task NewSMS(string phoneNumber)
        {
            IPlayer _client = GetClientWithPhoneNumber(this.PhoneNumber);

            if (_client == null)
                return;

            string contactName = GetNameForNumber(phoneNumber);
            await _client.NotifyAsync("Nouveau message de ~b~~h~" + (contactName ?? phoneNumber));

            //var receverList = await MP.Players.GetInRangeAsync( _client.GetPosition(), 3f);
            var receverList = _client.GetPlayersInRange(3f);

            if (Settings == null)
                return;

            if (Settings.silenceMode)
                return;


            foreach (IPlayer recever in receverList)
            {
                if (recever != null && recever.Exists)
                    await recever?.PlaySoundFromEntity(_client, -1, "MP_5_SECOND_TIMER", "HUD_FRONTEND_DEFAULT_SOUNDSET");
            }
        }

        public async Task GetMessages(IPlayer client, string phoneNumber)
        {
            Conversation checkConv = await PhoneManager.FindOrCreateConversation(PhoneNumber, phoneNumber, false);
            //checkConv.messages.Reverse();
            if (checkConv != null)
                await client.EmitAsync("MessagesReturned", JsonConvert.SerializeObject(checkConv));
        }

        #endregion

        #region Gestion des appels
        public async Task InitiateCall(IPlayer client, string phoneNumber)
        {
            IPlayer _client = GetClientWithPhoneNumber(phoneNumber);
            if (_client != null)
            {
                if (!_client.HasData("InToPhoneCommunication"))
                {
                    string contactName = this.GetNameForNumber(phoneNumber);
                    await client.EmitAsync("initiatedCall", phoneNumber, contactName);

                    var phoneDistant = GetPhoneWithPhoneNumber(phoneNumber);
                    string callerName = phoneDistant.GetNameForNumber(PhoneNumber);

                    if (PhoneManager.HasOpenPhone(client, out Phone phone))
                    {
                        await PhoneManager.OpenPhone(_client, phoneDistant, true, PhoneNumber, callerName);
                    }
                }
                else
                {
                    await client.SendNotificationError("Le contact est déjà en communication vocale.");
                    await client.EmitAsync("canceledCall");
                }
            }
            else
            {
                await client.SendNotificationError("Le contact n'est actuellement pas disponible");
            }
        }

        public async Task StartCall(IPlayer client, string phoneNumber)
        {
            var calledPlayer = GetClientWithPhoneNumber(phoneNumber);

            if (calledPlayer != null)
            {
                 calledPlayer.Emit("StartedCall", client.Id);
                 await client.EmitAsync("StartedCall", calledPlayer.Id);
                calledPlayer.SetData("InToPhoneCommunication", true);
                client.SetData("InToPhoneCommunication", true);
            }
        }

        public async Task CancelCall(IPlayer player, string phoneNumber)
        {
            var calledPlayer = GetClientWithPhoneNumber(phoneNumber);
            if (calledPlayer != null)
            {
                calledPlayer.ResetData("InToPhoneCommunication");
                player.ResetData("InToPhoneCommunication");
                 calledPlayer.Emit("canceledCall");
                 player.Emit("canceledCall");
            }
        }

        public async Task EndCall(IPlayer player, string phoneNumber)
        {
            var calledPlayer = GetClientWithPhoneNumber(phoneNumber);

            if (calledPlayer != null)
            {
                 calledPlayer.Emit("endedCall");
                 player.Emit("endedCall");

                calledPlayer.ResetData("InToPhoneCommunication");
                player.ResetData("InToPhoneCommunication");
            }
        }

        #endregion

        #region Gestion des contacts

        /// <summary>
        /// Attempts to add a new contact
        /// </summary>
        /// <param name="client">Client</param>
        /// <param name="contactName">Contact name</param>
        /// <param name="contactNumber">Contact number</param>
        public async Task<bool> TryAddNewContact(IPlayer client, String contactName, String contactNumber, bool message = true)
        {
            if (await ValidateContact(contactName, contactNumber))
            {
                await AddNameToAddressBook(client, contactName, contactNumber, message);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to edit an existing contact
        /// </summary>
        /// <param name="client">Client</param>
        /// <param name="contactName">Contact name</param>
        /// <param name="contactNumber">Contact number</param>
        public async Task<bool> TryEditContact(IPlayer client, String contactName, String contactNumber, String originalNumber, bool message = true)
        {
            if (await ValidateContact(contactName, contactNumber, true))
            {
                Address foundAddress = AddressBook.Find(address => address.phoneNumber == originalNumber);
                if (foundAddress.phoneNumber != null && foundAddress.phoneNumber != "")
                {
                    await RemoveContactFromAddressBook(originalNumber);
                    await AddNameToAddressBook(client, contactName, contactNumber);

                    await client.EmitAsync("ContactEdited");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Validate phone contact
        /// Contact name, number etc
        /// </summary>
        /// <param name="c">Client who adds the contact</param>
        /// <param name="name">Contact name</param>
        /// <param name="number">Contact number</param>
        /// <returns>True if validated, otherwise false</returns>
        private async Task<bool> ValidateContact(String name, String number, bool edit = false)
        {
            IPlayer _client = GetClientWithPhoneNumber(PhoneNumber);
            if (_client != null)
            {
                if (name.Length == 0)
                {
                    await _client.SendNotificationError("Le nom du contact ne peut être vide!");
                    return false;
                }

                if (number.Length != 9)
                {
                    await _client.SendNotificationError("Le numéro de téléphone doit comporter 9 chiffres!");
                    return false;
                }

                if (name.Length > 17)
                {
                    await _client.SendNotificationError("Le nom ne peut pas être plus long que 17 caractères!");
                    return false;
                }

                if (!edit && HasContactForNumber(number))
                {
                    await _client.SendNotificationError("Vous avez déjà un contact pour le numéro " + number);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds new contact to address book
        /// </summary>
        /// <param name="name">Contact name</param>
        /// <param name="number">Contact number</param>
        public async Task AddNameToAddressBook(IPlayer client, String name, String number, bool message = false)
        {
            AddressBook.Add(new Address() { contactName = name, phoneNumber = number });
            if (message)
                await client?.SendNotificationSuccess("Contact Ajouté!!");

            client?.Emit("ContactEdited");
        }

        /// <summary>
        /// Removes a contact from address book
        /// </summary>
        /// <param name="number">Contact number</param>
        public async Task<bool> RemoveContactFromAddressBook(string number)
        {
            int indexToRemove = -1;
            for (int i = 0; i < AddressBook.Count; i++)
            {
                Address a = AddressBook.ElementAt(i);
                if (a.phoneNumber.Equals(number))
                {
                    indexToRemove = i;
                    break;
                }
            }

            if (indexToRemove != -1)
            {
                AddressBook.RemoveAt(indexToRemove);
                var client = GetClientWithPhoneNumber(PhoneNumber);
                client?.Emit("ContactEdited");
                return true;
            }
            return false;
        }

        public async Task SendContactListToClient(IPlayer client)
        {
            await client.EmitAsync("ContactReturned", JsonConvert.SerializeObject(AddressBook));
        }

        #endregion

        #region Gestion des conversations
        public async Task LoadConversations(IPlayer client)
        {
            try
            {
                var list = await Database.MongoDB.GetCollectionSafe<Conversation>("conversations").Find(p => p.sender == PhoneNumber).ToListAsync();
                if (list.Count > 0)
                {
                    list.OrderByDescending(x => x.lastMessageDate);
                    list.ForEach((Conversation conv) =>
                    {
                        Address _adress = this.AddressBook.Find(p => p.phoneNumber == conv.receiver);

                        conv.receiverName = (_adress != null) ? _adress.contactName : conv.receiver;
                    });
                    await client.EmitAsync("ConversationsReturnedV2", JsonConvert.SerializeObject(list));
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("LoadConversations " + ex);
            }

        }


        #endregion

        #region Général

        public async Task ClosePhone()
        {/*
            if (props != null)
                await props.Destroy();*/
        }

        #endregion

        #region Static Methods
        public static void AddPhoneInList(IPlayer client, Phone phone)
        {
            //lock (GameMode.Instance.PhoneManager.PhoneClientList)
            //{
                if (GameMode.Instance.PhoneManager.PhoneClientList.ContainsKey(client))
                {
                    if (!GameMode.Instance.PhoneManager.PhoneClientList[client].Exists(p => p?.PhoneNumber == phone?.PhoneNumber))
                    {
                        GameMode.Instance.PhoneManager.PhoneClientList[client].Add(phone);
                    }
                }
                else
                {
                    GameMode.Instance.PhoneManager.PhoneClientList.Add(client, new List<Phone>() { phone });
                }
            //}
        }

        public static void RemovePhoneInList(IPlayer client, Phone phonee)
        {
            //lock (GameMode.Instance.PhoneManager.PhoneClientList)
            //{
                if (GameMode.Instance.PhoneManager.PhoneClientList.ContainsKey(client))
                {
                    if (GameMode.Instance.PhoneManager.PhoneClientList[client].Exists(p => p.PhoneNumber == phonee.PhoneNumber))
                    {
                        var id = GameMode.Instance.PhoneManager.PhoneClientList[client].FindIndex(p => p.PhoneNumber == phonee.PhoneNumber);
                        GameMode.Instance.PhoneManager.PhoneClientList[client].RemoveAt(id);
                    }
                }
            //}
        }

        public static IPlayer GetClientWithPhoneNumber(string phoneNumber)
        {
            
            //lock (GameMode.Instance.PhoneManager.PhoneClientList)
            //{
                foreach (var phones in GameMode.Instance.PhoneManager.PhoneClientList)
                {
                    if (phones.Value.Exists(f => f.PhoneNumber == phoneNumber))
                    {
                        return phones.Key;
                    }
                }
            //}
            return null;
        }

        public static Phone GetPhoneWithPhoneNumber(string phoneNumber)
        {
            //lock (GameMode.Instance.PhoneManager.PhoneClientList)
            //{
                foreach (var phones in GameMode.Instance.PhoneManager.PhoneClientList)
                {
                    if (phones.Value.Exists(f => f.PhoneNumber == phoneNumber))
                    {
                        return phones.Value.Find(p => p.PhoneNumber == phoneNumber);
                    }
                }
            //}

            return null;
        }
        #endregion
    }
}
