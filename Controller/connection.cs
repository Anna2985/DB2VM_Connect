using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using IBM.Data.DB2.Core;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class connection : ControllerBase
    {
        string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";
        string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
        string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
        string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
        string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";
        private String conString = "jdbc:db2://10.30.253.249:51031/DBHIS";


        [HttpGet]
        public string Get()
        {
            string vallue = "VGHLNXVG";
            string vv = "XVGHF3";
            String MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={"XVGHF3"};password={DB2_password};";
            DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);


            try
            {
                MyDb2Connection.Open();
                DB2Command cmd = new DB2Command();
                cmd.Connection = MyDb2Connection;
                cmd.CommandText = "SELECT * FROM VGHLNXVG.DIMAUTRN";
                cmd.ExecuteNonQuery();
            }
            catch
            {
                return $"DB2 Connecting failed! , {MyDb2ConnectionString}";
            }

            return $"DB2 Connecting sucess! , {MyDb2ConnectionString}";
        }

        
    }
}
