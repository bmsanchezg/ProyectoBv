using System.ComponentModel.DataAnnotations;

namespace BilleteraVirtual.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Clave { get; set; } = string.Empty;
    }
}
