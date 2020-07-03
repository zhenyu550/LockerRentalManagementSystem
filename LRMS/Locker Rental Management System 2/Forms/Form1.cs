using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using MySql.Data.MySqlClient;


namespace LockerRentalManagementSystem
{
    public partial class Form1 : Form
    {
        //Global variables
        bool Logout = false;
        bool rentalSettingsCompleted = false;
        private bool search = false;
        private string _searchCondition = "";
        private int _sortColumn = -1;

        private List<Type> _typeList = new List<Type>();
        private Dictionary<int, string> _comboBoxItems = new Dictionary<int, string>();
        private string _cabinetStatus = "";
        private string _cabinetSize = "";
        private string _oldCabinetStatus = "";
        private string _oldCabinetSize = "";
        private bool _pageFlip = false;
        private int _cabinetId;

        //Delcare all page number for all modules (for their list view)
        Page rentalPage = new Page();
        Page customerPage = new Page();
        Page employeePage = new Page();
        Page smallCabinetPage = new Page();
        Page cabinetPage = new Page();
        Page lockerPage = new Page();
        Page lockerTypePage = new Page();
        Page accessLogPage = new Page();
        Page deletedCustomerPage = new Page();
        Page deletedEmployeePage = new Page();
        Page deletedLockerTypePage = new Page();
        Page deletedCabinetPage = new Page();
        Page transactionPage = new Page();

