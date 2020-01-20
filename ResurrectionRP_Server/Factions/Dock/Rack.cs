using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Streamer.Data;
using ResurrectionRP_Server.Utils;
using System.Drawing;
using System.Numerics;

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
        public IColshape Colshape;
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
            if (GameMode.IsDebug)
                Entities.Marker.CreateMarker(Entities.MarkerType.VerticalCylinder, RackPos, new Vector3(2, 2, 1), Color.FromArgb(80, 255, 255, 255));
            if (!empty && InventoryBox != null)
            {
                InventoryBox.Spawn();
            }
            else if (!empty && InventoryBox == null)
            {
                InventoryBox = InventoryBox.CreateInventoryBox(RackName, BoxLocation, new Inventory.Inventory(500, 20));
            }
            if (InventoryBox != null && InventoryBox.Obj != null && RackName != null)
            {
                InventoryBox.Obj.SetData("RackName", RackName);
            }
            RefreshLabel();
            Colshape = ColshapeManager.CreateCylinderColshape(RackPos, 3, 6);
            Colshape.OnPlayerEnterColshape += OnPlayerEnterColShape;
        }

        public void OnPlayerEnterColShape(IColshape colshape, IPlayer client)
        {
            if (client.IsInVehicle)
            {
                IVehicle vehicle = client.Vehicle;
                uint model = vehicle.Model;

                if (model != (uint)VehicleModel.Forklift)
                    return;

                Menu menu = new Menu("ID_Rack", RackName, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
                menu.ItemSelectCallback = MenuCallBack;

                vehicle.GetData<InventoryBox>("BoxForks", out InventoryBox boxOnForks);

                if (boxOnForks != null && InventoryBox == null)
                    menu.Add(new MenuItem("Déposer le box", $"Déposer le box sur le rack {RackName}", "ID_OutRack", true));
                else if (boxOnForks == null && InventoryBox != null)
                    menu.Add(new MenuItem("Prendre le box", $"Prendre le box du rack {RackName}", "ID_TakeRack", true));

                if (client.GetPlayerHandler()?.StaffRank > 0 && InventoryBox != null && boxOnForks == null)
                    menu.Add(new MenuItem("~r~Retirer le box", $"Retirer le box {RackName}", "ID_Destroy", true));

                MenuManager.OpenMenu(client, menu);
            }
        }

        private void MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "ID_Rack")
            {
                IVehicle vehicle = client.Vehicle;

                if (menuItem.Id == "ID_TakeRack")
                {
                    InventoryBox.Obj.AttachEntity(vehicle, "forks_attach", new Vector3(), new Vector3()); 
                    vehicle.SetData("BoxForks", InventoryBox);
                    InventoryBox = null;
                    //client.DisplayHelp("Transport d'une box", 60000);
                }
                else if (menuItem.Id == "ID_OutRack")
                {
                    if (vehicle.GetData("BoxForks", out InventoryBox inventoryBox))
                    {
                        if (inventoryBox == null)
                            return;

                        inventoryBox.ID = RackName;
                        InventoryBox = inventoryBox;
                        InventoryBox.Obj.DetachEntity();
                        InventoryBox.Location = new Location(new Vector3(BoxLocation.Pos.X, BoxLocation.Pos.Y, BoxLocation.Pos.Z - 1), BoxLocation.Rot);                       
                        
                        vehicle.ResetData("BoxForks");
                    }
                }
                else if (menuItem.Id == "ID_Destroy")
                {
                    InventoryBox.Destruct();
                    InventoryBox.Inventory = null;
                    InventoryBox = null;
                }

                FactionManager.Dock.UpdateInBackground();
                MenuManager.CloseMenu(client);
                RefreshLabel();
            }
        }

        public void RefreshLabel()
        {
            string str = $"{RackName}\n";
            str += (InventoryBox != null) ? $"{InventoryBox.Inventory.CurrentSize()} : {InventoryBox.Inventory.MaxSize}" : "Vide";

            if (TextLabel != null)
                TextLabel.Text = str;
            else
                TextLabel = TextLabel.CreateTextLabel(str, BoxLocation.Pos, Color.FromArgb(168, 255, 255, 255), 1);
        }
        #endregion
    }
}
