using BUser = IntroSE.Kanban.Backend.BusinessLayer.IUser;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public struct User
    {
        public readonly string Email;

        internal User(string email)
        {
            this.Email = email;
        }

        internal User(BUser bUser)
        {
            Email = bUser.Email;
        }
    }
}