        //Form Loading / Closing Events
        public Form1()
        {
            InitializeComponent();
            List<Employee> item = Employee.Where(string.Format("username = '{0}'", Login.Username), 0, 1);
            var emp = item[0];
            label2.Text = string.Format("{0} ({1})", emp.Name, emp.Username);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Controls.Remove(tabControl1);
            this.Controls.Add(panel1_MainBasePanel);
            panel10.Controls.Remove(tabControl2);
            panel10.Controls.Add(panel6_RentalBasePanel);

            search = false;
            RentalPage();

            //Initialize locker to prevent NULL condition in Locker (Small cabinet), that will cause errors when adding / remove cabinets
            _typeList = Type.Where("status <> 'Disabled'", 0, 100);
            _comboBoxItems.Add(0, "All");
            foreach (Type t in _typeList)
            { _comboBoxItems.Add(t.Id, t.Name); }
            comboBox1.DataSource = new BindingSource(_comboBoxItems, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            radioButton1.Checked = true;

            //Check if rental settings was set, if no, direct user to set rental settings.
            var rentalCheck = new RentalSettings();
            int rowAmount = rentalCheck.Count("id IS NOT NULL");
            if (rowAmount >= 4)
                rentalSettingsCompleted = true;

            if (!rentalSettingsCompleted)
            {
                MessageBox.Show("Load Error: Rental Settings Not Detected" + Environment.NewLine + 
                    "No rental settings detected. You are required to set up the rental settings.", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                var forceSetRental = new RentalSettingsForm(rentalSettingsCompleted);
                forceSetRental.ShowDialog();
                rentalSettingsCompleted = forceSetRental.InsertComplete;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Logout)
            {
                var result = MessageBox.Show("Do you want to exit?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                { e.Cancel = true; }
                else
                {
                    var log = new AccessLog
                    {
                        User = Login.Username,
                        Action = "Log Out",
                        Item = "",
                        ItemId = ""
                    };
                    log.Insert();
                    Database.Disconnect();
                }
            }
        }

        //Main Modules
        private void Button1_Click(object sender, EventArgs e) //Log Out button
        {
            var result = MessageBox.Show("Do you want to log out?", "Log Out", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {

                var log = new AccessLog
                {
                    User = Login.Username,
                    Action = "Log Out",
                    Item = "",
                    ItemId = ""
                };
                log.Insert();

                Logout = true;
                Login LoginForm = new Login();
                LoginForm.Show();
                this.Close();
            }
        }

        private void Button2_Click(object sender, EventArgs e) //Admin Panel button
        {
            List<Employee> item = Employee.Where(string.Format("username = '{0}'", Login.Username), 0, 1);
            var emp = item[0];

            if (!emp.IsAdmin())
            {
                MessageBox.Show("Error: Acccess Denied. " + Environment.NewLine +
                    "Only administrators can access the admin panel.", "Access Denied", MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
            else
            {
                Controls.Add(panel2_AdminBasePanel);
                Controls.Remove(panel1_MainBasePanel);
                panel11.Controls.Remove(tabControl3);
                panel11.Controls.Add(panel12_EmployeeBasePanel);

                //Reset Employee page to Page 1
                search = false;
                _searchCondition = "";
                EmployeePage();
            }
        }

        private void Button45_Click(object sender, EventArgs e) //Rental Settings button
        {
            var RentalSettings = new RentalSettingsForm(rentalSettingsCompleted);
            RentalSettings.ShowDialog();
        }

        private void Button3_Click(object sender, EventArgs e) //Return Main Panel button
        {
            Controls.Add(panel1_MainBasePanel);
            Controls.Remove(panel2_AdminBasePanel);
            search = false;
        }

        private void Button8_Click(object sender, EventArgs e) //Exit button
        {
            this.Close();
        }

        private void Button41_Click(object sender, EventArgs e) //Generate Sales Report button
        {
            var GenerateSalesReportForm = new SalesReportForm();
            GenerateSalesReportForm.ShowDialog();
        }


        //Customer Modules
        private void ReloadCustomerList(int offset, int count, string condition)
        {
            listView2.Items.Clear();
            List<Customer> items = Customer.Where(condition, offset, count);
            foreach (Customer c in items)
            {
                ListViewItem lvi = new ListViewItem(c.Id.ToString());
                lvi.SubItems.Add(c.Name);
                lvi.SubItems.Add(c.Ic);

                listView2.Items.Add(lvi);
            }
        }

        private void Button5_Click(object sender, EventArgs e) //Customer button 
        {
            panel10.Controls.Add(panel8_CustomerBasePanel);       //Customer
            panel10.Controls.Remove(panel6_RentalBasePanel);    //Rental
            panel10.Controls.Remove(panel9_LockerBasePanel);    //Locker
            search = false;
            customerPage.PageReset();
            CustomerPage();
        }

        private void Button17_Click(object sender, EventArgs e) //Add Customer button
        {
            CustomerForm AddCustomerForm = new CustomerForm();
            AddCustomerForm.ShowDialog();

            if (!AddCustomerForm.InsertComplete)
                return;

            var cus = new Customer();
            if (!search)
            {
                customerPage.FinalIndex = Convert.ToDouble(cus.Count("status <> 'Disabled'"));
                customerPage.LastPage = Convert.ToInt32(Math.Ceiling(customerPage.FinalIndex / customerPage.MaxItems));
                customerPage.PageNumber = customerPage.LastPage;
                CustomerPage();
            }
            else
            {
                customerPage.FinalIndex = Convert.ToDouble(cus.Count(_searchCondition));
                customerPage.LastPage = Convert.ToInt32(Math.Ceiling(customerPage.FinalIndex / customerPage.MaxItems));
                customerPage.PageNumber = customerPage.LastPage;
                CustomerPage(_searchCondition);
            }
        }

        private void Button19_Click(object sender, EventArgs e) //Delete Customer button
        {
            if (listView2.SelectedItems.Count <= 0)
                return;
            var result = MessageBox.Show("Do you want to delete this customer?", "Delete Customer", MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView2.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);

                //Check if Customer involve in any rental
                var rental = new Rental();
                int noOfRental = rental.Count(String.Format("customer_id = {0}", id));
                if (noOfRental > 0)
                {
                    MessageBox.Show("Deletion Error: Rental detected." + Environment.NewLine +
                        "To delete this customer, please ensure that all rentals assoicated to this customer was ended.",
                        "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Customer cust = new Customer();
                cust = cust.Get(id);
                cust.TempDelete();

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Delete",
                    Item = "Customer",
                    ItemId = Convert.ToString(id),
                    Description = "IC: " + cust.Ic + "; Status: Active to Disabled"
                };
                log.Insert();
                if (!search)
                    CustomerPage();
                else
                    CustomerPage(_searchCondition);
            }
        }

        private void Button18_Click(object sender, EventArgs e) //Edit Customer button
        {
            if (listView2.SelectedItems.Count <= 0)
                return;
            ListViewItem lvi = listView2.SelectedItems[0];
            int id = Convert.ToInt32(lvi.Text);
            CustomerForm EditCustomerFrom = new CustomerForm(id);
            EditCustomerFrom.ShowDialog();
            if (!search)
                CustomerPage();
            else
                CustomerPage(_searchCondition);
        }

        private void ToolStripButton10_Click(object sender, EventArgs e) //Search button in Customer
        {
            string item;
            string searchValue;
            if (toolStripComboBox2.Text == "IC Number")
            { item = "ic"; }
            else if (toolStripComboBox2.Text == "Name")
            { item = "name"; }
            else
            { return; }

            if (toolStripComboBox5.Text == "Start with")
            { searchValue = "'{0}%'"; }
            else if (toolStripComboBox5.Text == "End with")
            { searchValue = "'%{0}'"; }
            else if (toolStripComboBox5.Text == "Contains")
            { searchValue = "'%{0}%'"; }
            else
            { return; }

            if (string.IsNullOrWhiteSpace(toolStripTextBox2.Text))
            { return; }

            searchValue = String.Format(searchValue, toolStripTextBox2.Text);

            listView2.Items.Clear();

            string condition = "{0} LIKE {1} AND status <> 'Disabled'";
            _searchCondition = String.Format(condition, item, searchValue);

            search = true;
            customerPage.PageNumber = 1;
            CustomerPage(_searchCondition);
        }

        private void ToolStripButton11_Click(object sender, EventArgs e) //First Page in Customer
        {
            if (customerPage.PageNumber == 1)
            { return; }
            customerPage.PageNumber = 1;
            if (!search)
                CustomerPage();
            else
                CustomerPage(_searchCondition);
        }

        private void ToolStripButton12_Click(object sender, EventArgs e) //Previous Page in Customer
        {
            if (customerPage.PageNumber == 1)
            { return; }
            else
            {
                customerPage.PageNumber -= 1;
                if (!search)
                    CustomerPage();
                else
                    CustomerPage(_searchCondition);
            }    
        }

        private void ToolStripButton13_Click(object sender, EventArgs e) //Next Page in Customer
        {
            if (customerPage.PageNumber == customerPage.LastPage)
            { return; }
            else
            {
                customerPage.PageNumber += 1;
                if (!search)
                    CustomerPage();
                else
                    CustomerPage(_searchCondition);
            }
        }

        private void ToolStripButton14_Click(object sender, EventArgs e) //Last Page in Customer
        {
            if (customerPage.PageNumber == customerPage.LastPage)
            { return; }
            customerPage.PageNumber = customerPage.LastPage;
            if (!search)
                CustomerPage();
            else
                CustomerPage(_searchCondition);
        }

        private void ToolStripButton15_Click(object sender, EventArgs e) //Refresh button for Customer
        {
            search = false;
            _searchCondition = "";
            toolStripComboBox2.SelectedIndex = -1;
            toolStripComboBox5.SelectedIndex = -1;
            toolStripTextBox2.Text = "";
            customerPage.PageNumber = 1;
            CustomerPage();
        }

        private void ListView2_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting in Customer List
        {
            // Determine whether the column is the same as the last column clicked.
            if (e.Column != _sortColumn)
            {
                // Set the sort column to the new column.
                _sortColumn = e.Column;
                // Set the sort order to ascending by default.
                listView2.Sorting = SortOrder.Ascending;
            }
            else
            {
                // Determine what the last sort order was and change it.
                if (listView2.Sorting == SortOrder.Ascending)
                    listView2.Sorting = SortOrder.Descending;
                else
                    listView2.Sorting = SortOrder.Ascending;
            }

            // Call the sort method to manually sort.
            listView2.Sort();
            // Set the ListViewItemSorter property to a new ListViewItemComparer object.
            this.listView2.ListViewItemSorter = new ListViewItemComparer(e.Column, listView2.Sorting);
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
                customerPage.PageReset();
            }
            if (customerPage.PageNumber == customerPage.LastPage)
            { customerPage.LastIndex = (int)customerPage.FinalIndex; }
            toolStripLabel8.Text = String.Format("Page {0} / {1}", customerPage.PageNumber, customerPage.LastPage);
            toolStripLabel9.Text = String.Format("Showing result {0}~{1}", customerPage.FirstIndex, customerPage.LastIndex);
            ReloadCustomerList(customerPage.IndexLimit, customerPage.MaxItems, condition);
        }

        private void CustomerPage(string condition)
        {
            var cust = new Customer();
            customerPage.FinalIndex = Convert.ToDouble(cust.Count(condition));

            customerPage.LastPage = Convert.ToInt32(Math.Ceiling(customerPage.FinalIndex / customerPage.MaxItems));
            customerPage.PageSetting();
            if (customerPage.FinalIndex == 0)
            {
                customerPage.PageReset();
            }
            if (customerPage.PageNumber == customerPage.LastPage)
            { customerPage.LastIndex = (int)customerPage.FinalIndex; }
            toolStripLabel8.Text = String.Format("Page {0} / {1}", customerPage.PageNumber, customerPage.LastPage);
            toolStripLabel9.Text = String.Format("Showing result {0}~{1}", customerPage.FirstIndex, customerPage.LastIndex);
            ReloadCustomerList(customerPage.IndexLimit, customerPage.MaxItems, condition);
        }


        //Employee Modules
        private void ReloadEmployeeList(int count, int offset, string condition)
        {
            listView1.Items.Clear();
            List<Employee> items = Employee.Where(condition, count, offset);
            foreach (Employee e in items)
            {
                ListViewItem lvi = new ListViewItem(e.Id.ToString());
                lvi.SubItems.Add(e.Name);
                lvi.SubItems.Add(e.Ic);
                lvi.SubItems.Add(e.Position);
                lvi.SubItems.Add(e.Username);
                lvi.SubItems.Add(e.Permission);

                listView1.Items.Add(lvi);
            }
        }

        private void Button12_Click(object sender, EventArgs e) //Employee button
        {
            panel11.Controls.Add(panel12_EmployeeBasePanel);
            panel11.Controls.Remove(panel13_CabinetBasePanel);
            panel11.Controls.Remove(panel15_TransactionBasePanel);
            panel11.Controls.Remove(panel16_DeletedRecordsBasePanel);
            panel11.Controls.Remove(panel14_LogBasePanel);
            search = false;
            employeePage.PageReset();
            EmployeePage();
        }

        private void Button20_Click(object sender, EventArgs e) //Add Employee button
        {
            EmployeeForm AddEmployeeForm = new EmployeeForm(true);
            AddEmployeeForm.ShowDialog();
            if (!AddEmployeeForm.InsertComplete)
                return;

            var emp = new Employee();
            if (!search)
            {
                employeePage.FinalIndex = Convert.ToDouble(emp.Count("status <> 'Disabled'"));
                employeePage.LastPage = Convert.ToInt32(Math.Ceiling(employeePage.FinalIndex / employeePage.MaxItems));
                employeePage.PageNumber = employeePage.LastPage;
                EmployeePage();
            }
            else
            {
                employeePage.FinalIndex = Convert.ToDouble(emp.Count(_searchCondition));
                employeePage.LastPage = Convert.ToInt32(Math.Ceiling(employeePage.FinalIndex / employeePage.MaxItems));
                employeePage.PageNumber = employeePage.LastPage;
                EmployeePage(_searchCondition);
            }
        }     

        private void Button21_Click(object sender, EventArgs e) //Delete Employee button
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            var result = MessageBox.Show("Do you want to delete this employee?", "Delete Employee", MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView1.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);
                if (id == 1)
                {
                    MessageBox.Show("Deletion Error: Unable to delete main admin." + Environment.NewLine +
                        "This employee cannot be deleted as he/she was registered as main admin.", "Deletion Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    Employee emp = new Employee();
                    emp = emp.Get(id);
                    emp.TempDelete();

                    var log = new AccessLog()
                    {
                        User = Login.Username,
                        Action = "Delete",
                        Item = "Employee",
                        ItemId = Convert.ToString(id),
                        Description = "IC: " + emp.Ic + "; Status: Active to Disabled"
                    };
                    log.Insert();
                }
                if (!search)
                    EmployeePage();
                else
                    EmployeePage(_searchCondition);
            }
        }
   
        private void Button7_Click(object sender, EventArgs e) //Edit Employee button
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            ListViewItem lvi = listView1.SelectedItems[0];
            int id = Convert.ToInt32(lvi.Text);
            var EditEmployeeFrom = new EmployeeForm(true,id);
            EditEmployeeFrom.ShowDialog();
            if (!search)
                EmployeePage();
            else
                EmployeePage(_searchCondition);
        }

        private void ToolStripButton1_Click(object sender, EventArgs e) //Search button in Employee
        {
            string item;
            string searchValue;
            
            if (toolStripComboBox1.Text == "IC Number")
            { item = "ic";}
            else if (toolStripComboBox1.Text == "Name")
            { item = "name";}
            else if (toolStripComboBox1.Text == "Username")
            { item = "username";}
            else
            { return;}
                    
            if (toolStripComboBox7.Text == "Start with")
            { searchValue = "'{0}%'";}
            else if (toolStripComboBox7.Text == "End with")
            { searchValue = "'%{0}'";}
            else if (toolStripComboBox7.Text == "Contains")
            { searchValue = "'%{0}%'";}
            else
            { return; }

            if (string.IsNullOrWhiteSpace(toolStripTextBox1.Text))
            { return; }

            searchValue = String.Format(searchValue, toolStripTextBox1.Text);
            
            _searchCondition = "{0} LIKE {1} AND status <> 'Disabled'";
            _searchCondition = String.Format(_searchCondition, item, searchValue);

            search = true;
            employeePage.PageNumber = 1;
            EmployeePage(_searchCondition);
        }

        private void ToolStripButton3_Click(object sender, EventArgs e) //FirstPage in Employee
        {
            if (employeePage.PageNumber == 1)
            { return; }
            employeePage.PageNumber = 1;
            if (!search)
            { EmployeePage(); }
            else
            { EmployeePage(_searchCondition); }
        }

        private void ToolStripButton2_Click(object sender, EventArgs e) //PreviousPage in Employee
        {
            if (employeePage.PageNumber == 1)
            { return; }
            else
            {
                employeePage.PageNumber -= 1;
                if (!search)
                { EmployeePage(); }
                else
                { EmployeePage(_searchCondition); }
            }
        }

        private void ToolStripButton4_Click(object sender, EventArgs e) //NextPage in Employee
        {

            if (employeePage.PageNumber == employeePage.LastPage)
            { return; }
            else
            {
                employeePage.PageNumber += 1;
                if (!search)
                { EmployeePage(); }
                else
                { EmployeePage(_searchCondition); }
            }
        }

        private void ToolStripButton5_Click(object sender, EventArgs e) //LastPage in Employee
        {

            if (employeePage.PageNumber == employeePage.LastPage)
            { return; }
            employeePage.PageNumber = employeePage.LastPage;
            if (!search)
            { EmployeePage(); }
            else
            { EmployeePage(_searchCondition); }

        }

        private void ToolStripButton16_Click(object sender, EventArgs e) //Refresh button for Employee
        {
            search = false;
            _searchCondition = "";
            toolStripComboBox1.SelectedIndex = -1;
            toolStripComboBox7.SelectedIndex = -1;
            toolStripTextBox1.Text = "";
            employeePage.PageNumber = 1;
            EmployeePage();
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Employee List
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

        private void EmployeePage()
        {
            string condition = "status <> 'Disabled'";
            var emp = new Employee();
            employeePage.FinalIndex = Convert.ToDouble(emp.Count(condition));
            employeePage.LastPage = Convert.ToInt32(Math.Ceiling(employeePage.FinalIndex / employeePage.MaxItems));
            employeePage.PageSetting();
            if (employeePage.FinalIndex == 0)
            {
                employeePage.PageReset();
            }
            if (employeePage.PageNumber == employeePage.LastPage)
            { employeePage.LastIndex = (int)employeePage.FinalIndex; }

            toolStripLabel4.Text = String.Format("Page {0} / {1}", employeePage.PageNumber, employeePage.LastPage);
            toolStripLabel1.Text = String.Format("Showing result {0}~{1}", employeePage.FirstIndex, employeePage.LastIndex);
            ReloadEmployeeList(employeePage.IndexLimit, employeePage.MaxItems, condition);
        }

        private void EmployeePage(string condition)
        {
            var emp = new Employee();
            employeePage.FinalIndex = Convert.ToDouble(emp.Count(condition));
            employeePage.LastPage = Convert.ToInt32(Math.Ceiling(employeePage.FinalIndex / employeePage.MaxItems));
            employeePage.PageSetting();
            if (employeePage.FinalIndex == 0)
            {
                employeePage.PageReset();
            }
            if (employeePage.PageNumber == employeePage.LastPage)
            { employeePage.LastIndex = (int)employeePage.FinalIndex; }

            toolStripLabel4.Text = String.Format("Page {0} / {1}", employeePage.PageNumber, employeePage.LastPage);
            toolStripLabel1.Text = String.Format("Showing result {0}~{1}", employeePage.FirstIndex, employeePage.LastIndex);
            ReloadEmployeeList(employeePage.IndexLimit, employeePage.MaxItems, condition);
        }


        //Access Log Modules
        private void ReloadLogList(int count, int offset, string condition)
        {
            listView3.Items.Clear();
            List<AccessLog> items = AccessLog.Where(condition, count, offset);
            foreach (AccessLog a in items)
            {
                ListViewItem lvi = new ListViewItem(a.Id.ToString());
                string logDateTime = a.LogDate + "   " + a.LogTime;
                lvi.SubItems.Add(logDateTime);
                lvi.SubItems.Add(a.User);
                lvi.SubItems.Add(a.Action);
                lvi.SubItems.Add(a.Item);
                lvi.SubItems.Add(a.ItemId);
                lvi.SubItems.Add(a.Description);

                listView3.Items.Add(lvi);
            }
        }

        private void Button16_Click(object sender, EventArgs e)  //Access Log Button
        {
            panel11.Controls.Remove(panel12_EmployeeBasePanel);
            panel11.Controls.Remove(panel13_CabinetBasePanel);
            panel11.Controls.Remove(panel15_TransactionBasePanel);
            panel11.Controls.Remove(panel16_DeletedRecordsBasePanel);
            panel11.Controls.Add(panel14_LogBasePanel);
            accessLogPage.PageReset();
            LogPage();
            search = false;
            _searchCondition = "";
        }

        private void ToolStripButton6_Click(object sender, EventArgs e) //FirstPage button in Access Log
        {
            if (accessLogPage.PageNumber == 1)
            { return; }
            accessLogPage.PageNumber = 1;
            if (!search)
            { LogPage(); }
            else
            { LogPage(_searchCondition); }
        }

        private void ToolStripButton7_Click(object sender, EventArgs e) //PreviousPage button in Access Log
        {
            if (accessLogPage.PageNumber == 1)
            { return; }
            else
            {
                accessLogPage.PageNumber -= 1;
                if (!search)
                { LogPage(); }
                else
                { LogPage(_searchCondition); }
            }
        }

        private void ToolStripButton8_Click(object sender, EventArgs e) //NextPage button in Access Log
        {
            if(accessLogPage.PageNumber == accessLogPage.LastPage)
            { return; }
            else
            {
                accessLogPage.PageNumber += 1;
                if (!search)
                { LogPage(); }
                else
                { LogPage(_searchCondition); }
            }
        }

        private void ToolStripButton9_Click(object sender, EventArgs e) //LastPage button in Access Log
        {
            if (accessLogPage.PageNumber == accessLogPage.LastPage)
            { return; }
            accessLogPage.PageNumber = accessLogPage.LastPage;
            if (!search)
            { LogPage(); }
            else
            { LogPage(_searchCondition); }
        }

        private void ToolStripButton17_Click(object sender, EventArgs e) //Refresh button in Access Log
        {
            search = false;
            accessLogPage.PageNumber = 1;
            LogPage(); 
        }

        private void ListView3_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Access Log
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView3.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView3.Sorting == SortOrder.Ascending)
                    listView3.Sorting = SortOrder.Descending;
                else
                    listView3.Sorting = SortOrder.Ascending;
            }

            listView3.Sort();
            this.listView3.ListViewItemSorter = new ListViewItemComparer(e.Column, listView3.Sorting);
        }

        private void Button24_Click(object sender, EventArgs e) //Filter Access Log button
        {
            var FilterLogForm = new AccessLogFilterForm();
            FilterLogForm.ShowDialog();
            if(String.IsNullOrWhiteSpace(FilterLogForm.Condition))
            { return; }
            else
            {
                _searchCondition = FilterLogForm.Condition;
                search = true;
                accessLogPage.PageNumber = 1;
                LogPage(_searchCondition);
            }
        }

        private void Button25_Click(object sender, EventArgs e) //Export Access Log button
        {
            var ExportLogForm = new AccessLogFilterForm(true);
            ExportLogForm.ShowDialog();
            if (!ExportLogForm.ExportComplete)
                return;

            if (!search)
            { LogPage(); }
            else
            { LogPage(_searchCondition); }
        }

        private void Button36_Click(object sender, EventArgs e) //View Full Description button
        {
            if (listView3.SelectedItems.Count <= 0)
                return;
            ListViewItem lvi = listView3.SelectedItems[0];
            int id = Convert.ToInt32(lvi.Text);
            var ViewDescriptionForm = new AccessLogFilterForm(id);
            ViewDescriptionForm.Show();
        }

        private void LogPage()
        {
            string condition = "id IS NOT NULL";
            var log = new AccessLog();
            accessLogPage.FinalIndex = Convert.ToDouble(log.Count(condition));
            accessLogPage.LastPage = Convert.ToInt32(Math.Ceiling(accessLogPage.FinalIndex / accessLogPage.MaxItems));
            accessLogPage.PageSetting();
            if (accessLogPage.FinalIndex == 0)
            {
                accessLogPage.PageReset();
            }
            if (accessLogPage.PageNumber == accessLogPage.LastPage)
            { accessLogPage.LastIndex = (int)accessLogPage.FinalIndex; }

            toolStripLabel5.Text = String.Format("Page {0} / {1}", accessLogPage.PageNumber, accessLogPage.LastPage);
            toolStripLabel6.Text = String.Format("Showing result {0}~{1}", accessLogPage.FirstIndex, accessLogPage.LastIndex);
            
            ReloadLogList(accessLogPage.IndexLimit, accessLogPage.MaxItems, condition);
        }

        private void LogPage(string condition)
        {
            var log = new AccessLog();
            accessLogPage.FinalIndex = Convert.ToDouble(log.Count(condition));
            accessLogPage.LastPage = Convert.ToInt32(Math.Ceiling(accessLogPage.FinalIndex / accessLogPage.MaxItems));
            accessLogPage.PageSetting();
            if (accessLogPage.FinalIndex == 0)
            {
                accessLogPage.PageReset();
            }
            if (accessLogPage.PageNumber == accessLogPage.LastPage)
            { accessLogPage.LastIndex = (int)accessLogPage.FinalIndex; }

            toolStripLabel5.Text = String.Format("Page {0} / {1}", accessLogPage.PageNumber, accessLogPage.LastPage);
            toolStripLabel6.Text = String.Format("Showing result {0}~{1}", accessLogPage.FirstIndex, accessLogPage.LastIndex);
            
            ReloadLogList(accessLogPage.IndexLimit, accessLogPage.MaxItems, condition);
        }


        // Cabinet Module
        private void Button13_Click(object sender, EventArgs e) //Cabinet button
        {
            panel11.Controls.Remove(panel12_EmployeeBasePanel);
            panel11.Controls.Add(panel13_CabinetBasePanel);
            panel11.Controls.Remove(panel15_TransactionBasePanel);
            panel11.Controls.Remove(panel16_DeletedRecordsBasePanel);
            panel11.Controls.Remove(panel14_LogBasePanel);
            search = false;
            cabinetPage.PageReset();
            TypePage();
            CabinetPage();
        }

        private void ReloadTypeList(int count, int offset, string condition)
        {
            listView6.Items.Clear();
            List<Type> items = Type.Where(condition, count, offset);
            foreach (Type t in items)
            {
                var cab = new Cabinet();
                int amount = cab.Count(String.Format("type_id = {0}", t.Id));
                ListViewItem lvi = new ListViewItem(t.Id.ToString());
                lvi.SubItems.Add(t.Name);
                lvi.SubItems.Add(t.Code);
                lvi.SubItems.Add(t.Rate.ToString("#0.00"));
                lvi.SubItems.Add(amount.ToString());

                listView6.Items.Add(lvi);
            }
        }

        private void TypePage()
        {
            string condition = "status <> 'Disabled'";
            var type = new Type();
            lockerTypePage.FinalIndex = Convert.ToDouble(type.Count(condition));
            lockerTypePage.LastPage = Convert.ToInt32(Math.Ceiling(lockerTypePage.FinalIndex / lockerTypePage.MaxItems));
            lockerTypePage.PageSetting();
            if (lockerTypePage.FinalIndex == 0)
            {
                lockerTypePage.PageReset();
            }
            if (lockerTypePage.PageNumber == lockerTypePage.LastPage)
            { lockerTypePage.LastIndex = (int)lockerTypePage.FinalIndex; }
            toolStripLabel18.Text = String.Format("Page {0} / {1}", lockerTypePage.PageNumber, lockerTypePage.LastPage);
            ReloadTypeList(lockerTypePage.IndexLimit, lockerTypePage.MaxItems, condition);
        }

        private void Button30_Click(object sender, EventArgs e) //Add Locker Type Button
        { 
            TypeForm AddRateForm = new TypeForm();
            AddRateForm.ShowDialog();
            if (!AddRateForm.InsertComplete)
                return;
            var type = new Type();
            lockerTypePage.FinalIndex = Convert.ToDouble(type.Count("status <> 'Disabled'"));
            lockerTypePage.LastPage = Convert.ToInt32(Math.Ceiling(lockerTypePage.FinalIndex / lockerTypePage.MaxItems));
            lockerTypePage.PageNumber = lockerTypePage.LastPage;
            TypePage();

            //Clear combo box 1 & combo box items to avoid error
            _comboBoxItems.Clear();

            //Load Locker Type Name into Combo Box 1
            _typeList = Type.Where("status <> 'Disabled'", 0, 100);
            _comboBoxItems.Add(0, "All");
            foreach (Type t in _typeList)
            { _comboBoxItems.Add(t.Id, t.Name); }
            comboBox1.DataSource = new BindingSource(_comboBoxItems, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";
        }

        private void Button31_Click(object sender, EventArgs e) //Edit Locker Type button
        {
            if (listView6.SelectedItems.Count <= 0)
                return;
            ListViewItem lvi = listView6.SelectedItems[0];
            int id = Convert.ToInt32(lvi.Text);
            TypeForm EditRateForm = new TypeForm(id);
            EditRateForm.ShowDialog();
            TypePage();

            //Clear combo box 1 & combo box items to avoid error
            _comboBoxItems.Clear();

            //Load Locker Type Name into Combo Box 1
            _typeList = Type.Where("status <> 'Disabled'", 0, 100);
            _comboBoxItems.Add(0, "All");
            foreach (Type t in _typeList)
            { _comboBoxItems.Add(t.Id, t.Name); }
            comboBox1.DataSource = new BindingSource(_comboBoxItems, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

        }

        private void Button32_Click(object sender, EventArgs e) //Delete Locker Type Button
        {
            if (listView6.SelectedItems.Count <= 0)
                return;

            var result = MessageBox.Show("Do you want to delete this locker type?", "Delete Locker Type",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView6.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);

                //Check if any cabinet exists for this locker type. If yes, show error.
                var cab = new Cabinet();
                if (cab.Count(String.Format("type_id = {0} AND status <> 'Disabled'", id)) > 0)
                {
                    MessageBox.Show("Error: Cabinets detected." + Environment.NewLine +
                        "To delete this locker type, please delete all cabinet(s) assoicated to it.",
                        "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Delete the locker type
                var type = new Type();
                type = type.Get(id);
                type.TempDelete();

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Delete",
                    Item = "Locker Type",
                    ItemId = Convert.ToString(id),
                    Description = "Code: " + type.Code + "; Status: Active to Disabled"
                };
                log.Insert();

                TypePage();

                //Clear combo box 1 & combo box items to avoid error
                _comboBoxItems.Clear();

                //Load Locker Type Name into Combo Box 1
                _typeList = Type.Where("status <> 'Disabled'", 0, 100);
                _comboBoxItems.Add(0, "All");
                foreach (Type t in _typeList)
                { _comboBoxItems.Add(t.Id, t.Name); }
                comboBox1.DataSource = new BindingSource(_comboBoxItems, null);
                comboBox1.DisplayMember = "Value";
                comboBox1.ValueMember = "Key";

            }
        }

        private void ToolStripButton34_Click(object sender, EventArgs e) //First Page in Locker Type
        {
            if (lockerTypePage.PageNumber == 1)
            { return; }
            lockerTypePage.PageNumber = 1;
            TypePage();
        }

        private void ToolStripButton35_Click(object sender, EventArgs e) //Previous Page in Locker Type
        {
            if (lockerTypePage.PageNumber == 1)
            { return; }
            else
            {
                lockerTypePage.PageNumber -= 1;
                TypePage();
            }
        }

        private void ToolStripButton36_Click(object sender, EventArgs e) //Next Page in Locker Type
        {
            if (lockerTypePage.PageNumber == lockerTypePage.LastPage)
            { return; }
            else
            {
                lockerTypePage.PageNumber += 1;
                TypePage();
            }
        }

        private void ToolStripButton37_Click(object sender, EventArgs e) //Last Page in Locker Type
        {
            if (lockerTypePage.PageNumber == lockerTypePage.LastPage)
            { return; }
            lockerTypePage.PageNumber = lockerTypePage.LastPage;
            TypePage();
        }

        private void ListView6_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Locker Type list
        {
            // Determine whether the column is the same as the last column clicked.
            if (e.Column != _sortColumn)
            {
                // Set the sort column to the new column.
                _sortColumn = e.Column;
                // Set the sort order to ascending by default.
                listView6.Sorting = SortOrder.Ascending;
            }
            else
            {
                // Determine what the last sort order was and change it.
                if (listView6.Sorting == SortOrder.Ascending)
                    listView6.Sorting = SortOrder.Descending;
                else
                    listView6.Sorting = SortOrder.Ascending;
            }

            // Call the sort method to manually sort.
            listView6.Sort();
            // Set the ListViewItemSorter property to a new ListViewItemComparer object.
            this.listView6.ListViewItemSorter = new ListViewItemComparer(e.Column, listView6.Sorting);
        }

        private void ReloadCabinetList(int count, int offset, string condition)
        {
            List<Type> _typeList = Type.Where("status <> 'Disabled'", 0, 100);

            listView7.Items.Clear();
            List<Cabinet> items = Cabinet.Where(condition, count, offset);
            foreach (Cabinet cab in items)
            {
                ListViewItem lvi = new ListViewItem(cab.Id.ToString());
                lvi.SubItems.Add(cab.Code);

                var cabinet_size = from selected in _typeList
                                   where selected.Id.Equals(cab.TypeID)
                                   select selected.Code;
                string size_code = cabinet_size.First();

                lvi.SubItems.Add(size_code);
                lvi.SubItems.Add(cab.Row.ToString());
                lvi.SubItems.Add(cab.Column.ToString());
                lvi.SubItems.Add(cab.Status);

                listView7.Items.Add(lvi);
            }
        }

        private void CabinetPage()
        {
            string condition = "status <> 'Disabled'";
            var cab = new Cabinet();
            cabinetPage.FinalIndex = Convert.ToDouble(cab.Count(condition));
            cabinetPage.LastPage = Convert.ToInt32(Math.Ceiling(cabinetPage.FinalIndex / cabinetPage.MaxItems));
            cabinetPage.PageSetting();
            if (cabinetPage.FinalIndex == 0)
            {
                cabinetPage.PageReset();
            }
            if (cabinetPage.PageNumber == cabinetPage.LastPage)
            { cabinetPage.LastIndex = (int)cabinetPage.FinalIndex; }
            toolStripLabel16.Text = String.Format("Page {0} / {1}", cabinetPage.PageNumber, cabinetPage.LastPage);
            toolStripLabel17.Text = String.Format("Showing result {0}~{1}", cabinetPage.FirstIndex, cabinetPage.LastIndex);
            ReloadCabinetList(cabinetPage.IndexLimit, cabinetPage.MaxItems, condition);
        }

        private void CabinetPage(string condition)
        {
            var cab = new Cabinet();
            cabinetPage.FinalIndex = Convert.ToDouble(cab.Count(condition));
            cabinetPage.LastPage = Convert.ToInt32(Math.Ceiling(cabinetPage.FinalIndex / cabinetPage.MaxItems));
            cabinetPage.PageSetting();
            if (cabinetPage.FinalIndex == 0)
            {
                cabinetPage.PageReset();
            }
            if (cabinetPage.PageNumber == cabinetPage.LastPage)
            { cabinetPage.LastIndex = (int)cabinetPage.FinalIndex; }
            toolStripLabel16.Text = String.Format("Page {0} / {1}", cabinetPage.PageNumber, cabinetPage.LastPage);
            toolStripLabel17.Text = String.Format("Showing result {0}~{1}", cabinetPage.FirstIndex, cabinetPage.LastIndex);
            ReloadCabinetList(cabinetPage.IndexLimit, cabinetPage.MaxItems, condition);
        }

        private void Button33_Click(object sender, EventArgs e) //Add Cabinet Button
        {
            CabinetForm AddCabinetForm = new CabinetForm();
            AddCabinetForm.ShowDialog();

            if (!AddCabinetForm.InsertComplete)
                return;

            TypePage();
            var cab = new Cabinet();
            if (!search)
            {
                cabinetPage.FinalIndex = Convert.ToDouble(cab.Count("status <> 'Disabled'"));
                cabinetPage.LastPage = Convert.ToInt32(Math.Ceiling(cabinetPage.FinalIndex / cabinetPage.MaxItems));
                cabinetPage.PageNumber = cabinetPage.LastPage;
                CabinetPage();
            }
            else
            {
                cabinetPage.FinalIndex = Convert.ToDouble(cab.Count(_searchCondition));
                cabinetPage.LastPage = Convert.ToInt32(Math.Ceiling(cabinetPage.FinalIndex / cabinetPage.MaxItems));
                cabinetPage.PageNumber = cabinetPage.LastPage;
                CabinetPage(_searchCondition);
            }
            _pageFlip = true;
            SmallCabinetPage();
        }

        private void Button28_Click(object sender, EventArgs e) //Filter Cabinet button
        {
            CabinetForm FilterCabinetForm = new CabinetForm(false);
            FilterCabinetForm.ShowDialog();
            if (String.IsNullOrWhiteSpace(FilterCabinetForm.Condition))
            { return; }
            else
            {
                _searchCondition = FilterCabinetForm.Condition;
                search = true;
                cabinetPage.PageNumber = 1;
                CabinetPage(_searchCondition);
            }
        }

        private void Button35_Click(object sender, EventArgs e) //Delete Cabinet Button
        {
            if (listView7.SelectedItems.Count <= 0)
                return;          

            var result = MessageBox.Show("Do you want to delete this cabinet?", "Delete Cabinet",
               MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView7.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);

                //Check any occupied locker for this cabinet. If yes, show error.
                var locker = new Locker();
                if (locker.Count(String.Format("cabinet_id = {0} AND status = 'Occupied'", id)) > 0)
                {
                    MessageBox.Show("Error: Occupied Lockers detected." + Environment.NewLine +
                        "To delete this cabinet, please ensure that all lockers assoicated to this cabinet were not occupied.",
                        "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Disable the cabinet
                var cab = new Cabinet();
                var deleteCabinet = cab.Get(id);
                deleteCabinet.TempDelete();

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Delete",
                    Item = "Cabinet",
                    ItemId = id.ToString(),
                    Description = "Code: " + cab.Code + "; Status: Active to Disabled"
                };
                log.Insert();
                
                //Calculate how many lockers in the cabinet
                int noOfLockers = deleteCabinet.Row * deleteCabinet.Column;

                //Disable all lockers associated to this cabinet
                List<Locker> lockerList = Locker.Where(String.Format("cabinet_id = {0}", id), 0, noOfLockers);
                for (int i = 0; i < noOfLockers; i++)
                {
                    var delLocker = new Locker();
                    delLocker.Id = lockerList[i].Id;
                    delLocker.TempDelete();

                    log.User = "system";
                    log.Action = "Delete";
                    log.Item = "Locker";
                    log.ItemId = lockerList[i].Id.ToString();
                    log.Description = "Code: " + lockerList[i].Code + ", Status: Available to Disabled";
                    log.Insert();
                }

                TypePage();
                if (!search)
                    CabinetPage();
                else
                    CabinetPage(_searchCondition);
                _pageFlip = true;
                SmallCabinetPage();
            }
        }

        private void ToolStripButton30_Click(object sender, EventArgs e) //First Page in Cabinet
        {
            if (cabinetPage.PageNumber == 1)
            { return; }
            cabinetPage.PageNumber = 1;
            if (!search)
                CabinetPage();
            else
                CabinetPage(_searchCondition);
        }

        private void ToolStripButton31_Click(object sender, EventArgs e) //Previous Page in Cabinet
        {
            if (cabinetPage.PageNumber == 1)
            { return; }
            else
            {
                cabinetPage.PageNumber -= 1;
                if (!search)
                    CabinetPage();
                else
                    CabinetPage(_searchCondition);
            }
        }

        private void ToolStripButton32_Click(object sender, EventArgs e) //Next Page in Cabinet
        {
            if (cabinetPage.PageNumber == cabinetPage.LastPage)
            { return; }
            else
            {
                cabinetPage.PageNumber += 1;
                if (!search)
                    CabinetPage();
                else
                    CabinetPage(_searchCondition);
            }
        }

        private void ToolStripButton33_Click(object sender, EventArgs e) //Last Page in Cabinet
        {
            if (cabinetPage.PageNumber == cabinetPage.LastPage)
            { return; }
            cabinetPage.PageNumber = cabinetPage.LastPage;
            if (!search)
                CabinetPage();
            else
                CabinetPage(_searchCondition);
        }

        private void ListView7_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Cabinet List
        {
            // Determine whether the column is the same as the last column clicked.
            if (e.Column != _sortColumn)
            {
                // Set the sort column to the new column.
                _sortColumn = e.Column;
                // Set the sort order to ascending by default.
                listView7.Sorting = SortOrder.Ascending;
            }
            else
            {
                // Determine what the last sort order was and change it.
                if (listView7.Sorting == SortOrder.Ascending)
                    listView7.Sorting = SortOrder.Descending;
                else
                    listView7.Sorting = SortOrder.Ascending;
            }

            // Call the sort method to manually sort.
            listView7.Sort();
            // Set the ListViewItemSorter property to a new ListViewItemComparer object.
            this.listView7.ListViewItemSorter = new ListViewItemComparer(e.Column, listView7.Sorting);
        }


        //Locker Module
        private void Button6_Click(object sender, EventArgs e) //Locker button
        {
            panel10.Controls.Remove(panel6_RentalBasePanel);       //Rental Panel
            panel10.Controls.Remove(panel8_CustomerBasePanel);    //Customer Panel
            panel10.Controls.Add(panel9_LockerBasePanel);    //Locker Panel
            search = false;

            //Clear combo box 1 & combo box items to avoid error
            _comboBoxItems.Clear();

            //Load Locker Type Name into Combo Box 1
            _typeList = Type.Where("status <> 'Disabled'", 0, 100);
            _comboBoxItems.Add(0, "All");
            foreach (Type t in _typeList)
            { _comboBoxItems.Add(t.Id, t.Name); }
            comboBox1.DataSource = new BindingSource(_comboBoxItems, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            //Default select all cabinets (ignore cabinet condition)
            radioButton1.Checked = true;
            //Default select the first cabinet to load
            List<Cabinet> items = Cabinet.Where("status <> 'Disabled'", 0, 1);
            if (!items.Any()) //If no cabinet in list, return
                return;
            
            _cabinetId = items[0].Id;
            textBox1.Text = items[0].Code;
            var locker = new Locker();
            textBox2.Text = locker.Count(String.Format("cabinet_id = {0} AND status = 'Available'", _cabinetId)).ToString();
            lockerPage.PageNumber = 1;
            LockerPage(_cabinetId);
        }

        private void ReloadSmallCabinetList(int count, int offset, string condition)
        {
            listView12.Items.Clear();

            List<Cabinet> items = Cabinet.Where(condition, count, offset);
            foreach (Cabinet cab in items)
            {
                ListViewItem lvi = new ListViewItem(cab.Id.ToString());
                lvi.SubItems.Add(cab.Code);
                lvi.SubItems.Add(cab.Status);

                listView12.Items.Add(lvi);
            }
        }

        private void SmallCabinetPage()
        {
            if (!_pageFlip)
            {
                //Buffer to filter and reduce refresh rounds for the small cabinet list.
                //This is due to event raidoButton_CheckedChanged will call this function 2 times for each change 
                //(1 time before change and 1 time after change).
                //For comboBox1_SelectedIndexChanged, if cabinet_status is empty, exit this function to prevent errors.
                
                //Return if cabinet_status OR cabient_size is empty
                if (String.IsNullOrWhiteSpace(_cabinetStatus) || String.IsNullOrWhiteSpace(_cabinetSize))
                    return;
                //Return if new cabinet status == old cabinet status AND new cabinet size == old cabinet size
                if (_cabinetStatus == _oldCabinetStatus && _cabinetSize == _oldCabinetSize)
                    return;

                _oldCabinetStatus = _cabinetStatus;
                _oldCabinetSize = _cabinetSize;
            }

            string condition = "status {0} AND type_id {1}";
            condition = String.Format(condition, _cabinetStatus, _cabinetSize);

            var cab = new Cabinet();
            smallCabinetPage.FinalIndex = cab.Count(condition);
            smallCabinetPage.LastPage = Convert.ToInt32(Math.Ceiling(smallCabinetPage.FinalIndex / smallCabinetPage.MaxItems));
            smallCabinetPage.PageSetting();
            if (smallCabinetPage.FinalIndex == 0)
            {
                smallCabinetPage.PageReset();
            }
            
            if (smallCabinetPage.PageNumber == smallCabinetPage.LastPage)
            { smallCabinetPage.LastIndex = (int)smallCabinetPage.FinalIndex; }
            toolStripLabel25.Text = String.Format("Page {0} / {1}", smallCabinetPage.PageNumber, smallCabinetPage.LastPage);
            ReloadSmallCabinetList(smallCabinetPage.IndexLimit, smallCabinetPage.MaxItems, condition);
            _pageFlip = false;
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
            SmallCabinetPage();
        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e) //Radio button "All"
        {
            _cabinetStatus = "<> 'Disabled'";
            smallCabinetPage.PageNumber = 1;
            SmallCabinetPage();
        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e) //Radio button "Available"
        {
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

        private void ToolStripButton52_Click(object sender, EventArgs e) //First Page for Small Cabinet List
        {
            if (smallCabinetPage.PageNumber == 1)
                return;
            _pageFlip = true;
            smallCabinetPage.PageNumber = 1;
            SmallCabinetPage();
        }

        private void ToolStripButton53_Click(object sender, EventArgs e) //Previous Page for Small Cabinet List
        {
            if (smallCabinetPage.PageNumber == 1)
            { return; }
            else
            {
                _pageFlip = true;
                smallCabinetPage.PageNumber -= 1;
                SmallCabinetPage();
            }
        }

        private void ToolStripButton54_Click(object sender, EventArgs e) //Next Page for Small Cabinet List
        {
            if (smallCabinetPage.PageNumber == smallCabinetPage.LastPage)
                return; 
            else
            {
                _pageFlip = true;
                smallCabinetPage.PageNumber += 1;
                SmallCabinetPage();
            }
        }

        private void ToolStripButton55_Click(object sender, EventArgs e) //Last Page for Small Cabinet List
        {
            if (smallCabinetPage.PageNumber == smallCabinetPage.LastPage)
                return; 
            _pageFlip = true;
            smallCabinetPage.PageNumber = smallCabinetPage.LastPage;
            SmallCabinetPage();
        }

        //Sorting for small cabinet list

        private void Button10_Click(object sender, EventArgs e) //Select Cabinet Button
        {
            if (listView12.SelectedItems.Count <= 0)
                return;
            ListViewItem lvi = listView12.SelectedItems[0];
            _cabinetId = Convert.ToInt32(lvi.Text);
            textBox1.Text = lvi.SubItems[1].Text;
            var locker = new Locker();
            textBox2.Text = locker.Count(String.Format("cabinet_id = {0} AND status = 'Available'", _cabinetId)).ToString();
            cabinetPage.PageNumber = 1;
            LockerPage(_cabinetId);
        }

        private void ReloadLockerList(int count, int offset, string condition)
        {
            listView11.Items.Clear();
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
                
                listView11.Items.Add(lvi);
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
                lockerPage.PageReset();
            }
            if (lockerPage.PageNumber == lockerPage.LastPage)
            { lockerPage.LastIndex = (int)lockerPage.FinalIndex; }
            toolStripLabel26.Text = String.Format("Page {0} / {1}", lockerPage.PageNumber, lockerPage.LastPage);
            toolStripLabel27.Text = String.Format("Showing result {0}~{1}", lockerPage.FirstIndex, lockerPage.LastIndex);
            ReloadLockerList(lockerPage.IndexLimit, lockerPage.MaxItems, condition);
        }

        private void ToolStripButton56_Click(object sender, EventArgs e) //First Page for Locker
        {
            if (lockerPage.PageNumber == 1)
                return;
            lockerPage.PageNumber = 1;
            LockerPage(_cabinetId);
        }

        private void ToolStripButton57_Click(object sender, EventArgs e) //Previous Page for Locker 
        {
            if (lockerPage.PageNumber == 1)
                return;
            else
            {
                lockerPage.PageNumber -= 1;
                LockerPage(_cabinetId);
            }
        }

        private void ToolStripButton58_Click(object sender, EventArgs e) //Next Page for Locker
        {
            if (lockerPage.PageNumber == lockerPage.LastPage)
                return;
            else
            {
                lockerPage.PageNumber += 1;
                LockerPage(_cabinetId);
            }
        }

        private void ToolStripButton59_Click(object sender, EventArgs e) //Last Page for Locker
        {
            if (lockerPage.PageNumber == lockerPage.LastPage)
                return;
            lockerPage.PageNumber = lockerPage.LastPage;
            LockerPage(_cabinetId);
        }

        private void Button42_Click(object sender, EventArgs e) //Reset Locker button
        {
            if (listView11.SelectedItems.Count <= 0)
                return;
            List<Employee> item = Employee.Where(string.Format("username = '{0}'", Login.Username), 0, 1);
            if (!item[0].IsAdmin())
            {
                MessageBox.Show("Error: Acccess Denied. " + Environment.NewLine +
                    "Only administrators can reset locker status.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var result = MessageBox.Show("Do you want to reset this locker?", "Reset Locker", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    string lockerStatus = "";
                    ListViewItem lvi = listView11.SelectedItems[0];
                    string lockerCode = String.Format("code = '{0}'", lvi.Text);
                    List<Locker> lockerList = Locker.Where(lockerCode, 0, 1);
                    var locker = new Locker();
                    var selectedLocker = locker.Get(lockerList[0].Id);

                    //Check if locker available, if yes, ignore.
                    if (selectedLocker.IsAvailable())
                        return;
                    //Check if locker occupied, if yes, show error message.
                    if (selectedLocker.IsOccupied())
                    {
                        MessageBox.Show("Error: Locker is occupied" + Environment.NewLine +
                            "You cannot reset an occupied locker.", "Occupied Locker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //If locker was overdue less than days, show error message.
                    if (selectedLocker.IsOverdued())
                    {
                        List<Rental> rentalItems = Rental.Where(String.Format("locker_id = {0}", selectedLocker.Id), 0, 1);
                        DateTime endDate = rentalItems[0].StartDate.Date.AddDays(rentalItems[0].Duration);
                        TimeSpan overdue = DateTime.Now.Date - endDate;
                        int overdueDays = Convert.ToInt32(overdue.Days);

                        var maxDueSettings = RentalSettings.Where("id = 1", 0, 1);
                        int maxOverdue = Convert.ToInt32(maxDueSettings[0].SettingValue);

                        if (overdueDays <= maxOverdue)
                        {
                            MessageBox.Show("Error: Locker overdue not more than " + maxOverdue + " days." + Environment.NewLine +
                                "You can only reset this locker when it's overdue is more than " + maxOverdue + " days.",
                                "Overdue Locker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        lockerStatus += "Overdue to Available";
                    }
                    else
                    {
                        lockerStatus += "Not Available to Available";
                    }

                    selectedLocker.Reset();

                    var log = new AccessLog
                    {
                        User = Login.Username,
                        Action = "Reset",
                        Item = "Locker",
                        ItemId = lockerList[0].Id.ToString(),
                        Description = "Code: " + lockerList[0].Code + "; Status: " + lockerStatus
                    };
                    log.Insert();

                    //Check was the cabinet full. If yes, set to available and insert log.
                    var cab = new Cabinet();
                    var selectedCab = cab.Get(selectedLocker.CabinetID);
                    if (selectedCab.IsFull())
                    {
                        selectedCab.Restore();
                        log.User = "system";
                        log.Action = "Update";
                        log.Item = "Cabinet";
                        log.ItemId = selectedLocker.CabinetID.ToString();
                        log.Description = "Code: " + selectedCab.Code + "; Status: Full to Available";
                        log.Insert();
                    }
                    _pageFlip = true;
                    SmallCabinetPage();
                    LockerPage(_cabinetId);

                    //Delete the rental for the locker (if it's a overdued locker)
                    if (selectedLocker.IsOverdued())
                    {
                        var rental = Rental.Where(String.Format("locker_id = {0}", selectedLocker.Id), 0, 1);
                        var transaction = Transaction.Where(String.Format("rental_id = {0}", rental[0].Id), 0, 1);
                        log = new AccessLog()
                        {
                            User = "system",
                            Action = "End",
                            Item = "Rental",
                            ItemId = rental[0].Id.ToString(),
                            Description = "Reset Overdue Locker " + selectedLocker.Code
                        };
                        log.Insert();
                        //rental[0].Delete();

                        //Set the transaction for the rental overdue, key lost
                        transaction[0].Fine = 0;
                        TimeSpan dayDiff = DateTime.Now.Date - transaction[0].StartDate;
                        transaction[0].OverdueTime = Convert.ToInt32(dayDiff.Days);
                        transaction[0].ReturnDate = DateTime.Now.Date;
                        transaction[0].Save();

                        log = new AccessLog()
                        {
                            User = "system",
                            Action = "Update",
                            Item = "Transaction",
                            ItemId = transaction[0].Id.ToString(),
                            Description = "Return Date: " + transaction[0].ReturnDate.ToString("dd-MM-yyyy") + "; Overdue Time: " + transaction[0].OverdueTime + 
                            " day; Fine: " + transaction[0].Fine
                        };
                        log.Insert();

                        var returnStatus = new RentalStatus();
                        returnStatus.TransactionId = transaction[0].Id;

                        //Set status key lost
                        returnStatus.StatusId = 3;
                        returnStatus.Insert();
                        log = new AccessLog()
                        {
                            User = "system",
                            Action = "Add",
                            Item = "Rental Status",
                            ItemId = returnStatus.TransactionId + ", " + returnStatus.StatusId,
                            Description = "Return Status: Key Lost"
                        };
                        log.Insert();

                        //Set status overdue
                        returnStatus.StatusId = 2;
                        returnStatus.Insert();
                        log = new AccessLog()
                        {
                            User = "system",
                            Action = "Add",
                            Item = "Rental Status",
                            ItemId = returnStatus.TransactionId + ", " + returnStatus.StatusId,
                            Description = "Return Status: Overdue"
                        };
                        log.Insert();
                    }
                }
            }
        }

        private void Button43_Click(object sender, EventArgs e) //Disable Locker button
        {
            if (listView11.SelectedItems.Count <= 0)
                return;
            var result = MessageBox.Show("Do you want to disable this locker?", "Disable Locker",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {

                ListViewItem lvi = listView11.SelectedItems[0];
                string lockerCode = String.Format("code = '{0}'", lvi.Text);
                var locker = new Locker();
                List<Locker> lockerList = Locker.Where(lockerCode, 0, 1);
                var selectedLocker = lockerList[0];
                if (selectedLocker.IsNotAvailable())
                {
                    MessageBox.Show("Error: Locker is disabled" + Environment.NewLine +
                        "This locker was already disabled.", "Disabled Locker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (!selectedLocker.IsAvailable())
                {
                    MessageBox.Show("Error: Locker is occupied or overdued." + Environment.NewLine +
                        "You cannot disable an occupied or overdued locker. To disable this locker, please ensure it was not occupied or overdued.",
                        "Occupied Locker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    selectedLocker.NotAvailable();

                    var log = new AccessLog
                    {
                        User = Login.Username,
                        Action = "Disable",
                        Item = "Locker",
                        ItemId = lockerList[0].Id.ToString(),
                        Description = "Code: " + lockerList[0].Code + "; Status: Available to Not Available"
                    };
                    log.Insert();

                    //Check is cabinet full. If yes, update and insert log.
                    int noOfEmptyLocker = locker.Count(String.Format("cabinet_id = {0} AND status = 'Available'",
                        lockerList[0].CabinetID));
                    if (noOfEmptyLocker <= 0)
                    {
                        var cab = new Cabinet();
                        cab = cab.Get(lockerList[0].CabinetID);
                        cab.Full();

                        log.User = "system";
                        log.Action = "Update";
                        log.Item = "Cabinet";
                        log.ItemId = cab.Id.ToString();
                        log.Description = "Code: " + cab.Code + "; Status: Available to Full";
                        log.Insert();
                    }
                    _pageFlip = true;
                    SmallCabinetPage();
                    LockerPage(_cabinetId);
                }
            }
        }

        //Rental Module
        private void Button4_Click(object sender, EventArgs e) //Rental button
        {
            panel10.Controls.Add(panel6_RentalBasePanel);       //Rental Panel
            panel10.Controls.Remove(panel8_CustomerBasePanel);    //Customer Panel
            panel10.Controls.Remove(panel9_LockerBasePanel);    //Locker Panel
            search = false;
            rentalPage.PageReset();
            RentalPage();
        }

        private void ReloadRentalList(int count, int offset, string condition)
        {
            listView10.Items.Clear();

            List<Rental> items = Rental.Where(condition, count, offset);
            foreach (Rental r in items)
            {
                ListViewItem lvi = new ListViewItem(r.Id.ToString());
                lvi.SubItems.Add(r.StartDate.ToString("dd-MM-yyyy"));

                DateTime endDate = r.StartDate.Date.AddDays(r.Duration);
                lvi.SubItems.Add(endDate.ToString("dd-MM-yyyy"));

                TimeSpan timeSpan = endDate.Date.Subtract(DateTime.Now.Date);
                int timeLeft = Convert.ToInt32(timeSpan.Days);
                lvi.SubItems.Add(timeLeft.ToString());

                var customer = new Customer();
                customer = customer.Get(r.CustomerID);
                lvi.SubItems.Add(customer.Ic);

                var locker = new Locker();
                locker = locker.Get(r.LockerID);
                lvi.SubItems.Add(locker.Code);

                if (timeLeft < 0)
                {
                    lvi.ImageIndex = 1;
                    if (!r.IsOverdue())
                    {
                        r.Status = "Overdue";
                        r.Save();
                        locker.Overdued();
                        var log = new AccessLog()
                        {
                            User = "system",
                            Action = "Update",
                            Item = "Rental",
                            ItemId = r.Id.ToString(),
                            Description = "Status: Normal to Overdue"
                        };
                        log.Insert();

                        log = new AccessLog()
                        {
                            User = "system",
                            Action = "Update",
                            Item = "Locker",
                            ItemId = locker.Id.ToString(),
                            Description = "Code: " + locker.Code + "; Status: Occupied to Overdue"
                        };
                        log.Insert();
                    }
                }
                else
                {
                    lvi.ImageIndex = 0;
                }
                lvi.SubItems.Add(r.Status);

                listView10.Items.Add(lvi);
            }
        }

        private void RentalPage()
        {
            string condition = "id IS NOT NULL";
            var rental = new Rental();
            
            rentalPage.FinalIndex = Convert.ToDouble(rental.Count(condition));
            rentalPage.LastPage = Convert.ToInt32(Math.Ceiling(rentalPage.FinalIndex / rentalPage.MaxItems));
            rentalPage.PageSetting();
            if (rentalPage.FinalIndex == 0)
            {
                rentalPage.PageReset();
            }
            if (rentalPage.PageNumber == rentalPage.LastPage)
            { rentalPage.LastIndex = (int)rentalPage.FinalIndex; }

            toolStripLabel22.Text = String.Format("Page {0} / {1}", rentalPage.PageNumber, rentalPage.LastPage);
            toolStripLabel23.Text = String.Format("Showing result {0}~{1}", rentalPage.FirstIndex, rentalPage.LastIndex);
            ReloadRentalList(rentalPage.IndexLimit, rentalPage.MaxItems, condition);
        }

        private void RentalPage(string condition)
        {
            var rental = new Rental();
            rentalPage.FinalIndex = Convert.ToDouble(rental.Count(condition));
            rentalPage.LastPage = Convert.ToInt32(Math.Ceiling(rentalPage.FinalIndex / rentalPage.MaxItems));
            rentalPage.PageSetting();
            if (rentalPage.FinalIndex == 0)
            {
                rentalPage.PageReset();
            }
            if (rentalPage.PageNumber == rentalPage.LastPage)
            { rentalPage.LastIndex = (int)rentalPage.FinalIndex; }

            toolStripLabel22.Text = String.Format("Page {0} / {1}", rentalPage.PageNumber, rentalPage.LastPage);
            toolStripLabel23.Text = String.Format("Showing result {0}~{1}", rentalPage.FirstIndex, rentalPage.LastIndex);
            ReloadRentalList(rentalPage.IndexLimit, rentalPage.MaxItems, condition);
        }

        private void Button9_Click(object sender, EventArgs e) //Add Rental button
        {
            var AddRentalForm = new RentalForm();
            AddRentalForm.ShowDialog();

            if (!AddRentalForm.InsertComplete)
                return;

            var rental = new Rental();
            if (!search)
            {
                rentalPage.FinalIndex = Convert.ToDouble(rental.Count("id IS NOT NULL"));
                rentalPage.LastPage = Convert.ToInt32(Math.Ceiling(rentalPage.FinalIndex / rentalPage.MaxItems));
                rentalPage.PageNumber = rentalPage.LastPage;
                RentalPage();
            }
            else
            {
                rentalPage.FinalIndex = Convert.ToDouble(rental.Count(_searchCondition));
                rentalPage.LastPage = Convert.ToInt32(Math.Ceiling(rentalPage.FinalIndex / rentalPage.MaxItems));
                rentalPage.PageNumber = rentalPage.LastPage;
                RentalPage(_searchCondition);
            }
        }

        private void Button44_Click(object sender, EventArgs e) //View Rental Detail button
        {
            if (listView10.SelectedItems.Count <= 0)
                return;

            ListViewItem lvi = listView10.SelectedItems[0];
            int id = Convert.ToInt32(lvi.Text);

            var ViewRentalDetailForm = new RentalForm(id);
            ViewRentalDetailForm.ShowDialog();

            if (!search)
                RentalPage();
            else
                RentalPage(_searchCondition);
        }

        private void Button11_Click(object sender, EventArgs e) //End Rental button
        {
            if (listView10.SelectedItems.Count <= 0)
                return;

            ListViewItem lvi = listView10.SelectedItems[0];
            int id = Convert.ToInt32(lvi.Text);

            var EndRentalForm = new RentalForm(id, true);
            EndRentalForm.ShowDialog();
            if (!search)
                RentalPage();
            else
                RentalPage(_searchCondition);
        }

        private void ToolStripButton46_Click(object sender, EventArgs e) //First Page for Rental
        {
            if (rentalPage.PageNumber == 1)
                return;
            rentalPage.PageNumber = 1;
            if (!search)
                RentalPage();
            else
                RentalPage(_searchCondition);
        }

        private void ToolStripButton47_Click(object sender, EventArgs e) //Previous Page for Rental
        {
            if (rentalPage.PageNumber == 1)
                return; 
            else
            {
                rentalPage.PageNumber -= 1;
                if (!search)
                { RentalPage(); }
                else
                { RentalPage(_searchCondition); }
            }
        }

        private void ToolStripButton48_Click(object sender, EventArgs e) //Next Page for Rental
        {
            if (rentalPage.PageNumber == rentalPage.LastPage)
                return; 
            else
            {
                rentalPage.PageNumber += 1;
                if (!search)
                    RentalPage(); 
                else
                    RentalPage(_searchCondition); 
            }
        }

        private void ToolStripButton49_Click(object sender, EventArgs e) //Last Page for Rental
        {
            if (rentalPage.PageNumber == rentalPage.LastPage)
                return;
            rentalPage.PageNumber = rentalPage.LastPage;
            if (!search)
                RentalPage();
            else
                RentalPage(_searchCondition);
        }

        private void ToolStripButton50_Click(object sender, EventArgs e) //Search button for Rental
        {
            string innerSqlItem;
            string outerSqlItem;
            string searchValue;
            string table;

            if (toolStripComboBox9.Text == "Customer IC")
            {
                innerSqlItem = "ic";
                outerSqlItem = "customer_id";
                table = "Customer";
            }
            else if (toolStripComboBox9.Text == "Locker Code")
            {
                innerSqlItem = "code";
                outerSqlItem = "locker_id";
                table = "Locker";
            }
            else
            { return; }

            if (toolStripComboBox10.Text == "Start with")
            { searchValue = "'{0}%'"; }
            else if (toolStripComboBox10.Text == "End with")
            { searchValue = "'%{0}'"; }
            else if (toolStripComboBox10.Text == "Contains")
            { searchValue = "'%{0}%'"; }
            else
            { return; }

            if (string.IsNullOrWhiteSpace(toolStripTextBox5.Text))
            { return; }

            searchValue = String.Format(searchValue, toolStripTextBox5.Text);

            string condition = "{0} IN (SELECT id FROM {1} WHERE {2} LIKE {3})";
            _searchCondition = String.Format(condition, outerSqlItem, table, innerSqlItem, searchValue);

            search = true;
            rentalPage.PageNumber = 1;
            RentalPage(_searchCondition);
        }

        private void ToolStripButton51_Click(object sender, EventArgs e) //Reset button for Rental
        {
            search = false;
            _searchCondition = "";
            toolStripComboBox9.SelectedIndex = -1;
            toolStripComboBox10.SelectedIndex = -1;
            toolStripTextBox5.Text = "";
            rentalPage.PageNumber = 1;
            RentalPage();
        }

        private void ListView10_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Rental
        {
            // Determine whether the column is the same as the last column clicked.
            if (e.Column != _sortColumn)
            {
                // Set the sort column to the new column.
                _sortColumn = e.Column;
                // Set the sort order to ascending by default.
                listView10.Sorting = SortOrder.Ascending;
            }
            else
            {
                // Determine what the last sort order was and change it.
                if (listView10.Sorting == SortOrder.Ascending)
                    listView10.Sorting = SortOrder.Descending;
                else
                    listView10.Sorting = SortOrder.Ascending;
            }

            // Call the sort method to manually sort.
            listView10.Sort();
            // Set the ListViewItemSorter property to a new ListViewItemComparer object.
            this.listView10.ListViewItemSorter = new ListViewItemComparer(e.Column, listView10.Sorting);
        }

        //Delete Records Module 
        private void Button15_Click(object sender, EventArgs e) //Deleted Records button
        {
            panel11.Controls.Remove(panel12_EmployeeBasePanel);
            panel11.Controls.Remove(panel13_CabinetBasePanel);
            panel11.Controls.Remove(panel15_TransactionBasePanel);
            panel11.Controls.Add(panel16_DeletedRecordsBasePanel);
            panel11.Controls.Remove(panel14_LogBasePanel);

            deletedCabinetPage.PageReset();
            deletedCustomerPage.PageReset();
            deletedEmployeePage.PageReset();
            deletedLockerTypePage.PageReset();

            search = false;
            DeletedCustomerPage();
            DeletedEmployeePage();
            DeletedTypePage();
            DeletedCabinetPage();
        }

        /// --Deleted Customer SubModule
        private void DeletedCustomerPage()
        {
            string condition = "status = 'Disabled'";
            var cust = new Customer();
            deletedCustomerPage.FinalIndex = cust.Count(condition);
            deletedCustomerPage.LastPage = Convert.ToInt32(Math.Ceiling(deletedCustomerPage.FinalIndex / deletedCustomerPage.MaxItems));
            deletedCustomerPage.PageSetting();
            if (deletedCustomerPage.FinalIndex == 0)
            {
                deletedCustomerPage.PageReset();
            }
            if (deletedCustomerPage.PageNumber == deletedCustomerPage.LastPage)
            { deletedCustomerPage.LastIndex = (int)deletedCustomerPage.FinalIndex; }
            toolStripLabel11.Text = String.Format("Page {0} / {1}", deletedCustomerPage.PageNumber, deletedCustomerPage.LastPage);
            toolStripLabel12.Text = String.Format("Showing result {0}~{1}", deletedCustomerPage.FirstIndex, deletedCustomerPage.LastIndex);
            ReloadDeletedCustomerList(deletedCustomerPage.IndexLimit, deletedCustomerPage.MaxItems, condition);
        }

        private void DeletedCustomerPage(string condition)
        {
            var cust = new Customer();
            deletedCustomerPage.FinalIndex = cust.Count(condition);
            deletedCustomerPage.LastPage = Convert.ToInt32(Math.Ceiling(deletedCustomerPage.FinalIndex / deletedCustomerPage.MaxItems));
            deletedCustomerPage.PageSetting();
            if (deletedCustomerPage.FinalIndex == 0)
            {
                deletedCustomerPage.PageReset();
            }
            if (deletedCustomerPage.PageNumber == deletedCustomerPage.LastPage)
            { deletedCustomerPage.LastIndex = (int)deletedCustomerPage.FinalIndex; }
            toolStripLabel11.Text = String.Format("Page {0} / {1}", deletedCustomerPage.PageNumber, deletedCustomerPage.LastPage);
            toolStripLabel12.Text = String.Format("Showing result {0}~{1}", deletedCustomerPage.FirstIndex, deletedCustomerPage.LastIndex);
            ReloadDeletedCustomerList(deletedCustomerPage.IndexLimit, deletedCustomerPage.MaxItems, condition);
        }

        private void ReloadDeletedCustomerList(int count, int offset, string condition)
        {
            listView4.Items.Clear();
            List<Customer> items = Customer.Where(condition, count, offset);
            foreach (Customer c in items)
            {
                ListViewItem lvi = new ListViewItem(c.Id.ToString());
                lvi.SubItems.Add(c.Name);
                lvi.SubItems.Add(c.Ic);

                listView4.Items.Add(lvi);
            }
        }

        private void ToolStripButton19_Click(object sender, EventArgs e) //Refresh button in Deleted Customer
        {
            search = false;
            _searchCondition = "";
            toolStripComboBox3.SelectedIndex = -1;
            toolStripComboBox8.SelectedIndex = -1;
            toolStripTextBox3.Text = "";
            deletedCustomerPage.PageNumber = 1;
            DeletedCustomerPage();
        }

        private void ToolStripButton20_Click(object sender, EventArgs e) //FirstPage button in Deleted Customer
        {
            if (deletedCustomerPage.PageNumber == 1)
            { return; }
            deletedCustomerPage.PageNumber = 1;
            if (!search)
                DeletedCustomerPage();
            else
                DeletedCustomerPage(_searchCondition);
        }

        private void ToolStripButton21_Click(object sender, EventArgs e) //PreviousPage button in Deleted Customer
        {
            if (!search)
            {
                if (deletedCustomerPage.PageNumber == 1)
                { return; }
                else
                {
                    deletedCustomerPage.PageNumber -= 1;
                    if (!search)
                        DeletedCustomerPage();
                    else
                        DeletedCustomerPage(_searchCondition);
                }
            }
        }

        private void ToolStripButton22_Click(object sender, EventArgs e) //NextPage button in Deleted Customer
        {
            if (deletedCustomerPage.PageNumber == deletedCustomerPage.LastPage)
            { return; }
            else
            {
                deletedCustomerPage.PageNumber += 1;
                if (!search)
                    DeletedCustomerPage();
                else
                    DeletedCustomerPage(_searchCondition);
            }
        }

        private void ToolStripButton23_Click(object sender, EventArgs e) //LastPage button in Deleted Customer
        {
            if (deletedCustomerPage.PageNumber == deletedCustomerPage.LastPage)
            { return; }
            deletedCustomerPage.PageNumber = deletedCustomerPage.LastPage;
            if (!search)
                DeletedCustomerPage();
            else
                DeletedCustomerPage(_searchCondition);
        }

        private void ToolStripButton18_Click(object sender, EventArgs e) //Search button in Deleted Customer
        {
            string item;
            string searchValue;
            if (toolStripComboBox3.Text == "IC Number")
            { item = "ic"; }
            else if (toolStripComboBox3.Text == "Name")
            { item = "name"; }
            else
            { return; }

            if (toolStripComboBox8.Text == "Start with")
            { searchValue = "'{0}%'"; }
            else if (toolStripComboBox8.Text == "End with")
            { searchValue = "'%{0}'"; }
            else if (toolStripComboBox8.Text == "Contains")
            { searchValue = "'%{0}%'"; }
            else
            { return; }

            if (string.IsNullOrWhiteSpace(toolStripTextBox3.Text))
            { return; }

            searchValue = String.Format(searchValue, toolStripTextBox3.Text);

            string condition = "{0} LIKE {1} AND status = 'Disabled'";
            _searchCondition = String.Format(condition, item, searchValue);

            search = true;
            deletedCustomerPage.PageNumber = 1;
            DeletedCustomerPage(_searchCondition);
        }

        private void ListView4_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Deleted Customer
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView4.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView4.Sorting == SortOrder.Ascending)
                    listView4.Sorting = SortOrder.Descending;
                else
                    listView4.Sorting = SortOrder.Ascending;
            }

            listView4.Sort();
            this.listView4.ListViewItemSorter = new ListViewItemComparer(e.Column, listView4.Sorting);
        }

        private void Button47_Click(object sender, EventArgs e) //Permanent Delete Customer button (One customer)
        {
            if (listView4.SelectedItems.Count <= 0)
                return;

            var result = MessageBox.Show("Do you want to export this customer from the database?\n" + Environment.NewLine +
                 "Note: Exported customer will be deleted from the database.",
                 "Export Deleted Customer", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView4.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);
                var deletedCus = Customer.Where(String.Format("id = {0}", id), 0, 1);
                string defaultFileName = String.Format("EXPORT_CUSTOMER_{0}_{1}", deletedCus[0].Id, DateTime.Now.ToString("ddMMyyyy_HHmmss"));

                var workbook = new XLWorkbook();
                var ws = workbook.AddWorksheet("DeletedCustomer");
                ws.Cell(1, 1).Value = "Customer";
                ws.Cell(2, 1).InsertTable(deletedCus);

                SaveFileDialog sf = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = "Excel Workbook (.xlsx) |*.xlsx",
                    Title = "Export Customer as",
                    FilterIndex = 1
                };

                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string savePath = Path.GetDirectoryName(sf.FileName);
                    string fileName = Path.GetFileName(sf.FileName);
                    string saveFile = Path.Combine(savePath, fileName);
                    try
                    {
                        workbook.SaveAs(saveFile); //Save the file

                        string escapedSaveFile = saveFile.Replace(@"\", @"\\");

                        var log = new AccessLog()
                        {
                            User = Login.Username,
                            Action = "Export",
                            Item = "Customer",
                            ItemId = id.ToString(),
                            Description = "IC: " + deletedCus[0].Ic + "; Path: " + escapedSaveFile
                        };
                        log.Insert();

                        log = new AccessLog()
                        {
                            User = "system",
                            Action = "Delete from database",
                            Item = "Customer",
                            ItemId = id.ToString(),
                            Description = "IC: " + deletedCus[0].Ic + "; exported in " + fileName
                        };
                        log.Insert();

                        deletedCus[0].Delete();

                        //Reload Deleted Customer List
                        if (!search)
                            DeletedCustomerPage();
                        else
                            DeletedCustomerPage(_searchCondition);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                }
            }
        }

        private void Button48_Click(object sender, EventArgs e) //Permanent Delete All Customer button 
        {
            var result = MessageBox.Show("Do you want to export all deleted customer from the database?\n" + Environment.NewLine +
               "Note: All exported customer will be deleted from the database.",
               "Export All Deleted Customer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                var delCusFirst = Customer.Where("id = (SELECT id FROM CUSTOMER WHERE status = 'Disabled' LIMIT 0,1)", 0, 1);
                var delCusLast = Customer.Where("id = (SELECT MAX(id) FROM CUSTOMER WHERE status = 'Disabled' LIMIT 0,1)", 0, 1);

                if (!delCusFirst.Any() || !delCusLast.Any())
                {
                    MessageBox.Show("Export Error: Empty Customer Records.\n" +
                        "There was no deleted customer to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string defaultFileName = String.Format("EXPORT_CUSTOMER_{0}~{1}_{2}", delCusFirst[0].Id, delCusLast[0].Id, DateTime.Now.ToString("ddMMyyyy_HHmmss"));

                var deletedCusList = Customer.Where("status = 'Disabled'", 0, 2147483647);

                var workbook = new XLWorkbook();
                var ws = workbook.AddWorksheet("DeletedCustomer");
                ws.Cell(1, 1).Value = "Customer";
                ws.Cell(2, 1).InsertTable(deletedCusList);

                SaveFileDialog sf = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = "Excel Workbook (.xlsx) |*.xlsx",
                    Title = "Export Customers as",
                    FilterIndex = 1
                };

                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string savePath = Path.GetDirectoryName(sf.FileName);
                    string fileName = Path.GetFileName(sf.FileName);
                    string saveFile = Path.Combine(savePath, fileName);
                    try
                    {
                        workbook.SaveAs(saveFile); //Save the file 
                        string escapedSaveFile = saveFile.Replace(@"\", @"\\");
                        var log = new AccessLog()
                        {
                            User = Login.Username,
                            Action = "Export",
                            Item = "Customer",
                            ItemId = delCusFirst[0].Id.ToString() + "~" + delCusLast[0].Id.ToString(),
                            Description = "Path: " + escapedSaveFile
                        };
                        log.Insert();

                        foreach (Customer item in deletedCusList)
                        {
                            log = new AccessLog()
                            {
                                User = "system",
                                Action = "Delete from database",
                                Item = "Customer",
                                ItemId = item.Id.ToString(),
                                Description = "IC: " + item.Ic + "; exported in " + fileName
                            };
                            log.Insert();

                            item.Delete();
                        }
                        //Reload Deleted Employee List
                        if (!search)
                            DeletedCustomerPage();
                        else
                            DeletedCustomerPage(_searchCondition);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                }
            }
        }

        private void Button23_Click(object sender, EventArgs e) //Restore Customer button
        {
            if (listView4.SelectedItems.Count <= 0)
                return;
            var result = MessageBox.Show("Do you want to restore this customer?", "Restore Customer", MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView4.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);
                Customer cust = new Customer();
                cust = cust.Get(id);
                cust.Restore();

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Restore",
                    Item = "Customer",
                    ItemId = Convert.ToString(id),
                    Description = "IC: " + cust.Ic + "; Status: Disabled to Active"
                };
                log.Insert();

                if (!search)
                    DeletedCustomerPage();
                else
                    DeletedCustomerPage(_searchCondition);
            }
        }

        /// --Deleted Employee SubModule
        private void DeletedEmployeePage()
        {
            string condition = "status = 'Disabled'";
            var emp = new Employee();
            deletedEmployeePage.FinalIndex = Convert.ToDouble(emp.Count(condition));
            deletedEmployeePage.LastPage = Convert.ToInt32(Math.Ceiling(deletedEmployeePage.FinalIndex / deletedEmployeePage.MaxItems));
            deletedEmployeePage.PageSetting();
            if (deletedEmployeePage.FinalIndex == 0)
            {
                deletedEmployeePage.PageReset();
            }
            if (deletedEmployeePage.PageNumber == deletedEmployeePage.LastPage)
            { deletedEmployeePage.LastIndex = (int)deletedEmployeePage.FinalIndex; }

            toolStripLabel14.Text = String.Format("Page {0} / {1}", deletedEmployeePage.PageNumber, deletedEmployeePage.LastPage);
            toolStripLabel15.Text = String.Format("Showing result {0}~{1}", deletedEmployeePage.FirstIndex, deletedEmployeePage.LastIndex);
            ReloadDeletedEmployeeList(deletedEmployeePage.IndexLimit, deletedEmployeePage.MaxItems, condition);
        }

        private void DeletedEmployeePage(string condition)
        {
            var emp = new Employee();
            deletedEmployeePage.FinalIndex = Convert.ToDouble(emp.Count(condition));
            deletedEmployeePage.LastPage = Convert.ToInt32(Math.Ceiling(deletedEmployeePage.FinalIndex / deletedEmployeePage.MaxItems));
            deletedEmployeePage.PageSetting();
            if (deletedEmployeePage.FinalIndex == 0)
            {
                deletedEmployeePage.PageReset();
            }
            if (deletedEmployeePage.PageNumber == deletedEmployeePage.LastPage)
            { deletedEmployeePage.LastIndex = (int)deletedEmployeePage.FinalIndex; }

            toolStripLabel14.Text = String.Format("Page {0} / {1}", deletedEmployeePage.PageNumber, deletedEmployeePage.LastPage);
            toolStripLabel15.Text = String.Format("Showing result {0}~{1}", deletedEmployeePage.FirstIndex, deletedEmployeePage.LastIndex);
            ReloadDeletedEmployeeList(deletedEmployeePage.IndexLimit, deletedEmployeePage.MaxItems, condition);
        }

        private void ReloadDeletedEmployeeList(int count, int offset, string condition)
        {
            listView5.Items.Clear();
            List<Employee> items = Employee.Where(condition, count, offset);
            foreach (Employee e in items)
            {
                ListViewItem lvi = new ListViewItem(e.Id.ToString());
                lvi.SubItems.Add(e.Name);
                lvi.SubItems.Add(e.Ic);
                lvi.SubItems.Add(e.Position);
                lvi.SubItems.Add(e.Username);
                lvi.SubItems.Add(e.Permission);

                listView5.Items.Add(lvi);
            }
        }

        private void ToolStripButton25_Click(object sender, EventArgs e) //Refresh button in Deleted Employee
        {
            search = false;
            deletedEmployeePage.PageNumber = 1;
            toolStripComboBox4.SelectedIndex = -1;
            toolStripComboBox6.SelectedIndex = -1;
            toolStripTextBox4.Text = "";
            DeletedEmployeePage();
        }

        private void ToolStripButton26_Click(object sender, EventArgs e) //FirstPage button in Deleted Employee
        {
            if (deletedEmployeePage.PageNumber == 1)
            { return; }
            deletedEmployeePage.PageNumber = 1;
            if (!search)
                DeletedEmployeePage();
            else
                DeletedEmployeePage(_searchCondition);
        }

        private void ToolStripButton27_Click(object sender, EventArgs e) //PreviousPage button in Deleted Employee
        {
            if (deletedEmployeePage.PageNumber == 1)
            { return; }
            else
            {
                deletedEmployeePage.PageNumber -= 1;
                if (!search)
                    DeletedEmployeePage();
                else
                    DeletedEmployeePage(_searchCondition);
            }
        }

        private void ToolStripButton28_Click(object sender, EventArgs e) //NextPage button in Deleted Employee
        {
            if (deletedEmployeePage.PageNumber == deletedEmployeePage.LastPage)
            { return; }
            else
            {
                deletedEmployeePage.PageNumber += 1;
                if (!search)
                    DeletedEmployeePage();
                else
                    DeletedEmployeePage(_searchCondition);
            }
        }

        private void ToolStripButton29_Click(object sender, EventArgs e) //LastPage button in Deleted Employee
        {
            if (deletedEmployeePage.PageNumber == deletedEmployeePage.LastPage)
            { return; }
            deletedEmployeePage.PageNumber = deletedEmployeePage.LastPage;
            if (!search)
                DeletedEmployeePage();
            else
                DeletedEmployeePage(_searchCondition);
        }

        private void ToolStripButton24_Click(object sender, EventArgs e) //Search button in Deleted Employee
        {
            string item;
            string searchValue;

            if (toolStripComboBox4.Text == "IC Number")
            { item = "ic"; }
            else if (toolStripComboBox4.Text == "Name")
            { item = "name"; }
            else if (toolStripComboBox4.Text == "Username")
            { item = "username"; }
            else
            { return; }

            if (toolStripComboBox6.Text == "Start with")
            { searchValue = "'{0}%'"; }
            else if (toolStripComboBox6.Text == "End with")
            { searchValue = "'%{0}'"; }
            else if (toolStripComboBox6.Text == "Contains")
            { searchValue = "'%{0}%'"; }
            else
            { return; }

            if(string.IsNullOrWhiteSpace(toolStripTextBox4.Text))
            { return; }

            searchValue = String.Format(searchValue, toolStripTextBox4.Text);

            string condition = "{0} LIKE {1} AND status = 'Disabled'";
            _searchCondition = String.Format(condition, item, searchValue);

            search = true;
            deletedEmployeePage.PageNumber = 1;
            DeletedEmployeePage(_searchCondition);
        }

        private void Button22_Click(object sender, EventArgs e) //Restore Employee button
        {
            if (listView5.SelectedItems.Count <= 0)
                return;
            var result = MessageBox.Show("Do you want to restore this employee?", "Restore Employee", MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView5.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);            
                
                Employee emp = new Employee();
                emp = emp.Get(id);
                emp.Restore();

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Restore",
                    Item = "Employee",
                    ItemId = Convert.ToString(id),
                    Description = "IC: " + emp.Ic + "; Status: Disabled to Active"
                };
                log.Insert();

                if (!search)
                    DeletedEmployeePage();
                else
                    DeletedEmployeePage(_searchCondition);
            }
        }

        private void Button26_Click(object sender, EventArgs e) //Permanent Delete Employee button (1 Employee)
        {
            if (listView5.SelectedItems.Count <= 0)
                return;

            var result = MessageBox.Show("Do you want to export this employee from the database?\n" + Environment.NewLine +
                 "Note: Exported employee will be deleted from the database.",
                 "Export Deleted Employee", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView5.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);
                var deletedEmp = Employee.Where(String.Format("id = {0}", id), 0, 1);
                string defaultFileName = String.Format("EXPORT_EMPLOYEE_{0}_{1}", deletedEmp[0].Id, DateTime.Now.ToString("ddMMyyyy_HHmmss"));

                var workbook = new XLWorkbook();
                var ws = workbook.AddWorksheet("DeletedEmployee");
                ws.Cell(1, 1).Value = "Employee";
                ws.Cell(2, 1).InsertTable(deletedEmp);

                SaveFileDialog sf = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = "Excel Workbook (.xlsx) |*.xlsx",
                    Title = "Export Employee as",
                    FilterIndex = 1
                };
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string savePath = Path.GetDirectoryName(sf.FileName);
                    string fileName = Path.GetFileName(sf.FileName);
                    string saveFile = Path.Combine(savePath, fileName);
                    try
                    {
                        workbook.SaveAs(saveFile); //Save the file

                        string escapedSaveFile = saveFile.Replace(@"\", @"\\");

                        var log = new AccessLog()
                        {
                            User = Login.Username,
                            Action = "Export",
                            Item = "Employee",
                            ItemId = id.ToString(),
                            Description = "IC: " + deletedEmp[0].Ic + "; Path: " + escapedSaveFile
                        };
                        log.Insert();

                        log = new AccessLog()
                        {
                            User = "system",
                            Action = "Delete from database",
                            Item = "Employee",
                            ItemId = id.ToString(),
                            Description = "IC: " + deletedEmp[0].Ic + "; exported in " + fileName
                        };
                        log.Insert();

                        deletedEmp[0].Delete();
                        //Reload Deleted Employee List
                        if (!search)
                            DeletedEmployeePage();
                        else
                            DeletedEmployeePage(_searchCondition);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                }
            }
        }

        private void Button27_Click(object sender, EventArgs e) //Permanent Delete All Employee button
        {
            var result = MessageBox.Show("Do you want to export all deleted employee from the database?" + Environment.NewLine +
                "Note: All exported employee will be deleted from the database.",
                "Export All Deleted Employee", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                var delEmpFirst = Employee.Where("id = (SELECT id FROM EMPLOYEE WHERE status = 'Disabled' LIMIT 0,1)", 0, 1);
                var delEmpLast = Employee.Where("id = (SELECT MAX(id) FROM EMPLOYEE WHERE status = 'Disabled' LIMIT 0,1)", 0, 1);
                if (!delEmpFirst.Any() || !delEmpLast.Any())
                {
                    MessageBox.Show("Export Error: Empty Employee Records.\n" +
                        "There was no deleted employee to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string defaultFileName = String.Format("EXPORT_EMPLOYEE_{0}~{1}_{2}", delEmpFirst[0].Id, delEmpLast[0].Id, DateTime.Now.ToString("ddMMyyyy_HHmmss"));

                var deletedEmpList = Employee.Where("status = 'Disabled'", 0, 2147483647);

                var workbook = new XLWorkbook();
                var ws = workbook.AddWorksheet("DeletedEmployee");
                ws.Cell(1, 1).Value = "Employee";
                ws.Cell(2, 1).InsertTable(deletedEmpList);

                SaveFileDialog sf = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = "Excel Workbook (.xlsx) |*.xlsx",
                    Title = "Export Employee as",
                    FilterIndex = 1
                };

                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string savePath = Path.GetDirectoryName(sf.FileName);
                    string fileName = Path.GetFileName(sf.FileName);
                    string saveFile = Path.Combine(savePath, fileName);
                    try
                    {
                        workbook.SaveAs(saveFile); //Save the file

                        string escapedSaveFile = saveFile.Replace(@"\", @"\\");

                        var log = new AccessLog()
                        {
                            User = Login.Username,
                            Action = "Export",
                            Item = "Employee",
                            ItemId = delEmpFirst[0].Id.ToString() + "~" + delEmpLast[0].Id.ToString(),
                            Description = "Path: " + escapedSaveFile
                        };
                        log.Insert();

                        foreach (Employee item in deletedEmpList)
                        {
                            log = new AccessLog()
                            {
                                User = "system",
                                Action = "Delete from database",
                                Item = "Employee",
                                ItemId = item.Id.ToString(),
                                Description = "IC: " + item.Ic + "; exported in " + fileName
                            };
                            log.Insert();
                            item.Delete();
                        }
                        //Reload Deleted Employee List
                        if (!search)
                            DeletedEmployeePage();
                        else
                            DeletedEmployeePage(_searchCondition);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Deletion error: Export file not saved." + Environment.NewLine +
                        "You cannot delete employee from the database without saving the exported file. ",
                        "Deletion Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ListView5_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Deleted Employee
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView5.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView5.Sorting == SortOrder.Ascending)
                    listView5.Sorting = SortOrder.Descending;
                else
                    listView5.Sorting = SortOrder.Ascending;
            }

            listView5.Sort();
            this.listView5.ListViewItemSorter = new ListViewItemComparer(e.Column, listView5.Sorting);
        }

        /// --Deleted Cabinet SubModule
        private void ReloadDeletedTypeList(int count, int offset, string condition)
        {
            listView8.Items.Clear();
            List<Type> items = Type.Where(condition, count, offset);
            foreach (Type t in items)
            {
                var cab = new Cabinet();
                ListViewItem lvi = new ListViewItem(t.Id.ToString());
                lvi.SubItems.Add(t.Name);
                lvi.SubItems.Add(t.Code);
                lvi.SubItems.Add(t.Rate.ToString("#0.00"));

                listView8.Items.Add(lvi);
            }
        }

        private void DeletedTypePage()
        {
            string condition = "status = 'Disabled'";
            var type = new Type();
            deletedLockerTypePage.FinalIndex = Convert.ToDouble(type.Count(condition));
            deletedLockerTypePage.LastPage = Convert.ToInt32(Math.Ceiling(deletedLockerTypePage.FinalIndex / deletedLockerTypePage.MaxItems));
            deletedLockerTypePage.PageSetting();
            if (deletedLockerTypePage.FinalIndex == 0)
            {
                deletedLockerTypePage.PageReset();
            }
            if (deletedLockerTypePage.PageNumber == deletedLockerTypePage.LastPage)
            { deletedLockerTypePage.LastIndex = (int)deletedLockerTypePage.FinalIndex; }
            toolStripLabel21.Text = String.Format("Page {0} / {1}", deletedLockerTypePage.PageNumber, deletedLockerTypePage.LastPage);
            ReloadDeletedTypeList(deletedLockerTypePage.IndexLimit, deletedLockerTypePage.MaxItems, condition);
        }

        private void Button29_Click(object sender, EventArgs e) //Restore Locker Type button
        {
            if (listView8.SelectedItems.Count <= 0)
                return;
            var result = MessageBox.Show("Do you want to restore this locker type?", "Restore Locker Type", MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView8.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);
                Type type = new Type();
                type = type.Get(id);
                type.Restore();

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Restore",
                    Item = "Locker Type",
                    ItemId = Convert.ToString(id),
                    Description = "Code: " + type.Code + "; Status Disabled to Active"
                };
                log.Insert();

                DeletedTypePage();

                //Clear combo box 1 & combo box items to avoid error
                _comboBoxItems.Clear();

                //Load Locker Type Name into Combo Box 1
                _typeList = Type.Where("status <> 'Disabled'", 0, 100);
                _comboBoxItems.Add(0, "All");
                foreach (Type t in _typeList)
                { _comboBoxItems.Add(t.Id, t.Name); }
                comboBox1.DataSource = new BindingSource(_comboBoxItems, null);
                comboBox1.DisplayMember = "Value";
                comboBox1.ValueMember = "Key";
            }
        }

        private void Button37_Click(object sender, EventArgs e) //Permanent Delete Locker Type button (1 Type)
        {
            if (listView8.SelectedItems.Count <= 0)
                return;

            var result = MessageBox.Show("Do you want to export this locker type from the database?\n" + Environment.NewLine +
                "Note: " + Environment.NewLine + "1. Exported locker type will be deleted from the database." + Environment.NewLine +
                "2. The locker type cannot be exported if existing cabinets for this locker type was not exported from the database.", 
                "Export Deleted Locker Type", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView8.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);

                var delType = Type.Where(String.Format("id = {0}", id), 0, 1);
                string defaultFileName = String.Format("EXPORT_LOCKER_TYPE_{0}_{1}", id, DateTime.Now.ToString("ddMMyyyy_HHmmss"));

                var typeCabinet = new Cabinet();
                int noOfCabinet = typeCabinet.Count(String.Format("type_id = {0}", id));
                if (noOfCabinet > 0)
                {
                    MessageBox.Show("Export error: Cabinet detected." + Environment.NewLine +
                        "You cannot export this locker type from the database with cabinet(s) assoicated to it exist in the database. " + Environment.NewLine + 
                        "To export this locker type, please export all cabinet(s) assoicated to it from the database.",
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var workbook = new XLWorkbook();
                var ws = workbook.AddWorksheet("DeletedCabinet");
                ws.Cell(1, 1).Value = "Locker Type";
                ws.Cell(2, 1).InsertTable(delType);

                SaveFileDialog sf = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = "Excel Workbook (.xlsx) |*.xlsx",
                    Title = "Export Locker Type as",
                    FilterIndex = 1
                };
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string savePath = Path.GetDirectoryName(sf.FileName);
                    string fileName = Path.GetFileName(sf.FileName);
                    string saveFile = Path.Combine(savePath, fileName);
                    try
                    {
                        workbook.SaveAs(saveFile); //Save the file

                        //As mysql database recognize '\' as escape, replace with '\\' to maintain the path name
                        string escapedSaveFile = saveFile.Replace(@"\", @"\\");

                        var log = new AccessLog()
                        {
                            User = Login.Username,
                            Action = "Export",
                            Item = "Locker Type",
                            ItemId = delType[0].Id.ToString(),
                            Description = "Code: " + delType[0].Code + "; Path: " + escapedSaveFile
                        };
                        log.Insert();

                        log = new AccessLog
                        {
                            User = "system",
                            Action = "Delete from database",
                            Item = "Locker Type",
                            ItemId = delType[0].Id.ToString(),
                            Description = "Code: " + delType[0].Code + "; exported in " + fileName
                        };
                        log.Insert();

                        delType[0].Delete();
                        //Reload Deleted Cabinet List
                        DeletedTypePage();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                }
            }
        }

        private void ToolStripButton42_Click(object sender, EventArgs e) //First Page for Deleted Locker Type
        {
            if (deletedLockerTypePage.PageNumber == 1)
            { return; }
            deletedLockerTypePage.PageNumber = 1;
            DeletedTypePage();
        }

        private void ToolStripButton43_Click(object sender, EventArgs e) //Previous Page for Deleted Locker Type
        {
            if (deletedLockerTypePage.PageNumber == 1)
            { return; }
            else
            {
                deletedLockerTypePage.PageNumber -= 1;
                DeletedTypePage();
            }
        }

        private void ToolStripButton44_Click(object sender, EventArgs e) //Next Page for Deleted Locker Type
        {
            if (deletedLockerTypePage.PageNumber == deletedLockerTypePage.LastPage)
            { return; }
            else
            {
                deletedLockerTypePage.PageNumber += 1;
                DeletedTypePage();
            }
        }

        private void ToolStripButton45_Click(object sender, EventArgs e) //Last Page for Deleted Locker Type
        {
            if (deletedLockerTypePage.PageNumber == deletedLockerTypePage.LastPage)
            { return; }
            deletedLockerTypePage.PageNumber = deletedLockerTypePage.LastPage;
            DeletedTypePage();
        }

        private void ListView8_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Deleted Locker Type
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView8.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView8.Sorting == SortOrder.Ascending)
                    listView8.Sorting = SortOrder.Descending;
                else
                    listView8.Sorting = SortOrder.Ascending;
            }

            listView8.Sort();
            this.listView8.ListViewItemSorter = new ListViewItemComparer(e.Column, listView8.Sorting);
        }

        private void ReloadDeletedCabinetList(int count, int offset, string condition)
        {
            List<Type> _typeList = Type.Where("status IS NOT NULL", 0, 100);

            listView9.Items.Clear();
            List<Cabinet> items = Cabinet.Where(condition, count, offset);
            foreach (Cabinet cab in items)
            {
                ListViewItem lvi = new ListViewItem(cab.Id.ToString());
                lvi.SubItems.Add(cab.Code);

                var cabinetSize = from selected in _typeList
                                   where selected.Id.Equals(cab.TypeID)
                                   select selected.Code;
                string sizeCode = cabinetSize.First();

                lvi.SubItems.Add(sizeCode);
                lvi.SubItems.Add(cab.Row.ToString());
                lvi.SubItems.Add(cab.Column.ToString());

                listView9.Items.Add(lvi);
            }
        }

        private void DeletedCabinetPage()
        {
            string condition = "status = 'Disabled'";
            var cab = new Cabinet();
            deletedCabinetPage.FinalIndex = Convert.ToDouble(cab.Count(condition));
            deletedCabinetPage.LastPage = Convert.ToInt32(Math.Ceiling(deletedCabinetPage.FinalIndex / deletedCabinetPage.MaxItems));
            deletedCabinetPage.PageSetting();
            if (deletedCabinetPage.FinalIndex == 0)
            {
                deletedCabinetPage.PageReset();
            }
            if (deletedCabinetPage.PageNumber == deletedCabinetPage.LastPage)
            { deletedCabinetPage.LastIndex = (int)deletedCabinetPage.FinalIndex; }
            toolStripLabel19.Text = String.Format("Page {0} / {1}", deletedCabinetPage.PageNumber, deletedCabinetPage.LastPage);
            toolStripLabel20.Text = String.Format("Showing result {0}~{1}", deletedCabinetPage.FirstIndex, deletedCabinetPage.LastIndex);
            ReloadDeletedCabinetList(deletedCabinetPage.IndexLimit, deletedCabinetPage.MaxItems, condition);
        }

        private void DeletedCabinetPage(string condition)
        {
            var cab = new Cabinet();
            deletedCabinetPage.FinalIndex = Convert.ToDouble(cab.Count(condition));
            deletedCabinetPage.LastPage = Convert.ToInt32(Math.Ceiling(deletedCabinetPage.FinalIndex / deletedCabinetPage.MaxItems));
            deletedCabinetPage.PageSetting();
            if (deletedCabinetPage.FinalIndex == 0)
            {
                deletedCabinetPage.PageReset();
            }
            if (deletedCabinetPage.PageNumber == deletedCabinetPage.LastPage)
            { deletedCabinetPage.LastIndex = (int)deletedCabinetPage.FinalIndex; }
            toolStripLabel19.Text = String.Format("Page {0} / {1}", deletedCabinetPage.PageNumber, deletedCabinetPage.LastPage);
            toolStripLabel20.Text = String.Format("Showing result {0}~{1}", deletedCabinetPage.FirstIndex, deletedCabinetPage.LastIndex);
            ReloadDeletedCabinetList(deletedCabinetPage.IndexLimit, deletedCabinetPage.MaxItems, condition);
        }

        private void Button34_Click(object sender, EventArgs e) //Restore Cabinet button
        {
            if (listView9.SelectedItems.Count <= 0)
                return;

            var result = MessageBox.Show("Do you want to restore this cabinet?", "Restore Cabinet", MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView9.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);
                var cab = new Cabinet();
                var cabItem = cab.Get(id);

                //Check if the Locker Type available in the list (Not deleted). If no, show error.
                var type = new Type();
                var typeItem = type.Get(cabItem.TypeID);
                if (typeItem.IsDisabled())
                {
                    MessageBox.Show("Restoration Error: Locker Type disabled." + Environment.NewLine +
                        "To restore this cabinet, please ensure that the locker type '" + typeItem.Name + "' is restored.",
                        "Restoration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                cabItem.Restore(); //Restore the cabinet
                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Restore",
                    Item = "Cabinet",
                    ItemId = Convert.ToString(id),
                    Description = "Code: " + cabItem.Code + "; Status: Disabled to Available"
                };
                log.Insert();

                //Calculate how many lockers in the cabinet
                int noOfLockers = cabItem.Row * cabItem.Column;

                //Restore the lockers assoicated to this cabinet
                List<Locker> lockerList = Locker.Where(String.Format("cabinet_id = {0}", id), 0, noOfLockers);
                for (int i = 0; i < noOfLockers; i++)
                {
                    var restoreLocker = new Locker();
                    restoreLocker.Id = lockerList[i].Id;
                    restoreLocker.Reset();

                    log.User = "system";
                    log.Action = "Restore";
                    log.Item = "Locker";
                    log.ItemId = lockerList[i].Id.ToString();
                    log.Description = "Code: " + lockerList[i].Code + "; Status: Status: Disabled to Available";
                    log.Insert();
                }

                if (!search)
                    DeletedCabinetPage();
                else
                    DeletedCabinetPage(_searchCondition);
                _pageFlip = true;
                SmallCabinetPage();
            }
        }

        private void Button38_Click(object sender, EventArgs e) //Filter Cabinet button in Deleted Cabinet
        {
            var FilterCabinetForm = new CabinetForm(true);
            FilterCabinetForm.ShowDialog();
            if (string.IsNullOrWhiteSpace(FilterCabinetForm.Condition))
            { return; }
            else
            {
                _searchCondition = FilterCabinetForm.Condition;
                search = true;
                deletedCabinetPage.PageNumber = 1;
                DeletedCabinetPage(_searchCondition);
            }
        }

        private void Button39_Click(object sender, EventArgs e) //Permanent Delete Cabinet button (1 cabinet)
        {
            if (listView9.SelectedItems.Count <= 0)
                return;
            var result = MessageBox.Show("Do you want to export this cabinet from the database?\n" + Environment.NewLine +
                 "Note:\n" + "1. The assoicated lockers for this cabinet will also be exported.\n" +
                 "2. Exported cabinets and lockers will be deleted from the database.",
                 "Export Deleted Cabinet", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                ListViewItem lvi = listView9.SelectedItems[0];
                int id = Convert.ToInt32(lvi.Text);

                var delCab = Cabinet.Where(String.Format("id = {0}", id), 0, 1);
                var delLockers = Locker.Where(String.Format("cabinet_id = {0}", id), 0, 2147483467);

                string defaultFileName = String.Format("EXPORT_CABINET_{0}_{1}", id, DateTime.Now.ToString("ddMMyyyy_HHmmss"));

                var workbook = new XLWorkbook();
                var ws = workbook.AddWorksheet("DeletedCabinet");
                ws.Cell(1, 1).Value = "Cabinet";
                ws.Cell(2, 1).InsertTable(delCab);
                ws.Cell(5, 1).Value = "Locker";
                ws.Cell(6, 1).InsertTable(delLockers);

                SaveFileDialog sf = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = "Excel Workbook (.xlsx) |*.xlsx",
                    Title = "Export Cabinet as",
                    FilterIndex = 0
                };
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string savePath = Path.GetDirectoryName(sf.FileName);
                    string fileName = Path.GetFileName(sf.FileName);
                    string saveFile = Path.Combine(savePath, fileName);
                    try
                    {
                        workbook.SaveAs(saveFile); //Save the file

                        string escapedSaveFile = saveFile.Replace(@"\", @"\\");

                        var log = new AccessLog()
                        {
                            User = Login.Username,
                            Action = "Export",
                            Item = "Cabinet",
                            ItemId = delCab[0].Id.ToString(),
                            Description = "Code: " + delCab[0].Code + "; Path: " + escapedSaveFile
                        };
                        log.Insert();

                        foreach(Locker dLocker in delLockers)
                        {
                            log = new AccessLog()
                            {
                                User = "system",
                                Action = "Delete from database",
                                Item = "Locker",
                                ItemId = dLocker.Id.ToString(),
                                Description = "Code: " + dLocker.Code + "; exported in " + fileName
                            };
                            log.Insert();

                            dLocker.Delete();
                        }

                        log = new AccessLog()
                        {
                            User = "system",
                            Action = "Delete from database",
                            Item = "Cabinet",
                            ItemId = delCab[0].Id.ToString(),
                            Description = "Code: " + delCab[0].Code + "; exported in " + fileName
                        };
                        log.Insert();

                        delCab[0].Delete();
                        //Reload Deleted Cabinet List
                        if (!search)
                            DeletedCabinetPage();
                        else
                            DeletedCabinetPage(_searchCondition);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                }
            }
        }

        private void ToolStripButton38_Click(object sender, EventArgs e) //First Page for Deleted Cabinet
        {
            if (deletedCabinetPage.PageNumber == 1)
            { return; }
            deletedCabinetPage.PageNumber = 1;
            if (!search)
                DeletedCabinetPage();
            else
                DeletedCabinetPage(_searchCondition);
        }

        private void ToolStripButton39_Click(object sender, EventArgs e) //Previous Page for Deleted Cabinet
        {
            if (deletedCabinetPage.PageNumber == 1)
            { return; }
            else
            {
                deletedCabinetPage.PageNumber -= 1;
                if (!search)
                    DeletedCabinetPage();
                else
                    DeletedCabinetPage(_searchCondition);
            }
        }

        private void ToolStripButton40_Click(object sender, EventArgs e) //Next Page for Deleted Cabinet
        {
            if (deletedCabinetPage.PageNumber == deletedCabinetPage.LastPage)
            { return; }
            else
            {
                deletedCabinetPage.PageNumber += 1;
                if (!search)
                    DeletedCabinetPage();
                else
                    DeletedCabinetPage(_searchCondition);
            }
        }

        private void ToolStripButton41_Click(object sender, EventArgs e) //Last Page for Deleted Cabinet
        {
            if (deletedCabinetPage.PageNumber == deletedCabinetPage.LastPage)
            { return; }
            deletedCabinetPage.PageNumber = deletedCabinetPage.LastPage;
            if (!search)
                DeletedCabinetPage();
            else
                DeletedCabinetPage(_searchCondition);
        }

        private void ListView9_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Deleted Cabinet
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView9.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView9.Sorting == SortOrder.Ascending)
                    listView9.Sorting = SortOrder.Descending;
                else
                    listView9.Sorting = SortOrder.Ascending;
            }

            listView9.Sort();
            this.listView9.ListViewItemSorter = new ListViewItemComparer(e.Column, listView9.Sorting);
        }

