using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entitties;

namespace Application.Interfaces.ExternalServices
{
    public interface ICodeExcution
    {
        Task<Judge0Result> ExecuteCodeAsync(Judge0CodeExecutionRequest answer);
    }
}
