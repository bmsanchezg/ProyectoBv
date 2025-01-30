using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilleteraVirtual.API.Models
{
    [Table("transactions")]
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Desactivar generación automática
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("accountsend")]
        public int AccountSend { get; set; }

        [Required]
        [Column("accountrecived")]
        public int AccountRecived { get; set; }

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "PENDING"; // Estado por defecto
    }
}
