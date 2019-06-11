namespace ProxyWebApi
{
    public class Product
    {
        public int PartNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string Vendor { get; set; }
        public int VendorPartNumber { get; set; }
        public string VendorDescription { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
    }
    
    public class JsonObject
    {
        public int Count { get; set; }        
        public Product[] Products { get; set; }
        public string Next { get; set; }
    }
}