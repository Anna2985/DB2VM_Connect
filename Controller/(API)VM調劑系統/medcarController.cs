using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basic;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using HIS_DB_Lib;
using System.Configuration;
using IBM.Data.DB2.Core;
using System.Data;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller._API_VM調劑系統
{
    [Route("api/[controller]")]
    [ApiController]
    public class medcarController : ControllerBase
    {
        static string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";
        static string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
        static string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
        static string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
        static string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";

        static private string API_Server = "http://127.0.0.1:4433/api/serversetting";
        static private MySqlSslMode SSLMode = MySqlSslMode.None;
        [HttpPost("get_bed_list_by_cart")]
        public string get_bed_list_by_cart([FromBody] returnData returnData)
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
                    returnData.Result = $"returnData.ValueAry 內容應為[藥局, 護理站]";
                    return returnData.JsonSerializationt(true);
                }
                string 藥局 = returnData.ValueAry[0];
                string 護理站 = returnData.ValueAry[1];
                //以下請修改
                
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
                        藥局 = 藥局,
                        護理站 = reader["HNURSTA"].ToString().Trim(),
                        床號 = reader["HBEDNO"].ToString().Trim(),
                        病歷號 = reader["HISTNUM"].ToString().Trim(),
                        住院號 = reader["PCASENO"].ToString().Trim(),
                        姓名 = reader["PNAMEC"].ToString().Trim(),
                        //占床狀態 = reader["HBEDSTAT"].ToString().Trim()
                    };
                    if (reader["HBEDSTAT"].ToString().Trim() == "O")
                    {
                        medCarInfoClass.占床狀態 = "已佔床";                 
                    }
                    else
                    {
                        medCarInfoClass.占床狀態 = "";
                    }
                    medCarInfoClasses.Add(medCarInfoClass);
                }
                MyDb2Connection.Close();

                
                procName = $"{DB2_schema}.UDPDPPF0";
                for (int i = 0; i < medCarInfoClasses.Count; i++)
                {
                    MyDb2Connection.Open();
                    string 病歷號 = medCarInfoClasses[i].病歷號;
                    string 住院號 = medCarInfoClasses[i].住院號;
                    cmd = MyDb2Connection.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = procName;
                    cmd.Parameters.Add("@THISTNO", DB2Type.VarChar,10).Value = 病歷號;
                    cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 8).Value = 住院號;
                    RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
                    RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 60);
                    reader = cmd.ExecuteReader();
                    List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int j = 0; j < reader.FieldCount; j++)
                        {
                            row[reader.GetName(j)] = reader.IsDBNull(j) ? null : reader.GetValue(j);
                        }
                        results.Add(row);
                    }

                    List<testResult> testResults = new List<testResult>();
                    testResult testResult = new testResult();

                    foreach (var row in results)
                    {
                        if (row.ContainsKey("UDPDPSY") && row.ContainsKey("UDPDPVL"))
                        {
                            string key = row["UDPDPSY"].ToString().Trim();
                            string value = row["UDPDPVL"].ToString().Trim();
                            if (key == "HSEXC") medCarInfoClasses[i].性別 = value;
                            if (key == "PBIRTH8") medCarInfoClasses[i].出生日期 = value;
                            if (key == "PSECTC") medCarInfoClasses[i].科別 = value;
                            if (key == "PFINC") medCarInfoClasses[i].財務 = value;
                            if (key == "PADMDT") medCarInfoClasses[i].入院日期 = value;
                            if (key == "PVSDNO") medCarInfoClasses[i].訪視號碼 = value;
                            if (key == "PVSNAM") medCarInfoClasses[i].診所名稱 = value;
                            if (key == "PRNAM") medCarInfoClasses[i].醫生姓名 = value;
                            if (key == "PBHIGHT") medCarInfoClasses[i].身高 = value;
                            if (key == "PBWEIGHT") medCarInfoClasses[i].體重 = value;
                            if (key == "PBBSA") medCarInfoClasses[i].體表面積 = value;
                            if (key == "HICD1") medCarInfoClasses[i].國際疾病分類代碼1 = value;
                            if (key == "HICDTX1") medCarInfoClasses[i].疾病說明1 = value;
                            if (key == "HICD2") medCarInfoClasses[i].國際疾病分類代碼2 = value;
                            if (key == "HICDTX2") medCarInfoClasses[i].疾病說明2 = value;
                            if (key == "HICD3") medCarInfoClasses[i].國際疾病分類代碼3 = value;
                            if (key == "HICDTX3") medCarInfoClasses[i].疾病說明3 = value;
                            if (key == "HICD4") medCarInfoClasses[i].國際疾病分類代碼4 = value;
                            if (key == "HICDTX4") medCarInfoClasses[i].疾病說明4 = value;
                            if (key == "NGTUBE") medCarInfoClasses[i].鼻胃管使用狀況 = value;
                            if (key == "TUBE") medCarInfoClasses[i].其他管路使用狀況 = value;
                            if (key == "RTALB") testResult.白蛋白 = value;
                            if (key == "RTCREA") testResult.肌酸酐 = value;
                            if (key == "RTEGFRM") testResult.估算腎小球過濾率 = value;
                            if (key == "RTALT") testResult.丙氨酸氨基轉移酶 = value;
                            if (key == "RTK") testResult.鉀離子 = value;
                            if (key == "RTCA") testResult.鈣離子 = value;
                            if (key == "RTTB") testResult.總膽紅素 = value;
                            if (key == "RTNA") testResult.鈉離子 = value;
                            if (key == "RTWBC") testResult.白血球計數 = value;
                            if (key == "RTHGB") testResult.血紅素 = value;
                            if (key == "RTPLT") testResult.血小板計數 = value;
                            if (key == "RTINR") testResult.國際標準化比率 = value;
                        }
                    }


                    testResults.Add(testResult);
                    medCarInfoClasses[i].檢驗結果 = testResults;
                    MyDb2Connection.Close();
                }
                medCarInfoClasses.Sort(new medCarInfoClass.ICP_By_bedNum());
                MyDb2Connection = new DB2Connection(MyDb2ConnectionString);
                MyDb2Connection.Open();
                for (int i = 0; i < medCarInfoClasses.Count; i++)
                {
                    string 住院號 = medCarInfoClasses[i].住院號;
                    if (住院號.StringIsEmpty()) continue;
                    string time = DateTime.Now.ToTimeString();
                    time = time.Replace(":", "").Substring(0, 4);
              
                    procName = $"{DB2_schema}.UDPDPDSP";
                    cmd = MyDb2Connection.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = procName;
                    cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 8).Value = 住院號;
                    cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = 護理站;
                    cmd.Parameters.Add("@TTIME", DB2Type.VarChar, 4).Value = time;
                    RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
                    reader = cmd.ExecuteReader();
                    List<medCpoeClass> prescription = new List<medCpoeClass>();
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
                        prescription.Add(medCpoeClass);
                    }

                    if (prescription.Count > 3)
                    {
                        medCarInfoClasses[i].處方 = prescription.GetRange(0, Math.Min(4, prescription.Count));
                        medCarInfoClasses[i].處方2 = prescription.GetRange(3, prescription.Count - 3);
                    }
                    else
                    {
                        medCarInfoClasses[i].處方 = prescription;
                        medCarInfoClasses[i].處方2 = new List<medCpoeClass>();  
                    }
                    cmd.Dispose();
       
                    //string json_prescription = prescription.JsonSerializationt();
                    //medCarInfoClasses[i].處方 = json_prescription;
                 
                }
                Logger.Log("medcarController", $"處方共<{medCarInfoClasses.Count}>筆");
                MyDb2Connection.Close();
                //以上要改

                List<ServerSettingClass> serverSettingClasses = ServerSettingClassMethod.WebApiGet($"{API_Server}");
                serverSettingClasses = serverSettingClasses.MyFind("Main", "網頁", "VM端");
                string Server = serverSettingClasses[0].Server;
                uint Port = (uint)serverSettingClasses[0].Port.StringToInt32();
                string url = $"http://{Server}:4436/api/med_cart/update_bed_list";
                returnData returnData_apiserver = new returnData();
                
                returnData_apiserver.Data = medCarInfoClasses;
                string jsonData = returnData_apiserver.JsonSerializationt(true);
                string responseString = Basic.Net.WEBApiPostJson(url, jsonData, false);
                returnData result = responseString.JsonDeserializet<returnData>();

                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = medCarInfoClasses;
                returnData.Result = $"取得 {護理站} 的病床資訊共筆";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        [HttpPost("get_cpoe_by_bedNum")]
        public string get_cpoe_by_bedNum([FromBody] returnData returnData)
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
                if (returnData.ValueAry.Count != 3)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[藥局, 護理站, 床號]";
                    return returnData.JsonSerializationt(true);
                }
                string 藥局 = returnData.ValueAry[0];
                string 護理站 = returnData.ValueAry[1];
                string 床號 = returnData.ValueAry[2];
                //更新病床資訊
                List<ServerSettingClass> serverSettingClasses = ServerSettingClassMethod.WebApiGet($"{API_Server}");
                serverSettingClasses = serverSettingClasses.MyFind("Main", "網頁", "VM端");
                string Server = serverSettingClasses[0].Server;
                //uint Port = (uint)serverSettingClasses[0].Port.StringToInt32();
                //DataList<object[]> dataList = new DataList<object[]>()
                //{
                //    ValueAry = new List<string> { 藥局, 護理站 }
                //};
                //string url = $"http://{Server}:4436/api/medcar/get_bed_list_by_cart";
                //string jsonData = dataList.JsonSerializationt(true);
                //string responseString = Basic.Net.WEBApiPostJson(url, jsonData, false);

                //取得目標病人資料
                string url = $"http://{Server}:4436/api/med_cart/get_patient_by_bedNum";
                DataList<object[]> dataList = new DataList<object[]>()
                {
                    ValueAry = new List<string> { 藥局, 護理站, 床號 }
                };
                string jsonData = dataList.JsonSerializationt(true);
                string responseString = Basic.Net.WEBApiPostJson(url, jsonData, false);
                returnData result = responseString.JsonDeserializet<returnData>();
                List<medCarInfoClass> target_patient = (result.Data).ObjToClass<List<medCarInfoClass>>();

                if (target_patient.Count != 1)
                {
                    returnData.Code = -200;
                    returnData.Result = $"資料錯誤";
                    return returnData.JsonSerializationt(true);
                }
                if (target_patient[0].住院號.StringIsEmpty())
                {
                    returnData.Code = -200;
                    returnData.Result = $"無處資料";
                    return returnData.JsonSerializationt(true);
                }

                string 住院號 = target_patient[0].住院號;
                string time = DateTime.Now.ToTimeString();
                time = time.Replace(":", "").Substring(0, 4);
                string MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
                DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);

                MyDb2Connection.Open();
                string procName = $"{DB2_schema}.UDPDPDSP";
                DB2Command cmd = MyDb2Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procName;
                cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 8).Value = 住院號;
                cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = 護理站;
                cmd.Parameters.Add("@TTIME", DB2Type.VarChar, 4).Value = time;

                DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);

                var reader = cmd.ExecuteReader();
                List<medCpoeClass> prescription = new List<medCpoeClass>();
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
                        交互作用等級 = reader["UDDDIC"].ToString().Trim(),
                    };
                    prescription.Add(medCpoeClass);
                }


                MyDb2Connection.Close();
                //從SP取得處方資料

                target_patient[0].處方 = prescription;
                List<medCarInfoClass> update_list = new List<medCarInfoClass>();
                update_list.Add(target_patient[0]);
                //server
                url = $"http://{Server}:4436/api/med_cart/update_bed_list";
                DataList<medCarInfoClass> dataList_class = new DataList<medCarInfoClass>
                {
                    Data = new List<medCarInfoClass>(update_list)
                };
                jsonData = dataList_class.JsonSerializationt(true);
                responseString = Basic.Net.WEBApiPostJson(url, jsonData, false);

                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = update_list;
                returnData.Result = $"取得{target_patient[0].姓名}的處方";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        [HttpPost("getAll_cpoe")]
        public string getAll_cpoe([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                   
                //更新病床資訊
                List<ServerSettingClass> serverSettingClasses = ServerSettingClassMethod.WebApiGet($"{API_Server}");
                serverSettingClasses = serverSettingClasses.MyFind("Main", "網頁", "VM端");
                string Server = serverSettingClasses[0].Server;
                //uint Port = (uint)serverSettingClasses[0].Port.StringToInt32();
                //DataList<object[]> dataList = new DataList<object[]>()
                //{
                //    ValueAry = new List<string> { 藥局, 護理站 }
                //};
                //string url = $"http://{Server}:4436/api/medcar/get_bed_list_by_cart";
                //string jsonData = dataList.JsonSerializationt(true);
                //string responseString = Basic.Net.WEBApiPostJson(url, jsonData, false);

                //取得目標病人資料
                string url = $"http://{Server}:4436/api/med_cart/get_all";
                DataList<object[]> dataList = new DataList<object[]>()
                {
                    ValueAry = new List<string>()
                };
                string jsonData = dataList.JsonSerializationt(true);
                string responseString = Basic.Net.WEBApiPostJson(url, jsonData, false);
                returnData result = responseString.JsonDeserializet<returnData>();
                List<medCarInfoClass> target_patient = (result.Data).ObjToClass<List<medCarInfoClass>>();
                List<medCarInfoClass> update_list = new List<medCarInfoClass>();

                for (int i = 0; i < target_patient.Count; i++)
                {
                    if (target_patient[i].住院號.StringIsEmpty()) continue;                 
                    string 護理站 = target_patient[i].護理站;
                    string 住院號 = target_patient[i].住院號;
                    string time = DateTime.Now.ToTimeString();
                    time = time.Replace(":", "").Substring(0, 4);
                    string MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
                    DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);

                    MyDb2Connection.Open();
                    string procName = $"{DB2_schema}.UDPDPDSP";
                    DB2Command cmd = MyDb2Connection.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = procName;
                    cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 8).Value = 住院號;
                    cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = 護理站;
                    cmd.Parameters.Add("@TTIME", DB2Type.VarChar, 4).Value = time;

                    DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);

                    var reader = cmd.ExecuteReader();
                    List<medCpoeClass> prescription = new List<medCpoeClass>();
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
                            交互作用等級 = reader["UDDDIC"].ToString().Trim(),
                        };
                        prescription.Add(medCpoeClass);
                    }


                    MyDb2Connection.Close();
                    //從SP取得處方資料

                    target_patient[i].處方 = prescription;
                    update_list.Add(target_patient[i]);

                }

                //server
                url = $"http://{Server}:4436/api/med_cart/update_bed_list";
                DataList<medCarInfoClass> dataList_class = new DataList<medCarInfoClass>
                {
                    Data = new List<medCarInfoClass>(update_list)
                };
                jsonData = dataList_class.JsonSerializationt(true);
                responseString = Basic.Net.WEBApiPostJson(url, jsonData, false);

                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = update_list;
                returnData.Result = $"取得處方";
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
