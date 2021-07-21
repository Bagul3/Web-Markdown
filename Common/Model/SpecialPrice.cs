using System;
using System.Net.Http;

namespace Cordners.Model
{
    public class SpecialPrice
    {
        public int store_id { get; set; } = 0;
        public decimal price { get; set; }
        public string sku { get; set; }
        public string price_from { get; set; }
        public string price_to { get; set; }

    }
}
