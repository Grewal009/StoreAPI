

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Store.Api.Entities;

public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }

    //foreign key
    public int CustomerId { get; set; }

    public DateTime OrderDateTime { get; set; }

    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(50)]
    public required string PaymentStatus { get; set; }

    [Required]
    [StringLength(50)]
    public required string DeliveryStatus { get; set; }

    //navigation property
    [JsonIgnore]
    public Customer? Customer { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();




}