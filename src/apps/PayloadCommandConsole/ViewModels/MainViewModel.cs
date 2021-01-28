using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using DynamicData;
using DynamicData.Binding;
using Hypergiant.HIVE;
using Hypergiant.HIVE.Views;
using PayloadCommandConsole.Models;
using PayloadCommandConsole.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace PayloadCommandConsole.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private CommandStore m_store;
        private ICommandExecutionService m_executive;

        private string m_passNumber;
        private CommandDetailsViewModel m_selectedCommand;
        private PassDetailsViewModel m_selectedPass;

        private ReadOnlyObservableCollection<CommandDetailsViewModel> m_commands;

        public ReadOnlyObservableCollection<CommandDetailsViewModel> Commands
        {
            get => m_commands;
            set => this.RaiseAndSetIfChanged(ref m_commands, value);
        }

        public System.Windows.Input.ICommand SettingsCommand { get; }
        public System.Windows.Input.ICommand SelectPassCommand { get; }
        public System.Windows.Input.ICommand RefreshCommand { get; }
        public System.Windows.Input.ICommand OpenCommand { get; }
        public System.Windows.Input.ICommand UplinkCommand { get; }
        private Window View { get; }

        public DateTime Now => DateTime.Now.ToUniversalTime();
        public bool PassSelected => SelectedPass != null;

        public MainViewModel()
            : this(null, null, true)
        {

        }

        public MainViewModel(Window view, INotificationManager notificationManager, bool mockData = false)
            : base(notificationManager)
        {
            View = view;

            m_store = new CommandStore(mockData);
            m_executive = new HGSCommandExecutionService(notificationManager);

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    this.RaisePropertyChanged(nameof(Now));
                    this.RaisePropertyChanged(nameof(TimeToPass));
                }
            });

            SettingsCommand = ReactiveCommand.Create(() =>
            {
                var settings = new SettingsWindow();
                settings.DataContext = new SettingsViewModel(settings, m_executive, notificationManager);
                settings.ShowDialog(View);
            });

            SelectPassCommand = ReactiveCommand.Create(async () =>
            {
                var w = new PassWindow();
                w.DataContext = new PassListViewModel(w, m_store.Passes);

                var result = await w.ShowDialog<ObservablePass>(View);

                if (result != null)
                {
                    SelectedPass = new PassDetailsViewModel(m_store, result);
                }
            });

            RefreshCommand = ReactiveCommand.Create(() =>
            {
                _ = RefreshCommands();
            });

            OpenCommand = ReactiveCommand.Create(async () =>
            {
                if (SelectedPass == null)
                {
                    return;
                }

                var ofd = new OpenFileDialog
                {
                    AllowMultiple = true,
                    Title = "Import Command File",
                };

                var result = await ofd.ShowAsync(View);
                if (result != null && result.Length > 0)
                {
                    var successcount = 0;
                    var toSelect = Guid.Empty;

                    foreach (var f in result)
                    {
                        // parse through the current executive
                        if (File.Exists(f))
                        {
                            var name = Path.GetFileName(f);
                            var payload = File.ReadAllText(f);
                            var cmd = m_executive.Parse(payload);
                            cmd.Name = name;

                            if (await m_store.ImportCommand(cmd, SelectedPass.PassID))
                            {
                                toSelect = cmd.CommandID;
                                successcount++;
                            }
                        }
                    }

                    this.RaisePropertyChanged(nameof(Commands));
                    NotificationManager?.Show(new Notification("Import", $"{successcount} file{(successcount == 1 ? string.Empty : "s")} imported"));

                    // select the last imported
                    if (toSelect != Guid.Empty)
                    {
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(500);
                            SelectCommand(toSelect);
                        });

                    }
                }
            });

            UplinkCommand = ReactiveCommand.Create(async () =>
            {
                if (SelectedPass == null)
                {
                    return;
                }

                // TODO: show uplink builder
                var builderWindow = new UplinkBuilderWindow();
                builderWindow.DataContext = new UplinkBuilderViewModel(builderWindow, NotificationManager);

                var result = await builderWindow.ShowDialog<UplinkCommand>(View);

                if (result != null)
                {
                    var envelope = HGSCommandEnvelope.Create(result.Name, false, result);

                    await m_store.ImportCommand(envelope, SelectedPass.PassID);
                }
            });
        }

        public void SelectCommand(Guid commandID)
        {
            SelectedCommand = Commands.FirstOrDefault(c => c.CommandID == commandID);
        }

        public string TimeToPass
        {
            get
            {
                if (SelectedPass == null)
                {
                    return null;
                }

                TimeSpan t;

                if (SelectedPass.ActualAos.HasValue)
                {
                    t = DateTime.Now.ToUniversalTime() - SelectedPass.ActualAos.Value;
                }
                else
                {
                    t = DateTime.Now.ToUniversalTime() - SelectedPass.ScheduledAos;
                }
                return $"{(t < TimeSpan.Zero ? "-" : "+")}{t:hh\\:mm\\:ss}";
            }
        }

        public CommandDetailsViewModel SelectedCommand
        {
            get => m_selectedCommand;
            set
            {
                if (value == null)
                {
                    m_executive.CurrentCommandID = Guid.Empty;
                }
                else
                {
                    m_executive.CurrentCommandID = value.CommandID;
                    Task.Run(async () =>
                    {
                        await SelectedCommand.RefreshCommandHistory();
                    });
                }
                this.RaiseAndSetIfChanged(ref m_selectedCommand, value);
            }
        }

        private async Task RefreshCommands()
        {
            if (SelectedPass == null)
            {
                Commands = null;
            }
            else
            {
                m_store.VerifyPass(SelectedPass.PassID);

                if (Commands == null)
                {
                    try
                    {
                        m_store.Commands[SelectedPass.PassID]
                            .ToObservableChangeSet()
                            .Transform(value => new CommandDetailsViewModel(value, NotificationManager, m_executive, m_store))
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Bind(out m_commands)
                            .Subscribe();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                await m_store.RefreshCommands(SelectedPass.PassID);

                this.RaisePropertyChanged(nameof(Commands));

                if (SelectedCommand != null)
                {
                    await SelectedCommand.RefreshCommandHistory();
                }
            }
        }

        public PassDetailsViewModel SelectedPass
        {
            get => m_selectedPass;
            set
            {
                this.RaiseAndSetIfChanged(ref m_selectedPass, value);
                this.RaisePropertyChanged(nameof(PassSelected));
                Commands = null;
                _ = RefreshCommands();
            }
        }

        public string PassNumber
        {
            get => m_passNumber;
            set => this.RaiseAndSetIfChanged(ref m_passNumber, value);
        }

        private void Commands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PassNumber = Environment.TickCount.ToString();
        }
    }
}
