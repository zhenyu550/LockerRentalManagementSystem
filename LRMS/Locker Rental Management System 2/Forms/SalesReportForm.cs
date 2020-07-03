using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
using System.Windows.Forms.DataVisualization.Charting;

namespace LockerRentalManagementSystem
{
    public partial class SalesReportForm : Form
    {
        private List<Reporting> _items;
        private int sortColumn = -1;

        public SalesReportForm()
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel1);
            radioButton1.Checked = true;
        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e) //Total Income radio button
        {
            label5.Text = "Total Income";
            richTextBox1.Text = "Total income report shows the total income of each locker types and fines for the month selected.\n\n" +
                "The calculation of the total income was based on the transactions stored in the database.\n\n" +
                "Only transactions for ended rentals are selected.\n\n" +
                "";
        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e) //Total Usage Frequency radio button
        {
            label5.Text = "Total Usage Frequency";
            richTextBox1.Text = "Total Usage Frequency report shows the most usage frequency of each locker types for the month selected.\n\n" +
                "The calculation of the usage frequency for each locker type was based on the transactions stored in the database.\n\n" +
                "Only transactions for ended rentals are selected.\n\n" +
                "Usage Frequency is how many times the locker type was used in the transactions.\n\n" +
                "Example:\nTransaction 1: Locker XL - 2 days;\nTransaction 2: Locker L - 3 days;\nHence, usage frequency for XL = 1 time and L = 1 time.";
        }

        private void RadioButton3_CheckedChanged(object sender, EventArgs e) //Total Usage Days radio button
        {
            label5.Text = "Total Usage Days";
            richTextBox1.Text = "Total Usage Days report shows the most usage days of each locker types for the month selected.\n\n" +
                "The calculation of the usage days for each locker type was based on the transactions stored in the database.\n\n" +
                "Only transactions for ended rentals are selected.\n\n" +
                "Usage Days are the total number of days the locker type was used in the transactions.\n\n" +
                "Example:\nTransaction 1: Locker XL - 2 days;\nTransaction 2: Locker L - 3 days;\nHence, usage days for XL = 2 days and L = 3 days.";
        }

        private void Button5_Click(object sender, EventArgs e) //Generate Report Button
        {
            string reportMonth = "return_date LIKE '{0}-{1}%'";
            string fullMonth = dateTimePicker1.Value.ToString("MMMM");
            string month = dateTimePicker1.Value.ToString("MM");
            string year = dateTimePicker1.Value.ToString("yyyy");
            reportMonth = String.Format(reportMonth, year, month);
            _items = Reporting.Where(reportMonth);
            if (!_items.Any())
            {
                MessageBox.Show("Error: Empty Record.\nThere are no records in " + fullMonth + " " + year + ". ",
                    "Empty Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (radioButton1.Checked)
            {
                label1.Text = String.Format("Total Income for {0} {1}", fullMonth, year);
                LoadMonthIncome();
            }
            else if (radioButton2.Checked)
            {
                label2.Text = String.Format("Total Usage Frequency for each locker type in {0} {1}", fullMonth, year);
                LoadMonthUseFrequency();
            }
            else
            {
                label3.Text = String.Format("Total Usage Days for each locker type in {0} {1}", fullMonth, year);
                LoadMonthUseDays();
            }
        }

        //Monthly income module
        private void LoadMonthIncome()
        {
            Controls.Remove(panel1);
            Controls.Add(panel2);

            listView1.Items.Clear();
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }
            chart1.Titles.Clear();

            decimal totalFine = 0;
            decimal totalIncome = 0;
            var itemDic = new Dictionary<string, decimal>();

            foreach (Reporting t in _items)
            {
                ListViewItem lvi = new ListViewItem(t.TypeName);
                lvi.SubItems.Add(t.TotalEarning.ToString("#0.00"));
                listView1.Items.Add(lvi);
                itemDic.Add(t.TypeName, t.TotalEarning);
                totalFine += t.TotalFine;
                totalIncome += t.TotalEarning;
            }

            if (totalFine != 0)
            {
                ListViewItem lvif = new ListViewItem("Fine");
                lvif.SubItems.Add(totalFine.ToString("#0.00"));
                listView1.Items.Add(lvif);
                itemDic.Add("Fine", totalFine);
            }
            decimal grandTotal = totalFine + totalIncome;

            ListViewItem lvit = new ListViewItem("Total");
            lvit.SubItems.Add(grandTotal.ToString("#0.00"));
            listView1.Items.Add(lvit);

            List<decimal> percentage = new List<decimal>();
            for (int i = 0; i < _items.Count(); i++)
            {
                percentage.Add(_items[i].TotalEarning / grandTotal);
            }
            if (totalFine != 0)
                percentage.Add(totalFine / grandTotal);
            percentage.Add(1);

            int j = 0;
            foreach (ListViewItem lvItem in listView1.Items)
            {
                lvItem.SubItems.Add(percentage[j].ToString("#0.00%"));
                j++;
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            string[] x = (from selected in itemDic
                          select selected.Key).ToArray();
            decimal[] y = (from selected in itemDic
                           select selected.Value).ToArray();
            string[] titles = { label1.Text };

            chart1.Titles.Add(titles[0]);
            chart1.Titles[0].Text = label1.Text;

            chart1.Series[0].ChartType = SeriesChartType.Pie;
            chart1.Series[0].Points.DataBindXY(x, y);
            chart1.Legends[0].Enabled = true;
            foreach (DataPoint p in chart1.Series[0].Points)
            {
                p.Label = "#PERCENT";
                p.LegendText = "#VALX";
            }
        }

        private void Button1_Click(object sender, EventArgs e) //Close button
        {
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e) //Back button in Monthly Income
        {
            Controls.Remove(panel2);
            Controls.Add(panel1);
        }

        private void Button3_Click(object sender, EventArgs e) //Export button in Monthly Income
        {
            string defaultFileName = String.Format("EXPORT_INCOME_REPORT_{0}", dateTimePicker1.Value.ToString("MMM_yyyy"));
            SaveFileDialog sf = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = "Portable Document File (.pdf) |*.pdf|Excel Workbook (.xlsx) |*.xlsx",
                Title = "Export Report as",
                FilterIndex = 1
            };

            if (sf.ShowDialog() == DialogResult.OK)
            {
                string savePath = Path.GetDirectoryName(sf.FileName);
                string fileName = Path.GetFileName(sf.FileName);
                string saveFile = Path.Combine(savePath, fileName);

                string imagePath = Path.Combine(savePath, "Chart1.png");
                chart1.SaveImage(imagePath, ChartImageFormat.Png);

                var exportTable = new DataTable();
                exportTable = ListViewToTable(exportTable, listView1);

                switch (sf.FilterIndex)
                {
                    case 1:
                        {
                            try
                            {
                                Document pdfDocument = new Document();

                                //Bind the PDF document to the FileStream using an iTextSharp PdfWriter
                                PdfWriter.GetInstance(pdfDocument, new FileStream(saveFile, FileMode.Create));

                                //Open the document for writing
                                pdfDocument.Open();

                                //Add title for the document
                                pdfDocument.AddTitle(label1.Text);

                                //Add title in the document
                                Paragraph p = new Paragraph(label1.Text, FontFactory.GetFont("Verdana", 20, 1));
                                p.Alignment = Element.ALIGN_CENTER;
                                p.SpacingAfter = 30;
                                pdfDocument.Add(p);

                                //Create a table with 3 columns (Type Name, Income, Percentage)
                                PdfPTable t = new PdfPTable(3);

                                //Add Table Header
                                foreach (ColumnHeader lvch in listView1.Columns)
                                {
                                    t.DefaultCell.BackgroundColor = new BaseColor(211, 211, 211); //Set Header Row Colour
                                    t.AddCell(lvch.Text);
                                }

                                //Add cells for each row. 
                                //Cells are added starting at the top left of the table working left to right first, then down
                                int rowCount = 0;
                                int maxRow = listView1.Items.Count;
                                foreach (ListViewItem lvi in listView1.Items)
                                {
                                    for (int i = 0; i < lvi.SubItems.Count; i++)
                                    {
                                        //Reset the background colour for all cells to White colour
                                        t.DefaultCell.BackgroundColor = new BaseColor(255, 255, 255);
                                        if (rowCount == maxRow - 1) //Final Row (Total) was set to Grey Colour 
                                        {
                                            t.DefaultCell.BackgroundColor = new BaseColor(230, 230, 230);
                                        }
                                        t.AddCell(lvi.SubItems[i].Text);
                                    }
                                    rowCount++;
                                }
                                t.SpacingAfter = 30;

                                //Add the table to the document
                                pdfDocument.Add(t);

                                //Add the image into the document
                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);
                                img.Alignment = Element.ALIGN_CENTER;
                                pdfDocument.Add(img);

                                //Close the document
                                pdfDocument.Close();
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                                return;
                            }

                            break;
                        }
                    case 2:
                        {
                            try
                            {
                                var workbook = new XLWorkbook();
                                var ws = workbook.AddWorksheet("Report");
                                ws.Cell(1, 1).Value = label1.Text;
                                ws.Cell(2, 1).InsertTable(exportTable);
                                ws.AddPicture(imagePath)
                                    .MoveTo(ws.Cell(ws.LastRowUsed().RowNumber() + 2, 1))
                                    .Scale(1.0);

                                workbook.SaveAs(saveFile);
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                                return;
                            }

                            break;
                        }
                }
                string escapedSaveFile = saveFile.Replace(@"\", @"\\");

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Export",
                    Item = "Sales Report",
                    Description = "Total Income Report for " + dateTimePicker1.Value.ToString("MMM yyyy") + 
                    "; Path: " + escapedSaveFile
                };
                log.Insert();

                MessageBox.Show("Export file saved sucessfully.", "Export File Saved", MessageBoxButtons.OK);
                File.Delete(imagePath);
            }
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for Monthly Income
        {
            if (e.Column != sortColumn)
            {
                sortColumn = e.Column;
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

        //Monthly Most Frequent Usage Module
        private void LoadMonthUseFrequency()
        {
            Controls.Remove(panel1);
            Controls.Add(panel6);

            listView2.Items.Clear();
            foreach (var series in chart2.Series)
            {
                series.Points.Clear();
            }
            chart2.Titles.Clear();

            int totalFrequency = 0;
            var itemDic = new Dictionary<string, int>();

            foreach (Reporting t in _items)
            {
                ListViewItem lvi = new ListViewItem(t.TypeName);
                lvi.SubItems.Add(t.Frequency.ToString());
                listView2.Items.Add(lvi);
                itemDic.Add(t.TypeName, t.Frequency);
                totalFrequency += t.Frequency;
            }

            ListViewItem lvit = new ListViewItem("Total");
            lvit.SubItems.Add(totalFrequency.ToString());
            listView2.Items.Add(lvit);

            List<decimal> percentage = new List<decimal>();
            for (int i = 0; i < _items.Count(); i++)
            {
                percentage.Add(Convert.ToDecimal(_items[i].Frequency) / Convert.ToDecimal(totalFrequency));
            }
            percentage.Add(1);

            int j = 0;
            foreach (ListViewItem lvItem in listView2.Items)
            {
                lvItem.SubItems.Add(percentage[j].ToString("#0.00%"));
                j++;
            }

            listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            string[] x = (from selected in itemDic
                          select selected.Key).ToArray();
            int[] y = (from selected in itemDic
                       select selected.Value).ToArray();
            string[] titles = { label2.Text };

            chart2.Titles.Add(titles[0]);
            chart2.Titles[0].Text = label2.Text;

            chart2.Series[0].ChartType = SeriesChartType.Pie;
            chart2.Series[0].Points.DataBindXY(x, y);
            chart2.Legends[0].Enabled = true;
            foreach (DataPoint p in chart2.Series[0].Points)
            {
                p.Label = "#PERCENT";
                p.LegendText = "#VALX";
            }

        }

        private void Button7_Click(object sender, EventArgs e) //Back button in Monthly MFU
        {
            Controls.Remove(panel6);
            Controls.Add(panel1);
        }

        private void Button8_Click(object sender, EventArgs e) //Export button for MFU
        {
            string defaultFileName = String.Format("EXPORT_FREQUENCY_REPORT_{0}", dateTimePicker1.Value.ToString("MMM_yyyy"));
            SaveFileDialog sf = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = "Portable Document File (.pdf) |*.pdf|Excel Workbook (.xlsx) |*.xlsx",
                Title = "Export Report as",
                FilterIndex = 1
            };

            if (sf.ShowDialog() == DialogResult.OK)
            {
                string savePath = Path.GetDirectoryName(sf.FileName);
                string fileName = Path.GetFileName(sf.FileName);
                string saveFile = Path.Combine(savePath, fileName);

                string imagePath = Path.Combine(savePath, "Chart2.png");
                chart2.SaveImage(imagePath, ChartImageFormat.Png);

                var exportTable = new DataTable();
                exportTable = ListViewToTable(exportTable, listView2);

                switch (sf.FilterIndex)
                {
                    case 1:
                        {
                            try
                            {
                                Document pdfDocument = new Document();
                                PdfWriter.GetInstance(pdfDocument, new FileStream(saveFile, FileMode.Create));
                                pdfDocument.Open();

                                pdfDocument.AddTitle(label2.Text);

                                Paragraph p = new Paragraph(label2.Text, FontFactory.GetFont("Verdana", 20, 1));
                                p.Alignment = Element.ALIGN_CENTER;
                                p.SpacingAfter = 30;
                                pdfDocument.Add(p);

                                PdfPTable t = new PdfPTable(3);
                                foreach (ColumnHeader lvch in listView2.Columns)
                                {
                                    t.DefaultCell.BackgroundColor = new BaseColor(211, 211, 211); //Set Header Row Colour
                                    t.AddCell(lvch.Text);
                                }

                                int rowCount = 0;
                                int maxRow = listView2.Items.Count;
                                foreach (ListViewItem lvi in listView2.Items)
                                {
                                    for (int i = 0; i < lvi.SubItems.Count; i++)
                                    {
                                        t.DefaultCell.BackgroundColor = new BaseColor(255, 255, 255);
                                        if (rowCount == maxRow - 1) 
                                        {
                                            t.DefaultCell.BackgroundColor = new BaseColor(230, 230, 230);
                                        }
                                        t.AddCell(lvi.SubItems[i].Text);
                                    }
                                    rowCount++;
                                }
                                t.SpacingAfter = 30;

                                pdfDocument.Add(t);

                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);
                                img.Alignment = Element.ALIGN_CENTER;
                                pdfDocument.Add(img);

                                pdfDocument.Close();
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                                return;
                            }

                            break;
                        }

                    case 2:
                        {
                            try
                            {
                                var workbook = new XLWorkbook();
                                var ws = workbook.AddWorksheet("Report");
                                ws.Cell(1, 1).Value = label2.Text;
                                ws.Cell(2, 1).InsertTable(exportTable);
                                ws.AddPicture(imagePath)
                                    .MoveTo(ws.Cell(ws.LastRowUsed().RowNumber() + 2, 1))
                                    .Scale(1.0);

                                workbook.SaveAs(saveFile);
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                                return;
                            }

                            break;
                        }
                }
                string escapedSaveFile = saveFile.Replace(@"\", @"\\");

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Export",
                    Item = "Sales Report",
                    Description = "Total Usage Frequency Report for " + dateTimePicker1.Value.ToString("MMM yyyy") + 
                    "; Path: " + escapedSaveFile
                };
                log.Insert();

                MessageBox.Show("Export file saved sucessfully.", "Export File Saved", MessageBoxButtons.OK);
                File.Delete(imagePath);
            }
        }

        private void ListView2_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for MFU
        {
            if (e.Column != sortColumn)
            {
                sortColumn = e.Column;
                listView2.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView2.Sorting == SortOrder.Ascending)
                    listView2.Sorting = SortOrder.Descending;
                else
                    listView2.Sorting = SortOrder.Ascending;
            }

            listView1.Sort();
            this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, listView2.Sorting);
        }

        //Monthly Most Day Usage Module
        private void LoadMonthUseDays()
        {
            Controls.Remove(panel1);
            Controls.Add(panel7);

            listView3.Items.Clear();
            foreach (var series in chart3.Series)
            {
                series.Points.Clear();
            }
            chart3.Titles.Clear();

            int totalDays = 0;
            var itemDic = new Dictionary<string, decimal>();

            foreach (Reporting t in _items)
            {
                ListViewItem lvi = new ListViewItem(t.TypeName);
                lvi.SubItems.Add(t.DaysUsed.ToString());
                listView3.Items.Add(lvi);
                itemDic.Add(t.TypeName, t.DaysUsed);
                totalDays += t.DaysUsed;
            }

            ListViewItem lvit = new ListViewItem("Total");
            lvit.SubItems.Add(totalDays.ToString());
            listView3.Items.Add(lvit);

            List<decimal> percentage = new List<decimal>();
            for (int i = 0; i < _items.Count(); i++)
            {
                percentage.Add(Convert.ToDecimal(_items[i].DaysUsed) / Convert.ToDecimal(totalDays));
            }
            percentage.Add(1);

            int j = 0;
            foreach (ListViewItem lvItem in listView3.Items)
            {
                lvItem.SubItems.Add(percentage[j].ToString("#0.00%"));
                j++;
            }

            listView3.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView3.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            string[] x = (from selected in itemDic
                          select selected.Key).ToArray();
            decimal[] y = (from selected in itemDic
                           select selected.Value).ToArray();
            string[] titles = { label3.Text };

            chart3.Titles.Add(titles[0]);
            chart3.Titles[0].Text = label3.Text;

            chart3.Series[0].ChartType = SeriesChartType.Pie;
            chart3.Series[0].Points.DataBindXY(x, y);
            chart3.Legends[0].Enabled = true;
            foreach (DataPoint p in chart3.Series[0].Points)
            {
                p.Label = "#PERCENT";
                p.LegendText = "#VALX";
            }
        }

        private void Button10_Click(object sender, EventArgs e) //Back button in Monthly MDU
        {
            Controls.Remove(panel7);
            Controls.Add(panel1);
        }

        private void Button11_Click(object sender, EventArgs e) //Export button in MDU
        {
            string defaultFileName = String.Format("EXPORT_DAYS_REPORT_{0}", dateTimePicker1.Value.ToString("MMM_yyyy"));
            SaveFileDialog sf = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = "Portable Document File (.pdf) |*.pdf|Excel Workbook (.xlsx) |*.xlsx",
                Title = "Export Report as",
                FilterIndex = 1
            };
            if (sf.ShowDialog() == DialogResult.OK)
            {
                string savePath = Path.GetDirectoryName(sf.FileName);
                string fileName = Path.GetFileName(sf.FileName);
                string saveFile = Path.Combine(savePath, fileName);

                string imagePath = Path.Combine(savePath, "Chart3.png");
                chart3.SaveImage(imagePath, ChartImageFormat.Png);

                var exportTable = new DataTable();
                exportTable = ListViewToTable(exportTable, listView3);

                switch (sf.FilterIndex)
                {
                    case 1:
                        {
                            try
                            {
                                Document pdfDocument = new Document();

                                PdfWriter.GetInstance(pdfDocument, new FileStream(saveFile, FileMode.Create));
                                pdfDocument.Open();

                                pdfDocument.AddTitle(label3.Text);
                                Paragraph p = new Paragraph(label3.Text, FontFactory.GetFont("Verdana", 20, 1));
                                p.Alignment = Element.ALIGN_CENTER;
                                p.SpacingAfter = 30;
                                pdfDocument.Add(p);

                                PdfPTable t = new PdfPTable(3);

                                foreach (ColumnHeader lvch in listView3.Columns)
                                {
                                    t.DefaultCell.BackgroundColor = new BaseColor(211, 211, 211); //Set Header Row Colour
                                    t.AddCell(lvch.Text);
                                }

                                int rowCount = 0;
                                int maxRow = listView3.Items.Count;
                                foreach (ListViewItem lvi in listView3.Items)
                                {
                                    for (int i = 0; i < lvi.SubItems.Count; i++)
                                    {
                                        t.DefaultCell.BackgroundColor = new BaseColor(255, 255, 255);
                                        if (rowCount == maxRow - 1)
                                        {
                                            t.DefaultCell.BackgroundColor = new BaseColor(230, 230, 230);
                                        }
                                        t.AddCell(lvi.SubItems[i].Text);
                                    }
                                    rowCount++;
                                }
                                t.SpacingAfter = 30;

                                pdfDocument.Add(t);

                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);
                                img.Alignment = Element.ALIGN_CENTER;
                                pdfDocument.Add(img);

                                pdfDocument.Close();
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                                return;
                            }

                            break;
                        }

                    case 2:
                        {
                            try
                            {
                                var workbook = new XLWorkbook();
                                var ws = workbook.AddWorksheet("Report");
                                ws.Cell(1, 1).Value = label3.Text;
                                ws.Cell(2, 1).InsertTable(exportTable);
                                ws.AddPicture(imagePath)
                                    .MoveTo(ws.Cell(ws.LastRowUsed().RowNumber() + 2, 1))
                                    .Scale(1.0);

                                workbook.SaveAs(saveFile);
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                                return;
                            }

                            break;
                        }
                }
                string escapedSaveFile = saveFile.Replace(@"\", @"\\");

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Export",
                    Item = "Sales Report",
                    Description = "Total Usage Days Report for " + dateTimePicker1.Value.ToString("MMM yyyy") + 
                    "; Path: " + escapedSaveFile
                };
                log.Insert();

                MessageBox.Show("Export file saved sucessfully.", "Export File Saved", MessageBoxButtons.OK);
                File.Delete(imagePath);
            }
        }

        private void ListView3_ColumnClick(object sender, ColumnClickEventArgs e) //Sorting for MDU
        {
            if (e.Column != sortColumn)
            {
                sortColumn = e.Column;
                listView3.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView3.Sorting == SortOrder.Ascending)
                    listView3.Sorting = SortOrder.Descending;
                else
                    listView3.Sorting = SortOrder.Ascending;
            }

            listView1.Sort();
            this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, listView3.Sorting);
        }

        //Other functions 

        private static DataTable ListViewToTable(DataTable table, ListView lvw)
        {
            table.Clear();
            var columns = lvw.Columns.Count;

            foreach (ColumnHeader column in lvw.Columns)
                table.Columns.Add(column.Text);

            foreach (ListViewItem item in lvw.Items)
            {
                var cells = new object[columns];
                for (var i = 0; i < columns; i++)
                    cells[i] = item.SubItems[i].Text;
                table.Rows.Add(cells);
            }

            return table;
        }
    }
}
