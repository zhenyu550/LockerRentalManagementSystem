using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace LockerRentalManagementSystem
{
    public static class Database
    {
        private static MySqlConnection _connection;
        public static MySqlConnection Connection
        {
            get { return _connection; }
        }

        private static string _databaseName;
        public static string DatabaseName
        {
            get { return _databaseName; }
        }

        public static void Initialize(string server, string port, string uid, string pw, string db)
        {
            _databaseName = db;
            string connString = String.Format("SERVER={0};PORT={1};UID={2};PASSWORD={3};DATABASE={4};SSLMODE=NONE", server, port, uid, pw, db);
            _connection = new MySqlConnection(connString);
        }

        public static bool Connect()
        {
            try
            {
                _connection.Open();
                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        }

        public static bool Disconnect()
        {
            try
            {
                _connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Number.ToString() + " " + ex.Message);
                return false;
            }
        }

        public static void CreateDatabase(string server, string port, string uid, string pw, string db)
        {
            try
            {
                _databaseName = db;
                string connString = String.Format("SERVER={0};PORT={1};UID={2};PASSWORD={3};SSLMODE=NONE", server, port, uid, pw);
                MySqlConnection con = new MySqlConnection(connString);
                con.Open();
                var sql = string.Format("CREATE DATABASE IF NOT EXISTS {0}", db);
                var command = new MySqlCommand(sql, con);
                command.ExecuteNonQuery();
                con.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Number.ToString() + " " + ex.Message);
            }

        }

        public static bool TableExists(string tableName)
        {
            try
            {
                var sql = string.Format("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' " +
                  "AND TABLE_NAME = '{1}'", _databaseName, tableName);
                var command = new MySqlCommand(sql, _connection);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0; 
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Number.ToString() + " " + ex.Message);
                return false;
            }
        }  

        public static void DropTable()
        {
            var sql = "DROP TABLE IF EXISTS RENTAL; DROP TABLE IF EXISTS LOCKER; " +
                     "DROP TABLE IF EXISTS CABINET; DROP TABLE IF EXISTS LOCKER_TYPE; " +
                     "DROP TABLE IF EXISTS CUSTOMER; DROP TABLE IF EXISTS EMPLOYEE; " +
                     "DROP TABLE IF EXISTS RENTAL_STATUS; DROP TABLE IF EXISTS TRANSACTIONS;" +
                     "DROP TABLE IF EXISTS RENTAL_SETTINGS; DROP TABLE IF EXISTS ACCESS_LOG;";
            MySqlCommand command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();
        }

        public static void CreateTable()
        {
            try
            {
                var str = "SET PERSIST information_schema_stats_expiry = 0";
                MySqlCommand cmd = new MySqlCommand(str, _connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {

            }
            _connection.Close();
            _connection.Open();

            //serial is alias for BIGINT UNSIGNED NOT NULL AUTO_INCREMENT UNIQUE
            var sql = "CREATE TABLE EMPLOYEE(id int(10) UNSIGNED NOT NULL AUTO_INCREMENT," +
                "username varchar(25) NOT NULL UNIQUE, password varchar(64) NOT NULL," +
                "permission varchar(10) NOT NULL, name varchar(100) NOT NULL, " +
                "ic varchar(12) NOT NULL UNIQUE, gender varchar(10) NOT NULL, " +
                "position varchar(10) NOT NULL, salary decimal(10, 2) NOT NULL, " +
                "dob date NOT NULL, phone_no varchar(20), hp_no varchar(20) NOT NULL, " +
                "email varchar(100) NOT NULL, home_address varchar(1000) NOT NULL," +
                "status varchar(10) NOT NULL DEFAULT 'Active', PRIMARY KEY(id)) " +
                "ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8;";
            MySqlCommand command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE CUSTOMER (id int(10) UNSIGNED NOT NULL AUTO_INCREMENT, " +
                "name varchar(100) NOT NULL, ic varchar(12) NOT NULL UNIQUE, gender varchar(10) NOT NULL, " +
                "dob date NOT NULL, phone_no varchar(20), hp_no varchar(20) NOT NULL, " +
                "email varchar(100) NOT NULL, home_address varchar(1000) NOT NULL, status varchar(10) NOT NULL DEFAULT 'Active', " +
                "PRIMARY KEY(id)) ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8;";
            command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE LOCKER_TYPE (id int(10) UNSIGNED NOT NULL AUTO_INCREMENT, " +
                "name varchar(20) NOT NULL, code varchar(10) NOT NULL UNIQUE, " +
                "rate decimal(10,2) NOT NULL DEFAULT 0, status varchar(20) NOT NULL DEFAULT 'Active', " +
                "PRIMARY KEY(id)) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;";
            command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE CABINET (id int(10) UNSIGNED NOT NULL AUTO_INCREMENT, " +
                "code varchar(10) NOT NULL UNIQUE, cabinet_rows int(3) NOT NULL, " +
                "cabinet_columns int(3) NOT NULL, type_id int(10) UNSIGNED NOT NULL, " +
                "status varchar(20) NOT NULL DEFAULT 'Available', " +
                "PRIMARY KEY(id), FOREIGN KEY(type_id) REFERENCES LOCKER_TYPE(id)) " +
                "ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;";
            command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE LOCKER (id int(10) UNSIGNED NOT NULL AUTO_INCREMENT, " +
                "code varchar(20) NOT NULL UNIQUE, cabinet_id int(10) UNSIGNED NOT NULL, " +
                "status varchar(20) NOT NULL DEFAULT 'Available', PRIMARY KEY(id), " +
                "FOREIGN KEY(cabinet_id) REFERENCES CABINET(id)) " +
                "ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;";
            command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE RENTAL (id int(10) UNSIGNED NOT NULL AUTO_INCREMENT, " +
                "start_date date NOT NULL, duration int(4) NOT NULL, " +
                "customer_id int(10) UNSIGNED NOT NULL, locker_id int(10) UNSIGNED NOT NULL, " +
                "status varchar(20) DEFAULT 'Normal', " +
                "PRIMARY KEY(id), FOREIGN KEY(customer_id) " +
                "REFERENCES CUSTOMER(id), FOREIGN KEY(locker_id) REFERENCES LOCKER(id)) " +
                "ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;";
            command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE TRANSACTIONS (id int(10) UNSIGNED NOT NULL AUTO_INCREMENT, " +
                "rental_id int(10) UNSIGNED NOT NULL, customer_id int(10) UNSIGNED NOT NULL, " +
                "locker_id int(10) UNSIGNED NOT NULL, type_name varchar(20) NOT NULL, " +
                "type_rate decimal(10,2) NOT NULL, start_date date NOT NULL, duration int(4) NOT NULL, " +
                "return_date date, overdue_time int(4) DEFAULT 0, fine decimal(10,2) DEFAULT 0, " +
                "PRIMARY KEY(id)) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8; ";
            command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE ACCESS_LOG (id serial, " +
                "log_date date NOT NULL, log_time time NOT NULL, log_user varchar(25) NOT NULL, " +
                "log_action varchar(50) NOT NULL, log_item varchar(50), log_item_id varchar(10), " +
                "description varchar(10000), PRIMARY KEY(id)) " +
                "ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;";
            command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE RENTAL_SETTINGS(id int(10) UNSIGNED NOT NULL AUTO_INCREMENT, " +
                "name varchar(20) NOT NULL, value decimal(10, 2) NOT NULL, " +
                "PRIMARY KEY(id))ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8; ";
            command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE RENTAL_STATUS(transaction_id int(10) UNSIGNED NOT NULL, status_id int(10) UNSIGNED NOT NULL, " +
                "PRIMARY KEY(transaction_id, status_id), FOREIGN KEY(transaction_id) REFERENCES TRANSACTIONS(id), " +
                "FOREIGN KEY(status_id) REFERENCES RENTAL_SETTINGS(id))ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8; ";
            command = new MySqlCommand(sql, _connection);
            command.ExecuteNonQuery();
        }

        public static bool CheckUnique(string tableName, string conditionAttribute, string condition)
        {
            try
            {
                var sql = string.Format("SELECT COUNT(*) FROM (SELECT * FROM {0} WHERE {1} = '{2}') AS CheckUnique",
                  tableName, conditionAttribute, condition);
                var command = new MySqlCommand(sql, _connection);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count < 1;
            }
            catch (MySqlException)
            {
                return true;
            }
        }
        public static bool CheckUnique(string tableName, string conditionAttribute, string condition, string id)
        {
            try
            {
                var sql = string.Format("SELECT COUNT(*) FROM (SELECT * FROM {0} WHERE {1} = '{2}' AND id <> {3}) AS CheckUnique;",
                  tableName, conditionAttribute, condition, id);
                var command = new MySqlCommand(sql, _connection);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count < 1;
            }
            catch (MySqlException)
            {
                return true;
            }
        }
    }
}
