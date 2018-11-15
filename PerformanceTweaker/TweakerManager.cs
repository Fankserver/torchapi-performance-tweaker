using NLog;
using Sandbox;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Torch.API;
using Torch.Managers;
using Torch.Managers.PatchManager;
using Torch.Managers.PatchManager.MSIL;

namespace PerformanceTweaker
{
    class TweakerManager : Manager
    {
        [Dependency] private readonly PatchManager _patchManager;
        private PatchContext _ctx;
        private static readonly ConcurrentDictionary<long, int> _largeTurretBaseSlowdown1 = new ConcurrentDictionary<long, int>();
        private static readonly ConcurrentDictionary<long, int> _largeTurretBaseSlowdown10 = new ConcurrentDictionary<long, int>();
        private static readonly MethodInfo _largeTurretBaseUpdateAfterSimulation =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _largeTurretBaseUpdateAfterSimulation10 =
            typeof(MyLargeTurretBase).GetMethod(nameof(MyLargeTurretBase.UpdateAfterSimulation10), BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _largeTurretBaseThrottler1 =
            typeof(TweakerManager).GetMethod(nameof(TweakerManager.LargeTurretBaseThrottler1), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _largeTurretBaseThrottler1x =
            typeof(TweakerManager).GetMethod(nameof(TweakerManager.LargeTurretBaseThrottler1x), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _largeTurretBaseThrottler10 =
            typeof(TweakerManager).GetMethod(nameof(TweakerManager.LargeTurretBaseThrottler10), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        private static readonly MethodInfo _largeTurretBaseThrottler10x =
            typeof(TweakerManager).GetMethod(nameof(TweakerManager.TranspilerForUpdate), BindingFlags.Static | BindingFlags.Public) ??
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
            //_ctx.GetPattern(_largeTurretBaseUpdateAfterSimulation).Prefixes.Add(_largeTurretBaseThrottler1);
            _ctx.GetPattern(_largeTurretBaseUpdateAfterSimulation).Transpilers.Add(_largeTurretBaseThrottler10x.MakeGenericMethod(typeof(MyLargeTurretBase)));
            _ctx.GetPattern(_largeTurretBaseUpdateAfterSimulation10).Prefixes.Add(_largeTurretBaseThrottler10);
            _patchManager.Commit();
        }

        public override void Detach()
        {
            base.Detach();

            _largeTurretBaseSlowdown1.Clear();
            _largeTurretBaseSlowdown10.Clear();

            _patchManager.FreeContext(_ctx);
        }

        public static bool LargeTurretBaseThrottler1(MyLargeTurretBase __instance)
        {
            return LargeTurretBaseThrottler(__instance, VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME, 100);
        }

        public static bool LargeTurretBaseThrottler1x()
        {
            return true;
        }

        public static bool LargeTurretBaseThrottler10(MyLargeTurretBase __instance)
        {
            return LargeTurretBaseThrottler(__instance, VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME, 10);
        }

        public static bool LargeTurretBaseThrottler(MyLargeTurretBase __instance, VRage.ModAPI.MyEntityUpdateEnum update, int tick)
        {
            //if (__instance.Target != null || !TweakerPlugin.Instance.Config.LargeTurretBaseTweakEnabled || MySandboxGame.TotalTimeInMilliseconds < 60 * 1000)
            //    return true;

            //int value = 0;
            //if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME)
            //    value = _largeTurretBaseSlowdown1.AddOrUpdate(__instance.EntityId, 1, (key, oldValue) => oldValue++);
            //else if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
            //    value = _largeTurretBaseSlowdown10.AddOrUpdate(__instance.EntityId, 1, (key, oldValue) => oldValue++);

            //if (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactorType == 0
            //    && Sync.ServerSimulationRatio < TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor
            //    && value < (int)(Sync.ServerSimulationRatio / TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor) * tick)
            //    return false;
            //else if (TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactorType == 1
            //    && Sync.ServerCPULoad - TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor > 0
            //    && value >= (Sync.ServerCPULoad / TweakerPlugin.Instance.Config.LargeTurretBaseTweakFactor) * 100)
            //    return false;

            //if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME)
            //    _largeTurretBaseSlowdown1[__instance.EntityId] = 0;
            //else if (update == VRage.ModAPI.MyEntityUpdateEnum.EACH_10TH_FRAME)
            //    _largeTurretBaseSlowdown10[__instance.EntityId] = 0;

            return true;
        }

        public static IEnumerable<MsilInstruction> TranspilerForUpdate<T>(IEnumerable<MsilInstruction> instructions,
            // ReSharper disable once InconsistentNaming
            Func<Type, MsilLocal> __localCreator,
            // ReSharper disable once InconsistentNaming
            MethodBase __methodBase)
        {
            bool firstRun = false;
            MsilLabel returnLabel = new MsilLabel();
            foreach (var i in instructions)
            {
                if (i.OpCode == OpCodes.Ret)
                {
                    foreach (var label in i.Labels)
                    {
                        returnLabel = label;
                        break;
                    }
                }
            }
            foreach (var i in instructions)
            {
                if (!firstRun)
                {
                    Log.Info(i.ToString());
                    yield return i;
                    //Log.Info(test.AsReferenceLoad().ToString());
                    //yield return test.AsReferenceLoad();
                    Log.Info(new MsilInstruction(OpCodes.Callvirt).InlineValue(_largeTurretBaseThrottler1x).ToString());
                    yield return new MsilInstruction(OpCodes.Callvirt).InlineValue(_largeTurretBaseThrottler1x);
                    Log.Info(new MsilInstruction(OpCodes.Brfalse_S).InlineTarget(returnLabel).ToString());
                    yield return new MsilInstruction(OpCodes.Brfalse_S).InlineTarget(returnLabel);
                    var label = new MsilLabel();
                    Log.Info(new MsilInstruction(OpCodes.Ldarg_0).LabelWith(label).ToString());
                    yield return new MsilInstruction(OpCodes.Ldarg_0).LabelWith(label);

                    firstRun = true;
                    continue;
                }

                Log.Info(i.ToString());
                yield return i;
            }
        }
    }
}