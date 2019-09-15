using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using AltV.Net;
using AltV.Net.Enums;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Business.Barber
{
    public partial class BarberStore : Business
    {
        #region Variables
        [BsonIgnore]
        public Entities.Players.PlayerHandler ClientSelected;

        public double ColorPrice = 50.0;
        #endregion

        #region Constructor
        public BarberStore(string businnessName, Models.Location location, uint blipSprite, int inventoryMax, PedModel pedhash = 0, string owner = null) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner)
        {
        }
        #endregion

        #region Methods
        public override Task Init()
        {
            this.MaxEmployee = 5;
            return base.Init();
        }
        #endregion

        /*
        #region Commands
        [Command("addbarber")]
        public void Addbarber(Client client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player) return;
            var barber = new Barber("Coiffeur", Location.FromVector3(client.Position, client.Rotation), 71, 500, PedHash.FemBarberSFM, client.SocialClubName);
#pragma warning disable CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
            barber.Insert();
#pragma warning restore CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
        }
        #endregion
        */
    }
}
