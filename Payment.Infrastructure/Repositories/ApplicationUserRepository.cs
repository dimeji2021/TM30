using Microsoft.EntityFrameworkCore;
using Payment.Core.Interfaces;
using Payment.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Infrastructure.Repositories
{
    public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly PaymentDbContext context;
        public ApplicationUserRepository(PaymentDbContext dbContext) : base(dbContext)
        {
            context = dbContext;
        }
        public async Task<ApplicationUser> GetUser(string email)
        {

            return await context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        }
        public async Task<ApplicationUser> GetUser(Expression<Func<ApplicationUser, bool>> expression)
        {
            return await context.Users.FirstOrDefaultAsync(expression);
        }
        public IQueryable<ApplicationUser> GetUser()
        {
            return context.Users;
        }
    }
}