        //Transaction Module
        private void Button14_Click(object sender, EventArgs e) //Transaction button
        {
            panel11.Controls.Remove(panel12_EmployeeBasePanel);
            panel11.Controls.Remove(panel13_CabinetBasePanel);
            panel11.Controls.Add(panel15_TransactionBasePanel);
            panel11.Controls.Remove(panel16_DeletedRecordsBasePanel);
            panel11.Controls.Remove(panel14_LogBasePanel);
            search = false;
            transactionPage.PageReset();
            TransactionPage();
        }

        private void ReloadTransactionList(int count, int offset, string condition)
        {
            listView13.Items.Clear();
            List<Transaction> items = Transaction.Where(condition, count, offset);
            foreach (Transaction t in items)
            {
                var cab = new Cabinet();
                ListViewItem lvi = new ListViewItem(t.Id.ToString());
                lvi.SubItems.Add(t.RentalID.ToString());
                lvi.SubItems.Add(t.CustomerID.ToString());
                lvi.SubItems.Add(t.LockerID.ToString());
                decimal totalPrice = t.Duration * t.TypeRate;
                lvi.SubItems.Add(totalPrice.ToString("#0.00"));
                string returnDate = t.ReturnDate.ToString("dd-MM-yyyy");
                if (returnDate == "01-01-0001")
                    returnDate = "";
                lvi.SubItems.Add(returnDate);
                lvi.SubItems.Add(t.Fine.ToString("#0.00"));

                listView13.Items.Add(lvi);
            }
        }

