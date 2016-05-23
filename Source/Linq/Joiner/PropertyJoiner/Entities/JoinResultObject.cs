namespace Bars.Minfin.Kaliningrad.Services.PropertiesJoiner.Entities
{
    /// <summary>Класс для представления результатов джойна</summary>
    /// <typeparam name="TLeft">Тип сущности левой части джойна</typeparam>
    /// <typeparam name="TRight">Тип сущности правой части джойна</typeparam>
    internal class JoinResultObject<TLeft, TRight>
    {
        /// <summary>Результат предыдущего запроса</summary>
        public TLeft Left { get; set; }

        /// <summary>Ссылка на сущность</summary>
        public TRight Right { get; set; }
    }
}