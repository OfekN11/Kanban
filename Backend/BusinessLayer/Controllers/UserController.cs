using System;
using log4net;
using log4net.Config;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using DUserController = IntroSE.Kanban.Backend.DataLayer.DUserController;
using DUser = IntroSE.Kanban.Backend.DataLayer.DUser;
using ConfigReader = IntroSE.Kanban.Backend.DataLayer.Controllers.ConfigReader;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class UserController
    {
        //fields

        private Dictionary<string, IUser> users; //key - email, value - user of that email
        private LoginInstance loginInstance;

        private DUserController dUserController; //parallel DController

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //password limiters
        private const int PASS_MIN_LENGTH = 4;
        private const int PASS_MAX_LENGTH = 20;
        private List<string> forbiddenPasswords = new List<string>();

        //constructors
        public UserController(LoginInstance loginInstance)
        {
            users = new Dictionary<string, IUser>();
            this.dUserController = new DUserController();
            this.loginInstance = loginInstance;
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        //methods

        ///<summary>This method loads the users data from the persistance </summary>
        internal void LoadData()
        {
            string errorMsg = null;
            IList<DUser> dUsers = null;
            try
            {
                dUsers = dUserController.Select();
            }
            catch (Exception e)
            {
                log.Fatal($"Failed to load data - {e.Message}");
                throw new Exception(e.Message);
            }

            foreach (DUser dUser in dUsers)
            {
                string email = dUser.Email;
                if (users.ContainsKey(email))
                {
                    log.Fatal($"FAILED to load user '{email}' - user already exists");
                    errorMsg = errorMsg + $"Couldn't load user '{email}' - user already exists\n";
                }
                else
                {
                    users[email] = new User(dUser);
                }
            }

            if (errorMsg != null)
                throw new Exception(errorMsg);

            this.forbiddenPasswords = ConfigReader.getInstance().ForbiddenPasswords;
        }

        ///<summary>Removes all persistent users data.</summary>
        internal void DeleteData()
        {
            users = new Dictionary<string, IUser>();
            if (loginInstance.ConnectedEmail != null)
            {
                loginInstance.Logout(loginInstance.ConnectedEmail);
            }
            try
            {
                dUserController.DeleteAll();
            }
            catch (Exception e)
            {
                log.Fatal($"FAILED to delete user data - {e.Message}");
                throw new Exception("Failed to delete user data");
            }
        }

        ///<summary>Registers a new user to the system.</summary>
        ///<param name="userEmail">the user e-mail address, used as the username for logging the system.</param>
        ///<param name="password">the user password.</param>
        ///<exception cref="Exception">thrown when email is null, not in email structure or when user with this email already exist.</exception>
        ///<returns>The User created by the registration.</returns>
        ///<remarks>calls CreateUser</remarks>
        internal IUser Register(string userEmail, string password)
        {
            if (users.ContainsKey(userEmail))
            {
                log.Warn($"FAILED register attempt: '{userEmail}' already exists");
                throw new Exception("A user already exist with this Email address");
            }
            IUser user = CreateUser(userEmail, password);
            try
            {
                user.Persist();
            }
            catch (InvalidOperationException)
            {
                log.Fatal($"FAILED to create user '{userEmail}' - exists in DataBase but not in BusinessLayer");
                throw new Exception($"Can't create user '{userEmail}' - this user already exists in the DataBase, please LoadData before continuing");
            }
            users[userEmail] = user;
            log.Info($"SUCCESSFULLY registered attempt: '{userEmail}'");
            return user;
        }

        /// <summary>
        /// Log in an existing user
        /// </summary>
        /// <param name="userEmail">The email address of the user to login</param>
        /// <param name="password">The password of the user to login</param>
        /// <returns cref="User">The User that logged in.</returns>
        /// <exception cref="Exception">thrown when a user with this email doesn't exist or when the password is incorrect.</exception>
        internal IUser Login(string userEmail, string password)
        {
            loginInstance.Login(userEmail); //login as check if even possible
            if (users.ContainsKey(userEmail))
            {
                IUser user = users[userEmail];
                if (user.ValidatePassword(password))
                {
                    log.Info("SUCCESSFULLY logged in: '" + userEmail + "'");
                    return user;
                }
            }
            loginInstance.Logout(userEmail); //logout again if logging in has failed at some point
            log.Warn($"FAILED log in attempt: '{userEmail}'");
            throw new Exception("Email or Password is invalid");
        }

        /// <summary>
        /// Log out an logged in user. 
        /// </summary>
        /// <param name="userEmail">The userEmail of the user to log out</param>
        internal void Logout(string userEmail)
        {
            try
            {
                loginInstance.Logout(userEmail);
                log.Info($"SUCCESSFULLY logged out: '{userEmail}'");
            }
            catch (Exception e)
            {
                log.Info($"FAILED to logout: '{userEmail}' tried to log out but wasn't logged in");
                throw new Exception(e.Message);
            }
        }

        private IUser CreateUser(string email, string password)
        {
            ValidateEmail(email);
            ValidatePasswordRules(password);
            return new User(email, password);
        }

        /// <summary>
        /// check if the input string match an email structure.
        /// </summary>
        /// <param name="email">the input email.</param>
        /// <exception cref="ArgumentNullException">Thrown if the given email is null</exception>
        /// <exception cref="ArgumentException">Thown if email is invalid</exception>
        private void ValidateEmail(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("Email cannot be null");
            }

            var emailValidator = new EmailAddressAttribute();
            Regex rg = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            bool validEmail = rg.IsMatch(email);
            var addr = new System.Net.Mail.MailAddress(email);
            if (!(addr.Address == email & emailValidator.IsValid(email) & validEmail))
            {
                throw new ArgumentException("This email address is invalid, please check for spellMistakes");
            }
        }

        /// <summary>
        /// check the structure of the password.
        /// </summary>
        /// <param name="pass">password to check.</param>
        /// <exception cref="ArgumentNullException">Thrown if the password is null</exception>
        /// <exception cref="ArgumentException">Throw if the password is invalid</exception>
        private void ValidatePasswordRules(string pass)
        {
            if (pass == null)
            { //check null input
                throw new ArgumentNullException("password is null");
            }

            //check length
            if (pass.Length < PASS_MIN_LENGTH)
                throw new ArgumentException("password too short");
            if (pass.Length > PASS_MAX_LENGTH)
                throw new ArgumentException("password too long");

            //check if forbidden password
            if (forbiddenPasswords.Contains(pass))
                throw new ArgumentException("password in forbidden passwords list");

            char[] numbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            if (pass.IndexOfAny(numbers) == -1)         // check contain a number
                throw new ArgumentException("the password doesn't contain a number");

            char[] lowerCase = { 'a','b','c','d','e','f','g','h','i','j','k','l','m',
                                'n','o','p','q','r','s','t','u','v','w','x','y','z'};
            if (pass.IndexOfAny(lowerCase) == -1)       // check contain a lower case letter
                throw new ArgumentException("the password doesn't contain a lowercase letter");

            char[] upperCase = { 'A','B','C','D','E','F','G','H','I','J','K','L','M',
                                'N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
            if (pass.IndexOfAny(upperCase) == -1)       // check contain an upper case letter
                throw new ArgumentException("the password doesn't contain a upper case letter");
        }
    }
}


