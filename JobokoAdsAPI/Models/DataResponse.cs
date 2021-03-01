using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobokoAdsAPI.Models
{
    public class DataResponsePaging : DataResponse
    {
        public long total { get; set; }
    }
    public class DataResponseSum
    {
        public object data_sum
        {
            get { return _data_sum; }
            set
            {
                if (value != null && value.GetType() == typeof(string))
                {
                    try
                    {
                        _data_sum = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(Convert.ToString(value));
                    }
                    catch (Exception)
                    {
                        _data_sum = value;
                    }
                }
                else
                {
                    var settings = new JsonSerializerSettings() { ContractResolver = new NullToEmptyStringResolver() };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);

                    _data_sum = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                }
            }
        }
        private object _data_sum;
    }
    public class DataResponse
    {
        private object _data;
        public bool success { get; set; }
        public object data
        {
            get { return _data; }
            set
            {
                if (value != null && value.GetType() == typeof(string))
                {
                    try
                    {
                        _data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(Convert.ToString(value));
                    }
                    catch (Exception)
                    {
                        _data = value;
                    }
                }
                else
                {
                    var settings = new JsonSerializerSettings() { ContractResolver = new NullToEmptyStringResolver() };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);

                    _data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                }
            }
        }
        public string msg { get; set; }
    }
    public class DataResponseExt : DataResponse
    {
        public object value { get; set; }
    }
}
