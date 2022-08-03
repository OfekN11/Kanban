using BBoard = IntroSE.Kanban.Backend.BusinessLayer.IBoard;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public struct Board
    {
        public readonly string CreatorEmail;
        public readonly string Name;
        public readonly int ColumnCount;
        public readonly int TaskCount;

        internal Board(string creatorEmail, string boardName, int columnCount, int taskCount)
        {
            CreatorEmail = creatorEmail;
            Name = boardName;
            ColumnCount = columnCount;
            TaskCount = taskCount;
        }

        internal Board(BBoard bBoard)
        {
            CreatorEmail = bBoard.Creator;
            Name = bBoard.Name;
            ColumnCount = bBoard.ColumnCount;
            TaskCount = bBoard.TaskCount;
        }
    }
}
