using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LockerRentalManagementSystem
{
    public partial class Login : Form
    {
        string server;
        string port;
        string uid;
        string pw;
        string db;

        private static string _username;
        public static string Username
        {
            get { return _username; }
        }

        public Login()
        {
            InitializeComponent();
            CheckIniFile();
            SetupForm();
        }

        void CheckIniFile()
        {
            if (!File.Exists("Settings.ini"))
            {
                var NewIni = new INIFile("Settings.ini");
                NewIni.Write("server", "");
                NewIni.Write("port", "0");
                NewIni.Write("uid", "");
                NewIni.Write("password", "");
                NewIni.Write("database", "");
            }
            var LoadIni = new INIFile("Settings.ini");

            textBox3.Text = server = LoadIni.Read("server");
            port = LoadIni.Read("port");
            numericUpDown1.Value = Convert.ToDecimal(port);
            textBox4.Text = uid = LoadIni.Read("uid");
            textBox5.Text = pw = LoadIni.Read("password");
            textBox6.Text = db = LoadIni.Read("database");
        }

        bool Validations()
        {
            if (!ValidateConnection())
            {
                MessageBox.Show("Initialization Error: Unable to verify server connection. " + Environment.NewLine +
                    "Please enter your database connection settings.", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!ValidateDatabase())
            {
                var result = MessageBox.Show("Initialization Error: Database does not exist." + Environment.NewLine +
                    "Do you want to create a new database?" + Environment.NewLine +
                    "If you press Yes, a new database with name '" + db + "' with new tables will be created. ",
                    "Initialization Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    Database.CreateDatabase(server, port, uid, pw, db);
                    Database.Connect();
                    CreateAccount();
                    return ValidateTable();
                }
                else
                { return false; }
            }
            if (!ValidateTable())
            {
                var result = MessageBox.Show("Initialization Error: Database tables corrupted." + Environment.NewLine +
                    "Do you want to recreate all tables?" + Environment.NewLine +
                    "If you press Yes, all existing tables will be dropped and new tables will be created after you create a main admin account.",
                    "Initialization Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    Database.DropTable();
                    CreateAccount();
                    return ValidateTable();
                }
                return false;

            }
            return true;
        }

        void SetupForm()
        {
            if (Validations())
            {
                SetupLogin();
            }
            else
            { 
                this.Controls.Remove(panel1);
                this.Height = 202;
            }
        }

        MySqlConnection con;

        bool ValidateConnection()
        {
            try
            {
                string conString = String.Format("SERVER={0};PORT={1};UID={2};PASSWORD={3};SSLMODE=NONE", server, port, uid, pw);
                con = new MySqlConnection(conString);
                con.Open();
                con.Close();
                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        }

        bool ValidateDatabase()
        {
            Database.Initialize(server, port, uid, pw, db);
            return (Database.Connect());
        }

        bool ValidateTable()
        {
            return (Database.TableExists("employee") &&
                    Database.TableExists("customer") &&
                    Database.TableExists("locker_type") &&
                    Database.TableExists("cabinet") &&
                    Database.TableExists("locker") &&
                    Database.TableExists("rental") &&
                    Database.TableExists("transactions") &&
                    Database.TableExists("access_log") &&
                    Database.TableExists("rental_settings") &&
                    Database.TableExists("rental_status"));
        }

        void SetupLogin()
        {
            label9.Hide();
            this.Controls.Clear();
            this.Controls.Add(panel1);
            this.Height = 134;
        }

        void CreateAccount()
        {
            EmployeeForm CreateNewAccount = new EmployeeForm(false);
            CreateNewAccount.ShowDialog();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "+")
            {
                button3.Text = "-";
                this.Controls.Clear();
                this.Controls.Add(panel2);
                this.Controls.Add(panel1);
                this.Height = 297;
            }
            else
            {
                button3.Text = "+";
                this.Controls.Remove(panel2);
                this.Height = 134;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            var NewIni = new INIFile("Settings.ini");
            NewIni.Write("server", textBox3.Text);
            NewIni.Write("port", numericUpDown1.Text);
            NewIni.Write("uid", textBox4.Text);
            NewIni.Write("password", textBox5.Text);
            NewIni.Write("database", textBox6.Text);

            Login ReloadForm = new Login();
            ReloadForm.Show();
            this.Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            _username = textBox1.Text.ToLower();
            string hashedPassword = Security.SHA256Hash(textBox2.Text);
           
            List<Employee> item = Employee.Where(string.Format("username = '{0}'", _username), 0, 1);
            if (item.Count == 0)
            {
                MessageBox.Show("Login Error: Incorrect username. " + Environment.NewLine +
                          "This username does not exists." + Environment.NewLine +
                          "Please double-check and try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var emp = item[0];
                if (emp.Password == hashedPassword && !emp.IsDisabled())
                {
                    var log = new AccessLog
                    {
                        User = _username,
                        Action = "Login",
                        Item = "",
                        ItemId = ""
                    };
                    log.Insert();

                    Form1 NextForm = new Form1();
                    NextForm.Show();
                    this.Close();
                }
                else
                {
                    if (emp.IsDisabled())
                    {
                        MessageBox.Show("Login Error: Account Disabled." + Environment.NewLine +
                            "Your account is suspended." + Environment.NewLine + "Please contact adminstrator for further details.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Login Error: Incorrect password. " + Environment.NewLine +
                            "Please double-check and try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }
    }
}