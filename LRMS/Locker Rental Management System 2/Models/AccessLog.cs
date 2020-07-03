using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace LockerRentalManagementSystem
{
    class AccessLog
    {
        //Attributes

        private int _id;
        private string _logDate;
        private string _logTime;
        private string _user;
        private string _action;
        private string _item;
        private string _itemId;
        private string _description;


        //Getters and Setters

        public int Id { get { return _id; } set { _id = value; } }
        public string LogDate { get { return _logDate; } set { _logDate = value; } }
        public string LogTime { get { return _logTime; } set { _logTime = value; } }
        public string User { get { return _user; } set { _user = value; } }
        public string Action { get { return _action; } set { _action = value; } }
        public string Item { get { return _item; } set { _item = value; } }
        public string ItemId { get { return _itemId; } set { _itemId = value; } }
        public string Description { get { return _description; } set { _description = value; } }

        //Constants

        const string TableName = "access_log";

        //Static Methods

        public static List<AccessLog> All(int count, int offset)
        {
            List<AccessLog> list = new List<AccessLog>();
            string query = String.Format("SELECT * FROM {0} ORDER BY id ASC LIMIT {1}, {2}", TableName, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new AccessLog(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public static List<AccessLog> Where(string condition, int count, int offset)
        {
            List<AccessLog> list = new List<AccessLog>();
            string query = String.Format("SELECT * FROM {0} WHERE {1} ORDER BY id ASC LIMIT {2}, {3}", TableName, condition, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new AccessLog(dataReader));
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

        public AccessLog Get(int id)
        {
            AccessLog item = null;
            string query = String.Format("SELECT * FROM {0} WHERE id = {1}", TableName, id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                item = new AccessLog(dataReader);
            }
            dataReader.Close();
            return item;
        }

        //Constructors

        public AccessLog()
        {
            _id = 0;
        }

        public AccessLog(MySqlDataReader dataReader)
        {
            Set(dataReader);
        }

        //Instance Methods - MySQL Related

        public void Set(MySqlDataReader dataReader)
        {
            _id = Convert.ToInt32(dataReader["id"] + "");
            DateTime date = DateTime.Parse(dataReader["log_date"] + "");
            _logDate = date.ToString("dd-MM-yyyy");
            _logTime = dataReader["log_time"] + "";
            _user = dataReader["log_user"] + "";
            _action = dataReader["log_action"] + "";
            _item = dataReader["log_item"] + "";
            _itemId = dataReader["log_item_id"] + "";
            _description = dataReader["description"] + "";
        }

        public void Insert()
        {
            _logDate = DateTime.Now.ToString("yyyy-MM-dd");
            _logTime = DateTime.Now.ToString("HH:mm:ss");
            string query = "INSERT INTO {0} (log_date, log_time, log_user, log_action, log_item, log_item_id, description)" +
                " VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')";
            query = String.Format(query, TableName, _logDate, _logTime, _user, _action, _item, _itemId, _description);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
            _id = (int)cmd.LastInsertedId;
        }

        public void Delete()
        {
            string query = String.Format("DELETE FROM {0} WHERE id = {1}", TableName, _id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
            _id = 0;
        }
    }
}
