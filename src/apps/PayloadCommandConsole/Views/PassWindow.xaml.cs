using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData.Binding;
using PayloadCommandConsole.Models;
using PayloadCommandConsole.ViewModels;

namespace PayloadCommandConsole.Views
{
    public class PassWindow : Window
    {
        public PassWindow()
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
