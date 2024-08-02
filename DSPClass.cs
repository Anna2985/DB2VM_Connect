using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace DB2VM_API
{
    public class DSPClass
    {
        public string 藥品碼 { get; set; }
        public string 藥名 { get; set; }
        public string 操作者姓名 { get; set; }
        public string 效期 { get; set; }
        public string 住院序號 { get; set; }
    }
}
