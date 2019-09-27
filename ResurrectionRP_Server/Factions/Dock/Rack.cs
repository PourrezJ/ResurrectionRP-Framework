using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Streamer.Data;
using ResurrectionRP_Server.Utils;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{
    public class Rack
    {
        #region Fields
        public string RackName;
        public Vector3 RackPos;
        public Location BoxLocation;
        public InventoryBox InventoryBox;

        [BsonIgnore, JsonIgnore]
        public TextLabel TextLabel = null;
        [BsonIgnore, JsonIgnore]
        public IColShape Colshape;
        #endregion

        #region Methods
        public static Rack CreateRack(string rackName, Vector3 rackPos, Location boxLocation, bool empty = false)
        {
            var rack = new Rack()
            {
                RackName = rackName,
                RackPos = rackPos,
                BoxLocation = boxLocation
            };

            if (!empty)
                rack.InventoryBox = InventoryBox.CreateInventoryBox(rackName, boxLocation, new Inventory.Inventory(500, 20));

            rack.Load(empty);

            return rack;
        }

        public void Load(bool empty = false)
        {
            if (GameMode.Instance.IsDebug)
                Entities.Marker.CreateMarker(Entities.MarkerType.VerticalCylinder,RackPos ,new Vector3(2,2,1), Color.FromArgb(80, 255, 255, 255) );
            if (!empty && InventoryBox != null)
            {
                InventoryBox.Spawn(); 
            }
            else if (!empty && InventoryBox == null)
            {
                InventoryBox = InventoryBox.CreateInventoryBox(RackName, BoxLocation, new Inventory.Inventory(500, 20));
            }

            if (InventoryBox.Obj != null)
            {
                InventoryBox.Obj.SetData("RackName", RackName);
            }

            RefreshLabel();
            Colshape = Alt.CreateColShapeCylinder(RackPos, 3, 6);
            Colshape.SetOnPlayerEnterColShape(OnPlayerEnterColShape);
        }

        public void OnPlayerEnterColShape(IColShape colShape, IPlayer client)
        {
            if (client.IsInVehicle)
            {
                IVehicle vehicle = client.Vehicle;
                uint model = vehicle.Model;

                if (model != (uint)VehicleModel.Forklift)
                    return;

                Menu menu = new Menu("ID_Rack", RackName, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
                menu.ItemSelectCallbackAsync = MenuCallBack;

                vehicle.GetData<InventoryBox>("BoxForks", out InventoryBox boxOnForks);

                if (boxOnForks != null && InventoryBox == null)
                    menu.Add(new MenuItem("Déposer le box", $"Déposer le box sur le rack {RackName}", "ID_OutRack", true));
                else if (boxOnForks == null && InventoryBox != null)
                    menu.Add(new MenuItem("Prendre le box", $"Prendre le box du rack {RackName}", "ID_TakeRack", true));

                if (client.GetPlayerHandler()?.StaffRank > 0 && InventoryBox != null && boxOnForks == null)
                    menu.Add(new MenuItem("~r~Retirer le box", $"Retirer le box {RackName}", "ID_Destroy", true));

                Task.Run(async () => { await MenuManager.OpenMenu(client, menu); }).Wait();
            }
        }

        private async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "ID_Rack")
            {
                var vehicle = await client.GetVehicleAsync();

                if (menuItem.Id == "ID_TakeRack")
                {
                    InventoryBox.Obj.SetAttachToEntity(vehicle, "forks_attach", new Vector3(), new Vector3()); 
                    vehicle.SetData("BoxForks", InventoryBox);
                    InventoryBox = null;
                    //client.DisplayHelp("Transport d'une box", 60000);
                }
                else if (menuItem.Id == "ID_OutRack")
                {
                    InventoryBox inventoryBox = null;

                    if (vehicle.GetData("BoxForks", out inventoryBox))
                    {
                        if (inventoryBox == null)
                            return;

                        inventoryBox.Obj.Destroy();
                        inventoryBox.ID = RackName;
                        InventoryBox = inventoryBox;
                        InventoryBox.Location = new Location(new Vector3(this.BoxLocation.Pos.X, this.BoxLocation.Pos.Y, this.BoxLocation.Pos.Z - 1), this.BoxLocation.Rot);                       
                        
                        InventoryBox.Spawn();
                        vehicle.ResetData("BoxForks");
                    }
                }
                else if (menuItem.Id == "ID_Destroy")
                {
                    InventoryBox.Destruct();
                    InventoryBox.Inventory = null;
                    InventoryBox = null;
                }

                await GameMode.Instance.FactionManager.Dock.UpdateDatabase();
                await MenuManager.CloseMenu(client);
                RefreshLabel();
            }
        }

        public void RefreshLabel()
        {
            string str = $"{RackName}\n";
            str += (InventoryBox != null) ? $"{InventoryBox.Inventory.CurrentSize()} : {InventoryBox.Inventory.MaxSize}" : "Vide";

            if (TextLabel != null)
                GameMode.Instance.Streamer.UpdateEntityTextLabel(TextLabel.id, str);
            else
                TextLabel = GameMode.Instance.Streamer.AddEntityTextLabel(str, BoxLocation.Pos, 1, 255, 255, 255, 160);
        }
        #endregion
    }
}
