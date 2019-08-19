using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Phone.Data
{
    public class Conversation
    {
        public ObjectId _id;
        public string sender;
        public string receiver;
        public string receiverName;
        public List<SMS> messages = new List<SMS>();
        public DateTime lastMessageDate = DateTime.Now.AddYears(19);
        public DateTime lastReadDate = DateTime.Now.AddYears(19);

        public async Task Update()
        {
            await Database.MongoDB.Update<Conversation>(this, "conversations", _id);
        }
    }
}
