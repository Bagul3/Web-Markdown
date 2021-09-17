using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Sales
    {
        public string Season { get; set; }
        public string StockType { get; set; }
        public string MasterSupplier { get; set; }
        public string Name { get; set; }
        public string Style { get; set; }
        public string Color { get; set; }
        public string Store { get; set; }
        public string RRP { get; set; }
        public decimal Sales_Price { get; set; }
        public int Discount { get; set; }
        public string SKU { get; set; }
        public string Price_From { get; set; }
        public string Price_To { get; set; }                

        public string Category { get; set; }

        public override string ToString()
        {
            var str = $"{Season} {StockType} {MasterSupplier} {Name} {Style} {Color} {Store} {RRP}";
            return str;
        }
    }
}
