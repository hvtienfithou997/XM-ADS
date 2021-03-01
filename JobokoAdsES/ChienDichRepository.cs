using ES;
using JobokoAdsModels;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobokoAdsES
{
    public class ChienDichRepository : IESRepository
    {
        #region Init

        protected static string _DefaultIndex;

        //protected new ElasticClient client;
        private static ChienDichRepository instance;

        public ChienDichRepository(string modify_index)
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

        public static ChienDichRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    _DefaultIndex = "jok_chiendich";
                    instance = new ChienDichRepository(_DefaultIndex);
                }
                return instance;
            }
        }

        public bool CreateIndex(bool delete_if_exist = false)
        {
            if (delete_if_exist && client.Indices.Exists(_DefaultIndex).Exists)
                client.Indices.Delete(_DefaultIndex);

            var createIndexResponse = client.Indices.Create(_DefaultIndex, s => s.Settings(st => st.NumberOfReplicas(0).NumberOfShards(1)).Map<ChienDich>(mm => mm.AutoMap().Dynamic(true)));
            return createIndexResponse.IsValid;
        }

        #endregion Init

        public IEnumerable<ChienDich> GetAll(string term, List<string> nguoi_tao, bool is_admin, string[] view_fields, Dictionary<string, bool> sort_fields, int trang_thai, string id_chien_dich, int page, int page_size, out long total_recs)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.Add(new TermsQuery()
            {
                Field = "nguoi_tao.keyword",
                Terms = nguoi_tao
            });

            if (!string.IsNullOrEmpty(id_chien_dich))
            {
                must.Add(new IdsQuery() { Values = new List<Id>() { new Id(id_chien_dich) } });
            }

            if (trang_thai > -1)
            {
                must.Add(new TermQuery()
                {
                    Field = "trang_thai",
                    Value = trang_thai
                });
            }

            var lst = GetAll<ChienDich>(_DefaultIndex, term, new string[] { "ten" }, view_fields, sort_fields, page, page_size, out total_recs, must);
            return lst;
        }

        public Dictionary<string, double> FilterAggregations(string term, string[] search_fields, int trang_thai, string id_chien_dich, List<string> nguoi_tao, bool is_admin)
        {
            Dictionary<string, double> data_sum = new Dictionary<string, double>();
            List<QueryContainer> must = new List<QueryContainer>
            {
                new TermsQuery() {Field = "nguoi_tao.keyword", Terms = nguoi_tao}
            };
            if (!string.IsNullOrEmpty(term))
            {
                must.Add(new QueryStringQuery
                {
                    Fields = search_fields,
                    Query = term
                });
            }
            if (!string.IsNullOrEmpty(id_chien_dich))
            {
                must.Add(new IdsQuery() { Values = new List<Id>() { new Id(id_chien_dich) } });
            }
            if (trang_thai > -1)
            {
                must.Add(new TermQuery()
                {
                    Field = "trang_thai",
                    Value = trang_thai
                });
            }

            var qc = new QueryContainer(
                new BoolQuery() { Must = must }
            );

            var res = client.Search<ChienDich>(x => x.Aggregations(o => o.Filter("filter",
                c => c.Filter(u => u.Bool(n => n.Must(qc)))
                    .Aggregations(ch =>
                        ch.Sum("chi_phi",
                        z => z.Field(ff => ff.chi_phi))
                        .Sum("luot_hien_thi",
                            ht => ht.Field(h => h.luot_hien_thi))
                        .Sum("luot_click",
                            lc => lc.Field(cl => cl.luot_click))
                        .Average("ty_le",
                            avg => avg.Field(a => a.ty_le_tuong_tac))
                        .Average("cpc",
                            cpc => cpc.Field(tb => tb.cpc_trung_binh)))))
                .Size(0));

            if (res.IsValid)
            {
                var filterAgg = res.Aggregations.Filter("filter");
                data_sum = new Dictionary<string, double>
                {
                    {"chi_phi", Convert.ToDouble(filterAgg.Sum("chi_phi")?.Value)},
                    {"luot_hien_thi", Convert.ToDouble(filterAgg.Sum("luot_hien_thi")?.Value)},
                    {"luot_click", Convert.ToDouble(filterAgg.Sum("luot_click")?.Value) },
                    {"ty_le", Convert.ToDouble(filterAgg.Average("ty_le")?.Value) },
                    //{"cpc", Convert.ToDouble(filterAgg.Average("cpc")?.Value) }
                    {"cpc", Math.Floor(Convert.ToDouble(filterAgg.Average("cpc")?.Value * 100) / 100)}
                };
            }

            return data_sum;
        }

        public List<ChienDich> Report(string tu_khoa, string quang_cao, long ngay_bat_dau, long ngay_ket_thuc,
            List<string> nguoi_tao, bool is_admin)
        {
            List<ChienDich> lst = new List<ChienDich>();
            List<QueryContainer> must = new List<QueryContainer>();
            List<QueryContainer> must_not = new List<QueryContainer>();
            List<string> lst_id = new List<string>();
            bool is_search = false;
            if (!string.IsNullOrEmpty(tu_khoa))
            {
                is_search = true;
                var all_tu_khoa = TuKhoaRepository.Instance.GetAll(tu_khoa, String.Empty, string.Empty, nguoi_tao,
                    is_admin, new[] { "*" }, null, -1, 1, 999, out long total_recs);
                var lst_chien_dich_tk = all_tu_khoa.Select(x => x.id_chien_dich).ToList();
                if (lst_chien_dich_tk.Count > 0)
                {
                    lst_id.AddRange(lst_chien_dich_tk);
                }
            }

            if (!string.IsNullOrEmpty(quang_cao))
            {
                is_search = true;
                var all_qc = QuangCaoRepository.Instance.GetAll(quang_cao, nguoi_tao, is_admin, new[] { "*" }, null, -1,
                    string.Empty, string.Empty, 1, 999, out long total_recs);
                var lst_chien_dich_qc = all_qc.Select(x => x.id_chien_dich).ToList();
                if (lst_chien_dich_qc.Count > 0)
                {
                    lst_id.AddRange(lst_chien_dich_qc);
                }
            }

            if (is_search && !lst_id.Any())
                return new List<ChienDich>();

            if (ngay_bat_dau > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "ngay_bat_dau",
                    GreaterThanOrEqualTo = ngay_bat_dau
                });
            }

            if (ngay_ket_thuc > 0)
            {
                must.Add(new LongRangeQuery()
                {
                    Field = "ngay_ket_thuc",
                    LessThanOrEqualTo = ngay_ket_thuc
                });
            }

            lst_id = lst_id.Distinct().ToList();
            if (lst_id.Count > 0)
            {
                must.Add(new IdsQuery()
                {
                    Values = lst_id.Select(x => (Id)x)
                });
            }

            must.Add(new TermsQuery()
            {
                Field = "nguoi_tao.keyword",
                Terms = nguoi_tao
            });
            var qc = new QueryContainer(
                new BoolQuery() { Must = must }
            );
            var res = client.Search<ChienDich>(x => x.Query(o => o.Bool(c => c.Must(qc))));

            if (res.Total > 0)
            {
                lst = res.Documents.ToList();
            }
            return lst;
        }

        public IEnumerable<ChienDich> GetAll(List<string> nguoi_tao, string[] view_fields)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.Add(new TermsQuery()
            {
                Field = "nguoi_tao.keyword",
                Terms = nguoi_tao
            });
            var source = new SourceFilter()
            {
                Includes = view_fields
            };
            if (view_fields == null) source = new SourceFilter();
            return GetObjectScroll<ChienDich>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), source);
        }

        public IEnumerable<string> GetAllTrangThaiBat()
        {
            List<QueryContainer> must = new List<QueryContainer>();

            var source = new SourceFilter()
            {
                Includes = new string[] {"id"}
            };

            return GetObjectScroll<ChienDich>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), source).Select(x => x.id).ToList();
        }

        public string IndexRetId(ChienDich data)

        {
            if (Index(_DefaultIndex, data, string.Empty, out string id))
                return id;
            else
                return string.Empty;
        }

        public bool Update(ChienDich data)
        {
            if (IsOwner<ChienDich>(data.id, data.nguoi_sua))
            {
                var re = client.Update<ChienDich>(data.id, u => u.Doc(data));
                return re.Result == Result.Updated || re.Result == Result.Noop;
            }
            return false;
        }

        public bool Delete(string id)
        {
            return DeleteById<ChienDich>(id);
        }

        public ChienDich GetById(string id, string[] view_fields)
        {
            var obj = GetById<ChienDich>(id, view_fields);
            if (obj != null)
            {
                obj.id = id;
                return obj;
            }
            return null;
        }

        public IEnumerable<ChienDich> GetMany(IEnumerable<string> ids)
        {
            var re = GetMany<ChienDich>(_DefaultIndex, ids);
            return re;
        }

        public IEnumerable<ChienDich> GetMany(IEnumerable<string> ids, string[] fields)
        {
            var re = GetMany<ChienDich>(_DefaultIndex, ids, fields);
            return re;
        }

        /// <summary>
        /// Hàm cộng dồn chi phí của 1 ngày, thay đổi tỷ lệ tương tác, cpc...
        /// </summary>
        /// <param name="id"></param>
        /// <param name="chi_phi_ngay"></param>
        /// <returns></returns>
        public bool UpdateChiPhiLuotClickCongDonChiPhiNgay(string id, long luot_click, double tong_chi_phi, double chi_phi_ngay)
        {
            long ngay_hien_tai = XMedia.XUtil.TimeInEpoch(DateTime.Now.Date);
            StringBuilder stb = new StringBuilder();

            //1 Nếu ngày tính toán trùng ngày hiện tại thì cộng dồn chi phí
            stb.Append("if(ctx._source.ngay_hien_tai==params.ngay_hien_tai){if(ctx._source.chi_phi_ngay==null){ctx._source.chi_phi_ngay=params.chi_phi_ngay}else{ctx._source.chi_phi_ngay+=params.chi_phi_ngay}}");
            //2 Nếu ngày tính toán khác ngày hiện tại thì reset ngày tính lại rồi mới cộng
            stb.Append("else{ctx._source.ngay_hien_tai=params.ngay_hien_tai;ctx._source.chi_phi_ngay=params.chi_phi_ngay}");
            stb.Append("ctx._source.luot_click=params.luot_click; ctx._source.chi_phi=params.tong_chi_phi;");
            stb.Append("ctx._source.ty_le_tuong_tac=(ctx._source.luot_hien_thi > 0 ? (float)params.luot_click*100/ctx._source.luot_hien_thi:0); ctx._source.cpc_trung_binh=(double)params.tong_chi_phi/params.luot_click");
            var re = client.Update<ChienDich, object>(id, u => u.Script(s =>
            s.Source(stb.ToString()).Params(new Dictionary<string, object>() {
                { "chi_phi_ngay",chi_phi_ngay},{ "ngay_hien_tai",ngay_hien_tai},
                { "tong_chi_phi",tong_chi_phi},{ "luot_click",luot_click}
            })));
            return re.Result == Result.Updated;
        }

        public bool UpdateTrangThai(string id, int trang_thai)
        {
            var re = client.Update<ChienDich, object>(id, u => u.Doc(new { trang_thai = trang_thai }));
            return re.Result == Result.Updated;
        }

        public bool UpdateNgayChay(string id, long ngay_chay)
        {
            var re = client.Update<ChienDich, object>(id, u => u.Doc(new { ngay_chay = ngay_chay }));
            return re.Result == Result.Updated;
        }

        public bool UpdateDiaDiem(string id, List<string> dia_diem_muc_tieu, List<string> dia_diem_loai_tru)
        {
            var re = client.Update<ChienDich, object>(id, u => u.Doc(new { dia_diem_muc_tieu = dia_diem_muc_tieu, dia_diem_loai_tru = dia_diem_loai_tru }));
            return re.Result == Result.Updated;
        }

        public IEnumerable<ChienDich> Overview(List<string> nguoi_tao, string[] view_fields, Dictionary<string, bool> sort_fields, int top = 10)
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
            QueryContainer queryFilterMultikey = new QueryContainer(new BoolQuery()
            {
                Must = must
            });

            SourceFilter soF = new SourceFilter() { Includes = view_fields };

            List<ISort> sort = new List<ISort>() { };

            foreach (var item in sort_fields)
            {
                sort.Add(new FieldSort() { Field = item.Key, Order = item.Value ? SortOrder.Descending : SortOrder.Ascending });
            }

            SearchRequest request = new SearchRequest(Indices.Index(_DefaultIndex))
            {
                TrackTotalHits = true,
                Query = queryFilterMultikey,
                Source = soF,
                Sort = sort,
                Size = top
            };
            var re = client.Search<ChienDich>(request);
            return re.Hits.Select(x => HitToDocument<ChienDich>(x));
        }

        /// <summary>
        /// Hàm tăng lượt hiển thị và thay đổi chỉ số tỷ lệ tương tác
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool IncreaseView(string id, int count = 1)
        {
            StringBuilder stb = new StringBuilder();
            stb.Append("if(ctx._source.luot_hien_thi==null){ctx._source.luot_hien_thi=params.count}else{ctx._source.luot_hien_thi+=params.count}");
            stb.Append("ctx._source.ty_le_tuong_tac=(float)ctx._source.luot_click*100/(ctx._source.luot_hien_thi+params.count)");

            var re = client.Update<ChienDich, object>(id, u =>
                u.Script(s => s.Source(stb.ToString()).Params(new Dictionary<string, object>() {
                    { "count",count }
                })));
            return re.Result == Result.Updated;
        }

        //public void TinhToanTongNganSachCacChienDich(string nguoi_tao, double percent = 0.9)
        //{
        //Tính tổng chi_phi các chiến dịch của người dùng này. Nếu tổng chi_phi chiến dịch >= percent*ngan_sach người dùng thì dừng
        //tất cả các chiến dịch đang chạy của người dùng này (chuyển sang trạng thái DUNG_DO_HET_NGAN_SACH)
        //    var user_ngan_sach = UserRepository.Instance.GetById(nguoi_tao, new[] { "ngan_sach" })?.ngan_sach;

        //    var chien_dich = GetAll(new List<string> { nguoi_tao }, new[] { "id", "chi_phi" }).ToList();

        //    var tong_chi_phi = client.Search<ChienDich>(s =>
        //        s.Query(x => x.Term(o => o.Field("nguoi_tao").Value(nguoi_tao)))
        //            .Aggregations(o => o.Sum("chi_phi", tt => tt.Field(c => c.chi_phi))).Size(0)).Aggregations.Sum("chi_phi").Value;

        //    if (tong_chi_phi >= (percent * user_ngan_sach))
        //    {
        //        if (chien_dich.Count > 0)
        //        {
        //            foreach (var cd in chien_dich)
        //            {
        //                if (UpdateTrangThai(cd.id, (int)TrangThaiChienDich.DUNG_DO_HET_NGAN_SACH))
        //                {
        //                    var lst_quang_cao = QuangCaoRepository.Instance.GetByIdChienDich(cd.id, new[] { "id" }).Select(x => x.id).ToList();
        //                    if (lst_quang_cao.Count > 0)
        //                    {
        //                        foreach (var qc_id in lst_quang_cao)
        //                        {
        //                            if (QuangCaoRepository.Instance.UpdateTrangThaiQcTheoChienDich(qc_id,
        //                                (int)TrangThaiQuangCao.TAM_DUNG))
        //                            {
        //                                var lst_tu_khoa = TuKhoaRepository.Instance.GetByIdQuangCao(qc_id, new[] { "id" })
        //                                    .Select(x => x.id).ToList();
        //                                if (lst_tu_khoa.Count > 0)
        //                                {
        //                                    foreach (var tk_id in lst_tu_khoa)
        //                                    {
        //                                        TuKhoaRepository.Instance.UpdateTrangThaiTkTheoChienDich(tk_id, (int)TrangThaiTuKhoa.TAM_DUNG);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        // Tính toán ngân sách các chiến dịch theo người tạo
        public void TinhToanTongNganSachCacChienDich(string nguoi_tao, double percent = 0.9)
        {
            var user_ngan_sach = UserRepository.Instance.GetById(nguoi_tao, new[] { "ngan_sach" })?.ngan_sach;
            var chien_dich = GetAll(new List<string> { nguoi_tao }, new[] { "id", "chi_phi" }).ToList();
            var tong_chi_phi = client.Search<ChienDich>(s =>
                s.Query(x => x.Term(o => o.Field("nguoi_tao").Value(nguoi_tao)))
                    .Aggregations(o => o.Sum("chi_phi", tt => tt.Field(c => c.chi_phi))).Size(0)).Aggregations.Sum("chi_phi").Value;
            if (tong_chi_phi >= (percent * user_ngan_sach))
            {
                if (chien_dich.Count > 0)
                {
                    UpdateTrangThaiDoHetNganSach(chien_dich.Select(x => x.id).ToList(),
                        (int)TrangThaiChienDich.DUNG_DO_HET_NGAN_SACH);
                    QuangCaoRepository.Instance.UpdateTrangThaiDoHetNganSachTheoChienDich(chien_dich.Select(x => x.id).ToList(), (int)TrangThaiQuangCao.TAM_DUNG);
                    TuKhoaRepository.Instance.UpdateTrangThaiDoHetNganSachTheoChienDich(
                        chien_dich.Select(x => x.id).ToList(), (int)TrangThaiTuKhoa.TAM_DUNG);
                }
            }
        }

        public void UpdateTrangThaiDoHetNganSach(List<string> ids, int trang_thai)
        {
            var re = client.UpdateByQuery<ChienDich>(u => u.Query(q => q.Ids(i => i.Values(ids)))
                .Script(sc => sc.Source("ctx._source.trang_thai=params.trang_thai")
                    .Params(new Dictionary<string, object>()
                    {
                        {
                            "trang_thai", trang_thai
                        }
                    })));
        }

        public bool UpdateDiemToiUuChienDich(string id, double diem_toi_uu)
        {
            var re = client.Update<ChienDich, object>(id, u => u.Doc(new { diem_toi_uu = diem_toi_uu }));
            return re.Result == Result.Updated || re.Result == Result.Noop;
        }

        public bool PartiallyUpdated(string id, string ten, int trang_thai, double ngan_sach, GiaThau gia_thau, long ngay_bat_dau, long ngay_ket_thuc, List<string> ip_loai_tru)
        {
            var re = client.Update<ChienDich, object>(id,
                u => u.Doc(new
                {
                    ten = ten,
                    trang_thai = trang_thai,
                    ngan_sach = ngan_sach,
                    gia_thau = gia_thau,
                    ngay_bat_dau = ngay_bat_dau,
                    ngay_ket_thuc = ngay_ket_thuc,
                    ip_loai_tru = ip_loai_tru,
                }));

            return re.Result == Result.Updated;
        }
    }
}