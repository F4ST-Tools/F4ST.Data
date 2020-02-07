using F4ST.Common.Containers;

namespace F4ST.Data
{
    public interface IBaseDbConnection<out T> : ITransient//, IDisposable
    {
        T Connection { get; }
    }
}