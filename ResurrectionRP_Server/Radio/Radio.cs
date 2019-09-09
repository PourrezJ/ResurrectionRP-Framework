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
        public Entities.Objects.ObjectHandler Objet;
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

        private int _volume = 5;
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

        public async Task OpenRadio(IPlayer client)
        {
            Owner = client;
            if (Favoris == null)
                return;

            await Owner.EmitAsync("OpenRadio", JsonConvert.SerializeObject(Favoris), CurrentFrequence, (int)Statut);
            
            await Owner.PlayAnimation((await Owner.GetVehicleAsync() != null) ? "cellphone@in_car@ds" : (await Owner.GetModelAsync() == Alt.Hash("mp_f_freemode_01")) ? "cellphone@female" : "cellphone@", "cellphone_text_read_base", 3, -1, -1, (AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.Loop | AnimationFlags.SecondaryTask));
        }

        public async Task HideRadio(IPlayer client)
        {
            await client.EmitAsync("HideRadio");
            await client.PlayAnimation((await Owner.GetVehicleAsync() != null) ? "cellphone@in_car@ds" : (await client.GetModelAsync() == Alt.Hash("mp_f_freemode_01")) ? "cellphone@female" : "cellphone@", "cellphone_text_out", 3, -1, -1, (AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody));
        }

        public async Task UseRadio(IPlayer client)
        {
            Owner = client;

            if (Statut == RadioModes.LISTENING)
            {
                Statut = RadioModes.SPEAKING;

                await SaltyServer.Voice.SetPlayerSendingOnRadioChannel(client, GetCurrentFrequence().ToString(), true); 

                await client.PlayAnimation("random@arrests", "generic_radio_chatter", 4, -8, -1, (AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl));
            }
        }

        public async Task DontUse(IPlayer client)
        {
            Owner = client;
            if (Statut == RadioModes.SPEAKING || Statut == RadioModes.LISTENING)
            {
                Statut = RadioModes.LISTENING;
                await SaltyServer.Voice.SetPlayerSendingOnRadioChannel(client, GetCurrentFrequence().ToString(), false);

                var ph = client.GetPlayerHandler();

                if (ph == null)
                    return;

                //await ph.StopAnimation("random@arrests", "generic_radio_chatter"); // TODO
            }
        }

        public async Task ShutdownRadio(IPlayer client)
        {
            Statut = RadioModes.OFF;
            await SaltyServer.Voice.RemovePlayerRadioChannel(client);
        }

        public void SaveFrequeceRadio(int channel, double frequence)
        { 
            Favoris[channel] = frequence;
        }

        public double GetCurrentFrequence() => Favoris[CurrentFrequence];
    }
}
