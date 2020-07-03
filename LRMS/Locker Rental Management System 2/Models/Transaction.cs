using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace LockerRentalManagementSystem
{
    class Transaction
    {
        //Attributes

        private int _id;
        private int _rentalId;
        private int _customerId;
        private int _lockerId;
        private string _typeName;
        private decimal _typeRate;
        private DateTime _startDate;
        private int _duration;
        private DateTime _returnDate;
        private int _overdueTime;
        private decimal _fine;

        //Getters and Setters

        public int Id { get { return _id; } set { _id = value; } }
        public int RentalID { get { return _rentalId; } set { _rentalId = value; } }
        public int CustomerID { get { return _customerId; } set { _customerId = value; } }
        public int LockerID { get { return _lockerId; } set { _lockerId = value; } }
        public string TypeName { get { return _typeName; } set { _typeName = value; } }
        public decimal TypeRate { get { return _typeRate; } set { _typeRate = value; } }
        public DateTime StartDate { get { return _startDate; } set { _startDate = value; } }
        public int Duration { get { return _duration; } set { _duration = value; } }
        public DateTime ReturnDate { get { return _returnDate; } set { _returnDate = value; } }
        public int OverdueTime { get { return _overdueTime; } set { _overdueTime = value; } }
        public decimal Fine { get { return _fine; } set { _fine = value; } }

        //Constants

        const string TableName = "transactions";

        //Static Methods

        public static List<Transaction> All(int count, int offset)
        {
            List<Transaction> list = new List<Transaction>();
            string query = String.Format("SELECT * FROM {0} ORDER BY id ASC LIMIT {1}, {2};", TableName, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Transaction(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public static List<Transaction> Where(string condition, int count, int offset)
        {
            List<Transaction> list = new List<Transaction>();
            string query = String.Format("SELECT * FROM {0} WHERE {1} ORDER BY id ASC LIMIT {2}, {3}", TableName,
                condition, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Transaction(dataReader));
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

        public Transaction Get(int id)
        {
            Transaction item = null;
            string query = String.Format("SELECT * FROM {0} WHERE id = {1}", TableName, id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                item = new Transaction(dataReader);
            }
            dataReader.Close();
            return item;
        }

        //Constructors

        public Transaction()
        {
            _id = 0;
        }

        public Transaction(MySqlDataReader dataReader)
        {
            Set(dataReader);
        }

        //Instance Methods - MySQL Related

        public void Set(MySqlDataReader dataReader)
        {
            _id = Convert.ToInt32(dataReader["id"] + "");
            _rentalId = Convert.ToInt32(dataReader["rental_id"] + "");
            _customerId = Convert.ToInt32(dataReader["customer_id"] + "");
            _lockerId = Convert.ToInt32(dataReader["locker_id"] + "");
            _typeName = dataReader["type_name"] + "";
            _typeRate = Convert.ToDecimal(dataReader["type_rate"] + "");
            _startDate = DateTime.Parse(dataReader["start_date"] + "");
            _duration = Convert.ToInt32(dataReader["duration"] + "");

            string tempDate = dataReader["return_date"] + "";
            if (!String.IsNullOrWhiteSpace(tempDate))
                _returnDate = DateTime.Parse(tempDate);

            _overdueTime = Convert.ToInt32(dataReader["overdue_time"] + "");
            _fine = Convert.ToDecimal(dataReader["fine"] + "");
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
            string dateStart = _startDate.ToString("yyyy-MM-dd");
            string query = "INSERT INTO {0} (rental_id, customer_id, locker_id, type_name, type_rate, " +
                "start_date, duration) VALUES ({1}, {2}, {3}, '{4}', {5}, '{6}', {7})";
            query = String.Format(query, TableName, _rentalId, _customerId, _lockerId, _typeName, _typeRate,
                dateStart, _duration);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
            _id = (int)cmd.LastInsertedId;
        }

        public void Update()
        {
            string dateStart = _startDate.ToString("yyyy-MM-dd");
            string dateReturn = _returnDate.ToString("yyyy-MM-dd");
            string query = "UPDATE {0} SET rental_id = {1}, customer_id = {2}, locker_id = {3}, " +
                "type_name = '{4}', type_rate = {5}, start_date = '{6}', duration = {7}, return_date = '{8}', " +
                "overdue_time = {9}, fine = {10} WHERE id = {11}";
            query = String.Format(query, TableName, _rentalId, _customerId, _lockerId, _typeName, 
                _typeRate, dateStart, _duration, dateReturn, _overdueTime, _fine, _id);
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

        public void ChangeLocker()
        {
            string query = "UPDATE {0} SET locker_id = {1} WHERE id = {2}";
            query = String.Format(query, TableName, _lockerId, _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
        }

    }
}
