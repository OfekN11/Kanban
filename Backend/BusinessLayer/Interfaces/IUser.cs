namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal interface IUser
    {
        //members 
        string Email { get; }

        //methods
        void Persist();
        bool ValidatePassword(string password);
    }
}
