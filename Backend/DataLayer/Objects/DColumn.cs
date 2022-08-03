using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace IntroSE.Kanban.Backend.DataLayer
{
    internal class DColumn : DTO
    {
        // properties

        private string _name;
        private string _creatorEmail;
        private string _boardName;
        private int _ordinal;
        private int _limit;
        private List<DTask> _tasks;

        private const string COL_NAME = "Name";
        private const string COL_CREATOR_EMAIL = "BoardCreator";
        private const string COL_BOARD_NAME = "BoardName";
        private const string COL_LIMIT = "Limit";
        private const string COL_ORDINAL = "Ordinal";
        private const string TASK_TABLE_NAME = "Task";

        internal string Name
        {
            get => _name;
            set
            {
                if (Persist)
                {
                    Update(COL_NAME, value);
                }
                _name = value;
            }
        }

        internal string CreatorEmail
        {
            get => _creatorEmail; set
            {
                if (Persist)
                {
                    Update(COL_CREATOR_EMAIL, value);
                }
                _creatorEmail = value;
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

        internal int Limit
        {
            get => _limit; set
            {
                if (Persist)
                {
                    Update(COL_LIMIT, value);
                }
                _limit = value;
            }
        }
        internal int Ordinal
        {
            get => _ordinal;
            set
            {
                if (Persist)
                {

                    Update(COL_ORDINAL, value);
                    Update(COL_ID, CreatorEmail + BoardName + value + Name);
                    ChangeTaskOrdinal(value);
                }
                
                _ordinal = value;
            }
        }

        internal List<DTask> Tasks
        {
            get => _tasks; set
            {
                _tasks = value;
            }
        }


        // constructor

        internal DColumn(string name, string creatorEmail, string boardName, int ordinal, int limit) : base(creatorEmail + boardName + ordinal +name, "Column")
        {
            _creatorEmail = creatorEmail;
            _boardName = boardName;
            _ordinal = ordinal;
            _limit = limit;
            _name = name;
        }

        protected override SQLiteCommand InsertCommand(SQLiteConnection connection)
        {
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = $"INSERT INTO {_tableName}  VALUES (@{COL_ID},@{COL_NAME}, @{COL_CREATOR_EMAIL}, @{COL_BOARD_NAME}, @{COL_ORDINAL}, @{COL_LIMIT})"
            };
            command.Parameters.Add(new SQLiteParameter(COL_ID, Id));
            command.Parameters.Add(new SQLiteParameter(COL_NAME, _name));
            command.Parameters.Add(new SQLiteParameter(COL_CREATOR_EMAIL, CreatorEmail));
            command.Parameters.Add(new SQLiteParameter(COL_BOARD_NAME, BoardName));
            command.Parameters.Add(new SQLiteParameter(COL_ORDINAL, Ordinal));
            command.Parameters.Add(new SQLiteParameter(COL_LIMIT, Limit));
            return command;
        }

        internal override void Remove()
        {
            base.Remove();
            bool errorOcurred = false;
            int res = -1;
            string thisColumn = "ThisColumn";
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = $"update {TASK_TABLE_NAME} set [{COL_ORDINAL}]=@{COL_ORDINAL} where ({COL_BOARD_NAME}=@{COL_BOARD_NAME} AND {COL_CREATOR_EMAIL}=@{COL_CREATOR_EMAIL} AND {COL_ORDINAL}=@{thisColumn}) "
                };
                try
                {
                    command.Parameters.Add(new SQLiteParameter(COL_ORDINAL, Ordinal - 1));
                    command.Parameters.Add(new SQLiteParameter(COL_BOARD_NAME, BoardName));
                    command.Parameters.Add(new SQLiteParameter(COL_CREATOR_EMAIL, CreatorEmail));
                    command.Parameters.Add(new SQLiteParameter(thisColumn, Ordinal));
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    errorOcurred = true;
                    log.Error($"Failed to update data in the DB, tried command: {command.CommandText},\n" +
                         $"the SQLite exception massage was: {e.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                    if (errorOcurred)
                        throw new InvalidOperationException();
                }

            }
            if (res <= 0)
            {
                log.Error($"SQLite Update query in table '{_tableName}' returned {res}.");
                throw new Exception("The data didn't update correctly");
            }

            foreach (DTask dTask in Tasks)
            {
                dTask.ReduceOrdinal();
            }

        }

        internal void ChangeTaskOrdinal(int newOrdinal)
        {
            bool errorOcurred = false;
            int res = -1;
            string thisColumn = "ThisColumn";
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = $"update {TASK_TABLE_NAME} set [{COL_ORDINAL}]=@{COL_ORDINAL} where ({COL_BOARD_NAME}=@{COL_BOARD_NAME} AND {COL_CREATOR_EMAIL}=@{COL_CREATOR_EMAIL} AND {COL_ORDINAL}=@{thisColumn}) "
                };
                try
                {
                    command.Parameters.Add(new SQLiteParameter(COL_ORDINAL, newOrdinal));
                    command.Parameters.Add(new SQLiteParameter(COL_BOARD_NAME, BoardName));
                    command.Parameters.Add(new SQLiteParameter(COL_CREATOR_EMAIL, CreatorEmail));
                    command.Parameters.Add(new SQLiteParameter(thisColumn, Ordinal));
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    errorOcurred = true;
                    log.Error($"Failed to update data in the DB, tried command: {command.CommandText},\n" +
                         $"the SQLite exception massage was: {e.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                    if (errorOcurred)
                        throw new InvalidOperationException();
                }

            }
        }
    }
}
