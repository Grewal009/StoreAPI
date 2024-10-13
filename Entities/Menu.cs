using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Store.Api.Entities;

public class Menu
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MenuId { get; set; }

    //foreign key
    public int ItemId { get; set; }

    [Required]
    [StringLength(50)]
    public required string Size { get; set; }

    public decimal Price { get; set; }

    //navigation property
    [JsonIgnore]
    public Item? Item { get; set; }




}
