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
    public class UDPDPDSPController : ControllerBase
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
                //    returnData.Code = -200;
                //    returnData.Result = "BarCode空白";
                //    return returnData.JsonSerializationt(true);
                //}
                string MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
                DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);

                MyDb2Connection.Open();
                string procName = $"{DB2_schema}.UDPDPDSP";
                DB2Command cmd = MyDb2Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procName;
                cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 8).Value = "31352287";
                cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = "C039";
                cmd.Parameters.Add("@TTIME", DB2Type.VarChar, 4).Value = "1800";

                DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);

                var reader = cmd.ExecuteReader();
                List<object[]> obj_temp_array = new List<object[]>();
                List<object> obj_temp = new List<object>();
                List<DSPClass> dSPClasses = new List<DSPClass>();
                List<medCpoeClass> medCpoeClasses = new List<medCpoeClass>();
                while (reader.Read())
                {
                    medCpoeClass medCpoeClass = new medCpoeClass 
                    {
                        住院號 = reader["UDCASENO"].ToString().Trim(),
                        序號 = reader["UDORDSEQ"].ToString().Trim(),
                        狀態 = reader["UDSTATUS"].ToString().Trim(),
                        開始日期 = reader["UDBGNDT2"].ToString().Trim(),
                        開始時間 = reader["UDBGNTM"].ToString().Trim(),
                        結束日期 = reader["UDENDDT2"].ToString().Trim(),
                        結束時間 = reader["UDENDTM"].ToString().Trim(),
                        藥碼 = reader["UDDRGNO"].ToString().Trim(),
                        頻次代碼 = reader["UDFREQN"].ToString().Trim(),
                        頻次屬性 = reader["UDFRQATR"].ToString().Trim(),
                        藥品名 = reader["UDDRGNAM"].ToString().Trim(),
                        途徑 = reader["UDROUTE"].ToString().Trim(),
                        數量 = reader["UDLQNTY"].ToString().Trim(),
                        劑量 = reader["UDDOSAGE"].ToString().Trim(),
                        單位 = reader["UDDUNIT"].ToString().Trim(),
                        期限 = reader["UDDURAT"].ToString().Trim(),
                        自動包藥機 = reader["UDDSPMF"].ToString().Trim(),
                        化癌分類 = reader["UDCHEMO"].ToString().Trim(),
                        自購 = reader["UDSELF"].ToString().Trim(),
                        血液製劑註記 = reader["UDALBUMI"].ToString().Trim(),
                        處方醫師 = reader["UDORSIGN"].ToString().Trim(),
                        處方醫師姓名 = reader["UDSIGNAM"].ToString().Trim(),
                        操作人員 = reader["UDLUSER"].ToString().Trim(),
                        藥局代碼 = reader["UDLRXID"].ToString().Trim(),
                        大瓶點滴 = reader["UDCNT02"].ToString().Trim(),
                        LKFLAG = reader["UDBRFNM"].ToString().Trim(),
                        排序 = reader["UDRANK"].ToString().Trim(),
                        判讀藥師代碼 = reader["PHARNUM"].ToString().Trim(),
                        判讀FLAG = reader["FLAG"].ToString().Trim(),
                        勿磨 = reader["UDNGT"].ToString().Trim(),
                        抗生素等級 = reader["UDANTICG"].ToString().Trim(),
                        重複用藥 = reader["UDSAMEDG"].ToString().Trim(),
                        配藥天數 = reader["UDDSPDY"].ToString().Trim(),
                        交互作用 = reader["UDDDI"].ToString().Trim(),
                        交互作用等級 = reader["UDDDIC"].ToString().Trim()
                    };

                    //DSPClass dSPClass = new DSPClass()
                    //{
                    //    藥品碼 = reader["UDDRGNO"].ToString().Trim(),
                    //    操作者姓名 = reader["UDLUSER"].ToString().Trim(),
                    //    效期 = reader["UDDURAT"].ToString().Trim(),
                    //    藥名 = reader["UDDRGNAM"].ToString().Trim(),
                    //    住院序號 = reader["UDORDSEQ"].ToString().Trim()
                    //};
                    medCpoeClasses.Add(medCpoeClass);
                }
                    //List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                    //while (reader.Read())
                    //{
                    //    Dictionary<string, object> row = new Dictionary<string, object>();
                    //    for (int i = 0; i < reader.FieldCount; i++)
                    //    {
                    //        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    //    }
                    //    results.Add(row);

                    //}
                MyDb2Connection.Close();
                if (medCpoeClasses.Count == 0)
                {
                    if (BarCode.StringIsEmpty())
                    {
                        returnData.Code = -200;
                        returnData.Result = "找無藥單資料";
                        return returnData.JsonSerializationt(true);
                    }
                }
                returnData.Code = 200;
                returnData.Data = medCpoeClasses;
                returnData.Result = $"取得xx成功,共<{medCpoeClasses.Count}>筆";
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