        private void TransactionPage()
        {
            string condition = "id IS NOT NULL";
            var trans = new Transaction();
            transactionPage.FinalIndex = Convert.ToDouble(trans.Count(condition));
            transactionPage.LastPage = Convert.ToInt32(Math.Ceiling(transactionPage.FinalIndex / transactionPage.MaxItems));
            transactionPage.PageSetting();
            if (transactionPage.FinalIndex == 0)
            {
                transactionPage.PageReset();
            }
            if (transactionPage.PageNumber == transactionPage.LastPage)
            { transactionPage.LastIndex = (int)transactionPage.FinalIndex; }
            toolStripLabel28.Text = String.Format("Page {0} / {1}", transactionPage.PageNumber, transactionPage.LastPage);
            toolStripLabel29.Text = String.Format("Showing result {0}~{1}", transactionPage.FirstIndex, transactionPage.LastIndex);
            ReloadTransactionList(transactionPage.IndexLimit, transactionPage.MaxItems, condition);
        }

        private void ToolStripButton60_Click(object sender, EventArgs e) //First Page for Transaction
        {
            if (transactionPage.PageNumber == 1)
            { return; }
            transactionPage.PageNumber = 1;
            TransactionPage();
        }

        private void ToolStripButton61_Click(object sender, EventArgs e) //Previous Page for Transaction
        {
            if (transactionPage.PageNumber == 1)
            { return; }
            else
            {
                transactionPage.PageNumber -= 1;
                TransactionPage();
            }
        }

