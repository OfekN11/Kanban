using IntroSE.Kanban.Frontend.Model;
using IntroSE.Kanban.Frontend.ViewModel;
using System.Collections.Generic;
using System.Windows;
using BoardWin = IntroSE.Kanban.Frontend.View.Board;

namespace IntroSE.Kanban.Frontend.View
{
    /// <summary>
    /// Interaction logic for UserView.xaml
    /// </summary>
    public partial class UserView : Window
    {
        private UserViewModel viewModel;
        public UserView(UserModel user )
        {
            InitializeComponent();
            this.viewModel = new UserViewModel(user);
            DataContext = viewModel;

        }

        private void Select_Board_Click(object sender, RoutedEventArgs e)
        {
            BoardModel board = viewModel.OpenBoard();
            if (board != null)
            {
                BoardWin boardWin = new BoardWin(board);
                boardWin.Show();
                this.Close();
            }
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Logout();
            Login loginWin = new Login();
            loginWin.Show();
            this.Close();
        }

        private void Join_Board_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Join_Board();
        }

        private void Create_Board_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CreateBoard();
        }


        private void In_Progress_Tasks_Click(object sender, RoutedEventArgs e)
        {
            IList<TaskModel> tasks = viewModel.GetInProggress();
            InProgressTasksView inProgress = new InProgressTasksView(tasks);
            inProgress.Show();

        }
    }
}