using MongoDB.Bson.Serialization.Attributes;
using AltV.Net.Enums;

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
        public override void Init()
        {
            MaxEmployee = 5;
            base.Init();
        }
        #endregion
    }
}
