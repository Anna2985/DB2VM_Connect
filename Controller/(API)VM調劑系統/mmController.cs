using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HIS_DB_Lib;
using Basic;
using System.Configuration;
using IBM.Data.DB2.Core;
using System.Data;
using SQLUI;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller._API_VM調劑系統
{
    [Route("api/[controller]")]
    [ApiController]
    public class mmController : ControllerBase
    {
        static string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";
        static string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
        static string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
        static string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
        static string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";
        [HttpPost("get_UDPDPDSP_by_bednum")]
        public string POST_get_UDPDPDSP([FromBody] returnData returnData)
        {

            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                if (returnData.ValueAry == null)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 無傳入資料";
                    return returnData.JsonSerializationt(true);
                }
                if (returnData.ValueAry.Count != 2)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[護理站,住院號]";
                    return returnData.JsonSerializationt(true);
                }
                String MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
                DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);
                string time = DateTime.Now.ToTimeString();
                time = time.Replace(":", "").Substring(0, 4);
                MyDb2Connection.Open();
                String procName = $"{DB2_schema}.UDPDPDSP";
                 DB2Command cmd = MyDb2Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procName;
                DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
                List<medCpoeClass> medCpoeClasses = new List<medCpoeClass>();

                string 護理站 = returnData.ValueAry[0];
                string 住院號 = returnData.ValueAry[1];
                cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 8).Value = 住院號;
                cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = 護理站;
                cmd.Parameters.Add("@TTIME", DB2Type.VarChar, 4).Value = time;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    medCpoeClass medCpoeClass = new medCpoeClass
                    {

                        住院號 = 住院號,
                        //序號 = reader["UDORDSEQ"].ToString().Trim(),
                        //狀態 = reader["UDSTATUS"].ToString().Trim(),
                        //開始日期 = reader["UDBGNDT2"].ToString().Trim(),
                        //開始時間 = reader["UDBGNTM"].ToString().Trim(),
                        //結束日期 = reader["UDENDDT2"].ToString().Trim(),
                        //結束時間 = reader["UDENDTM"].ToString().Trim(),
                        //藥碼 = reader["UDDRGNO"].ToString().Trim(),
                        //頻次代碼 = reader["UDFREQN"].ToString().Trim(),
                        //頻次屬性 = reader["UDFRQATR"].ToString().Trim(),
                        //藥品名 = reader["UDDRGNAM"].ToString().Trim(),
                        //途徑 = reader["UDROUTE"].ToString().Trim(),
                        //數量 = reader["UDLQNTY"].ToString().Trim(),
                        //劑量 = reader["UDDOSAGE"].ToString().Trim(),
                        //單位 = reader["UDDUNIT"].ToString().Trim(),
                        //期限 = reader["UDDURAT"].ToString().Trim(),
                        //自動包藥機 = reader["UDDSPMF"].ToString().Trim(),
                        //化癌分類 = reader["UDCHEMO"].ToString().Trim(),
                        //自購 = reader["UDSELF"].ToString().Trim(),
                        //血液製劑註記 = reader["UDALBUMI"].ToString().Trim(),
                        //處方醫師 = reader["UDORSIGN"].ToString().Trim(),
                        //處方醫師姓名 = reader["UDSIGNAM"].ToString().Trim(),
                        //操作人員 = reader["UDLUSER"].ToString().Trim(),
                        //藥局代碼 = reader["UDLRXID"].ToString().Trim(),
                        //大瓶點滴 = reader["UDCNT02"].ToString().Trim(),
                        //LKFLAG = reader["UDBRFNM"].ToString().Trim(),
                        //排序 = reader["UDRANK"].ToString().Trim(),
                        //判讀藥師代碼 = reader["PHARNUM"].ToString().Trim(),
                        //判讀FLAG = reader["FLAG"].ToString().Trim(),
                        //勿磨 = reader["UDNGT"].ToString().Trim(),
                        //抗生素等級 = reader["UDANTICG"].ToString().Trim(),
                        //重複用藥 = reader["UDSAMEDG"].ToString().Trim(),
                        //配藥天數 = reader["UDDSPDY"].ToString().Trim(),
                        //交互作用 = reader["UDDDI"].ToString().Trim(),
                        //交互作用等級 = reader["UDDDIC"].ToString().Trim(),
                    };
                    medCpoeClasses.Add(medCpoeClass);
                }


                MyDb2Connection.Close();
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
        [HttpPost("get_UDPDPPF1")]
        public string POST_get_UDPDPPF1([FromBody] returnData returnData)
        {

            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                if (returnData.ValueAry == null)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 無傳入資料";
                    return returnData.JsonSerializationt(true);
                }
                if (returnData.ValueAry.Count != 1)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[護理站]";
                    return returnData.JsonSerializationt(true);
                }

                string 護理站 = returnData.ValueAry[0];

                String MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
                DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);
                MyDb2Connection.Open();
                String procName = $"{DB2_schema}.UDPDPPF1";
                DB2Command cmd = MyDb2Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procName;

                cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = 護理站;

                DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
                DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 60);

                var reader = cmd.ExecuteReader();
                List<medCarInfoClass> medCarInfoClasses = new List<medCarInfoClass>();
                while (reader.Read())
                {
                    medCarInfoClass medCarInfoClass = new medCarInfoClass
                    {
                        護理站 = reader["HNURSTA"].ToString().Trim(),
                        床號 = reader["HBEDNO"].ToString().Trim(),
                        病歷號 = reader["HISTNUM"].ToString().Trim(),
                        住院號 = reader["PCASENO"].ToString().Trim(),
                        姓名 = reader["PNAMEC"].ToString().Trim(),
                        占床狀態 = reader["HBEDSTAT"].ToString().Trim()
                    };
                    if (medCarInfoClass.占床狀態 == "O")
                    {
                        medCarInfoClasses.Add(medCarInfoClass);
                    }
                }
                MyDb2Connection.Close();
                if (medCarInfoClasses.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.Result = "找無資料";
                    return returnData.JsonSerializationt(true);
                }

                List<object[]> list_病患資料 = medCarInfoClasses.ClassToSQL<medCarInfoClass, enum_病床資訊>();
                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = list_病患資料;
                returnData.Result = $"取得 {護理站} 的病床資訊共{list_病患資料.Count}筆";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        [HttpPost("get_bed_list_by_cart")]
        public string test_get_UDPDPPF1([FromBody] returnData returnData)
        {

            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                if (returnData.ValueAry == null)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 無傳入資料";
                    return returnData.JsonSerializationt(true);
                }
                if (returnData.ValueAry.Count != 2)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[住院藥局,藥車]";
                    return returnData.JsonSerializationt(true);
                }
                string 住院藥局 = returnData.ValueAry[0];
                string 護理站 = returnData.ValueAry[1];
                List<medCarInfoClass> medCarInfoClasses = new List<medCarInfoClass>();
                medCarInfoClass value1 = new medCarInfoClass
                {
                    護理站 = 護理站,
                    床號 = "18",
                    病歷號 = "50487939",
                    住院號 = "31463236",
                    姓名 = "克里斯",
                    占床狀態 = "O"
                };
                medCarInfoClasses.Add(value1);
                medCarInfoClass value2 = new medCarInfoClass
                {
                    護理站 = 護理站,
                    床號 = "8",
                    病歷號 = "21702181",
                    住院號 = "31540178",
                    姓名 = "方可炫",
                    占床狀態 = "O"
                };
                medCarInfoClasses.Add(value2);
                medCarInfoClass value3 = new medCarInfoClass
                {
                    護理站 = 護理站,
                    床號 = "3",
                    病歷號 = "",
                    住院號 = "",
                    姓名 = "",
                    占床狀態 = "U"

                };
                medCarInfoClasses.Add(value3);
                medCarInfoClass value4 = new medCarInfoClass
                {
                    護理站 = 護理站,
                    床號 = "20",
                    病歷號 = "16433275",
                    住院號 = "31490583",
                    姓名 = "湯馬士",
                    占床狀態 = "O"
                };
                medCarInfoClasses.Add(value1);
                medCarInfoClass value5 = new medCarInfoClass
                {
                    護理站 = 護理站,
                    床號 = "50",
                    病歷號 = "28870306",
                    住院號 = "31517761",
                    姓名 = "偉杰戰士",
                    占床狀態 = "O"
                };
                medCarInfoClasses.Add(value5);


                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = medCarInfoClasses;
                returnData.Result = $"取得 {護理站} 的病床資訊共{medCarInfoClasses.Count}筆";
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

