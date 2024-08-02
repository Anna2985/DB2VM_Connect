using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;
using System.Configuration;
using IBM.Data.DB2.Core;
using System.Data;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DIMUD22Controller : ControllerBase
    {
        string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";
        string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
        string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
        string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
        string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";
        // GET: api/<ValuesController>
        [HttpGet]
        public string Get()
        {
            try
            {
                string schema = DB2_schema;
                String MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
                DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);

                MyDb2Connection.Open();

                String procName = $"{DB2_schema}.DIMUD2";
                DB2Command cmd = MyDb2Connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $"select * from {procName}";
                DB2DataReader reader = cmd.ExecuteReader();
                string result = "";
                
                while (reader.Read())
                {
                    //result += reader["DRUGSTNO"].ToString().Trim() + " ";
                    string str = reader["DRUGSTNO"].ToString().Trim();

                }
                return result.Trim();
            }
            catch(Exception ex)
            {
                return $"{ex.Message}";
            }

            
        }

        
        
    }
}
