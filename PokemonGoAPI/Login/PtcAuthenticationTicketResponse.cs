using Newtonsoft.Json;
using System.Collections.Generic;

namespace PokemonGoAPI.Login
{
    internal class PtcAuthenticationTicketResponse : PtcLoginParameters
    {

        /// <summary>
        /// A list of errors returned from the Authentication request.
        /// </summary>
        [JsonProperty("errors")]
        public List<string> Errors { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("redirect")]
        public string Redirect { get; set; }

    }

}