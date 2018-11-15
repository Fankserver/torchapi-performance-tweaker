using NLog;
using Sandbox;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Torch.Managers.PatchManager.MSIL;

namespace PerformanceTweaker.Patch
{
    public class MyLargeTurretBasePatch
    {
        private static readonly ConcurrentDictionary<long, int> _largeTurretBaseSlowdown1 = new ConcurrentDictionary<long, int>();
        private static readonly ConcurrentDictionary<long, int> _largeTurretBaseSlowdown10 = new ConcurrentDictionary<long, int>();
        internal static readonly MethodInfo _updateAfterSimulation =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo _updateAfterSimulation10 =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation10), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo _throttler1 =
            typeof(MyLargeTurretBasePatch).GetMethod(nameof(MyLargeTurretBasePatch.Throttler1), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo _throttler10 =
            typeof(MyLargeTurretBasePatch).GetMethod(nameof(MyLargeTurretBasePatch.Throttler10), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Close()
        {
            _largeTurretBaseSlowdown1.Clear();
            _largeTurretBaseSlowdown10.Clear();
        }

        public static bool Throttler1(MyLargeTurretBase __instance)
        {
            return Throttler(__instance, VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME, 100);
        }

        public static bool Throttler10(MyLargeTurretBase __instance)
        {
            return Throttler(__instance, VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME, 10);
        }

        public static bool Throttler(MyLargeTurretBase __instance, VRage.ModAPI.MyEntityUpdateEnum update, int tick)
        {
            if (__instance.Target != null || !TweakerPlugin.Instance.Config.LargeTurretBaseTweakEnabled || TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor == 0f)
                return true;

            int value = 0;
            if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME)
                value = _largeTurretBaseSlowdown1.AddOrUpdate(__instance.EntityId, 1, (key, oldValue) => oldValue++);
            else if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
                value = _largeTurretBaseSlowdown10.AddOrUpdate(__instance.EntityId, 1, (key, oldValue) => oldValue++);

            if (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactorType == 0
                && Sync.ServerSimulationRatio < TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor
                && value < (int)(Sync.ServerSimulationRatio / TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor) * tick)
                return false;
            else if (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactorType == 1
                && Sync.ServerCPULoad > TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor
                && value < (Sync.ServerCPULoad / TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor) * tick)
                return false;

            if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME)
                _largeTurretBaseSlowdown1[__instance.EntityId] = 0;
            else if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
                _largeTurretBaseSlowdown10[__instance.EntityId] = 0;

            return true;
        }
    }
}
