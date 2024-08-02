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
using SQLUI;
using HIS_DB_Lib;
namespace DB2VM
{
    [Route("api/[controller]")]
    [ApiController]
    public class OPDBBAR : ControllerBase
    {
        static string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";

        //string DB2_server = $"10.30.253.248:51011";
        string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
        //string DB2_userid = $"XVGF3";
        //string DB2_password = $"QWER1234";
        static string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
        static string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
        string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";
        //string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";

        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";

        private SQLControl sQLControl_醫囑資料 = new SQLControl(MySQL_server, MySQL_database, "order_list", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);


        [HttpGet]
        public string Get(string? BarCode)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            try
            {
                if (BarCode.StringIsEmpty())
                {
                    returnData.Code = -200;
                    returnData.Result = "Barcode空白";
                    return returnData.JsonSerializationt(true);
                }
                String MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
                DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);

                MyDb2Connection.Open();

                String procName = $"{DB2_schema}.UDSDBBAR";
                DB2Command cmd = MyDb2Connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procName;
                //cmd.Transaction = trans;
                cmd.Parameters.Add("@TSYSD", DB2Type.VarChar, 5).Value = "ADMUD";
                //cmd.Parameters.Add("@TUDDRGNO", DB2Type.VarChar, 5).Value = "";

                cmd.Parameters.Add("@TBARCOD", DB2Type.VarChar, 12).Value = BarCode;


                DB2Parameter TFLAG = cmd.Parameters.Add("@TFLAG", DB2Type.Char);
                TFLAG.Direction = ParameterDirection.Output;


                DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);


                DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 200);
                RETMSG.Direction = ParameterDirection.Output;
                //cmd.CommandTimeout = 3000;


                var reader = cmd.ExecuteReader();
                Console.Write($"TFLAG  : {TFLAG.Value}\n");
                Console.Write($"RET    : {RET.Value}\n");
                Console.Write($"RET    : {RETMSG.Value}\n");
                Console.Write($"進行Query...\n");
                List<object[]> obj_temp_array = new List<object[]>();
                List<object> obj_temp = new List<object>();
                Console.Write($"FieldCount : {reader.FieldCount} \n");
                List<OrderClass> orderClasses = new List<OrderClass>();

                List<string> colNames = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    colNames.Add(reader.GetName(i));
                }
                while (reader.Read())
                {
                    if (TFLAG.Value.ToString() == "O")
                    {
                        OrderClass orderClass = new OrderClass();
                        orderClass.藥局代碼 = "OPD";
                        orderClass.藥品碼 = reader["UDDRGNO"].ToString().Trim();
                        orderClass.領藥號 = reader["ARNSECT"].ToString().Trim();
                        orderClass.批序 = reader["ARSBDNO"].ToString().Trim();
                        orderClass.藥袋條碼 = BarCode;
                        orderClass.藥品名稱 = reader["ARNHNAMP"].ToString().Trim();
                        orderClass.病歷號 = reader["ARNHIST"].ToString().Trim();
                        orderClass.劑量單位 = reader["ARNHUNIT"].ToString().Trim();
                        orderClass.單次劑量 = reader["ARNHDOSE"].ToString().Trim();
                        orderClass.頻次 = reader["ARNHFQCY"].ToString().Trim();
                        orderClass.途徑 = reader["ARNHROUT"].ToString().Trim();
                        //orderClass. = reader["ARNHDUR"].ToString().Trim();
                        orderClass.交易量 = (reader["ARNHDQTY"].ToString().Trim().StringToInt32() * -1).ToString();
                        orderClass.PRI_KEY = $"{orderClass.藥袋條碼}-{orderClass.領藥號}";
                        orderClass.狀態 = "未過帳";
                        string Time = "00:00:00";
                        //if (Time.Length == 4)
                        //{
                        //    string time0 = Time.Substring(0, 2);
                        //    string time1 = Time.Substring(2, 2);
                        //    Time = $"{time0}:{time1}:00";
                        //}
                        orderClass.開方日期 = $"{reader["ARSBDATE"].ToString().Trim()}";
                        orderClass.開方日期 = $"{orderClass.開方日期.StringToDateTime().ToDateTimeString()}";
                        orderClasses.Add(orderClass);
                    }

                }

                MyDb2Connection.Close();
                if (orderClasses.Count == 0)
                {
                    if (BarCode.StringIsEmpty())
                    {
                        returnData.Code = -200;
                        returnData.Result = "找無藥單資料";
                        return returnData.JsonSerializationt(true);
                    }
                }

                List<object[]> list_醫囑資料 = this.sQLControl_醫囑資料.GetRowsByDefult(null, enum_醫囑資料.藥袋條碼.GetEnumName(), BarCode);
                if (list_醫囑資料.Count != orderClasses.Count)
                {
                    for (int i = 0; i < list_醫囑資料.Count; i++)
                    {
                        this.sQLControl_醫囑資料.DeleteByDefult(null, "GUID", list_醫囑資料[i][(int)enum_醫囑資料.GUID].ObjectToString());
                    }
                    for (int i = 0; i < orderClasses.Count; i++)
                    {
                        object[] value = new object[new enum_醫囑資料().GetLength()];
                        value[(int)enum_醫囑資料.GUID] = Guid.NewGuid().ToString();
                        value[(int)enum_醫囑資料.藥局代碼] = orderClasses[i].藥局代碼;
                        value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                        value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                        value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                        value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                        value[(int)enum_醫囑資料.PRI_KEY] = orderClasses[i].PRI_KEY;
                        value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                        value[(int)enum_醫囑資料.途徑] = orderClasses[i].途徑;
                        value[(int)enum_醫囑資料.頻次] = orderClasses[i].頻次;
                        value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                        value[(int)enum_醫囑資料.單次劑量] = orderClasses[i].單次劑量;
                        value[(int)enum_醫囑資料.產出時間] = DateTime.Now.ToDateTimeString_6();
                        value[(int)enum_醫囑資料.過帳時間] = DateTime.MinValue.ToDateTimeString_6();
                        value[(int)enum_醫囑資料.展藥時間] = DateTime.MinValue.ToDateTimeString_6();
                        value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
                        value[(int)enum_醫囑資料.狀態] = "未過帳";
                        orderClasses[i].狀態 = "未過帳";
                        this.sQLControl_醫囑資料.AddRow(null, value);
                    }

                }
                returnData.Code = 200;
                returnData.Data = orderClasses;
                returnData.Result = $"取得醫令成功,共<{orderClasses.Count}>筆";
                returnData.TimeTaken = myTimerBasic.ToString();
                string jsonString = returnData.JsonSerializationt();
                return jsonString;
            }
            catch (Exception e)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{e.Message}";
                return returnData.JsonSerializationt(true);
            }

        }
    }
}
