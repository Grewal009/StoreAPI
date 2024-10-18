

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Store.Api.Entities;

public class OrderDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderDetailId { get; set; }

    //foreign key
    public int OrderId { get; set; }

    //foreign key
    public int CustomerId { get; set; }

    //foreign key
    public int ItemId { get; set; }

    [Required]
    [StringLength(25)]
    public required string Size { get; set; }

    public int Quantity { get; set; }

    public decimal PricePerPiece { get; set; }

    //navigation property
    [JsonIgnore]
    public Customer? Customer { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }



}