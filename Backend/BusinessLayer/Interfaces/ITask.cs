using System;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal interface ITask
    {
        //members
        int ID { get; }
        string Title { get; set; }
        string Description { get; set; }
        string Assignee { get; set; }
        DateTime CreationTime { get; } 
        DateTime DueDate { get; set; }

        //methods
        void persist();
        void Advance();
    }
}
