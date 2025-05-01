using System.Collections.Generic;
using MitarashiDango.PhysBonesSwitcher.Runtime;
using UnityEngine;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class PhysBonesSwitcherState
    {
        public bool NeedOptimizingPhaseProcessing { get; set; }
        public List<ExcludeObjectSetting> excludeObjectSettings { get; set; }
        public int customDelayTime { get; set; }
        public AudioClip physBoneOffAudioClip { get; set; }
    }
}