        private void ToolStripButton62_Click(object sender, EventArgs e) //Next Page for Transaction
        {
            if (transactionPage.PageNumber == transactionPage.LastPage)
            { return; }
            else
            {
                transactionPage.PageNumber += 1;
                TransactionPage();
            }
        }

        private void ToolStripButton63_Click(object sender, EventArgs e) //Last Page for Transaction
        {
            if (transactionPage.PageNumber == transactionPage.LastPage)
            { return; }
            transactionPage.PageNumber = transactionPage.LastPage;
            TransactionPage();
        }

        private void Button46_Click(object sender, EventArgs e) //View Transaction Detail button
        {
            if (listView13.SelectedItems.Count <= 0)
                return;
            ListViewItem lvi = listView13.SelectedItems[0];
            int id = Convert.ToInt32(lvi.Text);
            var ViewTransactionForm = new TransactionForm(id);
            ViewTransactionForm.ShowDialog();
        }

        private void Button49_Click(object sender, EventArgs e) //Export Transaction button
        {
            var ExportTransactionsForm = new TransactionForm();
            ExportTransactionsForm.ShowDialog();
            if (ExportTransactionsForm.ExportComplete)
                TransactionPage();
        }

        private void ListView13_ColumnClick(object sender, ColumnClickEventArgs e)  //Sorting for transaction list
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView13.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView13.Sorting == SortOrder.Ascending)
                    listView13.Sorting = SortOrder.Descending;
                else
                    listView13.Sorting = SortOrder.Ascending;
            }

            listView13.Sort();
            this.listView13.ListViewItemSorter = new ListViewItemComparer(e.Column, listView13.Sorting);
        }

    }
}