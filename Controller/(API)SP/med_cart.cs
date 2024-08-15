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

namespace DB2VM_API.Controller.API_SP
{
    [Route("api/[controller]")]
    [ApiController]
    public class med_cart : ControllerBase
    {
        static private string API_Server = "http://127.0.0.1:4433/api/serversetting";
        static string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";
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
                List<ServerSettingClass> serverSettingClasses = ServerSettingClassMethod.WebApiGet($"{API_Server}");
                serverSettingClasses = serverSettingClasses.MyFind("Main", "網頁", "VM端");
                if (serverSettingClasses.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.Result = $"找無Server資料!";
                    return returnData.JsonSerializationt();
                }

                string Server = serverSettingClasses[0].Server;
                string API = $"http://{Server}:4436";
                string 藥局 = returnData.ValueAry[0];
                string 護理站 = returnData.ValueAry[1];
                List<medCarInfoClass> bedList = ExecuteUDPDPPF1(藥局, 護理站);
                List<medCarInfoClass> bedListInfo = ExecuteUDPDPPF0(bedList);

                List<medCpoeClass> bedListCpoe = ExecuteUDPDPDSP(bedListInfo);

                //List<string> medCode = bedListCpoe
                //    .GroupBy(code => code.藥碼)
                //    .Select(group => group.Key)
                //    .ToList();

                //List<string> inputMedCode = new List<string> { string.Join(",", medCode) };
                //List<medClass> medClasses = medCpoeClass.get_med_clouds_by_codes(API, inputMedCode);
                //var medDict = medClasses.ToDictionary(med => med.藥品碼, med => med.中文名稱);
                //foreach (var cpoe in bedListCpoe)
                //{
                //    if (medDict.TryGetValue(cpoe.藥碼, out string medName))
                //    {
                //        cpoe.中文名 = medName;
                //    }
                //}

                List<string> valueAry = new List<string> { 藥局, 護理站 };

                List<medCarInfoClass> update_medCarInfoClass = medCarInfoClass.update_med_carinfo(API, bedListInfo);
                List<medCpoeClass> update_medCpoeClass = medCpoeClass.update_med_cpoe(API, bedListCpoe, valueAry);

                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = update_medCarInfoClass;
                returnData.Result = $"取得 {護理站} 病床資訊共{update_medCarInfoClass.Count}/{bedListInfo.Count}筆";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        [HttpPost("handover")]
        public string handover([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                DateTime now = DateTime.Now;
                if (now.TimeOfDay < new TimeSpan(15, 0, 0))
                {
                    returnData.Code = -200;
                    returnData.Result = "執行失敗：目前時間尚未超過下午三點。";
                    return returnData.JsonSerializationt(true);
                }
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
                List<ServerSettingClass> serverSettingClasses = ServerSettingClassMethod.WebApiGet($"{API_Server}");
                serverSettingClasses = serverSettingClasses.MyFind("Main", "網頁", "VM端");
                string Server = serverSettingClasses[0].Server;
                string API = $"http://{Server}:4436";
                string result = medCpoeClass.handover(API, returnData.ValueAry);
                returnData = new returnData();
                returnData = result.JsonDeserializet<returnData>();
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }


        [HttpGet("UDPDPPF1")]
        public string UDPDPPF1()
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            string 藥局 = "UC02";
            string 護理站 = "C039";
            string Server = "";
            List<medCarInfoClass> medCarInfoClasses = ExecuteUDPDPPF1(藥局, 護理站);
            //List<medCarInfoClass> output_medCarInfoClass = medCarInfoClass.update_bed_list(Server, medCarInfoClasses);
            returnData.Code = 200;
            returnData.TimeTaken = $"{myTimerBasic}";
            returnData.Data = medCarInfoClasses;
            returnData.Result = $"取得 {護理站} 的病床資訊共{medCarInfoClasses.Count}筆";
            return returnData.JsonSerializationt(true);
        }
        [HttpGet("UDPDPPF0")]
        public string UDPDPPF0()
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            List<medCarInfoClass> medCarInfoClassList = new List<medCarInfoClass>();
            medCarInfoClass v1 = new medCarInfoClass
            {
                病歷號 = "9394632",
                住院號 = "31620090"
            };
            medCarInfoClassList.Add(v1);
            List<medCarInfoClass> medCarInfoClasses = ExecuteUDPDPPF0(medCarInfoClassList);

