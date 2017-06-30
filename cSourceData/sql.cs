using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Configuration;

namespace cSourceData
{
    public class sql
    {
        #region " DATA "

        public static String GetConnStr 
        {
            get
            {
                try
                { 
                    string tConnStr = ConfigurationManager.ConnectionStrings["mainConnection"].ToString();
                    return tConnStr;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static Boolean ExecuteData(string queryString)
        {
            SqlTransaction oDB_trn;
            SqlCommand oDB_Cmd = new SqlCommand();
            SqlConnection oDB_Conn = new SqlConnection(GetConnStr);

            if (queryString == "") return false;

            if (oDB_Conn.State == ConnectionState.Closed) oDB_Conn.Open();

            oDB_Cmd.Connection = oDB_Conn;
            oDB_Cmd.CommandType = CommandType.Text;
            oDB_Cmd.CommandText = queryString;
            oDB_trn = oDB_Conn.BeginTransaction();
            oDB_Cmd.Transaction = oDB_trn;

            try
            {
                oDB_Cmd.ExecuteNonQuery();
                oDB_trn.Commit();
                oDB_Conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                oDB_trn.Rollback();
                oDB_Conn.Close();

                throw ex;
                //return false;
            }
        }

        public static string ExecuteData_ReturnString(string queryString)
        {
            SqlTransaction oDB_trn;
            SqlCommand oDB_Cmd = new SqlCommand();
            SqlConnection oDB_Conn = new SqlConnection(GetConnStr);

            if (queryString == "") return "";

            if (oDB_Conn.State == ConnectionState.Closed) oDB_Conn.Open();

            oDB_Cmd.Connection = oDB_Conn;
            oDB_Cmd.CommandType = CommandType.Text;
            oDB_Cmd.CommandText = queryString;
            oDB_trn = oDB_Conn.BeginTransaction();
            oDB_Cmd.Transaction = oDB_trn;

            try
            {
                oDB_Cmd.ExecuteNonQuery();
                oDB_trn.Commit();
                oDB_Conn.Close();
                return "";
            }
            catch (Exception ex)
            {
                oDB_trn.Rollback();
                oDB_Conn.Close();

                return ex.Message;
            }
        }

        public static Boolean ExecuteData(string queryString, SqlCommand Comm)
        {
            SqlTransaction oDB_trn;
            SqlConnection oDB_Conn = new SqlConnection(GetConnStr);

            if (queryString == "") return false;

            if (oDB_Conn.State == ConnectionState.Closed) oDB_Conn.Open();

            Comm.Connection = oDB_Conn;
            Comm.CommandType = CommandType.Text;
            Comm.CommandText = queryString;
            oDB_trn = oDB_Conn.BeginTransaction();
            Comm.Transaction = oDB_trn;
            try
            {
                Comm.ExecuteNonQuery();
                oDB_trn.Commit();
                oDB_Conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                oDB_trn.Rollback();
                oDB_Conn.Close();

                throw ex;
                //return false;
            }

        }

        public static Boolean ExecuteData(string queryString, SqlCommand Comm, SqlConnection Conn)
        {
            SqlTransaction oDB_trn;

            if (queryString == "") return false;

            Comm.Connection = Conn;
            Comm.CommandType = CommandType.Text;
            Comm.CommandText = queryString;
            oDB_trn = Conn.BeginTransaction();
            Comm.Transaction = oDB_trn;

            try
            {
                Comm.ExecuteNonQuery();
                oDB_trn.Commit();
                Conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                oDB_trn.Rollback();
                Conn.Close();

                throw ex;
                //return false;
            }
        }

        public static Boolean ExecuteData_Store(string storeName, SqlCommand Comm)
        {
            SqlTransaction oDB_trn;
            SqlConnection oDB_Conn = new SqlConnection(GetConnStr);

            if (storeName == "") return false;

            if (oDB_Conn.State == ConnectionState.Closed) oDB_Conn.Open();

            Comm.Connection = oDB_Conn;
            Comm.CommandType = CommandType.StoredProcedure;
            Comm.CommandText = storeName;
            oDB_trn = oDB_Conn.BeginTransaction();
            Comm.Transaction = oDB_trn;

            try
            {
                Comm.ExecuteNonQuery();
                oDB_trn.Commit();
                oDB_Conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                oDB_trn.Rollback();
                oDB_Conn.Close();
                
                throw ex;
                //return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dt"></param>
        /// <param name="liColumnMapping"> 
        /// <para>string[0] => sourceColumn</para> 
        /// <para>string[1] => destinationColumn</para> 
        /// <para>if null : Mapping Column by DataTable ColumnName</para>
        /// </param>
        /// <returns></returns>
        public static bool ExecuteData_BulkCopy(string tableName, DataTable dt, List<string[]> liColumnMapping = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnStr))
                {
                    if (connection.State == ConnectionState.Closed) connection.Open();

                    //using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction, null))
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        if (liColumnMapping == null)
                        {
                            foreach (DataColumn dc in dt.Columns)
                            {
                                bulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                            }
                        }
                        else
                        {
                            foreach (string[] col in liColumnMapping)
                            {
                                bulkCopy.ColumnMappings.Add(col[0], col[1]);
                            }
                        }

                        bulkCopy.BatchSize = 10000;
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.WriteToServer(dt);
                    }                    
                    connection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataSet GetData(String queryString)
        {
            String connectionString = GetConnStr;
            SqlConnection connection = new SqlConnection(connectionString);
            return GetData(queryString, new SqlCommand(), connection); 
        }
        public static DataSet GetData(String queryString, SqlCommand Comm)
        {
            String connectionString = GetConnStr;
            SqlConnection connection = new SqlConnection(connectionString);

            return GetData(queryString, Comm, connection); 
        }
        public static DataSet GetData(String queryString, SqlCommand Comm, String connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            return GetData(queryString, Comm, connection); 
        }
        public static DataSet GetData(String queryString, SqlCommand Comm, SqlConnection Conn)
        {
            try
            {                
                if (Conn.State == ConnectionState.Closed) Conn.Open();

                Comm.Connection = Conn;
                Comm.CommandText = queryString;

                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = Comm;
                adapter.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Conn.Close();
            }
        }

        public static DataTable GetDataTable(String queryString)
        {
            String connectionString = GetConnStr;
            SqlConnection connection = new SqlConnection(connectionString);
            return GetDataTable(queryString, new SqlCommand(), connection);
        }
        public static DataTable GetDataTable(String queryString, SqlCommand Comm)
        {
            String connectionString = GetConnStr;
            SqlConnection connection = new SqlConnection(connectionString);

            return GetDataTable(queryString, Comm, connection);
        }
        public static DataTable GetDataTable(string queryString, SqlCommand Comm, String connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            return GetDataTable(queryString, Comm, connection);
        }
        public static DataTable GetDataTable(string queryString, SqlCommand Comm, SqlConnection Conn)
        {
            try
            {
                if (Conn.State == ConnectionState.Closed) Conn.Open();

                Comm.Connection = Conn;
                Comm.CommandText = queryString;

                DataTable dt = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = Comm;
                adapter.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Conn.Close();
            }
        }

        public static DataSet GetData_Store(String StoredProcedureName, SqlCommand Comm)
        {
            String connectionString = GetConnStr;
            SqlConnection connection = new SqlConnection(connectionString);
            
            try
            {
                if (connection.State == ConnectionState.Closed) connection.Open();

                Comm.Connection = connection;
                Comm.CommandText = StoredProcedureName;
                Comm.CommandType = CommandType.StoredProcedure;
                Comm.CommandTimeout = 360;

                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = Comm;
                adapter.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        
        public static String ReadSetting(String key, String defaultValue)
        {
            try
            {
                object setting = ConfigurationManager.AppSettings[key];
                if (setting != null)
                {
                    if (setting.ToString() == "")
                    {
                        setting = null;
                    }
                }
                return (setting == null) ? defaultValue : (String)setting;
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion " DATA "

        #region " CheckNull "

        public static object CheckNull(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return DBNull.Value;
                }
                return obj;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion " CheckNull "
    }
}
