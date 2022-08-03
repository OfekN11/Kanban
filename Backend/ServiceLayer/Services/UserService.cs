using System;
using UC = IntroSE.Kanban.Backend.BusinessLayer.UserController;
using BUser = IntroSE.Kanban.Backend.BusinessLayer.IUser;
using LoginInstance = IntroSE.Kanban.Backend.BusinessLayer.LoginInstance;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    internal class UserService
    {
        //fields
        private UC uc;

        //constructor
        internal UserService(LoginInstance loginInstance)
        {
            uc = new UC(loginInstance);
        }

        //functions

        ///<summary>This method loads the users data from the persistance </summary>
        internal Response LoadData()
        {
            try
            {
                uc.LoadData();
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }

        ///<summary>Removes all persistent users data.</summary>
        internal Response DeleteData()
        {
            try
            {
                uc.DeleteData();
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }

        ///<summary>Registers a new user to the system.</summary>
        ///<param name="email">the user e-mail address, used as the username for logging the system.</param>
        ///<param name="password">the user password.</param>
        ///<returns>Response containing the newly created user or an error message in case of error</returns>
        internal Response<User> Register(string userEmail, string password)
        {
            try
            {
                BUser bUser = uc.Register(userEmail, password);
                User user = new User(bUser);
                return Response<User>.FromValue(user);
            }
            catch (Exception e)
            {
                return Response<User>.FromError(e.Message);
            }
        }
        
        /// <summary>
        /// Log in an existing user
        /// </summary>
        /// <param name="userEmail">The email address of the user to login</param>
        /// <param name="password">The password of the user to login</param>
        /// <returns>A response object with a value set to the user, instead the response should contain a error message in case of an error</returns>
        internal Response<User> Login(string userEmail, string password)
        {
            try
            {
                return Response<User>.FromValue(new User(uc.Login(userEmail, password)));
            }
            catch (Exception e)
            {
                return Response<User>.FromError(e.Message);
            }
        }

        /// <summary>        
        /// Log out an logged in user. 
        /// </summary>
        /// <param name="userEmail">The userEmail of the user to log out</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        internal Response Logout(string userEmail)
        {
            try
            {
                uc.Logout(userEmail);
                return new Response();
            }
            catch (Exception e)
            {
                return new Response(e.Message);
            }
        }
    }
}
