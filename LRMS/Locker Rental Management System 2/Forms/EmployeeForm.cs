using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LockerRentalManagementSystem
{
    public partial class EmployeeForm : Form
    {
        private bool _insertComplete;
        private bool _setupComplete = false;
        private string _hashedPassword;

        public bool InsertComplete { get { return _insertComplete; } }

        public EmployeeForm(bool setup)
        {
            InitializeComponent();
            _setupComplete = setup;
            if (!setup)
            {
                textBox7.Text = "1";
                label1.Hide();      //Add New Employee label
                label4.Show();      //Create Main Admin Account Label
                label17.Hide();     //Employee Details label
                label18.Hide();     //Edit Employee Details label
                button1.Show();     //Confirm button
                button2.Show();     //Cancel button
                button3.Hide();     //Close button
                button4.Hide();     //Edit button
                button5.Hide();     //Save button
                button6.Hide();     //Back button
            }
            else
            {
                textBox7.Text = Employee.CurrentID();
                label1.Show();      //Add New Employee label
                label4.Hide();      //Create Main Admin Account Label
                label17.Hide();     //Employee Details label
                label18.Hide();     //Edit Employee Details label
                button1.Show();     //Confirm button
                button2.Show();     //Cancel button
                button3.Hide();     //Close button
                button4.Hide();     //Edit button
                button5.Hide();     //Save button
                button6.Hide();     //Back button

            }
        }

        public EmployeeForm(bool setup, int id)
        {
            InitializeComponent();
            _setupComplete = setup;

            //Panel, Label and Button Settings
            label1.Hide();      //Add New Employee label
            label4.Hide();      //Create Main Admin Account Label
            label17.Show();     //Employee Details label
            label18.Hide();     //Edit Employee Details label
            button1.Hide();     //Confirm button
            button2.Hide();     //Cancel button
            button3.Show();     //Close button
            button4.Show();     //Edit button
            button5.Hide();     //Save button
            button6.Hide();     //Back button

            //Insert Employee Data Into Form
            Employee emp = new Employee();
            var item = emp.Get(id);
            LoadEmployeeData(item);

            LockInputBox();
        }

        private void EmployeeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (textBox7.Text == "1" && !_setupComplete)
            {
                var result = MessageBox.Show("Warning: No Data in Employee Table." + Environment.NewLine +
                    "Do you really want to exit?", "Exit Warning",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                { e.Cancel = true; }
            }
        }

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void TextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void TextBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Stop the character from being entered into the control
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void Button1_Click(object sender, EventArgs e) //Confirm button
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox4.Text) || string.IsNullOrWhiteSpace(textBox5.Text) ||
                string.IsNullOrWhiteSpace(textBox6.Text) || string.IsNullOrWhiteSpace(textBox8.Text) ||
                string.IsNullOrWhiteSpace(textBox9.Text) || string.IsNullOrWhiteSpace(comboBox1.Text) ||
                string.IsNullOrWhiteSpace(comboBox2.Text) || string.IsNullOrWhiteSpace(comboBox3.Text))
            {
                MessageBox.Show("Input Error: Empty Field(s) Detected!" + Environment.NewLine +
                   "Please ensure that all fields are filled. " + Environment.NewLine +
                   "The only field can be left blank is 'Telephone No.'", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!Database.CheckUnique("employee", "ic", textBox2.Text))
            {
                MessageBox.Show("Input Error: Duplicate attribute detected!" + Environment.NewLine +
                   "IC number '" + textBox2.Text + "' already exists in the employee table.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!Database.CheckUnique("employee", "username", textBox8.Text))
            {
                MessageBox.Show("Input Error: Duplicate attribute detected!" + Environment.NewLine +
                    "username '" + textBox8.Text + "' already exists in the employee table.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if ((comboBox3.Text != "Male" && comboBox3.Text != "Female") ||
                (comboBox2.Text != "Staff" && comboBox2.Text != "Manager" && comboBox2.Text != "Owner") ||
                (comboBox1.Text != "Admin" && comboBox1.Text != "Normal"))
            {
                MessageBox.Show("Input Error: Invalid input(s) Detected!" + Environment.NewLine +
                   "Please ensure that fields 'Gender', 'Account Permission' and 'Position'" +
                   " are filled with provided items. ", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (textBox7.Text == "1" && comboBox1.Text != "Admin")
            {
                MessageBox.Show("Initialization Error: No Admin Account in Employee Table!" + Environment.NewLine +
                   "You must create an admin account to continue.", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string username = textBox8.Text.ToLower();
                if (username.Contains("system"))
                {
                    MessageBox.Show("Input Error: Invalid username." + Environment.NewLine +
                        "Your username cannot contain the word 'system' as it was a reserved word.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                _hashedPassword = Security.SHA256Hash(textBox9.Text);
                if (textBox7.Text == "1")
                { Database.CreateTable(); }

                var employee = new Employee
                {
                    Username = username,
                    Password = _hashedPassword,
                    Permission = comboBox1.Text,
                    Name = textBox1.Text,
                    Ic = textBox2.Text,
                    Gender = comboBox3.Text,
                    Position = comboBox2.Text,
                    Salary = numericUpDown1.Value,
                    BirthDate = dateTimePicker1.Value,
                    TelephoneNo = textBox3.Text,
                    HandphoneNo = textBox4.Text,
                    Email = textBox5.Text,
                    Address = textBox6.Text
                };
                employee.Save();
                _insertComplete = true;
                _setupComplete = true;

                var log = new AccessLog
                {
                    Action = "Add",
                    Item = "Employee",
                    ItemId = textBox7.Text,
                    Description = "IC: " + textBox2.Text
                };

                if (textBox7.Text == "1")
                {
                    log.User = username;
                    log.Insert();
                }
                else
                {
                    log.User = Login.Username;
                    log.Insert();
                }

                this.Close();
            }
        }

        private void Button2_Click(object sender, EventArgs e) //Cancel button
        {
            this.Close();
        }

        private void Button3_Click(object sender, EventArgs e) //Close button
        {
            this.Close();
        }

        private void Button4_Click(object sender, EventArgs e) //Edit button
        {
            //Label & Button Settings
            label17.Hide();     //Employee Details label
            label18.Show();     //Edit Employee Details label


            button3.Hide();     //Close button
            button4.Hide();     //Edit button
            button5.Show();     //Save button
            button6.Show();     //Back button

            UnlockInputBox();
            if (textBox7.Text == "1")
            { comboBox1.Enabled = false; }
        }

        private void Button5_Click(object sender, EventArgs e) //Save button
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
               string.IsNullOrWhiteSpace(textBox4.Text) || string.IsNullOrWhiteSpace(textBox5.Text) ||
               string.IsNullOrWhiteSpace(textBox6.Text) || string.IsNullOrWhiteSpace(textBox8.Text) ||
               string.IsNullOrWhiteSpace(comboBox1.Text) || string.IsNullOrWhiteSpace(comboBox2.Text) ||
               string.IsNullOrWhiteSpace(comboBox3.Text))
            {
                MessageBox.Show("Input Error: Empty Field(s) Detected!" + Environment.NewLine +
                   "Please ensure that all fields are filled. " + Environment.NewLine +
                   "The only field can be left blank is 'Telephone No.' and 'Password'.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!Database.CheckUnique("employee", "ic", textBox2.Text, textBox7.Text))
            {
                MessageBox.Show("Input Error: Duplicate attribute detected!" + Environment.NewLine +
                   "IC number '" + textBox2.Text + "' already exists in the employee table.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if ((comboBox3.Text != "Male" && comboBox3.Text != "Female") ||
                (comboBox2.Text != "Staff" && comboBox2.Text != "Manager" && comboBox2.Text != "Owner") ||
                (comboBox1.Text != "Admin" && comboBox1.Text != "Normal"))
            {
                MessageBox.Show("Input Error: Invalid input(s) Detected!" + Environment.NewLine +
                   "Please ensure that fields 'Gender', 'Account Permission' and 'Position'" +
                   " are filled with provided items. ", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(textBox9.Text))
                {
                    _hashedPassword = Security.SHA256Hash(textBox9.Text);
                }

                var oldEmp = new Employee();
                oldEmp = oldEmp.Get(Convert.ToInt32(textBox7.Text));

                var employee = new Employee
                {
                    Id = Convert.ToInt32(textBox7.Text),
                    Username = textBox8.Text,
                    Password = _hashedPassword,
                    Permission = comboBox1.Text,
                    Name = textBox1.Text,
                    Ic = textBox2.Text,
                    Gender = comboBox3.Text,
                    Position = comboBox2.Text,
                    Salary = numericUpDown1.Value,
                    BirthDate = dateTimePicker1.Value,
                    TelephoneNo = textBox3.Text,
                    HandphoneNo = textBox4.Text,
                    Email = textBox5.Text,
                    Address = textBox6.Text
                };
                employee.Save();
                _setupComplete = true;

                string updatedAttr = "";

                if (oldEmp.Name != employee.Name)
                    updatedAttr += "; Name: " + oldEmp.Name + " to " + employee.Name;      
                
                if (oldEmp.TelephoneNo != employee.TelephoneNo)
                    updatedAttr += "; Phone No: " + oldEmp.TelephoneNo + " to " + employee.TelephoneNo;

                if (oldEmp.HandphoneNo != employee.HandphoneNo)
                    updatedAttr += "; H/P No: " + oldEmp.HandphoneNo + " to " + employee.HandphoneNo;

                if (oldEmp.Email != employee.Email)
                    updatedAttr += "; Email: " + oldEmp.Email + " to " + employee.Email;

                if (oldEmp.Address != employee.Address)
                    updatedAttr += "; Address: " + oldEmp.Address + " to " + employee.Address;

                if (oldEmp.Password != employee.Password)
                    updatedAttr += "; Password changed";

                if (oldEmp.Permission != employee.Permission)
                    updatedAttr += "; Permission: " + oldEmp.Permission + " to " + employee.Permission;

                if (oldEmp.Position != employee.Position)
                    updatedAttr += "; Position: " + oldEmp.Position + " to " + employee.Position;

                if (oldEmp.Salary != employee.Salary)
                    updatedAttr += "; Salary: " + oldEmp.Salary.ToString() + " to " + employee.Salary.ToString();

                var log = new AccessLog
                {
                    Action = "Update",
                    Item = "Employee",
                    ItemId = textBox7.Text,
                    User = Login.Username,
                    Description = "IC: " + employee.Ic + updatedAttr
                };
                log.Insert();

                if (_setupComplete)
                {
                    //Label & Button Settings
                    label17.Show();     //Employee Details label
                    label18.Hide();     //Edit Employee Details label

                    button3.Show();     //Close button
                    button4.Show();     //Edit button
                    button5.Hide();     //Save button
                    button6.Hide();     //Back button

                    LockInputBox();
                }
            }
        }

        private void Button6_Click(object sender, EventArgs e) //Back button
        {
            //Label & Button Settings
            label17.Show();     //Employee Details label
            label18.Hide();     //Edit Employee Details label

            button3.Show();     //Close button
            button4.Show();     //Edit button
            button5.Hide();     //Save button
            button6.Hide();     //Back button

            LockInputBox();

            //Reset unsave data to original data
            Employee emp = new Employee();
            var item = emp.Get(Convert.ToInt32(textBox7.Text));
            LoadEmployeeData(item);
        }

        private void LockInputBox()
        {
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            comboBox3.Enabled = false;
            dateTimePicker1.Enabled = false;
            textBox3.ReadOnly = true;
            textBox4.ReadOnly = true;
            textBox5.ReadOnly = true;
            textBox6.ReadOnly = true;

            textBox8.ReadOnly = true;
            textBox9.ReadOnly = true;
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            numericUpDown1.ReadOnly = true;
        }

        private void UnlockInputBox()
        {
            textBox1.ReadOnly = false;
            textBox2.ReadOnly = true;
            comboBox3.Enabled = false;
            dateTimePicker1.Enabled = false;
            textBox3.ReadOnly = false;
            textBox4.ReadOnly = false;
            textBox5.ReadOnly = false;
            textBox6.ReadOnly = false;

            textBox8.ReadOnly = true;
            textBox9.ReadOnly = false;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            numericUpDown1.ReadOnly = false;
        }

        private void LoadEmployeeData(Employee data)
        {
            textBox1.Text = data.Name;
            textBox2.Text = data.Ic;
            comboBox3.Text = data.Gender;
            dateTimePicker1.Value = data.BirthDate;
            textBox3.Text = data.TelephoneNo;
            textBox4.Text = data.HandphoneNo;
            textBox5.Text = data.Email;
            textBox6.Text = data.Address;
            textBox7.Text = data.Id.ToString();
            textBox8.Text = data.Username;
            textBox9.Text = "";
            comboBox1.Text = data.Permission;
            comboBox2.Text = data.Position;
            numericUpDown1.Value = data.Salary;
            _hashedPassword = data.Password;
        }
    }
}

