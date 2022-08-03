using System;
using IntroSE.Kanban.Frontend.Model;

namespace IntroSE.Kanban.Frontend.ViewModel
{
    class LoginViewModel : NotifiableObject
    {

        private BackendController controller;

        private string _username;
        private string _password;
        private string _message;

        public string Username 
        {
            get => _username;
            set => _username = value;
        }

        public string Password
        {
            get => _password;
            set => _password = value;
        }

        public string Message
        {
            get => _message;
            set
            {
                this._message = value;
                RaisePropertyChanged("Message");
            }
        }
        public UserModel Login()
        {
            Message = "";
            try
            {
                return controller.Login(Username, Password); 
            }
            catch (Exception e)
            {
                Message = e.Message;
                return null;
            }
        }

        public void Register()
        {
            Message = "";
            try
            {
                controller.Register(Username, Password);
                Message = $"{Username} had successfully register ";
            }
            catch(Exception e)
            {
                Message = e.Message;
            }
        }

        public LoginViewModel(BackendController controller)
        {
            this.controller = controller;
        }
    }
}
