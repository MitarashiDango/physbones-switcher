using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace MitarashiDango.PhysBonesSwitcher.Runtime
{
    public sealed class PhysBonesSwitcherAAOMarker : MonoBehaviour
    {
        public Transform avatarRoot;
        public List<Transform> controlledPhysBoneRoots = new();
        public List<VRCPhysBone> excludedPhysBones = new();
    }
}
