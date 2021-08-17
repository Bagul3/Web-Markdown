using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Sales
    {
        public string store { get; set; }
        public decimal price { get; set; }
        public string sku { get; set; }
        public string price_from { get; set; }
        public string price_to { get; set; }

        public string MasterSupplier { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string Season { get; set; }

        public string Style { get; set; }

        public string StockType { get; set; }

        public string Color { get; set; }
    }
}
