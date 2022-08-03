using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace IntroSE.Kanban.Backend.DataLayer
{
    internal class DBoard : DTO
    {

        //properties

        private readonly string _creatorEmail;
        private string _boardName;
        private List<DColumn> _columns;
        private HashSet<string> _members;
        private BoardMemberController _boardMemberController;

        private const string COL_BOARD_NAME = "Name";
        private const string COL_CREATOR_EMAIL = "Creator";
        private const string COLUMN_TABLE_NAME = "Column";


        internal string CreatorEmail { get => _creatorEmail;}

        internal string BoardName { get => _boardName; set 
            {

                if (Persist)
                {
                    Update(COL_BOARD_NAME, value);
                }
                _boardName = value;
            } }

        internal List<DColumn> Columns { get => _columns; set => _columns = value; }

        internal HashSet<string> Members { get => _members; set => _members = value; }

        //constructor
        internal DBoard (string creatorEmail, string boardName) : base(creatorEmail + boardName,"Board")
        {
            this._creatorEmail = creatorEmail;
            this._boardName = boardName;
            _columns = new List<DColumn>();
            _members = new HashSet<string>();
            _boardMemberController = new BoardMemberController();
        }

        //methods
        internal int numberOfTasks()
        {
            int output = 0;
            foreach (DColumn column in _columns)
            {
                output = output + column.Tasks.Count;
            }
            return output;
        }

        internal void AddMember (string memberEmail)
        {
             _boardMemberController.Insert(Id, memberEmail);
             Members.Add(memberEmail);
        }

        internal void RemoveColumn(int columnOrdinal)
        {
            string var1 = "Creator";
            string var2 = "Board";
            string var3 = "Ordinal";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"DELETE FROM {COLUMN_TABLE_NAME} WHERE ({var1}= @{var1} AND {var2}=@{var2} AND {var3}=@{var3})";
                command.Parameters.Add(new SQLiteParameter(var1, CreatorEmail));
                command.Parameters.Add(new SQLiteParameter(var2, BoardName));
                command.Parameters.Add(new SQLiteParameter(var3, columnOrdinal));
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    log.Error($"Failed to delete '{Id}', tried command: {command.CommandText},\n" +
                        $"the SQLite exception massage was: {e.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
            }
        }

        protected override SQLiteCommand InsertCommand(SQLiteConnection connection)
        {
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = $"INSERT INTO {_tableName}  VALUES (@{COL_ID}, @{COL_CREATOR_EMAIL}, @{COL_BOARD_NAME})"
            };
            command.Parameters.Add(new SQLiteParameter(COL_ID, Id));
            command.Parameters.Add(new SQLiteParameter(COL_CREATOR_EMAIL, CreatorEmail));
            command.Parameters.Add(new SQLiteParameter(COL_BOARD_NAME, BoardName));
            return command;
        }
    }

}
