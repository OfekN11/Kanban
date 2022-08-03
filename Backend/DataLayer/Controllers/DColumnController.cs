using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace IntroSE.Kanban.Backend.DataLayer
{
    internal class DColumnController : DalController <DColumn>
    {
        // field
        private DTaskController _taskController;
        private const string COL_CREATOR_EMAIL = "Creator";
        private const string COL_BOARD_NAME = "Board";

        // controller
        internal DColumnController() : base("Column")
        {
            _taskController = new DTaskController();
        }

        // methods

        internal List<DColumn> Select(string boardCreator, string boardName)
        {
            List<DColumn> results = new List<DColumn>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"select * from {_tableName} WHERE ({COL_CREATOR_EMAIL} = @{COL_CREATOR_EMAIL} AND {COL_BOARD_NAME} = @{COL_BOARD_NAME})";

                SQLiteDataReader dataReader = null;
                try
                {
                    connection.Open();
                    command.Parameters.Add(new SQLiteParameter(COL_CREATOR_EMAIL, boardCreator));
                    command.Parameters.Add(new SQLiteParameter(COL_BOARD_NAME, boardName));
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        DColumn column = ConvertReaderToObject(dataReader);
                        column.Tasks = _taskController.Select(boardCreator, boardName, column.Ordinal).ToList();
                        results.Add(column);
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Failed to load data from DB, tried command: {command.CommandText},\n" +
                         $"the SQLite exception massage was: {e.Message}");
                }
                finally
                {
                    if (dataReader != null)
                    {
                        dataReader.Close();
                    }

                    command.Dispose();
                    connection.Close();
                }
            }
            return results;
        }

        internal override void DeleteAll()
        {
            base.DeleteAll();
            _taskController.DeleteAll();
        }

        internal void DeleteBoardColumns(string creatorEmail, string boardName)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"DELETE FROM {_tableName} WHERE ({COL_CREATOR_EMAIL} = @{COL_CREATOR_EMAIL} AND {COL_BOARD_NAME} = @{COL_BOARD_NAME})";
                command.Parameters.Add(new SQLiteParameter(COL_CREATOR_EMAIL, creatorEmail));
                command.Parameters.Add(new SQLiteParameter(COL_BOARD_NAME, boardName));
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    log.Error($"Failed to delete board's columns from DB, tried command: {command.CommandText},\n" +
                        $"the SQLite exception massage was: {e.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
            }
            _taskController.DeleteBoardtask(creatorEmail, boardName);
        }

        protected override DColumn ConvertReaderToObject(SQLiteDataReader reader)
        {
            string name = reader.GetString(1);
            string creator = reader.GetString(2);
            string boardName = reader.GetString(3);
            int ordinal = reader.GetInt32(4);
            int limit = reader.GetInt32(5);

            DColumn result = new DColumn(name, creator, boardName, ordinal, limit);
            return result;
        }
    }
}
