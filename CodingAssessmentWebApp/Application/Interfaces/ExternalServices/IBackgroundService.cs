using System.Linq.Expressions;

namespace Application.Interfaces.ExternalServices
{
    public interface IBackgroundService
    {
        void Enqueue<T>(Expression<Action<T>> methodCall);
        void Schedule<T>(Expression<Action<T>> methodCall, DateTime delay);
    }
}
