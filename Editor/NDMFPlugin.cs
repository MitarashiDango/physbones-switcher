using MitarashiDango.PhysBonesSwitcher.Editor;
using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(NDMFPlugin))]

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class NDMFPlugin : Plugin<NDMFPlugin>
    {
        protected override void Configure()
        {
            InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("Run PhysBones Switcher Processes", ctx => Processing(ctx));
        }

        private void Processing(BuildContext ctx)
        {
            PhysBonesSwitcherProcess(ctx);
        }

        private void PhysBonesSwitcherProcess(BuildContext ctx)
        {
            var processor = new PhysBonesSwitcherProcessor();
            processor.Run(ctx);
        }
    }
}
