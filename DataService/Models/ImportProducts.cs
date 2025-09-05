using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using DataService;

namespace ImportProducts.Model
{
    class ImportProducts
    {
        private string attribut_set;
        private string category;
        private string color;
        private string condition;
        private string configurableAttribute;
        private string description;
        private string ean;
        private string gallery;
        private string gty;
        private string hasOption;
        private string image;
        private string infocare;
        private string manufactor;
        private string model;
        private string name;
        private string optionsContainer;
        private string pageLayout;
        private string price;
        private string productName;        
        private string rrp;
        private string rem1;
        private string rem2;
        private string season;
        private string shortDescription;
        private string sizeRange;
        private string sizeguide;
        private string sku;
        private string smallImage;
        private string status;
        private string stockType;
        private string store;
        private string subCategory;
        private string taxClass;
        private string thumbnail;
        private string type;
        private string url_key;
        private string url_path;
        private string visibility;
        private string websites;
        private string weight;
        private string suSKU;
        private string parentSku;
        private string stype;
        private string udef2;
        private string euro_special_price;
        private string usd_special_price;
        private string aud_special_price;
        private string new_from_date;
        private string new_to_date;
        private string creation_date;
        

        public ImportProducts()
        {
        }

        public ImportProducts SetParentSku(string parentSku)
        {
            this.parentSku = "\"" + parentSku + "\"";
            return this;
        }

        public ImportProducts SetName(string name)
        {
            this.name = "\"" + name + "\"";
            return this;
        }

        public ImportProducts SetStore(string store)
        {
            this.store = "\"\"";
            return this;
        }

        public ImportProducts SetWebsites()
        {
            this.websites = "\"" + "Main Website,Ireland Website" + "\"";
            return this;
        }

        public ImportProducts Setattribut_set(string attribut_set)
        {
            this.attribut_set = "\"" + attribut_set + "\"";
            return this;
        }

        public ImportProducts Settype(string type)
        {
            this.type = "\"" + type + "\"";
            return this;
        }

        public ImportProducts Setsku(string sku)
        {
            this.sku = "\"" + sku + "\"";
            return this;
        }

        public ImportProducts SethasOption(string hasOption)
        {
            this.hasOption = "\"" + hasOption + "\"";
            return this;
        }

        public ImportProducts SetoptionsContainer(string optionsContainer)
        {
            this.optionsContainer = "\"" + optionsContainer + "\"";
            return this;
        }

        public ImportProducts SetpageLayout(string pageLayout)
        {
            this.pageLayout = "\"" + pageLayout + "\"";
            return this;
        }

        public ImportProducts Setprice(string price)
        {
            this.price = "\"" + price + "\"";
            return this;
        }

        public ImportProducts Setweight(string weight)
        {
            this.weight = "\"0.01\"";
            return this;
        }

        public ImportProducts Setstatus(string status)
        {
            this.status = "\"Enabled\"";
            return this;
        }

        public ImportProducts Setvisibility(string visibility)
        {
            if (visibility != null)
            {
                this.visibility = "\"" +  visibility + "\"";
            }
            else
            {
                this.visibility = "\"" + CordnersImportProductsBuilder.Visibility() + "\"";
            }
            return this;
        }

        public ImportProducts SetshortDescription(string shortDescription)
        {
            if (shortDescription == null)
            {
                this.shortDescription = "\"\"";
            }
            else
            {
                this.shortDescription = "\"" + shortDescription + "\"";
            }
            return this;
        }

        public ImportProducts Setgty(string gty)
        {
            this.gty = "\"" + gty + "\"";
            return this;
        }

        public ImportProducts SetproductName(string productName)
        {
            this.productName = "\"" + productName + "\"";
            return this;
        }

        public ImportProducts Setcolor(string color)
        {
            this.color = "\"" + color + "\"";
            return this;
        }

        public ImportProducts SetsizeRange(string sizeRange)
        {
            if (sizeRange == null)
            {
                this.sizeRange = "\"\"";
            }
            else
            {
                this.sizeRange = "\"" + sizeRange + "\"";
            }
            
            return this;
        }

        public ImportProducts SettaxClass(string code)
        {
            string taxClass = "";
            if (code.Contains("2"))
                taxClass = "Taxable Goods";
            else if (code.Contains("0"))
                taxClass = "Non VAT";
            this.taxClass = "\"" + taxClass + "\"";
            return this;
        }

        public ImportProducts SetconfigurableAttribute(string configurableAttribute)
        {
            this.configurableAttribute = "\"" + configurableAttribute + "\"";
            return this;
        }

        public ImportProducts SetManufactoring(string manufactor)
        {
            this.manufactor = "\"" + manufactor + "\"";
            return this;
        }

        public ImportProducts Setcategory(DataRow category)
        {
            if (category == null)
            {
                this.category = "\"\"";
            }
            else
            {
                this.category = "\"" + category.Category() + "\"";
            }            
            return this;
        }

        public ImportProducts SetsubCategory(string subCategory)
        {
            if (subCategory == null)
            {
                this.subCategory = "\"\"";
            }
            else
            {
                this.subCategory = "\"" + subCategory.SubCategory() + "\"";
            }
            
            return this;
        }

        public ImportProducts Setseason(string season)
        {
            this.season = "\"" + season + "\"";
            return this;
        }

        public ImportProducts SetstockType(string stockType)
        {
            this.stockType = "\"" + stockType + "\"";
            return this;
        }

        public ImportProducts Setimage(string image)
        {
            this.image = "\"" + image + "\"";
            return this;
        }

