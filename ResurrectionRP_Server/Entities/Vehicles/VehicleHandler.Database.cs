using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using ResurrectionRP_Server.Utils;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler
    {
        #region Fields
        private DateTime _lastUpdateRequest;
        private bool _updateWaiting = false;
        private int _nbUpdateRequests;
        #endregion

        #region Methods
        public async Task InsertVehicle()
        {
            try
            {
                if (SpawnVeh)
                    return;

                await Database.MongoDB.Insert("vehicles", this);
            }
            catch (BsonException be)
            {
                Alt.Server.LogError(be.Message);
            }
        }

        public void UpdateInBackground(bool updateLastUse = true, bool immediatePropertiesUpdate = false)
        {
            if (Trailer != null)
                ((IVehicle)Trailer).GetVehicleHandler().UpdateInBackground(updateLastUse, immediatePropertiesUpdate);

            if (SpawnVeh)
                return;

            if (updateLastUse)
                LastUse = DateTime.Now;

            if (immediatePropertiesUpdate)
                UpdateProperties();

            _lastUpdateRequest = DateTime.Now;

            if (_updateWaiting)
            {
                _nbUpdateRequests++;
                return;
            }

            _updateWaiting = true;
            _nbUpdateRequests = 1;

            Task.Run(async () =>
            {
                DateTime updateTime = _lastUpdateRequest.AddMilliseconds(Globals.SAVE_WAIT_TIME);

                while (DateTime.Now < updateTime)
                {
                    TimeSpan waitTime = updateTime - DateTime.Now;

                    if (waitTime.TotalMilliseconds < 1)
                        waitTime = new TimeSpan(0, 0, 0, 0, 1);

                    await Task.Delay((int)waitTime.TotalMilliseconds);
                    updateTime = _lastUpdateRequest.AddMilliseconds(Globals.SAVE_WAIT_TIME);
                }

                try
                {
                    UpdateProperties();
                    var result = await Database.MongoDB.Update(this, "vehicles", Plate, _nbUpdateRequests);

                    if (result.ModifiedCount == 0)
                        Alt.Server.LogError($"Vehicule Update error for vehicle: {Plate} - Owner: {OwnerID}");

                    _updateWaiting = false;
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("VehicleHandler.UpdateInBackground(): " + ex);
                }
            });
        }

        public async Task<bool> RemoveInDatabase()
        {
            var result = await Database.MongoDB.Delete<VehicleHandler>("vehicles", Plate);
            return (result.DeletedCount != 0);
        }
        #endregion
    }
}