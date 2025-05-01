using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace MitarashiDango.PhysBonesSwitcher.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("PhysBones Switcher/PhysBones Switcher")]
    public class PhysBonesSwitcher : MonoBehaviour, IEditorOnly
    {
        [SerializeField]
        public List<ExcludeObjectSetting> excludeObjectSettings = new List<ExcludeObjectSetting>();
    }
}
