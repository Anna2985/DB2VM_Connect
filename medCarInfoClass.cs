using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Basic;
using System.ComponentModel;

namespace DB2VM_API
{
    [EnumDescription("med_carInfo")]
    public enum enum_病床資訊
    {
        [Description("GUID,VARCHAR,50,PRIMARY")]
        GUID,
        [Description("姓名,VARCHAR,30,NONE")]
        姓名,
        [Description("住院號,VARCHAR,50,INDEX")]
        住院號,
        [Description("病歷號,VARCHAR,50,NONE")]
        病歷號,
        [Description("藥局,VARCHAR,10,NONE")]
        藥局,
        [Description("護理站,VARCHAR,10,NONE")]
        護理站,
        [Description("床號,VARCHAR,10,NONE")]
        床號,
        [Description("占床狀態,VARCHAR,100,NONE")]
        占床狀態,
        [Description("調劑狀態,VARCHAR,100,NONE")]
        調劑狀態,
        [Description("性別,VARCHAR,100,NONE")]
        性別,
        [Description("出生日期,VARCHAR,100,NONE")]
        出生日期,
        [Description("科別,VARCHAR,100,NONE")]
        科別,
        [Description("財務,VARCHAR,100,NONE")]
        財務,
        [Description("入院日期,VARCHAR,100,NONE")]
        入院日期,
        [Description("訪視號碼,VARCHAR,100,NONE")]
        訪視號碼,
        [Description("診所名稱,VARCHAR,100,NONE")]
        診所名稱,
        [Description("醫生姓名,VARCHAR,100,NONE")]
        醫生姓名,
        [Description("身高,VARCHAR,100,NONE")]
        身高,
        [Description("體重,VARCHAR,100,NONE")]
        體重,
        [Description("體表面積,VARCHAR,100,NONE")]
        體表面積,
        [Description("國際疾病分類代碼1,VARCHAR,100,NONE")]
        國際疾病分類代碼1,
        [Description("疾病說明1,VARCHAR,100,NONE")]
        疾病說明1,
        [Description("國際疾病分類代碼2,VARCHAR,100,NONE")]
        國際疾病分類代碼2,
        [Description("疾病說明2,VARCHAR,100,NONE")]
        疾病說明2,
        [Description("國際疾病分類代碼3,VARCHAR,100,NONE")]
        國際疾病分類代碼3,
        [Description("疾病說明3,VARCHAR,100,NONE")]
        疾病說明3,
        [Description("國際疾病分類代碼4,VARCHAR,100,NONE")]
        國際疾病分類代碼4,
        [Description("疾病說明4,VARCHAR,100,NONE")]
        疾病說明4,
        [Description("鼻胃管使用狀況,VARCHAR,100,NONE")]
        鼻胃管使用狀況,
        [Description("其他管路使用狀況,VARCHAR,100,NONE")]
        其他管路使用狀況,
        [Description("過敏史,VARCHAR,100,NONE")]
        過敏史,
        [Description("檢驗結果,LONGTEXT,500,NONE")]
        檢驗結果,
        [Description("處方,LONGTEXT,10,NONE")]
        處方, 
        [Description("處方2,LONGTEXT,10,NONE")]
        處方2

