using System;
using System.Collections.Generic;
using DColumn = IntroSE.Kanban.Backend.DataLayer.DColumn;
using DTask = IntroSE.Kanban.Backend.DataLayer.DTask;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// Class Column stores tasks and allows column operations and manipulation.
    /// </summary>
    /// <remarks>All exceptions thrown will message the column's name for parsing uses of the calling method</remarks>
    internal class Column : IColumn
    {
        //fields
        private string _name;
        private Dictionary<int, ITask> _tasks = new Dictionary<int, ITask>(); //key - task's ID
        private int _limit = -1; //ranges from -1 (unlimited) to any positive number (actual limit)

        private DColumn _dColumn; //parallel DTO

        string IColumn.Name { 
            get => _name;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Column Name cannot be null");
                if (value.Equals(""))
                    throw new ArgumentException("New Name cannot be empty");
                _name = value;
                _dColumn.Name = value;

            }
        }
        int IColumn.Ordinal
        {
            get => _dColumn.Ordinal;
            set => _dColumn.Ordinal = value;
        }
        int IColumn.Limit
        {
            get => _limit;
            set
            {
                if (value != -1 && _tasks.Count > value) 
                    throw new InvalidOperationException(_name);
                _limit = value;
                _dColumn.Limit = value;
            }
        }

        //constructors
        internal Column(string name, string creatorEmail, string boardName, int columnOrdinal)
        {
            this._name = name;
            _dColumn = new DColumn(name, creatorEmail, boardName, columnOrdinal, -1);
        }

        /// <summary>
        /// Recreates Column from DTO
        /// </summary>
        /// <param name="dColumn">DTO representing the column</param>
        internal Column(DColumn dColumn)
        {
            _limit = dColumn.Limit;
            _name = dColumn.Name;
            foreach (DTask dTask in dColumn.Tasks)
            {
                _tasks[dTask.TaskId] = new Task(dTask);
            }
            this._dColumn = dColumn;
            this._dColumn.Persist = true;
        }

        //methods
        void IColumn.Persist()
        {
            _dColumn.Insert();
            _dColumn.Persist = true;
        }

        /// <summary>
        /// Adds the given task to the column
        /// </summary>
        /// <param name="task">the added task</param>
        /// <exception cref="OutOfMemoryException">Thrown when column is already at its limit</exception>
        void IColumn.AddTask(ITask task)
        {
            //no need to check that limit != -1 as then the boolean check will always be false anyway
            if (_tasks.Count == _limit)
                throw new OutOfMemoryException(_name);
            _tasks[task.ID] = task;
        }

        /// <summary>
        /// adds all given tasks in the IList to the column
        /// </summary>
        /// <param name="tasks">new tasks</param>
        void IColumn.AddTasks(IList<ITask> tasks)
        {
            if (_limit != -1 && (tasks.Count + this._tasks.Count) > _limit)
                throw new OutOfMemoryException(_name);
            foreach (ITask task in tasks)
            {
                tasks[task.ID] = task;
            }
        }

        /// <summary>
        /// Gets requested task
        /// </summary>
        /// <param name="taskId">task ID</param>
        /// <returns>requested task</returns>
        ITask IColumn.GetTask(int taskId)
        {
            if (!_tasks.ContainsKey(taskId))
                throw new IndexOutOfRangeException(_name);
            return _tasks[taskId];
        }

        /// <summary>
        /// Gets all the tasks in the column
        /// </summary>
        /// <returns>Lists of tasks</returns>
        IList<ITask> IColumn.GetTasks()
        {
            IList<ITask> tasks = new List<ITask>(_tasks.Values);
            return tasks;
        }

        /// <summary>
        /// Updates a contained task's due date
        /// </summary>
        /// <param name="userEmail">callig user's email</param>
        /// <param name="taskId">ID of updated task</param>
        /// <param name="assignee">new assignee</param>
        /// <remarks>calls validateAssignee</remarks>
        void IColumn.AssignTask(string userEmail, int taskId, string assignee)
        {
            validateAssignee(userEmail, taskId);
            _tasks[taskId].Assignee = assignee;
        }

        /// <summary>
        /// Updates a contained task's due date
        /// </summary>
        /// <param name="userEmail">callig user's email</param>
        /// <param name="taskId">ID of updated task</param>
        /// <param name="DueDate">new and updated due date</param>
        /// <remarks>calls validateAssignee</remarks>
        void IColumn.UpdateTaskDueDate(string userEmail, int taskId, DateTime DueDate)
        {
            validateAssignee(userEmail, taskId);
            _tasks[taskId].DueDate = DueDate;
        }

        /// <summary>
        /// Updates a contained task's title
        /// </summary>
        /// <param name="userEmail">callig user's email</param>
        /// <param name="taskId">ID of updated task</param>
        /// <param name="title">new and updated title</param>
        /// <remarks>calls validateAssignee</remarks>
        void IColumn.UpdateTaskTitle(string userEmail, int taskId, string title)
        {
            validateAssignee(userEmail, taskId);
            _tasks[taskId].Title = title;
        }

        /// <summary>
        /// Updates a contained task's description
        /// </summary>
        /// <param name="userEmail">callig user's email</param>
        /// <param name="taskId">ID of updated task</param>
        /// <param name="description">new and updated description</param>
        /// <remarks>calls validateAssignee</remarks>
        void IColumn.UpdateTaskDescription(string userEmail, int taskId,  string description)
        {
            validateAssignee(userEmail, taskId);
            _tasks[taskId].Description = description;
        }

        /// <summary>
        /// Removes task from the column
        /// </summary>
        /// <param name="taskId">ID of the removed task</param>
        /// <returns>the removed task</returns>
        /// <remarks>calls validateAssignee</remarks>
        ITask IColumn.RemoveTask(string userEmail, int taskId)
        {
            validateAssignee(userEmail, taskId);
            ITask removedTask = _tasks[taskId];
            _tasks.Remove(taskId);
            return removedTask;
        }

        /// <summary>
        /// Validates the working user is the assignee of the task
        /// </summary>
        /// <param name="userEmail">calling user's email</param>
        /// <param name="taskId">task's ID</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when the column doesn't hold a task with matching given ID</exception>
        private void validateAssignee(string userEmail, int taskId)
        {
            try
            {
                if (!_tasks[taskId].Assignee.Equals(userEmail))
                    throw new InvalidOperationException();
            }
            catch (KeyNotFoundException)
            {
                throw new IndexOutOfRangeException(_name);
            }
        }
    }
}
