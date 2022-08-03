using IntroSE.Kanban.Frontend.Model;
using IntroSE.Kanban.Frontend.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IntroSE.Kanban.Frontend.View
{
    /// <summary>
    /// Interaction logic for InProgressTasksView.xaml
    /// </summary>
    public partial class InProgressTasksView : Window
    {
        InProgressTasksViewModel viewModel;
        public InProgressTasksView(IList<TaskModel>tasks)
        {
            InitializeComponent();
            this.viewModel = new InProgressTasksViewModel(tasks);
            DataContext = viewModel;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
