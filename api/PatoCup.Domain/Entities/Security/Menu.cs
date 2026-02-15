using PatoCup.Domain.Common;

namespace PatoCup.Domain.Entities.Security
{
    public class Menu
    {
        public int UserId { get; set; }
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