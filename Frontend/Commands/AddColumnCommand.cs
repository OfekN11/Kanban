using IntroSE.Kanban.Frontend.Model;
using IntroSE.Kanban.Frontend.ViewModel;
using System;
using System.Windows.Input;

namespace IntroSE.Kanban.Frontend.Commands
{

    public class AddColumnCommand : ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var boardViewModel = parameter as BoardViewModel;
            if (boardViewModel.NewColumnName == "")
            {
                boardViewModel.Message = "Enter a name please";
            }
            else if (!int.TryParse(boardViewModel.NewColumnOrdinal, out int result))
            {
                boardViewModel.Message = "Enter a number please";
            }
            else
            {
                try
                {
                    boardViewModel.Board.AddColumn(result, boardViewModel.NewColumnName);
                    boardViewModel.RefreshColumns();
                    boardViewModel.Message = "Column added successfully!";
                }
                catch (Exception e)
                {
                    boardViewModel.Message = e.Message;
                }
            }
        }

    }
}