        //[Description("白蛋白,VARCHAR,10,NONE")]
        //白蛋白,
        //[Description("肌酸酐,VARCHAR,10,NONE")]
        //肌酸酐,
        //[Description("估算腎小球過濾率,VARCHAR,10,NONE")]
        //估算腎小球過濾率,
        //[Description("丙氨酸氨基轉移酶,VARCHAR,10,NONE")]
        //丙氨酸氨基轉移酶,
        //[Description("鉀離子,VARCHAR,10,NONE")]
        //鉀離子,
        //[Description("鈣離子,VARCHAR,10,NONE")]
        //鈣離子,
        //[Description("總膽紅素,VARCHAR,10,NONE")]
        //總膽紅素,
        //[Description("鈉離子,VARCHAR,10,NONE")]
        //鈉離子,
        //[Description("白血球計數,VARCHAR,10,NONE")]
        //白血球計數,
        //[Description("血紅素,VARCHAR,10,NONE")]
        //血紅素,
        //[Description("血小板計數,VARCHAR,10,NONE")]
        //血小板計數,
        //[Description("國際標準化比率,VARCHAR,10,NONE")]
        //國際標準化比率

    }
    public class medCarInfoClass
    {
        [JsonPropertyName("GUID")]
        public string GUID { get; set; }
        [JsonPropertyName("pnamec")]
        public string 姓名 { get; set; }
        [JsonPropertyName("caseno")]
        public string 住院號 { get; set; }
        [JsonPropertyName("histno")]
        public string 病歷號 { get; set; }
        [JsonPropertyName("phar")]
        public string 藥局 { get; set; }
        [JsonPropertyName("hnursta")]
        public string 護理站 { get; set; }
        [JsonPropertyName("hbedno")]
        public string 床號 { get; set; }
        [JsonPropertyName("bed_status")]
        public string 占床狀態 { get; set; }
        [JsonPropertyName("dispens_status")]
        public string 調劑狀態 { get; set; }
        [JsonPropertyName("hsexc")]
        public string 性別 { get; set; }
        [JsonPropertyName("pbirth8")]
        public string 出生日期 { get; set; }
        [JsonPropertyName("psectc")]
        public string 科別 { get; set; }
        [JsonPropertyName("pfinc")]
        public string 財務 { get; set; }
        [JsonPropertyName("padmdt")]
        public string 入院日期 { get; set; }
        [JsonPropertyName("pvsdo")]
        public string 訪視號碼 { get; set; }
        [JsonPropertyName("pvsnam")]
        public string 診所名稱 { get; set; }
        [JsonPropertyName("prnam")]
        public string 醫生姓名 { get; set; }
        [JsonPropertyName("pbhight")]
        public string 身高 { get; set; }
        [JsonPropertyName("pbweight")]
        public string 體重 { get; set; }
        [JsonPropertyName("pbbsa")]
        public string 體表面積 { get; set; }
        [JsonPropertyName("hicd1")]
        public string 國際疾病分類代碼1 { get; set; }
        [JsonPropertyName("hicdtx1")]
        public string 疾病說明1 { get; set; }
        [JsonPropertyName("hicd2")]
        public string 國際疾病分類代碼2 { get; set; }
        [JsonPropertyName("hicdtx2")]
        public string 疾病說明2 { get; set; }
        [JsonPropertyName("hicd3")]
        public string 國際疾病分類代碼3 { get; set; }
        [JsonPropertyName("hicdtx3")]
        public string 疾病說明3 { get; set; }
        [JsonPropertyName("hicd4")]
        public string 國際疾病分類代碼4 { get; set; }
        [JsonPropertyName("hicdtx4")]
        public string 疾病說明4 { get; set; }
        [JsonPropertyName("ngtube")]
        public string 鼻胃管使用狀況 { get; set; }
        [JsonPropertyName("tube")]
        public string 其他管路使用狀況 { get; set; }
        [JsonPropertyName("hallergy")]
        public string 過敏史 { get; set; }
        [JsonPropertyName("testResult")]
        public object 檢驗結果 { get; set; }
        [JsonPropertyName("prescription")]
        public object 處方 { get; set; }
        [JsonPropertyName("prescription2")]
        public object 處方2 { get; set; }
        public class ICP_By_bedNum : IComparer<medCarInfoClass>
        {
            public int Compare(medCarInfoClass x, medCarInfoClass y)
            {
                return (x.床號.StringToInt32()).CompareTo(y.床號.StringToInt32());
            }
        }

    }

    public class testResult
    {
        [JsonPropertyName("rtalb")]
        public string 白蛋白 { get; set; }
        [JsonPropertyName("rtcrea")]
        public string 肌酸酐 { get; set; }
        [JsonPropertyName("rtegfrm")]
        public string 估算腎小球過濾率 { get; set; }
        [JsonPropertyName("rtalt")]
        public string 丙氨酸氨基轉移酶 { get; set; }
        [JsonPropertyName("rtk")]
        public string 鉀離子 { get; set; }
        [JsonPropertyName("rtca")]
        public string 鈣離子 { get; set; }
        [JsonPropertyName("rttb")]
        public string 總膽紅素 { get; set; }
        [JsonPropertyName("rtna")]
        public string 鈉離子 { get; set; }
        [JsonPropertyName("rtwbc")]
        public string 白血球計數 { get; set; }
        [JsonPropertyName("rthgb")]
        public string 血紅素 { get; set; }
        [JsonPropertyName("rtptl")]
        public string 血小板計數 { get; set; }
        [JsonPropertyName("rtinr")]
        public string 國際標準化比率 { get; set; }
    }
    public class DataList<T>
    {
        public List<T> Data { get; set; }
        public List<string> ValueAry { get; set; }
        public DataList()
        {
            Data = new List<T>();
            ValueAry = new List<string>();
        }
    }
}
