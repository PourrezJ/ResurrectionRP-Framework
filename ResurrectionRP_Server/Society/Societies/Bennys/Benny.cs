using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Models;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Society.Societies.Bennys
{
    public partial class Bennys : Society
    {
        #region Variables
        private GarageType _garageType = GarageType.Car;
        private IColShape _workZone;
        private IVehicle _vehicleBench;
        #endregion

        #region Constructor
        public Bennys(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Models.Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Events
        private void OnInteractWithPnj(IPlayer client, Entities.Peds.Ped npc)
        {
            if (_vehicleBench != null)
                OpenMainMenu(client);
            else
                client.SendNotificationError("Aucun véhicule devant l'établi.");
        }

        public void OnVehicleEnterWorkzone(IColShape colShape, IVehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists)
                return;

            if (_vehicleBench == null)
                _vehicleBench = vehicle;
        }

        public void OnVehicleLeaveWorkzone(IColShape colShape, IVehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists)
                return;

            if (vehicle == _vehicleBench)
                _vehicleBench = null;
        }
        #endregion

        #region Init
        public override async Task Init()
        {
            Location pnjPos = new Location(new Vector3(-227.6015f, -1327.772f, 30.89038f), new Vector3(0, 0, 239.715f));
            Ped npc = Ped.CreateNPC(PedModel.Benny, pnjPos.Pos, pnjPos.Rot.Z, GameMode.GlobalDimension);
            npc.NpcInteractCallBack = OnInteractWithPnj;

            Vector3 workZonePos = new Vector3(-222.3765f, -1329.64f, 30.46614f);
            _workZone = Alt.CreateColShapeCylinder(new Vector3(workZonePos.X, workZonePos.Y, workZonePos.Z - 1), 10, 5);
            _workZone.SetOnVehicleEnterColShape(OnVehicleEnterWorkzone);
            _workZone.SetOnVehicleLeaveColShape(OnVehicleLeaveWorkzone);
            _garageType = GarageType.Car;

            await base.Init();
        }
        #endregion

        #region Methods
        private void HornPreview(IVehicle vehicle, byte horn)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(10f))
            {
                if (!client.Exists)
                    continue;

                client.EmitLocked("HornPreview", vehicle, horn - 1, true);
            }
        }

        private void HornStop(IVehicle vehicle, byte horn)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(58f))
            {
                if (!client.Exists)
                    continue;

                client.EmitLocked("HornPreview", vehicle, horn - 1, false);
            }
        }
        #endregion
    }
}
