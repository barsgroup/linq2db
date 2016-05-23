namespace Bars2Db.Linq.Joiner.Visitors.Entities
{
    /// <summary>Сущность хранящая соответствие между двумя путями</summary>
    public class FullPathBinding
    {
        /// <summary>Путь при текущем Query</summary>
        public FullPathInfo CurrentQueryPath { get; private set; }

        /// <summary>Новый путь на который будет происходить замена Query (соответственно более длинный путь)</summary>
        public FullPathInfo NewQueryPath { get; private set; }

        private FullPathBinding(FullPathInfo currentQueryPath, FullPathInfo newQueryPath)
        {
            CurrentQueryPath = currentQueryPath;
            NewQueryPath = newQueryPath;
        }

        public static FullPathBinding Create(FullPathInfo currentQueryPath, FullPathInfo newQueryPath)
        {
            return new FullPathBinding(currentQueryPath, newQueryPath);
        }

        public override string ToString()
        {
            return string.Format("{0} <= {1}", CurrentQueryPath, NewQueryPath);
        }
    }
}