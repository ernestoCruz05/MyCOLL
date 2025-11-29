using Microsoft.AspNetCore.Identity;

namespace MyCOLL.Data
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }

    }

}
