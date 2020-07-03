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
    public partial class TypeForm : Form
    {
        private bool _addNew = false;
        private bool _insertComplete = false;

        public bool InsertComplete { get { return _insertComplete; } }

        public TypeForm()
        {
            InitializeComponent();
            label1.Show();
            label2.Hide();
            textBox1.Text = Type.CurrentID();
            _addNew = true;
        }

        public TypeForm(int id)
        {
            InitializeComponent();
            label1.Hide();
            label2.Show();
            textBox3.ReadOnly = true;
            Type type = new Type();
            var item = type.Get(id);
            LoadTypeData(item);
        }

        private void LoadTypeData(Type data)
        {
            textBox1.Text = data.Id.ToString();
            textBox2.Text = data.Name;
            textBox3.Text = data.Code;
            numericUpDown1.Value = data.Rate;
        }

        private void Button2_Click(object sender, EventArgs e) //Save button
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Input Error: Empty Field(s) Detected!" + Environment.NewLine +
                    "Please ensure that all fields are filled. ", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (_addNew)
                {
                    if (!Database.CheckUnique("locker_type", "code", textBox3.Text))
                    {
                        MessageBox.Show("Input Error: Duplicate attribute detected!" + Environment.NewLine +
                            "Code '" + textBox3.Text + "' already exists in locker_type table.", "Input Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var item = new Type
                    {
                        Name = textBox2.Text,
                        Code = textBox3.Text,
                        Rate = numericUpDown1.Value
                    };
                    item.Save();

                    var log = new AccessLog
                    {
                        User = Login.Username,
                        Action = "Add",
                        Item = "Locker Type",
                        ItemId = textBox1.Text,
                        Description = "Code: " + textBox3.Text
                    };
                    log.Insert();
                    _insertComplete = true;
                }
                else
                {
                    var oldType = new Type();
                    oldType = oldType.Get(Convert.ToInt32(textBox1.Text));

                    var item = new Type
                    {
                        Id = Convert.ToInt32(textBox1.Text),
                        Name = textBox2.Text,
                        Code = textBox3.Text,
                        Rate = numericUpDown1.Value
                    };
                    item.Save();

                    string updatedAttr = "";
                    if (oldType.Name != item.Name)
                        updatedAttr += "; Name: " + oldType.Name + " to " + item.Name;

                    if (oldType.Rate != item.Rate)
                        updatedAttr += "; Rate: " + oldType.Rate + " to " + item.Rate;

                    var log = new AccessLog
                    {
                        User = Login.Username,
                        Action = "Update",
                        Item = "Locker Type",
                        ItemId = textBox1.Text,
                        Description = "Code: " + item.Code + updatedAttr
                    };
                    log.Insert();
                }
                this.Close();
            }
        }

        private void Button1_Click(object sender, EventArgs e) //Cancel Button
        {
            this.Close();
        }
    }
}
