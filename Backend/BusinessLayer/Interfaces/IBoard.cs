using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal interface IBoard
    {
        //members
        string Name { get; }
        string Creator { get; }
        int ColumnCount { get; }
        int TaskCount { get; }

        //methods
        void Persist();
        void AddMember(string memberEmail);
        void AddColumn(IColumn column);
        IColumn GetColumn(int columnOrdinal);
        void RenameColumn(int columnOrdinal, string newColumnName);
        void MoveColumn(int columnOrdinal, int shiftSize);
        void LimitColumn(int columnOrdinal, int limit);
        IColumn RemoveColumn(int columnOrdinal);
        IList<IColumn> GetColumns();
        void AddTask(ITask task);
        ITask GetTask(int columnOrdinal, int taskID);
        ITask GetTask(int taskID);
        void AssignTask(string userEmail, int columnOrdinal, int taskId, string assignee);
        void UpdateTaskDueDate(string userEmail, int columnOrdinal, int taskId, DateTime DueDate);
        void UpdateTaskTitle(string userEmail, int columnOrdinal, int taskId, string title);
        void UpdateTaskDescription(string userEmail, int columnOrdinal, int taskId, string description);
        void AdvanceTask(string userEmail, int columnOrdinal, int taskId);
        ITask RemoveTask(string userEmail, int columnOrdinal, int taskID);
        ITask RemoveTask(string userEmail, int taskID);
        IList<ITask> GetColumnTasks(int columnOrdinal);
        IList<ITask> GetInProgressTasks();
    }
}
