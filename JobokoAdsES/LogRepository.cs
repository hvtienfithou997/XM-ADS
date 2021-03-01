using ES;
using JobokoAdsModels;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobokoAdsES
{
    public class LogRepository : IESRepository
    {
        #region Init

        protected static string _DefaultIndex;

        //protected new ElasticClient client;
        private static LogRepository instance;

        public LogRepository(string modify_index)
        {
            _DefaultIndex = !string.IsNullOrEmpty(modify_index) ? modify_index : _DefaultIndex;
            ConnectionSettings settings = new ConnectionSettings(connectionPool).DefaultIndex(_DefaultIndex).DisableDirectStreaming(true);
            settings.MaximumRetries(10);
            client = new ElasticClient(settings);
            var ping = client.Ping(p => p.Pretty(true));
            if (ping.ServerError != null && ping.ServerError.Error != null)
            {
                throw new Exception("START ES FIRST");
            }
        }

        public static LogRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    _DefaultIndex = "testlog";
                    instance = new LogRepository(_DefaultIndex);
                }
                return instance;
            }
        }

        public bool CreateIndex(bool delete_if_exist = false)
        {
            if (delete_if_exist && client.Indices.Exists(_DefaultIndex).Exists)
                client.Indices.Delete(_DefaultIndex);

            var createIndexResponse = client.Indices.Create(_DefaultIndex, s => s.Settings(st => st.NumberOfReplicas(0).NumberOfShards(3)).Map<Log>(mm => mm.AutoMap().Dynamic(true)));
            return createIndexResponse.IsValid;
        }

        #endregion Init

        public string GetMany(string ai, string term, long date_start, long date_end)
        {
            List<QueryContainer> must = new List<QueryContainer>
            {
                new TermQuery() {Field = "ai", Value = ai}
            };
            if (!string.IsNullOrEmpty(term))
            {
                term = string.Format("\"{0}\"", term);
                must.Add(new QueryStringQuery
                {
                    Fields = "prox.xkj",
                    Query = term
                });
            }

            if (date_start > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "d",
                    GreaterThanOrEqualTo = date_start
                });
            }

            if (date_end > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "d",
                    LessThanOrEqualTo = date_end
                });
            }
            var qc = new QueryContainer(
                new BoolQuery() { Must = must }
            );
            var res = client.Search<dynamic>(x => x.Query(o => o.Bool(n => n.Must(qc))));

            if (res.Total > 0)
            {
                return JsonConvert.SerializeObject(res.Documents.ToList());
            }
            return string.Empty;
        }

        public Dictionary<string, double> TinhTongLog(string ai, string term, long date_start, long date_end)
        {
            Dictionary<string, double> data_sum = new Dictionary<string, double>();
            List<QueryContainer> must = new List<QueryContainer>
            {
                new TermQuery() {Field = "ai", Value = ai}
            };
            if (!string.IsNullOrEmpty(term))
            {
                term = string.Format("\"{0}\"", term);
                must.Add(new QueryStringQuery
                {
                    Fields = "prox.xkj",
                    Query = term
                });
            }

            if (date_start > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "d",
                    GreaterThanOrEqualTo = date_start
                });
            }

            if (date_end > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "d",
                    LessThanOrEqualTo = date_end
                });
            }
            var qc = new QueryContainer(
                new BoolQuery() { Must = must }
            );

            var re = client.Search<dynamic>(x => x.Aggregations(o => o.Filter("filter", c => c.Filter(u => u.Bool(n => n.Must(qc)))
                .Aggregations(a => a.Sum("luot_click", avg => avg.Field("ext.c.c")).Sum("luot_show", sh => sh.Field("ext.s.c"))))).Size(0));
            if (re.IsValid)
            {
                var filterAgg = re.Aggregations.Filter("filter");
                if (filterAgg != null)
                {
                    data_sum = new Dictionary<string, double>
                    {
                        {"luot_click", Convert.ToDouble(filterAgg.Sum("luot_click")?.Value) },
                        {"luot_show", Convert.ToDouble(filterAgg.Sum("luot_show")?.Value) },
                    };
                }
            }
            return data_sum;
        }

        /// <summary>
        /// Tra cứu từ khóa tính tổng tiền, tiền trung bình trong 1 ngày, tổng click, view, trung bình click,view
        /// </summary>
        /// <param name="tu_khoa"></param>
        /// <param name="kieu_doi_sanh"></param>
        /// <param name="date_start"></param>
        /// <param name="date_end"></param>
        public Dictionary<string, double> TraCuuTuKhoa(string tu_khoa, KieuDoiSanh kieu_doi_sanh, long date_start, long date_end)
        {
            return TraCuuTuKhoaV2(tu_khoa, new List<string>() { "ext.s.c", "ext.c.c" }, kieu_doi_sanh, date_start, date_end);
        }

        public Dictionary<string, double> TraCuuTuKhoaV2(string tu_khoa, List<string> fields, KieuDoiSanh kieu_doi_sanh, long date_start, long date_end)
        {
            Dictionary<string, double> data_sum = new Dictionary<string, double>();

            List<QueryContainer> must = new List<QueryContainer>();

            if (!string.IsNullOrEmpty(tu_khoa))
            {
                switch (kieu_doi_sanh)
                {
                    case KieuDoiSanh.DOI_SANH_RONG:
                        must.Add(new QueryStringQuery
                        {
                            Fields = "prox.xkj",
                            Query = tu_khoa
                        });
                        break;

                    case KieuDoiSanh.KHOP_CHINH_XAC:
                        must.Add(new TermQuery
                        {
                            Field = "prox.xkj.keyword",
                            Value = tu_khoa
                        });
                        break;

                    case KieuDoiSanh.KHOP_CUM_TU:
                        tu_khoa = $"\"{tu_khoa}\"";
                        must.Add(new QueryStringQuery
                        {
                            Fields = "prox.xkj",
                            Query = tu_khoa
                        });
                        break;
                }
            }

            if (date_start > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "d",
                    GreaterThanOrEqualTo = date_start
                });
            }

            if (date_end > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "d",
                    LessThanOrEqualTo = date_end
                });
            }
            var qc = new QueryContainer(
                new BoolQuery() { Must = must }
            );

            Dictionary<string, AggregationContainer> aggs = new Dictionary<string, AggregationContainer>();

            foreach (var field in fields)
            {
                aggs.Add("agg_avg_" + field, new AggregationContainer
                {
                    Average = new AverageAggregation("avg_" + field, field)
                });
                aggs.Add("agg_sum_" + field, new AggregationContainer
                {
                    Sum = new SumAggregation("sum_" + field, field)
                });
            }
            var res_joboko = client.Search<dynamic>(x => x.Size(0)
                .Aggregations(o => o.Filter("f_joboko",
                    c => c.Filter(u => u.Bool(n => n.Must(qc)) &&
                                       u.Term(cc => cc.Field("ai")
                                           .Value("joboko")))
                        .Aggregations(aggs))
                )
            );
            var res_xmads = client.Search<dynamic>(x => x.Size(0)
                .Aggregations(o => o.Filter("f_xmads",
                    c => c.Filter(u => u.Bool(n => n.Must(qc)) &&
                                       u.Term(oo => oo.Field("ai")
                                           .Value("xmads")))
                        .Aggregations(aggs))));
            var filterAgg_joboko = res_joboko.Aggregations.Filter("f_joboko");
            var filterAgg_xmads = res_xmads.Aggregations.Filter("f_xmads");
            foreach (var field in fields)
            {
                data_sum.Add("sum_joboko_" + field, Convert.ToDouble(filterAgg_joboko.Sum("agg_sum_" + field)?.Value));
                data_sum.Add("avg_joboko_" + field, Convert.ToDouble(filterAgg_joboko.Average("agg_avg_" + field)?.Value));
                data_sum.Add("sum_xmads_" + field, Convert.ToDouble(filterAgg_xmads.Sum("agg_sum_" + field)?.Value));
                data_sum.Add("avg_xmads_" + field, Convert.ToDouble(filterAgg_xmads.Average("agg_avg_" + field)?.Value));
            }

            return data_sum;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="fields"></param>
        /// <param name="date_start"></param>
        /// <param name="date_end"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, Dictionary<string, double>>> TraCuuTuKhoa(List<TuKhoa> lst, List<string> fields, long date_start, long date_end)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, double>>> response = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
            Dictionary<string, double> data_sum = new Dictionary<string, double>();
            List<QueryContainer> must = new List<QueryContainer>();
            List<QueryContainer> should = new List<QueryContainer>();
            fields = new List<string> { "ext.c.c", "ext.s.c" };

            lst.Add(new TuKhoa
            {
                id_chien_dich = "804182626448441344",
                tu_khoa = "kế toán",
                kieu_doi_sanh = KieuDoiSanh.KHOP_CHINH_XAC
            });
            lst.Add(new TuKhoa
            {
                id_chien_dich = "804182626448441344",
                tu_khoa = "Nhân Viên",
                kieu_doi_sanh = KieuDoiSanh.KHOP_CHINH_XAC
            });

            lst.Add(new TuKhoa
            {
                id_chien_dich = "799212750336163840",
                tu_khoa = "nhân viên",
                kieu_doi_sanh = KieuDoiSanh.KHOP_CHINH_XAC
            });

            lst.Add(new TuKhoa
            {
                id_chien_dich = "803887768076812288",
                tu_khoa = "Nhân Viên Kinh Doanh",
                kieu_doi_sanh = KieuDoiSanh.KHOP_CHINH_XAC
            });

            foreach (var item in lst)
            {
                if (!string.IsNullOrEmpty(item.tu_khoa))
                {
                    switch (item.kieu_doi_sanh)
                    {
                        case KieuDoiSanh.DOI_SANH_RONG:
                            should.Add(new QueryStringQuery
                            {
                                Fields = "prox.xkj",
                                Query = item.tu_khoa
                            });
                            break;

                        case KieuDoiSanh.KHOP_CHINH_XAC:
                            should.Add(new TermQuery
                            {
                                Field = "prox.xkj.keyword",
                                Value = item.tu_khoa
                            });
                            break;

                        case KieuDoiSanh.KHOP_CUM_TU:
                            item.tu_khoa = $"\"{item.tu_khoa}\"";
                            should.Add(new QueryStringQuery
                            {
                                Fields = "prox.xkj",
                                Query = item.tu_khoa
                            });
                            break;
                    }
                }
            }

            date_start = 0;
            if (date_start > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "d",
                    GreaterThanOrEqualTo = date_start
                });
            }

            date_end = 0;
            if (date_end > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "d",
                    LessThanOrEqualTo = date_end
                });
            }
            var qc = new QueryContainer(
                new BoolQuery { Should = should, Must = must }
            );

            Dictionary<string, AggregationContainer> aggs = new Dictionary<string, AggregationContainer>();

            foreach (var field in fields)
            {
                aggs.Add("agg_avg_" + field, new AggregationContainer
                {
                    Average = new AverageAggregation("avg_" + field, field)
                });
                aggs.Add("agg_sum_" + field, new AggregationContainer
                {
                    Sum = new SumAggregation("sum_" + field, field)
                });
            }

            //var res = client.Search<dynamic>(x => x.Size(0).Aggregations(o => o.Filter("f_ads", c => c.Filter(u => u.Bool(n => n.Must(qc)) && u.Term(t => t.Field("ai").Value("xmads"))).Aggregations(aggs)) && o.Terms("states", st => st.Field("pro.cam").Aggregations(aggs))));
            //var res = client.Search<dynamic>(x => x.Size(0).Aggregations(o =>
            //    o.Terms("data", y => y.Field("ai")) && o.Filter("ads",
            //        w => w.Filter(f => f.Bool(m => m.Must(qc)) && f.Term("ai", "xmads")).Aggregations(aggs))));

            return response;
        }

        public Dictionary<string, object[]> TraCuuLog(string term, string site_id, long date_start, long date_end, int page, out long total_recs, int page_size = 50)
        {
            List<QueryContainer> must_key = new List<QueryContainer>();
            List<QueryContainer> must_ade = new List<QueryContainer>();
            List<QueryContainer> should_ade = new List<QueryContainer>();

            must_key.Add(new TermQuery()
            {
                Field = "pro.type",
                Value = "key"
            });
            must_ade.Add(new TermQuery()
            {
                Field = "pro.type",
                Value = "ade"
            });

            if (!string.IsNullOrEmpty(term))
            {
                must_key.Add(new QueryStringQuery
                {
                    Fields = "n",
                    Query = term
                });
            }

            if (!string.IsNullOrEmpty(site_id))
            {
                must_key.Add(new TermQuery()
                {
                    Field = "pro.dm",
                    Value = site_id
                });
            }
            if (date_start > 0)
            {
                must_key.Add(new LongRangeQuery()
                {
                    Field = "d",
                    GreaterThanOrEqualTo = date_start
                });
                must_ade.Add(new LongRangeQuery()
                {
                    Field = "d",
                    GreaterThanOrEqualTo = date_start
                });
            }

            if (date_end > 0)
            {
                must_key.Add(new LongRangeQuery()
                {
                    Field = "d",
                    LessThanOrEqualTo = date_end
                });
                must_ade.Add(new LongRangeQuery()
                {
                    Field = "d",
                    LessThanOrEqualTo = date_end
                });
            }
            var qc = new QueryContainer(
                new BoolQuery() { Must = must_key }
            );

            var re = client.Search<dynamic>(x => x.Query(q => qc).Sort(so => so.Descending("ext.sm.c").Descending("sum.co.s")).From((page - 1) * page_size).Size(page_size));
            total_recs = re.Total;
            var hs = new HashSet<string>();
            foreach (var item in re.Documents)
            {
                var n = Convert.ToString(((Dictionary<string, object>)item)["n"]).ToLower();

                if (hs.Add(n))
                {
                    should_ade.Add(new QueryStringQuery
                    {
                        Fields = "prox.xkj",
                        Query = string.Format("\"{0}\"", ((Dictionary<string, object>)item)["n"])
                    });
                }
            }
            var qc_ade = new QueryContainer(
                new BoolQuery() { Must = must_ade, Should = should_ade, MinimumShouldMatch = 1 }
            );
            var re_ade = client.Search<dynamic>(s => s.Query(q => qc_ade).Size(999999));

            var dic_tu_khoa_tim_kiem = new Dictionary<string, object[]>();
            foreach (var item in re.Documents)
            {
                JObject json_item = JObject.Parse(JsonConvert.SerializeObject(item));
                if (json_item != null)
                {
                    var name = json_item["n"].Value<string>().ToLower();
                    var count_request = json_item["ext"]["sm"]["c"].Value<long>();
                    var sum_display = json_item["sum"]["co"]["s"].Value<long>();
                    if (!dic_tu_khoa_tim_kiem.ContainsKey(name))
                        dic_tu_khoa_tim_kiem.Add(name, new object[3] { count_request, sum_display, 0 });
                }
            }

            foreach (var item in re_ade.Documents)
            {
                JObject json_item = JObject.Parse(JsonConvert.SerializeObject(item));
                if (json_item != null)
                {
                    var count = json_item["ext"]["s"]["c"].Value<long>();
                    var xkj = json_item["prox"]["xkj"].Values<string>().ToList();
                    foreach (var kj in xkj)
                    {
                        if (dic_tu_khoa_tim_kiem.ContainsKey(kj.ToLower()))
                        {
                            var ob = dic_tu_khoa_tim_kiem[kj.ToLower()];
                            ob[2] = count;
                        }
                    }
                }
            }
            return dic_tu_khoa_tim_kiem;
        }

        public Dictionary<string, double> SumTraCuuLog(string term, long date_start, long date_end)
        {
            Dictionary<string, double> data_sum = new Dictionary<string, double>();
            List<QueryContainer> must_key = new List<QueryContainer>();
            List<QueryContainer> must_ade = new List<QueryContainer>();

            must_key.Add(new TermQuery()
            {
                Field = "pro.type",
                Value = "key"
            });
            must_ade.Add(new TermQuery()
            {
                Field = "pro.type",
                Value = "ade"
            });

            if (!string.IsNullOrEmpty(term))
            {
                must_key.Add(new QueryStringQuery
                {
                    Fields = "n",
                    Query = term
                });

                must_ade.Add(new QueryStringQuery
                {
                    Fields = "prox.xkj",
                    Query = term
                });
            }

            if (date_start > 0)
            {
                must_key.Add(new LongRangeQuery()
                {
                    Field = "d",
                    GreaterThanOrEqualTo = date_start
                });
            }

            if (date_end > 0)
            {
                must_key.Add(new LongRangeQuery()
                {
                    Field = "d",
                    LessThanOrEqualTo = date_end
                });
            }
            var qc = new QueryContainer(
                new BoolQuery() { Must = must_key }
            );
            var qc_ade = new QueryContainer(
                new BoolQuery() { Must = must_ade }
            );

            var res_key = client.Search<dynamic>(x => x.Size(0).Aggregations(o => o.Filter("filter_by_key",
               c => c.Filter(u => u.Bool(n => n.Must(qc)))
                   .Aggregations(ch => ch
                   .Sum("tong_so_qc", t => t.Field("sum.co.s"))
                   .Sum("luot_goi_qc", l => l.Field("ext.sm.c"))))));

            var res_ade = client.Search<dynamic>(x => x.Size(0).Aggregations(o => o.Filter("filter_by_ade",
                   c => c.Filter(u => u.Bool(n => n.Must(qc_ade)))
                       .Aggregations(ch => ch
                       .Sum("ext.s.c", e => e.Field("ext.s.c"))))));

            // sum khi pro.type = ade, prox.word = "tu khoa"
            var tong_qc_hien_thi = res_ade.Aggregations.Filter("filter_by_ade").Sum("ext.s.c").Value;

            // sum khi pro.type = key, n = "tu_khoa"
            var total_qc = res_key.Aggregations.Filter("filter_by_key").Sum("tong_so_qc").Value;

            var sum_luot_goi_qc = res_key.Aggregations.Filter("filter_by_key").Sum("luot_goi_qc").Value;

            data_sum = new Dictionary<string, double>
                {
                    {"tong_qc_hien_thi", Convert.ToDouble(tong_qc_hien_thi)},
                    {"tong_qc", Convert.ToDouble(total_qc)},
                    {"luot_goi_qc", Convert.ToDouble(sum_luot_goi_qc)},
                };

            return data_sum;
        }

        public IEnumerable<dynamic> ChiTietTuKhoaTimKiem(string term, string site_id, long date_start, long date_end, int page, out long total_recs, int page_size = 50)
        {
            List<QueryContainer> must_key = new List<QueryContainer>();
            must_key.Add(new TermQuery()
            {
                Field = "pro.type",
                Value = "key"
            });
            if (!string.IsNullOrEmpty(term))
            {
                must_key.Add(new QueryStringQuery
                {
                    Fields = "n",
                    Query = term
                });
            }
            if (!string.IsNullOrEmpty(site_id))
            {
                must_key.Add(new TermQuery()
                {
                    Field = "pro.dm",
                    Value = site_id
                });
            }
            if (date_start > 0)
            {
                must_key.Add(new LongRangeQuery()
                {
                    Field = "d",
                    GreaterThanOrEqualTo = date_start
                });
            }

            if (date_end > 0)
            {
                must_key.Add(new LongRangeQuery()
                {
                    Field = "d",
                    LessThanOrEqualTo = date_end
                });
            }
            var qc = new QueryContainer(
                new BoolQuery() { Must = must_key }
            );
            var re = client.Search<dynamic>(x => x.Query(q => qc).Sort(so => so.Descending("ext.sm.c").Descending("sum.co.s")).From((page - 1) * page_size).Size(page_size));
            total_recs = re.Total;
            return re.Documents;
        }

        public IEnumerable<dynamic> TrangHienThiTuKhoaTimKiem(string term, string site_id, long date_start, long date_end, int page, out long total_recs, int page_size = 50)
        {
            List<QueryContainer> must_key = new List<QueryContainer>();
            must_key.Add(new TermQuery()
            {
                Field = "pro.type",
                Value = "rel"
            });
            if (!string.IsNullOrEmpty(term))
            {
                must_key.Add(new QueryStringQuery
                {
                    Fields = "prox.xkj",
                    Query = term
                });
            }
            if (!string.IsNullOrEmpty(site_id))
            {
                must_key.Add(new TermQuery()
                {
                    Field = "pro.dm",
                    Value = site_id
                });
            }
            if (date_start > 0)
            {
                must_key.Add(new LongRangeQuery()
                {
                    Field = "d",
                    GreaterThanOrEqualTo = date_start
                });
            }

            if (date_end > 0)
            {
                must_key.Add(new LongRangeQuery()
                {
                    Field = "d",
                    LessThanOrEqualTo = date_end
                });
            }
            var qc = new QueryContainer(
                new BoolQuery() { Must = must_key }
            );
            var re = client.Search<dynamic>(x => x.Query(q => qc).Sort(so => so.Descending("ext.sm.c").Descending("sum.co.s")).From((page - 1) * page_size).Size(page_size));
            total_recs = re.Total;
            return re.Documents;
        }
    }
}