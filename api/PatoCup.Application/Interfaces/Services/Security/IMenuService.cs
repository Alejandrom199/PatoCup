using PatoCup.Application.DTOs.Security;
using PatoCup.Domain.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Application.Interfaces.Services.Security
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuResponseDto>> GetMenuByUser(int userId);
    }
}
