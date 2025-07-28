using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Entitties;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class AnswerSubmissionRepository : BaseRepository<AnswerSubmission>, IAnswerSubmissionRepository
    {
        public AnswerSubmissionRepository(ClhAssessmentAppDpContext context) : base(context)
        {
        }
    }
}
