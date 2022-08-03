using IntroSE.Kanban.Frontend.Model;
using System.Windows;
using TaskViewModel = IntroSE.Kanban.Frontend.ViewModel.TaskViewModel;

namespace IntroSE.Kanban.Frontend.View
{
    /// <summary>
    /// Interaction logic for TaskView.xaml
    /// </summary>
    public partial class TaskView : Window
    {
        TaskViewModel taskVM;
        public TaskView(TaskModel task)
        {
            InitializeComponent();
            taskVM = new TaskViewModel(task);
            DataContext = taskVM;
        }
    }
}
