using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basic;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace DB2VM_API
{
    //病床處方
    //Computerized Physician Order Entry
    [EnumDescription("med_cpoe")]
    public enum enum_病床處方
    {
        [Description("調劑狀態,VARCHAR,10,NONE")]
        調劑狀態,
        [Description("住院號,VARCHAR,50,INDEX")]
        住院號,
        [Description("序號,VARCHAR,10,NONE")]
        序號,
        [Description("狀態,VARCHAR,10,NONE")]
        狀態,
        [Description("開始日期,VARCHAR,10,NONE")]
        開始日期,
        [Description("開始時間,VARCHAR,10,NONE")]
        開始時間,
        [Description("結束日期,VARCHAR,10,NONE")]
        結束日期,
        [Description("結束時間,VARCHAR,10,NONE")]
        結束時間,
        [Description("藥碼,VARCHAR,10,NONE")]
        藥碼,
        [Description("頻次代碼,VARCHAR,10,NONE")]
        頻次代碼,
        [Description("頻次屬性,VARCHAR,10,NONE")]
        頻次屬性,
        [Description("藥品名,VARCHAR,10,NONE")]
        藥品名,
        [Description("途徑,VARCHAR,10,NONE")]
        途徑,
        [Description("數量,VARCHAR,10,NONE")]
        數量,
        [Description("劑量,VARCHAR,10,NONE")]
        劑量,
        [Description("單位,VARCHAR,10,NONE")]
        單位,
        [Description("期限,VARCHAR,10,NONE")]
        期限,
        [Description("自動包藥機,VARCHAR,10,NONE")]
        自動包藥機,
        [Description("化癌分類,VARCHAR,10,NONE")]
        化癌分類,
        [Description("自購,VARCHAR,10,NONE")]
        自購,
        [Description("血液製劑註記,VARCHAR,10,NONE")]
        血液製劑註記,
        [Description("處方醫師,VARCHAR,10,NONE")]
        處方醫師,
        [Description("處方醫師姓名,VARCHAR,10,NONE")]
        處方醫師姓名,
        [Description("操作人員,VARCHAR,10,NONE")]
        操作人員,
        [Description("藥局代碼,VARCHAR,10,NONE")]
        藥局代碼,
        [Description("大瓶點滴,VARCHAR,10,NONE")]
        大瓶點滴,
        [Description("LKFLAG,VARCHAR,10,NONE")]
        LKFLAG,
        [Description("排序,VARCHAR,10,NONE")]
        排序,
        [Description("判讀藥師代碼,VARCHAR,10,NONE")]
        判讀藥師代碼,
        [Description("判讀FLAG,VARCHAR,10,NONE")]
        判讀FLAG,
        [Description("勿磨,VARCHAR,10,NONE")]
        勿磨,
        [Description("抗生素等級,VARCHAR,10,NONE")]
        抗生素等級,
        [Description("重複用藥,VARCHAR,10,NONE")]
        重複用藥,
        [Description("配藥天數,VARCHAR,10,NONE")]
        配藥天數,
        [Description("交互作用,VARCHAR,10,NONE")]
        交互作用,
        [Description("交互作用等級,VARCHAR,10,NONE")]
        交互作用等級
    }
    public class medCpoeClass
    {
        [JsonPropertyName("dispens_status")]
        public string 調劑狀態 { get; set; }
        [JsonPropertyName("caseno")]
        public string 住院號 { get; set; }
        [JsonPropertyName("ordseq")]
        public string 序號 { get; set; }
        [JsonPropertyName("status")]
        public string 狀態 { get; set; }
        [JsonPropertyName("bgndt2")]
        public string 開始日期 { get; set; }
        [JsonPropertyName("bgntm")]
        public string 開始時間 { get; set; }
        [JsonPropertyName("enddt2")]
        public string 結束日期 { get; set; }
        [JsonPropertyName("endtm")]
        public string 結束時間 { get; set; }
        [JsonPropertyName("code")]
        public string 藥碼 { get; set; }
        [JsonPropertyName("freqn")]
        public string 頻次代碼 { get; set; }
        [JsonPropertyName("frqatr")]
        public string 頻次屬性 { get; set; }
        [JsonPropertyName("name")]
        public string 藥品名 { get; set; }
        [JsonPropertyName("route")]
        public string 途徑 { get; set; }
        [JsonPropertyName("lqnty")]
        public string 數量 { get; set; }
        [JsonPropertyName("dosage")]
        public string 劑量 { get; set; }
        [JsonPropertyName("dunit")]
        public string 單位 { get; set; }
        [JsonPropertyName("durat")]
        public string 期限 { get; set; }
        [JsonPropertyName("dspmf")]
        public string 自動包藥機 { get; set; }
        [JsonPropertyName("chemo")]
        public string 化癌分類 { get; set; }
        [JsonPropertyName("self")]
        public string 自購 { get; set; }
        [JsonPropertyName("albumi")]
        public string 血液製劑註記 { get; set; }
        [JsonPropertyName("orsign")]
        public string 處方醫師 { get; set; }
        [JsonPropertyName("signam")]
        public string 處方醫師姓名 { get; set; }
        [JsonPropertyName("luser")]
        public string 操作人員 { get; set; }
        [JsonPropertyName("lrxid")]
        public string 藥局代碼 { get; set; }
        [JsonPropertyName("cnt02")]
        public string 大瓶點滴 { get; set; }
        [JsonPropertyName("brfnm")]
        public string LKFLAG { get; set; }
        [JsonPropertyName("rank")]
        public string 排序 { get; set; }
        [JsonPropertyName("pharnum")]
        public string 判讀藥師代碼 { get; set; }
        [JsonPropertyName("flag")]
        public string 判讀FLAG { get; set; }
        [JsonPropertyName("udngt")]
        public string 勿磨 { get; set; }
        [JsonPropertyName("anticg")]
        public string 抗生素等級 { get; set; }
        [JsonPropertyName("samedg")]
        public string 重複用藥 { get; set; }
        [JsonPropertyName("dspdy")]
        public string 配藥天數 { get; set; }
        [JsonPropertyName("ddi")]
        public string 交互作用 { get; set; }
        [JsonPropertyName("ddic")]
        public string 交互作用等級 { get; set; }

    }

}
