using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Models
{
    public class Social
    {
        public String social;
        public ulong socialId;
        public Social(String social = null, ulong socialId = 0)
        {
            if(social != null) this.social = social;
            if(socialId != 0) this.socialId = socialId;
        }
    }
}
