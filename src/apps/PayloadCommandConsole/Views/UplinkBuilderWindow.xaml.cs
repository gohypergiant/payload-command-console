using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Hypergiant.HIVE.Views
{
    public class UplinkBuilderWindow : Window
    {
        public UplinkBuilderWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
