using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entitties;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Infrastructure.Repositories
{
    public class AssessmentRepository : BaseRepository<Assessment>, IAssessmentRepository
    {
        public AssessmentRepository(ClhAssessmentAppDpContext context) : base(context)
        {
        }
        public async Task<Assessment?> GetAsync(Guid id)
        {
           return await _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .Include(x => x.Submissions)
                .FirstOrDefaultAsync(x => x.Id == id);
        } 
        public Task<Assessment?> GetAsync(string name)
        {
            return _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .Include(x => x.Submissions)
                .FirstOrDefaultAsync(x => x.Title == name);
        }
        public async Task<PaginationDto<Assessment>> GetAllAsync(PaginationRequest request)
        {
            var query = _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .Include(x => x.Submissions);
            var totalRecord = query.Count();
            var totalPages = totalRecord / request.PageSize;
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Assessment>
            {
                TotalPages = totalPages,
                TotalItems = totalRecord,
                Items = result,
                HasNextPage = totalPages / request.CurrentPage == 1 ? false : true,
                HasPreviousPage = request.CurrentPage - 1 == 0 ? false : true,
                PageSize = request.PageSize,
                  CurrentPage = request.CurrentPage

            };
        }
        public async Task<PaginationDto<Assessment>> GetAllAsync(Guid userId,PaginationRequest request)
        {
            var query = _context.Set<Assessment>()
                  .Include(x => x.AssessmentAssignments)
                  .Include(x => x.Submissions);
            var totalRecord = query.Count();
            var totalPages = totalRecord / request.PageSize;
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Assessment>
            {
                TotalPages = totalPages,
                TotalItems = totalRecord,
                Items = result,
                HasNextPage = totalPages / request.CurrentPage == 1 ? false : true,
                HasPreviousPage = request.CurrentPage - 1 == 0 ? false : true,
                PageSize = request.PageSize,
                CurrentPage = request.CurrentPage

            };
        }
        public async Task<ICollection<Assessment>> GetSelectedIds(ICollection<Guid> ids)
        {
            var res = _context.Set<Assessment>().Include(x => x.AssessmentAssignments).Include(x => x.Submissions)
                .Where(x => ids.Contains(x.Id));
            return await res.ToListAsync();
        }

        public async Task<PaginationDto<Assessment>> GetAllAsync(Expression<Func<Assessment, bool>> exp, PaginationRequest request)
        {
            var query = _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .Include(x => x.Submissions)
                .Where(exp);
            var totalRecord = query.Count();
            var totalPages = totalRecord / request.PageSize;
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Assessment>
            {
                TotalPages = totalPages,
                TotalItems = totalRecord,
                Items = result,
                HasNextPage = totalPages / request.CurrentPage == 1 ? false : true,
                HasPreviousPage = request.CurrentPage - 1 == 0 ? false : true,
                PageSize = request.PageSize,
                CurrentPage = request.CurrentPage

            };
        }

        public async Task<ICollection<Assessment>> GetAllAsync(Expression<Func<Assessment, bool>> exp)
        {
            var query = _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .Include(x => x.Submissions)
                .Where(exp);
         var res = await query.ToListAsync();
            return res;
        }

        public async Task<Assessment?> GetForSubmissionAsync(Guid id)
        {
            return await _context.Assessments
                .Include(x => x. Submissions)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
