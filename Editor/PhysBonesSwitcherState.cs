using System.Collections.Generic;
using MitarashiDango.PhysBonesSwitcher.Runtime;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class PhysBonesSwitcherState
    {
        public bool NeedOptimizingPhaseProcessing { get; set; }
        public List<ExcludeObjectSetting> excludeObjectSettings { get; set; }
    }
}
