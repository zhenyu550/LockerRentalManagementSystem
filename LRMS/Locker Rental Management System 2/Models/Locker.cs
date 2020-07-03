using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockerRentalManagementSystem
{
    public class Locker
    {
        //Attributes

        private int _id;
        private string _code;
        private string _status;
        private int _cabinetId;

        //Getters and Setters

        public int Id { get { return _id; } set { _id = value; } }
        public string Code { get { return _code; } set { _code = value; } }
        public string Status { get { return _status; } set { _status = value; } }
        public int CabinetID { get { return _cabinetId; } set { _cabinetId = value; } }

        //Constants

        const string TableName = "locker";

        //Static Methods

        public static List<Locker> All(int count, int offset)
        {
            List<Locker> list = new List<Locker>();
            string query = String.Format("SELECT * FROM {0} ORDER BY id ASC LIMIT {1}, {2};", TableName, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Locker(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public static List<Locker> Where(string condition, int count, int offset)
        {
            List<Locker> list = new List<Locker>();
            string query = String.Format("SELECT * FROM {0} WHERE {1} ORDER BY id ASC LIMIT {2}, {3}", TableName,
                condition, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Locker(dataReader));
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

        public Locker Get(int id)
        {
            Locker item = null;
            string query = String.Format("SELECT * FROM {0} WHERE id = {1}", TableName, id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                item = new Locker(dataReader);
            }
            dataReader.Close();
            return item;
        }

        //Constructors

        public Locker()
        {
            _id = 0;
        }

        public Locker(MySqlDataReader dataReader)
        {
            Set(dataReader);
        }

        //Instance Methods - MySQL Related

        public void Set(MySqlDataReader dataReader)
        {
            _id = Convert.ToInt32(dataReader["id"] + "");
            _code = dataReader["code"] + "";
            _cabinetId = Convert.ToInt32(dataReader["cabinet_id"] + "");
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
            string query = "INSERT INTO {0} (code, cabinet_id) VALUES ('{1}', {2})";
            query = String.Format(query, TableName, _code, _cabinetId);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
            _id = (int)cmd.LastInsertedId;
        }

        public void Update()
        {
            string query = "UPDATE {0} SET code = '{1}', cabinet_id = {2}, status = '{3}' WHERE id = {4}";
            query = String.Format(query, TableName, _code, _cabinetId, _status, _id);
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

        public void Occupied()
        {
            string query = "UPDATE {0} SET status = '{1}' WHERE id = {2}";
            query = string.Format(query, TableName, "Occupied", _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
        }

        public void NotAvailable()
        {
            string query = "UPDATE {0} SET status = '{1}' WHERE id = {2}";
            query = string.Format(query, TableName, "Not Available", _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
        }

        public void Overdued()
        {
            string query = "UPDATE {0} SET status = '{1}' WHERE id = {2}";
            query = string.Format(query, TableName, "Overdue", _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
        }

        public void Reset()
        {
            string query = "UPDATE {0} SET status = '{1}' WHERE id = {2}";
            query = string.Format(query, TableName, "Available", _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
        }

        public void TempDelete()
        {
            string query = "UPDATE {0} SET status = '{1}' WHERE id = {2}";
            query = string.Format(query, TableName, "Disabled", _id);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
            _id = 0;
        }

        //Instance Methods - Functional 
        public bool IsAvailable()
        {
            return (Status == "Available");
        }

        public bool IsOccupied()
        {
            return (Status == "Occupied");
        }

        public bool IsNotAvailable()
        {
            return (Status == "Not Available");
        }

        public bool IsOverdued()
        {
            return (Status == "Overdue");
        }

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
