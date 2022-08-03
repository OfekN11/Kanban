using IntroSE.Kanban.Frontend.Model;
using IntroSE.Kanban.Frontend.ViewModel;
using System;
using System.Windows.Input;

namespace IntroSE.Kanban.Frontend.Commands
{
    public class AdvanceTaskCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var columnViewModel = parameter as ColumnViewModel;
            ColumnModel columnModel = columnViewModel.Column;
            try
            {
                columnViewModel.Column.AdvanceTask(columnViewModel.SelectedTask.ID);
                columnViewModel.RefreshTasks();
                columnViewModel.Message = "Advanced Task successfully!";
            }
            catch (Exception e)
            {
                columnViewModel.Message = "Failed to advance task. " + e.Message;
            }

        }
    }
}