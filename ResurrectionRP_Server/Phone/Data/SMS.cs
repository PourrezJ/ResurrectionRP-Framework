using MongoDB.Bson;
using System;

namespace ResurrectionRP_Server.Phone.Data
{
    public class SMS
    {
        public bool isOwn;
        public string content;
        public DateTime sentAt = DateTime.Now.AddYears(19);
    }
}
