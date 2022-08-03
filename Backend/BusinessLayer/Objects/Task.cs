using IntroSE.Kanban.Backend.DataLayer;
using System;
using System.Runtime.CompilerServices;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class Task : ITask
    {
        private const int MIN_TITLE_LENGTH =  1;
        private const int MAX_TITLE_LENGTH = 50;
        private const int MIN_DESCRIPTION_LENGTH = 0;
        private const int MAX_DESCRIPTION_LENGTH = 300;
        
        //fields
        private readonly int _id;
        private string _title;
        private string _description;
        private readonly DateTime _creationTime;
        private DateTime _dueDate;
        private string _assignee;

        private DTask dTask; //parallel DTO

        int ITask.ID { get => _id;}
        DateTime ITask.CreationTime { get => _creationTime;}
        DateTime ITask.DueDate { get => _dueDate;  
            set 
            {
                validateDueDate(value);
                dTask.DueDate = value;
                _dueDate = value; 
            } 
        }
        string ITask.Title { get => _title; 
            set
            {
                validateTitle(value);
                dTask.Title = value;
                _title = value;

            }
        }
        string ITask.Description { get => _description; 
            set
            {
                validateDescription(value);
                dTask.Description = value;
                _description = value;
            }
        }
        string ITask.Assignee { get => _assignee; 
            set 
            {
                dTask.Assignee = value;
                _assignee = value;
            } 
        }

        //constructors
        /// <summary>
        /// Creates a new Task
        /// </summary>
        /// <param name="id">ID of Task</param>
        /// <param name="creationTime">Creation Time of task</param>
        /// <param name="title">Title of task - has to stand conditions</param>
        /// <param name="description">Description of task - has to stand conditions</param>
        /// <param name="dueDate">Due date of task - has to stand conditions</param>
        /// <param name="assignee">Assignee of task</param>
        /// <param name="boardCreator">Creator of the board the task is in - delivered to the created DTO</param>
        /// <param name="boardName">Board name of the board the task is in - delivered to the created DTO</param>
        /// <remarks>calls ValidateDueDate, ValidateTitle, ValidateDescription</remarks>
        internal Task(int id, DateTime creationTime,  string title, string description, DateTime dueDate, string assignee, string boardCreator, string boardName)
        {
            _id = id;
            _creationTime = creationTime;
            validateDueDate(dueDate);
            _dueDate = dueDate;
            validateTitle(title);
            _title= title;
            validateDescription(description);
            _description = description;
            _assignee = assignee;
            dTask = new DTask(id, creationTime, title, description, dueDate, assignee, 0, boardCreator, boardName);
        }

        /// <summary>
        /// Recreates Task from DTO
        /// </summary>
        /// <param name="dTask">DTO representing the task</param>
        internal Task(DTask dTask)
        {
            _id = dTask.TaskId;
            _creationTime = dTask.CreationTime;
            _dueDate = dTask.DueDate;
            _title = dTask.Title;
            _description = dTask.Description;
            _assignee = dTask.Assignee;
            this.dTask = dTask;
            this.dTask.Persist = true;
        }

        //methods

        void ITask.persist()
        {
            dTask.Insert();
            dTask.Persist = true;
        }

        /// <summary>
        /// Sends a message to dTask to advance column-wise
        /// </summary>
        void ITask.Advance()
        {
            dTask.Ordinal = dTask.Ordinal + 1;
        }

        ///<summary>Validates the property of a given _description.</summary>
        ///<param name="description">The _description given to the Task</param>
        ///<exception cref="ArgumentNullException">Thrown if given _description is null</exception>
        ///<exception cref="FormatException">Thrown if the _description doesn't fit limits</exception>
        private void validateDescription(string description)
        {
            if (description == null)
            {
                throw new ArgumentNullException("Description must not be null");
            }
            if(description.Length > MAX_DESCRIPTION_LENGTH)
            {
                throw new FormatException("Description max length is" + MAX_DESCRIPTION_LENGTH + "characters");
            }

            if (description.Length < MIN_DESCRIPTION_LENGTH)
            {
                throw new FormatException("Description minimum length is" + MIN_DESCRIPTION_LENGTH + "characters");
            }
        }

        ///<summary>Validates the property of a given _title.</summary>
        ///<param name="title">The _title given to the Task</param>
        ///<exception cref="ArgumentNullException">Thrown if given _title is null</exception>
        ///<exception cref="FormatException">Thrown if the _title doesn't fit limits</exception>
        private void validateTitle(string title)
        {
            if(title == null)
            {
                throw new ArgumentNullException("Title must not be null");
            }
            if (title.Length < MIN_TITLE_LENGTH) 
            {
                throw new FormatException("Title must contain at list " + MIN_TITLE_LENGTH + " character");
            }
            if (title.Length > MAX_TITLE_LENGTH)
            {
                throw new FormatException("Title length cannot be more then " + MAX_TITLE_LENGTH + " characters");
            }
        }

        ///<summary>Validate the propriety of a given _dueDate.</summary>
        /// <param name="dueDate">The _dueDate given to the Task</param>
        /// <exception cref="ArgumentNullException">Thrown when _dueDate is null object </exception> 
        /// <exception cref="FormatException"> Thrown when the _dueDate is earlier then now. </exception>
        private void validateDueDate(DateTime dueDate)
        {
            if (dueDate == null)
            {
                throw new ArgumentNullException("Due date must not be null");
            }
            if (dueDate < DateTime.Now)
            {
                throw new ArgumentException("This _due date time already past");
            }
        }
    }
}
