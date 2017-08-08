using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;



namespace cExtractAddr
{
    public class AddressManager
    {
        private List<models.Provinces> _liProvince;
         
        public AddressManager(DataTable dtProvince)
        {
            initListProvince(dtProvince);
        }

        private DataTable initDataTable()
        {
            DataTable _dt = new DataTable();

            _dt.Columns.Add("AddressID", typeof(int));

            _dt.Columns.Add("FullAddress", typeof(string));
            
            _dt.Columns.Add("AddressNo", typeof(string));
            _dt.Columns.Add("Moo", typeof(string));
            _dt.Columns.Add("Place", typeof(string));
            _dt.Columns.Add("Floor", typeof(string));
            _dt.Columns.Add("Soi", typeof(string));
            _dt.Columns.Add("Road", typeof(string));
            _dt.Columns.Add("Tambon", typeof(string));
            
            _dt.Columns.Add("Amphur", typeof(string));
            _dt.Columns.Add("Province", typeof(string));
            _dt.Columns.Add("ProvinceCode", typeof(string));
            _dt.Columns.Add("PostCode", typeof(string));

            _dt.Columns.Add("isEngAddress", typeof(bool));
            _dt.Columns.Add("Lots", typeof(string));

            return _dt;
        }
        private void initListProvince(DataTable dtProvince)
        {
            _liProvince = dtProvince.AsEnumerable()
                                .Select(o => new
                                {
                                    ProvinceName = o.Field<string>("ProvinceName"),
                                    ProvinceCode = o.Field<string>("ProvinceCode")
                                })
                                .Distinct()
                                .Select(p => new models.Provinces
                                {
                                    ProvinceName = p.ProvinceName,
                                    ProvinceCode = p.ProvinceCode
                                })
                                .ToList();

            for (int i = 0; i < _liProvince.Count; i++)
            {
                var _liDistrict = dtProvince.AsEnumerable()
                                        .Where(o => o.Field<string>("ProvinceCode") == _liProvince[i].ProvinceCode)
                                        .Select(o => new
                                        {
                                            DistrictName = o.Field<string>("District")
                                        })
                                        .Distinct()
                                        .Select(p => new models.Districts
                                        {
                                            District = p.DistrictName
                                        })
                                        .ToList();

                if (_liDistrict.Count > 0)
                {
                    for (int j = 0; j < _liDistrict.Count; j++)
                    {
                        var _liSub = dtProvince.AsEnumerable()
                                            .Where(o => o.Field<string>("ProvinceCode") == _liProvince[i].ProvinceCode 
                                                     && o.Field<string>("District") == _liDistrict[j].District)
                                            .Select(p => new models.SubDistricts
                                            {
                                                SubDistrict = p.Field<string>("SubDistrict")
                                            })
                                            .ToList();

                        _liDistrict[j].liSubDistrict = _liSub;
                    }
                }

                _liProvince[i].liDistrict = _liDistrict;
            }
        }

        public DataTable eXtract(DataTable dtMfcAddress, string lots = "")
        {
            DataTable _dt = initDataTable();
            DataRow _dr;
            string _addr = string.Empty;
            string[] _province;
            foreach (DataRow _drMfc in dtMfcAddress.Rows)
            {
                _addr = Convert.ToString(_drMfc["address"]).manageStrAddress();
                if (_addr.isNotEmpty())
                {
                    _dr = _dt.NewRow();

                    _dr["Lots"] = lots;
                    _dr["AddressID"] = _drMfc["ID"];                    
                    _dr["PostCode"] = _drMfc["PostCode"];                    
                    _dr["FullAddress"] = _addr;
                    if (_addr.isAddressEng())
                    {
                        _province = extractProvince_Eng(_addr);
                        _dr["Province"] = _province[0];
                        _dr["ProvinceCode"] = _province[1];

                        _dr["AddressNo"] = extractNo_Eng(_addr);

                        _dr["isEngAddress"] = true;
                    }
                    else
                    {
                        _province = extractProvince(ref _addr);
                        _dr["Province"] = _province[0];
                        _dr["ProvinceCode"] = _province[1];

                        _dr["Amphur"] = extractAmphor(ref _addr, _province[0], _province[1]);
                        _dr["Tambon"] = extractTambon(ref _addr, Convert.ToString(_dr["Amphur"]), _province[1]);

                        _dr["Floor"] = extractFloor(ref _addr);
                        _dr["Road"] = extractRoad(ref _addr);
                        _dr["Soi"] = extractSoi(ref _addr);
                        _dr["Moo"] = extractMoo(ref _addr);
                        _dr["AddressNo"] = extractNo(ref _addr);
                        _dr["Place"] = _addr;

                        _dr["isEngAddress"] = false;
                    }


                    _dt.Rows.Add(_dr);
                }
            }

            return _dt;
        }

