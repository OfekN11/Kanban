using System;
using System.Collections.Generic;
using log4net;
using log4net.Config;
using System.Reflection;
using System.IO;
using DBC = IntroSE.Kanban.Backend.DataLayer.DBoardController;
using DBoard = IntroSE.Kanban.Backend.DataLayer.DBoard;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// A collection of all the boards in the system. Manages operationg within specific boards and general board related checks
    /// </summary>
    /// <remarks>in methods requesting columnOrdinal - the integer will represent 1 out of the 3 columns: 0 - Backlog, 1 - In Progress, 2 - Done</remarksY>
    internal class BoardController
    {
        //fields
        private Dictionary<string, Dictionary<string, IBoard>> boards; //first key is userEmail , second key will be the board name
        private Dictionary<string, HashSet<string>> userBoards; //first key is userEmail, second key is a set of all the boards he is a member of
        private LoginInstance loginInstance;
        private DBC dBoardController = new DBC(); //parallel DController

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //constructors
        internal BoardController(LoginInstance loginInstance)
        {
            boards = new Dictionary<string, Dictionary<string, IBoard>>();
            userBoards = new Dictionary<string, HashSet<string>>();
            this.loginInstance = loginInstance;
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            log.Info("Kanban.app booted");
        }

        //methods

        /// <summary>
        /// Loads all Boards and memberships from DAL
        /// </summary>
        /// <exception cref="Exception">Thrown exception if some boards couldn't be loaded</exception>
        internal void LoadData()
        {
            string errorMsg = null;
            
            IList<DBoard> dBoards = null;
            try
            {
                dBoards = dBoardController.Select();
            }
            catch (Exception e)
            {
                log.Fatal($"FAILED to load data - {e.Message}");
                throw new Exception(e.Message);
            }

            foreach (DBoard dBoard in dBoards) 
            {
                string creatorEmail = dBoard.CreatorEmail;
                string boardName = dBoard.BoardName;

                //load the board
                if (!boards.ContainsKey(creatorEmail))
                {
                    boards[creatorEmail] = new Dictionary<string, IBoard>();
                    boards[creatorEmail][boardName] = new Board(dBoard);
                }
                else if (boards[creatorEmail].ContainsKey(boardName))
                {
                    log.Fatal($"FAILED to load board '{creatorEmail}:{boardName}' - board already exists");
                    errorMsg = errorMsg + $"Couldn't load board '{creatorEmail}:{boardName}' - board already exists\n";
                }
                else
                {
                    boards[creatorEmail][boardName] = new Board(dBoard);
                }

                //load board's members
                foreach (string member in dBoard.Members)
                {
                    if (!userBoards.ContainsKey(member))
                    {
                        userBoards[member] = new HashSet<string>();
                    }
                    userBoards[member].Add($"{creatorEmail}:{boardName}");
                }
            }

            if (errorMsg != null)
                throw new Exception(errorMsg);
        }

        /// <summary>
        /// Deletes all board data
        /// </summary>
        internal void DeleteData()
        {
            boards = new Dictionary<string, Dictionary<string, IBoard>>();
            userBoards = new Dictionary<string, HashSet<string>>();
            try
            {
                dBoardController.DeleteAll();
            }
            catch (Exception e)
            {
                log.Fatal($"FAILED to delete board data - {e.Message}");
                throw new Exception("Failed to delete board data");
            }
        }

        /// <summary>
        /// Returns the list of board of a user. The user must be logged-in. The function returns all the board names the user created or joined.
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <returns>IList containinging all the board which the user is a member of</returns>
        /// <remarks>call ValidateLogin, CheckBoardExistance</remarks>
        internal IList<string> GetBoardNames(string userEmail)
        {
            ValidateLogin(userEmail, $"GetBoardNames({userEmail})");
            List<string> boards = new List<string>();
            if (userBoards.ContainsKey(userEmail))
            {
                foreach (string board in userBoards[userEmail])
                {
                    string[] boardDetails = board.Split(':', 2);
                    if (CheckBoardExistance(boardDetails[0], boardDetails[1]))
                    {
                        boards.Add(boardDetails[1]);
                    }
                    else
                    {
                        userBoards[userEmail].Remove(board);
                    }
                }
            }
            return boards;
        }

        /// <summary>
        /// Returns the list of boards of a user. The user must be logged-in. The function returns all the board names the user created or joined.
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <returns>IList containinging all the board which the user is a member of</returns>
        /// <remarks>call ValidateLogin, CheckBoardExistance</remarks>
        internal IList<string> GetBetterBoardNames(string userEmail)
        {
            ValidateLogin(userEmail, $"GetBoardNames({userEmail})");
            List<string> boards = new List<string>();
            if (userBoards.ContainsKey(userEmail))
            {
                foreach (string board in userBoards[userEmail])
                {
                    string[] boardDetails = board.Split(':', 2);
                    if (CheckBoardExistance(boardDetails[0], boardDetails[1]))
                    {
                        boards.Add($"{boardDetails[0]}:{boardDetails[1]}");
                    }
                    else
                    {
                        userBoards[userEmail].Remove(board);
                    }
                }
            }
            return boards;
        }

        /// <summary>
        /// adds a new board for a user
        /// </summary>
        /// <param name="userEmail">the calling user's email</param>
        /// <param name="boardName">the new board name</param>
        /// <exception cref="ArgumentException">thrown when the board already exists for that user</exception>
        /// <returns>the newly created Board</returns>
        /// <remarks>call ValidateLogin</remarks>
        internal IBoard AddBoard(string userEmail, string boardName)
        {
            ValidateLogin(userEmail, $"AddBoard({userEmail}, {boardName})");
            IBoard board = CreateBoard(userEmail, boardName);
            AddBoard(board);
            JoinBoard(userEmail, userEmail, boardName);
            try
            {
                board.Persist();
            }
            catch (InvalidOperationException)
            {
                boards[userEmail].Remove(boardName);
                log.Warn($"FAILED to create board '{userEmail}:{boardName}' - exists in DataBase but not in BusinessLayer");
                throw new Exception($"Can't create board '{userEmail}:{boardName}' - this board already exists in the DataBase, please LoadData before continueing");
            }
            catch (Exception)
            {
                boards[userEmail].Remove(boardName);
                log.Fatal($"FAILED to persist board '{board.Creator}:{board.Name}'");
                throw new Exception("WARNING: Board was created but couldn't be saved!\nPlease restart the program and try again!");
            }
            log.Info($"SUCCESSFULLY created '{userEmail}:{boardName}'");
            return boards[userEmail][boardName];
        }

        /// <summary>
        /// Adds new Board
        /// </summary>
        /// <param name="board">new board to add</param>
        void AddBoard(IBoard board)
        {
            if (!boards.ContainsKey(board.Creator)) //create new entry if needed
            {
                boards[board.Creator] = new Dictionary<string, IBoard>();
                boards[board.Creator][board.Name] = board;
            }
            else
            {
                if (boards[board.Creator].ContainsKey(board.Name))
                {
                    log.Warn($"FAILED to create board '{board.Creator}:{board.Name}' - already exists");
                    throw new ArgumentException($"Board '{board.Creator}:{board.Name}' already exist");
                }
                boards[board.Creator][board.Name] = board;
            }
        }

        /// <summary>
        /// Signs the user to an existing board
        /// </summary>
        /// <param name="userEmail">the calling user's email</param>
        /// <param name="creatorEmail">board's creator - idetifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <returns>The joined board</returns>
        /// <remarks>calls ValidateLogin</remarks>
        internal IBoard JoinBoard(string userEmail, string creatorEmail, string boardName)
        {
            ValidateLogin(userEmail, $"JoinBoard({userEmail}, {creatorEmail}, {boardName})");
            if (!CheckBoardExistance(creatorEmail, boardName)) //check board existance
            {
                log.Info($"FAILED to sign'{userEmail}' to '{creatorEmail}:{boardName}' - board doesn't exist");
                throw new ArgumentException($"Can't sign '{userEmail}' to '{creatorEmail}:{boardName}' - board doesn't exist");
            }
            if (!userBoards.ContainsKey(userEmail)) //create new entry if needed
            {
                userBoards[userEmail] = new HashSet<string>();
            }
            if (userBoards[userEmail].Contains($"{creatorEmail}:{boardName}")) //check if already member
            {
                throw new Exception($"'{userEmail}' already memeber of board '{creatorEmail}:{boardName}'");
            }
            userBoards[userEmail].Add($"{creatorEmail}:{boardName}");
            boards[creatorEmail][boardName].AddMember(userEmail);
            log.Info($"SUCCESSFULLY signed '{userEmail}' to '{creatorEmail}:{boardName}'");
            return boards[creatorEmail][boardName];
        }

        /// <summary>
        /// deletes an existing board
        /// </summary>
        /// <param name="userEmail">the calling user's email</param>
        /// <param name="boardName">the deleted board's name</param>
        /// <exception cref="ArgumentException">thrown when trying to delete a non-existing board</exception>
        /// <remarks>calls ValidateLogin, CheckBoardExistance</remarks>
        internal IBoard RemoveBoard(string userEmail, string creatorEmail, string boardName)
        {
            ValidateLogin(userEmail, $"RemoveBoard({userEmail}, {creatorEmail}, {boardName})");
            if (!userEmail.Equals(creatorEmail))
            {
                log.Warn($"OUT OF DOMAIN OPERATION: User '{loginInstance.ConnectedEmail}' attempted 'RemoveBoard({userEmail}, {creatorEmail}, {boardName})'");
                throw new InvalidOperationException("Can't remove boards you that wasn't created by you");
            }
            else if (CheckBoardExistance(creatorEmail, boardName)) {
                try
                {
                    dBoardController.DeleteBoard(creatorEmail,boardName);
                }
                catch (Exception e)
                {
                    log.Fatal($"FAILED to delete board '{creatorEmail}:{boardName}' from db - {e.Message}");
                }
                IBoard toRemove = boards[creatorEmail][boardName];
                boards[creatorEmail].Remove(boardName);
                log.Info($"SUCCESSFULLY removed '{creatorEmail}:{boardName}'");
                return toRemove;
            }
            else
            {
                log.Info($"FAILED to remove '{creatorEmail}:{boardName}' - board doesn't exist");
                throw new ArgumentException($"Can't remove '{creatorEmail}:{boardName}' - board doesn't exists");
            }
        }

        /// <summary>
        /// Adds a new column to given board
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">The location of the new column. Location for old columns with index>=columnOrdinal is increased by 1 (moved right). </param>
        /// <param name="columnName">The name for the new columns</param>        
        /// <returns>The newly added Column</returns>
        internal IColumn AddColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, string columnName)
        {
            ValidateLogin(userEmail, $"AddColumn({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal}, {columnName})");
            CheckMembership(userEmail, creatorEmail, boardName, "AddColumn");
            IBoard board = boards[creatorEmail][boardName];
            IColumn column = CreateColumn(columnName, creatorEmail, boardName, columnOrdinal);
            try
            {
                board.AddColumn(column);
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to add column at '{creatorEmail}:{boardName}[{columnOrdinal}]' - out of range 0 - {e.Message} (inclusive)");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
            try
            {
                column.Persist();
            }
            catch (Exception)
            {
                board.RemoveColumn(columnOrdinal);
                log.Fatal($"FAILED to persist column '{board.Creator}:{board.Name}[{column.Name}]'");
                throw new Exception("WARNING: Column was created but couldn't be saved!\nPlease restart the program and try again!");
            }
            log.Info($"SUCCESSFULLY added column '{column.Name}' to '{creatorEmail}:{boardName}' by '{userEmail}'");
            return column;
        }

        /// <summary>
        /// Renames a specific column
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">The column location. </param>
        /// <param name="newColumnName">The new column name</param>        
        internal void RenameColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, string newColumnName)
        {
            ValidateLogin(userEmail, $"RenameColumn({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal}, {newColumnName})");
            CheckMembership(userEmail, creatorEmail, boardName, "RenameColumn");
            try
            {
                boards[creatorEmail][boardName].RenameColumn(columnOrdinal, newColumnName);
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to rename column at '{creatorEmail}:{boardName}[{columnOrdinal}]' - out of range 0 - {e.Message} (inclusive)");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
        }

        /// <summary>
        /// Moves a column shiftSize times to the right. If shiftSize is negative, the column moves to the left
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">The column location. </param>
        /// <param name="shiftSize">The number of times to move the column, relativly to its current location. Negative values are allowed</param>  
        internal void MoveColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int shiftSize)
        {
            ValidateLogin(userEmail, $"MoveColumn({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal}, {shiftSize})");
            CheckMembership(userEmail, creatorEmail, boardName, "MoveColumn");
            try
            {
                boards[creatorEmail][boardName].MoveColumn(columnOrdinal, shiftSize);
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to move column at '{creatorEmail}:{boardName}[{columnOrdinal}]' - out of range 0 - {e.Message} (inclusive)");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
            catch (ArithmeticException)
            {
                log.Warn($"FAILED to move column at '{creatorEmail}:{boardName}[{columnOrdinal}]' - shift '{shiftSize}' too large");
                throw new ArithmeticException($"Shift out of range");
            }

        }

        /// <summary>
        /// Removes a specific column
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">The column location. Location for old columns with index>=columnOrdinal is decreases by 1 </param>
        internal void RemoveColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            ValidateLogin(userEmail, $"RemoveColumn({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal})");
            CheckMembership(userEmail, creatorEmail, boardName, "RemoveColumn");
            try
            {
                    boards[creatorEmail][boardName].RemoveColumn(columnOrdinal);
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to remove column at '{creatorEmail}:{boardName}[{columnOrdinal}]' - out of range 0 - {e.Message} (inclusive)");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
        }

        /// <summary>
        /// sets a limit to a specific column in a specific board
        /// </summary>
        /// <param name="userEmail">the calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">the column</param>
        /// <param name="limit">new and updated limit</param>
        /// <exception cref="ArgumentException">thrown if the new limit isn't legal, if it's impossible to set the limit due to column complications</exception>
        /// <remarks>calls ValidateLogin, CheckMembership</remarks>
        internal void LimitColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int limit)
        {
            ValidateLogin(userEmail, $"LimitColumn({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal}, {limit})");
            CheckMembership(userEmail, creatorEmail, boardName, "LimitColumn");
            if (limit < -1)
            {
                log.Warn($"FAILED to set an impossible limit to '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}'. Limit: " + limit);
                throw new ArgumentException("impossible limit");
            }
            try
            {
                boards[creatorEmail][boardName].LimitColumn(columnOrdinal, limit);
                log.Info($"SUCCESSFULLY set new limit for '{creatorEmail}:{boardName}'[{columnOrdinal}]' by '{userEmail}'. Limit: " + limit);
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to access '{creatorEmail}:{boardName}[{columnOrdinal}]' - Column doesn't exist");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
            catch (InvalidOperationException e)
            {
                log.Warn($"FAILED to set limit for '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Column has more than " + limit + " tasks. Limit: " + limit);
                throw new ArgumentException($"Cannot set limit: There are more than {limit} tasks in column '{e.Message}' of board '{creatorEmail}:{boardName}'");
            }
        }

        /// <summary>
        /// adds a new task to a board (always adds the task to 'Backlog' column)
        /// </summary>
        /// <param name="userEmail">the calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="creationTime">task's creation time</param>
        /// <param name="title">task's title</param>
        /// <param name="description">task's description</param>
        /// <param name="DueDate">task's due date</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">throw if one of the task's arguments isn't legal</exception>
        /// <exception cref="OutOfMemoryException">thrown if the column is already at its limit</exception>
        /// <remarks>calls ValidateLogin, CheckMembership</remarks>
        internal ITask AddTask(string userEmail, string creatorEmail, string boardName, DateTime creationTime, string title, string description, DateTime DueDate)
        {
            ValidateLogin(userEmail, $"AddTask({userEmail}, {creatorEmail}, {boardName}, {title}");
            CheckMembership(userEmail, creatorEmail, boardName, "AddTask");
            //arrange
            IBoard board = boards[creatorEmail][boardName];
            ITask task = CreateTask(board.TaskCount, creationTime, title, description, DueDate, userEmail, creatorEmail, boardName);
            //add the task
            try
            {
                board.AddTask(task);
            }
            catch (OutOfMemoryException e)
            {
                log.Warn($"FAILED to add task '{title}' to '{creatorEmail}:{boardName}[{0}]' by '{userEmail}' - Column is at it's limit");
                throw new OutOfMemoryException($"Cannot add task '{title}': Column '{e.Message}' of board '{creatorEmail}:{boardName}' is currently at its limit");
            }
            //persist the task
            try
            {
                task.persist();
            }
            catch (InvalidOperationException)
            {
                board.RemoveTask(userEmail, 0, task.ID);
                log.Warn($"FAILED to add task '{title} to '{userEmail}:{boardName}' - taskID exists in DataBase but not in BusinessLayer");
                throw new Exception($"Cannot ad task '{title}' to board '{userEmail}:{boardName}' - taskID collides with an existing task within the DataBase, please LoadData before continueing");
            }
            catch (Exception)
            {
                board.RemoveTask(userEmail, 0, task.ID);
                log.Fatal($"FAILED to persist task {task.ID} in board '{board.Creator}:{board.Name}'");
                throw new Exception("WARNING: Task was created but couldn't be saved!\nPlease restart the program and try again!");
            }
            //success
            log.Info($"SUCCESSFULLY added task '{task.ID}' to '{creatorEmail}:{boardName}' by '{userEmail}'");
            return task;
        }

        /// <summary>
        /// Assigns a task to a user
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">column in which the task is stored</param>
        /// <param name="taskId">The task to be updated identified task ID</param>        
        /// <param name="emailAssignee">userEmail of the user to assign to task to</param>
        /// <exception cref="ArgumentException">Thrown if asked to update a task from column 2</exception>
        /// <exception cref="ArgumentException">Throw if the task isn't stored in said column, if new DueDate isn't legal</exception>
        /// <remarks>calls ValidateLogin, CheckMembership</remarks>
        internal void AssignTask(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string emailAssignee)
        {
            ValidateLogin(userEmail, $"AssignTask({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal}, {taskId}, {emailAssignee})");
            CheckMembership(userEmail, creatorEmail, boardName, "AssignTask"); //membership of user
            CheckMembership(emailAssignee, creatorEmail, boardName, "AssignTask"); //membership of assignee
            if (columnOrdinal == (boards[creatorEmail][boardName].ColumnCount - 1) )
            {
                log.Warn($"FAILED to assign task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Updating tasks at column 'done' is prohibited");
                throw new ArgumentException("Cannot reassign tasks in rightmost column");
            }
            try
            {
                boards[creatorEmail][boardName].AssignTask(userEmail, columnOrdinal, taskId, emailAssignee);
                log.Info($"SUCCESSFULLY assigned Task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' to '{emailAssignee}' by '{userEmail}'");
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to access '{creatorEmail}:{boardName}[{columnOrdinal}]' - Column doesn't exist");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
            catch (IndexOutOfRangeException e)
            {
                log.Warn($"FAILED to assign task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' to '{emailAssignee}' by '{userEmail}' - Task not found");
                throw new ArgumentException($"Cannot assign task: A task with ID '{taskId}' does not exist in column '{e.Message}' of board '{creatorEmail}:{boardName}'");
            }
            catch (InvalidOperationException)
            {
                log.Warn($"FAILED to assign task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' to '{emailAssignee}' by '{userEmail}' - user is not the assignee");
                throw new InvalidOperationException($"Cannot update task: You are not the assignee of the task");
            }
        }

        /// <summary>
        /// updates an existing task's due date
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">column in which the task is stored</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="DueDate">new and updated due date</param>
        /// <exception cref="ArgumentException">Thrown if asked to update a task from column 2</exception>
        /// <exception cref="ArgumentException">Throw if the task isn't stored in said column, if new DueDate isn't legal</exception>
        /// <remarks>calls ValidateLogin, CheckMembership</remarks>
        internal void UpdateTaskDueDate(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, DateTime DueDate)
        {
            ValidateLogin(userEmail, $"UpdateTaskDueDate({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal}, {taskId}, {DueDate})");
            CheckMembership(userEmail, creatorEmail, boardName, "UpdateTaskDueDate");
            if (columnOrdinal == (boards[creatorEmail][boardName].ColumnCount - 1) )
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Updating tasks at column 'done' is prohibited");
                throw new ArgumentException("Cannot update tasks in rightmost column");
            }
            try
            {
                boards[creatorEmail][boardName].UpdateTaskDueDate(userEmail, columnOrdinal, taskId, DueDate);
                log.Info($"SUCCESSFULLY updated Task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - DueDate");
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to access '{creatorEmail}:{boardName}[{columnOrdinal}]' - Column doesn't exist");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
            catch (IndexOutOfRangeException e)
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Task not found");
                throw new ArgumentException($"Cannot update task: A task with ID '{taskId}' does not exist in column '{e.Message}' of board '{creatorEmail}:{boardName}'");
            }
            catch (InvalidOperationException)
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - user is not the assignee");
                throw new InvalidOperationException($"Cannot update task: You are not the assignee of the task");
            }
            catch (Exception e)
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Failed at Task: {e.Message}");
                throw new ArgumentException($"Cannot update task: {e.Message}");
            }
        }

        /// <summary>
        /// updates an existing task's title
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">column in which the task is stored</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="title">new and updated title</param>
        /// <exception cref="ArgumentException">Thrown if asked to update a task from column 2</exception>
        /// <exception cref="ArgumentException">Throw if the task isn't stored in said column, if new title isn't legal</exception>
        /// <remarks>calls ValidateLogin, CheckMembership</remarks>
        internal void UpdateTaskTitle(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string title)
        {
            ValidateLogin(userEmail, $"UpdateTaskTitle({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal}, {taskId}, {title})");
            CheckMembership(userEmail, creatorEmail, boardName, "UpdateTaskTitle");
            if (columnOrdinal == (boards[creatorEmail][boardName].ColumnCount - 1) )
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Updating tasks at column 'done' is prohibited");
                throw new ArgumentException("Cannot update tasks in rightmost column");
            }
            try
            {
                boards[creatorEmail][boardName].UpdateTaskTitle(userEmail, columnOrdinal, taskId, title);
                log.Info($"SUCCESSFULLY updated Task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Title");
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to access '{creatorEmail}:{boardName}[{columnOrdinal}]' - Column doesn't exist");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
            catch (IndexOutOfRangeException e)
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Task not found");
                throw new ArgumentException($"Cannot update task: A task with ID '{taskId}' does not exist in column '{e.Message}' of board '{creatorEmail}:{boardName}'");
            }
            catch (InvalidOperationException)
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - user is not the assignee");
                throw new InvalidOperationException($"Cannot update task: You are not the assignee of the task");
            }
            catch (Exception e)
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Failed at Task: {e.Message}");
                throw new ArgumentException($"Cannot update task: {e.Message}");
            }
        }

        /// <summary>
        /// updates an existing task's description
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">column in which the task is stored</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="description">new and updated description</param>
        /// <exception cref="ArgumentException">Thrown if asked to update a task from column 2</exception>
        /// <exception cref="ArgumentException">Throw if the task isn't stored in said column, if new description isn't legal</exception>
        /// <remarks>calls ValidateLogin CheckMembership</remarks>
        internal void UpdateTaskDescription(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string description)
        {
            ValidateLogin(userEmail, $"UpdateTaskDescription({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal}, {taskId}, {description})");
            CheckMembership(userEmail, creatorEmail, boardName, "UpdateTaskDescription");
            if (columnOrdinal == (boards[creatorEmail][boardName].ColumnCount - 1) )
            {
                log.Warn($"FAILED to update task '{taskId}' at '{userEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Updating tasks at column 'done' is prohibited");
                throw new ArgumentException("Cannot update tasks in rightmost column");
            }
            try
            {
                boards[creatorEmail][boardName].UpdateTaskDescription(userEmail, columnOrdinal, taskId, description);
                log.Info($"SUCCESSFULLY updated Task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Description");
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to access '{creatorEmail}:{boardName}[{columnOrdinal}]' - Column doesn't exist");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
            catch (IndexOutOfRangeException e)
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Task not found");
                throw new ArgumentException($"Cannot update task: A task with ID '{taskId}' does not exist in column '{e.Message}' of board '{creatorEmail}:{boardName}'");
            }
            catch (InvalidOperationException)
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - user is not the assignee");
                throw new InvalidOperationException($"Cannot update task: You are not the assignee of the task");
            }
            catch (Exception e)
            {
                log.Warn($"FAILED to update task '{taskId}' at '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Failed at Task: {e.Message}");
                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// advanced an existing task's to the next column
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">column in which the task is stored</param>
        /// <param name="taskId">task's ID</param>
        /// <exception cref="ArgumentException">Thrown if asked to advance a task from column 2</exception>
        /// <exception cref="ArgumentException">Throw if the task isn't stored in said column, if next column is at it's limit</exception>
        /// <remarks>calls ValidateLogin, CheckMembership</remarks>
        internal void AdvanceTask(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId)
        {
            ValidateLogin(userEmail, $"AdvanceTask({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal}, {taskId})");
            CheckMembership(userEmail, creatorEmail, boardName, "AdvanceTask");
            if (columnOrdinal == (boards[creatorEmail][boardName].ColumnCount - 1) )
            {
                log.Warn($"FAILED to advance task '{taskId}' from '{userEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Advancing tasks from column 'done' is prohibited");
                throw new ArgumentException("Cannot advance tasks from rightmost column");
            }
            try
            {
                boards[creatorEmail][boardName].AdvanceTask(userEmail, columnOrdinal, taskId);
                log.Info($"SUCCESSFULLY advanced Task '{taskId}' from '{creatorEmail}:{boardName}[{columnOrdinal}]' to '{creatorEmail}:{boardName}[{columnOrdinal + 1}]' by '{userEmail}'");
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to access '{creatorEmail}:{boardName}[{columnOrdinal}]' - Column doesn't exist");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
            catch (IndexOutOfRangeException e)
            {
                log.Warn($"FAILED to advance task '{taskId}' from '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Task not found");
                throw new ArgumentException($"Cannot advance task: A task with ID '{taskId}' does not exist in column '{e.Message}' of board '{creatorEmail}:{boardName}'");
            }
            catch (OutOfMemoryException e)
            {
                log.Warn($"FAILED to advance task '{taskId}' from '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - Column '{creatorEmail}:{boardName}[{columnOrdinal + 1}]' is currently at its limit");
                throw new OutOfMemoryException($"Cannot advance task: Column '{e.Message}' of board '{creatorEmail}:{boardName}' is currently at its limit");
            }
            catch (InvalidOperationException)
            {
                log.Warn($"FAILED to advance task '{taskId}' from '{creatorEmail}:{boardName}[{columnOrdinal}]' by '{userEmail}' - user is not the assignee");
                throw new InvalidOperationException($"Cannot advance task: You are not the assignee of the task");
            }
        }

        /// <summary>
        /// Finds and returns specific column of a specific board
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">column index</param>
        /// <returns>Requested column</returns>
        /// <remarks>calls ValidateLogin, CheckMembership</remarks>
        internal IColumn GetColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            ValidateLogin(userEmail, $"GetColumn({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal})");
            CheckMembership(userEmail, creatorEmail, boardName, "GetColumn");
            try
            {
                return boards[creatorEmail][boardName].GetColumn(columnOrdinal);
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to access '{creatorEmail}:{boardName}[{columnOrdinal}]' - Column doesn't exist");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
        }

        /// <summary>
        /// Finds and returns all tasks in specific column of a specific board
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">column index</param>
        /// <returns>IList of tasks of requested column</returns>
        /// <remarks>calls ValidateLogin, CheckMembership</remarks>
        internal IList<ITask> GetColumnTasks(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            ValidateLogin(userEmail, $"GetColumn({userEmail}, {creatorEmail}, {boardName}, {columnOrdinal})");
            CheckMembership(userEmail, creatorEmail, boardName, "GetColumn");
            try
            {
                return boards[creatorEmail][boardName].GetColumnTasks(columnOrdinal);
            }
            catch (ArgumentException e)
            {
                log.Warn($"FAILED to access '{creatorEmail}:{boardName}[{columnOrdinal}]' - Column doesn't exist");
                throw new ArgumentException($"Column ordinal out of range: Argument needs to be between 0 and {e.Message} (inclusive)");
            }
        }

        /// <summary>
        /// Finds and returns specific board
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <returns>Requested Board</returns>
        /// <remarks>calls ValidateLogin CheckMembership</remarks>
        internal IBoard GetBoard(string userEmail, string creatorEmail, string boardName)
        {
            ValidateLogin(userEmail, $"GetBoard({userEmail}, {creatorEmail}, {boardName})");
            CheckMembership(userEmail, creatorEmail, boardName, "GetBoard");
            return boards[creatorEmail][boardName];
        }

        /// <summary>
        /// gets all the tasks of a user that are 'In Progress'
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <returns>IList of the user's 'In Progress' tasks</returns>
        /// <remarks>calls ValidateLogin, CheckBoardExistance</remarks>
        internal IList<ITask> InProgressTasks(string userEmail)
        {
            ValidateLogin(userEmail, $"InProgressTasks({userEmail})");
            IList<ITask> inProgress = new List<ITask>();
            if (userBoards.ContainsKey(userEmail))
            {
                foreach (string board in userBoards[userEmail])
                {
                    string[] boardDetails = board.Split(':', 2);
                    if (CheckBoardExistance(boardDetails[0], boardDetails[1]))
                    {
                            foreach (ITask task in boards[boardDetails[0]][boardDetails[1]].GetInProgressTasks())
                            {
                                if (task.Assignee.Equals(userEmail))
                                    inProgress.Add(task);
                            }
                    }
                    else
                    {
                        userBoards[userEmail].Remove(board);
                    }
                }
            }
            return inProgress;
        }

        private IBoard CreateBoard(string userEmail, string boardName)
        {
                return new Board(userEmail, boardName);
        }

        private IColumn CreateColumn(string name, string creatorEmail, string boardName, int columnOrdinal)
        {
            return new Column(name, creatorEmail, boardName, columnOrdinal);
        }

        private ITask CreateTask(int id, DateTime creationTime, string title, string description, DateTime dueDate, string assignee, string boardCreator, string boardName)
        {
            try
            {
                return new Task(id, creationTime, title, description, dueDate, assignee, boardCreator, boardName);
            }
            catch (Exception e)
            {
                log.Warn($"FAILED to add task '{title}' to '{boardCreator}:{boardName}[{0}]' by '{assignee}' - Crashed at Task: {e.Message}");
                throw new ArgumentException($"Cannot add task '{title}': {e.Message}");
            }
        }

        /// <summary>
        /// checks whether a board exists or not
        /// </summary>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <returns>true if the board exists, false if it doesn't</returns>
        private bool CheckBoardExistance(string creatorEmail, string boardName)
        {
            if (creatorEmail == null || boardName == null || !boards.ContainsKey(creatorEmail) || !boards[creatorEmail].ContainsKey(boardName))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// checks whether a user is a member of given board
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="method">the method calling this check</param>
        /// <exception cref="AccessViolationException">thrown if the user is trying to access a board which he is not a member of</exception>
        /// <remarks>calls CheckBoardExistance</remarks>
        private void CheckMembership(string userEmail, string creatorEmail, string boardName, string method)
        {
            bool exists = CheckBoardExistance(creatorEmail, boardName);
            if (!userBoards.ContainsKey(userEmail) || !userBoards[userEmail].Contains($"{creatorEmail}:{boardName}") || !exists) {
                if (!exists)
                {
                    userBoards[userEmail].Remove($"{creatorEmail}:{boardName}");
                }
                log.Warn($"ACCESS VIOLATION - '{method}' - {userEmail} is not a member of '{creatorEmail}:{boardName}'");
                throw new AccessViolationException($"{userEmail} is not a member of '{creatorEmail}:{boardName}'");
            }
        }

        /// <summary>
        /// validates the calling user is logged in
        /// </summary>
        /// <param name="userEmail">the calling user's email</param>
        /// <param name="attempt">the attempt - for logging purposes</param>
        private void ValidateLogin(string userEmail, string attempt)
        {
            try
            {
                loginInstance.ValidateLogin(userEmail);
            }
            catch (InvalidOperationException e)
            {
                log.Warn($"OUT OF DOMAIN OPERATION: User '{loginInstance.ConnectedEmail}' attempted '{attempt}'");
                throw new InvalidOperationException(e.Message);
            }
        }
    }
}
