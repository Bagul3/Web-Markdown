//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataRepo
{
    using System;
    using System.Collections.Generic;
    
    public partial class OnSale
    {
        public int Id { get; set; }
        public Nullable<int> StoreId { get; set; }
        public string Sku { get; set; }
        public string Price { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }
}
