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
        public Entities.Objects.WorldObject props;

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

                GetMessages(client, receiver);

                IPlayer _client = GetClientWithPhoneNumber(this.PhoneNumber);

                if (_client != null)
                {
                    string contactName = GetNameForNumber(receiver);
                    _client.SendNotification("Message bien envoyé " + ("à " + contactName ?? "au " + receiver));
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

            if (! await _client.ExistsAsync())
                return;

            string contactName = GetNameForNumber(phoneNumber);
            _client.SendNotification("Nouveau message de ~b~~h~" + (contactName ?? phoneNumber));

            var receverList = _client.GetPlayersInRange(3f);

            if (Settings == null)
                return;

            if (Settings.silenceMode)
                return;

            await AltAsync.Do(() =>
            {
                foreach (IPlayer recever in receverList)
                {
                    if (!recever.Exists)
                        continue;

                    if (recever != null && recever.Exists)
                        recever.PlaySoundFromEntity(_client, -1, "MP_5_SECOND_TIMER", "HUD_FRONTEND_DEFAULT_SOUNDSET");
                }
            });

        }

        public void GetMessages(IPlayer client, string phoneNumber)
        {
            Task.Run(async() =>
            {
                Conversation checkConv = await PhoneManager.FindOrCreateConversation(PhoneNumber, phoneNumber, false);
                //checkConv.messages.Reverse();
                if (checkConv != null)
                    await client.EmitAsync("MessagesReturned", JsonConvert.SerializeObject(checkConv));
            });
        }

        #endregion

        #region Gestion des appels
        public void InitiateCall(IPlayer client, string phoneNumber)
        {
            IPlayer _client = GetClientWithPhoneNumber(phoneNumber);
            if (_client != null)
            {
                bool incommunication = false;
                _client.GetData<bool>("InToPhoneCommunication", out incommunication);

                if (!incommunication)
                {
                    string contactName = this.GetNameForNumber(phoneNumber);
                    client.EmitLocked("initiatedCall", phoneNumber, contactName);

                    var phoneDistant = GetPhoneWithPhoneNumber(phoneNumber);
                    string callerName = phoneDistant.GetNameForNumber(PhoneNumber);

                    if (PhoneManager.HasOpenPhone(client, out Phone phone))
                    {
                        PhoneManager.OpenPhone(_client, phoneDistant, true, PhoneNumber, callerName);
                    }
                }
                else
                {
                    client.SendNotificationError("Le contact est déjà en communication vocale.");
                    client.EmitLocked("canceledCall");
                }
            }
            else
            {
                client.SendNotificationError("Le contact n'est actuellement pas disponible");
            }
        }

        public void StartCall(IPlayer client, string phoneNumber)
        {
            var calledPlayer = GetClientWithPhoneNumber(phoneNumber);

            if (calledPlayer != null)
            {
                calledPlayer.EmitLocked("StartedCall", client);
                client.EmitLocked("StartedCall", calledPlayer);
                calledPlayer.SetData("InToPhoneCommunication", true);
                client.SetData("InToPhoneCommunication", true);
            }
        }

        public void CancelCall(IPlayer player, string phoneNumber)
        {
            var calledPlayer = GetClientWithPhoneNumber(phoneNumber);
            if (calledPlayer != null)
            {
                 calledPlayer.ResetData("InToPhoneCommunication");
                 player.ResetData("InToPhoneCommunication");
                 calledPlayer.EmitLocked("CanceledCall");
                 player.EmitLocked("CanceledCall");
            }
        }

        public void EndCall(IPlayer player, string phoneNumber)
        {
            var calledPlayer = GetClientWithPhoneNumber(phoneNumber);

            if (calledPlayer != null)
            {
                if (!calledPlayer.Exists)
                    return;

                if (player.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                    calledPlayer.EmitLocked("endedCall", tsName);

                if (calledPlayer.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsNamee))
                    player.EmitLocked("endedCall", tsNamee);

                calledPlayer.ResetData("InToPhoneCommunication");
                player.ResetData("InToPhoneCommunication");
            }
        }

        #endregion

        #region Gestion des contacts

        public bool TryAddNewContact(IPlayer client, String contactName, String contactNumber)
        {
            if (ValidateContact(contactName, contactNumber))
            {
                AddNameToAddressBook(client, contactName, contactNumber);
                return true;
            }
            return false;
        }

        public bool TryEditContact(IPlayer client, String contactName, String contactNumber, String originalNumber, bool message = true)
        {
            try
            {
                if (ValidateContact(contactName, contactNumber, true))
                {
                    if (AddressBook == null)
                        return false;

                    Address foundAddress = AddressBook.Find(address => address?.phoneNumber == originalNumber);
                    if (foundAddress.phoneNumber != null && foundAddress.phoneNumber != "")
                    {
                        RemoveContactFromAddressBook(originalNumber);
                        AddNameToAddressBook(client, contactName, contactNumber);

                        client.EmitLocked("ContactEdited");
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }

            return false;
        }

        private bool ValidateContact(String name, String number, bool edit = false)
        {
            IPlayer _client = GetClientWithPhoneNumber(PhoneNumber);
            if (_client != null)
            {
                if (name.Length == 0)
                {
                    _client.SendNotificationError("Le nom du contact ne peut être vide!");
                    return false;
                }

                if (number.Length != 9)
                {
                    _client.SendNotificationError("Le numéro de téléphone doit comporter 9 chiffres!");
                    return false;
                }

                if (name.Length > 17)
                {
                    _client.SendNotificationError("Le nom ne peut pas être plus long que 17 caractères!");
                    return false;
                }

                if (!edit && HasContactForNumber(number))
                {
                    _client.SendNotificationError("Vous avez déjà un contact pour le numéro " + number);
                    return false;
                }
            }
            return true;
        }

        public void AddNameToAddressBook(IPlayer client, String name, String number)
        {
            AddressBook.Add(new Address() { contactName = name, phoneNumber = number });
            client.EmitLocked("ContactEdited");
        }

        public bool RemoveContactFromAddressBook(string number)
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
                client?.EmitLocked("ContactEdited");
                return true;
            }
            return false;
        }

        public void SendContactListToClient(IPlayer client)
        {
            if (AddressBook == null)
                return;

            List<Address> contacts = AddressBook.GetRange(1, AddressBook.Count - 1).OrderBy(c => c?.contactName).ToList();
            contacts.Insert(0, AddressBook[0]);
            client.EmitLocked("ContactReturned", JsonConvert.SerializeObject(contacts));
        }

        #endregion

        #region Gestion des conversations
        public void LoadConversations(IPlayer client)
        {
            Task.Run(async () =>
            {
                var list = await Database.MongoDB.GetCollectionSafe<Conversation>("conversations").Find(p => p.sender == PhoneNumber).ToListAsync();
                await AltAsync.Do(() =>
                {
                    if (list.Count > 0)
                    {
                        list.OrderByDescending(x => x.lastMessageDate);
                        foreach (Conversation conv in list)
                        {
                            Address _adress = this.AddressBook.Find(p => p.phoneNumber == conv.receiver);

                            conv.receiverName = (_adress != null) ? _adress.contactName : conv.receiver;
                        };
                        client.EmitLocked("ConversationsReturnedV2", JsonConvert.SerializeObject(list));
                    }
                });
            });
        }
        #endregion

        #region Static Methods
        public static void AddPhoneInList(IPlayer client, Phone phone)
        {
                if (GameMode.Instance.PhoneManager.PhoneClientList.ContainsKey(client))
                {
                    if (!GameMode.Instance.PhoneManager.PhoneClientList[client].Exists(p => p?.PhoneNumber == phone?.PhoneNumber))
                    {
                        GameMode.Instance.PhoneManager.PhoneClientList[client].Add(phone);
                    }
                }
                else
                {
                    GameMode.Instance.PhoneManager.PhoneClientList.TryAdd(client, new List<Phone>() { phone });
                }
        }

        public static void RemovePhoneInList(IPlayer client, Phone phonee)
        {
            if (GameMode.Instance.PhoneManager.PhoneClientList.ContainsKey(client))
            {
                if (GameMode.Instance.PhoneManager.PhoneClientList[client].Exists(p => p.PhoneNumber == phonee.PhoneNumber))
                {
                    var id = GameMode.Instance.PhoneManager.PhoneClientList[client].FindIndex(p => p.PhoneNumber == phonee.PhoneNumber);
                    if (id != -1)
                        GameMode.Instance.PhoneManager.PhoneClientList[client].RemoveAt(id);
                }
            }
        }

        public static IPlayer GetClientWithPhoneNumber(string phoneNumber)
        {
            foreach (var phones in GameMode.Instance.PhoneManager.PhoneClientList)
            {
                if (phones.Value.Exists(f => f.PhoneNumber == phoneNumber))
                {
                    return phones.Key;
                }
            }
            return null;
        }

        public static Phone GetPhoneWithPhoneNumber(string phoneNumber)
        {
            foreach (var phones in GameMode.Instance.PhoneManager.PhoneClientList)
            {
                if (phones.Value.Exists(f => f.PhoneNumber == phoneNumber))
                {
                    return phones.Value.Find(p => p.PhoneNumber == phoneNumber);
                }
            }

            return null;
        }
        #endregion
    }
}
