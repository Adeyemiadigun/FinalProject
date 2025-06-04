using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class UnitOfWork(ClhAssessmentAppDpContext _context) : IUnitOfWork
    {
        public async Task SaveChangesAsync()
        {
           await _context.SaveChangesAsync();
        }
    }
}
