﻿using AltV.Net;
using ResurrectionRP_Server.Utils;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Society
{
    public partial class Society
    {
        #region Fields
        private DateTime _lastUpdateRequest;
        private bool _updateWaiting = false;
        private int _nbUpdateRequests;
        #endregion

        #region Methods
        public async Task Insert()
            => await Database.MongoDB.Insert("society", this);

        public void UpdateInBackground()
        {
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
                    var result = await Database.MongoDB.Update(this, "society", _id, _nbUpdateRequests);

                    if (result.MatchedCount == 0)
                        Alt.Server.LogWarning($"Update error for business {_id}");

                    _updateWaiting = false;
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"BankAccount.UpdateInBackground() - {_id} - {ex}");
                }
            });
        }
        #endregion
    }
}
