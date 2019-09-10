using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net;
using AltV.Net.Enums;
using ResurrectionRP_Server.EventHandlers;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.DrivingSchool
{
    public class DrivingSchool
    {
        #region Fields and properties
        public byte Id { get; }
        private Vector3 _position;
        private Location _spawnVeh;
        private LicenseType _schoolType;
        private int _price;
        private List<Ride> _circuit;
        private VehicleModel _vehicleModel;
        private int _markerId;
        private Entities.Blips.Blips _blipId;
        private IColShape _colshape;
        private ConcurrentDictionary<string, Exam> _clientsInExamen = new ConcurrentDictionary<string, Exam>();
        private List<IPlayer> _clientsInEchec = new List<IPlayer>();
        #endregion

        #region Constructor
        public DrivingSchool(byte id, Vector3 pos, Models.Location spawnVeh, Models.LicenseType licenseType, int price, List<Ride> circuit, VehicleModel vehicleSchool)
        {
            Id = id;
            _position = pos;
            _spawnVeh = spawnVeh;
            _schoolType = licenseType;
            _price = price;
            _circuit = circuit;
            _vehicleModel = vehicleSchool;
        }
        #endregion

        #region Private methods
        private async Task BeginDrivingExamen(IPlayer client)
        {
            var veh = await Entities.Vehicles.VehiclesManager.SpawnVehicle(client.GetSocialClub(), (uint)_vehicleModel, _spawnVeh.Pos, _spawnVeh.Rot, plate: "School", spawnVeh: true, locked: false);
            Exam Examitem = new Exam(client, veh, _circuit, this.Id);
            //Examitem.endExam = End();
            _clientsInExamen.GetOrAdd(client.GetSocialClub(), Examitem);
            client.GetPlayerHandler()?.AddKey(veh, "Auto école");
            //await client.EmitAsync("BeginDrivingExamen", veh.Vehicle.Id, Circuit, ID);

        }
        #endregion

        #region Public methods
        public void Load()
        {
            _markerId = GameMode.Instance.Streamer.AddEntityMarker(Streamer.Data.MarkerType.MarkerTypeCarSymbol, _position, new Vector3(1f, 1f, 1f), 150, 5, 168);
            _colshape = Alt.CreateColShapeCylinder(_position - new Vector3(0, 0, 1), 2, 3);
            //await MP.Markers.NewAsync(29, Position, new Vector3(), new Vector3(), 1f, Color.FromArgb(150, 5, 168, 0), true);
            //IColshape colshape = await MP.Colshapes.NewTubeAsync(Position, 1f, 1f);
            //colshape.SetSharedData("DrivingSchool", ID);
            Events.OnPlayerEnterColShape += OnPlayerEnterColshape;
            Events.OnPlayerLeaveColShape += OnPlayerLeaveColshape;
            string schoolName = string.Empty;

            switch (_schoolType)
            {
                case LicenseType.Car:
                    schoolName = "Auto-École";
                    break;

                case LicenseType.Air:
                    schoolName = "École de pilotage";
                    break;

                case LicenseType.Bike:
                    schoolName = "Moto-École";
                    break;

                case LicenseType.Boat:
                    schoolName = "Bateau-École";
                    break;
            }

            _blipId    = Entities.Blips.BlipsManager.CreateBlip(schoolName, _position, 69, 535, 0.5f);
            //IBlip blip = await MP.Blips.NewAsync(535, Position, 0.5f, 69, schoolName, 255, 10, true, 0);
        }

        public bool ClientIsInExamen(IPlayer client) =>
            _clientsInExamen.ContainsKey(client.GetSocialClub());

        public async Task End(IPlayer client, int advert)
        {
            if (_clientsInExamen.TryRemove(client.GetSocialClub(), out Exam exam))
            {
                await exam.End();

                if (advert >= 5)
                    //await client.SendNotificationPicture($"~r~ Vous avez échoué votre examen avec {advert} fautes.", Utils.Enums.CharPicture.CHAR_ANDREAS, false, 0, "Auto-école", "Examinateur");
                    client.SendNotificationError($"~r~ Vous avez échoué votre examen avec {advert} fautes.");
                else
                {
                    //await client.SendNotificationPicture($"~g~ Vous réussi votre examen avec {advert} faute(s).", Utils.Enums.CharPicture.CHAR_ANDREAS, false, 0, "Auto-école", "Examinateur");
                    client.GetPlayerHandler()?.Licenses.Add(new Models.License(_schoolType));
                    client.SendNotificationSuccess($"~g~ Vous réussi votre examen avec {advert} faute(s).");
                }
            }
        }

        public async Task Cancel(IPlayer client)
        {
            if (_clientsInExamen.TryRemove(client.GetSocialClub(), out Exam exam))
                await exam.End();
        }
        #endregion

        #region Menu
        public async Task OnPlayerEnterColshape(IColShape colShape, IPlayer client)
        {
            if (colShape != _colshape)
                return;

            await OpenMenuDrivingSchool(client);
        }

        public async Task OnPlayerLeaveColshape(IColShape colShape, IPlayer client)
        {
            if (colShape != _colshape)
                return;

            await MenuManager.CloseMenu(client);
        }

        public async Task OpenMenuDrivingSchool(IPlayer client)
        {

            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();

            if (ph != null) 
            {
                Menu drivingschoolmenu = new Menu("ID_DrivingShoolMenu", "Auto-école", "", 0, 0, Menu.MenuAnchor.MiddleRight, backCloseMenu: true);
                drivingschoolmenu.ItemSelectCallback = DrivingMenuCallBack;

                if (ClientIsInExamen(client))
                {
                    client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_ANTONIA, "Auto-école", "Secrétaire", "Vous êtes déjà en examen! Vous voulez abandonner et rester où est la voiture!?");
                    drivingschoolmenu.Add(new MenuItem("Abandonner l'examen.", "Permet de recommencer l'examen.", "ID_Cancel", true));
                }
                else
                {
                    if (!ph.HasLicense(Models.LicenseType.Car) && _schoolType == Models.LicenseType.Car)
                        drivingschoolmenu.Add(new MenuItem("Permis Voiture", $"Passer le permis voiture pour la somme de ~r~${_price} ~w~prélevée au début de l'examen.", "ID_Car", true, rightLabel: $"${_price}"));
                    else
                    {
                        client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_ANTONIA, "Auto-école", "Secrétaire", "MAIS VOUS AVEZ DEJA VOTRE PERMIS ?!");
                        return;
                    }
                }

                await drivingschoolmenu.OpenMenu(client);
            }
        }

        private async Task DrivingMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            await MenuManager.CloseMenu(client);
            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (menuItem.Id == "ID_Cancel")
            {
                if (ClientIsInExamen(client))
                {
                    await Cancel(client);
                    await client.EmitAsync("EndDrivingExamen");

                    if (ph != null)
                        await ph.AddMoney(_price);
                }
            }
            else if (menuItem.Id == "ID_Car")
            {
                if (!Entities.Vehicles.VehiclesManager.IsVehicleInSpawn(_spawnVeh, 2))
                {
                    if (await ph.HasMoney(_price))
                    {
                        client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_ANDREAS, "Auto-école", "Examinateur", "Votre examen de conduite commence! Vous avez le droit à ~r~5 erreurs~w~.");
                        await BeginDrivingExamen(client);
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
                }
                else
                    client.SendNotificationError("Un véhicule gêne la sorti du garage.");
            }
        }
        #endregion
    }
}
