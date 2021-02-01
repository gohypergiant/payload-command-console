using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Hypergiant.HIVE;
using ReactiveUI;
using System.Threading.Tasks;

namespace PayloadCommandConsole.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private string m_storage;
        private bool m_scp;
        private string m_satelliteAddress;
        private string m_uplinkFolder;
        private string m_userName;
        private string m_password;
        private int m_cfdpEntityID;
        private string m_cfdpConfig;
        private int m_commandPort;

        private Window View { get; }
        private ICommandExecutionService ExecutionService { get; }

        public System.Windows.Input.ICommand CancelCommand { get; }
        public System.Windows.Input.ICommand ApplyCommand { get; }

        public SettingsViewModel()
            : this(null)
        {

        }

        public SettingsViewModel(INotificationManager notificationManager)
            : base(notificationManager)
        {
            // TODO: pull all of this from user prefs/rest call
            StorageService = "http://localhost:5001";

            CancelCommand = ReactiveCommand.Create(() =>
            {
                View.Close();
            });
            ApplyCommand = ReactiveCommand.Create(async () =>
            {
                var cfg = new ExecutiveConfiguration
                {
                    DestinationUplinkFolder = "/home/pi/uplink",
                    SatelliteAddress = SatelliteAddress,
                };

                if (UseScp)
                {
                    cfg.ExecutiveType = "scp";
                    cfg.UserName = UserName;
                    cfg.Password = Password;
                }
                else
                {
                    cfg.ExecutiveType = "cfdp";
                    cfg.CfdpConfigFile = CfdpConfig;
                    cfg.SatelliteCfdpEntityID = CfdpEntityID;

                }

                if (!await ExecutionService.UpdateExecutiveConfiguration(cfg))
                {
                    NotificationManager?.Show(new Avalonia.Controls.Notifications.Notification("Error", "Unable to save settings"));
                }

                View.Close();
            });
        }

        private async Task RefreshExecutiveSettings()
        {
            var cfg = await ExecutionService.GetExecutiveConfiguration();

            UseScp = cfg.ExecutiveType == "scp";
            UplinkFolder = cfg.DestinationUplinkFolder;
            UserName = cfg.UserName;
            Password = cfg.Password;
            SatelliteAddress = cfg.SatelliteAddress;
            CfdpConfig = cfg.CfdpConfigFile;
            CfdpEntityID = cfg.SatelliteCfdpEntityID;
        }

        public SettingsViewModel(Window view, ICommandExecutionService executionService, INotificationManager notificationManager)
            : this(notificationManager)
        {
            View = view;

            ExecutionService = executionService;

            CommandExecutivePort = ExecutionService.ExecutivePort;

            _ = RefreshExecutiveSettings();
        }

        public string StorageService
        {
            get => m_storage;
            set => this.RaiseAndSetIfChanged(ref m_storage, value);
        }

        public int CommandExecutivePort
        {
            get => m_commandPort;
            set => this.RaiseAndSetIfChanged(ref m_commandPort, value);
        }

        public bool UseScp
        {
            get => m_scp;
            set => this.RaiseAndSetIfChanged(ref m_scp, value);
        }

        public string SatelliteAddress
        {
            get => m_satelliteAddress;
            set => this.RaiseAndSetIfChanged(ref m_satelliteAddress, value);
        }

        public string UplinkFolder
        {
            get => m_uplinkFolder;
            set => this.RaiseAndSetIfChanged(ref m_uplinkFolder, value);
        }

        public string UserName
        {
            get => m_userName;
            set => this.RaiseAndSetIfChanged(ref m_userName, value);
        }

        public string Password
        {
            get => m_password;
            set => this.RaiseAndSetIfChanged(ref m_password, value);
        }

        public string CfdpConfig
        {
            get => m_cfdpConfig;
            set => this.RaiseAndSetIfChanged(ref m_cfdpConfig, value);
        }

        public int CfdpEntityID
        {
            get => m_cfdpEntityID;
            set => this.RaiseAndSetIfChanged(ref m_cfdpEntityID, value);
        }
    }
}
