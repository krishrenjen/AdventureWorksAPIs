namespace AdventureWorksAPIs.DTO
{
    public class DiscoveryRow
    {
        public string CategoryName { get; set; } = null!;
        public string SubcategoryName { get; set; } = null!;
        public int ProductID { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
    }

    public class ProductItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
    }

    public class SubcategoryGroup
    {
        public string Subcategory { get; set; } = null!;
        public List<ProductItem> Products { get; set; } = new();
    }

}
