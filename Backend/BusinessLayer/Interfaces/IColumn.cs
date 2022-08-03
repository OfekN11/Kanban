using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal interface IColumn
    {
        //members 
        string Name { get; set; }
        int Ordinal { get; set; }
        int Limit { get; set; }

        //methods
        void Persist();
        void AddTask(ITask task);
        void AddTasks(IList<ITask> tasks);
        ITask GetTask(int taskId);
        IList<ITask> GetTasks();
        void AssignTask(string userEmail, int taskId, string assignee);
        void UpdateTaskDueDate(string userEmail, int taskId, DateTime DueDate);
        void UpdateTaskTitle(string userEmail, int taskId, string title);
        void UpdateTaskDescription(string userEmail, int taskId, string description);
        ITask RemoveTask(string userEmail, int taskId);
    }
}
