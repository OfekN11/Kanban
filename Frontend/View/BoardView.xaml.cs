using IntroSE.Kanban.Frontend.Model;
using IntroSE.Kanban.Frontend.ViewModel;
using System.Windows;

namespace IntroSE.Kanban.Frontend.View
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class Board : Window
    {

        private BoardViewModel viewModel;
        public Board(BoardModel board)
        {
            InitializeComponent();
            this.viewModel = new BoardViewModel(board);
            this.DataContext = viewModel;
        }

        private void Add_Task(object sender, RoutedEventArgs e)
        {
            AddingTaskView newTask = new AddingTaskView(viewModel.Board);
            newTask.Show();
        }

        private void Show_Column(object sender, RoutedEventArgs e)
        {
            ColumnModel columnModel = viewModel.GetSelectedColumn();
            if (columnModel != null)
            {
                ColumnView columnView = new ColumnView(new ColumnViewModel(columnModel));
                columnView.Show();
                this.Close();
            }
        }

        private void Roll_Back(object sender, RoutedEventArgs e)
        {
            UserView userView = new UserView(viewModel.Board.User);
            userView.Show();
            this.Close();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            viewModel.RefreshColumns();
        }

    }
}
