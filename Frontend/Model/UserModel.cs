using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SUser = IntroSE.Kanban.Backend.ServiceLayer.User;

namespace IntroSE.Kanban.Frontend.Model
{
    public class UserModel : NotifiableObject
    {
        public BackendController Controller { get; }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                RaisePropertyChanged("UserEmail");
            }
        }

        public UserModel(BackendController controller, SUser sUser)
        {
            this.Controller = controller;
            this._email = sUser.Email;
        }

        public ObservableCollection<BoardModel> GetBoards()
        {
            return Controller.GetBoards(this);
        }
    }
}
