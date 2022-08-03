using IntroSE.Kanban.Frontend.Commands;
using IntroSE.Kanban.Frontend.Model;
using System.Collections.Generic;

namespace IntroSE.Kanban.Frontend.ViewModel
{
    public class ColumnViewModel : NotifiableObject
    {

        private ColumnModel _column;
        private List<TaskModel> _tasks;
        private TaskModel _selectedTask;

        private List<TaskModel> _filteredTasks;
        private bool isFiltered = false;
        private string _message;
        private bool _enableForward;

        public ColumnModel Column { get => _column; set => _column = value; }

        public List<TaskModel> Tasks
        {
            get 
            {
                if (isFiltered)
                    return FilteredTasks;
                else
                    return _tasks;
            }
            set
            {
                _tasks = value;
                RaisePropertyChanged("Tasks");
            }
        }

        public TaskModel SelectedTask
        {
            get
            {
                return _selectedTask;
            }
            set
            {
                _selectedTask = value;
                EnableForward = value != null;
            }
        }

        public List<TaskModel> FilteredTasks 
        { 
            get => _filteredTasks; 
            set => _filteredTasks = value; 
        }

        public string Filter 
        {
            set
            {
                if (value == "")
                {
                    isFiltered = false;
                }
                else
                {
                    FilteredTasks = new List<TaskModel>();
                    _tasks.ForEach (x => { if(x.Title.Contains(value) | x.Description.Contains(value)) { FilteredTasks.Add(x); } });
                    isFiltered = true;
                }
                RaisePropertyChanged("Tasks");
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        public bool EnableForward
        {
            get => _enableForward;
            private set
            {
                _enableForward = value;
                RaisePropertyChanged("EnableForward");
            }
        }

        public AdvanceTaskCommand AdvanceTaskCommand { get; } = new AdvanceTaskCommand();

        // constructor

        public ColumnViewModel(ColumnModel column)
        {
            this._column = column;
            RefreshTasks();
        }

        //  methods

        public TaskModel GetSelectedTask()
        {
            return SelectedTask;
        }

        public void RefreshTasks()
        {
            _tasks = new List<TaskModel>();
            foreach (var task in Column.GetTasks())
                _tasks.Add(task);
            RaisePropertyChanged("Tasks");
        }
    }


}
