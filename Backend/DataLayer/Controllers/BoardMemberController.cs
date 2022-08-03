using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using log4net;
using log4net.Config;
using System.Reflection;

namespace IntroSE.Kanban.Backend.DataLayer
{
    internal class BoardMemberController 
    {

        // properties

        private readonly string _connectionString;
        private const string _tableName = "BoardMember";
        private const string CAL_ID = "ID";
        private const string CAL_USER = "User";
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // constructor

        internal BoardMemberController()
        {
            string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "kanban.db"));
            this._connectionString = $"Data Source={path}; Version=3;";

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }


        // methods

        internal HashSet<string> Select(string ID)  // email, set of boards with syntax creatorEmail:boardName
        {
            HashSet<string> results = new HashSet<string>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"select * from {_tableName} WHERE ({CAL_ID} = @{CAL_ID})";
                SQLiteDataReader dataReader = null;

                try
                {
                    command.Parameters.Add(new SQLiteParameter(CAL_ID, ID));
                    connection.Open();
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        results.Add(dataReader.GetString(1));
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Failed to load data from the DB, tried command: {command.CommandText},\n" +
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

        internal void DeleteAll()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"DELETE FROM {_tableName}";

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    log.Error($"Failed to Delete '{_tableName}' from the DB, tried command: {command.CommandText},\n" +
                        $"the SQLite exception massage was: {e.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
            }
        }

        internal void DeleteBoardMembers(string creatorEmail, string boardName)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"DELETE FROM {_tableName} WHERE {CAL_ID} = @{CAL_ID}";
                try
                {
                    command.Parameters.Add(new SQLiteParameter(CAL_ID, creatorEmail + boardName));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    log.Error($"Failed to Delete board members from the DB, tried command: {command.CommandText},\n" +
                        $"the SQLite exception massage was: {e.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
            }
        }
        internal void Insert(string ID, string userEmail)
        {
            bool failed = false;
            int res = -1;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = $"INSERT INTO {_tableName}  VALUES (@{CAL_ID}, @{CAL_USER})"
                };

                try
                {
                    command.Parameters.Add(new SQLiteParameter(CAL_ID, ID));
                    command.Parameters.Add(new SQLiteParameter(CAL_USER, userEmail));
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    //log
                    failed = true;
                    log.Error($"Failed to Insert to the DB, tried command: {command.CommandText},\n" +
                        $"the SQLite exception massage was: {e.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                    if (failed)
                        throw new InvalidOperationException();
                }
                
                if (res <= 0)
                    throw new Exception($"SQLite Update query '{command.CommandText}' returned {res}.");
            }

        }
    }
}
