using ClosedXML.Excel;
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
    public partial class AccessLogFilterForm : Form
    {
        private string _condition;
        private string _action;
        private string _item;
        private DateTime _firstLogDate;

        public string Condition { get { return _condition; } set { _condition = value; } }

        //Filter Access Log
        public AccessLogFilterForm()
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel1);
            List<AccessLog> logList = AccessLog.All(0,1);
            _firstLogDate = DateTime.Parse(logList[0].LogDate);
            dateTimePicker1.MinDate = dateTimePicker1.Value = _firstLogDate;
            dateTimePicker1.MaxDate = DateTime.Now;
            dateTimePicker2.MinDate = _firstLogDate;
            dateTimePicker2.MaxDate = DateTime.Now;
            radioButton1.Checked = true;
            radioButton13.Checked = true;
        }

        private void Button1_Click(object sender, EventArgs e) //Close button
        {
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e) //OK button
        {
            string startDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string endDate = dateTimePicker2.Value.ToString("yyyy-MM-dd");
            string dateCondition = string.Format("DATE(log_date) BETWEEN '{0}' AND '{1}'", startDate, endDate);

            if (DateTime.Compare(dateTimePicker1.Value.Date, dateTimePicker2.Value.Date) > 0)
            {
                MessageBox.Show("Input Error: Invalid Date Range" + Environment.NewLine +
                    "Please ensure date in 'From: ' is not later than the date in 'Until: '.", "Input Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string usernameCond;
            if(comboBox1.Text == "Start with")
            { usernameCond = "'{0}%'"; }
            else if (comboBox1.Text == "End with")
            { usernameCond = "'%{0}'"; }
            else if (comboBox1.Text == "Contains")
            { usernameCond = "'%{0}%'"; }
            else
            { usernameCond = "";}

            if (usernameCond != "")
            {
                usernameCond = String.Format(usernameCond, textBox1.Text);
                usernameCond = String.Format("log_user LIKE {0}", usernameCond);
            }
            else
            { usernameCond = "log_user IS NOT NULL"; }

            string actionCond;
            actionCond = String.Format("log_action {0}", _action);

            string itemCond;
            itemCond = String.Format("log_item {0}", _item);

            _condition = string.Format("{0} AND {1} AND {2} AND {3}", dateCondition, usernameCond, actionCond,
                itemCond);
            this.Close();
        }

        //Action
        private void RadioButton1_CheckedChanged(object sender, EventArgs e) //All
        {
            _action = "IS NOT NULL";
        }
        private void RadioButton2_CheckedChanged(object sender, EventArgs e) //Add
        {
            _action = "= 'Add'";
        }
        private void RadioButton3_CheckedChanged(object sender, EventArgs e) //Update
        {
            _action = "= 'Update'";
        }
        private void RadioButton4_CheckedChanged(object sender, EventArgs e) //Delete
        {
            _action = "= 'Delete'";
        }
        private void RadioButton5_CheckedChanged(object sender, EventArgs e) //Login
        {
            _action = "= 'Login'";
        }
        private void RadioButton6_CheckedChanged(object sender, EventArgs e) //Logout
        {
            _action = "= 'Log Out'";
        }
        private void RadioButton7_CheckedChanged(object sender, EventArgs e) //Restore
        {
            _action = "= 'Restore'";
        }
        private void RadioButton8_CheckedChanged(object sender, EventArgs e) //Export
        {
           _action = "= 'Export'";
        }
        private void RadioButton9_CheckedChanged(object sender, EventArgs e) //End 
        {
            _action = "= 'End'";
        }
        private void RadioButton10_CheckedChanged(object sender, EventArgs e) //Reset 
        {
            _action = "= 'Reset'";
        }
        private void RadioButton11_CheckedChanged(object sender, EventArgs e) //Disable
        {
            _action = "= 'Disable'";
        }
        private void RadioButton12_CheckedChanged(object sender, EventArgs e) // Delete from database
        {
            _action = "= 'Delete from database'";
        }

        //Item
        private void RadioButton13_CheckedChanged(object sender, EventArgs e) //All
        {
            _item = "IS NOT NULL";
        }
        private void RadioButton21_CheckedChanged(object sender, EventArgs e) //Access Log
        {
            _item = "= 'Access Log'";
        }
        private void RadioButton17_CheckedChanged(object sender, EventArgs e) //Cabinet
        {
            _item = "= 'Cabinet'";
        }
        private void RadioButton14_CheckedChanged(object sender, EventArgs e) //Customer
        {
            _item = "= 'Customer'";
        }
        private void RadioButton15_CheckedChanged(object sender, EventArgs e) //Employee
        {
            _item = "= 'Employee'";
        }
        private void RadioButton18_CheckedChanged(object sender, EventArgs e) //Locker
        {
            _item = "= 'Locker'";
        }
        private void RadioButton16_CheckedChanged(object sender, EventArgs e) //Locker Type
        {
            _item = "= 'Locker Type'";
        }
        private void RadioButton19_CheckedChanged(object sender, EventArgs e) //Rental
        {
            _item = "= 'Rental'";
        }
        private void RadioButton22_CheckedChanged(object sender, EventArgs e) //Rental Settings
        {
            _item = "= 'Rental Settings'";
        }
        private void RadioButton24_CheckedChanged(object sender, EventArgs e) //Rental Status
        {
            _item = "= 'Rental Status'";
        }
        private void RadioButton23_CheckedChanged(object sender, EventArgs e) //Sales Report
        {
            _item = "= 'Sales Report'";
        }
        private void RadioButton20_CheckedChanged(object sender, EventArgs e) //Transaction
        {
            _item = "= 'Transaction'";
        }
        private void RadioButton25_CheckedChanged(object sender, EventArgs e) //Empty
        {
            _item = "= ''";
        }


        //Export Access Log
        private bool _exportComplete = false;
        public bool ExportComplete { get { return _exportComplete; } }

        public AccessLogFilterForm(bool export)
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel4);
            this.Height = 200;
            List<AccessLog> logList = AccessLog.All(0, 1);
            _firstLogDate = DateTime.Parse(logList[0].LogDate);
            dateTimePicker3.MinDate = dateTimePicker3.Value = _firstLogDate;
            dateTimePicker3.MaxDate = DateTime.Now.Date;
            dateTimePicker4.MinDate = _firstLogDate;
            dateTimePicker4.MaxDate = DateTime.Now.Date;
        }

        private void Button3_Click(object sender, EventArgs e) //Close button
        {
            this.Close();
        }

        private void Button4_Click(object sender, EventArgs e) //Export button
        {
            if (DateTime.Compare(dateTimePicker3.Value.Date, dateTimePicker4.Value.Date) > 0)
            {
                MessageBox.Show("Input Error: Invalid Date Range" + Environment.NewLine +
                    "Please ensure date in 'From: ' is not later than the date in 'Until: '.", "Input Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dateTimePicker3.Value.Date == DateTime.Now || dateTimePicker4.Value.Date == DateTime.Now.Date)
            {
                MessageBox.Show("Export Error: Date Today Detected.\n" +
                    "You cannot export access logs for today (" + DateTime.Now.Date.ToString("dd MMMM yyyy") + ").", 
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string startDate = dateTimePicker3.Value.ToString("dd-MM-yyyy");
            string endDate = dateTimePicker4.Value.ToString("dd-MM-yyyy");

            var result = MessageBox.Show("Do you want to export access log from " + startDate + " until " + endDate + "?\n\n" +
                "Note: Exported access log will be deleted from the database.",
                "Export Access Log", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                string defaultFileName = String.Format("EXPORT_ACCESS_LOG_{0}_{1}", 
                    String.Format("{0}~{1}", startDate, endDate), DateTime.Now.ToString("ddMMyyyy_HHmmss"));

                string dbStartDate = dateTimePicker3.Value.ToString("yyyy-MM-dd");
                string dbEndDate = dateTimePicker4.Value.ToString("yyyy-MM-dd");
                string dateCond = String.Format("DATE(log_date) BETWEEN '{0}' AND '{1}'", dbStartDate, dbEndDate);

                List<AccessLog> logList = AccessLog.Where(dateCond, 0, 2147483647);
                var workbook = new XLWorkbook();
                var ws = workbook.AddWorksheet("AccessLog");
                ws.Cell(1, 1).Value = "Access Log";
                ws.Cell(2, 1).InsertTable(logList);

                SaveFileDialog sf = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = "Excel Workbook (.xlsx) |*.xlsx",
                    Title = "Export Access Log as",
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
                            Item = "Access Log",
                            Description = "Log Date: " + startDate + "~" + endDate + "; Path: " + escapedSaveFile
                        };
                        log.Insert();

                        foreach (var item in logList)
                        {
                            log = new AccessLog()
                            {
                                User = "system",
                                Action = "Delete from database",
                                Item = "Access Log",
                                Description = "Exported in " + fileName
                            };
                            log.Insert();
                            item.Delete();
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                    _exportComplete = true;
                    this.Close();
                }
            }
        }

        //View Full Description of Access Log
        public AccessLogFilterForm(int id)
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel7);

            var log = new AccessLog();
            log = log.Get(id);

            LoadAccessLogData(log);
        }

        private void LoadAccessLogData(AccessLog item)
        {
            textBox2.Text = item.Id.ToString();
            textBox3.Text = DateTime.Parse(item.LogDate).ToString("dd-MM-yyyy");
            textBox4.Text = item.LogTime;
            textBox5.Text = item.User;
            textBox6.Text = item.Action;
            textBox7.Text = item.Item;
            textBox8.Text = item.ItemId;

            string tempDes = item.Description;
            if (!String.IsNullOrWhiteSpace(tempDes))
            {
                tempDes = tempDes.Replace("; ", ";\n\n");
            }
            richTextBox1.Text = tempDes;
        }
    }
}
