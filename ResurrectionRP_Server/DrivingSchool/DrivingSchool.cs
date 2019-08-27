using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net;
using AltV.Net.Enums;

namespace ResurrectionRP_Server.DrivingSchool
{

    public class DrivingSchool
    {
        #region Variables
        public byte ID;
        public Vector3 Position;
        public Models.Location SpawnVeh;
        public Models.LicenseType SchoolType;
        public int Price;
        public List<Trajet> Circuit;
        public VehicleModel VehicleSchool;
        public int MarkerId;
        public int BlipId;
        public IColShape colshape;

        private Dictionary<string, Examen> clientsInExamenList = new Dictionary<string, Examen>();
        private List<IPlayer> clientsInEchec = new List<IPlayer>();
        #endregion

        #region Constructor
        public DrivingSchool(byte id, Vector3 pos, Models.Location spawnVeh, Models.LicenseType licenseType, int price, List<Trajet> circuit, VehicleModel vehicleSchool)
        {
            ID = id;
            Position = pos;
            SpawnVeh = spawnVeh;
            SchoolType = licenseType;
            Price = price;
            Circuit = circuit;
            VehicleSchool = vehicleSchool;
        }
        #endregion

        #region Methods
        public async Task Load()
        {
            this.MarkerId = GameMode.Instance.Streamer.addEntityMarker(Streamer.Data.MarkerType.MarkerTypeCarSymbol, Position, new Vector3(1f, 1f, 1f), 150, 5, 168);
            colshape = Alt.CreateColShapeCylinder(Position - new Vector3(0,0,1), 1, 3);
            colshape.SetData("DrivingSchool", ID);
            //await MP.Markers.NewAsync(29, Position, new Vector3(), new Vector3(), 1f, Color.FromArgb(150, 5, 168, 0), true);
            //IColshape colshape = await MP.Colshapes.NewTubeAsync(Position, 1f, 1f);
            //colshape.SetSharedData("DrivingSchool", ID);
            Alt.OnColShape += OnEntityColShape;
            string schoolName = "Auto-École";

            switch (SchoolType)
            {
                case Models.LicenseType.Air:
                    schoolName = "École de pilotage";
                    break;

                case Models.LicenseType.Bike:
                    schoolName = "Moto-École";
                    break;

                case Models.LicenseType.Boat:
                    schoolName = "Bateau-École";
                    break;
            }
            this.BlipId = GameMode.Instance.Streamer.addStaticEntityBlip(schoolName, Position, 69, 535, 0.5f);
            //IBlip blip = await MP.Blips.NewAsync(535, Position, 0.5f, 69, schoolName, 255, 10, true, 0);
        }

        public async Task<bool> ClientIsInExamen(IPlayer client) => clientsInExamenList.ContainsKey( client.GetSocialClub());

        public async Task<Examen> GetClientExamen(IPlayer client) => clientsInExamenList[ client.GetSocialClub()];

        private async Task BeginDrivingExamen(IPlayer client)
        {
            var veh = await Entities.Vehicles.VehiclesManager.SpawnVehicle( client.GetSocialClub(), (uint)VehicleSchool, SpawnVeh.Pos, SpawnVeh.Rot, plate: "School", spawnVeh: true, locked: false);
            Examen Examitem = new Examen(client, veh, Circuit, this.ID);
            //Examitem.endExam = End();
            clientsInExamenList.GetOrAdd(client.GetSocialClub(), Examitem);
            Entities.Players.PlayerManager.GetPlayerByClient(client)?.AddKey(veh, "Auto école");
            //await client.EmitAsync("BeginDrivingExamen", veh.Vehicle.Id, Circuit, ID);
            
        }

