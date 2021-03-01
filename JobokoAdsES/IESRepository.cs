using Elasticsearch.Net;
using IdGen;
using Nest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ES
{
    public class IESRepository
    {
        //public static string _DefaultIndex = "";
        protected static Uri node = new Uri(XMedia.XUtil.ConfigurationManager.AppSetting["ES:Server"]);
        protected IdGenerator id_gen = new IdGenerator(0);
        protected ElasticClient client;
        protected static string formatDatePattern = "yyyy-MM-ddTHH:mm";
        protected static string formatDatePatternEndDay = "yyyy-MM-ddT23:59:59";
        protected static string formatDatePatternStartDay = "yyyy-MM-ddT00:00:00";
        protected static StickyConnectionPool connectionPool = new StickyConnectionPool(new[] { node });
        protected static double maxPriceValue = 3097976931348623;
        protected static uint cacheDate = 43200;
        protected static DateTime dateTime3000 = new DateTime(3000, 1, 1);
        protected static DateMath dateMathNow = DateMath.FromString(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
        protected static DateMath dateMathEndDay = DateMath.FromString(DateTime.Now.ToString("yyyy-MM-ddT23:59:59"));
        protected static string NULL_VALUE = "_null_";
        
        public string EscapeTerm(string term)
        {
            string[] rmChars = new string[] { "-", "\"", "+", "=", "&&", "||", ">", "<", "!", "(", ")", "{", "}", "[", "]", "^", "~", ":", "\\", "/" };
            if (term.Count(p => p == '"') % 2 == 0)
            {
                rmChars = new string[] { "-", "+", "=", "&&", "||", ">", "<", "!", "(", ")", "{", "}", "[", "]", "^", "~", ":", "\\", "/" };
            }
            foreach (string item in rmChars)
            {
                term = term.Replace(item, " ");
            }

            return term;
        }

        public string RemoveCharNotAllow(string term)
        {
            string[] rmChars = new string[] { "-", "\"", "+", "=", "&&", "||", ">", "<", "!", "(", ")", "{", "}", "[", "]", "^", "~", ":", "\\", "/", ",", "?", "@", "#", "$", "%", "*", "." };

            foreach (string item in rmChars)
            {
                term = term.Replace(item, " ");
            }

            return term;
        }

        public bool ValidateQuery(string term)
        {
            var vali = new ValidateQueryRequest();
            vali.Query = new QueryContainer(new QueryStringQuery() { Query = term });
            return client.Indices.ValidateQuery(vali).Valid;
        }

        public int GetHashCode(string s)
        {
            int n = s.Length;
            int h = 0;
            for (int i = 0; i < n; i++)
            {
                h += s[i];
                h *= 123123123;
            }
            return h;
        }

        //public ConcurrentBag<T> GetObjectScroll<T>(QueryContainer qc, SourceFilter so, List<ISort> lSort = null) where T : class
        //{
        //    return GetObjectScroll<T>(_DefaultIndex, qc, so, lSort);
        //}
        protected ConcurrentBag<T> GetObjectScroll<T>(string _DefaultIndex, QueryContainer query, SourceFilter so, List<ISort> lSort = null, string routing = "", int scroll_pageize = 2000) where T : class
        {
            try
            {
                string scroll_timeout = "5m";
                //int scroll_pageize = 2000;

                var seenSlices = new ConcurrentBag<int>();
                ConcurrentBag<T> bag = new ConcurrentBag<T>();

                if (query == null)
                    query = new MatchAllQuery();
                if (so == null)
                    so = new SourceFilter() { };
                try
                {
                    var searchResponse = client.Search<T>(sd => sd.Source(s => so).Index(_DefaultIndex).From(0).Take(scroll_pageize).Query(q => query).Scroll(scroll_timeout));

                    while (true)
                    {
                        if (!searchResponse.IsValid || string.IsNullOrEmpty(searchResponse.ScrollId))
                            break;

                        if (!searchResponse.Documents.Any())
                            break;

                        var tmp = searchResponse.Hits;
                        Parallel.ForEach(tmp, new ParallelOptions { MaxDegreeOfParallelism = 4 }, item =>
                        {
                            var doc = HitToDocument(item);
                            bag.Add(doc);
                        });
                        /*
                        foreach (var item in tmp)
                        {
                            var doc = HitToDocument(item);
                            bag.Add(doc);
                        }
                        */
                        searchResponse = client.Scroll<T>(scroll_timeout, searchResponse.ScrollId);
                    }

                    client.ClearScroll(new ClearScrollRequest(searchResponse.ScrollId));
                }
                catch (Exception)
                {
                }
                finally
                {
                }
                return bag;
            }
            catch
            {
            }
            return null;
        }

        protected ConcurrentDictionary<string, T> GetDicObjectScroll<T>(string _DefaultIndex, QueryContainer query, SourceFilter so) where T : class
        {
            try
            {
                ConcurrentDictionary<string, T> bag = new ConcurrentDictionary<string, T>();

                string scroll_timeout = "5m";
                int scroll_pageize = 2000;

                if (query == null)
                    query = new MatchAllQuery();
                if (so == null)
                    so = new SourceFilter() { };
                try
                {
                    var searchResponse = client.Search<T>(sd => sd.Source(s => so).Index(_DefaultIndex).From(0).Take(scroll_pageize).Query(q => query).Scroll(scroll_timeout));

                    while (true)
                    {
                        if (!searchResponse.IsValid || string.IsNullOrEmpty(searchResponse.ScrollId))
                            break;

                        if (!searchResponse.Documents.Any())
                            break;

                        var tmp = searchResponse.Hits;
                        foreach (var item in tmp)
                        {
                            bag.TryAdd(item.Id, item.Source);
                        }
                        searchResponse = client.Scroll<T>(scroll_timeout, searchResponse.ScrollId);
                    }

                    client.ClearScroll(new ClearScrollRequest(searchResponse.ScrollId));
                }
                catch (Exception)
                {
                }
                finally
                {
                }
                return bag;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        protected bool Index<T>(string _DefaultIndex, T data, string route, string id = "") where T : class
        {
            IndexRequest<object> req = new IndexRequest<object>(_DefaultIndex, typeof(T));
            if (!string.IsNullOrEmpty(route))
                req.Routing = route;
            req.Document = data;
            IndexResponse re = null;
            if (string.IsNullOrEmpty(id))
            {
                id = id_gen.CreateId().ToString();
            }
            if (!string.IsNullOrEmpty(id))
                re = client.Index<T>(data, i => i.Id(id));
            else
                re = client.Index(req);
            return re.Result == Result.Created;
        }

        protected bool Index<T>(string _DefaultIndex, T data, string route, out string id) where T : class
        {
            id = "";
            IndexRequest<T> req = new IndexRequest<T>(_DefaultIndex, new Nest.Id(id_gen.CreateId()));
            if (!string.IsNullOrEmpty(route))
                req.Routing = route;
            req.Document = data;
            IndexResponse re = null;

            re = client.Index(req);
            if (re.Result == Result.Created)
                id = re.Id;
            return re.Result == Result.Created;
        }

        protected bool Index<T>(string _DefaultIndex, T data, string route, string id, out string id_ret) where T : class
        {
            id_ret = "";
            IndexRequest<object> req = new IndexRequest<object>(_DefaultIndex, typeof(T));
            if (!string.IsNullOrEmpty(route))
                req.Routing = route;
            req.Document = data;
            IndexResponse re = null;
            if (string.IsNullOrEmpty(id))
            {
                id = id_gen.CreateId().ToString();
            }
            if (!string.IsNullOrEmpty(id))
                re = client.Index<T>(data, i => i.Id(id));
            else
                re = client.Index(req);
            if (re.Result == Result.Created)
                id_ret = re.Id;
            return re.Result == Result.Created;
        }

        protected bool Refresh(string _DefaultIndex)
        {
            var re = client.Indices.Refresh(_DefaultIndex, r => r.RequestConfiguration(c => c.RequestTimeout(TimeSpan.FromSeconds(5))));
            return re.IsValid;
        }

        public async Task<ConcurrentBag<IHit<T>>> BackupAndScrollAsync<T>(string _default_index, QueryContainer query, SourceFilter so, string scroll_timeout = "5m", int scroll_pageize = 2000) where T : class
        {
            if (query == null)
                query = new MatchAllQuery();
            if (so == null)
                so = new SourceFilter() { };
            ConcurrentBag<IHit<T>> bag = new ConcurrentBag<IHit<T>>();
            try
            {
                var searchResponse = await client.SearchAsync<T>(sd => sd.Source(s => so).Index(_default_index).From(0).Take(scroll_pageize).Query(q => query).Scroll(scroll_timeout));

                while (true)
                {
                    if (!searchResponse.IsValid || string.IsNullOrEmpty(searchResponse.ScrollId))
                        break;

                    if (!searchResponse.Documents.Any())
                        break;

                    var tmp = searchResponse.Hits;
                    foreach (var item in tmp)
                    {
                        bag.Add(item);
                    }
                    searchResponse = await client.ScrollAsync<T>(scroll_timeout, searchResponse.ScrollId);
                }

                await client.ClearScrollAsync(new ClearScrollRequest(searchResponse.ScrollId));
            }
            catch (Exception)
            {
            }
            finally
            {
            }
            return bag;
        }

        public ConcurrentBag<IHit<T>> BackupAndScroll<T>(string _default_index, QueryContainer query, SourceFilter so, string scroll_timeout = "5m", int scroll_pageize = 2000) where T : class
        {
            if (query == null)
                query = new MatchAllQuery();
            if (so == null)
                so = new SourceFilter() { };
            ConcurrentBag<IHit<T>> bag = new ConcurrentBag<IHit<T>>();
            try
            {
                var searchResponse = client.Search<T>(sd => sd.Source(s => so).Index(_default_index).From(0).Take(scroll_pageize).Query(q => query).Scroll(scroll_timeout));

                while (true)
                {
                    if (!searchResponse.IsValid || string.IsNullOrEmpty(searchResponse.ScrollId))
                        break;

                    if (!searchResponse.Documents.Any())
                        break;

                    var tmp = searchResponse.Hits;
                    foreach (var item in tmp)
                    {
                        bag.Add(item);
                    }
                    searchResponse = client.Scroll<T>(scroll_timeout, searchResponse.ScrollId);
                }

                client.ClearScroll(new ClearScrollRequest(searchResponse.ScrollId));
            }
            catch (Exception)
            {
            }
            finally
            {
            }
            return bag;
        }

        protected T HitToDocument<T>(IMultiGetHit<T> hit) where T : class, new()
        {
            if (hit.Found)
            {
                var doc = hit.Source;

                Type t = doc.GetType();
                PropertyInfo piShared = t.GetProperty("id");
                piShared.SetValue(doc, hit.Id);
                return doc;
            }
            return new T();
        }

        protected T HitToDocument<T>(IHit<T> hit) where T : class
        {
            var doc = hit.Source;

            Type t = doc.GetType();
            PropertyInfo piShared = t.GetProperty("id");
            if (piShared != null)
                piShared.SetValue(doc, hit.Id);
            return doc;
        }

        public T GetById<T>(string id, string[] view_fields = null) where T : class
        {
            GetResponse<T> re;
            if (view_fields == null || view_fields.Contains("*"))
            {
                re = client.Get<T>(id);
            }
            else
            {
                re = client.Get<T>(id, g => g.SourceIncludes(view_fields));
            }

            if (re.Found)
            {
                var doc = re.Source;

                Type t = doc.GetType();
                PropertyInfo piShared = t.GetProperty("id");
                if (piShared != null)
                    piShared.SetValue(doc, re.Id);
                return doc;
            }

            return null;
        }

        public bool DeleteById<T>(string id) where T : class
        {
            var re = client.Delete<T>(id);
            return re.Result == Result.Deleted;
        }

        protected IEnumerable<T> GetAll<T>(string _default_index, string term, string[] search_fields, string[] view_fields, Dictionary<string, bool> sort_fields, int page, int page_size, out long total_recs, List<QueryContainer> must_add = null) where T : class
        {
            total_recs = 0;

            List<QueryContainer> must = new List<QueryContainer>();
            if (must_add != null)
            {
                must.AddRange(must_add);
            }
            if (!string.IsNullOrEmpty(term))
            {
                must.Add(new QueryStringQuery()
                {
                    Fields = search_fields,
                    Query = term
                });
            }
            else if (must_add == null)
            {
                must.Add(new MatchAllQuery());
            }
            QueryContainer queryFilterMultikey = new QueryContainer(new BoolQuery()
            {
                Must = must
            });

            SourceFilter soF = new SourceFilter() { Includes = view_fields };
            if (view_fields.Contains("*") || view_fields == null)
            {
                soF = new SourceFilter();
            }

            List<ISort> sort = new List<ISort>() { };

            if (!string.IsNullOrEmpty(term))
                sort.Add(new FieldSort { Field = "_score", Order = SortOrder.Descending });
            else
            {
                foreach (var sf in sort_fields)
                {
                    sort.Add(new FieldSort { Field = sf.Key, Order = sf.Value ? SortOrder.Descending : SortOrder.Ascending });
                }
            }

            SearchRequest request = new SearchRequest(Indices.Index(_default_index))
            {
                TrackTotalHits = true,
                Query = queryFilterMultikey,
                Source = soF,
                Sort = sort,
                Size = page_size,
                From = (page - 1) * page_size,
            };
            var re = client.Search<T>(request);

            total_recs = re.Total;

            var lst = re.Hits.Select(HitToDocument);

            return lst;
        }

        


        protected IEnumerable<T> GetAllAggregation<T>(string _default_index, string term, string[] search_fields, string[] view_fields, Dictionary<string, bool> sort_fields, int page, int page_size, out long total_recs, out Dictionary<string, double> data_sum, List<QueryContainer> must_add = null) where T : class
        {
            total_recs = 0;

            List<QueryContainer> must = new List<QueryContainer>();
            if (must_add != null)
            {
                must.AddRange(must_add);
            }
            if (!string.IsNullOrEmpty(term))
            {
                must.Add(new QueryStringQuery()
                {
                    Fields = search_fields,
                    Query = term
                });
            }
            else if (must_add == null)
            {
                must.Add(new MatchAllQuery());
            }
            QueryContainer queryFilterMultikey = new QueryContainer(new BoolQuery()
            {
                Must = must
            });

            SourceFilter soF = new SourceFilter() { Includes = view_fields };
            if (view_fields.Contains("*") || view_fields == null)
            {
                soF = new SourceFilter();
            }

            List<ISort> sort = new List<ISort>() { };

            if (!string.IsNullOrEmpty(term))
                sort.Add(new FieldSort { Field = "_score", Order = SortOrder.Descending });
            else
            {
                foreach (var sf in sort_fields)
                {
                    sort.Add(new FieldSort { Field = sf.Key, Order = sf.Value ? SortOrder.Descending : SortOrder.Ascending });
                }
            }

            SearchRequest request = new SearchRequest(Indices.Index(_default_index))
            {
                TrackTotalHits = true,
                Query = queryFilterMultikey,
                Source = soF,
                Sort = sort,
                Size = page_size,
                From = (page - 1) * page_size,
                Aggregations = new AggregationDictionary()
                {
                    {
                        "chi_phi", new SumAggregation("cp", "chi_phi")
                    },
                    {
                        "luot_hien_thi", new SumAggregation("hien_thi", "luot_hien_thi")
                    },
                    {
                        "luot_click", new SumAggregation("click", "luot_click")
                    }
                }
            };
            var re = client.Search<T>(request);

            total_recs = re.Total;

            var lst = re.Hits.Select(x => HitToDocument(x));

            data_sum = new Dictionary<string, double>()
            {
                {"chi_phi", Convert.ToDouble(re.Aggregations.Sum("chi_phi")?.Value)},
                {"luot_hien_thi", Convert.ToDouble(re.Aggregations.Sum("luot_hien_thi")?.Value)},
                {"luot_click", Convert.ToDouble(re.Aggregations.Sum("luot_click")?.Value) }
            };

            return lst;
        }

        public bool IsOwner<T>(string id, string nguoi_tao) where T : class
        {
            try
            {
                var re = client.Get<T>(id, g => g.SourceIncludes(new string[] { "nguoi_tao" }));
                if (re.Found)
                {
                    var doc = re.Source;

                    Type t = doc.GetType();
                    PropertyInfo piShared = t.GetProperty("nguoi_tao");
                    if (piShared != null)
                    {
                        string owner = Convert.ToString(piShared.GetValue(doc));
                        return owner == nguoi_tao;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        public IEnumerable<T> GetMany<T>(string _DefaultIndex, IEnumerable<string> ids) where T : class
        {
            var re = client.GetMany<T>(ids, _DefaultIndex);
            return re.Where(x => x.Found).Select(x =>
            {
                var doc = x.Source;
                Type t = doc.GetType();
                PropertyInfo piShared = t.GetProperty("id");
                piShared.SetValue(doc, x.Id);
                return doc;
            });
        }

        public IEnumerable<T> GetMany<T>(string _DefaultIndex, IEnumerable<string> ids, string[] fields) where T : class
        {
            //var re = client.GetMany<QuangCao>(ids, _DefaultIndex);
            //return re;
            var m = new MultiGetRequest(_DefaultIndex);
            var lst_get = new List<MultiGetOperation<T>>();
            foreach (var item in ids)
            {
                lst_get.Add(new MultiGetOperation<T>(item));
            }
            m.Documents = lst_get;
            m.SourceIncludes = fields;
            var mre = client.MultiGet(m);
            var lst_quang_cao = new List<T>();
            foreach (var id in ids)
            {
                var quang_cao = mre.Source<T>(id);
                if (quang_cao != null)
                {
                    Type t = quang_cao.GetType();
                    PropertyInfo piShared = t.GetProperty("id");
                    piShared.SetValue(quang_cao, id);
                    lst_quang_cao.Add(quang_cao);
                }
            }
            return lst_quang_cao;
        }
    }
}