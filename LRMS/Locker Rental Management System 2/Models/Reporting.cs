using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockerRentalManagementSystem
{
    class Reporting
    {
        private string _typeName;
        private int _frequency;
        private int _daysUsed;
        private decimal _totalEarning;
        private decimal _totalFine;

        public string TypeName { get { return _typeName; } }
        public int Frequency { get { return _frequency; } }
        public int DaysUsed { get { return _daysUsed; } }
        public decimal TotalEarning { get { return _totalEarning; } }
        public decimal TotalFine { get { return _totalFine; } }

        //Constructors

        public Reporting()
        {

        }

        public Reporting(MySqlDataReader dataReader)
        {
            Set(dataReader);
        }

        //
        public static List<Reporting> Where(string condition)
        {
            List<Reporting> list = new List<Reporting>();
            string query = "SELECT type_name, count(id) AS frequency, sum(duration) as days_used, " +
                "sum(duration * type_rate) as total_earning, sum(fine) as total_fine FROM Transactions " +
                "WHERE {0} GROUP BY type_name";
            query = String.Format(query, condition);
            MySqlCommand cmd = new MySqlCommand(query, Database.Connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                list.Add(new Reporting(dataReader));
            }
            dataReader.Close();
            return list;
        }

        public void Set(MySqlDataReader dataReader)
        {
            _typeName = dataReader["type_name"] + "";
            _frequency = Convert.ToInt32(dataReader["frequency"] + "");
            _daysUsed = Convert.ToInt32(dataReader["days_used"] + "");
            _totalEarning = Convert.ToDecimal(dataReader["total_earning"] + "");
            _totalFine = Convert.ToDecimal(dataReader["total_fine"] + "");
        }
    }
}
