
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ResurrectionRP_Server.Jobs
{
    public static class JobsManager
    {
        public static List<Jobs> JobsList = new List<Jobs>();

        public static void Init()
        {
            var validator_type = typeof(Jobs);

            var sub_validator_types =
                validator_type
                .Assembly
                .DefinedTypes
                .Where(x => validator_type.IsAssignableFrom(x) && x != validator_type)
                .ToList();


            foreach (var sub_validator_type in sub_validator_types)
            {
                Jobs jobs = Activator.CreateInstance(sub_validator_type) as Jobs;

                if (jobs != null)
                {
                    jobs.Load();
                    JobsList.Add(jobs);
                }
            }
        }
    }
}