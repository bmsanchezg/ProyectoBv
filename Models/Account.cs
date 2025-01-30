using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilleteraVirtual.API.Models
{
    [Table("accounts")]  // 🔹 Especificamos el nombre exacto de la tabla en PostgreSQL
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]  // 🔹 Nombre exacto de la columna en PostgreSQL
        public int Id { get; set; }

        [Required]
        [Column("userid")]  // 🔹 Aseguramos que coincida con PostgreSQL
        public int UserId { get; set; }

        [Required]
        [Column("amount")]  // 🔹 Convertimos "Amount" a "amount"
        public decimal Amount { get; set; }

        [Required]
        [Column("status")]  // 🔹 Convertimos "Status" a "status"
        public int Status { get; set; }
    }
}
