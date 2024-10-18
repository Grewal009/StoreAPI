
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Api.Entities;

public class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(50)]
    public required string CustomerName { get; set; }

    [Required]
    [StringLength(50)]
    public required string Email { get; set; }

    [Required]
    [StringLength(50)]
    public required string Password { get; set; }

    //navigation property
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

}