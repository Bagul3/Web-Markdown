using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService
{
    public static class CordnersImportProductsBuilder
    {
        public static string Category(this DataRow dr)
        {
            var category = "Default Category/Brands/" + dr["MasterSupplier"] + ",";
            // Ladies bags
            if (dr["MasterStocktype"].ToString() == "UNISEX")
            {
                category = category + "Default Category/UNISEX" + "/Shop By Brand/" +
                       dr["MasterSupplier"];
                return category;
            }
            if (dr["STYPE"].ToString() == "BAG" && dr["MasterStocktype"].ToString() == "LADIES")
            {
                category = category + "Default Category/BAGS & ACCESSORIES" + "/Shop By Department" + "/" + "LADIES BAGS" + ",";
                category = category + "Default Category/BAGS & ACCESSORIES" + "/Shop By Brand" + "/" + dr["MasterSupplier"].ToString();
                return category;
            }

            if (dr["STYPE"].ToString() == "BAG")
            {
                category = category + "Default Category/BAGS & ACCESSORIES" + "/Shop By Department" + "/" + "BACKPACKS" + ",";
                category = category + "Default Category/BAGS & ACCESSORIES" + "/Shop By Brand" + "/" + dr["MasterSupplier"].ToString();
                return category;
            }

            if (dr["STYPE"].ToString() == "PEN")
            {
                category = category + "Default Category/BAGS & ACCESSORIES" + "/Shop By Department" + "/" + "PENCIL CASES" + ",";
                category = category + "Default Category/BAGS & ACCESSORIES" + "/Shop By Brand" + "/" + dr["MasterSupplier"].ToString();
                return category;
            }
            // FIX LATER           

            //Bags and Accessories and Shoecare
            if (dr["MasterStocktype"].ToString() == "SUNDRIES")
            {
                category = category + "Default Category/BAGS & ACCESSORIES" + "/Shop By Department" + "/" + "SHOE CARE" + ",";
                category = category + "Default Category/BAGS & ACCESSORIES" + "/Shop By Brand" + "/" + dr["MasterSupplier"].ToString();
                return category;
            }

            // Old category Formatting
            category += "Default Category/" + dr["MasterStocktype"] + "/Shop By Department/" + dr["MasterDept"] + ",";

            if (dr["MasterSubDept"] != "ANY" || dr["MasterSubDept"] != "")
            {
                category = category + "Default Category/" + dr["MasterStocktype"] + "/Shop By Department/" +
                           dr["MasterDept"] + "/" + dr["MasterSubDept"] + ",";
            }

            category = category + "Default Category/" + dr["MasterStocktype"] + "/Shop By Brand/" +
                       dr["MasterSupplier"] + ",";
            category = category + "Default Category/" + dr["MasterStocktype"] + "/Shop By Brand/" +
                       dr["MasterSupplier"] + "/" + dr["MasterDept"] + ",";

            if (dr["MasterSubDept"] != "ANY" || dr["MasterSubDept"] != "")
            {
                category = category + "," + "Default Category/" + dr["MasterStocktype"] + "/Shop By Brand/" +
                           dr["MasterSupplier"] + "/" + dr["MasterDept"] +
                           "/" + dr["MasterSubDept"] + ",";
            }

            if (dr["USER1"].ToString().Contains("B"))
            {
                category += ",Default Category/BACK TO SCHOOL/" + dr["MasterStocktype"];
            }

            if (category.Last()==',')
            {
                category = category.Remove(category.Length - 1, 1);
            }

            return category.Replace(",,",",");
        }

        public static string SubCategory(this string stype)
        {
            switch (stype)
            {
                case "ACC":
                    return "ACCESSORIES";
                case "C&C":
                    return "CLEAN & CARE";
                case "C&P":
                    return "CREAMS & POLISH";
                case "I&S":
                    return "INSOLES & SUPPORT";
                case "LAC":
                    return "LACES";
                default:
                    return "";
            }
        }

        public static string BuildGalleryImages(IEnumerable<string> t2TreFs, string reff)
        {
            var images = t2TreFs.Where(t2TRef => t2TRef.Contains(reff)).ToList();
            var result = images.Aggregate("", (current, image) => current + (image + ".jpg,"));
            return result.Remove(result.Length - 1, 1);
        }

        public static string BuildSimpleSku(IEnumerable<string> t2TreFs, IEnumerable<string> sizes)
        {
            // sku=021488010001,size=B4|
            var output = new StringBuilder();
            for(var i=0; i < t2TreFs.Count();i++)
            {
                output.Append($"sku={t2TreFs.ElementAt(i)},size={sizes.ElementAt(i)}|");              
            }
            return output.ToString().Remove(output.Length - 1, 1);
        }

        public static string Visibility()
        {
            return "Catalog, Search";
        }
    }
}
