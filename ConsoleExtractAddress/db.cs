using cSourceData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ConsoleExtractAddress
{
    internal class db
    {
        public static void insertBulkToImportDB(string tbName, DataTable dt)
        {
            bool _b = sql.ExecuteData_BulkCopy(tbName, dt); 
        }
        public static DataTable getDataProvince()
        {
            string _sql = @" ";
             
            DataTable dt = sql.GetDataTable(_sql);
            return dt;
        }
        public static DataTable getYearCreateAddress()
        {
            string _sql = @" ";
             
            DataTable dt = sql.GetDataTable(_sql);
            return dt;
        }
        public static DataTable getDataAddress(object year)
        {
            string _sql = @" "; 
             
            DataTable dt = sql.GetDataTable(_sql);
            return dt;
        }

        public static void clearExistsDataIn_TbExtract(DataTable dt)
        {
            string _tbName = "tempID";
            string _chk = string.Empty;

            string _sql = string.Format( 
            @"  if exists( select top 1 id from tempAddress )
                begin
                    if object_id('{0}') is null      
                        create table {0} (ID int)
                    else
                        delete from {0}
                    select 1
                end
                else select 0 "
            , _tbName);
            using (DataTable _dtCheck = sql.GetDataTable(_sql))
            {
                if (_dtCheck != null && _dtCheck.Rows.Count > 0)
                {
                    _chk = Convert.ToString(_dtCheck.Rows[0][0]);
                }
            }

            if (!string.IsNullOrEmpty(_chk))
            {
                if (_chk == "1")
                {
                    bool _b = sql.ExecuteData_BulkCopy(_tbName, dt, new List<string[]> { new string[] { "ID", "ID" } });

                    _sql = string.Format(
                    @"  delete addr
                        from tempAddress addr with(nolock)
	                        inner join {0} tmp on ext.ID = tmp.ID"
                    , _tbName);
                    _b = sql.ExecuteData(_sql); 
                }
            }
        }
    }
}
