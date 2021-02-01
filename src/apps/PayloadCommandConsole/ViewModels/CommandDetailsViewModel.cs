using Avalonia.Controls.Notifications;
using Hypergiant.HIVE;
using PayloadCommandConsole.Models;
using ReactiveUI;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PayloadCommandConsole.ViewModels
{
    public class CommandDetailsViewModel : ViewModelBase
    {
        private ObservableCommand m_command;
        private ICommandExecutionService m_executionService;
        private CommandStore m_commandStore;
        private Timer m_payloadApplyTimer;
        private string m_lastPayloadEdit;

        public Guid CommandID => m_command.CommandID;
        public string ID => m_command.CommandID.ToString();
        public string Name => m_command.Name;
        public string State => m_command.CurrentState.ToString();

        public System.Windows.Input.ICommand EditCommand { get; }
        public System.Windows.Input.ICommand SendCommand { get; }
        public System.Windows.Input.ICommand AbandonCommand { get; }
        public System.Windows.Input.ICommand FailCommand { get; }
        public System.Windows.Input.ICommand SucceedCommand { get; }
        public System.Windows.Input.ICommand NotesCommand { get; }

        public CommandDetailsViewModel(ObservableCommand command, INotificationManager notificationManager, ICommandExecutionService executionService, CommandStore commandStore)
            : base(notificationManager)
        {
            m_command = command;
            m_executionService = executionService;
            m_commandStore = commandStore;

            EditCommand = ReactiveCommand.Create(() =>
            {
            });
            SendCommand = ReactiveCommand.Create(async () =>
            {
                try
                {
                    if (!await m_executionService.DeliverCommand(m_command))
                    {
                        //update the UI
                        NotificationManager?.Show(new Avalonia.Controls.Notifications.Notification("Command", "Unable to deliver command"));
                    }
                }
                catch (Exception ex)
                {
                    // TODO: logging
                    NotificationManager?.Show(new Avalonia.Controls.Notifications.Notification("Command", ex.Message));
                }

                await RefreshCommandHistory();
                // todo; refresh history
            });

            AbandonCommand = ReactiveCommand.Create(() =>
            {
                m_command.CurrentState = CommandState.ExecutionFailed;
                this.RaisePropertyChanged(nameof(State));
            });
            FailCommand = ReactiveCommand.Create(() =>
            {
                m_command.CurrentState = CommandState.ExecutionFailed;
                this.RaisePropertyChanged(nameof(State));
            });
            SucceedCommand = ReactiveCommand.Create(() =>
            {
                m_command.CurrentState = CommandState.ExecutionComplete;
                this.RaisePropertyChanged(nameof(State));
            });
            NotesCommand = ReactiveCommand.Create(() =>
            {
            });
        }

        public async Task RefreshCommandHistory()
        {
            await m_commandStore.RefreshCommandHistory(this.CommandID);
            this.RaisePropertyChanged(nameof(Results));
        }

        /*
        private void OnCommandStateChanged(CommandStateData data)
        {
            if (data.CommandID == this.CommandID)
            {
                string s = string.Empty;

                if (data.ExecutionStart.HasValue)
                {
                    s = $"[{data.ExecutionStart:HH:mm:ss}] Execution Started\n";
                }

                foreach (var di in data.Info)
                {
                    s += $"[{(di.Timestamp):HH:mm:ss}] {di.Info}\n";
                }
                if (data.ExecutionEnd.HasValue)
                {
                    var et = data.ExecutionEnd.Value - data.ExecutionStart.Value;

                    s += $"[{data.ExecutionEnd:HH:mm:ss}] Execution Completed after {et:mm\\:ss\\.ff}\n";
                }
                else
                {
                    s += $"[No recorded completion time]\n";
                }

                m_command.Results = s;
                m_command.CurrentState = CommandState.ExecutionComplete;

                this.RaisePropertyChanged(nameof(Results));
                this.RaisePropertyChanged(nameof(State));
            }
        }
        */

        private string Prettify(string json)
        {
            json = json.Replace("\r", string.Empty).Replace("\n", string.Empty);

            var INDENT_STRING = "  ";
            int indentation = 0;
            int quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, --indentation)) + ch : ch.ToString()
                select lineBreak == null
                            ? openChar.Length > 1
                                ? openChar
                                : closeChar
                            : lineBreak;

            return String.Concat(result);
        }

        private string Prettify(CommandHistory history)
        {
            var sb = new StringBuilder($"[{history.Timestamp:MM/dd/yy HH:mm:ss.f}]");
            if (history.NewState.HasValue)
            {
                sb.Append($" State became {history.NewState}:");
            }
            if (!string.IsNullOrEmpty(history.Data))
            {
                sb.Append($" {history.Data}");
            }

            return sb.ToString();
        }

        public string Payload
        {
            get => Prettify(m_command.Payload);
            set
            {
                m_lastPayloadEdit = value;

                // we only want it to "apply" after a period
                if (m_payloadApplyTimer == null)
                {
                    m_payloadApplyTimer = new Timer((o) =>
                    {
                        m_command.Payload = m_lastPayloadEdit;
                    }, null, 1000, Timeout.Infinite);
                }
                else
                {
                    m_payloadApplyTimer.Change(1000, Timeout.Infinite);
                }
            }
        }

        public string Results
        {
            get
            {
                if (!m_commandStore.CommandHistory.ContainsKey(CommandID))
                {
                    return "[No history for Command]";
                }

                return string.Join('\n', m_commandStore.CommandHistory[this.CommandID].Select(h => Prettify(h)));
                switch (m_command.CurrentState)
                {
                    case CommandState.ExecutionComplete:
                    case CommandState.ExecutionFailed:
                        return string.Join('\n', m_command.History);
                    default:
                        return "[Command has not completed execution]";
                }
            }
        }
    }
}
