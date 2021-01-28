using Hypergiant.HIVE;
using PayloadCommandConsole.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace PayloadCommandConsole.ViewModels
{
    public class PassDetailsViewModel : ReactiveObject
    {
        private ObservablePass m_pass;

        private CommandStore Store { get; }

        public System.Windows.Input.ICommand AosCommand { get; }
        public System.Windows.Input.ICommand GoCmdCommand { get; }
        public System.Windows.Input.ICommand LosCommand { get; }
        public System.Windows.Input.ICommand NoteCommand { get; }

        public PassDetailsViewModel(CommandStore store, ObservablePass pass)
        {
            Store = store;
            m_pass = pass;

            AosCommand = ReactiveCommand.Create(async () =>
            {
                var now = DateTime.Now.ToUniversalTime();
                ActualAos = now;
                await Store.RecordPassHistoryRecord(m_pass.PassID, now, PassAction.AOS);
            });
            GoCmdCommand = ReactiveCommand.Create(async () =>
            {
                var now = DateTime.Now.ToUniversalTime();
                ActualGoCmd = now;
                await Store.RecordPassHistoryRecord(m_pass.PassID, now, PassAction.GoForCommand, "Go for Command");
            });
            LosCommand = ReactiveCommand.Create(async () =>
            {
                var now = DateTime.Now.ToUniversalTime();
                ActualLos = now;
                await Store.RecordPassHistoryRecord(m_pass.PassID, now, PassAction.LOS);
            });
            NoteCommand = ReactiveCommand.Create(async () =>
            {
                var now = DateTime.Now.ToUniversalTime();
                var note = $"Note {Notes.Count + 1}";

                await Store.RecordPassHistoryRecord(m_pass.PassID, now, PassAction.Note, "Note", note);

                Notes.Add(note);

                this.RaisePropertyChanged(nameof(HasNotes));
            });
        }

        public Guid PassID => m_pass.PassID;

        public DateTime ScheduledAos => m_pass.ScheduledAos;
        public DateTime? ScheduledLos => m_pass.ScheduledLos;
        public TimeSpan? ExpectedDuration => ScheduledLos.HasValue ? (TimeSpan?)(ScheduledLos.Value - ScheduledAos) : null;
        public decimal? ExpectedElevation => m_pass.ExpectedElevation;
        public string StationName => m_pass.StationName;

        private ObservableCollection<string> Notes => m_pass.Notes;

        public bool HasActualAos => ActualAos.HasValue;
        public bool HasActualGoCmd => ActualGoCmd.HasValue;
        public bool HasActualLos => ActualLos.HasValue;
        public bool HasNotes => Notes.Count > 0;

        public DateTime? ActualAos
        {
            get => m_pass.ActualAos;
            set
            {
                m_pass.ActualAos = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(HasActualAos));
            }
        }

        public DateTime? ActualGoCmd
        {
            get => m_pass.ActualGoCmd;
            set
            {
                m_pass.ActualGoCmd = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(HasActualGoCmd));
            }
        }

        public DateTime? ActualLos
        {
            get => m_pass.ActualLos;
            set
            {
                m_pass.ActualLos = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(HasActualLos));
            }
        }
    }
}
