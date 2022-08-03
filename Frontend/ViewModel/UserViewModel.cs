using IntroSE.Kanban.Frontend.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IntroSE.Kanban.Frontend.ViewModel
{

    class UserViewModel : NotifiableObject
    {
        private UserModel _user;
        private string _message;
        private BoardModel _selectedBoard;
        private string _joinBoardCreator;
        private string _joinBoardName;
        private string _newBoardName;
        private ObservableCollection<BoardModel> _boards;
        public UserModel User { get => _user; private set { _user = value; } }
        public string Title { get => "Boards for " + User.Email; }

        

        public BoardModel SelectedBoard { 
            get=>_selectedBoard; 
            set 
            {
                _selectedBoard = value;
            } 
        }

        public string JoinBoardCreator 
        { 
            get=>_joinBoardCreator;
            set 
            {
                _joinBoardCreator = value;
            }
        }

        internal void Logout()
        {
            User.Controller.Logout(User);
        }

        public string JoinBoardName
        {
            get => _joinBoardName;
            set
            {
                _joinBoardName = value;
            }
        }

        public string NewBoardName
        {
            get => _newBoardName;
            set
            {
                _newBoardName = value;

            }
        }


        public string Message { get=>_message; 
            set { _message= value;
                RaisePropertyChanged("Message"); } }
   

        internal void Join_Board()
        {
            Message = "";
            try
            {
                BoardModel joindBoard = User.Controller.JoinBoard(User, JoinBoardCreator, JoinBoardName);
                Message = "You Joined the board " + JoinBoardName + " Successfully";
                Boards.Add(joindBoard);
            }
            catch(Exception e)
            {
                Message = "You Failed to join Board " + JoinBoardName + " because " + e.Message;
            }
        }

        internal void CreateBoard()
        {
            Message = "";
            try
            {
                BoardModel board = User.Controller.AddBoard(User, NewBoardName);
                Message = $"You Created the board {NewBoardName} successfully";
                Boards.Add(board);
            }
            catch (Exception e)
            {
                Message = "You Failed to create Board " + NewBoardName + " because " + e.Message;
            }
        }
        internal BoardModel OpenBoard()
        {

            return SelectedBoard;
        }

        public  IList<TaskModel> GetInProggress()
        {
            IList<TaskModel> tasks= User.Controller.GetInProgressTasks(User.Email, User.Controller);

            return tasks;
        }

        public ObservableCollection<BoardModel> Boards { get => _boards; private set => _boards=value; }

        public UserViewModel(UserModel user)
        {
            User = user;
            Boards = User.GetBoards();
            Boards.CollectionChanged += HandleBoardsChange;
        }

        private void HandleBoardsChange(object sender, NotifyCollectionChangedEventArgs e)
        { 
            RaisePropertyChanged("Boards");
        }

    }
}
