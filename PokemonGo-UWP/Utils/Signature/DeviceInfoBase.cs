using PokemonGo.RocketAPI.Helpers;
using PokemonGo_UWP.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    /// Common infos not platform dependent
    /// </summary>
    public abstract class DeviceInfoBase
    {

        private VersionData _versionData = new VersionData();
        public IVersionData VersionData => _versionData;

        public long TimeSnapshot => DeviceInfos.RelativeTimeFromStart;

    }


    public class VersionData : IVersionData
    {
        public ulong HashSeed1 => VersionInfo.Instance.seed1;

        public long VersionHash => VersionInfo.Instance.unknown25;

        public int Version => VersionInfo.Instance.version_number;
    }
}
