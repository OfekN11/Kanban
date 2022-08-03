using System;
using System.Collections.Generic;
using System.Windows;
using SColumn = IntroSE.Kanban.Backend.ServiceLayer.Column;

namespace IntroSE.Kanban.Frontend.Model
{
    public class ColumnModel : NotifiableObject
    {
        private UserModel _user;
        private BoardModel _board;

        private string _name;
        public int ordinal { get; private set; }
        private int _limit;

        public UserModel User { get => _user; }
        public BoardModel Board { get => _board; }


        public string Name
        {
            get => _name;
            set
            {
                try
                {
                    User.Controller.RenameColumn(User.Email, Board.CreatorEmail, Board.BoardName, ordinal, value);
                    _name = value;
                    MessageBox.Show("Name changed successfully!");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cannot change name. " + e.Message);
                }
                RaisePropertyChanged("Name");
            }
        }

        public string Ordinal
        {
            get => ordinal.ToString();
            set
            {
                if (!int.TryParse(value, out int result))
                {
                    MessageBox.Show("Enter a number please");
                }
                else
                {
                    try
                    {
                        User.Controller.MoveColumn(User.Email, Board.CreatorEmail, Board.BoardName, ordinal, result - ordinal);
                        ordinal = result;
                        MessageBox.Show("Ordinal changed successfully!");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Cannot change ordinal. " + e.Message);
                    }
                    RaisePropertyChanged("Ordinal");
                }

            }
        }

        public string Limit
        {
            get
            {
                if (_limit == -1)
                    return "unlimited";
                else
                    return _limit.ToString();
            }
            set
            {
                if (value == "unlimited")
                    value = "-1";
                if (!int.TryParse(value, out int result))
                {
                    MessageBox.Show("Enter a number please");
                }
                else
                {
                    try
                    {
                        User.Controller.LimitColumn(User.Email, Board.CreatorEmail, Board.BoardName, ordinal, result);
                        _limit = result;
                        MessageBox.Show("Tasks limit changed successfully!");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Cannot change limit. " + e.Message);
                    }
                }
                RaisePropertyChanged("Limit");
            }
        }

        public ColumnModel(BoardModel board, SColumn sColumn)
        {
            this._user = board.User;
            this._board = board;
            this._name = sColumn.Name;
            this.ordinal = sColumn.Ordinal;
            this._limit = sColumn.Limit;
        }

        // methods

        public List<TaskModel> GetTasks()
        {
            return User.Controller.GetColumnTasks(this);
        }

        public void AdvanceTask(int taskID)
        {
            User.Controller.AdvanceTask(User.Email, Board.CreatorEmail, Board.BoardName, ordinal, taskID);
        }

    }
}
