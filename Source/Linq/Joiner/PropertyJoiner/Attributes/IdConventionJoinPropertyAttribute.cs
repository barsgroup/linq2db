namespace Bars2Db.Linq.Joiner.PropertyJoiner.Attributes
{
    /// <summary>
    ///     Атрибут для получения наименования свойства по которому делать join (JOIN ON JoinEntity.[JoinOn] ==
    ///     FromEntity.[JoinFrom])
    /// </summary>
    public class IdConventionJoinPropertyAttribute : BaseJoinPropertyAttribute
    {
        public override string JoinOnPropertyName
        {
            get
            {
                return "Id";
            }
        }
    }
}