using System;
using System.Reflection;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;


namespace CAMicroORM
{
    public static class MicroORM
    {

        public static IEnumerable<T> MapObjects<T>(this SqlConnection cnsql, string strsql) where T : new()
        {
           
                if (cnsql.State == ConnectionState.Closed) cnsql.Open();
                var cmsql = cnsql.CreateCommand();
                cmsql.CommandText = strsql;
                var Props = typeof(T).GetProperties().ToList();
                using (SqlDataAdapter da = new SqlDataAdapter(strsql, cnsql))
                {
                    using (DataTable dt = new DataTable())
                    {
                        da.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            var CurrentModelToReturn = new T();
                            Props.ForEach(currentProperty => {
                                try
                                {
                                    currentProperty.SetValue(CurrentModelToReturn, dr[currentProperty.Name]);
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            });
                            yield return CurrentModelToReturn;
                        }
                    }
                }
            }
           
              
           
          

        public static int InsertObjects<T>(this SqlConnection cnsql,  string TableName = "" , params T[] Given) where T : new()
        {
            try
            {
                var Props = typeof(T).GetProperties().ToList().OrderBy(p => p.Name).ToList();
                var TypeName = typeof(T).Name;
                int AffectedRows = 0;
                var TableColumns = string.Join(", ", Props.Select(p => p.Name));
                var ValuesColumns = string.Join(", ", Props.Select(p => $"@{p.Name}"));
                if (cnsql.State == ConnectionState.Closed) cnsql.Open();                
                SqlCommand cmsql = cnsql.CreateCommand();
                foreach (T CurrentModel in Given)
                {
                    string strsql = $"Insert Into {(TableName == "" ? TypeName : TableName)}  ";
                    strsql += $" ({TableColumns}) ";
                    strsql += $" VALUES ({ValuesColumns}) ";

                    cmsql.CommandText = strsql;
                    Props.ForEach(currentProperty => cmsql.Parameters.AddWithValue(currentProperty.Name, currentProperty.GetValue(CurrentModel)));
                    AffectedRows += cmsql.ExecuteNonQuery();
                    cmsql.Parameters.Clear();
                }
                return AffectedRows;
            }
            catch (Exception)
            {

                throw;
            }        
        }

        public static int UpdateObjects<T>(this SqlConnection cnsql, string PrimaryKeyColumn, string TableName = "", params T[] Given) where T : new()
        {
            try {
                var Props = typeof(T).GetProperties().ToList().OrderBy(p => p.Name).ToList();
                var TypeName = typeof(T).Name;
                var UpdateStatement = string.Join(", ", Props.Where(p => p.Name != PrimaryKeyColumn).Select(p => $"{p.Name} = @{p.Name}"));
                int AffectedRows = 0;
                if (cnsql.State == ConnectionState.Closed) cnsql.Open();
                SqlCommand cmsql = cnsql.CreateCommand();
                foreach (T CurrentModel in Given)
                {
                    string strsql = $"Update {(TableName == "" ? TypeName : TableName)} set  ";
                    strsql += UpdateStatement;
                    strsql += $" where  {PrimaryKeyColumn} = @{PrimaryKeyColumn}";

                    cmsql.CommandText = strsql;
                    Props.ForEach(currentProperty => cmsql.Parameters.AddWithValue(currentProperty.Name, currentProperty.GetValue(CurrentModel)));
                    AffectedRows += cmsql.ExecuteNonQuery();
                    cmsql.Parameters.Clear();
                }
                return AffectedRows;
            }
            catch (Exception)
            {
                throw;
            }
     
        }
        }
    }

