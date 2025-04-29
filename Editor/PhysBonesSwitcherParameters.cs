using System.Collections.Generic;
using nadena.dev.modular_avatar.core;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class PhysBonesSwitcherParameters
    {
        public static readonly string PhysBonesOff = "PBS_PhysBonesOff";

        public List<ParameterConfig> GetParameterConfigs()
        {
            var parameterConfigs = new List<ParameterConfig>
            {
                new ParameterConfig
                {
                    nameOrPrefix = PhysBonesOff,
                    defaultValue = 0,
                    syncType = ParameterSyncType.Bool,
                    saved = false,
                    localOnly = false,
                },
            };

            return parameterConfigs;
        }
    }
}
