using System.Windows;
using System.Windows.Controls;

namespace PerformanceTweaker
{
    /// <summary>
    /// Interaktionslogik für RoleControl.xaml
    /// </summary>
    public partial class TweakerControl : UserControl
    {
        private TweakerPlugin Plugin { get; }

        public TweakerControl()
        {
            InitializeComponent();
        }

        public TweakerControl(TweakerPlugin plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
        }

        private void SaveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
        }
    }
}
