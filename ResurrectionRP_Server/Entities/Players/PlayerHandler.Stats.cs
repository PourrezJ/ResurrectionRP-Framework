using System.Collections.Concurrent;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        #region Fields
        public ConcurrentDictionary<string, dynamic> Stats = new ConcurrentDictionary<string, dynamic>();
        #endregion

        #region Update Stat Methods 
        public int UpdateStat(Stats stat, int data) =>
            Stats.AddOrUpdate(stat.ToString(), data, (key, value) => { return value + data; });
        public double UpdateStat(Stats stat, double data) =>
            Stats.AddOrUpdate(stat.ToString(), data, (key, value) => { return value + data; });

        public int UpdateStat(Stats stat, float data) =>
                Stats.AddOrUpdate(stat.ToString(), data, (key, value) => { return value + data; });
        public int UpdateStat(Stats stat, string data) =>
                Stats.AddOrUpdate(stat.ToString(), data, (key, value) => { return data; });
        #endregion

    }
    public enum Stats
    {
        ToolBroken,
        ToolUses,
        FoodEaten,
        FoodDrunk
    }
}
