using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace LockerRentalManagementSystem
{
    class Rental
    {
        //Attributes

        private int _id;
        private DateTime _startDate;
        private int _duration;
        private int _customerId;
        private int _lockerId;
        private string _status;

        //Getters and Setters

        public int Id { get { return _id; } set { _id = value; } }
        public DateTime StartDate { get { return _startDate; } set { _startDate = value; } }
        public int Duration { get { return _duration; } set { _duration = value; } }
        public int CustomerID { get { return _customerId; } set { _customerId = value; } }
        public int LockerID { get { return _lockerId; } set { _lockerId = value; } }
        public string Status { get { return _status; } set { _status = value; } }
       
        //Constants

        const string TableName = "rental";

        //Static Methods

        public static List<Rental> All(int count, int offset)
        {
            List<Rental> list = new List<Rental>();
            string query = String.Format("SELECT * FROM {0} ORDER BY id ASC LIMIT {1}, {2};", TableName, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Rental(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public static List<Rental> Where(string condition, int count, int offset)
        {
            List<Rental> list = new List<Rental>();
            string query = String.Format("SELECT * FROM {0} WHERE {1} ORDER BY id ASC LIMIT {2}, {3}", TableName,
                condition, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Rental(dataReader));
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

        public Rental Get(int id)
        {
            Rental item = null;
            string query = String.Format("SELECT * FROM {0} WHERE id = {1}", TableName, id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                item = new Rental(dataReader);
            }
            dataReader.Close();
            return item;
        }

        //Constructors

        public Rental()
        {
            _id = 0;
        }

        public Rental(MySqlDataReader dataReader)
        {
            Set(dataReader);
        }

        //Instance Methods - MySQL Related

        public void Set(MySqlDataReader dataReader)
        {
            _id = Convert.ToInt32(dataReader["id"] + "");
            _startDate = DateTime.Parse(dataReader["start_date"] + "");
            _duration = Convert.ToInt32(dataReader["duration"] + "");
            _customerId = Convert.ToInt32(dataReader["customer_id"] + "");
            _lockerId = Convert.ToInt32(dataReader["locker_id"] + "");
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
            string date = _startDate.ToString("yyyy-MM-dd");
            string query = "INSERT INTO {0} (start_date, duration, customer_id, locker_id) " +
                "VALUES ('{1}', {2}, {3}, {4})";
            query = String.Format(query, TableName, date, _duration, _customerId, _lockerId);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
            _id = (int)cmd.LastInsertedId;
        }

        public void Update()
        {
            string date = _startDate.ToString("yyyy-MM-dd");
            string query = "UPDATE {0} SET start_date = '{1}', duration = {2}, customer_id = {3}, locker_id = {4}," +
                " status = '{5}' WHERE id = {6}";
            query = String.Format(query, TableName, date, _duration, _customerId, _lockerId, _status, _id);
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

        //Instance Methods - Functional 
        public bool IsNormal()
        {
            return (Status == "Normal");
        }

        public bool IsOverdue()
        {
            return (Status == "Overdue");
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
