using System;

namespace ResurrectionRP_Server.Models
{
    public class Time
    {
        public int Hours { set; get; }
        public int Minutes { set; get; }
        public int Seconds { set; get; }

        public Time(int Hours = 8, int Minutes = 0, int Seconds = 0)
        {
            this.Hours = Hours;
            this.Minutes = Minutes;
            this.Seconds = Seconds;
        }

        public void Update()
        {
            Seconds += 4;

            if (Seconds >= 60)
            {
                Seconds = 0;
                Minutes++;
            }

            if (Minutes == 60)
            {
                Minutes = 0;
                Hours++;
            };

            if (Hours == 24)
                Hours = 0;
        }
    }
}