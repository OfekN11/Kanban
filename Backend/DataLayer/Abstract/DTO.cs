using System;
using System.Data.SQLite;
using System.IO;
using log4net;
using log4net.Config;
using System.Reflection;

namespace IntroSE.Kanban.Backend.DataLayer
{
    internal abstract class DTO
    {

        // properties

        private string _id;
        protected const string COL_ID = "ID";
        protected readonly string _connectionString;
        protected readonly string _tableName;
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal string Id
        {
            get => _id; set
            {
                if (Persist)
                {
                  Update(COL_ID, value);
                }
                _id = value;
            }
        }

        internal bool Persist { get; set; }


        // constructor

        internal DTO(string id, string tableName)
        {
            Persist= false;
            string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "kanban.db"));
            this._connectionString = $"Data Source={path}; Version=3;";
            this._tableName = tableName;
            Id = id;

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }


        // methods

        internal void Insert()
        {
            bool errorOcurred = false;
            int res = -1;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = InsertCommand(connection);
                try             
                {
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    errorOcurred = true;
                    log.Error($"Failed to insert data to the DB, tried command: {command.CommandText},\n" +
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
                log.Error($"SQLite Insert query in table '{_tableName}' returned {res}.");
                throw new Exception("The data didn't save correctly");
            }

        }

        protected void Update(string attributeName, string attributeValue)
        {
            bool errorOcurred = false;
            int res = -1;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection, 
                    CommandText = $"update {_tableName} set [{attributeName}]=@{attributeName} where {COL_ID}=@{COL_ID}"
                };
                try
                {
                    command.Parameters.Add(new SQLiteParameter(attributeName, attributeValue));
                    command.Parameters.Add(new SQLiteParameter(COL_ID, Id));
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

        protected void Update(string attributeName, long attributeValue)
        {
            bool errorOcurred = false;
            int res = -1;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = $"update {_tableName} set [{attributeName}]=@{attributeName} where {COL_ID} = @{COL_ID}"
                };

                try
                {
                    command.Parameters.Add(new SQLiteParameter(attributeName, attributeValue));
                    command.Parameters.Add(new SQLiteParameter(COL_ID, Id));
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

        protected void Update(string attributeName, DateTime attributeValue)
        {
            bool errorOcurred = false;
            int res = -1;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = $"update {_tableName} set [{attributeName}]=@{attributeName} where {COL_ID} =@ {COL_ID}"
                };

                try
                {
                    command.Parameters.Add(new SQLiteParameter(attributeName, attributeValue));
                    command.Parameters.Add(new SQLiteParameter(COL_ID,Id));
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
        }

        /// <summary>
        /// this function remove the DTO line from the relative table
        /// </summary>
        /// <remarks> this function do not delete related line</remarks>
        internal virtual void Remove()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"DELETE FROM {_tableName} WHERE {COL_ID} = @{COL_ID}";
                command.Parameters.Add(new SQLiteParameter(COL_ID, Id));
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

        protected abstract SQLiteCommand InsertCommand(SQLiteConnection connection);

    }
}
