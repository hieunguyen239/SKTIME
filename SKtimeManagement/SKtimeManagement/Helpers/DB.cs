using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;

namespace SKtimeManagement
{
    public class Repo
    {
        internal static ConnectionManager DB
        {
            get
            {
                return new ConnectionManager(SiteConfiguration.DbUser, SiteConfiguration.DbPass, SiteConfiguration.DbServer);
            }
        }
    }
    internal class ConnectionManager
    {
        private SqlConnection Connection { get; set; }
        private readonly string DB_SERVER;
        private readonly string DB_USER;
        private readonly string DB_PASS;
        internal ConnectionManager(string username = "iis", string password = "<]Bo&$8'7s", string server = "10.0.2.44")
        {
            DB_SERVER = server;
            DB_USER = username;
            DB_PASS = password;
        }
        private SqlConnection OpenAndSwitch(string dbname)
        {
            var dbserver = DB_SERVER ?? "10.0.2.44";
            var connection_string = "User ID=" + DB_USER + "; Password=" + DB_PASS + ";Initial Catalog='" + dbname + "'; Data Source=" + dbserver + "; Pooling=True";
            if (SiteConfiguration.UseLocalDb)
                connection_string = String.Format("Server=localhost\\SQLEXPRESS;Database={0};Trusted_Connection=True;", dbname);
            if (Connection == null)
            {
                Connection = new SqlConnection(connection_string);
                Connection.Open();
            }
            else if (Connection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    Connection.Close();
                }
                catch (Exception e)
                {
                    //EmailService.Send(String.Join(";", SiteConfiguration.AdminEmail), "error@tpf.com.au", "SKtime Management Notification - Error", EmailService.RenderException(e));
                }
                Connection.ConnectionString = connection_string;
                Connection.Open();
            }
            else
            {
                Connection.ChangeDatabase(dbname);
            }

            return Connection;
        }
        internal SqlConnection SKtimeManagement
        {
            get
            {
                return OpenAndSwitch("SKtimeManagement");
            }
        }
    }
}