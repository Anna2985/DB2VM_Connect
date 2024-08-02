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
    public class UDSDBBCMController : ControllerBase
    {
  
        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";

        private SQLControl sQLControl_藥檔資料 = new SQLControl(MySQL_server, MySQL_database, "medicine_page_cloud", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);


        string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";
        string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
        string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
        string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
        string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";
        [HttpGet]
        public string Get(string? Code)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();

            if (Code.StringIsEmpty()) Code = "";
            String MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
            DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);

            MyDb2Connection.Open();

            String procName = $"{DB2_schema}.UDSDBBCM";
            DB2Command cmd = new DB2Command();
            cmd.Connection = MyDb2Connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = procName;
            //cmd.Transaction = trans;
            cmd.Parameters.Add("@TSYSD", DB2Type.VarChar, 5).Value = "ADMUD";
            cmd.Parameters.Add("@TUDDRGNO", DB2Type.VarChar, 5).Value = $"{Code}";

            DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
            RET.Direction = ParameterDirection.Output;

            DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 200);
            RETMSG.Direction = ParameterDirection.Output;
            var reader = cmd.ExecuteReader();
            List<medClass> medClasses = new List<medClass>();
            while (reader.Read())
            {
                string 藥品條碼1 = "";
                string 藥品條碼2 = "";
                string 藥品條碼3 = "";
                string 藥品條碼4 = "";
                string 藥品條碼5 = "";
                medClass medClass = new medClass();
                medClass.藥品碼 = reader["UDDRGNO"].ToString().Trim();
                medClass.藥品名稱 = reader["UDARNAME"].ToString().Trim();
                medClass.料號 = reader["UDSTOKNO"].ToString().Trim();

                藥品條碼1 = reader["UDBARCD1"].ToString().Trim();
                藥品條碼2 = reader["UDBARCD2"].ToString().Trim();
                藥品條碼3 = reader["UDBARCD3"].ToString().Trim();
                藥品條碼4 = reader["UDBARCD4"].ToString().Trim();
                藥品條碼5 = reader["UDBARCD5"].ToString().Trim();
                if (藥品條碼1.StringIsEmpty() == false) medClass.Add_BarCode(藥品條碼1);
                if (藥品條碼2.StringIsEmpty() == false) medClass.Add_BarCode(藥品條碼2);
                if (藥品條碼3.StringIsEmpty() == false) medClass.Add_BarCode(藥品條碼3);
                if (藥品條碼4.StringIsEmpty() == false) medClass.Add_BarCode(藥品條碼4);
                if (藥品條碼5.StringIsEmpty() == false) medClass.Add_BarCode(藥品條碼5);
                medClasses.Add(medClass);
            }

            MyDb2Connection.Close();


            List<object[]> list_藥檔資料 = sQLControl_藥檔資料.GetAllRows(null);
            List<object[]> list_藥檔資料_buf = new List<object[]>();
            List<object[]> list_藥檔資料_add = new List<object[]>();
            List<object[]> list_藥檔資料_replace = new List<object[]>();
            for (int i = 0; i < medClasses.Count; i++)
            {
                list_藥檔資料_buf = list_藥檔資料.GetRows((int)enum_雲端藥檔.藥品碼, medClasses[i].藥品碼);
                if (list_藥檔資料_buf.Count == 0)
                {
                    object[] value = new object[new enum_雲端藥檔().GetLength()];
                    value[(int)enum_雲端藥檔.GUID] = Guid.NewGuid().ToString();
                    value[(int)enum_雲端藥檔.藥品碼] = medClasses[i].藥品碼;
                    value[(int)enum_雲端藥檔.料號] = medClasses[i].料號;
                    value[(int)enum_雲端藥檔.藥品名稱] = medClasses[i].藥品名稱;
                    //value[(int)enum_雲端藥檔.管制級別] = medClasses[i].管制級別;
                    if (medClasses[i].藥品條碼2 == null) medClasses[i].藥品條碼2 = new List<string>().JsonSerializationt();
                    value[(int)enum_雲端藥檔.藥品條碼2] = medClasses[i].藥品條碼2;
                    list_藥檔資料_add.Add(value);
                }
                else
                {
                    bool flag_replace = false;
                    object[] value = list_藥檔資料_buf[0];
                    if (medClasses[i].藥品條碼2 == null) medClasses[i].藥品條碼2 = new List<string>().JsonSerializationt();
                    if (value[(int)enum_雲端藥檔.藥品碼].ObjectToString() != medClasses[i].藥品碼) flag_replace = true;
                    if (value[(int)enum_雲端藥檔.料號].ObjectToString() != medClasses[i].料號) flag_replace = true;
                    if (value[(int)enum_雲端藥檔.藥品名稱].ObjectToString() != medClasses[i].藥品名稱) flag_replace = true;
                    if (value[(int)enum_雲端藥檔.藥品條碼2].ObjectToString() != medClasses[i].藥品條碼2) flag_replace = true;
                    value[(int)enum_雲端藥檔.藥品碼] = medClasses[i].藥品碼;
                    value[(int)enum_雲端藥檔.料號] = medClasses[i].料號;
                    value[(int)enum_雲端藥檔.藥品名稱] = medClasses[i].藥品名稱;
                    //value[(int)enum_雲端藥檔.管制級別] = medClasses[i].管制級別;
                    value[(int)enum_雲端藥檔.藥品條碼2] = medClasses[i].藥品條碼2;
                    if(flag_replace) list_藥檔資料_replace.Add(value);
                }
            }
            if (list_藥檔資料_add.Count > 0) sQLControl_藥檔資料.AddRows(null, list_藥檔資料_add);
            if(list_藥檔資料_replace.Count > 0) sQLControl_藥檔資料.UpdateByDefulteExtra(null, list_藥檔資料_replace);

            returnData returnData = new returnData();
            returnData.Code = 200;
            returnData.TimeTaken = myTimerBasic.ToString();
            returnData.Data = medClasses;
            returnData.Result = $"取得藥檔成功,新增<{list_藥檔資料_add.Count}>筆,修改<{list_藥檔資料_replace.Count}>筆";
            string jsonString = returnData.JsonSerializationt();
            return jsonString;
        }
    }
}
