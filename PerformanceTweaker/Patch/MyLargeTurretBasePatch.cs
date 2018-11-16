using NLog;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using System;
using System.Collections.Concurrent;
using System.Reflection;

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
            if (__instance.Target != null || __instance.IsPlayerControlled || !TweakerPlugin.Instance.Config.LargeTurretBaseTweakEnabled || TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor == 0f)
                return true;

            int value = 0;
            if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME)
                value = _largeTurretBaseSlowdown1.AddOrUpdate(__instance.EntityId, 1, (key, oldValue) => ++oldValue);
            else if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
                value = _largeTurretBaseSlowdown10.AddOrUpdate(__instance.EntityId, 1, (key, oldValue) => ++oldValue);

            switch (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactorType)
            {
                case 0:
                    if (value < (int)((Sync.ServerSimulationRatio < TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor) ? (Sync.ServerSimulationRatio / TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor) : 1) * tick)
                        return false;
                    break;
                case 1:
                    if (value < ((Sync.ServerCPULoad > TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor) ? (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor / Sync.ServerCPULoad) : 1) * tick)
                        return false;
                    break;
                case 2:
                    if (value * (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME ? 10 : 1) < TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor)
                        return false;
                    break;
                case 3:
                    return false;
            }

#if DEBUG
            Log.Debug($"MyLargeTurretBase update={update.ToString()} value={value} entity={__instance.EntityId}({__instance.DisplayNameText})");
#endif

            if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME)
                _largeTurretBaseSlowdown1[__instance.EntityId] = 0;
            else if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
                _largeTurretBaseSlowdown10[__instance.EntityId] = 0;

            return true;
        }
    }
}
