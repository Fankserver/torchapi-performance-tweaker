using NLog;
using System;
using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Plugins;

namespace PerformanceTweaker
{
    public class TweakerPlugin : TorchPluginBase, IWpfPlugin
    {
        private TweakerControl _control;
        private Persistent<TweakerConfig> _config;

        public static TweakerPlugin Instance { get; private set; }
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public TweakerConfig Config => _config?.Data;

        /// <inheritdoc />
        public UserControl GetControl() => _control ?? (_control = new TweakerControl(this));

        public void Save() => _config.Save();

        /// <inheritdoc />
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            var configFile = Path.Combine(StoragePath, "PerformanceTweaker.cfg");

            try
            {
                _config = Persistent<TweakerConfig>.Load(configFile);
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            if (_config?.Data == null)
                _config = new Persistent<TweakerConfig>(configFile, new TweakerConfig());

            var pgmr = new TweakerManager(torch);
            torch.Managers.AddManager(pgmr);

            Instance = this;
        }
    }
}
