using System;
using System.Data.SQLite;

namespace IntroSE.Kanban.Backend.DataLayer
{
    internal class DTask : DTO
    {
        // properties

        private readonly int taskId;
        private string title;
        private string description;
        private readonly DateTime creationTime;
        private DateTime dueDate;
        private string assignee;
        private int _ordinal;
        private string _boardCreator;
        private string _boardName;

        private const string COL_TASK_ID = "TaskId";
        private const string COL_TITLE = "Title";
        private const string COL_DESCRIPTION = "Description";
        private const string COL_CREATIONTIME = "CreationTime";
        private const string COL_DUE_DATE = "DueDate";
        private const string COL_ASSIGNEE = "Assignee";
        private const string COL_ORDINAL = "Ordinal";
        private const string COL_CREATOR_EMAIL = "BoardCreator";
        private const string COL_BOARD_NAME = "BoardName";

        internal int TaskId
        { get { return taskId; } }

        internal string Title
        {
            get { return title; }
            set
            {
                if (Persist)
                {
                    Update(COL_TITLE, value);
                }
                title = value;
            }
        }

        internal string Description
        {
            get { return description; }
            set
            {

                if (Persist)
                {
                    Update(COL_DESCRIPTION, value);
                }
                description = value;
            }
        }

        internal DateTime CreationTime { get { return creationTime; } }

        internal DateTime DueDate
        {
            get { return dueDate; }
            set
            {

                if (Persist)
                {
                    Update(COL_DUE_DATE, value.ToString());
                }
                dueDate = value;
            }
        }

        internal string Assignee
        {
            get { return assignee; }
            set
            {
                if (Persist)
                {
                    Update(COL_ASSIGNEE, value);
                }
                assignee = value;
            }
        }

        internal int Ordinal
        {
            get => _ordinal; set
            {
                if (Persist)
                {
                    Update(COL_ORDINAL, value);
                }
                _ordinal = value;
            }
        }

        internal string BoardCreator
        {
            get => _boardCreator; set
            {
                if (Persist)
                {
                    Update(COL_CREATOR_EMAIL, value);
                }
                _boardCreator = value;
            }
        }

        internal string BoardName
        {
            get => _boardName; set
            {
                if (Persist)
                {
                    Update(COL_BOARD_NAME, value);
                }
                _boardName = value;
            }
        }

        // constructor
        internal DTask(int taskId, DateTime creationTime, string title, string description, DateTime dueDate, string assignee, int ordinal, string boardCreator, string boardName) : base(boardCreator + boardName + taskId, "Task")
        {
            this.taskId = taskId;
            Title = title;
            Description = description;
            this.creationTime = creationTime;
            DueDate = dueDate;
            Assignee = assignee;
            Ordinal = ordinal;
            BoardCreator = boardCreator;
            BoardName = boardName;
        }

        // method
        protected override SQLiteCommand InsertCommand(SQLiteConnection connection)
        {
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = $"INSERT INTO {_tableName}  VALUES (@{COL_ID},@{COL_TASK_ID}, @{COL_CREATIONTIME}, @{COL_TITLE}, @{COL_DESCRIPTION}, @{COL_DUE_DATE}, @{COL_ASSIGNEE}, @{COL_ORDINAL}, @{COL_CREATOR_EMAIL}, @{COL_BOARD_NAME})"
            };
            command.Parameters.Add(new SQLiteParameter(COL_ID, Id));
            command.Parameters.Add(new SQLiteParameter(COL_TASK_ID,TaskId));
            command.Parameters.Add(new SQLiteParameter(COL_CREATIONTIME, CreationTime));
            command.Parameters.Add(new SQLiteParameter(COL_TITLE,Title));
            command.Parameters.Add(new SQLiteParameter(COL_DESCRIPTION, Description));
            command.Parameters.Add(new SQLiteParameter(COL_DUE_DATE, DueDate));
            command.Parameters.Add(new SQLiteParameter(COL_ASSIGNEE, Assignee));
            command.Parameters.Add(new SQLiteParameter(COL_ORDINAL, Ordinal));
            command.Parameters.Add(new SQLiteParameter(COL_CREATOR_EMAIL, BoardCreator));
            command.Parameters.Add(new SQLiteParameter(COL_BOARD_NAME, BoardName));

            return command;
        }

        internal void ReduceOrdinal()
        {
            _ordinal = _ordinal - 1;
        }
    }
}
