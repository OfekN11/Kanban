using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using IntroSE.Kanban.Backend.ServiceLayer;
using SUser = IntroSE.Kanban.Backend.ServiceLayer.User;
using SBoard = IntroSE.Kanban.Backend.ServiceLayer.Board;
using SColumn = IntroSE.Kanban.Backend.ServiceLayer.Column;
using STask = IntroSE.Kanban.Backend.ServiceLayer.Task;
using System.Data.Entity.Migrations.Model;

namespace IntroSE.Kanban.Frontend.Model
{
    public class BackendController
    {
        private Service Service { get; set; }
        public BackendController(Service service)
        {
            this.Service = service;
        }

        public BackendController()
        {
            this.Service = new Service();
            Service.LoadData();
        }

        public UserModel Login(string username, string password)
        {
            Response<SUser> user = Service.Login(username, password);
            if (user.ErrorOccured)
            {
                throw new Exception(user.ErrorMessage);
            }
            return new UserModel(this, user.Value);
        }

        internal void Register(string username, string password)
        {
            Response res = Service.Register(username, password);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        internal void Logout(UserModel user)
        {
            Service.Logout(user.Email);
        }

        ///<summary>Removes all persistent data.</summary>
        public void DeleteData()
        {
            Service.DeleteData();
        }


        internal void AddColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, string columnName)
        {
            Response res = Service.AddColumn(userEmail, creatorEmail, boardName, columnOrdinal, columnName);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        internal void RenameColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, string newColumnName)
        {
            Response res = Service.RenameColumn(userEmail, creatorEmail, boardName, columnOrdinal, newColumnName);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        internal void MoveColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int shiftSize)
        {
            Response res = Service.MoveColumn(userEmail, creatorEmail, boardName, columnOrdinal, shiftSize);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        internal void RemoveColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            Response res = Service.RemoveColumn(userEmail, creatorEmail, boardName, columnOrdinal);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);



            }
        }

        /*
        internal Task AddTask(string userEmail, string creatorEmail, string boardName, string title, string description, DateTime dueDate)
        {
            Response<STask> res = Service.AddTask(userEmail, creatorEmail, boardName, title, description, dueDate);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
            return new Task(res.Value);
        }
        */
        internal void LimitColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int limit)
        {
            Response res = Service.LimitColumn(userEmail, creatorEmail, boardName, columnOrdinal, limit);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        internal void UpdateTaskDueDate(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, DateTime dueDate)
        {
            Response res = Service.UpdateTaskDueDate(userEmail, creatorEmail, boardName, columnOrdinal, taskId, dueDate);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        internal void UpdateTaskTitle(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string title)
        {
            Response res = Service.UpdateTaskTitle(userEmail, creatorEmail, boardName, columnOrdinal, taskId, title);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        internal void UpdateTaskDescription(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string description)
        {
            Response res = Service.UpdateTaskDescription(userEmail, creatorEmail, boardName, columnOrdinal, taskId, description);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        public void AdvanceTask(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId)
        {
            Response res = Service.AdvanceTask(userEmail, creatorEmail, boardName, columnOrdinal, taskId);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        /*
        internal IList<Task> GetColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            Response<IList<STask>> res = Service.GetColumn(userEmail, creatorEmail, boardName, columnOrdinal);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
            else
            {
                IList<Task> tasks = new List<Task>();
                foreach (STask s_task in res.Value)
                {
                    tasks.Add(new Task(s_task));
                }
                return tasks;
            }
        }
        */

        public BoardModel AddBoard(UserModel user, string newBoardName)
        {
            Response<SBoard> res = (Response<SBoard>)Service.AddBoard(user.Email, newBoardName);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
            else
            {
                return new BoardModel(user, res.Value);
            }
        }

        internal BoardModel JoinBoard(UserModel user, string creatorEmail, string boardName)
        {
            Response<SBoard> res = (Response<SBoard>)Service.JoinBoard(user.Email, creatorEmail, boardName);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
            return new BoardModel(user, res.Value);
        }

        internal void RemoveBoard(string userEmail, string creatorEmail, string boardName)
        {
            Response res = Service.RemoveBoard(userEmail, creatorEmail, boardName);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        internal IList<TaskModel> GetInProgressTasks(string userEmail, BackendController controller)
        {
            Response<IList<STask>> res = Service.InProgressTasks(userEmail);

            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
            else
            {
                IList<TaskModel> tasks = new List<TaskModel>();
                foreach (STask s_task in res.Value)
                {
                    tasks.Add(new TaskModel(s_task));
                }
                return tasks;
            }
        }

        internal void AssignTask(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string emailAssignee)
        {
            Response res = Service.AssignTask(userEmail, creatorEmail, boardName, columnOrdinal, taskId, emailAssignee);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
        }

        internal IList<String> GetBoardNames(string userEmail)
        {
            Response<IList<String>> res = Service.GetBoardNames(userEmail);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
            else
            {
                IList<String> board_names = new List<String>();
                foreach (String name in res.Value)
                {
                    board_names.Add(name);
                }
                return board_names;
            }
        }

        internal TaskModel AddTask(string userEmail, string creatorEmail, string boardName, string title, string description, DateTime dueDate)
        {
            Response<STask> res = Service.AddTask(userEmail, creatorEmail, boardName, title, description, dueDate);
            if (res.ErrorOccured)
            {
                throw new Exception(res.ErrorMessage);
            }
            return new TaskModel(res.Value);
        }

        internal IList<string> GetBetterBoardNames(string email)
        {
            return Service.GetBetterBoardNames(email).Value;
        }

        //-----------------------------------------------------------------------------------------

        internal ObservableCollection<BoardModel> GetBoards(UserModel user)
        {
            Response<IList<string>> boardNamesRes = Service.GetBetterBoardNames(user.Email);
            if (boardNamesRes.ErrorOccured)
            {
                throw new Exception(boardNamesRes.ErrorMessage);
            }
            ObservableCollection<BoardModel> boards = new ObservableCollection<BoardModel>();
            foreach (string boardID in boardNamesRes.Value)
            {
                string[] boardDetails = boardID.Split(':', 2);
                Response<SBoard> boardRes = Service.GetBoard(user.Email, boardDetails[0], boardDetails[1]);
                if (boardRes.ErrorOccured)
                {
                    throw new Exception(boardRes.ErrorMessage);
                }
                boards.Add(new BoardModel(user, boardRes.Value));
            }
            return boards;
        }

        internal List<ColumnModel> GetBoardColumns(BoardModel board)
        {
            List<ColumnModel> columns = new List<ColumnModel>();
            for (int i = 0; i < board.ColumnCount; i++)
            {
                Response<SColumn> columnRes = Service.GetSColumn(board.User.Email, board.CreatorEmail, board.BoardName, i);
                if (columnRes.ErrorOccured)
                {
                    throw new Exception(columnRes.ErrorMessage);
                }
                columns.Add(new ColumnModel(board, columnRes.Value));
            }
            return columns;
        }

        internal List<TaskModel> GetColumnTasks(ColumnModel column)
        {
            List<TaskModel> tasks = new List<TaskModel>();
            Response<IList<STask>> tasksRes = Service.GetColumn(column.User.Email, column.Board.CreatorEmail, column.Board.BoardName, column.ordinal);
            if (tasksRes.ErrorOccured)
            {
                throw new Exception(tasksRes.ErrorMessage);
            }
            foreach (STask sTask in tasksRes.Value)
            {
                tasks.Add(new TaskModel(column, sTask));
            }
            return tasks;
        }
    }
}
