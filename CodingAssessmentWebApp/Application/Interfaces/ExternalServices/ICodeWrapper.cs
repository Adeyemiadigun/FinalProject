
using Domain.Enum;

namespace Application.Interfaces.ExternalServices
{
    public interface ICodeWrapper
    {
        string Wrap(TechnologyStack language, string studentCode, string methodName, string input);
    }
}