        public string manageStrAddress(string addr)
        {
            return addr.manageStrAddress(); 
        }
        public bool isAddressEng(string addr)
        {
            bool b = addr.isAddressEng();
            return b;
        }
                
        public string[] extractProvince(ref string addr)
        {
            string _province = string.Empty;
            string _provinceCode = string.Empty;
            string _rawProvince = string.Empty;

            string _pattern = "( กรุงเทพมหานคร| กรุงเทพ[ ]?ฯ?| กทม\\.?| ก\\.ท\\.ม\\.?)|(กรุงเทพ[ ]?ฯ?|กทม\\.?|ก\\.ท\\.ม\\.?| bangkok\\.?| bkk\\.?)$|(( จังหวัด[ ]?| จ\\.)[\\S]{1,})|((จ\\.)[\\S]{1,})$";  
            MatchCollection _match = Regex.Matches(addr, _pattern);
            if (_match.Count > 0)
            {
                _province = _match[_match.Count - 1].Value;
            }

            if (_province.isNotEmpty())
            {
                _rawProvince = _province; // #for remove Province in address

                _pattern = "กรุงเทพมหานคร|กรุงเทพ[ ]?ฯ?|กทม\\.?|ก\\.ท\\.ม\\.?|bangkok\\.?|bkk\\.?"; 
                if (Regex.IsMatch(_province, _pattern))
                {
                    _province = "กรุงเทพมหานคร";
                }
                else
                {
                    _province = Regex.Replace(_province, "จังหวัด|จ\\.", "").Trim(); 
                }

                _province = searchProvinceInDB(_province); 
            }
            
            // #case : บ.สยามโต๊โต อุตสาหะ จ.ก. 69/96 นิคมอมตะนคร อ.พานทอง ชลบุรี => จ.ก. : (searchInDB) => empty
            if (_province.isEmpty())
            {
                // #get Last word for check in database 
                _rawProvince = addr.getLastSplitBySpace();

                _province = searchProvinceInDB(_rawProvince);               
            }

            if (_province.isNotEmpty())
            {
                _provinceCode = getProvinceCode(_province);

                // #remove Province in address 
                addr = addr.removeWordInString(_rawProvince, true);
            }

            return new string[] 
            { 
                _province, 
                _provinceCode 
            };
        }
        private string searchProvinceInDB(string province)
        {
            string _province = string.Empty;
            models.Provinces _provinceList = _liProvince.Where(p => p.ProvinceName == province).FirstOrDefault();
            if (_provinceList != null)
            {
                _province = _provinceList.ProvinceName; 
            }
            else
            {
                string _tmp = string.Empty;
                List<models.Provinces> _li;
                int _len = province.Length;
                if (_len > 1)
                {
                    // #forward search
                    for (int i = 2; i <= _len; i++)
                    {
                        _tmp = province.Substring(0, i);
                        _li = _liProvince.Where(p => p.ProvinceName.StartsWith(_tmp)).ToList();
                        if (_li.Count == 1)
                        {
                            _province = _li[0].ProvinceName;
                            break;
                        }
                    }

                    if (_province.isEmpty())
                    {                    
                        // #backward search
                        for (int i = _len - 2; i > 0; i--)
                        {
                            _tmp = province.Substring(i);
                            _li = _liProvince.Where(p => p.ProvinceName.EndsWith(_tmp)).ToList();
                            if (_li.Count == 1)
                            {
                                _province = _li[0].ProvinceName;
                                break;
                            }
                        }
                    }
                }
            }

            return _province;
        }
        private string getProvinceCode(string province)
        {
            string _province = string.Empty;
            models.Provinces _provinceList = _liProvince.Where(p => p.ProvinceName == province).FirstOrDefault();
            if (_provinceList != null)
            {
                _province = _provinceList.ProvinceCode;
            }
            return _province;
        }

