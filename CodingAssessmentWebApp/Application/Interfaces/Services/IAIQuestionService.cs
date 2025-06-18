using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces.Services
{

    public interface IAIQuestionService
    {
        Task<BaseResponse<object>> GenerateQuestionAsync(AiQuestionGenerationRequestDto request);
    }

    
}
