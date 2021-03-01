using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobokoAdsModels
{
    [ElasticsearchType(IdProperty = nameof(id))]

    public class BaseModel
    {
        [Keyword]
        public string id { get; set; }
        public long ngay_tao { get; set; }
        [Keyword]
        public string nguoi_tao { get; set; }
        public long ngay_sua { get; set; }
        [Keyword]
        public string nguoi_sua { get; set; }
    }
}