        public string extractAmphor(ref string addr, string provinceName , string provinceCode)
        {
            string _amphor = string.Empty;
            string _rawAmphor = string.Empty;
            
            string _pattern = "(เขต |อำเภอ |กิ่งอำเภอ )[\\S]{1,}"; // #case : อำเภอ เมือง
            addr =  addr.removeSpaceBetweenWords(_pattern); 
                    
            _pattern = "(เขต|อำเภอ|กิ่งอำเภอ|อ\\.| อ )[\\S]{1,}";
            MatchCollection _match = Regex.Matches(addr, _pattern);
            if (_match.Count > 0)
            {
                _amphor = _match[_match.Count - 1].Value;
            }
            
            if (_amphor.isNotEmpty())
            {
                _rawAmphor = _amphor;
                if (_amphor.StartsWith("เขต/เขต"))
                {
                    addr = addr.Replace("เขต/เขต", "เขต/แขวง");

                    _rawAmphor = _rawAmphor.Replace("/เขต", "/แขวง");
                    _amphor = _amphor.Replace("เขต/เขต", "").Trim();
                }
                else if (_amphor.StartsWith("เขต")) // #prevent replace case : อ.สนามชัยเขต
                {
                    _amphor = _amphor.Substring(3).Trim();
                }
                else
                {
                    _pattern = "อำเภอ|กิ่งอำเภอ|อ |อ\\.";
                    _amphor = Regex.Replace(_amphor, _pattern, "").Trim();
                }
            }
            else
            {
                // #get Last word 
                _rawAmphor = _amphor = addr.getLastSplitBySpace(); 
                if (_amphor.StartsWith("แขวง/แขวง"))
                {
                    addr = addr.Replace("แขวง/แขวง", "เขต/แขวง");

                    _rawAmphor = _rawAmphor.Replace("แขวง/", "เขต/");
                    _amphor = _amphor.Replace("แขวง/แขวง", "").Trim();
                }
            }

            if (_amphor == "เมือง")
            {
                _amphor += provinceName;
            }

            _amphor = searchAmphorInDB(_amphor, provinceCode);
            if (_amphor.isNotEmpty())
            {
                // #case district and subdistrict name same
                if (_rawAmphor.StartsWith("เขต/แขวง"))
                {
                    _rawAmphor = "เขต/";
                }
                
                // #remove Amphor in address 
                addr = addr.removeWordInString(_rawAmphor, true);
            }

            return _amphor;
        }
        private string searchAmphorInDB(string amphor, string provinceCode)
        {
            string _district = string.Empty;
            if (provinceCode.isNotEmpty())
            {
                models.Provinces _provinceList = _liProvince.Where(p => p.ProvinceCode == provinceCode).FirstOrDefault();
                if (_provinceList != null)
                {
                    _district = _provinceList.liDistrict
                                    .Where(d => d.District == amphor)
                                    .Select(d => d.District)
                                    .FirstOrDefault();

                    int _len = amphor.Length;
                    if (_district.isEmpty() && _len > 1)
                    {
                        _district = string.Empty;
                        string _tmp = string.Empty;
                        List<string> _li;
                        
                        // #forward search
                        for (int i = 2; i <= _len; i++)
                        {
                            _tmp = amphor.Substring(0, i);
                            _li = _provinceList.liDistrict
                                            .Where(p => p.District.StartsWith(_tmp))
                                            .Select(d => d.District)
                                            .ToList();

                            if (_li.Count == 1)
                            {
                                _district = _li[0]; 
                            }
                        }

                        if (_district.isEmpty())
                        {
                            // #backward search
                            for (int i = _len - 2; i > 0; i--)
                            {
                                _tmp = amphor.Substring(i);
                                _li = _provinceList.liDistrict
                                                .Where(p => p.District.EndsWith(_tmp))
                                                .Select(d => d.District)
                                                .ToList();

                                if (_li.Count == 1)
                                {
                                    _district = _li[0];  
                                }
                            }
                        }
                    }
                }
            }
            return _district;
        }

