using System;
using System.Collections.Generic;
using SBoard = IntroSE.Kanban.Backend.ServiceLayer.Board;

namespace IntroSE.Kanban.Frontend.Model
{
    public class BoardModel : NotifiableObject
    {
        private UserModel _user;

        private string _boardName;
        private string _creatorEmail;
        private int _columnCount;
        private int _taskCount;

        public UserModel User { get => _user; set => _user = value; }

        public string BoardName { get => _boardName; set => _boardName = value; }
        public string CreatorEmail { get => _creatorEmail; set => _creatorEmail = value; }
        public int ColumnCount { get => _columnCount; set => _columnCount = value; }
        public int TaskCount { get => _taskCount; set => _taskCount = value; }

        public string FullName { get => CreatorEmail + " : " + BoardName; }


        // constructor
        public BoardModel(UserModel user, SBoard sBoard)
        {
            this.User = user;
            this.CreatorEmail = sBoard.CreatorEmail;
            this._boardName = sBoard.Name;
            this._columnCount = sBoard.ColumnCount;
            this._taskCount = sBoard.TaskCount;
        }

        // methods

        public List<ColumnModel> GetColumn()
        {
            return User.Controller.GetBoardColumns(this);
        }

        public void AddColumn(int newColumnOrdinal, string newColumnName)
        {
            User.Controller.AddColumn(User.Email, CreatorEmail, BoardName, newColumnOrdinal, newColumnName);
            ColumnCount++;
        }

        public void DeleteColumn(int columnOrdinal)
        {
            User.Controller.RemoveColumn(User.Email, CreatorEmail, BoardName, columnOrdinal);
            ColumnCount--;
        }

        public void AddTask(string title, string description, DateTime dueDate)
        {
            User.Controller.AddTask(User.Email, CreatorEmail, BoardName, title, description, dueDate);
        }
    }
}
