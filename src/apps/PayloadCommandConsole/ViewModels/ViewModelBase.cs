using Avalonia.Controls.Notifications;
using ReactiveUI;

namespace PayloadCommandConsole.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected INotificationManager NotificationManager { get; }

        protected ViewModelBase(INotificationManager notificationManager)
        {
            NotificationManager = notificationManager;
        }
    }
}
