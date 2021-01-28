using Avalonia.Controls;
using ReactiveUI;

namespace PayloadCommandConsole.ViewModels
{
    public class InputBoxViewModel : ReactiveObject
    {
        private string m_input;
        private string m_capture;

        public System.Windows.Input.ICommand Button1Command { get; }
        public System.Windows.Input.ICommand Button2Command { get; }
        private Window View { get; }

        public InputBoxViewModel(Window view, string caption, string input = "")
        {
            View = view;
            Caption = caption;
            Input = input;

            Button1Command = ReactiveCommand.Create(async () =>
            {
                View.Close(Input);
            });
            Button2Command = ReactiveCommand.Create(async () =>
            {
                View.Close(null);
            });
        }

        public InputBoxViewModel()
        {
            Input = "this is the input";
            Caption = "Enter the new name";
        }

        public string Input
        {
            get => m_input;
            set => this.RaiseAndSetIfChanged(ref m_input, value);
        }

        public string Caption
        {
            get => m_capture;
            set => this.RaiseAndSetIfChanged(ref m_capture, value);
        }

    }
}