        public string extractTambon(ref string addr, string amphor, string provinceCode)
        {
            string _tambon = string.Empty;
            string _rawTambon = string.Empty;

            string _pattern = "(แขวง |ตำบล )[\\S]{1,}"; // #case : ตำบล คลองสาม
            addr = addr.removeSpaceBetweenWords(_pattern);

            _pattern = "(แขวง|ตำบล|ต\\.| ต )[\\S]{1,}"; 
            MatchCollection _match = Regex.Matches(addr, _pattern);
            if (_match.Count > 0)
            {
                _tambon = _match[_match.Count - 1].Value.Trim();
            }

            if (_tambon.isNotEmpty())
            {
                _rawTambon = _tambon;

                if (Regex.IsMatch(_tambon, "(แขวง|ตำบล|ต\\.)/")) // #case district and subdistrict name same
                {
                    _tambon = amphor;
                } 
                else if (_tambon.StartsWith("แขวง") || _tambon.StartsWith("ตำบล")) // #prevent replace case : ต.สามตำบล
                {
                    _tambon = _tambon.Substring(4).Trim();
                }
                else if (_tambon.StartsWith("ต.") || _tambon.StartsWith("ต "))
                {
                    _tambon = _tambon.Substring(2).Trim();
                } 
            }
            else
            {
                // #get Last word to check in Database
                _rawTambon = _tambon = addr.getLastSplitBySpace();
            }

            _tambon = searchTambonInDB(_tambon, amphor, provinceCode);
            if (_tambon.isNotEmpty())
            {
                // #remove Tambon in address 
                addr = addr.removeWordInString(_rawTambon, true);
            }

            return _tambon;
        }
        private string searchTambonInDB(string tambon, string amphor, string provinceCode)
        {
            string _subDistrict = string.Empty;
            if (provinceCode.isNotEmpty() && amphor.isNotEmpty())
            {
                List<models.SubDistricts> _liSubDistrict = _liProvince.Where(p => p.ProvinceCode == provinceCode)
                                                                    .SelectMany(o => o.liDistrict)
                                                                    .Where(d => d.District == amphor)
                                                                    .Select(d => d.liSubDistrict)
                                                                    .FirstOrDefault();

                if (_liSubDistrict != null)
                {
                    _subDistrict = _liSubDistrict
                                        .Where(s => s.SubDistrict == tambon)
                                        .Select(s => s.SubDistrict)
                                        .FirstOrDefault();

                    int _len = tambon.Length;
                    if (_subDistrict.isEmpty() && _len > 1)
                    {
                        _subDistrict = string.Empty;
                        string _tmp = string.Empty;
                        List<string> _li;
                        
                        // #forward search
                        for (int i = 2; i <= tambon.Length; i++)
                        {
                            _tmp = tambon.Substring(0, i);
                            _li = _liSubDistrict
                                        .Where(p => p.SubDistrict.StartsWith(_tmp))
                                        .Select(d => d.SubDistrict)
                                        .ToList();

                            if (_li.Count == 1)
                            {
                                _subDistrict = _li[0];
                            }
                        }

                        if (_subDistrict.isEmpty())
                        {
                            // #backward search
                            for (int i = _len - 2; i > 0; i--)
                            {
                                _tmp = tambon.Substring(i);
                                _li = _liSubDistrict
                                            .Where(p => p.SubDistrict.EndsWith(_tmp))
                                            .Select(d => d.SubDistrict)
                                            .ToList();

                                if (_li.Count == 1)
                                {
                                    _subDistrict = _li[0];
                                }
                            }
                        }
                    }
                }

            }
            return _subDistrict;
        }

