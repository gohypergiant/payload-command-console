using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PayloadCommandConsole.ViewModels;
using System.ComponentModel;

namespace Hypergiant.HIVE.Views
{
    public class InputBox : Window, INotifyPropertyChanged
    {
        public InputBox(string caption, string input = "")
            : this()
        {
            this.DataContext = new InputBoxViewModel(this, caption, input);
        }

        public InputBox()
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
