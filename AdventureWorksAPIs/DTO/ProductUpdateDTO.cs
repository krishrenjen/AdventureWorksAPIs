using System.ComponentModel.DataAnnotations;

namespace AdventureWorksAPIs.DTO
{
    public class ProductUpdateDTO
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [Range(0.0, double.MaxValue)]
        public decimal ListPrice { get; set; }

        [Required]
        [Range(0.0, double.MaxValue)]
        public decimal StandardCost { get; set; }

        [StringLength(5)]
        public string? Size { get; set; }

        [StringLength(3)]
        public string? SizeUnitMeasureCode { get; set; }

        [Range(0, 999999.99)]
        [RegularExpression(@"^\d{1,6}(\.\d{1,2})?$", ErrorMessage = "Max 6 digits before and 2 after the decimal point.")]
        public decimal? Weight { get; set; }

        [StringLength(3)]
        public string? WeightUnitMeasureCode { get; set; }

        [StringLength(400)]
        public string? Description { get; set; }
    }
}
