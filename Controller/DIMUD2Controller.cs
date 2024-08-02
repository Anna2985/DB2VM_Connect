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
    public class DIMUD2Controller : ControllerBase
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

            String procName = $"{DB2_schema}.DIMUD2";
            DB2Command cmd = MyDb2Connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from 'DIMUD2'";
            //cmd.Transaction = trans;
            //cmd.Parameters.Add("@TSYSD", DB2Type.VarChar, 5).Value = "ADMUD";
            ////cmd.Parameters.Add("@TUDDRGNO", DB2Type.VarChar, 5).Value = "";



            //DB2Parameter TFLAG = cmd.Parameters.Add("@TFLAG", DB2Type.Char);
            //TFLAG.Direction = ParameterDirection.Output;


            //DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);


            //DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 200);
            //RETMSG.Direction = ParameterDirection.Output;
            ////cmd.CommandTimeout = 3000;


            var reader = cmd.ExecuteReader();
            //Console.Write($"TFLAG  : {TFLAG.Value}\n");
            //Console.Write($"RET    : {RET.Value}\n");
            //Console.Write($"RET    : {RETMSG.Value}\n");
            //Console.Write($"進行Query...\n");
            //List<object[]> obj_temp_array = new List<object[]>();
            //List<object> obj_temp = new List<object>();
            //Console.Write($"FieldCount : {reader.FieldCount} \n");
            while (reader.Read())
            {
                string str = reader["DRUGSTNO"].ToString().Trim();

            }
            return "OK";
        }
    }
}
