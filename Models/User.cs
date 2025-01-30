using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilleteraVirtual.API.Models
{
    [Table("users")] // 🔹 Asegura que la tabla se llame "users" en PostgreSQL
    public class User
    {
        [Key]
        [Column("id")] // 🔹 Forzar nombre de columna en minúsculas
        public int Id { get; set; }

        [Required]
        [Column("cedula")] // 🔹 Forzar nombre de columna en minúsculas
        public string Cedula { get; set; } = string.Empty;

        [Required]
        [Column("firstname")] // 🔹 Forzar nombre de columna en minúsculas
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Column("email")] // 🔹 Forzar nombre de columna en minúsculas
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("clave")] // 🔹 Forzar nombre de columna en minúsculas
        public string Clave { get; set; } = string.Empty;
    }
}
