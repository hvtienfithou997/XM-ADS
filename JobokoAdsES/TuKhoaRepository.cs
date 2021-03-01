using ES;
using JobokoAdsModels;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobokoAdsES
{
    public class TuKhoaRepository : IESRepository
    {
        #region Init

        protected static string _DefaultIndex;

        //protected new ElasticClient client;
        private static TuKhoaRepository instance;

        public TuKhoaRepository(string modify_index)
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

        public static TuKhoaRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    _DefaultIndex = "jok_tukhoa";
                    instance = new TuKhoaRepository(_DefaultIndex);
                }
                return instance;
            }
        }

        public bool CreateIndex(bool delete_if_exist = false)
        {

            if (delete_if_exist && client.Indices.Exists(_DefaultIndex).Exists)
                client.Indices.Delete(_DefaultIndex);

            var createIndexResponse = client.Indices.Create(_DefaultIndex, s => s.Settings(st => st.NumberOfReplicas(0).NumberOfShards(3)).Map<TuKhoa>(mm =>
                mm.AutoMap().Properties(p => p.Percolator(pe => pe.Name("query"))).Dynamic(true)));
            return createIndexResponse.IsValid;



        }
        private void UpdateMapping()
        {
            var properties = new PropertyWalker(typeof(TuKhoa), null).GetProperties();
            var propertiesToIndex = new Dictionary<PropertyName, IProperty>();
            propertiesToIndex.Add("query", new PercolatorProperty());
            var request = new PutMappingRequest<TuKhoa>()
            {
                Properties = new Properties(propertiesToIndex)
            };

            var putMappingResponse = client.Map(request);

        }
        #endregion Init

        public IEnumerable<TuKhoa> GetAll(string term, string id_chien_dich, string id_quang_cao, List<string> nguoi_tao, bool is_admin, string[] view_fields, Dictionary<string, bool> sort_fields, int trang_thai, int page, int page_size, out long total_recs)
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
                must.Add(new TermQuery() { Field = "id_chien_dich.keyword", Value = id_chien_dich });
            }
            if (!string.IsNullOrEmpty(id_quang_cao))
            {
                must.Add(new TermQuery() { Field = "id_quang_cao.keyword", Value = id_quang_cao });
            }

            if (trang_thai > -1)
            {
                must.Add(new TermQuery()
                {
                    Field = "trang_thai",
                    Value = trang_thai
                });
            }

            var lst = GetAll<TuKhoa>(_DefaultIndex, term, new string[] { "tu_khoa" }, view_fields, sort_fields, page, page_size, out total_recs, must);
            return lst;
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
                must.Add(new TermQuery() { Field = "id_chien_dich.keyword", Value = id_chien_dich });
            }
            if (!string.IsNullOrEmpty(id_quang_cao))
            {
                must.Add(new TermQuery() { Field = "id_quang_cao.keyword", Value = id_quang_cao });
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
            var res = client.Search<TuKhoa>(x => x.Aggregations(o => o.Filter("filter",
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

        public IEnumerable<TuKhoa> GetAll(List<string> nguoi_tao, string[] view_fields)
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
            return GetObjectScroll<TuKhoa>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), source);
        }

        public string IndexRetId(TuKhoa data)
        {
            if (data.kieu_doi_sanh == KieuDoiSanh.DOI_SANH_RONG)
            {
                data.query = new QueryStringQuery() { Query = data.tu_khoa, DefaultField = "tu_khoa" };
            }


            if (Index(_DefaultIndex, data, string.Empty, out string id))
                return id;
            else
                return string.Empty;
        }

        public bool Update(TuKhoa data)
        {
            if (data.kieu_doi_sanh == KieuDoiSanh.DOI_SANH_RONG)
            {
                data.query = new QueryStringQuery() { Query = data.tu_khoa, DefaultField = "tu_khoa" };
            }
            var re = client.Update<TuKhoa>(data.id, u => u.Doc(data));
            return re.Result == Result.Updated || re.Result == Result.Noop;
        }

        public bool Delete(string id)
        {
            return DeleteById<TuKhoa>(id);
        }

        public TuKhoa GetById(string id, string[] view_fields)
        {
            var obj = GetById<TuKhoa>(id, view_fields);
            if (obj != null)
            {
                obj.id = id;
                return obj;
            }
            return null;
        }

        public IEnumerable<TuKhoa> GetByIdQuangCao(string id, string[] view_fields)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.Add(new TermQuery()
            {
                Field = "id_quang_cao.keyword",
                Value = id
            });
            var source = new SourceFilter
            {
                Includes = view_fields
            };
            if (view_fields == null) source = new SourceFilter();
            return GetObjectScroll<TuKhoa>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), source);
        }

        public IEnumerable<TuKhoa> GetByIdChienDich(string id, string[] view_fields)
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
            return GetObjectScroll<TuKhoa>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), source);
        }
        public IEnumerable<TuKhoaDauGia> DauGiaQuangCaoTheoTuKhoa(string tu_khoa, int so_luong, string dia_diem)
        {
            List<TuKhoaDauGia> lst_tu_khoa_dau_gia = new List<TuKhoaDauGia>();
            double DIEM_CPC = 10, DIEM_NGAN_SACH = 8, DIEM_QUANG_CAO_HIEU_QUA = 15;
            var dic_diem_chien_dich = new Dictionary<string, double>();
            var dic_diem_quang_cao = new Dictionary<string, TuKhoaDauGia>();
            var dic_chien_dich = new Dictionary<string, ChienDich>();
            tu_khoa = tu_khoa.ToLower();
            long ngay_hien_tai = XMedia.XUtil.TimeInEpoch(DateTime.Now.Date);
            //bid dựa vào độ chính xác, giá thầu, thời gian chạy quảng cáo, độ hiệu quả của quảng cáo, độ ưu tiên của chiến dịch
            //1 độ chính xác của từ khóa
            //2 giá thầu, ngân sách còn lại
            //3 độ hiệu quả= thời gian chạy quảng cáo, tỉ lệ CTR
            //4 độ ưu tiên
            #region 1 độ chính xác của từ khóa
            string[] view_fields_tu_khoa = new string[] { "tu_khoa", "kieu_doi_sanh", "url_cuoi", "luot_click", "luot_hien_thi", "chi_phi", "luot_chuyen_doi", "id_chien_dich", "id_quang_cao" };
            List<TuKhoa> ls_tu_khoa_quang_cao = new List<TuKhoa>();
            var re_tu_khoa = client.Search<TuKhoa>(s => s.Source(so => so.Includes(ic => ic.Fields(view_fields_tu_khoa))).Size(so_luong * 10).
            Query(q => q.Term(t => t.Field("trang_thai").Value(TrangThaiTuKhoa.BAT)) &&
                 q.Term(t => t.Field("trang_thai_quang_cao").Value(TrangThaiQuangCao.BAT))
                && q.Term(t => t.Field("trang_thai_chien_dich").Value(TrangThaiChienDich.BAT))
                && ((q.QueryString(o => o.Fields("dia_diem_muc_tieu").Query(dia_diem)) || q.QueryString(o => o.Fields("dia_diem_muc_tieu").Query("\"Toàn quốc\"")))
                && !q.QueryString(c => c.Fields("dia_diem_loai_tru").Query(dia_diem)))
                && (
                    q.Bool(b => b.Must(m => m.QueryString(qs => qs.DefaultField("tu_khoa").Query(string.Format("\"{0}\"", tu_khoa))) && m.Term(t => t.Field("kieu_doi_sanh").Value(KieuDoiSanh.KHOP_CHINH_XAC))).Boost(10))
                    || q.Bool(b => b.Must(m => m.QueryString(qs => qs.DefaultField("tu_khoa").Query(string.Format("\"{0}\"", tu_khoa))) && m.Term(t => t.Field("kieu_doi_sanh").Value(KieuDoiSanh.KHOP_CUM_TU))).Boost(5))
                    //|| q.Bool(b => b.Must(m => m.QueryString(qs => qs.DefaultField("tu_khoa").Query(tu_khoa)) && m.Term(t => t.Field("kieu_doi_sanh").Value(KieuDoiSanh.DOI_SANH_RONG))))
                    || q.Bool(b => b.Must(m => m.Percolate(p => p.Document(new { tu_khoa = tu_khoa }).Field("query"))))
                )
            ));

            if (re_tu_khoa.Total <= 0) return lst_tu_khoa_dau_gia;
            ls_tu_khoa_quang_cao = re_tu_khoa.Documents.ToList();
            #endregion
            #region 2 giá thầu, ngân sách còn lại, 3 độ hiệu quả= thời gian chạy quảng cáo, tỉ lệ CTR, 4 độ ưu tiên
            var lst_id_chien_dich = re_tu_khoa.Documents.Select(x => x.id_chien_dich).Distinct();
            var lst_id_quang_cao = re_tu_khoa.Documents.Select(x => x.id_quang_cao).Distinct();

            var all_chien_dich = ChienDichRepository.Instance.GetMany(lst_id_chien_dich,
                new string[] { "trang_thai", "ngay_bat_dau", "ngay_ket_thuc", "gia_thau", "ngan_sach",
                    "ngan_sach_toi_da", "chi_phi", "ngay_chay","ip_loai_tru","do_uu_tien","diem_toi_uu"
                }).ToList();
            all_chien_dich = all_chien_dich.Where(x => x.trang_thai == TrangThaiChienDich.BAT && x.ngay_bat_dau <= XMedia.XUtil.TimeInEpoch() && x.ngay_ket_thuc >= XMedia.XUtil.TimeInEpoch()).ToList();
            if (all_chien_dich.Count == 0) return lst_tu_khoa_dau_gia;
            //CPC tối đa cao nhất được tính 10 điểm, ngân sách cao nhất được tính 8 điểm
            var max_cpc = all_chien_dich.Max(x => x.gia_thau.gia_toi_da);
            var max_ngan_sach_chien_dich = all_chien_dich.Max(x => x.ngan_sach_toi_da);
            var max_ngan_sach_ngay = all_chien_dich.Max(x => x.ngan_sach);

            //Tính lại điểm ngân sách = 40% của điểm từ khóa phù hợp

            DIEM_NGAN_SACH = 0.4 * re_tu_khoa.MaxScore;
            DIEM_QUANG_CAO_HIEU_QUA = 2 * DIEM_NGAN_SACH;
            DIEM_CPC = 1.25 * DIEM_NGAN_SACH;

            foreach (var chien_dich in all_chien_dich)
            {
                dic_chien_dich.Add(chien_dich.id, chien_dich);
                var diem_cpc = chien_dich.gia_thau.gia_toi_da * DIEM_CPC / max_cpc;
                var diem_ngan_sach = chien_dich.ngan_sach * DIEM_NGAN_SACH / max_ngan_sach_ngay;
                if (chien_dich.ngay_hien_tai < ngay_hien_tai)
                    chien_dich.chi_phi_ngay = 0;
                //Cần tính chi phí theo ngày và so sánh với ngân sách (ngân sách/ngày). Ngân sách ngày có thể cao hơn (ko gấp đôi) hoặc thấp hơn ngân sách cài đặt trong chiến dịch
                //Chi phí ngày của chiến dịch được cập nhật từ hệ thống log tổng
                if (chien_dich.chi_phi_ngay >= 2 * chien_dich.ngan_sach)
                {
                    dic_diem_chien_dich.Add(chien_dich.id, -100);//quá ngân sách thì ko ưu tiên hiển thị
                }
                else
                    dic_diem_chien_dich.Add(chien_dich.id, diem_cpc + diem_ngan_sach);
                //Cổng điểm thời gian chạy chiến dịch
                var diem_thoi_gian_chay_chien_dich = Convert.ToInt32((DateTime.Now - XMedia.XUtil.EpochToTime(chien_dich.ngay_chay)).TotalDays);
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
                dic_diem_chien_dich[chien_dich.id] += diem_thoi_gian_chay_chien_dich;
                //Cộng điểm CTR
                dic_diem_chien_dich[chien_dich.id] += chien_dich.do_uu_tien;
                //Cộng điểm tối ưu
                dic_diem_chien_dich[chien_dich.id] += chien_dich.diem_toi_uu * 100;
            }

            foreach (var item in re_tu_khoa.Hits)
            {
                dic_diem_chien_dich.TryGetValue(item.Source.id_chien_dich, out double diem_chien_dich);
                dic_chien_dich.TryGetValue(item.Source.id_chien_dich, out ChienDich chien_dich);
                if (chien_dich != null)
                {
                    lst_tu_khoa_dau_gia.Add(new TuKhoaDauGia()
                    {
                        id_chien_dich = item.Source.id_chien_dich,
                        tu_khoa = item.Source.tu_khoa,
                        id_quang_cao = item.Source.id_quang_cao,
                        id_tu_khoa = item.Id,
                        link_dich = item.Source.url_cuoi,
                        diem_sap_xep = diem_chien_dich + item.Score.GetValueOrDefault(),
                        gia_thau = chien_dich.gia_thau.gia_toi_da,
                        diem_toi_uu = chien_dich.diem_toi_uu == 0 ? 0.25 : chien_dich.diem_toi_uu
                    });
                }
            }

            lst_tu_khoa_dau_gia = lst_tu_khoa_dau_gia.GroupBy(x => x.id_quang_cao).Select(x => x.First()).OrderByDescending(x => x.diem_sap_xep).ToList();

            if (lst_tu_khoa_dau_gia.Count > 1) //Có từ khóa đấu giá cạnh tranh
            {
                for (int i = 0; i < lst_tu_khoa_dau_gia.Count - 1; i++)
                {
                    double chenh_gia_thau = 0;
                    try
                    {
                        chenh_gia_thau = Math.Abs(lst_tu_khoa_dau_gia[i].gia_thau - lst_tu_khoa_dau_gia[i + 1].gia_thau);
                    }
                    catch (Exception)
                    {
                        chenh_gia_thau = lst_tu_khoa_dau_gia[i].gia_thau;
                    }
                    if (chenh_gia_thau < lst_tu_khoa_dau_gia[i].gia_thau)
                        lst_tu_khoa_dau_gia[i].gia_thau -= chenh_gia_thau * lst_tu_khoa_dau_gia[i].diem_toi_uu;

                    lst_tu_khoa_dau_gia[i].gia_thau = Math.Round(lst_tu_khoa_dau_gia[i].gia_thau);

                }
            }
            else //Không có từ khóa đấu giá cạnh tranh
            {
                lst_tu_khoa_dau_gia[0].gia_thau -= (1 - lst_tu_khoa_dau_gia[0].diem_toi_uu) * lst_tu_khoa_dau_gia[0].gia_thau;
                lst_tu_khoa_dau_gia[0].gia_thau = Math.Round(lst_tu_khoa_dau_gia[0].gia_thau);
            }
            #endregion
            lst_tu_khoa_dau_gia = lst_tu_khoa_dau_gia.Where(x => x.gia_thau > 0).Take(so_luong).ToList();
            var lst_quang_cao = QuangCaoRepository.Instance.GetMany(lst_tu_khoa_dau_gia.Select(x => x.id_quang_cao),
                new string[] { "id", "id_chien_dich", "ten_hien_thi", "link_dich", "link_hien_thi", "tieu_de_1", "tieu_de_2", "mo_ta_1", "mo_ta_2", "ids_tu_khoa" });
            for (int i = lst_tu_khoa_dau_gia.Count - 1; i >= 0; i--)
            {

                var item = lst_tu_khoa_dau_gia[i];
                var quang_cao = lst_quang_cao.FirstOrDefault(x => x.ids_tu_khoa.Contains(item.id_tu_khoa));
                if (quang_cao != null)
                {
                    item.tieu_de_1 = quang_cao.tieu_de_1;
                    item.tieu_de_2 = quang_cao.tieu_de_2;
                    item.mo_ta_1 = quang_cao.mo_ta_1;
                    item.mo_ta_2 = quang_cao.mo_ta_2;
                    item.link_dich = !string.IsNullOrEmpty(item.link_dich) ? item.link_dich : quang_cao.link_dich;
                }
                else
                {
                    lst_tu_khoa_dau_gia.RemoveAt(i);
                }
            }


            return lst_tu_khoa_dau_gia;
        }
        public IEnumerable<TuKhoa> GetTuKhoaHienThi(string tu_khoa, int so_luong, string dia_diem)
        {
            tu_khoa = tu_khoa.ToLower();
            long ngay_hien_tai = XMedia.XUtil.TimeInEpoch(DateTime.Now.Date);

            int DIEM_CPC = 10, DIEM_NGAN_SACH = 8, DIEM_QUANG_CAO_HIEU_QUA = 15;
            if (!string.IsNullOrEmpty(dia_diem))
                dia_diem = string.Format("\"{0}\"", dia_diem);

            List<TuKhoa> ls_tu_khoa_quang_cao = new List<TuKhoa>();
            var re_tu_khoa = client.Search<TuKhoa>(s => s.Source(so => so.Includes(ic => ic.Fields(new string[]
            { "tu_khoa", "kieu_doi_sanh", "url_cuoi","luot_click","luot_hien_thi","chi_phi","luot_chuyen_doi","id_chien_dich","id_quang_cao"}))).Size(so_luong * 10).Query(q => q.Term(t => t.Field("trang_thai").Value(TrangThaiTuKhoa.BAT))
            && q.Term(t => t.Field("trang_thai_quang_cao").Value(TrangThaiQuangCao.BAT))
            && q.Term(t => t.Field("trang_thai_chien_dich").Value(TrangThaiChienDich.BAT))
            && q.QueryString(qs => qs.Fields("tu_khoa").Query(tu_khoa))
            && ((q.QueryString(o => o.Fields("dia_diem_muc_tieu").Query(dia_diem)) || q.QueryString(o => o.Fields("dia_diem_muc_tieu").Query("\"Toàn quốc\"")))
            && !q.QueryString(c => c.Fields("dia_diem_loai_tru").Query(dia_diem)))));
            if (re_tu_khoa.Total == 0)
                return ls_tu_khoa_quang_cao;
            var dic_diem_chien_dich = new Dictionary<string, double>();
            var dic_diem_quang_cao = new Dictionary<string, double>();
            foreach (var hit_tu_khoa in re_tu_khoa.Hits)
            {
                int diem_doi_sanh = 0;
                var it_tu_khoa = hit_tu_khoa.Source;
                it_tu_khoa.id = hit_tu_khoa.Id;
                bool quang_cao_khop_tu_khoa = false;
                switch (it_tu_khoa.kieu_doi_sanh)
                {
                    case KieuDoiSanh.DOI_SANH_RONG: //Có chứa bất kỳ từ nào cũng được
                        ls_tu_khoa_quang_cao.Add(it_tu_khoa); quang_cao_khop_tu_khoa = true;
                        break;

                    case KieuDoiSanh.KHOP_CUM_TU: //Có chứa cụm từ
                        if (tu_khoa.Contains(it_tu_khoa.tu_khoa.ToLower()) || it_tu_khoa.tu_khoa.ToLower().Contains(tu_khoa))
                        {
                            ls_tu_khoa_quang_cao.Add(it_tu_khoa); quang_cao_khop_tu_khoa = true;
                            diem_doi_sanh = 3;
                        }
                        break;

                    case KieuDoiSanh.KHOP_CHINH_XAC: //Chứa chính xác từ
                        if (tu_khoa == it_tu_khoa.tu_khoa.ToLower())
                        {
                            ls_tu_khoa_quang_cao.Add(it_tu_khoa); quang_cao_khop_tu_khoa = true;
                            diem_doi_sanh = 5;
                        }
                        break;
                }
                if (quang_cao_khop_tu_khoa)
                {
                    if (it_tu_khoa.luot_hien_thi < 50)
                    {
                        dic_diem_quang_cao.Add(hit_tu_khoa.Id, DIEM_QUANG_CAO_HIEU_QUA + diem_doi_sanh);
                    }
                    else
                    {
                        var diem_quang_cao = Convert.ToDouble(it_tu_khoa.luot_click) / it_tu_khoa.luot_hien_thi;
                        dic_diem_quang_cao.Add(hit_tu_khoa.Id, diem_quang_cao);
                    }
                }
            }
            if (ls_tu_khoa_quang_cao.Count == 0)
                return new List<TuKhoa>();
            //Kiểm tra ngân sách, ngày bắt đầu + kết thúc của chiến dịch đang chứa các từ khóa đó
            var all_chien_dich = ChienDichRepository.Instance.GetMany(ls_tu_khoa_quang_cao.Select(x => x.id_chien_dich).Distinct()).ToList();

            if (all_chien_dich.Count > 0)
            {
                all_chien_dich = all_chien_dich.Where(x => x.trang_thai == TrangThaiChienDich.BAT && x.ngay_bat_dau <= XMedia.XUtil.TimeInEpoch() && x.ngay_ket_thuc >= XMedia.XUtil.TimeInEpoch()).ToList();
                if (all_chien_dich.Count == 0) return new List<TuKhoa>();
                //CPC tối đa cao nhất được tính 10 điểm, ngân sách cao nhất được tính 8 điểm
                var max_cpc = all_chien_dich.Max(x => x.gia_thau.gia_toi_da);
                var max_ngan_sach = all_chien_dich.Max(x => x.ngan_sach);

                foreach (var chien_dich in all_chien_dich)
                {
                    var diem_cpc = chien_dich.gia_thau.gia_toi_da * DIEM_CPC / max_cpc;
                    var diem_ngan_sach = chien_dich.ngan_sach * DIEM_NGAN_SACH / max_ngan_sach;
                    if (chien_dich.ngay_hien_tai < ngay_hien_tai)
                        chien_dich.chi_phi_ngay = 0;
                    //Cần tính chi phí theo ngày và so sánh với ngân sách (ngân sách/ngày). Ngân sách ngày có thể cao hơn (ko gấp đôi) hoặc thấp hơn ngân sách cài đặt trong chiến dịch
                    //Chi phí ngày của chiến dịch được cập nhật từ hệ thống log tổng
                    if (chien_dich.chi_phi_ngay >= 2 * chien_dich.ngan_sach)
                    {
                        dic_diem_chien_dich.Add(chien_dich.id, -100);//quá ngân sách thì ko ưu tiên hiển thị
                    }
                    else
                        dic_diem_chien_dich.Add(chien_dich.id, diem_cpc + diem_ngan_sach);
                }

                ///tỉ lệ click/view mà nhỏ nghĩa là quảng cáo ko hiệu quả => ưu tiên hiển thị ít hơn (tính điểm hiệu quả)

                foreach (var item in ls_tu_khoa_quang_cao)
                {
                    if (!dic_diem_quang_cao.ContainsKey(item.id))
                        dic_diem_quang_cao.Add(item.id, 0);
                    dic_diem_chien_dich.TryGetValue(item.id_chien_dich, out double diem_chien_dich);
                    dic_diem_quang_cao[item.id] += diem_chien_dich;
                }
            }
            var ls_tu_khoa_quang_cao_sap_xep = new List<TuKhoa>();
            foreach (var id_tu_khoa in dic_diem_quang_cao.OrderByDescending(x => x.Value).Select(x => x.Key))
            {
                var tu_khoa_quang_cao = ls_tu_khoa_quang_cao.First(x => x.id == id_tu_khoa);
                if (!ls_tu_khoa_quang_cao_sap_xep.Any(x => x.id_quang_cao == tu_khoa_quang_cao.id_quang_cao))
                    ls_tu_khoa_quang_cao_sap_xep.Add(tu_khoa_quang_cao);
            }

            return ls_tu_khoa_quang_cao_sap_xep.Take(so_luong).ToList();
        }

        public IEnumerable<TuKhoa> GetMany(IEnumerable<string> ids)
        {
            var re = GetMany<TuKhoa>(_DefaultIndex, ids);
            return re;
        }

        public IEnumerable<TuKhoa> GetMany(IEnumerable<string> ids, string[] fields)
        {
            var re = GetMany<TuKhoa>(_DefaultIndex, ids, fields);
            return re;
        }

        public bool IncreaseView(string id, int count = 1)
        {
            StringBuilder stb = new StringBuilder();
            stb.Append("if(ctx._source.luot_hien_thi==null){ctx._source.luot_hien_thi=params.count}else{ctx._source.luot_hien_thi+=params.count} ");
            stb.Append("ctx._source.ty_le_tuong_tac=(float)ctx._source.luot_click*100/(ctx._source.luot_hien_thi+params.count)");

            var re = client.Update<TuKhoa, object>(id, u =>
                u.Script(s => s.Source(stb.ToString()).Params(new Dictionary<string, object>() {
                    { "count",count }
                })));
            return re.Result == Result.Updated;
        }

        public bool IncreaseClick(string id, int count = 1)
        {
            var tu_khoa = client.Get<TuKhoa>(id, g => g.SourceIncludes(new string[] { "id", "id_chien_dich", "luot_click", "chi_phi", "luot_hien_thi" }));

            //Click thành công thì cập nhật lại chi phí cho quảng cáo theo giá thầu
            var chien_dich = ChienDichRepository.Instance.GetById(tu_khoa.Source.id_chien_dich, new string[] { "id", "ngan_sach", "gia_thau", "chi_phi" });
            if (chien_dich != null)
            {
                var chi_phi_tu_khoa = chien_dich.gia_thau.gia_toi_da;
                chi_phi_tu_khoa = chi_phi_tu_khoa < 1000 ? 1000 : chi_phi_tu_khoa;
                var re_update_chi_phi_quang_cao = client.Update<TuKhoa, object>(id, u => u.Doc(new
                {
                    luot_click = tu_khoa.Source.luot_click + count,
                    chi_phi = tu_khoa.Source.chi_phi + chi_phi_tu_khoa,
                    ty_le_tuong_tac = tu_khoa.Source.luot_hien_thi > 0 ? (float)(tu_khoa.Source.luot_click + count) * 100 / tu_khoa.Source.luot_hien_thi : 0,
                    cpc_trung_binh = (tu_khoa.Source.chi_phi + chi_phi_tu_khoa) / (tu_khoa.Source.luot_click + count)
                }));

                return re_update_chi_phi_quang_cao.Result == Result.Updated;
            }

            return false;
        }

        public bool PartiallyUpdated(string id, int trang_thai, string tu_khoa, int kieu_doi_sanh, string url_cuoi)
        {
            var query = new QueryContainer();

            switch (kieu_doi_sanh)
            {
                case (int)KieuDoiSanh.DOI_SANH_RONG:
                    query = new QueryStringQuery() { Query = tu_khoa, DefaultField = "tu_khoa" };
                    break;
                case (int)KieuDoiSanh.KHOP_CHINH_XAC:
                    query = new TermQuery() { Value = tu_khoa, Field = "tu_khoa" };
                    break;
                case (int)KieuDoiSanh.KHOP_CUM_TU:
                    query = new QueryStringQuery() { Query = string.Format("\"{0}\"", tu_khoa), DefaultField = "tu_khoa" };
                    break;
            }
            var re = client.Update<TuKhoa, object>(id,
                u => u.Doc(new
                {
                    trang_thai = trang_thai,
                    kieu_doi_sanh = kieu_doi_sanh,
                    tu_khoa = tu_khoa,
                    query = query,
                    url_cuoi = url_cuoi
                }));

            return re.Result == Result.Updated;
        }

        public bool UpdateTrangThai(string id, int trang_thai)
        {
            var re = client.Update<TuKhoa, object>(id, u => u.Doc(new { trang_thai = trang_thai }));
            return re.Result == Result.Updated;
        }

        public bool UpdateTrangThaiTkTheoChienDich(string id, int trang_thai)
        {
            var re = client.Update<TuKhoa, object>(id, u => u.Doc(new { trang_thai_chien_dich = trang_thai }));

            return re.Result == Result.Updated;
        }

        public bool UpdateTrangThaiTkTheoQuangCao(string id, int trang_thai)
        {
            var re = client.Update<TuKhoa, object>(id, u => u.Doc(new { trang_thai_quang_cao = trang_thai }));

            return re.Result == Result.Updated;
        }

        public bool UpdateDiaDiem(string id, List<string> dia_diem_muc_tieu, List<string> dia_diem_loai_tru)
        {
            var re = client.Update<TuKhoa, object>(id, u => u.Doc(new { dia_diem_muc_tieu = dia_diem_muc_tieu, dia_diem_loai_tru = dia_diem_loai_tru }));
            return re.Result == Result.Updated;
        }

        public IEnumerable<TuKhoa> Overview(List<string> nguoi_tao, string[] view_fields, Dictionary<string, bool> sort_fields, int top = 10)
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
            var re = client.Search<TuKhoa>(request);
            return re.Hits.Select(HitToDocument<TuKhoa>);
        }

        public void UpdateTrangThaiByChienDich(string id_chien_dich, int trang_thai)
        {
            var re = client.UpdateByQuery<TuKhoa>(u => u.Query(q => q.Term(t => t.Field("id_chien_dich.keyword")
                    .Value(id_chien_dich)))
                .Script(sc => sc.Source("ctx._source.trang_thai_chien_dich=params.trang_thai")
                    .Params(new Dictionary<string, object>()
                    {
                        {
                            "trang_thai", trang_thai
                        }
                    })));
        }

        public void UpdateTrangThaiByQuangCao(string id_quang_cao, int trang_thai)
        {
            var re = client.UpdateByQuery<TuKhoa>(u => u.Query(q => q.Term(t => t.Field("id_quang_cao.keyword")
                    .Value(id_quang_cao)))
                .Script(sc => sc.Source("ctx._source.trang_thai_quang_cao=params.trang_thai")
                    .Params(new Dictionary<string, object>()
                    {
                        {
                            "trang_thai", trang_thai
                        }
                    })));
        }

        public void UpdateTrangThaiDoHetNganSachTheoChienDich(List<string> ids, int trang_thai)
        {
            var re = client.UpdateByQuery<TuKhoa>(u => u.Query(q => q.Terms(i => i.Field("id_chien_dich.keyword").Terms(ids)))
                .Script(sc => sc.Source("ctx._source.trang_thai_chien_dich=params.trang_thai")
                    .Params(new Dictionary<string, object>()
                    {
                        {
                            "trang_thai", trang_thai
                        }
                    })));
        }
        public List<TuKhoa> GetTuKhoaTheoChienDich(IEnumerable<string> ids_chien_dich)
        {
            List<TuKhoa> lst_tu_khoa = new List<TuKhoa>();
            List<QueryContainer> must = new List<QueryContainer>();
            must.Add(new TermsQuery()
            {
                Field = "id_chien_dich.keyword",
                Terms = ids_chien_dich
            });
            must.Add(new TermQuery()
            {
                Field = "trang_thai",
                Value = TrangThaiTuKhoa.BAT
            });
            must.Add(new TermQuery()
            {
                Field = "trang_thai_quang_cao",
                Value = TrangThaiQuangCao.BAT
            });
            must.Add(new TermQuery()
            {
                Field = "trang_thai_chien_dich",
                Value = TrangThaiChienDich.BAT
            });
            var source = new SourceFilter()
            {
                Includes = new string[] { "tu_khoa", "kieu_doi_sanh", "id_chien_dich" }
            };
            lst_tu_khoa = GetObjectScroll<TuKhoa>(_DefaultIndex, new QueryContainer(new BoolQuery() { Must = must }), source).ToList();
            return lst_tu_khoa;
        }
    }
}