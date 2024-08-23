﻿using Microsoft.AspNetCore.Mvc;
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
        /// <summary>
        ///以藥局和護理站取得占床資料
        /// </summary>
        /// <remarks>
        /// 以下為JSON範例
        /// <code>
        ///     {
        ///         "ValueAry":[藥局, 護理站]
        ///     }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns></returns>
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
                List<string> valueAry = new List<string> { 藥局, 護理站 };
                List<medCarInfoClass> update_medCarInfoClass = medCarInfoClass.update_med_carinfo(API, bedList);
                List<medCarInfoClass> out_medCarInfoClass = medCarInfoClass.get_bed_list_by_cart(API, valueAry);
                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = out_medCarInfoClass;
                returnData.Result = $"取得{藥局} {護理站} 病床資訊共{update_medCarInfoClass.Count}/{bedList.Count}筆";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        /// <summary>
        ///以藥局和護理站和床號取得病人詳細資料
        /// </summary>
        /// <remarks>
        /// 以下為JSON範例
        /// <code>
        ///     {
        ///         "ValueAry":[藥局, 護理站, 床號]
        ///     }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns></returns>
        [HttpPost("get_patient_by_GUID")]
        public string get_patient_by_GUID([FromBody] returnData returnData)
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
                    returnData.Result = $"returnData.ValueAry 內容應為[GUID]";
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
                string GUID = returnData.ValueAry[0];
                List<medCarInfoClass> targetPatient = medCarInfoClass.get_patient_by_GUID(API, returnData.ValueAry);
                if (targetPatient.Count != 1)
                {
                    returnData.Code = -200;
                    returnData.Result = "無對應的病人資料";
                    return returnData.JsonSerializationt(true);
                }
                string 藥局 = targetPatient[0].藥局;
                string 護理站 = targetPatient[0].護理站;
                string 床號 = targetPatient[0].床號;
                //List<medCarInfoClass> bedList = ExecuteUDPDPPF1(藥局, 護理站);
                //medCarInfoClass targetPatient = bedList.FirstOrDefault(temp => temp.床號 == 床號);
                //List<medCarInfoClass> update = new List<medCarInfoClass> { targetPatient };

                List <medCarInfoClass> bedListInfo = ExecuteUDPDPPF0(targetPatient);
                List<medCpoeClass> bedListCpoe = ExecuteUDPDPDSP(bedListInfo);
                bedListInfo[0].處方 = bedListCpoe;

                List<string> valueAry = new List<string> { 藥局, 護理站 };
                List<string> valueAry2 = new List<string> { 藥局, 護理站, 床號 };

                List<medCarInfoClass> update_medCarInfoClass = medCarInfoClass.update_med_carinfo(API, bedListInfo);
                List<medCpoeClass> update_medCpoeClass = medCpoeClass.update_med_cpoe(API, bedListCpoe, valueAry);
                List<medCarInfoClass> out_medCarInfoClass = medCarInfoClass.get_patient_by_GUID(API, valueAry2);

                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = bedListInfo;
                returnData.Result = $"取得{藥局} {護理站} 第{床號}病床資訊";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        /// <summary>
        ///以護理站取得藥品總量
        /// </summary>
        /// <remarks>
        /// 以下為JSON範例
        /// <code>
        ///     {
        ///         "ValueAry":[藥局, 護理站]
        ///     }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns></returns>
        [HttpPost("get_med_qty")]
        public string get_med_qty([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                List<ServerSettingClass> serverSettingClasses = ServerSettingClassMethod.WebApiGet($"{API_Server}");
                serverSettingClasses = serverSettingClasses.MyFind("Main", "網頁", "VM端");
                if (serverSettingClasses.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.Result = $"找無Server資料";
                    return returnData.JsonSerializationt();
                }
                string Server = serverSettingClasses[0].Server;
                string DB = serverSettingClasses[0].DBName;
                string UserName = serverSettingClasses[0].User;
                string Password = serverSettingClasses[0].Password;
                uint Port = (uint)serverSettingClasses[0].Port.StringToInt32();

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
                string API = $"http://{Server}:4436";
                List<string> valueAry = new List<string> { 藥局, 護理站 };
                List<medCarInfoClass> bedList = ExecuteUDPDPPF1(藥局, 護理站);
                List<medCpoeClass> bedListCpoe = ExecuteUDPDPDSP(bedList);
                List<medCpoeClass> update_medCpoeClass = medCpoeClass.update_med_cpoe(API, bedListCpoe, valueAry);
                List<medQtyClass> get_med_qty = medCpoeClass.get_med_qty(API, valueAry);

                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = get_med_qty;
                returnData.Result = $"{藥局} {護理站} 的藥品清單";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = ex.Message;
                return returnData.JsonSerializationt(true);
            }
        }
        
        [HttpPost("get_medChange_by_GUID")]
        public string get_medChange_by_GUID([FromBody] returnData returnData)
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
                    returnData.Result = $"returnData.ValueAry 內容應為[GUID]";
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
                string GUID = returnData.ValueAry[0];
                List<string> valueAry = new List<string> { GUID };


                List<medCarInfoClass> bedList = medCarInfoClass.get_patient_by_GUID(API, valueAry);
                List<medCpoeRecClass> medCpoe_change = ExecuteUDPDPORD(bedList);
                List<medCpoeRecClass> update_medCpoe_change = medCpoeRecClass.update_med_CpoeRec(API,medCpoe_change);
                List<medCarInfoClass> get_patient = medCpoeRecClass.get_medChange_by_GUID(API, valueAry);




                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = get_patient;
                //returnData.Result = $"取得 {護理站} 病床資訊共{update_medCarInfoClass.Count}/{bedListInfo.Count}筆";
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
        [HttpPost("get_all_by_cart")]
        public string get_all_by_cart([FromBody] returnData returnData)
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
                List<medCarInfoClass> update_medCarInfoClass = medCarInfoClass.update_med_carinfo(API, bedListInfo);
                List<medCpoeClass> bedListCpoe = ExecuteUDPDPDSP(update_medCarInfoClass);
                List<string> valueAry = new List<string> { 藥局, 護理站 };
                List<medCpoeClass> update_medCpoeClass = medCpoeClass.add_med_cpoe(API, bedListCpoe);
                //List<medCarInfoClass> update_medCarInfoClass = medCarInfoClass.update_med_carinfo(API, bedList);

                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = bedListInfo;
                returnData.Result = $"取得 {護理站} 病床資訊共{bedList.Count}筆";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        [HttpPost("get_medChange_by_cart")]
        public string get_medChange_by_cart([FromBody] returnData returnData)
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
                List<string> valueAry = new List<string> { 藥局, 護理站 };
                //List<medCarInfoClass> bedList = ExecuteUDPDPPF1(藥局, 護理站);
                List<medCarInfoClass> bedList = medCarInfoClass.get_bed_list_by_cart(API, valueAry);
                List <medCpoeRecClass> medCpoe_change = ExecuteUDPDPORD(bedList);
                List<medCpoeRecClass> update_medCpoe_change = medCpoeRecClass.update_med_CpoeRec(API, medCpoe_change);
                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = update_medCpoe_change;
                //returnData.Result = $"取得 {護理站} 病床資訊共{update_medCarInfoClass.Count}/{bedListInfo.Count}筆";
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
            returnData.Result = $"取得病人資料";
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
            List<medCpoeRecClass> medCarInfoClasses = ExecuteUDPDPORD(medCarInfoClassList);
            returnData.Code = 200;
            returnData.TimeTaken = $"{myTimerBasic}";
            returnData.Data = medCarInfoClasses;
            returnData.Result = $"取得病床處方共{medCarInfoClasses.Count}筆";
            return returnData.JsonSerializationt(true);
        }
        [HttpPost("UDPDPORD")]
        public string Post_UDPDPORD([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();           
            List<medCarInfoClass> medCarInfoClassList = returnData.Data.ObjToClass<List<medCarInfoClass>>();        
            List<medCpoeRecClass> medCarInfoClasses = ExecuteUDPDPORD(medCarInfoClassList);
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
                                更新時間 = DateTime.Now.ToDateTimeString(),
                                護理站 = reader["HNURSTA"].ToString().Trim(),
                                床號 = reader["HBEDNO"].ToString().Trim(),
                                病歷號 = reader["HISTNUM"].ToString().Trim(),
                                住院號 = reader["PCASENO"].ToString().Trim(),
                                姓名 = reader["PNAMEC"].ToString().Trim()                              
                            };
                            if (!string.IsNullOrWhiteSpace(medCarInfoClass.姓名)) medCarInfoClass.占床狀態 = "已佔床";
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


                            diseaseClass diseaseClass = new diseaseClass();
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
                                    if (key == "PVSNAM") medCarInfoClass.主治醫師 = value;
                                    if (key == "PRNAM") medCarInfoClass.住院醫師 = value;
                                    if (key == "PBHIGHT") medCarInfoClass.身高 = value;
                                    if (key == "PBWEIGHT") medCarInfoClass.體重 = value;
                                    if (key == "PBBSA") medCarInfoClass.體表面積 = value;
                                    
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
                                    if (key == "PBIRTH8") medCarInfoClass.年齡 = age(value);

                                    if (key == "HICD1") diseaseClass.國際疾病分類代碼1 = value;
                                    if (key == "HICDTX1") diseaseClass.疾病說明1 = value;
                                    if (key == "HICD2") diseaseClass.國際疾病分類代碼2 = value;
                                    if (key == "HICDTX2") diseaseClass.疾病說明2 = value;
                                    if (key == "HICD3") diseaseClass.國際疾病分類代碼3 = value;
                                    if (key == "HICDTX3") diseaseClass.疾病說明3 = value;
                                    if (key == "HICD4") diseaseClass.國際疾病分類代碼4 = value;
                                    if (key == "HICDTX4") diseaseClass.疾病說明4 = value;
                                }
                            }
                            (string 疾病代碼, string 疾病說明) = disease(diseaseClass);
                            medCarInfoClass.疾病代碼 = 疾病代碼;
                            medCarInfoClass.疾病說明 = 疾病說明;
                            abnormal(medCarInfoClass);
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
                                    Master_GUID = medCarInfoClass.GUID,
                                    更新時間 = DateTime.Now.ToDateTimeString(),
                                    住院號 = reader["UDCASENO"].ToString().Trim(),
                                    序號 = reader["UDORDSEQ"].ToString().Trim(),
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
                                if (reader["UDSTATUS"].ToString().Trim() == "80") medCpoeClass.狀態 = "DC";
                                if (reader["UDSTATUS"].ToString().Trim() == "30") medCpoeClass.狀態 = "New";
                                if (medCpoeClass.藥局代碼 == "UB01") medCpoeClass.藥局名稱 = "中正樓總藥局";
                                if (medCpoeClass.藥局代碼 == "UB18") medCpoeClass.藥局名稱 = "中正樓十三樓藥局";
                                if (medCpoeClass.藥局代碼 == "UA05") medCpoeClass.藥局名稱 = "思源樓思源藥局";
                                if (medCpoeClass.藥局代碼 == "ERS1") medCpoeClass.藥局名稱 = "中正樓急診藥局";
                                if (medCpoeClass.藥局代碼 == "UBAA") medCpoeClass.藥局名稱 = "中正樓配方機藥局";
                                if (medCpoeClass.藥局代碼 == "UATP") medCpoeClass.藥局名稱 = "中正樓TPN藥局";
                                if (medCpoeClass.藥局代碼 == "EW01") medCpoeClass.藥局名稱 = "思源樓神經再生藥局";
                                if (medCpoeClass.藥局代碼 == "UBTP") medCpoeClass.藥局名稱 = "中正樓臨床試驗藥局";
                                if (medCpoeClass.藥局代碼 == "UC02") medCpoeClass.藥局名稱 = "長青樓藥局";
                                if (string.IsNullOrWhiteSpace(medCpoeClass.狀態)) medCpoeClass.狀態 = "New";



                                prescription.Add(medCpoeClass);
                            }
                        }
                    }
                }
                return prescription;
            }         
        }
        private List<medCpoeRecClass> ExecuteUDPDPORD(List<medCarInfoClass> medCarInfoClasses)
        {
            using (DB2Connection MyDb2Connection = GetDB2Connection())
            {
                MyDb2Connection.Open();
                string procName = $"{DB2_schema}.UDPDPORD";
                List<medCpoeRecClass> medCpoeRecClasses = new List<medCpoeRecClass>();
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
                        try
                        {
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
                                    medCpoeRecClass medCpoeRecClass = new medCpoeRecClass
                                    {
                                        GUID = Guid.NewGuid().ToString(),
                                        Master_GUID = medCarInfoClass.GUID,
                                        藥局 = medCarInfoClass.藥局,
                                        護理站 = medCarInfoClass.護理站,
                                        床號 = medCarInfoClass.床號,
                                        住院號 = reader["UDCASENO"].ToString().Trim(),
                                        序號 = reader["UDORDSEQ"].ToString().Trim(),
                                        開始時間 = 開始日期時間.ToDateTimeString_6(),
                                        結束時間 = 結束日期時間.ToDateTimeString_6(),
                                        藥碼 = reader["UDDRGNO"].ToString().Trim(),
                                        藥品名 = reader["UDDRGNAM"].ToString().Trim(),
                                        數量 = reader["UDLQNTY"].ToString().Trim(),
                                        劑量 = reader["UDDOSAGE"].ToString().Trim(),
                                        單位 = reader["UDDUNIT"].ToString().Trim(),                            
                                        處方醫師 = reader["UDORSIGN"].ToString().Trim(),
                                        處方醫師姓名 = reader["UDSIGNAM"].ToString().Trim(),
                                        操作人員 = reader["UDLUSER"].ToString().Trim(),
                                        藥局代碼 = reader["UDLRXID"].ToString().Trim(),
                                        大瓶點滴 = reader["UDCNT02"].ToString().Trim(),
                                   
                                    };
                                    if (reader["UDSTATUS"].ToString().Trim() == "80") medCpoeRecClass.狀態 = "DC";
                                    if (reader["UDSTATUS"].ToString().Trim() == "30") medCpoeRecClass.狀態 = "New";
                                    if (medCpoeRecClass.狀態 == "DC") medCpoeRecClass.更新時間 = medCpoeRecClass.結束時間;
                                    if (medCpoeRecClass.狀態 == "New") medCpoeRecClass.更新時間 = medCpoeRecClass.開始時間;
                                    if (string.IsNullOrWhiteSpace(medCpoeRecClass.狀態))
                                    {
                                        DateTime startday = medCpoeRecClass.開始時間.StringToDateTime().Date;
                                        DateTime endday = medCpoeRecClass.結束時間.StringToDateTime().Date;
                                        DateTime today = DateTime.Now.Date;
                                        if(startday == today)
                                        {
                                            medCpoeRecClass.狀態 = "New";
                                            medCpoeRecClass.更新時間 = medCpoeRecClass.開始時間;
                                        }
                                        if(endday == today)
                                        {
                                            medCpoeRecClass.狀態 = "DC";
                                            medCpoeRecClass.更新時間 = medCpoeRecClass.結束時間;
                                        }
                                    }
                                    medCpoeRecClasses.Add(medCpoeRecClass);
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"DB2Exception: {ex.Message}");                         
                        }
                        
                    }
                }
                return medCpoeRecClasses;
            }

        }
        private string age(string birthday)
        {
            int birthYear = birthday.Substring(0, 4).StringToInt32();
            int birthMon = birthday.Substring(4, 2).StringToInt32();
            int birthDay = birthday.Substring(6, 2).StringToInt32();

            DateTime today = DateTime.Now;
            int todayYear = today.Year;
            int todayMon = today.Month;
            int todayDay = today.Day;

            int ageYears = todayYear - birthYear;
            int ageMonths = todayMon - birthMon;

            if (ageMonths < 0 || (ageMonths == 0 && todayDay < birthDay))
            {
                ageYears--;
                ageMonths += 12;
            }

            if (todayDay < birthDay)
            {
                ageMonths--;
                if (ageMonths < 0)
                {
                    ageYears--;
                    ageMonths += 12;
                }
            }
            string ages = $"{ageYears}歲{ageMonths}月";

            return ages;
        }
        private (string dieaseCode, string dieaseName) disease(diseaseClass diseaseClass)
        {
            string dieaseCode = "";
            string dieaseName = "";

            if (!string.IsNullOrWhiteSpace(diseaseClass.國際疾病分類代碼1)) dieaseCode += diseaseClass.國際疾病分類代碼1;
            if (!string.IsNullOrWhiteSpace(diseaseClass.國際疾病分類代碼2)) dieaseCode += $";{diseaseClass.國際疾病分類代碼2}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.國際疾病分類代碼3)) dieaseCode += $";{diseaseClass.國際疾病分類代碼3}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.國際疾病分類代碼4)) dieaseCode += $";{diseaseClass.國際疾病分類代碼4}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.疾病說明1)) dieaseName += diseaseClass.疾病說明1;
            if (!string.IsNullOrWhiteSpace(diseaseClass.疾病說明2)) dieaseName += $";{diseaseClass.疾病說明2}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.疾病說明3)) dieaseName += $";{diseaseClass.疾病說明3}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.疾病說明4)) dieaseName += $";{diseaseClass.疾病說明4}";
            return (dieaseCode, dieaseName);
        }
        private medCarInfoClass abnormal(medCarInfoClass medCarInfoClasses)
        {
            List<string> abnormalList = new List<string>();
            double 白蛋白 = medCarInfoClasses.白蛋白.StringToDouble();
            double 肌酸酐 = medCarInfoClasses.肌酸酐.StringToDouble();
            double 估算腎小球過濾率 = medCarInfoClasses.估算腎小球過濾率.StringToDouble();
            double 丙氨酸氨基轉移酶 = medCarInfoClasses.丙氨酸氨基轉移酶.StringToDouble();
            double 鉀離子 = medCarInfoClasses.鉀離子.StringToDouble();
            double 鈣離子 = medCarInfoClasses.鈣離子.StringToDouble();
            double 總膽紅素 = medCarInfoClasses.總膽紅素.StringToDouble();
            double 鈉離子 = medCarInfoClasses.鈉離子.StringToDouble();
            double 白血球 = medCarInfoClasses.白血球.StringToDouble();
            double 血紅素 = medCarInfoClasses.血紅素.StringToDouble();
            double 血小板 = medCarInfoClasses.血小板.StringToDouble();
            double 國際標準化比率 = medCarInfoClasses.國際標準化比率.StringToDouble();


            if (白蛋白 < 3.7 || 白蛋白 > 5.3) abnormalList.Add("alb");
            if (肌酸酐 < 0.5 || 肌酸酐 > 0.9) abnormalList.Add("scr");
            if (估算腎小球過濾率 <= 60) abnormalList.Add("egfr");
            if (丙氨酸氨基轉移酶 < 33) abnormalList.Add("alt");
            if (鉀離子 <= 3.5 || 鉀離子 >= 5.1) abnormalList.Add("k");
            if (鈣離子 <= 8.6 || 鈣離子 >= 10.0) abnormalList.Add("ca");
            if (總膽紅素 < 1.2) abnormalList.Add("tb");
            if (鈉離子 <= 136 || 鈉離子 >= 145) abnormalList.Add("na");
            if (白血球 <= 4180 || 白血球 >= 9380) abnormalList.Add("wbc");
            if (血紅素 <= 10.9 || 血紅素 >= 15.6) abnormalList.Add("hgb");
            if (血小板 <= 145000.0 || 血小板 >= 383000) abnormalList.Add("plt");
            if (國際標準化比率 < 0.82 || 國際標準化比率 > 1.15) abnormalList.Add("inr");

            string[] abnormalArray = abnormalList.ToArray();
            string abnormal = string.Join(";", abnormalArray);
            medCarInfoClasses.檢驗數值異常 = abnormal;
            return medCarInfoClasses;
        }
    }  
}
