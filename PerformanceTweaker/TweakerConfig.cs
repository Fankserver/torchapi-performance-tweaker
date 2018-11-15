using Torch;

namespace PerformanceTweaker
{
    public class TweakerConfig : ViewModel
    {
        private bool _largeTurretBaseTweakEnabled = true;
        public bool LargeTurretBaseTweakEnabled { get => _largeTurretBaseTweakEnabled; set => SetValue(ref _largeTurretBaseTweakEnabled, value); }
    }
}
