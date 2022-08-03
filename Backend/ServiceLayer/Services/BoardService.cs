using System.Collections.Generic;
using System;
using BC = IntroSE.Kanban.Backend.BusinessLayer.BoardController;
using BBoard = IntroSE.Kanban.Backend.BusinessLayer.IBoard;
using BColumn = IntroSE.Kanban.Backend.BusinessLayer.IColumn;
using BTask = IntroSE.Kanban.Backend.BusinessLayer.ITask;
using LoginInstance = IntroSE.Kanban.Backend.BusinessLayer.LoginInstance;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    /// <summary>
    /// Service platform which handles all operations board related
    /// </summary>
    /// <remarks>in method requesting columnOrdinal - the integer will represent 1 out of the 3 columns: 0 - Backlog, 1 - In Progress, 2 - Done</remarks>
    internal class BoardService
    {
        //fields
        private BC bc;

        //constructors
        internal BoardService(LoginInstance loginInstance)
        {
            bc = new BC(loginInstance);
        }

        //methods 

        ///<summary>This method loads all the boards and tasks data from the persistance </summary>
        internal Response LoadData()
        {
            try
            {
                bc.LoadData();
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }

        ///<summary>Removes all the boards and tasks data from the persistence.</summary>
        internal Response DeleteData()
        {
            try
            {
                bc.DeleteData();
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// creates a new board for the user
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="boardName">name of the new board</param>
        /// <returns>Reponse containing the new board, or message detailing the error if occured</returns>
        internal Response<Board> AddBoard(string userEmail, string boardName)
        {
            try
            {
                BBoard bBoard = bc.AddBoard(userEmail, boardName);
                Board board = new Board(bBoard);
                return Response<Board>.FromValue(board);
            }
            catch (Exception e)
            {
                return Response<Board>.FromError(e.Message);
            }
        }

        /// <summary>
        /// Adds a board created by another user to the logged-in user. 
        /// </summary>
        /// <param name="userEmail">userEmail of the current user.</param>
        /// <param name="creatorEmail">userEmail of the board creator</param>
        /// <param name="boardName">The name of the new board</param>
        /// <returns>A response object. The response should contain the board if succeeded or an error message in case of an error</returns>
        internal Response<Board> JoinBoard(string userEmail, string creatorEmail, string boardName)
        {
            try
            {
                BBoard bBoard = bc.JoinBoard(userEmail, creatorEmail, boardName);
                Board board = new Board(bBoard);
                return Response<Board>.FromValue(board);
            }
            catch (Exception e)
            {
                return Response<Board>.FromError(e.Message);
            }
        }

        /// <summary>
        /// Retrieves specific board
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <returns>Response holding: ServiceLayer.Board if succeeded, message detailing error if occured</returns>
        internal Response<Board> GetBoard(string userEmail, string creatorEmail, string boardName)
        {
            try
            {
                BBoard bBoard = bc.GetBoard(userEmail, creatorEmail, boardName);
                Board board = new Board(bBoard);
                return Response<Board>.FromValue(board);
            }
            catch (Exception e)
            {
                return Response<Board>.FromError(e.Message);
            }
        }

        /// <summary>
        /// removes existing board of the user
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">email of the board creator - identifier</param>
        /// <param name="boardName">name of the board - identifier</param>
        /// <returns>Reponse containing message detailing the error if occured</returns>
        internal Response<Board> RemoveBoard(string userEmail, string creatorEmail, string boardName)
        {
            try
            {
                BBoard bBoard = bc.RemoveBoard(userEmail, creatorEmail, boardName);
                Board board = new Board(bBoard);
                return Response<Board>.FromValue(board);
            }
            catch (Exception e)
            {
                return Response<Board>.FromError(e.Message);
            }
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The location of the new column. Location for old columns with index>=columnOrdinal is increased by 1 (moved right). The first column is identified by 0, the location increases by 1 for each column.</param>
        /// <param name="columnName">The name for the new columns</param>        
        /// <returns>A response object. The response should contain the newly added column or an error message in case of an error</returns>
        public Response<Column> AddColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, string columnName)
        {
            try
            {
                BColumn bColumn = bc.AddColumn(userEmail, creatorEmail, boardName, columnOrdinal, columnName);
                Column column = new Column(bColumn);
                return Response<Column>.FromValue(column);
            }
            catch(Exception e)
            {
                return Response<Column>.FromError(e.Message);
            }
        }
        
        /// <summary>
        /// Retrieves specific column of a specific board
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">column index</param>
        /// <returns>Response holding: ServiceLayer.Column if succeeded, message detailing error if occured</returns>
        internal Response<Column> GetColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            try
            {
                return Response<Column>.FromValue(new Column(bc.GetColumn(userEmail, creatorEmail, boardName, columnOrdinal)));
            }
            catch (Exception e)
            {
                return Response<Column>.FromError(e.Message);
            }
        }

        /// <summary>
        /// limits specific column in one of the user's boards
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">email of the board creator - identifier</param>
        /// <param name="boardName">name of the board - identifier</param>
        /// <param name="columnOrdinal">number representing a column</param>
        /// <param name="limit">new limit</param>
        /// <returns>Response containing message detailing the error - if occured</returns>
        internal Response LimitColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int limit)
        {
            try
            {
                bc.LimitColumn(userEmail, creatorEmail, boardName, columnOrdinal, limit);
                return new Response();
            }
            catch(Exception e)
            {
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// Renames a specific column
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column location. The first column location is identified by 0, the location increases by 1 for each column</param>
        /// <param name="newColumnName">The new column name</param>        
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response RenameColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, string newColumnName)
        {
            try
            {
                bc.RenameColumn(userEmail, creatorEmail, boardName, columnOrdinal, newColumnName);
                return new Response();
            }
            catch(Exception e)
            {
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// Moves a column shiftSize times to the right. If shiftSize is negative, the column moves to the left
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column location. The first column location is identified by 0, the location increases by 1 for each column</param>
        /// <param name="shiftSize">The number of times to move the column, relativly to its current location. Negative values are allowed</param>  
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response MoveColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int shiftSize)
        {
            try
            {
                bc.MoveColumn(userEmail, creatorEmail, boardName, columnOrdinal, shiftSize);
                return new Response();
            }
            catch(Exception e)
            {
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// Removes a specific column
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column location. The first column location is identified by 0, the location increases by 1 for each column</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response RemoveColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            try
            {
                bc.RemoveColumn(userEmail, creatorEmail, boardName, columnOrdinal);
                return new Response();
            }
            catch(Exception e)
            {
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// Adds new task to a user's board
        /// </summary>
        /// <param name="userEmail">user's email</param>
        /// <param name="creatorEmail">email of the board creator - identifier</param>
        /// <param name="boardName">name of the board - identifier</param>
        /// <param name="title">task's title</param>
        /// <param name="description">task's description</param>
        /// <param name="DueDate">task's due date</param>
        /// <returns>Response holding: the newly added task if succeeded, message detailing the error if occured</returns>
        internal Response<Task> AddTask(string userEmail, string creatorEmail, string boardName, DateTime creationTime, string title, string description, DateTime DueDate)
        {
            try
            {
                BTask bTask = bc.AddTask(userEmail, creatorEmail, boardName, creationTime, title, description, DueDate);
                Task task = new Task(bTask);
                return Response<Task>.FromValue(task);
            }
            catch (Exception e)
            {
                return Response<Task>.FromError(e.Message);
            }
        }

        /// <summary>
        /// Assigns a task to a user
        /// </summary>
        /// <param name="userEmail">userEmail of the current user.</param>
        /// <param name="creatorEmail">userEmail of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>        
        /// <param name="emailAssignee">userEmail of the user to assign to task to</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        internal Response AssignTask(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string emailAssignee)
        {
            try
            {
                bc.AssignTask(userEmail, creatorEmail, boardName, columnOrdinal, taskId, emailAssignee);
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// updates an existing task's due date
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">email of the board creator - identifier</param>
        /// <param name="boardName">name of the board - identifier</param>
        /// <param name="columnOrdinal">column in which the task is stored</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="DueDate">new and updated due date</param>
        /// <returns>Response containing message detailing the error if occured</returns>
        internal Response UpdateTaskDueDate(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, DateTime DueDate)
        {
            try
            {
                bc.UpdateTaskDueDate(userEmail, creatorEmail, boardName, columnOrdinal, taskId,DueDate);
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// updates an existing task's title
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">email of the board creator - identifier</param>
        /// <param name="boardName">name of the board - identifier</param>
        /// <param name="columnOrdinal">column in which the task is stored</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="title">new and updated title</param>
        /// <returns>Response containing message detailing the error if occured</returns>
        internal Response UpdateTaskTitle(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string title)
        {
            try
            {
                bc.UpdateTaskTitle(userEmail, creatorEmail, boardName, columnOrdinal, taskId, title);
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// updates an existing task's description
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">email of the board creator - identifier</param>
        /// <param name="boardName">name of the board - identifier</param>
        /// <param name="columnOrdinal">column in which the task is stored</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="description">new and updated description</param>
        /// <returns>Response containing message detailing the error if occured</returns>
        internal Response UpdateTaskDescription(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string description)
        {
            try
            {
                bc.UpdateTaskDescription(userEmail, creatorEmail, boardName, columnOrdinal, taskId, description);
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// advanced a task to the next column
        /// </summary>
        /// <param name="userEmail">the calling user's email</param>
        /// <param name="creatorEmail">email of the board creator - identifier</param>
        /// <param name="boardName">name of the board - identifier</param>
        /// <param name="columnOrdinal">column in which the task is stored</param>
        /// <param name="taskId">task's ID</param>
        /// <returns>Response containing message detailing the error if occured</returns>
        internal Response AdvanceTask(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId)
        {
            try
            {
                bc.AdvanceTask(userEmail, creatorEmail, boardName, columnOrdinal, taskId);
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }
        
        /// <summary>
        /// Retrieves all tasks within specific column of a specific board
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="creatorEmail">board's creator - identifier</param>
        /// <param name="boardName">board's name - identifier</param>
        /// <param name="columnOrdinal">column index</param>
        /// <returns>Response holding: all tasks if succeeded, message detailing error if occured</returns>
        internal Response<IList<Task>> GetColumnTasks(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            try
            {
                IList<BTask> bTasks = bc.GetColumnTasks(userEmail, creatorEmail, boardName, columnOrdinal);
                IList<Task> tasks = translateList(bTasks);
                return Response<IList<Task>>.FromValue(tasks);
            }
            catch (Exception e)
            {
                return Response<IList<Task>>.FromError(e.Message);
            }
        }

        /// <summary>
        /// Returns all 'In Progress' tasks of a user
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <returns>Response holding: IList containing all of the user's 'In Progress' tasks if succeeded, a message detailing the error if occured</returns>
        internal Response<IList<Task>> InProgressTasks(string userEmail)
        {
            try
            {
                IList<BTask> bTasks = bc.InProgressTasks(userEmail);
                IList<Task> tasks = translateList(bTasks);
                return Response<IList<Task>>.FromValue(tasks);
            }
            catch (Exception e)
            {
                return Response<IList<Task>>.FromError(e.Message);
            }
        }

        /// <summary>
        /// Returns the list of board of a user. The user must be logged-in. The function returns all the board names the user created or joined.
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <returns>A response object with a value set to the board, instead the response should contain a error message in case of an error</returns>
        public Response<IList<string>> GetBoardNames(string userEmail)
        {
            try
            {
                IList<string> boardNames = bc.GetBoardNames(userEmail);
                return Response<IList<string>>.FromValue(boardNames);
            }
            catch (Exception e)
            {
                return Response<IList<string>>.FromError(e.Message);
            }
        }

        /// <summary>
        /// Returns the list of board of a user. The user must be logged-in. The function returns all the board names the user created or joined.
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <returns>A response object with a value set to the board, instead the response should contain a error message in case of an error</returns>
        public Response<IList<string>> GetBetterBoardNames(string userEmail)
        {
            try
            {
                IList<string> boardNames = bc.GetBetterBoardNames(userEmail);
                return Response<IList<string>>.FromValue(boardNames);
            }
            catch (Exception e)
            {
                return Response<IList<string>>.FromError(e.Message);
            }
        }

        /// <summary>
        /// translates a list containing tasks of business layer to a list containing the service layer form of the same tasks
        /// </summary>
        /// <param name="originalList">list that will be translated</param>
        /// <returns>IList of service layer tasks</returns>
        private IList<Task> translateList(IList<BTask> originalList)
        {
            IList<Task> translatedList = new List<Task>();
            foreach(BTask bTask in originalList)
            {
                translatedList.Add(new Task(bTask));
            }
            return translatedList;
        }
    }
}
