using ES;
using JobokoAdsModels;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobokoAdsES
{
    public class UserRepository : IESRepository
    {
        #region Init
        protected static string _DefaultIndex;
        //protected new ElasticClient client;
        private static UserRepository instance;
        public UserRepository(string modify_index)
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
        public static UserRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    _DefaultIndex = "jok_user";
                    instance = new UserRepository(_DefaultIndex);
                }
                return instance;
            }
        }
        public bool CreateIndex(bool delete_if_exist = false)
        {
            if (delete_if_exist && client.Indices.Exists(_DefaultIndex).Exists)
                client.Indices.Delete(_DefaultIndex);

            var createIndexResponse = client.Indices.Create(_DefaultIndex, s => s.Settings(st => st.NumberOfReplicas(0).NumberOfShards(3)).Map<User>(mm => mm.AutoMap().Dynamic(true)));
            return createIndexResponse.IsValid;
        }
        #endregion
        public IEnumerable<User> GetAll(string term, List<string> nguoi_tao, bool is_admin, string[] view_fields, Dictionary<string, bool> sort_fields, int page, int page_size, out long total_recs)
        {
            total_recs = 0;
            List<QueryContainer> must = new List<QueryContainer>();
            must.Add(new TermsQuery()
            {
                Field = "nguoi_tao",
                Terms = nguoi_tao
            });
            var lst = GetAll<User>(_DefaultIndex, term, new string[] { "user_name", "full_name" }, view_fields, sort_fields, page, page_size, out total_recs, must);
            return lst;
        }
        public IEnumerable<User> GetAll(List<string> nguoi_tao, string[] view_fields)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.Add(new TermsQuery()
            {
                Field = "nguoi_tao",
                Terms = nguoi_tao
            });
            var source = new SourceFilter()
            {
                Includes = view_fields
            };
            if (view_fields == null) source = new SourceFilter();
            return GetObjectScroll<User>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), source);
        }
        public string IndexRetId(User data)
        {
            var re_exist = client.DocumentExists<User>(data.user_name, g => g.SourceEnabled(false));

            if (!re_exist.Exists)
            {
                if (Index(_DefaultIndex, data, string.Empty, data.user_name, out string id))
                    return id;
                else
                    return string.Empty;
            }
            return string.Empty;
        }

        public bool Update(User data)
        {
            var re = client.Update<User>(data.id, u => u.Doc(data));
            return re.Result == Result.Updated || re.Result == Result.Noop;
        }

        public bool Delete(string id)
        {
            return DeleteById<User>(id);
        }

        public User GetById(string id, string[] view_fields)
        {
            var obj = GetById<User>(id, view_fields);
            if (obj != null)
            {
                obj.id = id;
                return obj;
            }
            return null;
        }
        public User Login(string username, string password)
        {
            var re = client.Get<User>(username);

            if (re.Found)
            {
                if (re.Source.password == password)
                {
                    re.Source.password = "";
                    return re.Source;
                }
            }
            return null;
        }

        public bool UserInfo(string id, string fullname)
        {
            var user = new User();
            user.id = $"{id}";

            var re_u = client.Search<User>(s => s.Query(q => q.Term(t => t.Field("user_name.keyword").Value(id))).Size(1));
            user = re_u.Hits.First().Source;
            user.id = re_u.Hits.First().Id;
            if (re_u.Total > 0)
            {
                var re = client.Update<User, object>(user.id, u => u.Doc(new { full_name = fullname }));
                return re.Result == Result.Updated || re.Result == Result.Noop;
            }

            return false;
        }
        public bool UserPass(string id, string password)
        {
            var user = new User();
            user.id = $"{id}";

            var re_u = client.Search<User>(s => s.Query(q => q.Term(t => t.Field("user_name.keyword").Value(id))).Size(1));
            user = re_u.Hits.First().Source;
            user.id = re_u.Hits.First().Id;
            if (re_u.Total > 0)
            {
                var re = client.Update<User, object>(user.id, u => u.Doc(new { password = XMedia.XUtil.Encode(password) }));
                return re.Result == Result.Updated || re.Result == Result.Noop;
            }

            return false;
        }

        public bool UpdateNganSach(string id, double ngan_sach)
        {
            var re = client.Update<User, object>(id, u => u.Doc(new { ngan_sach = ngan_sach }));
            return re.Result == Result.Updated;
        }
    }
}