        public ImportProducts SetsmallImage(string smallImage)
        {
            this.smallImage = "\"" + smallImage + "\"";
            return this;
        }

        public ImportProducts Setthumbnail(string thumbnail)
        {
            this.thumbnail = "\"" + thumbnail + "\"";
            return this;
        }

        public ImportProducts Setgallery(IEnumerable<string> t2TreFs, string gallery)
        {
            if (t2TreFs == null)
            {
                this.gallery = "\"\"";
            }
            else
            {
                this.gallery = "\"" + CordnersImportProductsBuilder.BuildGalleryImages(t2TreFs, gallery) + "\"";
            }            
            return this;
        }

        public ImportProducts Setcondition(string condition)
        {
            this.condition = "\"new\"";
            return this;
        }

        public ImportProducts Setmodel(string model)
        {
            this.model = "\"" + model + "\"";
            return this;
        }

        public ImportProducts Setinfocare(string infocare)
        {
            this.infocare = "\"" + "row-product-featured-shoe-care" + "\"";
            return this;
        }

        public ImportProducts Setsizeguide(string sizeguide)
        {
            this.sizeguide = "\"" + "product_tab_size_guide" + "\"";
            return this;
        }

        public ImportProducts Setrrp(string rrp)
        {
            this.rrp = "\"" + rrp + "\"";
            return this;
        }

        public ImportProducts Seturl_key(string url_key)
        {
            this.url_key = "\"" + url_key + "\"";
            return this;
        }

        public ImportProducts Seturl_path(string url_path)
        {
            this.url_path = "\"" + url_path + "\"";
            return this;
        }

        public ImportProducts Setrem1(string rem1)
        {
            this.rem1 = "\"" + rem1 + "\"";
            return this;
        }

        public ImportProducts Setrem2(string rem2)
        {
            this.rem2 = "\"" + rem2 + "\"";
            return this;
        }

        public ImportProducts SetsuSKU(string suSKU)
        {
            this.suSKU = "\"" + suSKU + "\"";
            return this;
        }

        public ImportProducts SetDescription(string desc)
        {
            this.description = "\"" + desc + "\"";
            return this;
        }

        public ImportProducts Setean(string ean)
        {
            if(ean == null)
            {
                this.ean = "\"\"";
            }
            else
            {
                this.ean = "\"" + RemoveLineEndings(ean.Trim().Replace(",", "")) + "\"";
            }            
            return this;
        }

        public ImportProducts SetUDef(string udef2)
        {
            if (udef2 == "")
            {
                this.udef2 = "\" \"";
            }
            else
            {
                this.udef2 = "\"" + udef2 + "\"";
            }
            
            return this;
        }

        public ImportProducts SetSType(string stype)
        {
            if (stype == "")
            {
                this.stype = "\" \"";
            }
            else
            {
                this.stype = "\"" + stype + "\"";
            }
            return this;
        }

        public ImportProducts Seteuro_special_price(string euro_special_price)
        {
            if (euro_special_price == "")
            {
                this.euro_special_price = "\" \"";
            }
            else
            {
                this.euro_special_price = "\"" + euro_special_price + "\"";
            }
            return this;
        }

        public ImportProducts Setusd_special_price(string usd_special_price)
        {
            if (usd_special_price == "")
            {
                this.usd_special_price = "\" \"";
            }
            else
            {
                this.usd_special_price = "\"" + usd_special_price + "\"";
            }
            return this;
        }

        public ImportProducts Setaud_special_price(string aud_special_price)
        {
            if (aud_special_price == "")
            {
                this.aud_special_price = "\" \"";
            }
            else
            {
                this.aud_special_price = "\"" + aud_special_price + "\"";
            }
            return this;
        }

        public ImportProducts Setnew_from_date(bool isEmpty)
        {
            if (isEmpty)
            {
                this.new_from_date = "\" \"";
            }
            else
            {
                this.new_from_date = "\"" + DateTime.Now.ToString("yyyy/MM/dd") + "\"";
            }            
            return this;
        }

        public ImportProducts Setnew_to_date(bool isEmpty)
        {
            if (isEmpty)
            {
                this.new_to_date = "\" \"";
            }
            else
            {
                this.new_to_date = "\"" + DateTime.Now.AddDays(30).ToString("yyyy/MM/dd") + "\"";
            }
            
            return this;
        }

        public ImportProducts Setcreation_date(bool isEmpty)
        {
            if (isEmpty)
            {
                this.creation_date =  "\" \"";
            }
            else
            {
                this.creation_date = "\"" + DateTime.Now.ToString("yyyy/MM/dd") + "\"";
            }            
            return this;
        }

        public override string ToString()
        {
            return $"{sku},{store}," +
                          $"{websites},{attribut_set},{type},{hasOption},{name.TrimEnd()},{pageLayout},{optionsContainer},{price},{weight},{status},{visibility}," +
                          $"{shortDescription},{gty},{productName},{color}," +
                          $"{sizeRange},{taxClass},{configurableAttribute},{manufactor}," +
                          $"{category},{subCategory},{season},{stockType},{image},{smallImage},{thumbnail},{gallery},{condition},{ean}," +
                          $"{description},{model},{infocare},{sizeguide},{rrp},{url_key},{url_path},{rem1},{rem2},{suSKU},{parentSku},{udef2},{stype},{euro_special_price},{usd_special_price},{aud_special_price},{new_from_date},{new_to_date},{creation_date}";
        }

        public string RemoveLineEndings(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return "";
            }

            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace(lineSeparator, string.Empty)
                .Replace(paragraphSeparator, string.Empty);
        }
    }
}
