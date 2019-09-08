using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Numerics;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Enums;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server.Jobs
{

    public class TrashZone
    {
        public string NameZone;
        public Vector3 ZonePosition;
        public List<Vector3> TrashList = new List<Vector3>();

        public TrashZone(string NameZone, Vector3 ZonePosition)
        {
            this.NameZone = NameZone;
            this.ZonePosition = ZonePosition;
        }
    }

    public class DustMan : Jobs
    {
        #region Public Static Variables
        public ConcurrentDictionary<DustManManager, VehicleHandler> TrashVehiclesList = new ConcurrentDictionary<DustManManager, VehicleHandler>();
        public List<TrashZone> TrashZoneList = new List<TrashZone>();
        #endregion

        #region Private Variables
        private Location _spawn = new Location(new Vector3(-316.7995f, -1537.94f, 27.36676f), new Vector3(0.1620936f, 0.8791512f, 351.5811f));
        private Vector3 _depotZone = new Vector3(-348.8411f, -1558.844f, 24.22907f);
        private int _price = 250;
        #endregion

        public DustMan()
        {
            Name = "~r~[JOB] ~w~Éboueur ";
            ServicePos = new Vector3(-321.3953f, -1545.846f, 31.01991f);
            BlipSprite = 318;
            BlipColor = 0;
            VehicleSpawnLocation = new Location(new Vector3(-316.7995f, -1537.94f, 27.36676f), new Vector3(0.1620936f, -40, 0));
            VehicleSpawnHash = VehicleModel.Trash;
        }

        public override async Task Load()
        {
            #region ColShape

            IColShape DepotColshape = Alt.CreateColShapeCylinder(_depotZone, 3f, 3f);
            DepotColshape.SetData("Jobs", "DustMan");
            #endregion

            #region Zone making
            
            TrashZone _debug = new TrashZone("Debug Zone", new Vector3(-317.1852f, -1519.029f, 27.55757f));
            _debug.TrashList = new List<Vector3>()
            {
               new Vector3(-317.1852f, -1519.029f, 27.55757f),
               new Vector3(-317.1852f, -1516.029f, 27.55757f)
            };
            TrashZoneList.Add(_debug);
            

            TrashZone _groove = new TrashZone("Groove Street", new Vector3(94.61163f, -1921.357f, 20.788f));
            _groove.TrashList = new List<Vector3>()
            {
               new Vector3(94.61163f, -1921.357f, 20.788f),
               new Vector3(104.7821f, -1925.48f, 20.78789f),
               new Vector3(114.8322f, -1930.07f, 20.79043f),
               new Vector3(117.4339f, -1935.138f, 20.75762f),
               new Vector3(111.6096f, -1951.643f, 20.7954f),
               new Vector3(95.93438f, -1951.006f, 20.78847f),
               new Vector3(76.45612f, -1927.3f, 20.89648f),
               new Vector3(59.65182f, -1911.886f, 21.64377f),
               new Vector3(41.28979f, -1897.743f, 21.90289f)
            };
            TrashZoneList.Add(_groove);

            TrashZone _innoncence = new TrashZone("Innoncence Boulevard", new Vector3(1165.308f, -1654.442f, 36.7879f));
            _innoncence.TrashList = new List<Vector3>()
            {
               new Vector3(1165.308f, -1654.442f, 36.7879f),
               new Vector3(1171.742f, -1645.593f, 36.81382f),
               new Vector3(1190.462f, -1633.959f, 43.10063f),
               new Vector3(1219.432f, -1615.723f, 49.15627f),
               new Vector3(1236.925f, -1604.32f, 52.38581f),
               new Vector3(1251.547f, -1592.5f, 53.37238f),
               new Vector3(1280.277f, -1589.276f, 52.00247f),
               new Vector3(1252.644f, -1609.278f, 53.28855f),
               new Vector3(1237.399f, -1620.27f, 51.86275f),
               new Vector3(1206.994f, -1637.096f, 46.0007f),
               new Vector3(1190.669f, -1648.375f, 41.48578f)
            };
            TrashZoneList.Add(_innoncence);

            TrashZone _vinewood = new TrashZone("Vinewood", new Vector3(214.3203f, 619.3665f, 187.4744f));
            _vinewood.TrashList = new List<Vector3>()
            {
               new Vector3(214.3203f, 619.3665f, 187.4744f),
               new Vector3(147.656f, 568.6603f, 183.8577f),
               new Vector3(114.1055f, 566.7968f, 182.9887f),
               new Vector3(90.49673f, 563.0635f, 182.5776f),
               new Vector3(46.66833f, 563.047f, 180.09f),
               new Vector3(12.31671f, 543.1911f, 175.9095f),
               new Vector3(-70.58949f, 497.2914f, 144.3527f),
               new Vector3(-3.413553f, 470.3774f, 145.8072f),
               new Vector3(66.86223f, 459.1354f, 146.853f),
               new Vector3(119.0361f, 492.1701f, 147.1561f)
            };
            TrashZoneList.Add(_vinewood);

            TrashZone _puerta = new TrashZone("La Puerta", new Vector3(-1077.472f, -1550.366f, 4.630476f));
            _puerta.TrashList = new List<Vector3>()
            {
               new Vector3(-1077.472f, -1550.366f, 4.630476f),
               new Vector3(-1088.552f, -1597.26f, 4.411292f),
               new Vector3(-1057.986f, -1633.688f, 4.403187f),
               new Vector3(-1072.951f, -1613.682f, 4.388186f),
               new Vector3(-1119.801f, -1558.509f, 4.382564f),
               new Vector3(-1121.038f, -1515.782f, 4.38515f),
               new Vector3(-1084.702f, -1555.698f, 4.4751f),
               new Vector3(-1059.598f, -1541.258f, 5.016018f),
               new Vector3(-1071.596f, -1549.002f, 4.753887f)
            };
            TrashZoneList.Add(_puerta);

            #endregion

            AltAsync.OnServer("DustMan_Callback", DustMan_Callback);

            Utils.Utils.Delay((int)TimeSpan.FromMinutes(2).TotalMilliseconds, false, async () =>
            {
                if (VehicleSpawnLocation != null && VehiclesManager.IsVehicleInSpawn(VehicleSpawnLocation.Pos))
                {
                    //var vehs = await MP.Vehicles.GetInRangeAsync(VehicleSpawnLocation.Pos, 4, MP.GlobalDimension);
                    var vehs = VehicleSpawnLocation.Pos.GetVehiclesInRange(4);

                    if (vehs.Count > 0)
                    {
                        foreach (var veh in vehs)
                        {
                            await AltAsync.Do(() =>
                            {
                                if (!veh.Exists)
                                    return;

                                if (veh.Driver == null)
                                {
                                    veh.Remove();
                                }
                            });
                        }
                    }
                }
            });

            await base.Load();
        }

        public override async Task OnPlayerEnterVehicleJob(IVehicle vehicle, IPlayer client, byte seat)
        {
            if (await IsInService(client) && seat == 1)
            {
                if (!client.Exists || !vehicle.Exists)
                    return;

                foreach (KeyValuePair<DustManManager, VehicleHandler> item in TrashVehiclesList)
                {
                    if (item.Key.DustManClient == client)
                        return;
                }

                //TrashZoneList[Utils.Utils.RandomNumber(TrashZoneList.Count)]
                DustManManager DustManmanager = new DustManManager(client, TrashZoneList[0], _depotZone);
                TrashVehiclesList.TryAdd(DustManmanager, vehicle.GetVehicleHandler());
                //await client.EmitAsync("Jobs_Dustman", "Init", vehicle.Id, TrashZoneList[Utils.Utils.RandomNumber(TrashZoneList.Count)], _depotZone);
            }
        }

        private async Task DustMan_Callback(object[] args)
        {
            IPlayer client = args[0] as IPlayer;
            if (!client.Exists)
                return;
/*
            PlayerHandler ph = PlayerManager.GetPlayerByClient(client); TODO
            if (ph != null)
            {
                await client.SendNotificationSuccess($"Vous avez gagné ${_price}");
                await ph.AddMoney(_price);

                Menu menu = new Menu("ID_DustMan", "Déchetterie", "Que voulez-vous faire?", 0, 0, Menu.MenuAnchor.MiddleRight, true, true, false);
                menu.Callback = DustManCallBack;
                menu.Add(new MenuItem("~g~Prendre un autre quartier", "", "ID_Quartier", true));
                menu.Add(new MenuItem("~r~Fin de mission", "", "ID_End", true));

                await menu.OpenMenu(arg.Player);
            }*/
        }

/*        private async Task DustManCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex) TODO
        {
            switch (menuItem.Id)
            {
                case "ID_Quartier":
                    await client.CallAsync("Jobs_Dustman", "ReInit", (await GetJobVehiclePlayer(client)).Vehicle.Id, TrashZoneList[Utils.RandomNumber(TrashZoneList.Count)], _depotZone);
                    break;

                case "ID_End":
                    await QuitterService(client);
                    break;
            }
            await MenuManager.CloseMenu(client);
        }
*/
        public override async Task QuitterService(IPlayer client)
        {
            await client.EmitAsync("Jobs_Dustman", "End");
            await base.QuitterService(client);
        }
    }
}
