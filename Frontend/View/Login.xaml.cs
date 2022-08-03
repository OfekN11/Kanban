using IntroSE.Kanban.Frontend.ViewModel;
using IntroSE.Kanban.Frontend.Model;
using System.Windows;


namespace IntroSE.Kanban.Frontend.View
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private LoginViewModel viewModel;
        public Login()
        {
            InitializeComponent();
            this.viewModel = new LoginViewModel(new BackendController());
            this.DataContext = viewModel;
        }

        

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            UserModel user = viewModel.Login();
            if(user != null)
            {
                UserView boardView = new UserView(user);
                boardView.Show();
                this.Close();
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
             viewModel.Register();
        }
    }
}
