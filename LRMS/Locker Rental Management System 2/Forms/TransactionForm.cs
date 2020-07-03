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
    public partial class TransactionForm : Form
    {
        //View Transaction Part
        public TransactionForm(int id)
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel2);

            var item = new Transaction();
            item = item.Get(id);
            LoadTransactionData(item);
        }

        private void LoadTransactionData(Transaction item)
        {
            //Rental
            textBox1.Text = item.Id.ToString();
            textBox2.Text = item.RentalID.ToString();
            textBox3.Text = item.StartDate.ToString("dd-MM-yyyy");
            DateTime endDate = item.StartDate.Date.AddDays(item.Duration);
            textBox4.Text = endDate.ToString("dd-MM-yyyy");
            textBox5.Text = item.Duration.ToString();
            numericUpDown1.Value = item.TypeRate * item.Duration;

            //Customer
            textBox7.Text = item.CustomerID.ToString();
            var cusList = Customer.Where(String.Format("id = {0}", item.CustomerID), 0, 1);
            if (cusList.Any())
            {
                textBox8.Text = cusList[0].Name;
                textBox9.Text = cusList[0].Ic;
            }

            //Locker
            textBox10.Text = item.LockerID.ToString();
            var lockerList = Locker.Where(String.Format("id = {0}", item.LockerID), 0, 1);
            if (lockerList.Any())
            {
                textBox11.Text = lockerList[0].Code;
                var cabList = Cabinet.Where(String.Format("id = {0}", lockerList[0].CabinetID), 0, 1);
                if (cabList.Any())
                {
                    textBox12.Text = cabList[0].Code;
                }
            }
            textBox13.Text = item.TypeName;
            numericUpDown2.Value = item.TypeRate;

            //End Rental
            var tempReturnDate = item.ReturnDate.ToString("dd-MM-yyyy");
            //If the return date is not initialized, do not show the date.
            if (tempReturnDate != "01-01-0001")
                textBox15.Text = tempReturnDate;
            textBox16.Text = item.OverdueTime.ToString();
            numericUpDown3.Value = item.Fine;

            //RentalStatus
            if (item.OverdueTime > 0)
                checkBox1.Checked = true;

            List<RentalStatus> statusList = RentalStatus.Where(String.Format("transaction_id = {0}", item.Id), 0, 10);
            var statuses = from selected in statusList
                           where selected.StatusId.ToString().Contains("3")
                           select selected;
            if (statuses.Any())
                checkBox2.Checked = true;

            statuses = from selected in statusList
                       where selected.StatusId.ToString().Contains("4")
                       select selected;
            if (statuses.Any())
                checkBox3.Checked = true;
        }

        private void Button1_Click(object sender, EventArgs e) //Ok button or Cancel button
        {
            this.Close();
        }

        //Export Transaction Part
        private bool _exportComplete = false;
        public bool ExportComplete { get { return _exportComplete; } }

        public TransactionForm()
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel3);

            this.Height = 210;
            this.Width = 330;
        }

        private void Button3_Click(object sender, EventArgs e) //Export button
        {
            string month = dateTimePicker1.Value.ToString("MM");
            string longMonth = dateTimePicker1.Value.ToString("MMM");
            string fullMonth = dateTimePicker1.Value.ToString("MMMM");
            string year = dateTimePicker1.Value.ToString("yyyy");

            var result = MessageBox.Show("Do you want to export transactions for " + fullMonth + " " + year +"?\n\n" +
                "Note:\n1. Exported transactions will be deleted from the database. \n2. If you did not generate the report for "+ 
                fullMonth + " " + year + ", please generate and export it first.", 
                "Export Transactions", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                if (dateTimePicker1.Value.Month == DateTime.Now.Month && dateTimePicker1.Value.Year == DateTime.Now.Year)
                {
                    MessageBox.Show("Export Error: You can't export transactions for this month \n(" + fullMonth + " " + year + ").", 
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string defaultFileName = String.Format("EXPORT_TRANSACTIONS_{0}_{1}_{2}", longMonth, year,
                    DateTime.Now.ToString("ddMMyyyy_HHmmss"));

                string dateCond = String.Format("return_date LIKE '{0}-{1}-%'", year, month);
                var transactionsList = Transaction.Where(dateCond, 0, 2147483467);
                if (!transactionsList.Any())
                {
                    MessageBox.Show("Error: Empty Record.\nThere are no records in " + fullMonth + " " + year + ".", "Empty Record",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                SaveFileDialog sf = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = "Excel Workbook (.xlsx) |*.xlsx",
                    Title = "Export Transactions as",
                    FilterIndex = 1
                };

                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string savePath = Path.GetDirectoryName(sf.FileName);
                    string fileName = Path.GetFileName(sf.FileName);
                    string saveFile = Path.Combine(savePath, fileName);
                    try
                    {
                        var workbook = new XLWorkbook();
                        var ws = workbook.AddWorksheet("Transactions");
                        ws.Cell(1, 1).Value = String.Format("Transactions in {0} {1}", fullMonth, year);
                        ws.Cell(2, 1).InsertTable(transactionsList);

                        DataTable statusTable = new DataTable();
                        statusTable.Columns.Add("Transaction ID");
                        statusTable.Columns.Add("Status ID");
                        foreach (Transaction t in transactionsList)
                        {
                            var statusList = RentalStatus.Where(String.Format("transaction_id = {0}", t.Id), 0, 5);
                            foreach (RentalStatus s in statusList)
                                statusTable.Rows.Add(s.TransactionId, s.StatusId);
                        }
                        ws.Cell(ws.LastRowUsed().RowNumber() + 2, 1).InsertTable(statusTable);

                        workbook.SaveAs(saveFile); //Save the file

                        string escapedSaveFile = saveFile.Replace(@"\", @"\\");

                        var log = new AccessLog() //AccessLog for export
                        {
                            User = Login.Username,
                            Action = "Export",
                            Item = "Transaction",
                            Description = "Month of Transaction Return Date: " + fullMonth + " " + year + 
                            "; Path: " + escapedSaveFile
                        };
                        log.Insert();

                        foreach (Transaction t in transactionsList)
                        {
                            var statusList = RentalStatus.Where(String.Format("transaction_id = {0}", t.Id), 0, 5);
                            foreach (RentalStatus s in statusList)
                            {
                                s.Delete();
                                log = new AccessLog()
                                {
                                    User = "system",
                                    Action = "Delete from database",
                                    ItemId = s.TransactionId + ", " + s.StatusId,
                                    Item = "Rental Status",
                                    Description = "Exported in " + fileName
                                };
                                log.Insert();
                            }

                            t.Delete();
                            log = new AccessLog()
                            {
                                User = "system",
                                Action = "Delete from database",
                                Item = "Transaction",
                                Description = "Month of Transaction Return Date: " + fullMonth + " " + year 
                                + "; exported in " + fileName
                            };
                            log.Insert();
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
    }
}
