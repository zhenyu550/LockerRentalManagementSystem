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
    public partial class CabinetForm : Form
    {
        //Add Cabinet section
        private List<Type> _typeList = new List<Type>();
        private Dictionary<int, string> _comboBoxItems = new Dictionary<int, string>();
        private string _sizeCode;
        private bool _insertComplete;

        public bool InsertComplete { get { return _insertComplete; } }

        public CabinetForm()
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel1);
            textBox1.Text = Cabinet.CurrentID().ToString();
            _typeList = Type.Where("status <> 'Disabled'", 0, 100);

            //Load Locker Type Name into Combo Box 1
            foreach (Type t in _typeList)
            { _comboBoxItems.Add(t.Id, t.Name); }

            if (!_comboBoxItems.Any())
                return;

            comboBox1.DataSource = new BindingSource(_comboBoxItems, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";
            comboBox1.SelectedIndex = -1;
        }

        private void ComboBox1_TextChanged(object sender, EventArgs e) //Auto generate Cabinet Code
        {
            if (comboBox1.SelectedIndex < 0) { return; }

            //Select Code for Cabinet Locker Type
            var item = from selected in _typeList
                       where selected.Name.Contains(comboBox1.Text)
                       select selected;
            _sizeCode = item.First().Code;

            //Auto generate locker code for each cabinet
            int newCabCodeNo = 0;
            string cabCond = "id = (SELECT MAX(id) FROM cabinet WHERE type_id = {0})";
            List<Cabinet> cabList = Cabinet.Where(String.Format(cabCond, item.First().Id),0,1);
            if (!cabList.Any())
            {
                newCabCodeNo = 1;
            }
            else
            {
                string currCabCode = cabList[0].Code;
                string currCabCodeNo = String.Empty;
                for (int i = 0; i < currCabCode.Length; i++)
                {
                    if (Char.IsDigit(currCabCode[i]))
                    { currCabCodeNo += currCabCode[i]; }
                }
                newCabCodeNo = Convert.ToInt32(currCabCodeNo) + 1;
            }
            textBox2.Text = String.Format("{0}-{1}", _sizeCode, newCabCodeNo.ToString("D2"));
        }

        private void Button1_Click(object sender, EventArgs e) //Cancel Button
        {
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e) //Save Button
        {
            //Select item the dictonary<int (key), string (value)>, which contains the type_name (comboBox1.Text)
            //In _comboBoxItems, key = type_id, value = type_name
            var dictValue = from selected in _comboBoxItems
                            where selected.Value.Contains(comboBox1.Text)
                            select selected;

            //Check if comboBox1.Text was empty or invalid input, 
            //dictValue.Any() determines whether the sequence in dictvalue contains any element
            if (comboBox1.SelectedIndex < 0 || !dictValue.Any())
            {
                MessageBox.Show("Input Error: Invalid input detected!" + Environment.NewLine +
                  "Please ensure that field 'Locker Type' was filled with provided items. ",
                  "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Get the key of the selected item
            int typeId = Convert.ToInt32(dictValue.First().Key);
            var cab = new Cabinet
            {
                Code = textBox2.Text,
                Row = Convert.ToInt32(numericUpDown1.Value),
                Column = Convert.ToInt32(numericUpDown2.Value),
                TypeID = typeId
            };
            cab.Save();
            int latestCabId = cab.Id;
            
            var log = new AccessLog
            {
                User = Login.Username,
                Action = "Add",
                Item = "Cabinet",
                ItemId = textBox1.Text.ToString(),
                Description = "Code: " + textBox2.Text
            };
            log.Insert();

            _insertComplete = true;

            for (int i = 1; i <= (cab.Row * cab.Column); i++)
            {
                log.ItemId = Locker.CurrentID();

                //Auto increment for locker codes
                var locker = new Locker
                {
                    Code = String.Format("{0}-{1}", cab.Code, i.ToString("D3")),
                    CabinetID = latestCabId
                };
                locker.Save();

                log.User = "system";
                log.Action = "Add";
                log.Item = "Locker";
                log.Description = "Code: " + locker.Code;
                log.Insert();
            }
            this.Close();
        }

        //Filter Cabinet Section
        private string _condition;
        private string _cabinetSize;
        private string _cabinetStatus;
        public string Condition { get { return _condition; } set { _condition = value; } }

        public CabinetForm(bool deleted)
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            
            if (deleted == false) //Filter Cabinet for normal cabinets
            {
                Controls.Add(panel4);
                this.Height = 260;
                _typeList = Type.Where("status <> 'Disabled'", 0, 100);

                //Load Locker Type Name into Combo Box 2
                _comboBoxItems.Add(0, "All");
                foreach (Type t in _typeList)
                { _comboBoxItems.Add(t.Id, t.Name); }

                comboBox2.DataSource = new BindingSource(_comboBoxItems, null);
                comboBox2.DisplayMember = "Value";
                comboBox2.ValueMember = "Key";
                comboBox2.SelectedIndex = 0; //Default select all cabinets (ignore locker type)

                //Default select all cabinets (ignore cabinet condition)
                radioButton1.Checked = true;
            }
            else //Filter Cabinet for Deleted Cabinet
            {
                Controls.Add(panel3);
                this.Height = 150;
                _typeList = Type.Where("status IS NOT NULL", 0, 100);

                //Load Locker Type Name into Combo Box 3
                _comboBoxItems.Add(0, "All");
                foreach (Type t in _typeList)
                { _comboBoxItems.Add(t.Id, t.Name); }

                comboBox3.DataSource = new BindingSource(_comboBoxItems, null);
                comboBox3.DisplayMember = "Value";
                comboBox3.ValueMember = "Key";
                comboBox3.SelectedIndex = 0; //Default select all cabinets (ignore locker type)
            }
           
        }

        private void Button3_Click(object sender, EventArgs e) //Close button
        {
            this.Close();
        }

        private void Button4_Click(object sender, EventArgs e) //OK button for Normal Cabinet
        {
            var dictValue = from selected in _comboBoxItems
                            where selected.Value.Contains(comboBox2.Text)
                            select selected;
            if (comboBox2.SelectedIndex < 0 || !dictValue.Any())
            {
                MessageBox.Show("Input Error: Invalid input detected!" + Environment.NewLine +
                  "Please ensure that field 'Locker Type' was filled with provided items. ",
                  "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int typeId = Convert.ToInt32(dictValue.First().Key);
            if (typeId == 0)
            { _cabinetSize = "IS NOT NULL"; }
            else
            { _cabinetSize = String.Format(" = {0}", typeId); }
            _condition = "type_id {0} AND status {1}";
            _condition = String.Format(_condition, _cabinetSize, _cabinetStatus);
            this.Close();
        }

        private void Button6_Click(object sender, EventArgs e) //OK button for Deleted Cabinet
        {
            _cabinetStatus = " = 'Disabled'";
            var dictValue = from selected in _comboBoxItems
                            where selected.Value.Contains(comboBox3.Text)
                            select selected;
            if (comboBox3.SelectedIndex < 0 || !dictValue.Any())
            {
                MessageBox.Show("Input Error: Invalid input detected!" + Environment.NewLine +
                  "Please ensure that field 'Locker Type' was filled with provided items. ",
                  "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int typeId = Convert.ToInt32(dictValue.First().Key);
            if (typeId == 0)
            { _cabinetSize = "IS NOT NULL"; }
            else
            { _cabinetSize = String.Format(" = {0}", typeId); }
            _condition = "type_id {0} AND status {1}";
            _condition = String.Format(_condition, _cabinetSize, _cabinetStatus);
            this.Close();
        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            _cabinetStatus = "<> 'Disabled'";
        }
        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            _cabinetStatus = "= 'Available'";
        }
        private void RadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            _cabinetStatus = "= 'Full'";
        }

    }
}
