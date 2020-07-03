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
    public partial class SelectCustomerForm : Form
    {
        Page customerPage = new Page(); 
        private bool search = false;
        private string searchCondition = "";
        private int _sortColumn = -1;

        private int customerId;
        public int CustomerId { get { return customerId; } set { customerId = value; } }

        public SelectCustomerForm()
        {
            InitializeComponent();
            customerPage.PageReset();
            customerPage.PageNumber = 1;
            CustomerPage();
        }

        private void ReloadCustomerList(int limit, int offset, string condition)
        {
            listView1.Items.Clear();
            List<Customer> items = Customer.Where(condition, limit, offset);
            foreach (Customer c in items)
            {
                ListViewItem lvi = new ListViewItem(c.Id.ToString());
                lvi.SubItems.Add(c.Name);
                lvi.SubItems.Add(c.Ic);

                listView1.Items.Add(lvi);
            }
        }

        private void CustomerPage()
        {
            string condition = "status <> 'Disabled'";
            var cust = new Customer();
            customerPage.FinalIndex = Convert.ToDouble(cust.Count(condition));
            customerPage.LastPage = Convert.ToInt32(Math.Ceiling(customerPage.FinalIndex / customerPage.MaxItems));
            customerPage.PageSetting();
            if (customerPage.FinalIndex == 0)
            {
                customerPage.FirstIndex = 0;
                customerPage.LastIndex = 0;
                customerPage.IndexLimit = 1;
                customerPage.PageNumber = 1;
                customerPage.LastPage = 1;
            }
            if (customerPage.PageNumber == customerPage.LastPage)
            { customerPage.LastIndex = (int)customerPage.FinalIndex; }
            toolStripLabel1.Text = String.Format("Page {0} / {1}", customerPage.PageNumber, customerPage.LastPage);
            toolStripLabel2.Text = String.Format("Showing result {0}~{1}", customerPage.FirstIndex, customerPage.LastIndex);
            ReloadCustomerList(customerPage.IndexLimit, customerPage.MaxItems, condition);
        }

        private void CustomerPage(string condition)
        {
            var cust = new Customer();
            customerPage.FinalIndex = Convert.ToDouble(cust.Count(condition));
            customerPage.LastPage = Convert.ToInt32(Math.Ceiling(customerPage.FinalIndex / customerPage.MaxItems));
            customerPage.PageSetting();
            if (customerPage.FirstIndex == 0)
            {
                customerPage.FirstIndex = 0;
                customerPage.LastIndex = 0;
                customerPage.IndexLimit = 1;
                customerPage.PageNumber = 1;
                customerPage.LastPage = 1;
            }
            if (customerPage.PageNumber == customerPage.LastPage)
            { customerPage.LastIndex = (int)customerPage.FinalIndex; }
            toolStripLabel1.Text = String.Format("Page {0} / {1}", customerPage.PageNumber, customerPage.LastPage);
            toolStripLabel2.Text = String.Format("Showing result {0}~{1}", customerPage.FirstIndex, customerPage.LastIndex);
            ReloadCustomerList(customerPage.IndexLimit, customerPage.MaxItems, condition);
        }

        private void ToolStripButton1_Click(object sender, EventArgs e) //Search button
        {
            string item;
            string search_value;
            if (toolStripComboBox1.Text == "IC Number")
            { item = "ic"; }
            else if (toolStripComboBox1.Text == "Name")
            { item = "name"; }
            else
            { return; }

            if (toolStripComboBox2.Text == "Start with")
            { search_value = "'{0}%'"; }
            else if (toolStripComboBox2.Text == "End with")
            { search_value = "'%{0}'"; }
            else if (toolStripComboBox2.Text == "Contains")
            { search_value = "'%{0}%'"; }
            else
            { return; }

            if (string.IsNullOrWhiteSpace(toolStripTextBox1.Text))
            { return; }

            search_value = String.Format(search_value, toolStripTextBox1.Text);

            listView1.Items.Clear();

            string condition = "{0} LIKE {1} AND status <> 'Disabled'";
            searchCondition = String.Format(condition, item, search_value);

            search = true;
            customerPage.PageNumber = 1;
            CustomerPage(searchCondition);
        }

        private void ToolStripButton2_Click(object sender, EventArgs e) //FirstPage button
        {
            if (customerPage.PageNumber == 1)
            { return; }
            customerPage.PageNumber = 1;
            if (!search)
                CustomerPage();
            else
                CustomerPage(searchCondition);
        }

        private void ToolStripButton3_Click(object sender, EventArgs e) //PreviousPage Button
        {
            if (customerPage.PageNumber == 1)
            { return; }
            else
            {
                customerPage.PageNumber -= 1;
                if (!search)
                    CustomerPage();
                else
                    CustomerPage(searchCondition);
            }
        }

        private void ToolStripButton4_Click(object sender, EventArgs e) //NextPage Button
        {
            if (customerPage.PageNumber == customerPage.LastPage)
            { return; }
            else
            {
                customerPage.PageNumber += 1;
                if (!search)
                    CustomerPage();
                else
                    CustomerPage(searchCondition);
            }
        }

        private void ToolStripButton5_Click(object sender, EventArgs e) //LastPage Button
        {
            if (customerPage.PageNumber == customerPage.LastPage)
            { return; }
            customerPage.PageNumber = customerPage.LastPage;
            if (!search)
                CustomerPage();
            else
                CustomerPage(searchCondition);
        }

        private void ToolStripButton6_Click(object sender, EventArgs e) //Refresh button
        {
            search = false;
            searchCondition = "";
            toolStripComboBox1.SelectedIndex = -1;
            toolStripComboBox2.SelectedIndex = -1;
            toolStripTextBox1.Text = "";
            customerPage.PageNumber = 1;
            CustomerPage();
        }

        private void Button1_Click(object sender, EventArgs e) //Cancel button
        {
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e) //Select button
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            ListViewItem lvi = listView1.SelectedItems[0];
            customerId = Convert.ToInt32(lvi.Text);
            this.Close();
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView1.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView1.Sorting == SortOrder.Ascending)
                    listView1.Sorting = SortOrder.Descending;
                else
                    listView1.Sorting = SortOrder.Ascending;
            }

            listView1.Sort();
            this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, listView1.Sorting);
        }
    }
}
