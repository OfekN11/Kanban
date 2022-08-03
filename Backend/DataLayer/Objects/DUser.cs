using System.Data.SQLite;

namespace IntroSE.Kanban.Backend.DataLayer
{
    internal class DUser : DTO
    {
        // properties

        private readonly string email;
        private string password;
        private const string COL_EMAIL = "Email";
        private const string COL_PASSWORD = "Password";

        internal string Email { get { return email; } }
        internal string Password { get { return password; } }

        // constructor

        internal DUser(string email, string password) : base(email, "User")
        {
            this.email = email;
            this.password = password;
        }


        // method

        protected override SQLiteCommand InsertCommand(SQLiteConnection connection)
        {
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = $"INSERT INTO {_tableName}  VALUES (@{COL_ID}, @{COL_EMAIL}, @{COL_PASSWORD})"
            };
            command.Parameters.Add(new SQLiteParameter(COL_ID, Id));
            command.Parameters.Add(new SQLiteParameter(COL_EMAIL, Email));
            command.Parameters.Add(new SQLiteParameter(COL_PASSWORD, Password));

            return command;
        }
    }
}

