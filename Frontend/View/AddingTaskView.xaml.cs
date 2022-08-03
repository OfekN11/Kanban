using IntroSE.Kanban.Frontend.Model;
using IntroSE.Kanban.Frontend.ViewModel;
using System.Windows;
namespace IntroSE.Kanban.Frontend.View
{
    /// <summary>
    /// Interaction logic for AddingNewTask.xaml
    /// </summary>
    public partial class AddingTaskView : Window
    {
        private AddingTaskViewModel viewModel;
        public AddingTaskView(BoardModel board)
        
        {
            InitializeComponent();
            viewModel = new AddingTaskViewModel(board);
            DataContext = viewModel;

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Add_Task_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddTask();
        }
    }
}
