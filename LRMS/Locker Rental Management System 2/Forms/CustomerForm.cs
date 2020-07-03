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
    public partial class CustomerForm : Form
    {
        private bool _insertComplete = false;
        public bool InsertComplete { get { return _insertComplete; } }

        private Customer result = null;
        public Customer Result { get { return result; } }

        public CustomerForm()
        {
            InitializeComponent();
            textBox7.Text = Customer.CurrentID();

            //Panel, Label & Button Settings
            label1.Show();      //Add New Customer label
            label2.Hide();      //Customer Details label
            label3.Hide();      //Edit Customer Details label

            button1.Show();     //Confirm button
            button2.Show();     //Cancel button
            button3.Hide();     //Close button
            button4.Hide();     //Edit button
            button5.Hide();     //Back button
            button6.Hide();     //Save button
        }

        public CustomerForm(int id)
        {
            InitializeComponent();

            //Panel, Label & Button Settings
            label1.Hide();      //Add New Customer label
            label2.Show();      //Customer Details label
            label3.Hide();      //Edit Customer Details label

            button1.Hide();     //Confirm button
            button2.Hide();     //Cancel button
            button3.Show();     //Close button
            button4.Show();     //Edit button
            button5.Hide();     //Back button
            button6.Hide();     //Save button

            //Insert Customer Data into Form
            Customer cus = new Customer();
            var item = cus.Get(id);
            LoadCustomerData(item);
    
            //Lock all input box
            LockInputBox();
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
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void Button1_Click(object sender, EventArgs e) //Confirm button
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
               string.IsNullOrWhiteSpace(textBox4.Text) || string.IsNullOrWhiteSpace(textBox5.Text) ||
               string.IsNullOrWhiteSpace(textBox6.Text) || string.IsNullOrWhiteSpace(comboBox3.Text))
            {
                MessageBox.Show("Input Error: Empty Field(s) Detected!" + Environment.NewLine +
                   "Please ensure that all fields are filled. " + Environment.NewLine +
                   "The only field can be left blank is 'Telephone No.'", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (comboBox3.Text != "Male" && comboBox3.Text != "Female")
            {
                MessageBox.Show("Input Error: Invalid input(s) Detected!" + Environment.NewLine +
                   "Please ensure that fields 'Gender' are filled with provided items. ",
                   "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!Database.CheckUnique("customer", "ic", textBox2.Text))
            {
                MessageBox.Show("Input Error: Duplicate attribute detected!" + Environment.NewLine +
                   "IC number '" + textBox2.Text + "' already exists in customer table.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var customer = new Customer
                {
                    Name = textBox1.Text,
                    Ic = textBox2.Text,
                    Gender = comboBox3.Text,
                    BirthDate = dateTimePicker1.Value,
                    TelephoneNo = textBox3.Text,
                    HandphoneNo = textBox4.Text,
                    Email = textBox5.Text,
                    Address = textBox6.Text
                };
                customer.Save();
                result = customer;
                var log = new AccessLog
                {
                    User = Login.Username,
                    Action = "Add",
                    Item = "Customer",
                    ItemId = textBox7.Text,
                    Description = "IC: " + customer.Ic
                };
                log.Insert();
                _insertComplete = true;
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
            label2.Hide();      //Customer Details label
            label3.Show();      //Edit Customer Details label

            button3.Hide();     //Close button
            button4.Hide();     //Edit button
            button5.Show();     //Back button
            button6.Show();     //Save button

            UnlockInputBox();
        }

        private void Button5_Click(object sender, EventArgs e) //Back button
        {
            //Label & Button Settings
            label2.Show();      //Customer Details label
            label3.Hide();      //Edit Customer Details label

            button3.Show();     //Close button
            button4.Show();     //Edit button
            button5.Hide();     //Back button
            button6.Hide();     //Save button

            LockInputBox();

            //Reset unsave data to original data
            Customer cus = new Customer();
            var item = cus.Get(Convert.ToInt32(textBox7.Text));
            LoadCustomerData(item);
        }

        private void Button6_Click(object sender, EventArgs e) //Save button
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
               string.IsNullOrWhiteSpace(textBox4.Text) || string.IsNullOrWhiteSpace(textBox5.Text) ||
               string.IsNullOrWhiteSpace(textBox6.Text) || string.IsNullOrWhiteSpace(comboBox3.Text))
            {
                MessageBox.Show("Input Error: Empty Field(s) Detected!" + Environment.NewLine +
                   "Please ensure that all fields are filled. " + Environment.NewLine +
                   "The only field can be left blank is 'Telephone No.'", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (comboBox3.Text != "Male" && comboBox3.Text != "Female")
            {
                MessageBox.Show("Input Error: Invalid input(s) Detected!" + Environment.NewLine +
                   "Please ensure that fields 'Gender' are filled with provided items. ",
                   "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!Database.CheckUnique("customer", "ic", textBox2.Text, textBox7.Text))
            {
                MessageBox.Show("Input Error: Duplicate attribute detected!" + Environment.NewLine +
                   "IC number '" + textBox2.Text + "' already exists in customer table.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var oldCus = new Customer();
                oldCus = oldCus.Get(Convert.ToInt32(textBox7.Text));

                var customer = new Customer()
                {
                    Id = Convert.ToInt32(textBox7.Text),
                    Name = textBox1.Text,
                    Ic = textBox2.Text,
                    Gender = comboBox3.Text,
                    BirthDate = dateTimePicker1.Value,
                    TelephoneNo = textBox3.Text,
                    HandphoneNo = textBox4.Text,
                    Email = textBox5.Text,
                    Address = textBox6.Text
                };
                customer.Save();

                string updated_attr = "";

                if (oldCus.Name != customer.Name)
                    updated_attr += "; Name: " + oldCus.Name + " to " + customer.Name;

                if (oldCus.TelephoneNo != customer.TelephoneNo)
                    updated_attr += "; Phone No: " + oldCus.TelephoneNo + " to " + customer.TelephoneNo;

                if (oldCus.HandphoneNo != customer.HandphoneNo)
                    updated_attr += "; H/P No: " + oldCus.HandphoneNo + " to " + customer.HandphoneNo;

                if (oldCus.Email != customer.Email)
                    updated_attr += "; Email: " + oldCus.Email + " to " + customer.Email;

                if (oldCus.Address != customer.Address)
                    updated_attr += "; Address: " + oldCus.Address + " to " + customer.Address;

                var log = new AccessLog
                {
                    Action = "Update",
                    Item = "Customer",
                    ItemId = textBox7.Text,
                    User = Login.Username,
                    Description = "IC: " + customer.Ic + updated_attr
                };
                log.Insert();

                //Label & Button Settings
                label2.Show();      //Customer Details label
                label3.Hide();      //Edit Customer Details label

                button3.Show();     //Close button
                button4.Show();     //Edit button
                button5.Hide();     //Back button
                button6.Hide();     //Save button

                LockInputBox();
            }
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
        }

        private void LoadCustomerData(Customer data)
        {
            textBox7.Text = data.Id.ToString();
            textBox1.Text = data.Name;
            textBox2.Text = data.Ic;
            comboBox3.Text = data.Gender;
            dateTimePicker1.Value = data.BirthDate;
            textBox3.Text = data.TelephoneNo;
            textBox4.Text = data.HandphoneNo;
            textBox5.Text = data.Email;
            textBox6.Text = data.Address;
        }
    }
}
