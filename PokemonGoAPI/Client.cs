using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.HttpClient;
using PokemonGo.RocketAPI.Rpc;
using PokemonGoAPI.Session;
using POGOProtos.Networking.Envelopes;

namespace PokemonGo.RocketAPI
{
    public class Client
    {
        internal readonly PokemonHttpClient PokemonHttpClient = new PokemonHttpClient();

        public IDeviceInfo DeviceInfo;
        public Download Download;
        public Encounter Encounter;
        public Fort Fort;
        public Inventory Inventory;
        public Rpc.Login Login;
        public Map Map;
        public Misc Misc;
        public Player Player;

        public Client(ISettings settings, IApiFailureStrategy apiFailureStrategy, IDeviceInfo deviceInfo, AccessToken accessToken = null)
        {
            Settings = settings;
            ApiFailure = apiFailureStrategy;
            AccessToken = accessToken;

            Login = new Rpc.Login(this);
            Player = new Player(this);
            Download = new Download(this);
            Inventory = new Inventory(this);
            Map = new Map(this);
            Fort = new Fort(this);
            Encounter = new Encounter(this);
            Misc = new Misc(this);
            DeviceInfo = deviceInfo;

            Player.SetCoordinates(Settings.DefaultLatitude, Settings.DefaultLongitude, Settings.DefaultAltitude);
        }

        public IApiFailureStrategy ApiFailure { get; set; }
        public ISettings Settings { get; }
        public string AuthToken => AccessToken?.Token;

        public double CurrentLatitude { get; internal set; }
        public double CurrentLongitude { get; internal set; }
        public double CurrentAltitude { get; internal set; }

        public AuthType AuthType => Settings.AuthType;
        internal string ApiUrl { get; set; }
        internal AuthTicket AuthTicket => AccessToken?.AuthTicket;
        public AccessToken AccessToken { get; set; }
    }
}