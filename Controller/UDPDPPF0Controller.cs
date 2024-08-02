using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Basic;
using SQLUI;
using HIS_DB_Lib;
using IBM.Data.DB2.Core;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UDPDPPF0Controller : ControllerBase
    {
        static string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";
        static string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
        static string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
        static string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
        static string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";

        [HttpGet]
        public string Get(string? BarCode)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            try
            {
                //if (BarCode.StringIsEmpty())
                //{
                //    returnData.Code = 200;
                //    returnData.Result = "BarCode空白";
                //    return returnData.JsonSerializationt(true);
                //}
                string MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
                DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);

                MyDb2Connection.Open();
                string SP = "UDPDPPF0";
                string procName = $"{DB2_schema}.{SP}";
                DB2Command cmd = MyDb2Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procName;

                cmd.Parameters.Add("@THISTNO", DB2Type.VarChar, 10).Value = "41168109";
                cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 08).Value = "31580064";

                DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
                DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 60);

                var reader = cmd.ExecuteReader();
            
                List<PF1Class> PF1Classes = new List<PF1Class>();
                List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    results.Add(row);
                }
                MyDb2Connection.Close();
                PF1Class pF1Class = new PF1Class();
                foreach (var row in results)
                {
                    if (row.ContainsKey("UDPDPSY") && row.ContainsKey("UDPDPVL"))
                    {
                        string key = row["UDPDPSY"].ToString().Trim();
                        string value = row["UDPDPVL"].ToString().Trim();

                        switch (key)
                        {
                            case "PNAMEC":
                                pF1Class.姓名 = value;
                                break;
                            case "HSEXC":
                                pF1Class.性別 = value;
                                break;
                            case "PBIRTH8":
                                pF1Class.床號 = value;
                                break;
                        }
                    }
                }

                //if (results.Count == 0)
                //{
                //    if (BarCode.StringIsEmpty())
                //    {
                //        returnData.Code = -200;
                //        returnData.Result = "找無藥單資料";
                //        return returnData.JsonSerializationt(true);
                //    }
                //}
                returnData.Code = 200;
                returnData.Data = "";
                returnData.Result = $"取得xx成功,共<>筆";
                returnData.TimeTaken = myTimerBasic.ToString();
                return returnData.JsonSerializationt(true);

            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }

        }
    }
}
