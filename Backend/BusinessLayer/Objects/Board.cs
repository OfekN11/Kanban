using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DBoard = IntroSE.Kanban.Backend.DataLayer.DBoard;
using DColumn= IntroSE.Kanban.Backend.DataLayer.DColumn;

[assembly: InternalsVisibleTo("IntroSE.Kanban.Tests")]
namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// Contains 3 columns: Backlog, In Progress, Done
    /// manages those 3 columns and the addition/movement of tasks in them
    /// </summary>
    /// <remarks>in methods requesting columnOrdinal - the integer will represent 1 out of the 3 columns: 0 - Backlog, 1 - In Progress, 2 - Done</remarksY>
    internal class Board : IBoard
    {
        //fields
        private int _taskIdCounter = 0; //will be updated by every task added to the board and so keeping each ID unique
        private int _columnCounter = 3; //will be updated by every column added/removed from the board 
        private IList<IColumn> _columns = new List<IColumn>();

        private DBoard _dBoard; //parallel DTO

        string IBoard.Name { get => _dBoard.BoardName; }
        string IBoard.Creator { get => _dBoard.CreatorEmail; }
        int IBoard.TaskCount { get => _taskIdCounter; }
        int IBoard.ColumnCount { get => _columnCounter; }

        //constructor
        /// <summary>
        /// creates a new Board object
        /// </summary>
        internal Board(string creatorEmail, string boardName)
        {
            _columns.Insert(0, new Column("backlog", creatorEmail, boardName, 0));
            _columns.Insert(1, new Column("in progress", creatorEmail, boardName, 1));
            _columns.Insert(2, new Column("done", creatorEmail, boardName, 2));
            _dBoard = new DBoard(creatorEmail, boardName);
        }

        internal Board(DBoard dBoard)
        {
            foreach (DColumn dColumn in dBoard.Columns)
            {
                _columns.Add(new Column(dColumn));
            }
            _taskIdCounter = dBoard.numberOfTasks();
            _dBoard = dBoard;
            _dBoard.Persist = true;
            _columnCounter = dBoard.Columns.Count;
        }

        //methods

        void IBoard.Persist()
        {
            foreach (IColumn column in _columns)
            {
                try
                {
                    column.Persist();
                }
                catch (InvalidOperationException)
                {
                    for (int i = column.Ordinal - 1; i >= 0; i++)
                    {
                        _dBoard.RemoveColumn(i);
                    }
                    throw new InvalidOperationException();
                }
                catch (Exception)
                {
                    for (int i = column.Ordinal - 1; i >= 0; i++)
                    {
                        _dBoard.RemoveColumn(i);
                    }
                    throw new Exception();
                }
            }
            _dBoard.Insert();
            _dBoard.Persist = true;
        }

        /// <summary>
        /// Adds new member to DTO
        /// </summary>
        /// <param name="memberEmail">new member's email</param>
        void IBoard.AddMember(string memberEmail)
        {
            _dBoard.AddMember(memberEmail);
        }

        /// <summary>
        /// Adds the given column to the board
        /// </summary>
        /// <param name="column">the added column</param>
        /// <exception cref="ArgumentException">Thrown when column's ordinal is out of legal range</exception>
        void IBoard.AddColumn(IColumn column)
        {
            if (column.Ordinal < 0 || column.Ordinal > _columnCounter)
                throw new ArgumentException($"{_columnCounter}");
            _columns.Insert(column.Ordinal, column);
            _columnCounter++;
            for (int i = column.Ordinal + 1; i < _columnCounter; i++)
            {
                _columns[i].Ordinal = i;
            }
        }
        
        /// <summary>
        /// Returns specific column
        /// </summary>
        /// <param name="columnOrdinal">represents the requested column</param>
        /// <returns>Requested Column</returns>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        IColumn IBoard.GetColumn(int columnOrdinal)
        {
            CheckColumnOrdinal(columnOrdinal);
            return _columns[columnOrdinal];
        }

        /// <summary>
        /// Renames a specific column
        /// </summary>
        /// <param name="columnOrdinal">The column location. </param>
        /// <param name="newColumnName">The new column name</param>        
        /// <remarks>calls CheckColumnOrdinal</remarks>
        void IBoard.RenameColumn(int columnOrdinal, string newColumnName)
        {
            CheckColumnOrdinal(columnOrdinal);
            _columns[columnOrdinal].Name = newColumnName;
        }

        /// <summary>
        /// Moves a column shiftSize times to the right. If shiftSize is negative, the column moves to the left
        /// </summary>
        /// <param name="columnOrdinal">The column location. </param>
        /// <param name="shiftSize">The number of times to move the column, relativly to its current location. Negative values are allowed</param>  
        /// <remarks>calls CheckColumnOrdinal</remarks>
        void IBoard.MoveColumn(int columnOrdinal, int shiftSize)
        {
            CheckColumnOrdinal(columnOrdinal);
            if (columnOrdinal + shiftSize >= _columnCounter || columnOrdinal + shiftSize < 0)
                throw new ArithmeticException();
            if (_columns[columnOrdinal].GetTasks().Count != 0)
                throw new Exception("Cannot move non-empty column");
            int sign = Math.Sign(shiftSize);
            IColumn tmp = _columns[columnOrdinal];
            while (shiftSize != 0) //updating all other columns' ordinal
            { 
                _columns[columnOrdinal] = _columns[columnOrdinal + sign];
                _columns[columnOrdinal].Ordinal = columnOrdinal;
                columnOrdinal += sign; shiftSize -= sign;
            }
            _columns[columnOrdinal] = tmp;
            _columns[columnOrdinal].Ordinal = columnOrdinal;
        }

        /// <summary>
        /// limits a column within the board
        /// </summary>
        /// <param name="columnOrdinal">represent the limited column</param>
        /// <param name="limit">new limit</param>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        void IBoard.LimitColumn(int columnOrdinal, int limit)
        {
            CheckColumnOrdinal(columnOrdinal);
            _columns[columnOrdinal].Limit = limit;
        }

        /// <summary>
        /// Removes a specific column
        /// </summary>
        /// <param name="columnOrdinal">The column location. Location for old columns with index>=columnOrdinal is decreases by 1 </param>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        IColumn IBoard.RemoveColumn(int columnOrdinal)
        {
            CheckColumnOrdinal(columnOrdinal);
            if (_columnCounter == 2)
                throw new ArgumentException($"Cannot remove column {_columns[columnOrdinal].Name} - Board cannot have less than 2 columns");
            IColumn column = _columns[columnOrdinal];
            _columns.RemoveAt(columnOrdinal);
            _columnCounter--;
            _dBoard.RemoveColumn(columnOrdinal);
            if (columnOrdinal == 0)
            {
                _columns[columnOrdinal].AddTasks(column.GetTasks());
            }
            else
            {
                column.Ordinal = columnOrdinal - 1;
                _columns[columnOrdinal - 1].AddTasks(column.GetTasks());
            }
            while (columnOrdinal < _columnCounter)
            {
                _columns[columnOrdinal].Ordinal = columnOrdinal;
                columnOrdinal = columnOrdinal + 1;
            }
            return column;
        }
        
        /// <summary>
        /// Adds a new task to the board at column 'Backlog'
        /// </summary>
        /// <param name="creationTime">Creation Time of task</param>
        /// <param name="title">Title of task - has to stand conditions</param>
        /// <param name="description">Description of task - has to stand conditions</param>
        /// <param name="dueDate">Due date of task - has to stand conditions</param>
        /// <param name="assignee">Assignee of task</param>
        /// <param name="boardCreator">Creator of the board the task is in - delivered to the created DTO</param>
        /// <param name="boardName">Board name of the board the task is in - delivered to the created DTO</param>
        /// <remarks>the added task will always be added to the Backlog column</remarks>
        /// <returns>the newly added task</returns>
        void IBoard.AddTask(ITask task) 
        {
            _columns[0].AddTask(task);
            _taskIdCounter++;
        }

        /// <summary>
        /// Gets specific task from specific column if exists
        /// </summary>
        /// <param name="columnOrdinal"></param>
        /// <param name="taskID"></param>
        /// <returns>the requested task</returns>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        ITask IBoard.GetTask(int columnOrdinal, int taskID)
        {
            CheckColumnOrdinal(columnOrdinal);
            return _columns[columnOrdinal].GetTask(taskID);
        }

        /// <summary>
        /// Gets specific task from board if exists
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns>the requested task</returns>
        /// <exception cref="IndexOutOfRangeException">thrown if the task does not exists in the board</exception>
        ITask IBoard.GetTask(int taskID)
        {
            if (taskID >= _taskIdCounter || taskID < 0)
            {
                throw new IndexOutOfRangeException($"No task with ID {taskID} in board");
            }
            ITask task = null;
            for (int i = 0; task == null; i++)
            {
                try
                {
                    task = _columns[i].GetTask(taskID);
                }
                catch (Exception) { }
            }
            return task;
        }
        
        /// <summary>
        /// updates an existing task's assignee
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="columnOrdinal">which column the task is in</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="assignee">new assignee</param>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        void IBoard.AssignTask(string userEmail, int columnOrdinal, int taskId, string assignee)
        {
            CheckColumnOrdinal(columnOrdinal);
            _columns[columnOrdinal].AssignTask(userEmail, taskId, assignee);
        }
        
        /// <summary>
        /// updates an existing task's due date
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="columnOrdinal">which column the task is in</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="DueDate">new and updated due date</param>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        void IBoard.UpdateTaskDueDate(string userEmail, int columnOrdinal, int taskId, DateTime DueDate)
        {
            CheckColumnOrdinal(columnOrdinal);
            _columns[columnOrdinal].UpdateTaskDueDate(userEmail, taskId, DueDate);
        }

        /// <summary>
        /// updates an existing task's title
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="columnOrdinal">which column the task is in</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="title">new and updated title</param>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        void IBoard.UpdateTaskTitle(string userEmail, int columnOrdinal, int taskId, string title)
        {
            CheckColumnOrdinal(columnOrdinal);
            _columns[columnOrdinal].UpdateTaskTitle(userEmail, taskId, title);
        }
        
        /// <summary>
        /// updates an existing task's description
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="columnOrdinal">which column the task is in</param>
        /// <param name="taskId">task's ID</param>
        /// <param name="description">new and updated description</param>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        void IBoard.UpdateTaskDescription(string userEmail, int columnOrdinal, int taskId, string description)
        {
            CheckColumnOrdinal(columnOrdinal);
            _columns[columnOrdinal].UpdateTaskDescription(userEmail, taskId, description);
        }
        
        /// <summary>
        /// advances a task to the next column
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="columnOrdinal">which column the task is in</param>
        /// <param name="taskId">the advanced task's ID</param>
        /// <exception cref="OutOfMemoryException">Thrown if the next column is already at its limit</exception>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        void IBoard.AdvanceTask(string userEmail, int columnOrdinal, int taskId)
        {
            CheckColumnOrdinal(columnOrdinal);
            ITask task = _columns[columnOrdinal].RemoveTask(userEmail, taskId); //removes task from current column
            try
            {
                _columns[columnOrdinal + 1].AddTask(task); //adds the task to the next column
                task.Advance();
            }
            catch (OutOfMemoryException e) //if the next column is already at its limit
            {
                _columns[columnOrdinal].AddTask(task); //returns the task to its original column
                throw new OutOfMemoryException(e.Message); //sends forward the same exception
            }
        }

        /// <summary>
        /// Removes specific task from specific column if exists
        /// </summary>
        /// <param name="userEmail">calling user's email, must be assignee of task</param>
        /// <param name="columnOrdinal"></param>
        /// <param name="taskID"></param>
        /// <returns>removed task</returns>
        ITask IBoard.RemoveTask(string userEmail, int columnOrdinal, int taskID)
        {
            CheckColumnOrdinal(columnOrdinal);
            ITask task = _columns[columnOrdinal].RemoveTask(userEmail, taskID);
            _taskIdCounter--;
            return task;
        }

        /// <summary>
        /// Removes specific task from board if exists
        /// </summary>
        /// <param name="userEmail">calling user's email, must be assignee of task</param>
        /// <param name="taskID"></param>
        /// <returns>the removed task</returns>
        ITask IBoard.RemoveTask(string userEmail, int taskID)
        {
            if (taskID >= _taskIdCounter)
            {
                throw new IndexOutOfRangeException($"No task with ID {taskID} in board");
            }
            ITask task = null;
            for (int i = 0; task == null; i++)
            {
                try
                {
                    task = _columns[i].RemoveTask(userEmail, taskID);
                }
                catch (Exception) { }
            }
            _taskIdCounter--;
            return task;
        }

        /// <summary>
        /// Returns all the tasks of spcified column
        /// </summary>
        /// <param name="columnOrdinal">represents the requested column</param>
        /// <returns>IList<Task> containing all the tasks</Task></returns>
        /// <remarks>calls CheckColumnOrdinal</remarks>
        IList<ITask> IBoard.GetColumnTasks(int columnOrdinal)
        {
            CheckColumnOrdinal(columnOrdinal);
            return _columns[columnOrdinal].GetTasks();
        }

        /// <summary>
        /// Gets all the columns of the board
        /// </summary>
        /// <returns>List of Columns</returns>
        IList<IColumn> IBoard.GetColumns()
        {
            return _columns;
        }

        /// <summary>
        /// Gets all 'InProgress' tasks of the board
        /// </summary>
        /// <returns>List of tasks in progress</returns>
        IList<ITask> IBoard.GetInProgressTasks()
        {
            IList<ITask> inProgress = new List<ITask>();
            for (int i = 1; i < _columnCounter-1; i++)
            {
                foreach (ITask task in _columns[i].GetTasks())
                {
                    inProgress.Add(task);
                }
            }
            return inProgress;
        }

        /// <summary>
        /// checks legality of columnOrdinal
        /// </summary>
        /// <param name="columnOrdinal">columnOrdinal that needs to be checked</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given ordinal isn't legal</exception>
        private void CheckColumnOrdinal(int columnOrdinal)
        {
            if (columnOrdinal < 0 || columnOrdinal >= _columnCounter)
            {
                throw new ArgumentException($"{_columnCounter-1}");
            }
        }
    }
}
