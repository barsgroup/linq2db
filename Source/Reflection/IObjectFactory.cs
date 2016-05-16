namespace Bars2Db.Reflection
{
    public interface IObjectFactory
    {
        object CreateInstance(TypeAccessor typeAccessor);
    }
}