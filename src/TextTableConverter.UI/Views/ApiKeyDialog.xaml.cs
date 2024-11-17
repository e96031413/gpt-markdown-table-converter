using System.Windows;

namespace TextTableConverter.UI.Views
{
    public partial class ApiKeyDialog : Window
    {
        public string ApiKey { get; private set; } = string.Empty;

        public ApiKeyDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ApiKey = ApiKeyBox.Password;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
