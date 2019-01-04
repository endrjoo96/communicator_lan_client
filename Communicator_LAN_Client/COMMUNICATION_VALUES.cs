using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communicator_LAN_Client
{
    public static class COMMUNICATION_VALUES
    {
        public const string CONNECTION_CLIENT = "CLIENT:";
        public const string CONNECTION_SERVER = "SERVER:";
        public static class SENDING
        {
            public const string USERNAME_AND_PASSWORD = "USERNAMEPASSWORD|";
            public const string TALKING = "TALKING|";
            public const string NOT_TALKING = "NOTTALKING|";
            public const string I_AM_MUTTED = "IAMMUTTED|";
            public const string I_AM_DISCONNECTING = "IMDISCONNECTING|";

            public const string SEND_ME_CLIENT = "CLIENTREQUEST|";
        }
        public static class RECEIVING
        {
            public const string PASSWORD_INCORRECT = "PASSWORDINCORRECT|";
            public const string SERVER_NAME = "SERVERNAME|";
            public const string SERVER_FULL = "SERVERISFULL|";
            public const string YOU_HAVE_BEEN_KICKED = "UVEBEENKICKED|";
            public const string YOU_HAVE_BEEN_MUTED = "UVEBEENMUTED|";
            public const string SERVER_SHUT_DOWN = "SERVERSHUTDOWN|";

            public const string NEXT_CLIENT_FROM_LIST = "NEXT|";
            public const string END_OF_LIST = "EOF|";

            public const string REFRESH_YOUR_LIST = "REFURLIST|";
        }
    }
}
