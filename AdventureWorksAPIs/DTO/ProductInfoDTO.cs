namespace AdventureWorksAPIs.DTO
{
    public class ProductInfoDTO
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? ProductNumber { get; set; }
        public decimal ListPrice { get; set; }
        public decimal StandardCost { get; set; }
        public string? Size { get; set; }
        public string? SizeUnitMeasureCode { get; set; }
        public decimal? Weight { get; set; }
        public string? WeightUnitMeasureCode { get; set; }
        public DateTime? SellStartDate { get; set; }
        public string? Category { get; set; }
        public string? Subcategory { get; set; }
        public string? Description { get; set; }
    }

}
