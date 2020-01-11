using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net;
using ResurrectionRP_Server.Colshape;
using Newtonsoft.Json;

namespace ResurrectionRP_Server.DrivingSchool
{
    public class Exam
    {
        #region Fields
        public IPlayer Player;
        public Entities.Vehicles.VehicleHandler Vehicle;

        public int CurrentCheckpoint = -1;
        public List<Ride> Trajectoire;

        public ICheckpoint checkpoint;
        public IColshape colshape = null;

        public int Avert = 0;
        public DrivingSchool School = null;

        #endregion

        #region Constructor
        public Exam(IPlayer client, Entities.Vehicles.VehicleHandler vehicle, DrivingSchool school)
        {
            Player = client;
            Vehicle = vehicle;
            School = school;

            Trajectoire = school.RidePoints;

            client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_ANTONIA, school.SchoolName, "Secrétaire", "La voiture vous attend sur le parking.");

            Alt.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
        }
        #endregion

        #region Methods
        private void Colshape_OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            if (colshape.GetData("DrivingSchool_ID", out int ID) && ID != CurrentCheckpoint)
                return;

            colshape.OnPlayerEnterColshape -= Colshape_OnPlayerEnterColshape;
            Trajectoire[CurrentCheckpoint].Colshape.OnPlayerEnterColshape += Colshape_OnPlayerEnterColshape;

            NextTraj();
        }

        public void NextTraj()
        {
            CurrentCheckpoint++;
            if (CurrentCheckpoint == Trajectoire.Count )
            {
                Player.Emit("DriveSchool_CreateCP");
                End();
                return;
            }

            Player.Emit("DriveSchool_CreateCP", JsonConvert.SerializeObject( this.Trajectoire[this.CurrentCheckpoint].Position), this.Trajectoire[this.CurrentCheckpoint].Speed);

        }
        /*
        private Task VehicleChecker(IPlayer client, object[] args)
        {
            if ((long)args[0] > Trajectoire[CurrentCheckpoint].Speed)
            {
                client.SendNotificationError("Votre vitesse est bien trop excessive ! Ralentissez bon sang !");
                avert++;
            }

            return Task.CompletedTask;
        }*/

        private void OnPlayerEnterVehicle(IVehicle vehicle, IPlayer client, byte seat)
        {
            if (seat != 1)
                return;
            if (vehicle != this.Vehicle)
                return;
            client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_ANDREAS, "Auto-Ecole", "Information", "Ok! Maintenant allumer le moteur ~r~(F3) ~w~et rendez-vous au prochain point.");
            NextTraj();
        }

        public void End()
        {
            Player.GetPlayerHandler()?.RemoveKey(Vehicle);
            if (Vehicle != null && Vehicle.Exists)
                Task.Run(async () => await Vehicle.DeleteAsync());

        }
        #endregion
    }
}
