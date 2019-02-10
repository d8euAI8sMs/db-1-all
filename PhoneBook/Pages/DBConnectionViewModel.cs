using FirstFloor.ModernUI.Presentation;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

namespace PhoneBook.Pages
{

    class DBConnectionViewModel
        : NotifyPropertyChanged
    {
        string connectionString = "Data Source=;Database=KIS;User ID=sa;Password=sa;Connection Timeout=3;";
        string createSchemaString;
        string sqlString;

        string message = "";

        public DBConnectionViewModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "PhoneBook.create_schema.sql";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                createSchemaString = reader.ReadToEnd();
            }
            sqlString = createSchemaString;
        }

        public string ConnectionString
        {
            get { return connectionString; }
            set { this.connectionString = value; OnPropertyChanged("DataSource"); }
        }
        public string SqlString
        {
            get { return sqlString; }
            set { this.sqlString = value; OnPropertyChanged("DataSource"); }
        }

        public string Message
        {
            get { return message; }
            set { this.message = value; OnPropertyChanged("Message"); }
        }

        public SqlConnection NewConnection() {
            try
            {
                return new SqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                throw ex;
            }
        }

        public bool Test()
        {
            bool success = true;
            SqlConnection connection;
            try
            {
                 connection = NewConnection();
            } catch (Exception ex)
            {
                Message = ex.Message;
                return false;
            }
            try
            {
                connection.Open();
                Message = "Connected";
            }
            catch (SqlException ex)
            {
                Message = ex.Message;
                success = false;
            }
            try
            {
                connection.Close();
            }
            catch (SqlException ex)
            {
            }
            return success;
        }

        public void Apply()
        {
            MainModel.Instance.Connection = connectionString;
        }

        public void CreateSchema()
        {
            try
            {
                MainModel.Instance.RunSql(createSchemaString);
            } catch(Exception ex)
            {
                Message = ex.Message;
            }
        }

        public void RunSql()
        {
            try
            {
                MainModel.Instance.RunSql(sqlString);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }
    }
}
