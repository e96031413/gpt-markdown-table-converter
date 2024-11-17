using System.Windows;
using TextTableConverter.UI.ViewModels;

namespace TextTableConverter.UI.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            Loaded += SettingsWindow_Loaded;
            ApiKeyBox.PasswordChanged += ApiKeyBox_PasswordChanged;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApiKeyBox.Password = _viewModel.ApiKey;
        }

        private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.ApiKey = ApiKeyBox.Password;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.ApiKey))
            {
                if (ApiKeyBox.Password != _viewModel.ApiKey)
                {
                    ApiKeyBox.Password = _viewModel.ApiKey;
                }
            }
        }
    }
}
