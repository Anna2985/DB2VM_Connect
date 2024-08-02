using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IBM.Data.DB2.Core;
using System.Data;
using System.Configuration;
using Basic;

namespace DB2VM_API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UDSDBPF1Controller : ControllerBase
    {
        string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";
        string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
        string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
        string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
        string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";
        [HttpGet]
        public string Get()
        {
            String MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
            DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);

            MyDb2Connection.Open();

            String procName = $"{DB2_schema}.UDSDBPF1";
            DB2Command cmd = new DB2Command();
            cmd.Connection = MyDb2Connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = procName;
            //cmd.Transaction = trans;
            cmd.Parameters.Add("@TSYSD", DB2Type.VarChar, 5).Value = "ADMUD";
            cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = "0013";

            DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
            RET.Direction = ParameterDirection.Output;

            DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 200);
            RETMSG.Direction = ParameterDirection.Output;
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {

            }
            return "OK";
        }
    }
}
