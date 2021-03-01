using ES;
using JobokoAdsModels;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobokoAdsES
{
    public class QuangCaoRepository : IESRepository
    {
        #region Init

        protected static string _DefaultIndex;

        //protected new ElasticClient client;
        private static QuangCaoRepository instance;

        public QuangCaoRepository(string modify_index)
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

        public static QuangCaoRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    _DefaultIndex = "jok_quangcao";
                    instance = new QuangCaoRepository(_DefaultIndex);
                }
                return instance;
            }
        }

        public bool CreateIndex(bool delete_if_exist = false)
        {
            if (delete_if_exist && client.Indices.Exists(_DefaultIndex).Exists)
                client.Indices.Delete(_DefaultIndex);

            var createIndexResponse = client.Indices.Create(_DefaultIndex, s => s.Settings(st => st.NumberOfReplicas(0).NumberOfShards(3)).Map<QuangCao>(mm => mm.AutoMap().Dynamic(true)));
            return createIndexResponse.IsValid;
        }

        #endregion Init

        public IEnumerable<QuangCao> GetAll(string term, List<string> nguoi_tao, bool is_admin, string[] view_fields, Dictionary<string, bool> sort_fields, int trang_thai, string id_chien_dich, string id_quang_cao, int page, int page_size, out long total_recs)
        {
            total_recs = 0;
            List<QueryContainer> must = new List<QueryContainer>();

            must.Add(new TermsQuery()
            {
                Field = "nguoi_tao",
                Terms = nguoi_tao
            });

            if (!string.IsNullOrEmpty(id_chien_dich))
            {
                must.Add(new TermQuery()
                {
                    Field = "id_chien_dich.keyword",
                    Value = id_chien_dich
                });
            }
            if (!string.IsNullOrEmpty(id_quang_cao))
            {
                must.Add(new IdsQuery()
                {
                    Values = new List<Id>() { new Id(id_quang_cao) }
                });
            }

            if (trang_thai > -1)
            {
                must.Add(new TermQuery()
                {
                    Field = "trang_thai",
                    Value = trang_thai
                });
            }

            var lst = GetAll<QuangCao>(_DefaultIndex, term, new string[] { "ten_hien_thi", "link_dich", "link_hien_thi", "tieu_de_1", "tieu_de_2" }, view_fields, sort_fields, page, page_size, out total_recs, must);
            return lst;
        }

        public IEnumerable<QuangCao> GetAll(List<string> nguoi_tao, string[] view_fields)
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
            return GetObjectScroll<QuangCao>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), source);
        }

        public Dictionary<string, double> FilterAggregations(string term, string[] search_fields, int trang_thai, List<string> nguoi_tao, string id_chien_dich, string id_quang_cao, bool is_admin)
        {
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
                must.Add(new TermQuery()
                {
                    Field = "id_chien_dich.keyword",
                    Value = id_chien_dich
                });
            }
            if (!string.IsNullOrEmpty(id_quang_cao))
            {
                must.Add(new IdsQuery() { Values = new List<Id>() { new Id(id_quang_cao) } });
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

            var res = client.Search<QuangCao>(x => x.Aggregations(o => o.Filter("filter",
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

            var filterAgg = res.Aggregations.Filter("filter");
            Dictionary<string, double> data_sum = new Dictionary<string, double>
            {
                {"chi_phi", Convert.ToDouble(filterAgg.Sum("chi_phi")?.Value)},
                {"luot_hien_thi", Convert.ToDouble(filterAgg.Sum("luot_hien_thi")?.Value)},
                {"luot_click", Convert.ToDouble(filterAgg.Sum("luot_click")?.Value) },
                {"ty_le", Convert.ToDouble(filterAgg.Average("ty_le")?.Value) },
                {"cpc", Convert.ToDouble(filterAgg.Average("cpc")?.Value) }
            };

            return data_sum;
        }

        public bool PartiallyUpdated(string id, string ten_hien_thi, int trang_thai, int loai_quang_cao, string link_hien_thi, string tieu_de_1, string tieu_de_2, string mo_ta_1, string mo_ta_2)
        {
            var re = client.Update<QuangCao, object>(id,
                u => u.Doc(new
                {
                    ten_hien_thi = ten_hien_thi,
                    trang_thai = trang_thai,
                    loai_quang_cao = loai_quang_cao,
                    link_hien_thi = link_hien_thi,
                    tieu_de_1 = tieu_de_1,
                    tieu_de_2 = tieu_de_2,
                    mo_ta_1 = mo_ta_1,
                    mo_ta_2 = mo_ta_2,
                }));

            return re.Result == Result.Updated;
        }

        public IEnumerable<QuangCao> GetByIdChienDich(string id, string[] view_fields)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.Add(new TermQuery()
            {
                Field = "id_chien_dich.keyword",
                Value = id
            });

            var source = new SourceFilter
            {
                Includes = view_fields
            };
            if (view_fields == null) source = new SourceFilter();
            return GetObjectScroll<QuangCao>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), source);
        }

        public IEnumerable<QuangCao> GetAll()
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.Add(new MatchAllQuery());
            return GetObjectScroll<QuangCao>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), new SourceFilter());
        }

        public string IndexRetId(QuangCao data)
        {
            string custom_id = $"{data.id_chien_dich}_{id_gen.CreateId()}";
            if (Index(_DefaultIndex, data, string.Empty, custom_id, out string id))
                return id;
            else
                return string.Empty;
        }

        public bool Update(QuangCao data)
        {
            var re = client.Update<QuangCao>(data.id, u => u.Doc(data));
            return re.Result == Result.Updated || re.Result == Result.Noop;
        }

        public bool Delete(string id)
        {
            return DeleteById<QuangCao>(id);
        }

        public QuangCao GetById(string id, string[] view_fields)
        {
            var obj = GetById<QuangCao>(id, view_fields);
            if (obj != null)
            {
                obj.id = id;
                return obj;
            }
            return null;
        }

        public IEnumerable<QuangCao> GetMany(IEnumerable<string> ids)
        {
            var re = GetMany<QuangCao>(_DefaultIndex, ids);
            return re;
        }

        public IEnumerable<QuangCao> GetMany(IEnumerable<string> ids, IEnumerable<TuKhoa> lst_tukhoa, string[] fields)
        {
            var m = new MultiGetRequest(_DefaultIndex);
            var lst_get = new List<MultiGetOperation<QuangCao>>();
            foreach (var item in ids)
            {
                lst_get.Add(new MultiGetOperation<QuangCao>(item));
            }
            m.Documents = lst_get;
            m.SourceIncludes = fields;
            var mre = client.MultiGet(m);
            var lst_quang_cao = new List<QuangCao>();
            var ids_tukhoa = lst_tukhoa.Select(x => x.id);
            foreach (var id in ids)
            {
                var quang_cao = mre.Source<QuangCao>(id);
                if (quang_cao != null)
                {
                    quang_cao.ids_tu_khoa = quang_cao.ids_tu_khoa != null ? quang_cao.ids_tu_khoa.Where(x => ids_tukhoa.Contains(x)).ToList() : new List<string>();
                    //nếu từ khóa có url cuối thì lấy để thay thế cho url của quảng cáo

                    var tk = lst_tukhoa.FirstOrDefault(x => quang_cao.ids_tu_khoa.Contains(x.id));
                    if (tk != null && !string.IsNullOrEmpty(tk.url_cuoi))
                        quang_cao.link_dich = tk.url_cuoi;
                    quang_cao.id = id;
                    lst_quang_cao.Add(quang_cao);
                }
            }
            return lst_quang_cao;
        }
        public IEnumerable<QuangCao> GetMany(IEnumerable<string> ids, string[] fields)
        {
            var m = new MultiGetRequest(_DefaultIndex);
            var lst_get = new List<MultiGetOperation<QuangCao>>();
            foreach (var item in ids)
            {
                lst_get.Add(new MultiGetOperation<QuangCao>(item));
            }
            m.Documents = lst_get;
            m.SourceIncludes = fields;
            var mre = client.MultiGet(m);
            var lst_quang_cao = new List<QuangCao>();

            foreach (var id in ids)
            {
                var quang_cao = mre.Source<QuangCao>(id);
                if (quang_cao != null)
                {
                    quang_cao.id = id;
                    lst_quang_cao.Add(quang_cao);
                }
            }
            return lst_quang_cao;
        }
        public bool UpdateIdTuKhoa(string id, string id_tu_khoa, out string msg)
        {
            msg = "";
            try
            {
                StringBuilder stb = new StringBuilder();
                stb.AppendFormat("if(ctx._source.ids_tu_khoa==null){{ctx._source.ids_tu_khoa= [params.id_tu_khoa]}}");
                stb.AppendFormat("else{{");
                stb.AppendFormat("  String id_tk = null;");
                stb.AppendFormat("  Set hs_id_tk = new HashSet();");
                stb.AppendFormat("  for(ListIterator it = ctx._source.ids_tu_khoa.listIterator(); it.hasNext();){{");
                stb.AppendFormat("      id_tk = it.next();");
                stb.AppendFormat("      it.remove();");
                stb.AppendFormat("      hs_id_tk.add(id_tk);");
                stb.AppendFormat("  }}");

                stb.AppendFormat("  hs_id_tk.add('{0}');", id_tu_khoa);

                stb.AppendFormat("  ctx._source.ids_tu_khoa.addAll(hs_id_tk);");
                stb.AppendFormat("}}");

                var re = client.Update<QuangCao, object>(id, u => u.Script(s => s.Source(stb.ToString()).Params(new Dictionary<string, object>() { { "id_tu_khoa", id_tu_khoa } })));
                if (!re.IsValid && re.ServerError.Error != null)
                    msg = re.ServerError.Error.Reason;
                return re.Result == Result.Noop || re.Result == Result.Updated;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return false;
        }

        private bool IncreaseView(string id, int count = 1)
        {
            StringBuilder stb = new StringBuilder();
            stb.Append("if(ctx._source.luot_hien_thi==null){ctx._source.luot_hien_thi=params.count}else{ctx._source.luot_hien_thi+=params.count} ");
            stb.Append("ctx._source.ty_le_tuong_tac=(float)ctx._source.luot_click*100/(ctx._source.luot_hien_thi+params.count)");
            var re = client.Update<QuangCao, object>(id, u =>
                u.Script(s => s.Source(stb.ToString())
                .Params(new Dictionary<string, object>() {
                    { "count",count }
                })));
            return re.Result == Result.Updated;
        }

        public bool IncreaseView(string id, string id_tk, int count = 1)
        {
            if (TuKhoaRepository.Instance.IncreaseView(id_tk, count) && IncreaseView(id, count))
            {
                //Tăng view của chiến dịch
                string id_chien_dich = id.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                return ChienDichRepository.Instance.IncreaseView(id_chien_dich, count);
            }
            return false;
        }

        public bool IncreaseClick(string id, string id_tk, double gia_thau = 0)
        {
            if (TuKhoaRepository.Instance.IncreaseClick(id_tk))
            {
                var quang_cao = client.Get<QuangCao>(id, g => g.SourceIncludes(new string[] { "id", "id_chien_dich", "luot_click", "chi_phi", "luot_hien_thi" }));

                string id_chien_dich = quang_cao.Source.id_chien_dich;
                //Click thành công thì cập nhật lại chi phí cho quảng cáo theo giá thầu
                var chien_dich = ChienDichRepository.Instance.GetById(id_chien_dich, new string[] { "id", "ngan_sach", "gia_thau", "chi_phi", "luot_click" });
                if (chien_dich != null)
                {
                    var chi_phi_quang_cao = gia_thau == 0 ? chien_dich.gia_thau.gia_toi_da : gia_thau;
                    chi_phi_quang_cao = chi_phi_quang_cao < 1000 ? 1000 : chi_phi_quang_cao;
                    //Cập nhật ngân sách ngày?

                    var re_update_chi_phi_quang_cao = client.Update<QuangCao, object>(id, u => u.Doc(new
                    {
                        chi_phi = quang_cao.Source.chi_phi + chi_phi_quang_cao,
                        luot_click = quang_cao.Source.luot_click + 1,
                        ty_le_tuong_tac = quang_cao.Source.luot_hien_thi > 0 ? (float)(quang_cao.Source.luot_click + 1) * 100 / quang_cao.Source.luot_hien_thi : 0,
                        cpc_trung_binh = (quang_cao.Source.chi_phi + chi_phi_quang_cao) / (quang_cao.Source.luot_click + 1)
                    }
                    ));
                    bool need_roll_back_click = !(re_update_chi_phi_quang_cao.Result == Result.Updated);
                    if (!need_roll_back_click)
                    {
                        need_roll_back_click = !ChienDichRepository.Instance.UpdateChiPhiLuotClickCongDonChiPhiNgay(id_chien_dich, chien_dich.luot_click + 1,
                            chien_dich.chi_phi + chi_phi_quang_cao, chi_phi_quang_cao);
                    }
                    if (need_roll_back_click)
                    {
                        //Rollback
                        re_update_chi_phi_quang_cao = client.Update<QuangCao, object>(id, u => u.Doc(new
                        {
                            chi_phi = quang_cao.Source.chi_phi,
                            luot_click = quang_cao.Source.luot_click,
                            ty_le_tuong_tac = quang_cao.Source.luot_hien_thi > 0 ? (float)quang_cao.Source.luot_click * 100 / quang_cao.Source.luot_hien_thi : 0,
                            cpc_trung_binh = quang_cao.Source.chi_phi / quang_cao.Source.luot_click
                        }));
                        TuKhoaRepository.Instance.IncreaseClick(id_tk, -1);
                    }
                    return !need_roll_back_click;
                }
            }
            return false;
        }

        public bool UpdateTrangThai(string id, int trang_thai)
        {
            var re = client.Update<QuangCao, object>(id, u => u.Doc(new { trang_thai = trang_thai }));
            return re.Result == Result.Updated;
        }

        //public bool UpdateTrangThaiQcTheoChienDich(string id, int trang_thai)
        //{
        //    var re = client.Update<QuangCao, object>(id, u => u.Doc(new { trang_thai_chien_dich = trang_thai }));
        //    return re.Result == Result.Updated;
        //}

        public void UpdateTrangThaiQcTheoChienDich(string id_chien_dich, int trang_thai)
        {
            var re = client.UpdateByQuery<QuangCao>(u => u.Query(q => q.Term(t => t.Field("id_chien_dich.keyword")
                    .Value(id_chien_dich)))
                .Script(sc => sc.Source("ctx._source.trang_thai_chien_dich=params.trang_thai")
                    .Params(new Dictionary<string, object>()
                    {
                        {
                            "trang_thai", trang_thai
                        }
                    })));
        }

        public void UpdateTrangThaiDoHetNganSachTheoChienDich(List<string> ids, int trang_thai)
        {
            var re = client.UpdateByQuery<QuangCao>(u => u.Query(q => q.Terms(i => i.Field("id_chien_dich.keyword").Terms(ids)))
                .Script(sc => sc.Source("ctx._source.trang_thai_chien_dich=params.trang_thai")
                    .Params(new Dictionary<string, object>()
                    {
                        {
                            "trang_thai", trang_thai
                        }
                    })));
        }

        public double TinhDiemToiUu(string id)
        {
            double diem_toi_uu = 0;
            long diem_click_view = 0, diem_thoi_gian_chay_chien_dich = 0, diem_ngan_sach_ngay = 0, diem_cpc_toi_da = 0, diem_quang_cao_ua_thich = 0, diem_ngan_sach_gio = 0;
            double ngan_sach_binh_thuong = 500000, ngan_sach_cao = 1000000;
            try
            {
                var quang_cao = GetById(id, new string[] { "vi_tri_trung_binh", "luot_click", "luot_hien_thi", "id_chien_dich" });
                if (quang_cao != null)
                {
                    var chien_dich = ChienDichRepository.Instance.GetById<ChienDich>(quang_cao.id_chien_dich);
                    if (chien_dich == null)
                        return diem_toi_uu;

                    #region Điểm click/view

                    diem_click_view = 100 * quang_cao.luot_click / quang_cao.luot_hien_thi;
                    if (diem_click_view < 10) //0-10
                    {
                        diem_click_view = 2;
                    }
                    else
                    {
                        if (diem_click_view < 20)//10-20
                        {
                            diem_click_view = 4;
                        }
                        else
                        {
                            if (diem_click_view < 40)//20-40
                            {
                                diem_click_view = 6;
                            }
                            else
                            {
                                if (diem_click_view < 60)//40-60
                                {
                                    diem_click_view = 8;
                                }
                                else //>60
                                {
                                    diem_click_view = 10;
                                }
                            }
                        }
                    }

                    #endregion Điểm click/view

                    #region Điểm thời gian chạy quảng cáo

                    diem_thoi_gian_chay_chien_dich = Convert.ToInt32((DateTime.Now - XMedia.XUtil.EpochToTime(chien_dich.ngay_chay)).TotalDays);
                    if (diem_thoi_gian_chay_chien_dich < 3)
                    {
                        diem_thoi_gian_chay_chien_dich = 10;
                    }
                    else
                    {
                        if (diem_thoi_gian_chay_chien_dich < 5)
                        {
                            diem_thoi_gian_chay_chien_dich = 8;
                        }
                        else
                        {
                            if (diem_thoi_gian_chay_chien_dich < 7)
                            {
                                diem_thoi_gian_chay_chien_dich = 6;
                            }
                            else
                            {
                                if (diem_thoi_gian_chay_chien_dich < 10)
                                {
                                    diem_thoi_gian_chay_chien_dich = 4;
                                }
                                else
                                {
                                    diem_thoi_gian_chay_chien_dich = 2;
                                }
                            }
                        }
                    }

                    #endregion Điểm thời gian chạy quảng cáo

                    #region Điểm ngân sách ngày

                    if (chien_dich.ngan_sach < ngan_sach_binh_thuong)
                    {
                        diem_ngan_sach_ngay = 1;
                    }
                    else
                    {
                        if (chien_dich.ngan_sach < ngan_sach_cao)
                        {
                            diem_ngan_sach_ngay = 3;
                        }
                        else
                        {
                            diem_ngan_sach_ngay = 5;
                        }
                    }

                    #endregion Điểm ngân sách ngày

                    #region Điểm CPC tối đa

                    if (chien_dich.gia_thau.gia_toi_da < 3000)
                    {
                        diem_cpc_toi_da = 10;
                    }
                    else
                    {
                        if (chien_dich.gia_thau.gia_toi_da < 7000)
                        {
                            diem_cpc_toi_da = 20;
                        }
                        else
                        {
                            diem_cpc_toi_da = 30;
                        }
                    }

                    #endregion Điểm CPC tối đa

                    #region Điểm ưa thích quảng cáo của người dùng

                    var diem_vi_tri = 2.0;
                    if (quang_cao.vi_tri_trung_binh == 0)
                    {
                        diem_vi_tri = 2.0;
                    }
                    else
                    {
                        if (quang_cao.vi_tri_trung_binh <= 2)
                        {
                            diem_vi_tri = 1.2;
                        }
                        else
                        {
                            if (quang_cao.vi_tri_trung_binh <= 3)
                            {
                                diem_vi_tri = 1.5;
                            }
                            else
                            {
                                if (quang_cao.vi_tri_trung_binh <= 4)
                                {
                                    diem_vi_tri = 1.7;
                                }
                                else
                                {
                                    diem_vi_tri = 2.0;
                                }
                            }
                        }
                    }
                    if (quang_cao.luot_hien_thi == 0) quang_cao.luot_hien_thi = 1;
                    if (quang_cao.luot_click == 0) quang_cao.luot_click = 1;
                    diem_quang_cao_ua_thich = Convert.ToInt64(((double)quang_cao.luot_click / quang_cao.luot_hien_thi) * (diem_vi_tri / 2) * 30);

                    #endregion Điểm ưa thích quảng cáo của người dùng

                    #region Điểm ngân sách giờ

                    var ngan_sach_con_lai = chien_dich.ngan_sach - chien_dich.chi_phi_ngay;
                    if (ngan_sach_con_lai < ngan_sach_binh_thuong)
                    {
                        diem_ngan_sach_gio = 1;
                    }
                    else
                    {
                        if (ngan_sach_con_lai < ngan_sach_cao)
                        {
                            diem_ngan_sach_gio = 3;
                        }
                        else
                        {
                            diem_ngan_sach_gio = 5;
                        }
                    }
                    diem_ngan_sach_gio = Convert.ToInt64(15 * (double)(24 * ((double)diem_ngan_sach_gio / 5)) / (24 - DateTime.Now.Hour));
                    diem_ngan_sach_gio = diem_ngan_sach_gio > 15 ? 15 : diem_ngan_sach_gio;
                    #endregion Điểm ngân sách giờ
                }
            }
            catch (Exception)
            {
            }
            diem_toi_uu = diem_click_view + diem_thoi_gian_chay_chien_dich + diem_ngan_sach_ngay + diem_cpc_toi_da + diem_quang_cao_ua_thich + diem_ngan_sach_gio;
            return diem_toi_uu / 100;
        }
        public double TinhDiemToiUuChienDich(string id_chien_dich)
        {
            double diem_toi_uu = 0;
            double ngan_sach_binh_thuong = 500000, ngan_sach_cao = 1000000;
            try
            {
                var chien_dich = ChienDichRepository.Instance.GetById<ChienDich>(id_chien_dich);
                if (chien_dich == null)
                    return diem_toi_uu;
                var lst_quang_cao = GetByIdChienDich(id_chien_dich, new string[] { "vi_tri_trung_binh", "luot_click", "luot_hien_thi", "id_chien_dich" });
                if (lst_quang_cao.Count() == 0)
                    return diem_toi_uu;
                foreach (var quang_cao in lst_quang_cao)
                {
                    long diem_click_view = 0, diem_thoi_gian_chay_chien_dich = 0, diem_ngan_sach_ngay = 0, diem_cpc_toi_da = 0, diem_quang_cao_ua_thich = 0, diem_ngan_sach_gio = 0;

                    #region Điểm click/view

                    diem_click_view = 100 * quang_cao.luot_click / quang_cao.luot_hien_thi;
                    if (diem_click_view < 10) //0-10
                    {
                        diem_click_view = 2;
                    }
                    else
                    {
                        if (diem_click_view < 20)//10-20
                        {
                            diem_click_view = 4;
                        }
                        else
                        {
                            if (diem_click_view < 40)//20-40
                            {
                                diem_click_view = 6;
                            }
                            else
                            {
                                if (diem_click_view < 60)//40-60
                                {
                                    diem_click_view = 8;
                                }
                                else //>60
                                {
                                    diem_click_view = 10;
                                }
                            }
                        }
                    }

                    #endregion Điểm click/view

                    #region Điểm thời gian chạy quảng cáo

                    diem_thoi_gian_chay_chien_dich = Convert.ToInt32((DateTime.Now - XMedia.XUtil.EpochToTime(chien_dich.ngay_chay)).TotalDays);
                    if (diem_thoi_gian_chay_chien_dich < 3)
                    {
                        diem_thoi_gian_chay_chien_dich = 10;
                    }
                    else
                    {
                        if (diem_thoi_gian_chay_chien_dich < 5)
                        {
                            diem_thoi_gian_chay_chien_dich = 8;
                        }
                        else
                        {
                            if (diem_thoi_gian_chay_chien_dich < 7)
                            {
                                diem_thoi_gian_chay_chien_dich = 6;
                            }
                            else
                            {
                                if (diem_thoi_gian_chay_chien_dich < 10)
                                {
                                    diem_thoi_gian_chay_chien_dich = 4;
                                }
                                else
                                {
                                    diem_thoi_gian_chay_chien_dich = 2;
                                }
                            }
                        }
                    }

                    #endregion Điểm thời gian chạy quảng cáo

                    #region Điểm ngân sách ngày

                    if (chien_dich.ngan_sach < ngan_sach_binh_thuong)
                    {
                        diem_ngan_sach_ngay = 1;
                    }
                    else
                    {
                        if (chien_dich.ngan_sach < ngan_sach_cao)
                        {
                            diem_ngan_sach_ngay = 3;
                        }
                        else
                        {
                            diem_ngan_sach_ngay = 5;
                        }
                    }

                    #endregion Điểm ngân sách ngày

                    #region Điểm CPC tối đa

                    if (chien_dich.gia_thau.gia_toi_da < 3000)
                    {
                        diem_cpc_toi_da = 10;
                    }
                    else
                    {
                        if (chien_dich.gia_thau.gia_toi_da < 7000)
                        {
                            diem_cpc_toi_da = 20;
                        }
                        else
                        {
                            diem_cpc_toi_da = 30;
                        }
                    }

                    #endregion Điểm CPC tối đa

                    #region Điểm ưa thích quảng cáo của người dùng

                    var diem_vi_tri = 2.0;
                    if (quang_cao.vi_tri_trung_binh == 0)
                    {
                        diem_vi_tri = 2.0;
                    }
                    else
                    {
                        if (quang_cao.vi_tri_trung_binh <= 2)
                        {
                            diem_vi_tri = 1.2;
                        }
                        else
                        {
                            if (quang_cao.vi_tri_trung_binh <= 3)
                            {
                                diem_vi_tri = 1.5;
                            }
                            else
                            {
                                if (quang_cao.vi_tri_trung_binh <= 4)
                                {
                                    diem_vi_tri = 1.7;
                                }
                                else
                                {
                                    diem_vi_tri = 2.0;
                                }
                            }
                        }
                    }
                    if (quang_cao.luot_hien_thi == 0) quang_cao.luot_hien_thi = 1;
                    if (quang_cao.luot_click == 0) quang_cao.luot_click = 1;
                    diem_quang_cao_ua_thich = Convert.ToInt64(((double)quang_cao.luot_click / quang_cao.luot_hien_thi) * (diem_vi_tri / 2) * 30);

                    #endregion Điểm ưa thích quảng cáo của người dùng

                    #region Điểm ngân sách giờ

                    var ngan_sach_con_lai = chien_dich.ngan_sach - chien_dich.chi_phi_ngay;
                    if (ngan_sach_con_lai < ngan_sach_binh_thuong)
                    {
                        diem_ngan_sach_gio = 1;
                    }
                    else
                    {
                        if (ngan_sach_con_lai < ngan_sach_cao)
                        {
                            diem_ngan_sach_gio = 3;
                        }
                        else
                        {
                            diem_ngan_sach_gio = 5;
                        }
                    }
                    diem_ngan_sach_gio = Convert.ToInt64(15 * (double)(24 * ((double)diem_ngan_sach_gio / 5)) / (24 - DateTime.Now.Hour));
                    diem_ngan_sach_gio = diem_ngan_sach_gio > 15 ? 15 : diem_ngan_sach_gio;
                    #endregion Điểm ngân sách giờ
                    diem_toi_uu += diem_click_view + diem_thoi_gian_chay_chien_dich + diem_ngan_sach_ngay + diem_cpc_toi_da + diem_quang_cao_ua_thich + diem_ngan_sach_gio;
                }

                diem_toi_uu = diem_toi_uu / lst_quang_cao.Count();
            }
            catch (Exception)
            {
            }
            ChienDichRepository.Instance.UpdateDiemToiUuChienDich(id_chien_dich, diem_toi_uu / 100);
            return diem_toi_uu / 100;
        }
    }
}