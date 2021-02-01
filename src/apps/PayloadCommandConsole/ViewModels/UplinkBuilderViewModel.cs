using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using DynamicData;
using Hypergiant.HIVE.Views;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace PayloadCommandConsole.ViewModels
{
    public class UplinkCommand : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private JsonSerializerOptions m_opts;
        private string m_destination;
        private int m_entityID;
        private Timer m_destinationApplyTimer;
        private string m_lastDestination;
        private Timer m_entityApplyTimer;
        private int m_lastEntity;
        private string m_name;

        public UplinkCommand()
        {
            Name = "Uplink";

            m_opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            Files = new ObservableCollection<FileInfo>();
            Files.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(CommandText));
            };
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [JsonIgnore]
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string Command => "uplink";
        [JsonIgnore]
        public string DestinationFolder
        {
            get => m_destination;
            set
            {
                m_lastDestination = value;
                // we only want it to "apply" after a period
                if (m_destinationApplyTimer == null)
                {
                    m_destinationApplyTimer = new Timer((o) =>
                    {
                        m_destination = m_lastDestination;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DestinationFolder)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandText)));
                    }, null, 1000, Timeout.Infinite);
                }
                else
                {
                    m_destinationApplyTimer.Change(1000, Timeout.Infinite);
                }
            }
        }

        public int RemoteEntityID
        {
            get => m_entityID;
            set
            {
                m_lastEntity = value;
                // we only want it to "apply" after a period
                if (m_entityApplyTimer == null)
                {
                    m_entityApplyTimer = new Timer((o) =>
                    {
                        m_entityID = m_lastEntity;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemoteEntityID)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandText)));
                    }, null, 1000, Timeout.Infinite);
                }
                else
                {
                    m_entityApplyTimer.Change(1000, Timeout.Infinite);
                }
            }
        }

        public ObservableCollection<FileInfo> Files { get; }

        [JsonIgnore]
        public string CommandText
        {
            get => JsonSerializer.Serialize(this, m_opts);
        }
    }

    public class FileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private UplinkCommand m_parent;
        private string m_name;

        private Window View { get; }

        [JsonIgnore]
        public System.Windows.Input.ICommand RemoveCommand { get; }
        [JsonIgnore]
        public System.Windows.Input.ICommand RenameCommand { get; }

        public FileInfo(Window view, UplinkCommand parent)
        {
            View = view;
            m_parent = parent;

            RemoveCommand = ReactiveCommand.Create(async () =>
            {
                var msgbox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ButtonDefinitions = ButtonEnum.YesNo,
                    ContentMessage = "Are you sure you want to remove this file?",
                    Icon = Icon.None,
                    Style = Style.RoundButtons
                });
                var result = await msgbox.Show();

                if (result == ButtonResult.Yes)
                {
                    m_parent.Files.Remove(this);
                }
            });
            RenameCommand = ReactiveCommand.Create(async () =>
            {
                var ib = new InputBox("Rename Destination File", this.Name);
                var result = await ib.ShowDialog<string>(View);
                if (!string.IsNullOrEmpty(result))
                {
                    this.Name = result;
                }
            });
        }

        [JsonIgnore]
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                m_parent.RaisePropertyChanged(nameof(m_parent.CommandText));
            }
        }

        public string RemoteName
        {
            get => Path.Combine(m_parent.DestinationFolder ?? string.Empty, Name);
        }
        public string Encoding { get; set; }
        public string Contents { get; set; }
    }

    public class UplinkBuilderViewModel : ViewModelBase
    {
        private UplinkCommand CurrentCommand { get; } = new UplinkCommand();
        private Window View { get; }

        public System.Windows.Input.ICommand AddFileCommand { get; }
        public System.Windows.Input.ICommand BuildCommand { get; }
        public System.Windows.Input.ICommand CancelCommand { get; }

        public UplinkBuilderViewModel(Window view, INotificationManager notificationManager)
            : base(notificationManager)
        {
            View = view;

            AddFileCommand = ReactiveCommand.Create(async () =>
            {
                await AddFile();
            });
            BuildCommand = ReactiveCommand.Create(() =>
            {
                if (CurrentCommand.Files.Count > 0)
                {
                    view.Close(CurrentCommand);
                }
                else
                {
                    view.Close(null);
                }
            });
            CancelCommand = ReactiveCommand.Create(() =>
            {
                view.Close(null);
            });
        }

        private async Task AddFile()
        {
            var ofd = new OpenFileDialog
            {
                AllowMultiple = true,
                Title = "Import Command File",
            };

            var result = await ofd.ShowAsync(View);
            if (result != null && result.Length > 0)
            {
                foreach (var f in result)
                {
                    if (File.Exists(f))
                    {
                        var bytes = File.ReadAllBytes(f);
                        var fi = new FileInfo(View, CurrentCommand)
                        {
                            Name = Path.GetFileName(f),
                            Encoding = "Base64",
                            Contents = Convert.ToBase64String(bytes)
                        };
                        fi.PropertyChanged += (s, p) =>
                        {
                            this.RaisePropertyChanged(nameof(CurrentCommand));
                            this.RaisePropertyChanged(nameof(CurrentCommand.Files));
                        };
                        CurrentCommand.Files.Add(fi);
                    }
                }
            }

            this.RaisePropertyChanged(nameof(CurrentCommand));
            this.RaisePropertyChanged(nameof(CurrentCommand.Files));
        }

        public UplinkBuilderViewModel()
            : base(null)
        {
            CurrentCommand.DestinationFolder = @"C:\Temp\hive2";

            CurrentCommand.Files.AddRange(new FileInfo[]
                {
                new FileInfo(View, CurrentCommand)
                {
                    Name = "foo.txt",
                    Encoding = "ASCII",
                    Contents = "stuff goes here"
                },
                new FileInfo(View, CurrentCommand)
                {
                    Name = "bar.txt",
                    Encoding = "UTF-8",
                    Contents = "more stuff goes here"
                }
                });
        }
    }
}
