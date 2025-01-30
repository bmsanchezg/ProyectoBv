using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilleteraVirtual.API.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountSend { get; set; }

        [Required]
        public int AccountRecived { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; }
    }
}
