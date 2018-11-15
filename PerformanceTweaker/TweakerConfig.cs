using Torch;

namespace PerformanceTweaker
{
    public class TweakerConfig : ViewModel
    {
        private bool _largeTurretBaseTweakEnabled = true;
        public bool LargeTurretBaseTweakEnabled { get => _largeTurretBaseTweakEnabled; set => SetValue(ref _largeTurretBaseTweakEnabled, value); }

        private int _largeTurretBaseTweakFactorType = 0;
        public int LargeTurretBaseTweakFactorType { get => _largeTurretBaseTweakFactorType; set => SetValue(ref _largeTurretBaseTweakFactorType, value); }

        private float _largeTurretBaseTweakFactor = 1f;
        public float LargeTurretBaseTweakFactor { get => _largeTurretBaseTweakFactor; set => SetValue(ref _largeTurretBaseTweakFactor, value); }
    }
}