        public string extractRoad(ref string addr)
        {
            string _road = string.Empty;
            string _rawRoad = string.Empty;
            int _index = -1;

            string _pattern = "(ถนน )[\\S]{1,}"; 
            addr = addr.removeSpaceBetweenWords(_pattern);

            _pattern = "( ถนน|ถ\\.)";
            MatchCollection _match = Regex.Matches(addr, _pattern);
            if (_match.Count > 0)
            {
                _index = _match[_match.Count - 1].Index;
                _road = _match[_match.Count - 1].Value;
            }
            
            if (_road.isNotEmpty())
            {
                _road = addr.Substring(_index);
                extractSoi(ref _road);

                _rawRoad = _road;

                _road = Regex.Replace(_road, _pattern, "");
            }


            // #remove Road in address 
            addr = addr.removeWordInString(_rawRoad);


            return _road;
        }
        public string extractSoi(ref string addr)
        {
            string _soi = string.Empty;
            string _rawSoi = string.Empty;

            string _pattern = "(ซอย )[\\S]{1,}";
            addr = addr.removeSpaceBetweenWords(_pattern);

            _pattern = "( ซอย|ซ\\.)";
            Match _match = Regex.Match(addr, _pattern);
            int _index = _match.Index;

            _soi = _match.Value;
            if (_soi.isNotEmpty())
            {
                _soi = addr.Substring(_index);
                _rawSoi = _soi;

                Regex rgx = new Regex(_pattern);
                _soi = rgx.Replace(_soi, "", 1); 
            }


            addr = addr.removeWordInString(_rawSoi);

            return _soi;
        }
        public string extractFloor(ref string addr)
        {
            string _floor = string.Empty;

            string _pattern = "((และ)?ชั้น(ที่)?[ ]?)[\\dA-Z]+\\w*([ ]?(และ|[,\\-])[ ]?\\d+)*([ ]?(\\(?(โซน|ส่วน)[\\p{L}\\p{Mn}\\-,]+\\)?|โซน \\w+))?";
            MatchCollection _matches = Regex.Matches(addr, _pattern);
            if (_matches.Count > 0)
            {
                _pattern = "(และ)?ชั้น(ที่)?"; // #pattern for replace
                string _tmp, _tmp2;
                foreach (Match _m in _matches)
                {
                    _tmp = _m.Value;
                    if (_tmp.isNotEmpty())
                    {
                        _tmp2 = Regex.Replace(_tmp, _pattern, string.Empty);
                        _tmp2 = _tmp2.Replace("และ", ",");
                        _floor += string.Format(",{0}", _tmp2.Trim());

                        addr = addr.removeWordInString(_tmp);  
                    }
                }

            }

            if (_floor.isNotEmpty())
            {
                _floor = _floor.Replace(",,", ",");
                _floor = _floor.Substring(1);
            }

            return _floor;
        }
        public string extractMoo(ref string addr)
        {
            string _moo = string.Empty;

            string _pattern = "(หมู่(ที่)? )[\\S]{1,}";
            addr = addr.removeSpaceBetweenWords(_pattern);

            _pattern = "(หมู่ที่|หมู่|ม\\.)[0-9]{1,}";
            Match _match = Regex.Match(addr, _pattern);

            _moo = _match.Value;
            if (_moo.isNotEmpty())
            {
                addr = addr.removeWordInString(_moo);

                _pattern = "หมู่ที่|หมู่|ม\\.";
                _moo = Regex.Replace(_moo, _pattern, "");
            }

            return _moo;
        }
        public string extractNo(ref string addr)
        {
            string _no = string.Empty;

            string _pattern = "(เลขที่ )[\\d]+([ ]?[0-9,./\\-\\(\\)]+)*";
            addr = addr.removeSpaceBetweenWords(_pattern);

            _pattern = "เลขที่[\\d]+([0-9,./\\-\\(\\)]+)*";
            Match _match = Regex.Match(addr, _pattern);

            _no = _match.Value;
            if (_no.isNotEmpty())
            {
                addr = addr.removeWordInString(_no);

                _no = _no.Replace("เลขที่", "");
            }
            else
            {
                _pattern = "[0-9]+([ ]?[0-9,./\\-\\(\\)]+)*";
                addr = addr.removeSpaceBetweenWords(_pattern);

                int _n = -1;
                string[] _arr = addr.Split(' ');
                if (_arr[0].isAddressNo())
                {
                    _n = 0;
                    _no = _arr[0];
                }
                else
                {
                    List<string[]> _liAddressNo = new List<string[]>();
                    for (int i = 0; i < _arr.Length; i++)
                    {
                        if (_arr[i].isAddressNo())
                        {
                            _n = i;
                            _liAddressNo.Add(new string[] { _n.ToString(), _arr[i] });
                        }
                    }

                    if (_liAddressNo.Count > 0)
                    {
                        // #check เอาชุดตัวเลขที่มีตัวอักขระพิเศษ [ / , - ( ) ]
                        var _liTmp = _liAddressNo.Where(o => o[1].isAddressNoSpecialChar()).ToList();
                        if (_liTmp.Count > 0)
                        {
                            _n = Convert.ToInt32(_liTmp[0][0]);
                            _no = _liTmp[0][1];
                        }
                        else
                        {
                            // #เอาเลขที่ชุดแรกที่เจอ
                            _n = Convert.ToInt32(_liAddressNo[0][0]);
                            _no = _liAddressNo[0][1];
                        }
                    }
                }

                // #remove word in string address
                if (_n > -1)
                {
                    _arr = _arr.Where((val, i) => i != _n).ToArray();
                    addr = string.Join(" ", _arr);
                }
            }

            return _no;
        }


        public string[] extractProvince_Eng(string addr)
        {
            string _province = null;
            string _provinceCode = null; 

            string _pattern = "( bangkok| bkk)\\.?$";
            Match _match = Regex.Match(addr, _pattern, RegexOptions.IgnoreCase);
            if (Regex.IsMatch(addr, _pattern, RegexOptions.IgnoreCase))
            { 
                _province = searchProvinceInDB("กรุงเทพมหานคร");
                _provinceCode = getProvinceCode(_province);
            } 

            return new string[] 
            { 
                _province, 
                _provinceCode 
            };
        }
        public string extractNo_Eng(string addr)
        {
            string _no = null;
            string _tmp = addr.Split(' ').First();
            if (_tmp.isAddressNo())
            {
                _no = _tmp;
            }
            return _no;
        }
    }
}
