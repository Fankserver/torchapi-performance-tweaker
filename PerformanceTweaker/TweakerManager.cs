using NLog;
using PerformanceTweaker.Patch;
using System.Threading.Tasks;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Session;
using Torch.Managers;
using Torch.Managers.PatchManager;
using Torch.Session;

namespace PerformanceTweaker
{
    class TweakerManager : Manager
    {
        [Dependency] private readonly PatchManager _patchManager;
        private PatchContext _ctx;
        private TorchSessionManager _sessionManager;

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public TweakerManager(ITorchBase torchInstance) : base(torchInstance)
        {
        }

        public override void Attach()
        {
            base.Attach();

            _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (_sessionManager != null)
                _sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");

            if (_ctx == null)
                _ctx = _patchManager.AcquireContext();
        }

        public override void Detach()
        {
            base.Detach();

            MyLargeTurretBasePatch.Close();

            _patchManager.FreeContext(_ctx);
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {
            switch (state)
            {
                case TorchSessionState.Loaded:
                    Task.Delay(3000).ContinueWith((t) =>
                    {
                        Log.Debug("Patching MyLargeTurretBasePatch");
                        _ctx.GetPattern(MyLargeTurretBasePatch._updateAfterSimulation).Prefixes.Add(MyLargeTurretBasePatch._throttler1);
                        _ctx.GetPattern(MyLargeTurretBasePatch._updateAfterSimulation10).Prefixes.Add(MyLargeTurretBasePatch._throttler10);
                        _patchManager.Commit();
                    });
                    break;
            }
        }
    }
}