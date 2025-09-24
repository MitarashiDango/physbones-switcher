using System.Collections.Generic;
using nadena.dev.modular_avatar.core;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class PhysBonesSwitcherParameters
    {
        public static readonly string PhysBonesOffMenuItemOn = "PBS_PhysBonesOffMenuItemOn";
        public static readonly string PhysBonesOff = "PBS_PhysBonesOff";
        public static readonly string DelayType = "PBS_DelayType";
        public static readonly string VehicleMode = "PBS_VehicleMode";
        public static readonly string VehicleModeDelayType = "PBS_VehicleModeDelayType";

        public List<ParameterConfig> GetParameterConfigs(bool vehicleModeSaved)
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
                new ParameterConfig
                {
                    nameOrPrefix = VehicleMode,
                    defaultValue = 0,
                    syncType = ParameterSyncType.Bool,
                    saved = vehicleModeSaved,
                    localOnly = true,
                },
                new ParameterConfig
                {
                    nameOrPrefix = VehicleModeDelayType,
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
