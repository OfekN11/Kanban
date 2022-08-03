using IntroSE.Kanban.Frontend.Model;
using System;

namespace IntroSE.Kanban.Frontend.ViewModel
{
    public class AddingTaskViewModel : NotifiableObject
    {
        private BoardModel _board;

        private string _title;
        private string _description;
        private DateTime _dueDate;

        private string _message;

        public BoardModel Board { get => _board; }

        public string Title { get => _title; set => _title = value; }
        public string Description { get => _description; set => _description = value; }
        public DateTime DueDate { get => _dueDate; set => _dueDate = value; }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        public AddingTaskViewModel(BoardModel boardModel)
        {
            this._board = boardModel;
        }

        public void AddTask()
        {
            try
            {
                Board.AddTask(Title, Description, DueDate);
                Message = $"Task '{Title}' has added successfully!";
            }
            catch (Exception e)
            {
                Message = e.Message;
            }
        }
    }
}
