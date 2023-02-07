using Payment.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Core.Interfaces
{
    public interface IApplicationUserRepository : IGenericRepository<ApplicationUser>
    {
        Task<ApplicationUser> GetUser(string email);
        Task<ApplicationUser> GetUser(Expression<Func<ApplicationUser, bool>> expression);
        IQueryable<ApplicationUser> GetUser();
    }
}
