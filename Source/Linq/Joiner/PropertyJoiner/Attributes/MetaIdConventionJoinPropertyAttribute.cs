namespace Bars2Db.Linq.Joiner.PropertyJoiner.Attributes
{
    /// <summary>
    ///     Атрибут join по MetaId с использованием наименований по соглашению. Для отмеченного поля должно быть
    ///     соответствующее поле с таким же наименованием с суфиксом 'MetaId'. В сущности, на которую ссылается поле, должно
    ///     быть поле MetaId.
    /// </summary>
    public class MetaIdConventionJoinPropertyAttribute : BaseJoinPropertyAttribute
    {
        public override string JoinOnPropertyName
        {
            get
            {
                return "MetaId";
            }
        }
    }
}