            returnData.Code = 200;
            returnData.TimeTaken = $"{myTimerBasic}";
            returnData.Data = medCarInfoClasses;
            returnData.Result = $"取得病人資料共{medCarInfoClasses.Count}筆";
            return returnData.JsonSerializationt(true);
        }
        [HttpGet("UDPDPDSP")]
        public string UDPDPDSP()
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            List<medCarInfoClass> medCarInfoClassList = new List<medCarInfoClass>();
            medCarInfoClass v1 = new medCarInfoClass
            {
                藥局 = "UC02",
                護理站 = "C079",
                住院號 = "31695645"
            };
            medCarInfoClassList.Add(v1);
            List<medCpoeClass> medCarInfoClasses = ExecuteUDPDPDSP(medCarInfoClassList);

            returnData.Code = 200;
            returnData.TimeTaken = $"{myTimerBasic}";
            returnData.Data = medCarInfoClasses;
            returnData.Result = $"取得病床處方共{medCarInfoClasses.Count}筆";
            return returnData.JsonSerializationt(true);
        }
        [HttpGet("UDPDPORD")]
        public string UDPDPORD()
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            List<medCarInfoClass> medCarInfoClassList = new List<medCarInfoClass>();
            medCarInfoClass v1 = new medCarInfoClass
            {
                藥局 = "UC02",
                護理站 = "C079",
                住院號 = "31641549"
            };
            medCarInfoClassList.Add(v1);
            List<medCpoeClass> medCarInfoClasses = ExecuteUDPDPORD(medCarInfoClassList);
            returnData.Code = 200;
            returnData.TimeTaken = $"{myTimerBasic}";
            returnData.Data = medCarInfoClasses;
            returnData.Result = $"取得病床處方共{medCarInfoClasses.Count}筆";
            return returnData.JsonSerializationt(true);
        }
        private DB2Connection GetDB2Connection()
        {
            string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";
            string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
            string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
            string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
            string MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
            return new DB2Connection(MyDb2ConnectionString);
        }
        private List<medCarInfoClass> ExecuteUDPDPPF1(string phar, string hnursta)
        {
            using (DB2Connection MyDb2Connection = GetDB2Connection())
            {
                MyDb2Connection.Open();
                string SP = "UDPDPPF1";
                string procName = $"{DB2_schema}.{SP}";
                using (DB2Command cmd = MyDb2Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = procName;
                    cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = hnursta;
                    DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
                    DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 60);
                    using (DB2DataReader reader = cmd.ExecuteReader())
                    {
                        List<medCarInfoClass> medCarInfoClasses = new List<medCarInfoClass>();
                        while (reader.Read())
                        {


                            medCarInfoClass medCarInfoClass = new medCarInfoClass
                            {
                                藥局 = phar,
                                護理站 = reader["HNURSTA"].ToString().Trim(),
                                床號 = reader["HBEDNO"].ToString().Trim(),
                                病歷號 = reader["HISTNUM"].ToString().Trim(),
                                住院號 = reader["PCASENO"].ToString().Trim(),
                                姓名 = reader["PNAMEC"].ToString().Trim()                              
                                //占床狀態 = reader["HBEDSTAT"].ToString().Trim() == "O" ? "已佔床" : ""
                            };
                            if (medCarInfoClass.姓名 != null) medCarInfoClass.占床狀態 = "已佔床";
                            medCarInfoClasses.Add(medCarInfoClass);
                        }
                        return medCarInfoClasses;
                    }
                }
            }
        }
        private List<medCarInfoClass> ExecuteUDPDPPF0(List<medCarInfoClass> medCarInfoClasses)
        {
            using (DB2Connection MyDb2Connection = GetDB2Connection())
            {
                MyDb2Connection.Open();
                string SP = "UDPDPPF0";
                string procName = $"{DB2_schema}.{SP}";
                foreach (var medCarInfoClass in medCarInfoClasses)
                {
                    if (medCarInfoClass.住院號.StringIsEmpty()) continue;
                    using (DB2Command cmd = MyDb2Connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = procName;
                        cmd.Parameters.Add("@THISTNO", DB2Type.VarChar, 10).Value = medCarInfoClass.病歷號;
                        cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 8).Value = medCarInfoClass.住院號;
                        DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
                        DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 60);
                        using (DB2DataReader reader = cmd.ExecuteReader())
                        {
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



                            foreach (var row in results)
                            {
                                if (row.ContainsKey("UDPDPSY") && row.ContainsKey("UDPDPVL"))
                                {
                                    string key = row["UDPDPSY"].ToString().Trim();
                                    string value = row["UDPDPVL"].ToString().Trim();
                                    if (key == "HSEXC") medCarInfoClass.性別 = value;
                                    if (key == "PBIRTH8") medCarInfoClass.出生日期 = value;
                                    if (key == "PSECTC") medCarInfoClass.科別 = value;
                                    if (key == "PFINC") medCarInfoClass.財務 = value;
                                    if (key == "PADMDT") medCarInfoClass.入院日期 = value;
                                    if (key == "PVSDNO") medCarInfoClass.主治醫師代碼 = value;
                                    if (key == "PRDNO") medCarInfoClass.住院醫師代碼 = value;
                                    if (key == "PVSNAM") medCarInfoClass.診所名稱 = value;
                                    if (key == "PRNAM") medCarInfoClass.醫生姓名 = value;
                                    if (key == "PBHIGHT") medCarInfoClass.身高 = value;
                                    if (key == "PBWEIGHT") medCarInfoClass.體重 = value;
                                    if (key == "PBBSA") medCarInfoClass.體表面積 = value;
                                    if (key == "HICD1") medCarInfoClass.國際疾病分類代碼1 = value;
                                    if (key == "HICDTX1") medCarInfoClass.疾病說明1 = value;
                                    if (key == "HICD2") medCarInfoClass.國際疾病分類代碼2 = value;
                                    if (key == "HICDTX2") medCarInfoClass.疾病說明2 = value;
                                    if (key == "HICD3") medCarInfoClass.國際疾病分類代碼3 = value;
                                    if (key == "HICDTX3") medCarInfoClass.疾病說明3 = value;
                                    if (key == "HICD4") medCarInfoClass.國際疾病分類代碼4 = value;
                                    if (key == "HICDTX4") medCarInfoClass.疾病說明4 = value;
                                    if (key == "NGTUBE") medCarInfoClass.鼻胃管使用狀況 = value;
                                    if (key == "TUBE") medCarInfoClass.其他管路使用狀況 = value;
                                    if (key == "HAllERGY") medCarInfoClass.過敏史 = value;
                                    if (key == "RTALB") medCarInfoClass.白蛋白 = value;
                                    if (key == "RTCREA") medCarInfoClass.肌酸酐 = value;
                                    if (key == "RTEGFRM") medCarInfoClass.估算腎小球過濾率 = value;
                                    if (key == "RTALT") medCarInfoClass.丙氨酸氨基轉移酶 = value;
                                    if (key == "RTK") medCarInfoClass.鉀離子 = value;
                                    if (key == "RTCA") medCarInfoClass.鈣離子 = value;
                                    if (key == "RTTB") medCarInfoClass.總膽紅素 = value;
                                    if (key == "RTNA") medCarInfoClass.鈉離子 = value;
                                    if (key == "RTWBC") medCarInfoClass.白血球 = value;
                                    if (key == "RTHGB") medCarInfoClass.血紅素 = value;
                                    if (key == "RTPLT") medCarInfoClass.血小板 = value;
                                    if (key == "RTINR") medCarInfoClass.國際標準化比率 = value;
                                }
                            }
                        }
                    }
                }
                return medCarInfoClasses;
            }
        }
        private List<medCpoeClass> ExecuteUDPDPDSP(List<medCarInfoClass> medCarInfoClasses)
        {
            using (DB2Connection MyDb2Connection = GetDB2Connection())
            {
                MyDb2Connection.Open();
                string procName = $"{DB2_schema}.UDPDPDSP";
                List<medCpoeClass> prescription = new List<medCpoeClass>();
                foreach (var medCarInfoClass in medCarInfoClasses)
                {
                    if (medCarInfoClass.住院號.StringIsEmpty()) continue;
                    string time = DateTime.Now.ToString("HHmm");

                    using (DB2Command cmd = MyDb2Connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = procName;
                        cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 8).Value = medCarInfoClass.住院號;
                        cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = medCarInfoClass.護理站;
                        cmd.Parameters.Add("@TTIME", DB2Type.VarChar, 4).Value = time;
                        DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
                        using (DB2DataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string 開始日期 = reader["UDBGNDT2"].ToString().Trim();
                                string 開始時間 = reader["UDBGNTM"].ToString().Trim();
                                string 日期時間 = $"{開始日期} {開始時間.Substring(0, 2)}:{開始時間.Substring(2, 2)}:00";
                                DateTime 開始日期時間 = DateTime.ParseExact(日期時間, "yyyy-MM-dd HH:mm:ss", null);
                                string 結束日期 = reader["UDENDDT2"].ToString().Trim();
                                string 結束時間 = reader["UDENDTM"].ToString().Trim();
                                日期時間 = $"{結束日期} {結束時間.Substring(0, 2)}:{結束時間.Substring(2, 2)}:00";
                                DateTime 結束日期時間 = DateTime.ParseExact(日期時間, "yyyy-MM-dd HH:mm:ss", null);
                                medCpoeClass medCpoeClass = new medCpoeClass
                                {
                                    藥局 = medCarInfoClass.藥局,
                                    護理站 = medCarInfoClass.護理站,
                                    床號 = medCarInfoClass.床號,
                                    住院號 = reader["UDCASENO"].ToString().Trim(),
                                    序號 = reader["UDORDSEQ"].ToString().Trim(),
                                    狀態 = reader["UDSTATUS"].ToString().Trim(),
                                    開始時間 = 開始日期時間.ToDateTimeString_6(),
                                    結束時間 = 結束日期時間.ToDateTimeString_6(),
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
                        }
                    }
                }
                return prescription;
            }
           
        }
        private List<medCpoeClass> ExecuteUDPDPORD(List<medCarInfoClass> medCarInfoClasses)
        {
            using (DB2Connection MyDb2Connection = GetDB2Connection())
            {
                MyDb2Connection.Open();
                string procName = $"{DB2_schema}.UDPDPORD";
                List<medCpoeClass> prescription = new List<medCpoeClass>();
                foreach (var medCarInfoClass in medCarInfoClasses)
                {
                    if (medCarInfoClass.住院號.StringIsEmpty()) continue;
                    string time = DateTime.Now.ToString("HHmm");

                    using (DB2Command cmd = MyDb2Connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = procName;
                        cmd.Parameters.Add("@TCASENO", DB2Type.VarChar, 8).Value = medCarInfoClass.住院號;
                        cmd.Parameters.Add("@TNURSTA", DB2Type.VarChar, 4).Value = medCarInfoClass.護理站;
                        cmd.Parameters.Add("@TTIME1", DB2Type.VarChar, 4).Value = "0000";
                        cmd.Parameters.Add("@TTIME2", DB2Type.VarChar, 4).Value = time;
                        DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
                        using (DB2DataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string 開始日期 = reader["UDBGNDT2"].ToString().Trim();
                                string 開始時間 = reader["UDBGNTM"].ToString().Trim();
                                string 日期時間 = $"{開始日期} {開始時間.Substring(0, 2)}:{開始時間.Substring(2, 2)}:00";
                                DateTime 開始日期時間 = DateTime.ParseExact(日期時間, "yyyy-MM-dd HH:mm:ss", null);
                                string 結束日期 = reader["UDENDDT2"].ToString().Trim();
                                string 結束時間 = reader["UDENDTM"].ToString().Trim();
                                日期時間 = $"{結束日期} {結束時間.Substring(0, 2)}:{結束時間.Substring(2, 2)}:00";
                                DateTime 結束日期時間 = DateTime.ParseExact(日期時間, "yyyy-MM-dd HH:mm:ss", null);
                                medCpoeClass medCpoeClass = new medCpoeClass
                                {
                                    藥局 = medCarInfoClass.藥局,
                                    護理站 = medCarInfoClass.護理站,
                                    床號 = medCarInfoClass.床號,
                                    住院號 = reader["UDCASENO"].ToString().Trim(),
                                    序號 = reader["UDORDSEQ"].ToString().Trim(),
                                    狀態 = reader["UDSTATUS"].ToString().Trim(),
                                    開始時間 = 開始日期時間.ToDateTimeString_6(),
                                    結束時間 = 結束日期時間.ToDateTimeString_6(),
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
                        }
                    }
                }
                return prescription;
            }

        }

        //private List<medCpoeClass> ExcuteUDPDPHLP(List<string> CODE )
        //{
        //    using (DB2Connection MyDb2Connection = GetDB2Connection())
        //    {
        //        MyDb2Connection.Open();
        //        string procName = $"{DB2_schema}.UDPDPHLP";
        //        foreach (var code in CODE)
        //        {
        //            using (DB2Command cmd = MyDb2Connection.CreateCommand())
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.CommandText = procName;
        //                cmd.Parameters.Add("@UDDRGNO", DB2Type.VarChar, 5).Value = code;
        //                DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
        //                DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 60);
        //                using (DB2DataReader reader = cmd.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {

        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

    }   
}
