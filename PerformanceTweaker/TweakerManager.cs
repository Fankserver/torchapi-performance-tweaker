using Sandbox.Game.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.API;
using Torch.Managers;
using Torch.Managers.PatchManager;

namespace PerformanceTweaker
{
    class TweakerManager : Manager
    {
        [Dependency] private readonly PatchManager _patchManager;
        private PatchContext _ctx;

        private static readonly MethodInfo _LargeTurretBaseUpdateAfterSimulation10 =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation10), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _LargeTurretBaseUpdateAfterSimulation100 =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation100), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _LargeTurretBaseThrottler =
            typeof(TweakerManager).GetMethod(nameof(TweakerManager.LargeTurretBaseThrottler), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        public TweakerManager(ITorchBase torchInstance) : base(torchInstance)
        {
        }

        public override void Attach()
        {
            base.Attach();

            if (_ctx == null)
                _ctx = _patchManager.AcquireContext();
            _ctx.GetPattern(_LargeTurretBaseUpdateAfterSimulation10).Prefixes.Add(_LargeTurretBaseThrottler);
            _ctx.GetPattern(_LargeTurretBaseUpdateAfterSimulation100).Prefixes.Add(_LargeTurretBaseThrottler);
            _patchManager.Commit();
        }

        public override void Detach()
        {
            base.Detach();

            _patchManager.FreeContext(_ctx);
        }

        public static bool LargeTurretBaseThrottler(MyLargeTurretBase __instance)
        {
            if (__instance.Target != null || !TweakerPlugin.Instance.Config.LargeTurretBaseTweakEnabled)
                return true;

            return false;
        }
    }
}