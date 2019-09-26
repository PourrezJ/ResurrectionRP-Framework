using Newtonsoft.Json;
using MongoDB.Bson.Serialization.Attributes;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Async;
using AnimationFlags = ResurrectionRP_Server.Utils.Enums.AnimationFlags;
using RadioModes = ResurrectionRP_Server.Radio.Data.RadioModes;
namespace ResurrectionRP_Server.Radio
{
    public class Radio
    {
        #region JsonIgnore
        [JsonIgnore, BsonIgnore]
        public Entities.Objects.WorldObject Objet;
        [JsonIgnore, BsonIgnore]
        public IPlayer Owner;
        [BsonIgnore]
        public RadioModes Statut = RadioModes.OFF;
        #endregion

        public int CurrentFrequence = 0;
        public double[] Favoris = new double[6]
        {
            55.2,
            72.5,
            86.3,
            93.7,
            102.6,
            123.4
        };

        private int _volume = 10;
        public int Volume
        {
            get => _volume;
            set
            {
                if (value > 10) _volume = 10;
                else if (value < 0) _volume = 0;
                else _volume = value;
            }
        }

        public void OpenRadio(IPlayer client)
        {
            Owner = client;
            if (Favoris == null)
                return;

            Owner.EmitLocked("OpenRadio", JsonConvert.SerializeObject(Favoris), CurrentFrequence, (int)Statut, Volume);
        }

        public void CloseRadio(IPlayer client)
        {
            client.EmitLocked("CloseRadio");
        }

        public Task UseRadio(IPlayer client)
        {
            Owner = client;

            if (Statut == RadioModes.LISTENING)
            {
                Statut = RadioModes.SPEAKING;
                SaltyServer.Voice.SetPlayerSendingOnRadioChannel(client, GetCurrentFrequence().ToString(), true); 

                client.PlayAnimation("random@arrests", "generic_radio_chatter", 4, -8, -1, (AnimationFlags)49);
            }
            return Task.CompletedTask;
        }

        public void DontUse(IPlayer client)
        {
            Owner = client;
            if (Statut == RadioModes.SPEAKING || Statut == RadioModes.LISTENING)
            {
                Statut = RadioModes.LISTENING;
                SaltyServer.Voice.SetPlayerSendingOnRadioChannel(client, GetCurrentFrequence().ToString(), false);

                var ph = client.GetPlayerHandler();

                if (ph == null)
                    return;

                //client.StopAnimation("random@arrests", "generic_radio_chatter"); // TODO
                client.StopAnimation(); // TODO
            }
        }

        public void ShutdownRadio(IPlayer client)
        {
            Statut = RadioModes.OFF;
            SaltyServer.Voice.RemovePlayerRadioChannel(client);
        }

        public void SaveFrequeceRadio(int channel, double frequence)
        { 
            Favoris[channel] = frequence;
        }

        public double GetCurrentFrequence() => Favoris[CurrentFrequence];
    }
}
