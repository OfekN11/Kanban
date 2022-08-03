using IntroSE.Kanban.Frontend.ViewModel;
using System.ComponentModel;
using System.Windows;

namespace IntroSE.Kanban.Frontend.View
{
    /// <summary>
    /// Interaction logic for ColumnView.xaml
    /// </summary>
    public partial class ColumnView : Window
    {

        private ColumnViewModel viewModel;
        public ColumnView(ColumnViewModel columnViewModel)
        {
            InitializeComponent();
            this.viewModel = columnViewModel;
            this.DataContext = viewModel;
        }

        private void Roll_Back(object sender, RoutedEventArgs e)
        {
            Board boardView = new Board(viewModel.Column.Board);
            boardView.Show();
            this.Close();
        }

        private void Button_Click_Show_Task(object sender, RoutedEventArgs e)
        {
            TaskView taskView = new TaskView(viewModel.GetSelectedTask());
            if (taskView != null)
            {
                taskView.Show();
            }
        }

        private void Button_Click_Sort_by_Due_Date(object sender, RoutedEventArgs e)
        {
            TasksListBox.Items.SortDescriptions.Add(new SortDescription("DueDate", ListSortDirection.Ascending));
        }

    }
}
