﻿using NLog;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace PerformanceTweaker.Patch
{
    public class MyLargeTurretBasePatch
    {
        private static readonly Random _rnd = new Random();
        private static readonly ConcurrentDictionary<long, int> _largeTurretBaseKeepalive = new ConcurrentDictionary<long, int>();
        private static readonly ConcurrentDictionary<long, int> _largeTurretBaseSlowdown1 = new ConcurrentDictionary<long, int>();
        private static readonly ConcurrentDictionary<long, int> _largeTurretBaseSlowdown10 = new ConcurrentDictionary<long, int>();
        internal static readonly MethodInfo _updateAfterSimulation =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo _updateAfterSimulation10 =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation10), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo _throttler1 =
            typeof(MyLargeTurretBasePatch).GetMethod(nameof(Throttler1), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo _throttler10 =
            typeof(MyLargeTurretBasePatch).GetMethod(nameof(Throttler10), BindingFlags.Static | BindingFlags.Public) ??
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
            // Not enabled or wrongly configured
            if (!TweakerPlugin.Instance.Config.LargeTurretBaseTweakEnabled || TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor == 0f)
                return true;

            // Block is not working
            if (!__instance.IsWorking)
                return true;
                
            // Is doing some shooting
            if (__instance.Target != null || __instance.IsPlayerControlled)
            {
                _largeTurretBaseKeepalive.AddOrUpdate(__instance.EntityId, 30, (key, oldValue) => 30);
#if DEBUG
                if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
                    Log.Debug($"MyLargeTurretBase reset keepalive update={update.ToString()} value=30 entity={__instance.EntityId}({__instance.DisplayNameText})");
#endif
                return true;
            }

            // Was doing some shooting and needs to be active until hot
            else if (_largeTurretBaseKeepalive.TryGetValue(__instance.EntityId, out var keepalive) && keepalive > 0)
            {
                if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
                {
                    _largeTurretBaseKeepalive.TryUpdate(__instance.EntityId, keepalive - 1, keepalive);
#if DEBUG
                    Log.Debug($"MyLargeTurretBase decreasing keepalive update={update.ToString()} value={keepalive} entity={__instance.EntityId}({__instance.DisplayNameText})");
#endif
                }
                
                return true;
            }

            int value = 0;
            if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME)
                value = _largeTurretBaseSlowdown1.AddOrUpdate(__instance.EntityId, _rnd.Next(1, 90), (key, oldValue) => ++oldValue);
            else if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
                value = _largeTurretBaseSlowdown10.AddOrUpdate(__instance.EntityId, _rnd.Next(1, 9), (key, oldValue) => ++oldValue);

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
            Log.Debug($"MyLargeTurretBase pass update={update.ToString()} value={value} entity={__instance.EntityId}({__instance.DisplayNameText})");
#endif

            if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME)
                _largeTurretBaseSlowdown1[__instance.EntityId] = 0;
            else if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
                _largeTurretBaseSlowdown10[__instance.EntityId] = 0;

            return true;
        }
    }
}
