#if AVATAR_OPTIMIZER && UNITY_EDITOR
using Anatawa12.AvatarOptimizer.API;
using MitarashiDango.PhysBonesSwitcher.Runtime;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    [ComponentInformation(typeof(PhysBonesSwitcherAAOMarker))]
    internal sealed class PhysBonesSwitcherAaoMarkerInformation : ComponentInformation<PhysBonesSwitcherAAOMarker>
    {
        protected override void CollectDependency(PhysBonesSwitcherAAOMarker component, ComponentDependencyCollector collector)
        {
            collector.MarkEntrypoint();

            foreach (var target in component.controlledPhysBoneRoots)
            {
                if (target != null)
                {
                    collector.AddDependency(target);
                }
            }

            foreach (var excludedPhysBone in component.excludedPhysBones)
            {
                var target = excludedPhysBone.rootTransform != null
                    ? excludedPhysBone.rootTransform
                    : excludedPhysBone.transform;

                if (target != null && target.IsChildOf(component.avatarRoot))
                {
                    collector.AddPathDependency(target, component.avatarRoot);
                }
            }
        }
    }
}
#endif
