using IntroSE.Kanban.Frontend.Model;
using System.Collections.Generic;


namespace IntroSE.Kanban.Frontend.ViewModel
{
    class InProgressTasksViewModel : NotifiableObject
    {
        IList<TaskModel> _tasks;
        TaskModel _selectedTask;

        public string Title { get => "In progress Tasks"; }
        public string ID 
        { 
            get 
            {
                if(SelectedTask == null)
                {
                    return "";
                }
                else
                {
                    return SelectedTask.ID.ToString();
                }
            }
        }

        public string CreationTime
        {
            get
            {
                if (SelectedTask == null)
                {
                    return "";
                }
                else
                {
                    return SelectedTask.CreationTime.ToString();
                }
            }
        }

        public string Description
        {
            get
            {
                if (SelectedTask == null)
                {
                    return "";
                }
                else
                {
                    return SelectedTask.Description.ToString();
                }
            }
        }

        public string DueDate
        {
            get
            {
                if (SelectedTask == null)
                {
                    return "";
                }
                else
                {
                    return SelectedTask.DueDate.ToString();
                }
            }
        }

        public string EmailAssignee
        {
            get
            {
                if (SelectedTask == null)
                {
                    return "";
                }
                else
                {
                    return SelectedTask.EmailAssignee.ToString();
                }
            }
        }

        public TaskModel SelectedTask 
        {
            get => _selectedTask;
            set 
            {
                _selectedTask = value;
                RaisePropertyChanged("ID");
                RaisePropertyChanged("CreationTime");
                RaisePropertyChanged("Description");
                RaisePropertyChanged("DueDate");
                RaisePropertyChanged("EmailAssignee");
            } 
        }

        public IList<TaskModel> Tasks { get => _tasks; }
        public InProgressTasksViewModel(IList<TaskModel> tasks)
        {
            this._tasks = tasks;
        }
    }
}
