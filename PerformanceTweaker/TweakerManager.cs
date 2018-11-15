using NLog;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using Torch.API;
using Torch.Managers;
using Torch.Managers.PatchManager;

namespace PerformanceTweaker
{
    class TweakerManager : Manager
    {
        [Dependency] private readonly PatchManager _patchManager;
        private PatchContext _ctx;
        private static readonly ConcurrentDictionary<long, int> _largeTurretBaseSlowdown = new ConcurrentDictionary<long, int>();
        private static readonly MethodInfo _largeTurretBaseUpdateAfterSimulation =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _largeTurretBaseUpdateAfterSimulation10 =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation10), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _largeTurretBaseThrottler =
            typeof(TweakerManager).GetMethod(nameof(TweakerManager.LargeTurretBaseThrottler), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _largeTurretBaseThrottler10 =
            typeof(TweakerManager).GetMethod(nameof(TweakerManager.LargeTurretBaseThrottler10), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public TweakerManager(ITorchBase torchInstance) : base(torchInstance)
        {
        }

        public override void Attach()
        {
            base.Attach();

            if (_ctx == null)
                _ctx = _patchManager.AcquireContext();
            _ctx.GetPattern(_largeTurretBaseUpdateAfterSimulation).Prefixes.Add(_largeTurretBaseThrottler);
            _ctx.GetPattern(_largeTurretBaseUpdateAfterSimulation10).Prefixes.Add(_largeTurretBaseThrottler10);
            _patchManager.Commit();
        }

        public override void Detach()
        {
            base.Detach();

            _patchManager.FreeContext(_ctx);
        }

        public static bool LargeTurretBaseThrottler(MyLargeTurretBase __instance)
        {
            return LargeTurretBaseThrottler(__instance, 100);
        }

        public static bool LargeTurretBaseThrottler10(MyLargeTurretBase __instance)
        {
            return LargeTurretBaseThrottler(__instance, 10);
        }

        public static bool LargeTurretBaseThrottler(MyLargeTurretBase __instance, int tick)
        {
            if (__instance.Target != null || !TweakerPlugin.Instance.Config.LargeTurretBaseTweakEnabled)
                return true;

            int value = _largeTurretBaseSlowdown.AddOrUpdate(__instance.EntityId, 1, (key, oldValue) => oldValue++);
            if (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactorType == 0 && value < (int)(1 - (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor - Sync.ServerSimulationRatio) * tick))
                return false;
            else if (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactorType == 1
                && Sync.ServerCPULoad - TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor > 0
                && value < (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor / Sync.ServerCPULoad) * tick)
                return false;
            _largeTurretBaseSlowdown[__instance.EntityId] = 0;

            return true;
        }
    }
}