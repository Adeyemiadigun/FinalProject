using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enum;

namespace Application.Interfaces.ExternalServices
{
    public interface IExtractMethodName
    {
        string ExtractMethodName(string code, TechnologyStack language);
    }
}
