using PatoCup.Domain.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Domain.Interfaces.Repositories.Security
{
    public interface IMenuRepository
    {
        Task<IEnumerable<Menu>> GetMenuByUserAsync(int userId);
    }
}
