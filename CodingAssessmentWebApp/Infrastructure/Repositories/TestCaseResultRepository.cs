using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class TestCaseResultRepository : BaseRepository<TestCaseResult>, ITestCaseResultRepository
    {
        public TestCaseResultRepository(ClhAssessmentAppDpContext context) : base(context)
        {
        }
    }
}
