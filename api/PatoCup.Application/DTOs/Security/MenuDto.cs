using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Application.DTOs.Security
{
    public class MenuResponseDto
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string MenuIcon { get; set; } = string.Empty;
        public int MenuOrder { get; set; }

        public int OptionId { get; set; }
        public string OptionName { get; set; } = string.Empty;
        public string OptionRoute { get; set; } = string.Empty;
        public string OptionIcon { get; set; } = string.Empty;
    }
}
