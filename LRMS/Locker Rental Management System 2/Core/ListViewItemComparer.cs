using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace LockerRentalManagementSystem
{
    class ListViewItemComparer : IComparer
    {
        private int col;
        private SortOrder order;
        public ListViewItemComparer()
        {
            col = 0;
            order = SortOrder.Ascending;
        }
        public ListViewItemComparer(int column, SortOrder order)
        {
            col = column;
            this.order = order;
        }
        public int Compare(object x, object y)
        {
            if (!(x is ListViewItem))
                return 0;
            if (!(y is ListViewItem))
                return 0;

            ListViewItem l1 = (ListViewItem)x;
            ListViewItem l2 = (ListViewItem)y;

            if (l1.ListView.Columns[col].Tag == null)
            {
                l1.ListView.Columns[col].Tag = "Text";
            }

            if (l1.ListView.Columns[col].Tag.ToString() == "Int")
            {
                string string1 = l1.SubItems[col].Text;
                string string2 = l2.SubItems[col].Text;
                if (string.IsNullOrWhiteSpace(string1))
                { string1 = "0"; }
                if (string.IsNullOrWhiteSpace(string2))
                { string2 = "0"; }

                int int1 = Convert.ToInt32(string1);
                int int2 = Convert.ToInt32(string2);
                if (order == SortOrder.Ascending)
                    return int1.CompareTo(int2);
                else
                    return int2.CompareTo(int1);
            }
            else if (l1.ListView.Columns[col].Tag.ToString() == "Double")
            {
                string string1 = l1.SubItems[col].Text;
                string string2 = l2.SubItems[col].Text;

                if (string.IsNullOrWhiteSpace(string1))
                { string1 = "0"; }
                if (string.IsNullOrWhiteSpace(string2))
                { string2 = "0"; }

                double double1 = Convert.ToDouble(string1);
                double double2 = Convert.ToDouble(string2);

                if (order == SortOrder.Ascending)
                    return double1.CompareTo(double2);
                else
                    return double2.CompareTo(double1);
            }
            else if(l1.ListView.Columns[col].Tag.ToString() == "Date" )
            {
                if (!string.IsNullOrWhiteSpace(l1.SubItems[col].Text) && 
                    !string.IsNullOrWhiteSpace(l2.SubItems[col].Text))
                {
                    DateTime date1 = DateTime.Parse(l1.SubItems[col].Text);
                    DateTime date2 = DateTime.Parse(l2.SubItems[col].Text);
                    if (order == SortOrder.Ascending)
                        return date1.CompareTo(date2);
                    else
                        return date2.CompareTo(date1);
                }
                else
                    return 0;
            }
            else if (l1.ListView.Columns[col].Tag.ToString() == "Percentage")
            {
                string string1 = l1.SubItems[col].Text;
                string string2 = l2.SubItems[col].Text;

                if (string.IsNullOrWhiteSpace(string1))
                { string1 = "0"; }
                if (string.IsNullOrWhiteSpace(string2))
                { string2 = "0"; }

                //Remove the last character ('%') from the string
                StringBuilder sb = new StringBuilder(string1);
                sb.Remove(string1.Length - 1, 1);
                string1 = sb.ToString();

                sb = new StringBuilder(string2);
                sb.Remove(string2.Length - 1, 1);
                string2 = sb.ToString();

                double double1 = Convert.ToDouble(string1);
                double double2 = Convert.ToDouble(string2);

                if (order == SortOrder.Ascending)
                    return double1.CompareTo(double2);
                else
                    return double2.CompareTo(double1);
            }
            else
            {
                string string1 = l1.SubItems[col].Text;
                string string2 = l2.SubItems[col].Text;
                if (order == SortOrder.Ascending)
                    return string1.CompareTo(string2);
                else
                    return string2.CompareTo(string1);
            }
        }
    }
}
