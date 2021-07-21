using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;

namespace DataRepo
{
    public class SkuRepository
    {
        public DataSet RetrieveQuery(string query)
        {
            var dataset = new DataSet();
            using (var connectionHandler =
                new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"])
            )
            {
                connectionHandler.OpenAsync();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                //myAccessCommand.Parameters.AddWithValue("?", reff);
                var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                myDataAdapter.Fill(dataset);
            }
            return dataset;
            //return await Task.Run(() =>
            //{
            //    var dataset = new DataSet();
            //    using (var connectionHandler =
            //        new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"])
            //    )
            //    {
            //        connectionHandler.OpenAsync();
            //        var myAccessCommand = new OleDbCommand(query, connectionHandler);
            //        //myAccessCommand.Parameters.AddWithValue("?", reff);
            //        var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
            //        myDataAdapter.Fill(dataset);
            //    }
            //    return dataset;
            //});
        }

        public async Task<DataSet> RetrieveQueryAsync(string query)
        {
            return await Task.Run(() =>
            {
                var dataset = new DataSet();
                using (var connectionHandler =
                    new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"])
                )
                {
                    connectionHandler.OpenAsync();
                    var myAccessCommand = new OleDbCommand(query, connectionHandler);
                    //myAccessCommand.Parameters.AddWithValue("?", reff);
                    var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                    myDataAdapter.Fill(dataset);
                }
                return dataset;
            });
        }

        public IEnumerable<string> QueryDescriptionRefs()
        {
            var dvEmp = new DataView();
            //new LogWriter().LogWrite("Getting refs from description file");
            try
            {
                using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["ExcelConnectionString"]))
                {
                    connectionHandler.Open();
                    var adp = new OleDbDataAdapter("SELECT * FROM [Sheet1$B:B]", connectionHandler);

                    var dsXls = new DataSet();
                    adp.Fill(dsXls);
                    dvEmp = new DataView(dsXls.Tables[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //new LogWriter().LogWrite("Error occured getting refs from description file: " + e);
            }

            return (from DataRow row in dvEmp.Table.Rows select row.ItemArray[0].ToString()).ToList();
        }

        public void RetrieveQuery(List<string> reff, string query)
        {
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.Open();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                if (reff != null)
                {
                    myAccessCommand.Parameters.AddWithValue("?", reff);
                }

                for (var i = 0; i < reff.Count; i++)
                {
                    myAccessCommand.Parameters.AddWithValue("?", reff[i]);
                    myAccessCommand.ExecuteNonQuery();
                }
            }
        }

        public DataSet FetcheQuery(List<string> reff, string query)
        {
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.Open();
                var dataset = new DataSet();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                if (reff != null)
                {
                    myAccessCommand.Parameters.AddWithValue("?", reff);
                }

                for (var i = 0; i < reff.Count; i++)
                {
                    myAccessCommand.Parameters.AddWithValue("?", reff[i]);
                    var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                    myDataAdapter.Fill(dataset);
                }
                return dataset;
            }
        }

        public void PackTable()
        {
            using (OleDbConnection connection = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                using (OleDbCommand scriptCommand = connection.CreateCommand())
                {
                    connection.Open();

                    string vfpScript = @"SET EXCLUSIVE ON
                                DELETE FROM [CONFIG]
                                PACK";

                    scriptCommand.CommandType = CommandType.StoredProcedure;
                    scriptCommand.CommandText = "ExecScript";
                    scriptCommand.Parameters.Add("myScript", OleDbType.Char).Value = vfpScript;
                    scriptCommand.ExecuteNonQuery();
                }
            }
        }

        public void InsertREM(string name, string rem, string id, string rem_property, string query)
        {
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.Open();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                myAccessCommand.Parameters.AddWithValue("@name", name);
                myAccessCommand.Parameters.AddWithValue("@rem", rem);
                myAccessCommand.Parameters.AddWithValue("@id", id);
                myAccessCommand.Parameters.AddWithValue("@property", rem_property);
                myAccessCommand.ExecuteNonQuery();
            }
        }

        public void InsertSeasonData(string season, string id, string top, string bottom)
        {
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.Open();
                var myAccessCommand = new OleDbCommand(SqlQueries.InsertSeasonalData, connectionHandler);
                myAccessCommand.Parameters.AddWithValue("@season", season);
                myAccessCommand.Parameters.AddWithValue("@id", id);
                myAccessCommand.Parameters.AddWithValue("@toppage", top);
                myAccessCommand.Parameters.AddWithValue("@bottompage", bottom);
                myAccessCommand.ExecuteNonQuery();
            }
        }

        public void DeleteREM(string query)
        {
            using (var connectionHandler = new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"]))
            {
                connectionHandler.Open();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                myAccessCommand.ExecuteNonQuery();
            }
        }

        public DataSet RetrieveQuery(string reff, string query)
        {
            var dataset = new DataSet();
            using (var connectionHandler =
                new OleDbConnection(System.Configuration.ConfigurationManager.AppSettings["AccessConnectionString"])
            )
            {
                connectionHandler.OpenAsync();
                var myAccessCommand = new OleDbCommand(query, connectionHandler);
                myAccessCommand.Parameters.AddWithValue("?", reff);
                var myDataAdapter = new OleDbDataAdapter(myAccessCommand);
                myDataAdapter.Fill(dataset);
            }
            return dataset;
        }
    }
}
