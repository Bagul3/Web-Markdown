using N.EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataRepo
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public CordnersEntities _context = null;
        private DbSet<T> table = null;
        public GenericRepository()
        {
            this._context = new CordnersEntities();
            table = _context.Set<T>();
        }
        public GenericRepository(CordnersEntities _context)
        {
            this._context = _context;
            table = _context.Set<T>();
        }
        public IEnumerable<T> GetAll()
        {
            return table.ToList();
        }
        public T GetById(object id)
        {
            return table.Find(id);
        }
        public void Insert(T obj)
        {
            table.Add(obj);
        }
        public void Update(T obj)
        {
            table.Attach(obj);
            _context.Entry(obj).State = EntityState.Modified;
        }   
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
