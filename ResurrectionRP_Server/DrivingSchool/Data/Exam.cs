using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using System.Numerics;
using ResurrectionRP_Server.Colshape;
using Newtonsoft.Json;

namespace ResurrectionRP_Server.DrivingSchool
{
    public class Exam : IDisposable
    {
        #region Fields
        public IPlayer Player;
        public Entities.Vehicles.VehicleHandler Vehicle;

        public int CurrentCheckpoint = -1;
        public List<Ride> Trajectoire;

        public ICheckpoint checkpoint;
        public IColshape colshape = null;

        public int avert = 0;
        public int id = 0;
        #endregion

        #region Constructor
        public Exam(IPlayer client, Entities.Vehicles.VehicleHandler vehicle, List<Ride> traj, int SchooldId)
        {
            Player = client;
            Vehicle = vehicle;
            this.id = SchooldId;
            this.Trajectoire = traj;

            AltAsync.OnPlayerEnterVehicle += this.OnEnterVehicle;
            
        }
        #endregion

        #region Methods
        private void Colshape_OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            lock (colshape)
            {
                this.NextTraj();
            }
        }

        private void NextTraj()
        {
            if (this.colshape != null)
                ColshapeManager.DeleteColshape(this.colshape);


            this.CurrentCheckpoint++;
            if(this.CurrentCheckpoint == this.Trajectoire.Count )
            {
                this.Player.Emit("DriveSchool_CreateCP");
                endTraj();
                return;
            }

            this.colshape = ColshapeManager.CreateCylinderColshape(this.Trajectoire[this.CurrentCheckpoint].Position, 3, 6);
            this.colshape.OnPlayerEnterColshape += Colshape_OnPlayerEnterColshape;
            this.Player.Emit("DriveSchool_CreateCP", JsonConvert.SerializeObject( this.Trajectoire[this.CurrentCheckpoint].Position));

        }


        private void endTraj()
        {
            this.End();
        }

        private Task VehicleChecker(IPlayer client, object[] args)
        {
            if ((long)args[0] > Trajectoire[CurrentCheckpoint].Speed)
            {
                client.SendNotificationError("Votre vitesse est bien trop excessive ! Ralentissez bon sang !");
                avert++;
            }

            return Task.CompletedTask;
        }

        private async Task OnEnterVehicle(IVehicle vehicle, IPlayer client, byte seat)
        {
            if (seat != 1)
                return;
            if (vehicle != this.Vehicle.Vehicle)
                return;
            client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_ANDREAS, "Auto-Ecole", "Information", "Ok! Maintenant allumer le moteur ~r~(F3) ~w~et rendez-vous au prochain point.");
            NextTraj();
        }
        public async Task End()
        {
            Player.GetPlayerHandler()?.RemoveKey(Vehicle);
            await Vehicle.DeleteAsync();
            Dispose();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
