using System.Collections.Generic;

namespace WooProductManager
{
    public class Product
    {
        public int id { get; set; }
        public string name { get; set; }
        public string regular_price { get; set; }
        public List<ProductImage> images { get; set; }
    }

    public class ProductImage
    {
        public string src { get; set; }
    }
}