using NLog;
using PerformanceTweaker.Patch;
using Sandbox.Game.Weapons;
using Torch.API;
using Torch.Managers;
using Torch.Managers.PatchManager;

namespace PerformanceTweaker
{
    class TweakerManager : Manager
    {
        [Dependency] private readonly PatchManager _patchManager;
        private PatchContext _ctx;

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public TweakerManager(ITorchBase torchInstance) : base(torchInstance)
        {
        }

        public override void Attach()
        {
            base.Attach();

            if (_ctx == null)
                _ctx = _patchManager.AcquireContext();
            //_ctx.GetPattern(MyLargeTurretBasePatch._updateAfterSimulation).Transpilers.Add(MyLargeTurretBasePatch._transpilerForAfterUpdate1);
            //_ctx.GetPattern(MyLargeTurretBasePatch._updateAfterSimulation10).Transpilers.Add(MyLargeTurretBasePatch._transpilerForAfterUpdate10);
            _ctx.GetPattern(MyLargeTurretBasePatch._updateAfterSimulation).Prefixes.Add(MyLargeTurretBasePatch._throttler1);
            _ctx.GetPattern(MyLargeTurretBasePatch._updateAfterSimulation10).Prefixes.Add(MyLargeTurretBasePatch._throttler10);
            _patchManager.Commit();
        }

        public override void Detach()
        {
            base.Detach();

            MyLargeTurretBasePatch.Close();

            _patchManager.FreeContext(_ctx);
        }
    }
}