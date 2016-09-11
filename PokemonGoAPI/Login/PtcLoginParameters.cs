using Newtonsoft.Json;

namespace PokemonGoAPI.Login
{

    /// <summary>
    /// 
    /// </summary>
    internal class PtcLoginParameters
    {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("lt")]
        public string Lt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("execution")]
        public string Execution { get; set; }

    }

}