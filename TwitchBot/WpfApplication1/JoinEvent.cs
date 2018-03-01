using System;

namespace DudeBot
{
    public class JoinEvent
    {
        public String message { get; set; }
        public String userJoin { get; set; }

        public JoinEvent(String userJoin, String message)
        {
            this.message = message;
            this.userJoin = userJoin;
        }
    }
}
