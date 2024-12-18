using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Api.Entities;

public class Item
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ItemId { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [Url]
    [StringLength(150)]
    public required string Image { get; set; }


    [StringLength(500)]
    public string Ingredients { get; set; } = string.Empty;


    [StringLength(100)]
    public string Allergens { get; set; } = string.Empty;

    public bool IsVegetarian { get; set; }

    public bool IsGlutenFree { get; set; }

    public bool IsDrink { get; set; }

    public bool IsDressing { get; set; }

    //navigation property
    public ICollection<Menu> Menus { get; set; } = new List<Menu>();

}
