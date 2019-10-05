using System;

namespace ResurrectionRP_Server.Illegal.WeedLab.Data
{
    public class Drying
    {
        public SeedType WeedType = SeedType.Aucune;
        public DateTime FinalDateTime { private set; get; } = DateTime.Now.AddMinutes(10);
        public Drying(SeedType weedtype) { WeedType = weedtype; }
    }
}
