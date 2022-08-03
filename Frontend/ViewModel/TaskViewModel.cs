using IntroSE.Kanban.Frontend.Model;

namespace IntroSE.Kanban.Frontend.ViewModel
{
    class TaskViewModel :  NotifiableObject
    {

        private TaskModel _task;

        public TaskModel Task { get => _task; set => _task = value; }

        public TaskViewModel(TaskModel task)
        {
            this._task = task;
        }

    }
}
