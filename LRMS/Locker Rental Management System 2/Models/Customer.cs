using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace LockerRentalManagementSystem
{
    public class Customer
    {
        //Attributes

        private int _id;
        private string _name;
        private string _ic;
        private string _gender;
        private DateTime _birthDate;
        private string _telephoneNo;
        private string _handphoneNo;
        private string _email;
        private string _address;

        //Getters and Setters

        public int Id { get { return _id; } set { _id = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public string Ic { get { return _ic; } set { _ic = value; } }
        public string Gender { get { return _gender; } set { _gender = value; } }
        public DateTime BirthDate { get { return _birthDate; } set { _birthDate = value; } }
        public string TelephoneNo { get { return _telephoneNo; } set { _telephoneNo = value; } }
        public string HandphoneNo { get { return _handphoneNo; } set { _handphoneNo = value; } }
        public string Email { get { return _email; } set { _email = value; } }
        public string Address { get { return _address; } set { _address = value; } }

        //Constants

        const string TableName = "customer";

        //Static Methods

        public static List<Customer> All(int count, int offset)
        {
            List<Customer> list = new List<Customer>();
            string query = String.Format("SELECT * FROM {0} ORDER BY id ASC LIMIT {1}, {2}", TableName, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Customer(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public static List<Customer> Where(string condition, int offset, int count)
        {
            List<Customer> list = new List<Customer>();
            string query = String.Format("SELECT * FROM {0} WHERE {1} ORDER BY id ASC LIMIT {2}, {3}", TableName, condition, offset, count);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Customer(dataReader));
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

        public Customer Get(int id)
        {
            Customer item = null;
            string query = String.Format("SELECT * FROM {0} WHERE id = {1}", TableName, id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                item = new Customer(dataReader);
            }
            dataReader.Close();
            return item;
        }

        //Constructors

        public Customer()
        {
            _id = 0;
        }
        
        public Customer(MySqlDataReader dataReader)
        {
            Set(dataReader);
        }

        //Instance Methods - MySQL Related

        public void Set(MySqlDataReader dataReader)
        {
            _id = Convert.ToInt32(dataReader["id"] + "");
            _name = dataReader["name"] + "";
            _ic = dataReader["ic"] + "";
            _gender = dataReader["gender"] + "";
            _birthDate = dataReader.GetDateTime("dob");
            _telephoneNo = dataReader["phone_no"] + "";
            _handphoneNo = dataReader["hp_no"] + "";
            _email = dataReader["email"] + "";
            _address = dataReader["home_address"] + "";
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
            string dob = _birthDate.ToString("yyyy-MM-dd");
            string query = "INSERT INTO {0} (name, ic, gender, dob, phone_no, hp_no, email, home_address)" +
                " VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')";
            query = String.Format(query, TableName, _name, _ic, _gender, dob, _telephoneNo, _handphoneNo, _email, _address);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
            _id = (int)cmd.LastInsertedId;
        }

        public void Update()
        {
            string dob = _birthDate.ToString("yyyy-MM-dd");
            string query = "UPDATE {0} SET name = '{1}', ic = '{2}', gender = '{3}', dob = '{4}'," +
                "phone_no = '{5}', hp_no = '{6}', email = '{7}', home_address = '{8}' WHERE id = {9}";
            query = String.Format(query, TableName, _name, _ic, _gender, dob, _telephoneNo, _handphoneNo, _email, _address, _id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
        }

        public void Delete()
        {
            string query = String.Format("DELETE FROM {0} WHERE id = {1}", TableName, _id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
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
