using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace IntroSE.Kanban.Backend.DataLayer
{
    internal class DBoardController : DalController <DBoard>
    {
        
        // fields

        DColumnController _columnController;
        BoardMemberController _boardMemberController;

        // constructor

        internal DBoardController() : base("Board")
        {
            _columnController = new DColumnController();
            _boardMemberController = new BoardMemberController();
        }

        
        // methods

        internal override List<DBoard> Select()
        {
            List<DBoard> results = base.Select(); 

            foreach(DBoard dBoard in results)
            {
                dBoard.Columns = _columnController.Select(dBoard.CreatorEmail, dBoard.BoardName);
                dBoard.Members = _boardMemberController.Select(dBoard.Id);
            }

            return results;
        }

        internal override void DeleteAll()
        {
            base.DeleteAll();
            _columnController.DeleteAll();
            _boardMemberController.DeleteAll();
        }

        internal void DeleteBoard(string creatorEmail, string boardName)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"DELETE FROM {_tableName} WHERE {COL_ID} = @{COL_ID}";
                command.Parameters.Add(new SQLiteParameter(COL_ID, creatorEmail + boardName));
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    log.Error($"Failed to delete board '{boardName}' of '{creatorEmail}', tried command: {command.CommandText},\n" +
                        $"the SQLite exception massage was: {e.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
            }
            _boardMemberController.DeleteBoardMembers(creatorEmail, boardName);
            _columnController.DeleteBoardColumns(creatorEmail, boardName);
        }

        protected override DBoard ConvertReaderToObject(SQLiteDataReader reader)
        {
            string creator = reader.GetString(1);
            string name = reader.GetString(2);
            return new DBoard(creator, name);
        }
    }
}