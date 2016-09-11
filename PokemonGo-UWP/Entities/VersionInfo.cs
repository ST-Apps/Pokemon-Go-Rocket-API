using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 0649

namespace PokemonGo_UWP.Entities
{
    class VersionInfo
    {
        private static VersionInfo instance;

        static VersionInfo()
        {
        }

        private VersionInfo()
        {
        }

        public static VersionInfo Instance
        {
            get
            {
                return instance;
            }
        }

        public static bool SetInstance(string json)
        {
            instance = JsonConvert.DeserializeObject<VersionInfo>(json);
            return instance != null;
        }

        public string minimum_version;
        public long unknown25;
        public ulong seed1;
        public int version_number;
        public VersionRelease latest_release;
    }

    class VersionRelease
    {
        public string version;
        public string setup_file;
        public string changes;
        public string[] dependencies;
    }
}
