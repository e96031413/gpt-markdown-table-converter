using CommunityToolkit.Mvvm.ComponentModel;

namespace TextTableConverter.UI.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        private bool _isBusy;
        private string _statusMessage = string.Empty;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        protected async Task ExecuteAsync(Func<Task> action, string busyMessage)
        {
            try
            {
                IsBusy = true;
                StatusMessage = busyMessage;
                await action();
            }
            finally
            {
                IsBusy = false;
                StatusMessage = "就緒";
            }
        }
    }
}
