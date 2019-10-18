using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class MiningPoint
    {
        public Vector3 Position;
        public Item ReturnedItem;
        public float DoubleLuck = 0.0f; // Un randomint sera fait pour savoir si on veut 2 minerais
        public IColshape Colshape;

        public MiningPoint(Vector3 position, Item returnedItem, float doubleLuck)
        {
            Position = position;
            ReturnedItem = returnedItem;
            DoubleLuck = doubleLuck;

            Colshape = ColshapeManager.CreateCylinderColshape(Position - new Vector3(0, 0, 1), 2, 3);
            Colshape.OnPlayerEnterColshape += Colshape_OnPlayerEnterColshape;

        }

        private void Colshape_OnPlayerEnterColshape(IColshape colshape, AltV.Net.Elements.Entities.IPlayer client)
        {
            PlayerHandler _client = client.GetPlayerHandler();
            Inventory.Inventory inventory = _client.HasItemInAnyInventory(ItemID.Pioche);
            if (inventory != null)
                client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour commencer à miner", 5000);
            else
                client.DisplayHelp("Vopus devez avoir une pioche pour miner", 10000);
            ≤
            throw new System.NotImplementedException();
        }
    }
    class Cuivre : Farm
    {
        public List<MiningPoint> MinagePoints = new List<MiningPoint> {

            new MiningPoint( new Vector3(2938.51f, 2770.522f, 38.47805f), Inventory.Inventory.ItemByID(ItemID.MineraiCuivre), 0  ),
            new MiningPoint( new Vector3(2951.971f, 2769.79f, 38.42247f), Inventory.Inventory.ItemByID(ItemID.MineraiCuivre), 0  ),
            new MiningPoint( new Vector3(2970.657f, 2776.359f, 37.7042f), Inventory.Inventory.ItemByID(ItemID.MineraiCuivre), 0  ),
            new MiningPoint( new Vector3(2943.139f, 2819.934f, 42.5276f), Inventory.Inventory.ItemByID(ItemID.MineraiCuivre), 0  ),
            new MiningPoint( new Vector3(2997.679f, 2758.425f, 42.34139f), Inventory.Inventory.ItemByID(ItemID.MineraiCuivre), 0  ),
            new MiningPoint( new Vector3(2946.29f, 2733.697f, 44.75487f), Inventory.Inventory.ItemByID(ItemID.MineraiCuivre), 0  ),

        };
        public Cuivre()
        {
            Harvest_Name = "Mine de cuivre";
            Process_Name = "Fonderie de cuivre";
            Selling_Name = "Revendeur de cuivre";

            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_BlipPosition = new Vector3(2684.724f, 2866.268f, 33f);
            Harvest_Position.Add(new Vector3(2684.724f, 2866.268f, 33f));

            Process_PosRot = new Location(new Vector3(741.797f, -972.2991f, 24.50507f), new Vector3(0f, 0f, -94.82858f));
            Selling_PosRot = new Location(new Vector3(606, -3073.102f, 6.06f), new Vector3(0, 0, -11.52882f));

            Harvest_Range = 50f;

            Process_PedHash = AltV.Net.Enums.PedModel.Dockwork01SMY;
            Selling_PedHash = AltV.Net.Enums.PedModel.Cntrybar01SMM;

            BlipColor = Entities.Blips.BlipColor.Complexion;

            ItemIDBrute = ItemID.MineraiCuivre;
            ItemIDProcess = ItemID.Cuivre;
            ItemPrice = 92;
        }
    }
}
