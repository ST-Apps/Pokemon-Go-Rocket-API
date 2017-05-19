using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoAPI.Enums
{
    public enum StatusCode : int
    {
        Unknown = 0,
        OK = 1,                                 // valid response with no api url
        OK_RPC_Url_In_Response = 2,             // the response envelope has api_url set and this response is valid
        Bad_Request = 3,                        // bad request
        Invalid_Request = 51,                   // using unimplemented request or corrupt request
        Invalid_Platform_Request = 52,          // invalid platform request or corrupt platform request
        Redirect = 53,                          // a new rpc endpoint is available and you should redirect to there
        Session_Invalidated = 100,              // occurs when you send blank authinfo, or sending nonsense things (ie LocationFix.timestampSnapshot == Signature.timestampSinceStart)
        InvalidToken = 102,                     // occurs when the login token is invalid
    }
}
