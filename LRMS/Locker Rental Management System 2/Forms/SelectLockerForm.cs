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
    public partial class SelectLockerForm : Form
    {
        private int _lockerId;
        private int _cabinetId;
        private int _typeId;
        private bool _lockerSelected = false;

        public int LockerID { get { return _lockerId; } set { _lockerId = value; } }
        public int CabinetID { get { return _cabinetId; } set { _cabinetId = value; } }
        public int TypeID { get { return _typeId; } set { _typeId = value; } }
        public bool LockerSelected { get { return _lockerSelected; } }

        Page lockerPage = new Page();
        Page smallCabinetPage = new Page();

        private List<Type> _typeList = new List<Type>();
        private Dictionary<int, string> _comboBoxItems = new Dictionary<int, string>();
        private string _cabinetStatus = "";
        private string _cabinetSize = "";
        private int _sortColumn = -1;

        //Select Locker in Add Rental
        public SelectLockerForm()
        {
            InitializeComponent();

            //Load Locker Type Name into Combo Box 1
            _typeList = Type.Where("status <> 'Disabled'", 0, 100);
            _comboBoxItems.Add(0, "All");
            foreach (Type t in _typeList)
            { _comboBoxItems.Add(t.Id, t.Name); }
            comboBox1.DataSource = new BindingSource(_comboBoxItems, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            comboBox1.SelectedIndex = -1;   //Trigger SelectedIndexChanged event
            comboBox1.SelectedIndex = 0;    //default select all cabinet (ignore locker type)

            //Default select the first available cabinet to load
            List<Cabinet> items = Cabinet.Where("status = 'Available'", 0, 1);
            if (!items.Any())
            {
                LockerPage(0);
            }
            else
            {
                _cabinetId = items[0].Id;
                textBox1.Text = items[0].Code;
                var locker = new Locker();
                textBox2.Text = locker.Count(String.Format("cabinet_id = {0} AND status = 'Available'", _cabinetId)).ToString();
                lockerPage.PageNumber = 1;
                LockerPage(_cabinetId);
            }
        }

        //Select Locker in Change Locker
        public SelectLockerForm(int lockerId)
        {
            InitializeComponent();

            //Get the size of the locker
            var locker = new Locker();
            var cabinet = new Cabinet();
            var type = new Type();

            locker = locker.Get(lockerId);
            cabinet = cabinet.Get(locker.CabinetID);
            type = type.Get(cabinet.TypeID);

            //Load Locker Type of the Locker ID in rental into Combo Box 1

            _comboBoxItems.Add(type.Id, type.Name);
            comboBox1.DataSource = new BindingSource(_comboBoxItems, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            comboBox1.SelectedIndex = -1;   //Trigger SelectedIndexChanged event
            comboBox1.SelectedIndex = 0;    //select the only type in combo box 1
            comboBox1.Enabled = false;      //Disable locker type (Disable comboBox1)

            //Load the cabinet which contains the old locker by default
            List<Cabinet> items = Cabinet.Where(String.Format("id = {0}", cabinet.Id), 0, 1);
            _cabinetId = items[0].Id;
            textBox1.Text = items[0].Code;
            textBox2.Text = locker.Count(String.Format("cabinet_id = {0} AND status = 'Available'", _cabinetId)).ToString();
            lockerPage.PageNumber = 1;
            LockerPage(_cabinetId);
        }

        private void ReloadSmallCabinetList(int count, int offset, string condition)
        {
            listView2.Items.Clear();

            List<Cabinet> items = Cabinet.Where(condition, count, offset);
            foreach (Cabinet cab in items)
            {
                ListViewItem lvi = new ListViewItem(cab.Id.ToString());
                lvi.SubItems.Add(cab.Code);
                lvi.SubItems.Add(cab.Status);

                listView2.Items.Add(lvi);
            }
        }

        private void SmallCabinetPage()
        {
            string condition = "status {0} AND type_id {1}";
            condition = String.Format(condition, _cabinetStatus, _cabinetSize);

            var cab = new Cabinet();
            smallCabinetPage.FinalIndex = cab.Count(condition);
            smallCabinetPage.LastPage = Convert.ToInt32(Math.Ceiling(smallCabinetPage.FinalIndex / smallCabinetPage.MaxItems));
            smallCabinetPage.PageSetting();
            if (smallCabinetPage.FinalIndex == 0)
            {
                smallCabinetPage.FirstIndex = 0;
                smallCabinetPage.LastIndex = 0;
                smallCabinetPage.LastPage = 1;
            }
            if (smallCabinetPage.PageNumber == smallCabinetPage.LastPage)
            { smallCabinetPage.LastIndex = (int)smallCabinetPage.FinalIndex; }
            toolStripLabel1.Text = String.Format("Page {0} / {1}", smallCabinetPage.PageNumber, smallCabinetPage.LastPage);
            ReloadSmallCabinetList(smallCabinetPage.IndexLimit, smallCabinetPage.MaxItems, condition);
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e) //Combo Box "Locker Type"
        {
            var dictValue = from selected in _comboBoxItems
                            where selected.Value.Contains(comboBox1.Text)
                            select selected;
            int typeId = dictValue.First().Key;
            if (typeId == 0)
            {
                _cabinetSize = "IS NOT NULL";
            }
            else
            {
                _cabinetSize = String.Format("= {0}", typeId);
            }
            _cabinetStatus = String.Format("{0}", "= 'Available'");
            smallCabinetPage.PageNumber = 1;
            SmallCabinetPage();
        }

        private void RadioButton3_CheckedChanged(object sender, EventArgs e) //Radio button "Full"
        {
            _cabinetStatus = String.Format("{0}", "= 'Full'");
            smallCabinetPage.PageNumber = 1;
            SmallCabinetPage();
        }

        private void ToolStripButton1_Click(object sender, EventArgs e) //First Page for Small Cabinet List
        {
            if (smallCabinetPage.PageNumber == 1)
                return;
            smallCabinetPage.PageNumber = 1;
            SmallCabinetPage();
        }

        private void ToolStripButton2_Click(object sender, EventArgs e) //Previous Page for Small Cabinet List
        {
            if (smallCabinetPage.PageNumber == 1)
            { return; }
            else
            {
                smallCabinetPage.PageNumber -= 1;
                SmallCabinetPage();
            }
        }

        private void ToolStripButton3_Click(object sender, EventArgs e) //Next Page for Small Cabinet List
        {
            if (smallCabinetPage.PageNumber == smallCabinetPage.LastPage)
                return;
            else
            {
                smallCabinetPage.PageNumber += 1;
                SmallCabinetPage();
            }
        }

        private void ToolStripButton4_Click(object sender, EventArgs e) //Last Page for Small Cabinet List
        {
            if (smallCabinetPage.PageNumber == smallCabinetPage.LastPage)
                return;
            smallCabinetPage.PageNumber = smallCabinetPage.LastPage;
            SmallCabinetPage();
        }

        private void Button1_Click(object sender, EventArgs e) //Select Cabinet button
        {
            if (listView2.SelectedItems.Count <= 0)
                return;
            ListViewItem lvi = listView2.SelectedItems[0];
            _cabinetId = Convert.ToInt32(lvi.Text);
            textBox1.Text = lvi.SubItems[1].Text;
            var locker = new Locker();
            textBox2.Text = locker.Count(String.Format("cabinet_id = {0} AND status = 'Available'", _cabinetId)).ToString();
            lockerPage.PageNumber = 1;
            LockerPage(_cabinetId);
        }

        private void Button2_Click(object sender, EventArgs e) //Cancel button
        {
            this.Close();
        }

        private void Button3_Click(object sender, EventArgs e) //Select Locker button
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            ListViewItem lvi = listView1.SelectedItems[0];
            string lockerCode = String.Format("code = '{0}'", lvi.Text);
            var locker = new Locker();
            List<Locker> lockerList = Locker.Where(lockerCode, 0, 1);
            var selectedLocker = locker.Get(lockerList[0].Id);
            if (selectedLocker.IsOccupied())
            {
                MessageBox.Show("Error: Locker Occupied" + Environment.NewLine + 
                    "You cannot select an occupied locker for the rental process.", "Locker Occupied", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (selectedLocker.IsNotAvailable())
            {
                MessageBox.Show("Error: Locker Not Available" + Environment.NewLine + 
                    "You cannot select a not available locker for the rental process.", "Locker Not Available", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                _lockerId = selectedLocker.Id;
                _cabinetId = selectedLocker.CabinetID;

                var cab = new Cabinet();
                var item = cab.Get(_cabinetId);
                _typeId = item.TypeID;

                this.Close();
                _lockerSelected = true;
            }
        }

        private void ListView2_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for small cabinet list
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView2.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView2.Sorting == SortOrder.Ascending)
                    listView2.Sorting = SortOrder.Descending;
                else
                    listView2.Sorting = SortOrder.Ascending;
            }

            listView2.Sort();
            this.listView2.ListViewItemSorter = new ListViewItemComparer(e.Column, listView2.Sorting);

        }

        private void ReloadLockerList(int count, int offset, string condition)
        {
            listView1.Items.Clear();
            List<Locker> items = Locker.Where(condition, count, offset);
            foreach (Locker locker in items)
            {
                ListViewItem lvi = new ListViewItem(locker.Code);
                lvi.SubItems.Add(locker.Status);
                if (locker.Status == "Available")
                {
                    lvi.ImageIndex = 0;
                }
                else if (locker.Status == "Occupied")
                {
                    lvi.ImageIndex = 1;
                }
                else if (locker.Status == "Not Available")
                {
                    lvi.ImageIndex = 2;
                }
                else
                {
                    lvi.ImageIndex = 3;
                }
                listView1.Items.Add(lvi);
            }
        }

        private void LockerPage(int cabinetId)
        {
            string condition = String.Format("cabinet_id = {0}", cabinetId);

            var locker = new Locker();
            lockerPage.FinalIndex = Convert.ToDouble(locker.Count(condition));
            lockerPage.LastPage = Convert.ToInt32(Math.Ceiling(lockerPage.FinalIndex / lockerPage.MaxItems));
            lockerPage.PageSetting();
            if (lockerPage.FinalIndex == 0)
            {
                lockerPage.FirstIndex = 0;
                lockerPage.LastIndex = 0;
                lockerPage.LastPage = 1;
            }
            if (lockerPage.PageNumber == lockerPage.LastPage)
            { lockerPage.LastIndex = (int)lockerPage.FinalIndex; }
            toolStripLabel2.Text = String.Format("Page {0} / {1}", lockerPage.PageNumber, lockerPage.LastPage);
            toolStripLabel3.Text = String.Format("Showing result {0}~{1}", lockerPage.FirstIndex, lockerPage.LastIndex);
            ReloadLockerList(lockerPage.IndexLimit, lockerPage.MaxItems, condition);
        }

        private void ToolStripButton5_Click(object sender, EventArgs e) //First Page for Locker
        {
            if (lockerPage.PageNumber == 1)
                return;
            lockerPage.PageNumber = 1;
            LockerPage(_cabinetId);
        }

        private void ToolStripButton6_Click(object sender, EventArgs e) //Previous Page for Locker 
        {
            if (lockerPage.PageNumber == 1)
                return;
            else
            {
                lockerPage.PageNumber -= 1;
                LockerPage(_cabinetId);
            }
        }

        private void ToolStripButton7_Click(object sender, EventArgs e) //Next Page for Locker
        {
            if (lockerPage.PageNumber == lockerPage.LastPage)
                return;
            else
            {
                lockerPage.PageNumber += 1;
                LockerPage(_cabinetId);
            }
        }

        private void ToolStripButton8_Click(object sender, EventArgs e) //Last Page for Locker
        {
            if (lockerPage.PageNumber == lockerPage.LastPage)
                return;
            lockerPage.PageNumber = lockerPage.LastPage;
            LockerPage(_cabinetId);
        }
    }
}
