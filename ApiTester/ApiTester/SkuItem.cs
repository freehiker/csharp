// <copyright>free to everyone.</copyright>

namespace ApiTester
{
    public class SkuItem
    {
        public Sku Item { get; set; }
    }

    public class Sku
    {
        public string description { get; set; }
        public string price { get; set; }
        public string sku { get; set; }
    }
}