        public async Task End(IPlayer client, int advert)
        {
            if (await ClientIsInExamen(client))
            {
                Examen exam = await GetClientExamen(client);
                await exam.End();
                clientsInExamenList.Remove(client.GetSocialClub());

                if (advert >= 5)
                    //await client.SendNotificationPicture($"~r~ Vous avez échoué votre examen avec {advert} fautes.", Utils.Enums.CharPicture.CHAR_ANDREAS, false, 0, "Auto-école", "Examinateur");
                    await client.SendNotificationError($"~r~ Vous avez échoué votre examen avec {advert} fautes.");
                else
                {
                    //await client.SendNotificationPicture($"~g~ Vous réussi votre examen avec {advert} faute(s).", Utils.Enums.CharPicture.CHAR_ANDREAS, false, 0, "Auto-école", "Examinateur");
                    await client.SendNotificationSuccess($"~g~ Vous réussi votre examen avec {advert} faute(s).");
                    Entities.Players.PlayerManager.GetPlayerByClient(client)?.Licenses.Add(new Models.License(SchoolType));
                }
            }
        }

        public async Task Cancel(IPlayer client)
        {
            if (await ClientIsInExamen(client))
            {
                Examen exam = await GetClientExamen(client);
                await exam.End();
                clientsInExamenList.Remove(client.GetSocialClub());
            }
        }
        #endregion

        #region Menu
        public void OnEntityColShape(IColShape colShape, IEntity Client, bool state)
        {
            if (!state)
                return;
            if (colShape.Position != this.colshape.Position)
                return;
            colShape.GetData("DrivingSchool", out int ID);
            if (ID == null || ID != this.ID)
                return;
            IPlayer client = Client as IPlayer;
            OpenMenuDrivingSchool(client);
        }
        public async void OpenMenuDrivingSchool(IPlayer client)
        {

            Entities.Players.PlayerHandler ph = Entities.Players.PlayerManager.GetPlayerByClient(client);

            /*            if (ph != null) TODO
                        {
                            Menu drivingschoolmenu = new Menu("ID_DrivingShoolMenu", "Auto-école", "", 0, 0, Menu.MenuAnchor.MiddleRight, backCloseMenu: true);
                            drivingschoolmenu.Callback = DrivingMenuCallBack;

                            if (await ClientIsInExamen(client))
                            {
                                await client.SendNotificationPicture("Vous êtes déjà en examen! Vous voulez abandonner et rester où est la voiture!?", CharPicture.CHAR_ANTONIA, false, 0, "Auto-école", "Secrétaire");
                                drivingschoolmenu.Add(new MenuItem("Abandonner l'examen.", "Permet de recommencer l'examen.", "ID_Cancel", true));
                            }
                            else
                            {
                                if (!ph.HasLicense(LicenseType.Car) && SchoolType == LicenseType.Car)
                                    drivingschoolmenu.Add(new MenuItem("Permis Voiture", $"Passer le permis voiture pour la somme de ~r~${Price} ~w~prélevée au début de l'examen.", "ID_Car", true, rightLabel: $"${Price}"));
                                else
                                {
                                    await client.SendNotificationPicture("MAIS VOUS AVEZ DEJA VOTRE PERMIS ?!", CharPicture.CHAR_ANTONIA, false, 0, "Auto-école", "Secrétaire");
                                    return;
                                }
                            }

                            await drivingschoolmenu.OpenMenu(client);
                        }*/

        }

/*        private async Task DrivingMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex) TODO
        {
            await MenuManager.CloseMenu(client);
            Entities.Players.PlayerHandler ph = Entities.Players.PlayerManager.GetPlayerByClient(client);

            if (ph == null)
                return;

            if (menuItem.Id == "ID_Cancel")
            {
                if (await ClientIsInExamen(client))
                {
                    await Cancel(client);
                    await client.EmitAsync("EndDrivingExamen");

                    if (ph != null)
                        await ph.AddMoney(Price);
                }
            }
            else if (menuItem.Id == "ID_Car")
            {
                if (!await Entities.Vehicles.VehiclesManager.GetVehicleInSpawn(SpawnVeh, 2))
                {
                    if (await ph.HasMoney(Price))
                    {
                        await client.SendNotificationPicture("Votre examen de conduite commence! Vous avez le droit à ~r~5 erreurs~w~.", Utils.Enums.CharPicture.CHAR_ANDREAS, false, 0, "Auto-école", "Examinateur");
                        await BeginDrivingExamen(client);
                    }
                    else
                        await client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
                }
                else
                    await client.SendNotificationError("Un véhicule gêne la sorti du garage.");
            }
        }*/
        #endregion
    }
}
