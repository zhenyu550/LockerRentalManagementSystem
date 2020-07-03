using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace LockerRentalManagementSystem
{
    class RentalStatus
    {
        //Attributes

        private int _transactionId;
        private int _statusId;

        //Getters and Setters

        public int TransactionId { get { return _transactionId; } set { _transactionId = value; } }
        public int StatusId { get { return _statusId; } set { _statusId = value; } }


        //Constants

        const string TableName = "rental_status";

        public static List<RentalStatus> All(int count, int offset)
        {
            List<RentalStatus> list = new List<RentalStatus>();
            string query = String.Format("SELECT * FROM {0} ORDER BY _transaction_id ASC LIMIT {1}, {2};", TableName, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new RentalStatus(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public static List<RentalStatus> Where(string condition, int count, int offset)
        {
            List<RentalStatus> list = new List<RentalStatus>();
            string query = String.Format("SELECT * FROM {0} WHERE {1} ORDER BY transaction_id ASC LIMIT {2}, {3}", TableName,
                condition, count, offset);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new RentalStatus(dataReader));
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

        public RentalStatus Get(int id)
        {
            RentalStatus item = null;
            string query = String.Format("SELECT * FROM {0} WHERE transaction_id = {1}", TableName, id);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                item = new RentalStatus(dataReader);
            }
            dataReader.Close();
            return item;
        }

        public RentalStatus()
        {
            _transactionId = 0;
            _statusId = 0;
        }

        public RentalStatus(MySqlDataReader dataReader)
        {
            Set(dataReader);
        }

        //Instance Methods - MySQL Related

        public void Set(MySqlDataReader dataReader)
        {
            _transactionId = Convert.ToInt32(dataReader["transaction_id"] + "");
            _statusId = Convert.ToInt32(dataReader["status_id"] + "");
        }

        public void Insert()
        {
            string query = "INSERT INTO {0} (transaction_id, status_id) VALUES ({1}, {2})";
            query = String.Format(query, TableName, _transactionId, _statusId);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            cmd.ExecuteNonQuery();
        }

        public void Delete()
        {
            string query = String.Format("DELETE FROM {0} WHERE transaction_id = {1}", TableName, _transactionId);
            MySqlCommand command = new MySqlCommand(query, Database.Connection);
            command.ExecuteNonQuery();
            _transactionId = 0;
        }
    }
}