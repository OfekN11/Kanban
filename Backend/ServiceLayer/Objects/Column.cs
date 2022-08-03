using BColumn = IntroSE.Kanban.Backend.BusinessLayer.IColumn;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public struct Column
    {
        public readonly string Name;
        public readonly int Limit;
        public readonly int Ordinal;

        internal Column(string Name, int Limit, int Ordinal)
        {
            this.Name = Name;
            this.Limit = Limit;
            this.Ordinal = Ordinal;
        }

        internal Column(BColumn bColumn)
        {
            Name = bColumn.Name;
            Limit = bColumn.Limit;
            this.Ordinal = bColumn.Ordinal;
        }
    }
}
