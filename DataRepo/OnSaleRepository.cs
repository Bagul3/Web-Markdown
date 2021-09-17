using N.EntityFramework.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DataRepo
{
    public class OnSaleRepository : GenericRepository<OnSale>
    {
        public void BulkUpdate(List<OnSale> obj)
        {
            _context.BulkInsert(obj);
        }

        public OnSale GetBySku(string sku, int storeId)
        {
            return _context.OnSales.FirstOrDefault(x => x.Sku == sku && x.StoreId == storeId);
        }

        public void Delete(OnSale onsale)
        {
            _context.OnSales.Attach(onsale);
            _context.OnSales.Remove(onsale);
        }
    }
}
