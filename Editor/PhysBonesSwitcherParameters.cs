using System.Collections.Generic;
using nadena.dev.modular_avatar.core;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class PhysBonesSwitcherParameters
    {
        public static readonly string PhysBonesOffMenuItemOn = "PBS_PhysBonesOffMenuItemOn";
        public static readonly string PhysBonesOff = "PBS_PhysBonesOff";
        public static readonly string DelayType = "PBS_DelayType";

        public List<ParameterConfig> GetParameterConfigs()
        {
            var parameterConfigs = new List<ParameterConfig>
            {
                new ParameterConfig
                {
                    nameOrPrefix = PhysBonesOffMenuItemOn,
                    defaultValue = 0,
                    syncType = ParameterSyncType.Bool,
                    saved = false,
                    localOnly = true,
                },
                new ParameterConfig
                {
                    nameOrPrefix = PhysBonesOff,
                    defaultValue = 0,
                    syncType = ParameterSyncType.Bool,
                    saved = false,
                    localOnly = false,
                },
                new ParameterConfig
                {
                    nameOrPrefix = DelayType,
                    defaultValue = 0,
                    syncType = ParameterSyncType.Int,
                    saved = true,
                    localOnly = true,
                },
            };

            return parameterConfigs;
        }
    }
}
