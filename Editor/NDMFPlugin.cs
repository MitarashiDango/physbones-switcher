using MitarashiDango.PhysBonesSwitcher.Editor;
using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(NDMFPlugin))]

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class NDMFPlugin : Plugin<NDMFPlugin>
    {
        public override string DisplayName => $"PhysBones Switcher";
        public override string QualifiedName => "com.matcha-soft.physbones-switcher";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("Run PhysBones Switcher Processes (Generating Phase)", ctx => GeneratingPhaseProcess(ctx));

            // VRC Phys Bone の走査は Avatar Optimizer による最適化後に実行する
            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("com.anatawa12.avatar-optimizer")
                .Run("Run PhysBones Switcher Processes (Optimizing Phase)", ctx => OptimizingPhaseProcess(ctx));
        }

        private void GeneratingPhaseProcess(BuildContext ctx)
        {
            var processor = new PhysBonesSwitcherProcessor();
            processor.GeneratingProcess(ctx);
        }

        private void OptimizingPhaseProcess(BuildContext ctx)
        {
            var processor = new PhysBonesSwitcherProcessor();
            processor.OptimizingProcess(ctx);
        }
    }
}
