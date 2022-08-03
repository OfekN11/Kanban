using System;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// Instance holding currently logged in user. Will provide all logic surrounding validation + logging out
    /// </summary>
    internal class LoginInstance
    {
        //fields
        private string connectedEmail;
        internal string ConnectedEmail { get => connectedEmail; }

        internal LoginInstance()
        {
            connectedEmail = null;
        }

        /// <summary>
        /// Sets the instance
        /// </summary>
        /// <param name="userEmail">calling user's userEmail</param>
        /// <exception cref="InvalidOperationException">Thrown if another user is currently logged in</exception>
        internal void Login(string userEmail)
        {
            if (connectedEmail != null)
                throw new InvalidOperationException($"User '{connectedEmail}' is currently logged in. Log out before attempting to log in.");
            connectedEmail = userEmail;
        }

        /// <summary>
        /// Validates the user operates legally - i.e. is logged in and operates on his domain
        /// </summary>
        /// <param name="userEmail">calling user's userEmail</param>
        /// <exception cref="NullReferenceException">Thrown if no user is logged in</exception>
        /// <exception cref="InvalidOperationException">Thrown if the user logged in is trying to access data outside his domain</exception>
        internal void ValidateLogin(string userEmail)
        {
            if (connectedEmail == null)
                throw new NullReferenceException("Can't operate -  Please log in first");
            if (!connectedEmail.Equals(userEmail))
                throw new InvalidOperationException($"Can't operate -  User '{userEmail}' is not logged in");
        }

        /// <summary>        
        /// Logs out the logged in user. 
        /// </summary>
        /// <param name="userEmail">The userEmail of the user to log out</param>
        /// <excetion cref="ArgumentException">Thrown if the given userEmail is not logged in</excetion>
        internal void Logout(string userEmail)
        {
            if (connectedEmail == null || !connectedEmail.Equals(userEmail))
            {
                throw new ArgumentException("Can't logout: user " + userEmail + " is not logged in");
            }
            connectedEmail = null;
        }
    }
}
