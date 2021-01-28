using Avalonia.Controls;
using DynamicData.Binding;
using PayloadCommandConsole.Models;
using ReactiveUI;

namespace PayloadCommandConsole.ViewModels
{
    public class PassListViewModel : ReactiveObject
    {
        private ObservablePass m_selected;

        private ObservableCollectionExtended<ObservablePass> Passes { get; }
        private Window View { get; }

        public System.Windows.Input.ICommand SelectCommand { get; }
        public System.Windows.Input.ICommand CancelCommand { get; }

        public PassListViewModel(Window view, ObservableCollectionExtended<ObservablePass> passes)
        {
            View = view;
            Passes = passes;

            SelectCommand = ReactiveCommand.Create(() =>
            {
                view.Close(SelectedPass);
            });
            CancelCommand = ReactiveCommand.Create(() =>
            {
                view.Close(null);
            });
        }

        public ObservablePass SelectedPass
        {
            get => m_selected;
            set => this.RaiseAndSetIfChanged(ref m_selected, value);
        }
    }
}
