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

namespace ResurrectionRP_Server.DrivingSchool
{
    public class Examen : IDisposable
    {
        #region Fields
        public IPlayer PlayerExam;
        public Entities.Vehicles.VehicleHandler VehicleExam;
        public int CurrentCheckpoint = -1;
        public List<Trajet> Trajectoire;
        public ICheckpoint checkpoint;
        public IColShape colshape = null;
        public int avert = 0;
        public int id = 0;


        #endregion

        #region Constructor
        public Examen(IPlayer client, Entities.Vehicles.VehicleHandler vehicle, List<Trajet> traj, int SchooldId)
        {
            PlayerExam = client;
            VehicleExam = vehicle;
            this.id = SchooldId;
            this.Trajectoire = traj;

            Alt.OnPlayerEnterVehicle += this.OnEnterVehicle;
            
        }
        #endregion

        #region Methods
        private async void onColShape(IColShape colShape, IEntity entity, bool state)
        {
            if (!state || entity.Type != BaseObjectType.Player)
                return;
            if (colShape.Position != this.colshape.Position)
                return;
            this.colshape.GetData("DrivingSchool", out bool result);
            if (result != true)
                return;
            this.NextTraj();
        }

        private async void NextTraj()
        {
            if(this.checkpoint != null)
                Alt.RemoveCheckpoint(this.checkpoint);
            if (this.colshape != null)
                Alt.RemoveColShape(this.colshape);


            this.CurrentCheckpoint++;
            if(this.CurrentCheckpoint == this.Trajectoire.Count )
            {
                this.endTraj();
                return;
            }

            this.checkpoint = Alt.CreateCheckpoint(this.PlayerExam, (byte)CheckpointType.Cyclinder2, this.Trajectoire[this.CurrentCheckpoint].Position, 3, 6, new Rgba(255, 255, 255, 128));
            this.colshape = Alt.CreateColShapeCylinder(this.Trajectoire[this.CurrentCheckpoint].Position, 3, 6);
            await this.PlayerExam.setWaypoint(this.Trajectoire[this.CurrentCheckpoint].Position);
            this.colshape.SetData("DrivingSchool", true);


        }

        private async void endTraj()
        {
            await this.PlayerExam.EmitAsync("DrivingSchool_End");
            await this.End();
            Alt.Emit("DrivingSchool_End", this.PlayerExam, this.id, this.avert);
        }

        private async void VehicleChecker(IPlayer client, object[] args)
        {
            Alt.Server.LogColored("~grey~DrivingSchool ~w~| Trigger the checker | " + args[0]);
            if ( (Int64) args[0] > this.Trajectoire[this.CurrentCheckpoint].Speed)
            {
                await client.SendNotificationError("Votre vitesse est bien trop excessive ! Ralentissez bon sang !");
                this.avert++;
            }
        }

        private async void OnEnterVehicle(IVehicle vehicle, IPlayer client, byte seat)
        {
            if (seat != 1)
                return;
            if (vehicle != this.VehicleExam.Vehicle)
                return;
            //await client.SendNotificationPicture("Ok! Maintenant allumer le moteur ~r~(F3) ~w~et rendez-vous au prochain point.", "CHAR_ANDREAS", false, 0, "CHAR_ANDREAS",  "Auto-école");
            await client.SendNotificationSuccess("Génial, l'instructeur vous fait signe de démarrer le moteur (F3), et de rejoindre le point blanc");
            await client.EmitAsync("DrivingSchool_Start");
            Alt.OnClient("DrivingSchool_Checker", VehicleChecker);
            this.NextTraj();

            Alt.OnColShape += this.onColShape;
        }
        public async Task End()
        {
            await VehicleExam.Delete();
            Entities.Players.PlayerManager.GetPlayerByClient(PlayerExam)?.RemoveKey(VehicleExam);
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
