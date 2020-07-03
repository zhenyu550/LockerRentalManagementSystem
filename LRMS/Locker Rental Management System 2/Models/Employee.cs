using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockerRentalManagementSystem
{
    public class Employee
    {
        //Attributes

        private int _id;
        private string _username;
        private string _password;
        private string _permission;
        private string _name;
        private string _ic;
        private string _gender;
        private string _position;
        private decimal _salary;
        private DateTime _birthDate;
        private string _telephoneNo;
        private string _handphoneNo;
        private string _email;
        private string _address;
        private string _status;       

        //Getters and Setters

        public int Id { get { return _id; } set { _id = value; } }
        public string Username { get { return _username; } set { _username = value; } }
        public string Password { get { return _password; } set { _password = value; } }
        public string Permission { get { return _permission; } set { _permission = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public string Ic { get { return _ic; } set { _ic = value; } }
        public string Gender { get { return _gender; } set { _gender = value; } }
        public string Position { get { return _position; } set { _position = value; } }
        public decimal Salary { get { return _salary; } set { _salary = value; } }
        public DateTime BirthDate { get { return _birthDate; } set { _birthDate = value; } }
        public string TelephoneNo { get { return _telephoneNo; } set { _telephoneNo = value; } }
        public string HandphoneNo { get { return _handphoneNo; } set { _handphoneNo = value; } }
        public string Email { get { return _email; } set { _email = value; } }
        public string Address { get { return _address; } set { _address = value; } }      
        public string Status { get { return _status; } set { _status = value; } }

        //Constants

        const string TableName = "employee";

        //Static Methods

        public static List<Employee> All(int count, int offset)
        {
            List<Employee> list = new List<Employee>();
            string query = String.Format("SELECT * FROM {0} ORDER BY id ASC LIMIT {1}, {2};", TableName, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Employee(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public static List<Employee> Where(string condition, int count, int offset)
        {
            List<Employee> list = new List<Employee>();
            string query = String.Format("SELECT * FROM {0} WHERE {1} ORDER BY id ASC LIMIT {2}, {3}", TableName, 
                condition, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Employee(dataReader));
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

        public Employee Get(int id)
        {
            Employee item = null;
            string query = String.Format("SELECT * FROM {0} WHERE id = {1}", TableName, id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                item = new Employee(dataReader);
            }
            dataReader.Close();
            return item;
        }

        //Constructors

        public Employee()
        {
            _id = 0;
            _permission = "Active";
        }

        public Employee(MySqlDataReader dataReader)
        {
            Set(dataReader);
        }

        //Instance Methods - MySQL Related

        public void Set(MySqlDataReader dataReader)
        {
            _id = Convert.ToInt32(dataReader["id"] + "");
            _username = dataReader["username"] + "";
            _password = dataReader["password"] + "";
            _permission = dataReader["permission"] + "";
            _name = dataReader["name"] + "";
            _ic = dataReader["ic"] + "";
            _gender = dataReader["gender"] + "";
            _position = dataReader["position"] + "";
            _salary = Convert.ToDecimal(dataReader["salary"] + "");
            _birthDate = dataReader.GetDateTime("dob");
            _telephoneNo = dataReader["phone_no"] + "";
            _handphoneNo = dataReader["hp_no"] + "";
            _email = dataReader["email"] + "";
            _address = dataReader["home_address"] + "";
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
            string dob = _birthDate.ToString("yyyy-MM-dd");
            string query = "INSERT INTO {0} (username, password, permission, name, ic, gender, position, " +
                "salary, dob, phone_no, hp_no, email, home_address)" +
                " VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}','{8}','{9}','{10}','{11}','{12}', '{13}')";
            query = String.Format(query, TableName, _username, _password, _permission, _name, _ic, _gender, _position, 
                _salary, dob, _telephoneNo, _handphoneNo, _email, _address);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
            _id = (int)cmd.LastInsertedId;
        }

        public void Update()
        {
            string dob = _birthDate.ToString("yyyy-MM-dd");
            string query = "UPDATE {0} SET username = '{1}', password = '{2}', permission = '{3}', name = '{4}', " +
                "ic = '{5}', gender = '{6}', position = '{7}', salary = '{8}', dob='{9}', phone_no='{10}', " +
                "hp_no = '{11}', email = '{12}', home_address = '{13}' WHERE id = {14}";
            query = String.Format(query, TableName, _username, _password, _permission, _name, _ic, _gender, _position,
                _salary, dob, _telephoneNo, _handphoneNo, _email, _address, _id);
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

        public bool IsAdmin()
        {
            return (Permission == "Admin");
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
