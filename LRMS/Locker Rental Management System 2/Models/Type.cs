using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockerRentalManagementSystem
{
    class Type
    {
        //Attributes

        private int _id;
        private string _name;
        private string _code;
        private decimal _rate;
        private string _status;

        //Getters and Setters

        public int Id { get { return _id; } set { _id = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public string Code { get { return _code; } set { _code = value; } }
        public decimal Rate { get { return _rate; } set { _rate = value; } }
        public string Status { get { return _status; } set { _status = value; } }

        //Constants

        const string TableName = "locker_type";

        //Static Methods

        public static List<Type> All(int count, int offset)
        {
            List<Type> list = new List<Type>();
            string query = String.Format("SELECT * FROM {0} ORDER BY id ASC LIMIT {1}, {2};", TableName, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Type(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public static List<Type> Where(string condition, int count, int offset)
        {
            List<Type> list = new List<Type>();
            string query = String.Format("SELECT * FROM {0} WHERE {1} ORDER BY id ASC LIMIT {2}, {3}", TableName,
                condition, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Type(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public int Count(string condition)
        {
            string query = String.Format("SELECT COUNT(*) FROM {0} WHERE {1}", TableName, condition);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            return int.Parse(cmd.ExecuteScalar().ToString());
        }

        public Type Get(int id)
        {
            Type item = null;
            string query = String.Format("SELECT * FROM {0} WHERE id = {1}", TableName, id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                item = new Type(dataReader);
            }
            dataReader.Close();
            return item;
        }

        //Constructors

        public Type()
        {
            _id = 0;
            _rate = 0;
        }

        public Type(MySqlDataReader dataReader)
        {
            Set(dataReader);
        }

        //Instance Methods - MySQL Related

        public void Set(MySqlDataReader dataReader)
        {
            _id = Convert.ToInt32(dataReader["id"] + "");
            _name = dataReader["name"] + "";
            _code = dataReader["code"] + "";
            _rate = Convert.ToDecimal(dataReader["rate"] + "");
            _status = dataReader["status"] + "";
        }

        public void Save()
        {
            if (_id == 0)
            {
                Insert();
            }
            else
            {
                Update();
            }
        }

        public void Insert()
        {
            string query = "INSERT INTO {0} (name, code, rate) VALUES ('{1}', '{2}', {3});";
            query = String.Format(query, TableName, _name, _code, _rate);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
            _id = (int)cmd.LastInsertedId;
        }

        public void Update()
        {
            string query = "UPDATE {0} SET name = '{1}', code = '{2}', rate = {3} WHERE id = {4}";
            query = String.Format(query, TableName, _name, _code, _rate, _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
        }

        public void Delete()
        {
            string query = String.Format("DELETE FROM {0} WHERE id = {1}", TableName, _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
            _id = 0;
        }

        public void TempDelete()
        {
            string query = "UPDATE {0} SET status = '{1}' WHERE id = {2}";
            query = string.Format(query, TableName, "Disabled", _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
        }

        public void Restore()
        {
            string query = "UPDATE {0} SET status = '{1}' WHERE id = {2}";
            query = string.Format(query, TableName, "Active", _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
        }

        //Instance Methods - Functional 
        public bool IsDisabled()
        {
            return (Status == "Disabled");
        }

        public static string CurrentID()
        {
            string sql = "SELECT `AUTO_INCREMENT` FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' " +
                "AND TABLE_NAME = '{1}';";
            sql = string.Format(sql, Database.DatabaseName, TableName);
            MySqlCommand command = new MySqlCommand(sql, Database.Connection);
            string autoIncrement = "";
            MySqlDataReader dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                autoIncrement = dataReader["AUTO_INCREMENT"] + "";
            }
            dataReader.Close();
            return autoIncrement;
        }
    }
}