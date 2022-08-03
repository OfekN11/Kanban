using System;
using System.Windows;
using System.Windows.Media;
using STask = IntroSE.Kanban.Backend.ServiceLayer.Task;

namespace IntroSE.Kanban.Frontend.Model
{
    public class TaskModel : NotifiableObject
    {
        private UserModel _user;
        private BoardModel _board;
        private ColumnModel _column;

        private int _id;
        private DateTime _creationTime;
        private string _title;
        private string _description;
        private DateTime _dueDate;
        private string _emailAssignee;

        public UserModel User { get => _user; }
        public BoardModel Board { get => _board; }
        public ColumnModel Column { get => _column; }

        public int ID { get => _id; }

        public DateTime CreationTime { get => _creationTime; }

        public string Title
        {
            get => _title;
            set
            {
                try
                {
                    User.Controller.UpdateTaskTitle(User.Email, Board.CreatorEmail, Board.BoardName, Column.ordinal, ID, value);
                    _title = value;
                    MessageBox.Show("Title changed successfully!");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to change title. " + e.Message);
                }
                RaisePropertyChanged("Title");
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                try
                {
                    User.Controller.UpdateTaskDescription(User.Email, Board.CreatorEmail, Board.BoardName, Column.ordinal, ID, value);
                    _description = value;
                    MessageBox.Show("Description changed successfully");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cannot change description. " + e.Message);
                }
                RaisePropertyChanged("Description");
            }

        }
        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                try
                {
                    User.Controller.UpdateTaskDueDate(User.Email, Board.CreatorEmail, Board.BoardName, Column.ordinal, ID, value);
                    _dueDate = value;
                    MessageBox.Show("Due date changed successfully!");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cannot change due date. " + e.Message);
                }
                RaisePropertyChanged("DueDate");
                RaisePropertyChanged("DueDateBackgroundColor");
            }
        }
        public string EmailAssignee
        {
            get => _emailAssignee;
            set
            {
                try
                {
                    User.Controller.AssignTask(User.Email, Board.CreatorEmail, Board.BoardName, Column.ordinal, ID, value);
                    _emailAssignee = value;
                    MessageBox.Show("Assignee changed successfully!");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cannot change Assignee. " + e.Message);
                }
                RaisePropertyChanged("EmailAssignee");
                RaisePropertyChanged("AssigneeBorderColor");
            }
        }

        public SolidColorBrush DueDateBackgroundColor
        {
            get
            {
                Color c;
                if (DateTime.Now >= DueDate)
                    c = Colors.Red;
                else if( (DateTime.Now - CreationTime) >= 0.75 * (DueDate - CreationTime))
                    c = Colors.Orange;
                else
                    c = Colors.White;
                return new SolidColorBrush(c);
            }
        }

        public SolidColorBrush AssigneeBorderColor
        {
            get
            {
                Color c;
                if (EmailAssignee == User.Email)
                    c = Colors.Blue;
                else
                    c = Colors.Transparent;
                return new SolidColorBrush(c);
            }
        }

        public TaskModel(ColumnModel column, STask sTask)
        {
            this._user = column.User;
            this._board = column.Board;
            this._column = column;
            this._id = sTask.Id;
            this._creationTime = sTask.CreationTime;
            this._title = sTask.Title;
            this._description = sTask.Description;
            this._dueDate = sTask.DueDate;
            this._emailAssignee = sTask.emailAssignee;
        }

        public TaskModel(STask sTask)
        {
            this._id = sTask.Id;
            this._creationTime = sTask.CreationTime;
            this._title = sTask.Title;
            this._description = sTask.Description;
            this._dueDate = sTask.DueDate;
            this._emailAssignee = sTask.emailAssignee;
        }
    }
}
