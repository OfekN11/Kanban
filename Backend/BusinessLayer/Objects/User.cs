using DUser = IntroSE.Kanban.Backend.DataLayer.DUser;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class User : IUser
    {
        //fields

        private readonly string _email;
        private string _password;
        private DUser _dUser;

        string IUser.Email { get => _email; }


        //constructor

        internal User(string email, string password)
        {
            this._email = email;
            this._password = password;
            _dUser = new DUser(_email, _password);
        }

        //constructor for Duser objects
        internal User(DUser dUser)
        {
            _email = dUser.Email;
            _password = dUser.Password;
            _dUser = dUser;
            _dUser.Persist = true;

        }

        //methods

        void IUser.Persist()
        {
            _dUser.Insert();
            _dUser.Persist = true;
        }

        /// <summary>validate password with the user saved password.</summary>
        /// <param name="password">the password given for check.</param>
        /// <returns>true/false accordingly.</returns>
        bool IUser.ValidatePassword(string password)
        {
            return _password.Equals(password);
        }
    }
}



