using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common
{
    public class CsvMapper
    {
        public sealed class CsvProduct
        {
            public string Sku { get; set; }
            public string ProductWebsites { get; set; }
            public string Name { get; set; }
            public string TaxClassName { get; set; }
            public string UDef2 { get; set; }
            public string DType { get; set; }
            public string Gender { get; set; }
        }

        public List<CsvProduct> MapProductsFromCsv(string csvPath)
        {
            var products = new List<CsvProduct>();
            _logger.LogWrite($"Reading CSV: {csvPath}");

            if (!File.Exists(csvPath))
                throw new FileNotFoundException("CSV file not found", csvPath);

            try
            {
                var lines = File.ReadAllLines(csvPath);

                if (lines.Length == 0)
                    throw new InvalidDataException("CSV file is empty.");

                // Header row
                var header = lines[0].Split(',').Select(h => h.Trim().ToLowerInvariant()).ToArray();

                var required = new[] { "sku", "product_websites", "name", "tax_class_name", "udef2", "d_type", "gender" };
                var missing = required.Where(r => !header.Contains(r)).ToList();

                if (missing.Any())
                    throw new InvalidDataException("CSV missing required headers: " + string.Join(", ", missing));

                // Build header index
                var headerIndex = new Dictionary<string, int>();
                for (int i = 0; i < header.Length; i++)
                    headerIndex[header[i]] = i;

                for (int row = 1; row < lines.Length; row++)
                {
                    if (string.IsNullOrWhiteSpace(lines[row]))
                        continue;

                    var cols = lines[row].Split(',').Select(c => c.Trim()).ToArray();

                    string Get(string col)
                    {
                        return headerIndex.TryGetValue(col, out var idx) && idx < cols.Length
                            ? cols[idx]
                            : string.Empty;
                    }

                    var rawSku = Get("sku");
                    var sku = NormalizeSku(rawSku, row);

                    var product = new CsvProduct
                    {
                        Sku = sku,
                        ProductWebsites = "",
                        Name = Get("name"),
                        TaxClassName = Get("tax_class_name"),
                        UDef2 = Get("udef2"),
                        DType = Get("d_type"),
                        Gender = Get("gender")
                    };

                    products.Add(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWrite("Error reading CSV: " + ex);
                throw;
            }

            _logger.LogWrite($"Finished reading CSV. Rows mapped: {products.Count}");
            return products;
        }

        private string NormalizeSku(string rawSku, int rowNum)
        {
            var sku = rawSku ?? string.Empty;

            if (!int.TryParse(sku, out _))
            {
                _logger.LogWrite($"Row {rowNum}: invalid SKU '{sku}', replaced with 000000");
                return "000000";
            }

            while (sku.Length < 9)
                sku = "0" + sku;

            return sku;
        }

        private readonly LogWriter _logger = new LogWriter();
    }
}
