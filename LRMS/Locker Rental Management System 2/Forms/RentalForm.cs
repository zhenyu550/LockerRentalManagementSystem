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
    public partial class RentalForm : Form
    {
        //Add Rental & Payment Part

        private List<Customer> _customerList;
        private List<Locker> _lockerList;
        private List<Cabinet> _cabinetList;
        private List<Type> _typeList;

        private bool _setCustomer = false;
        private bool _setLocker = false;
        private bool _insertComplete = false;

        public bool InsertComplete { get { return _insertComplete; } }

        public RentalForm()
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel1);
            textBox1.Text = Rental.CurrentID();
        }

        private void Button1_Click(object sender, EventArgs e) //Select Customer button
        {
            var SelectCustomerForm = new SelectCustomerForm();
            SelectCustomerForm.ShowDialog();
            int customerId = SelectCustomerForm.CustomerId;
            _customerList = Customer.Where(String.Format("id = {0}", customerId), 0, 1);

            if (!_customerList.Any())
                return;

            textBox4.Text = _customerList[0].Name;
            textBox5.Text = _customerList[0].Ic;
            _setCustomer = true;
        }

        private void Button2_Click(object sender, EventArgs e) //Select Locker button
        {
            var SelectLockerForm = new SelectLockerForm();
            SelectLockerForm.ShowDialog();

            int lockerId = SelectLockerForm.LockerID;
            int cabinetId = SelectLockerForm.CabinetID;
            int typeId = SelectLockerForm.TypeID;

            _typeList = Type.Where(String.Format("id = {0}", typeId), 0, 1);
            _cabinetList = Cabinet.Where(String.Format("id = {0}", cabinetId), 0, 1);
            _lockerList = Locker.Where(String.Format("id = {0}", lockerId), 0, 1);

            if(!_typeList.Any() || !_cabinetList.Any() || !_lockerList.Any())
                return;

            textBox6.Text = _lockerList[0].Code;        //Code
            textBox7.Text = _cabinetList[0].Code;       //Cabinet
            textBox8.Text = _typeList[0].Name;          //Size
            numericUpDown6.Value = _typeList[0].Rate;   //Rate
            _setLocker = true;
        }

        private void DateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker2.Value.Date < dateTimePicker1.Value.Date)
            {
                MessageBox.Show("Input Error: Invalid Date Range" + Environment.NewLine +
                    "End date must be later than today date.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dateTimePicker2.Value = DateTime.Now;
                return;
            }
            else
            {
                TimeSpan timeSpan = dateTimePicker2.Value.Date.Subtract(dateTimePicker1.Value.Date);
                numericUpDown1.Value = Convert.ToDecimal(timeSpan.Days);
            }
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker2.Value = dateTimePicker1.Value.AddDays(Convert.ToDouble(numericUpDown1.Value));
        }

        private void Button3_Click(object sender, EventArgs e) //Cancel button
        {
            this.Close();
        }

        private void Button4_Click(object sender, EventArgs e) //Next button
        {
            if (!_setCustomer || !_setLocker)
            {
                if (!_setCustomer && !_setLocker)
                {
                    MessageBox.Show("Error: Empty Customer & Locker" + Environment.NewLine +
                        "Please select a customer and a locker for the rental process.", "Empty Customer & Locker",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (!_setCustomer)
                {
                    MessageBox.Show("Error: Empty Customer" + Environment.NewLine +
                        "Please select a customer for the rental process.", "Empty Customer & Locker",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Error: Empty Locker" + Environment.NewLine +
                        "Please select a locker for the rental process.", "Empty Locker",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            if (numericUpDown1.Value <=0)
            {
                MessageBox.Show("Input Error: Invalid Duration." + Environment.NewLine + 
                    "Duration must be larger than 0.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            Controls.Clear();
            Controls.Add(panel2);
            textBox9.Text = Transaction.CurrentID();
            button7.Hide();

            //Customer
            textBox10.Text = _customerList[0].Name;
            textBox15.Text = _customerList[0].Ic;

            //Locker
            textBox11.Text = _lockerList[0].Code;
            textBox12.Text = _cabinetList[0].Code;
            textBox13.Text = _typeList[0].Name;
            numericUpDown7.Value = _typeList[0].Rate;

            //Rental
            textBox3.Text = textBox1.Text;
            textBox16.Text = dateTimePicker1.Value.ToString("dd-MM-yyyy");
            textBox17.Text = dateTimePicker2.Value.ToString("dd-MM-yyyy");
            numericUpDown2.Value = numericUpDown1.Value;

            //Calculate Payment
            numericUpDown3.Value = _typeList[0].Rate * numericUpDown2.Value;
        }

        private void Button5_Click(object sender, EventArgs e) //Back button
        {
            Controls.Clear();
            Controls.Add(panel1);
            
            //Customer
            textBox4.Text = _customerList[0].Name;
            textBox5.Text = _customerList[0].Ic;

            //Locker
            textBox6.Text = _lockerList[0].Code;        //Code
            textBox7.Text = _cabinetList[0].Code;       //Cabinet
            textBox8.Text = _typeList[0].Name;          //Size
            numericUpDown6.Value = _typeList[0].Rate;   //Rate

            //Rental
            textBox1.Text = textBox3.Text;
            dateTimePicker1.Value = DateTime.Parse(textBox16.Text);
            dateTimePicker2.Value = DateTime.Parse(textBox17.Text);
        }

        private void Button6_Click(object sender, EventArgs e) //Confirm Payment button
        {
            if (numericUpDown4.Value < numericUpDown3.Value)
            {
                MessageBox.Show("Input Error: Insufficient Payment." + Environment.NewLine + 
                    "Payment amount must be equal or higher than total price.","Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            numericUpDown5.Value = numericUpDown4.Value - numericUpDown3.Value;

            button5.Hide();
            button6.Hide();
            button7.Show();

            var rental = new Rental
            {
                StartDate = dateTimePicker1.Value,
                Duration = Convert.ToInt32(numericUpDown2.Value),
                CustomerID = _customerList[0].Id,
                LockerID = _lockerList[0].Id,
            };
            rental.Save();

            //Insert access_log for rental
            var log = new AccessLog
            {
                User = Login.Username,
                Action = "Add",
                Item = "Rental",
                ItemId = textBox9.Text
            };
            log.Insert();

            _insertComplete = true;

            //Set the locker is occupied, and insert into accesslog
            _lockerList[0].Occupied(); 
            log.User = "system";
            log.Action = "Update";
            log.Item = "Locker";
            log.ItemId = rental.LockerID.ToString();
            log.Description = "Code: " + _lockerList[0].Code + "; Status: Available to Occupied";
            log.Insert();

            //Check if the cabinet full. If yes, set cabinet to full, and insert into access log.
            var locker = new Locker();
            int emptyLockerNo = locker.Count(String.Format("cabinet_id = {0} AND status = 'Available'", 
                _cabinetList[0].Id));
            if (emptyLockerNo <= 0)
            {
                _cabinetList[0].Full();
                log.User = "system";
                log.Action = "Update";
                log.Item = "Cabinet";
                log.ItemId = _cabinetList[0].Id.ToString();
                log.Description = "Code: " + _cabinetList[0].Code + "; Status: Available to Full";
                log.Insert();
            }

            //Insert rental details into Transaction
            var transaction = new Transaction
            {
                RentalID = Convert.ToInt32(textBox3.Text),
                CustomerID = rental.CustomerID,
                LockerID = rental.LockerID,
                TypeName = _typeList[0].Name,
                TypeRate = _typeList[0].Rate,
                StartDate = rental.StartDate,
                Duration = rental.Duration,
            };
            transaction.Save();
            log = new AccessLog
            {
                User = "system",
                Action = "Add",
                Item = "Transaction",
                ItemId = textBox9.Text
            };
            log.Insert();
        }

        private void Button7_Click(object sender, EventArgs e) //OK button
        {
            this.Close();
        }

        //View Rental Details Part
        Rental selectedRental = new Rental();

        public RentalForm(int id)
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel3);
            selectedRental = selectedRental.Get(id);
            LoadRentalData(selectedRental);
        }

        private void LoadRentalData(Rental item)
        {
            //Rental
            textBox2.Text = item.Id.ToString();
            textBox14.Text = item.StartDate.ToString("dd-MM-yyyy");
            DateTime endDate = item.StartDate.Date.AddDays(item.Duration);
            textBox18.Text = endDate.ToString("dd-MM-yyyy");
            textBox19.Text = item.Duration.ToString();
            TimeSpan timeSpan = endDate.Date.Subtract(DateTime.Now.Date);
            int daysLeft = Convert.ToInt32(timeSpan.Days);
            textBox20.Text = daysLeft.ToString();
            if(daysLeft < 0)
                textBox21.Text = "Overdue";
            else
                textBox21.Text = "Normal";

            //Customer
            var customer = new Customer();
            customer = customer.Get(item.CustomerID);
            textBox22.Text = item.CustomerID.ToString();
            textBox23.Text = customer.Name;
            textBox24.Text = customer.Ic;

            //Locker
            var locker = new Locker();
            var cabinet = new Cabinet();
            var type = new Type();

            locker = locker.Get(item.LockerID);
            cabinet = cabinet.Get(locker.CabinetID);
            type = type.Get(cabinet.TypeID);

            textBox25.Text = item.LockerID.ToString();
            textBox26.Text = locker.Code;
            textBox27.Text = cabinet.Code;
            textBox28.Text = type.Name;
            textBox29.Text = type.Rate.ToString("0.00");

            //Payment
            decimal totalPrice = item.Duration * type.Rate;
            textBox30.Text = totalPrice.ToString("0.00");
        }

        private void Button8_Click(object sender, EventArgs e) //OK button
        {
            this.Close();
        }

        private void Button9_Click(object sender, EventArgs e) //Change Locker button
        {
            var result = MessageBox.Show("Do you want to change the locker for this rental?", "Change Locker", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {   //Check if the rental overdue. If yes, show error message and return.
                var endDate = selectedRental.StartDate.AddDays(selectedRental.Duration);
                TimeSpan timeSpan = endDate.Date.Subtract(DateTime.Now.Date);
                int daysLeft = Convert.ToInt32(timeSpan.Days);
                if (daysLeft < 0)
                {
                    MessageBox.Show("Access Error: Rental Overdued." + Environment.NewLine +
                        "You cannot change details for an overdued rental.", "Access Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Assign the old rental data to a temp variable
                int oldLockerId = selectedRental.LockerID;
                var oldLocker = new Locker();
                oldLocker = oldLocker.Get(oldLockerId);

                //Open Select Locker Form
                var ChangeLockerForm = new SelectLockerForm(selectedRental.LockerID);
                ChangeLockerForm.ShowDialog();

                //If cancel select, return.
                if (!ChangeLockerForm.LockerSelected)
                    return;

                //Get the new selected type, cabinet and locker for the selected locker
                _typeList = Type.Where(String.Format("id = {0}", ChangeLockerForm.TypeID), 0, 1);
                _cabinetList = Cabinet.Where(String.Format("id = {0}", ChangeLockerForm.CabinetID), 0, 1);
                _lockerList = Locker.Where(String.Format("id = {0}", ChangeLockerForm.LockerID), 0, 1);

                //Assign the new locker into rental, and save access log
                selectedRental.LockerID = ChangeLockerForm.LockerID;
                selectedRental.Save();
                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Update",
                    Item = "Rental",
                    ItemId = selectedRental.Id.ToString(),
                    Description = "Locker: " + oldLocker.Code + " to " + _lockerList[0].Code
                };
                log.Insert();

                //Release the old locker (status = available) and insert into access log
                oldLocker.Reset();
                log.User = "system";
                log.Action = "Update";
                log.Item = "Locker";
                log.ItemId = oldLocker.Id.ToString();
                log.Description = "Code: " + oldLocker.Code + "; Status: Occupied to Available";
                log.Insert();

                //Check if the old cabinet is full. If yes, set the cabinet to available.
                var oldCabinet = new Cabinet();
                oldCabinet = oldCabinet.Get(oldLocker.CabinetID);
                if (oldCabinet.IsFull())
                {
                    oldCabinet.Restore();
                    log.User = "system";
                    log.Action = "Update";
                    log.Item = "Cabinet";
                    log.ItemId = oldLocker.CabinetID.ToString();
                    log.Description = "Code: " + oldCabinet.Code + "; Status: Full to Available";
                    log.Insert();
                }

                //Set the new locker is occupied, and insert into access log
                _lockerList[0].Occupied();
                log.User = "system";
                log.Action = "Update";
                log.Item = "Locker";
                log.ItemId = selectedRental.LockerID.ToString();
                log.Description = "Code: " + _lockerList[0].Code + "; Status: Available to Occupied";
                log.Insert();

                //Check if the new cabinet full. If yes, set cabinet to full, and insert into access log.
                var locker = new Locker();
                int EmptyLockerNo = locker.Count(String.Format("cabinet_id = {0} AND status = 'Available'",
                    _cabinetList[0].Id));
                if (EmptyLockerNo <= 0)
                {
                    _cabinetList[0].Full();
                    log.User = "system";
                    log.Action = "Update";
                    log.Item = "Cabinet";
                    log.ItemId = _cabinetList[0].Id.ToString();
                    log.Description = "Code: " + _cabinetList[0].Code + "; Status: Available to Full";
                    log.Insert();
                }

                //Change the details in transaction and save in access log
                var selectedTrans = Transaction.Where(String.Format("rental_id = {0}", selectedRental.Id), 0, 1);
                selectedTrans[0].LockerID = _lockerList[0].Id;
                selectedTrans[0].ChangeLocker();
                log.User = "system";
                log.Action = "Update";
                log.Item = "Transaction";
                log.ItemId = selectedTrans[0].Id.ToString();
                log.Description = "Locker: " + oldLocker.Code + " to " + _lockerList[0].Code;
                log.Insert();

                //Change the locker details in the View Rental Details 
                textBox25.Text = _lockerList[0].Id.ToString();
                textBox26.Text = _lockerList[0].Code;
                textBox27.Text = _cabinetList[0].Code;
                textBox28.Text = _typeList[0].Name;
                textBox29.Text = _typeList[0].Rate.ToString("0.00");
            }
        }

        //End Rental Part
        private decimal _totalFine = 0;
        private bool _keyLostFineAdded = false;
        private bool _lockerDamagedFineAdded = false;
        private int _overdueDays;
        List<RentalSettings> rentalSettingsList;

        public RentalForm(int id, bool rentalEnd)
        {
            InitializeComponent();
            Controls.Remove(tabControl1);
            Controls.Add(panel4);
            button12.Hide();
            var item = new Rental();
            item = item.Get(id);
            LoadRentalData(item, rentalEnd);
        }

        private void LoadRentalData(Rental item, bool rentalEnd)
        {
            rentalSettingsList = RentalSettings.All(0, 10);
            selectedRental = item;

            //Rental
            textBox31.Text = item.Id.ToString();
            textBox32.Text = item.StartDate.ToString("dd-MM-yyyy");
            var endDate = item.StartDate.AddDays(item.Duration);
            textBox33.Text = endDate.ToString("dd-MM-yyyy");
            textBox34.Text = item.Duration.ToString();

            //Customer
            var customer = new Customer();
            customer = customer.Get(item.CustomerID);
            textBox35.Text = customer.Id.ToString();
            textBox36.Text = customer.Name;
            textBox37.Text = customer.Ic;

            //Locker
            var locker = new Locker();
            var cabinet = new Cabinet();
            var type = new Type();

            locker = locker.Get(item.LockerID);
            cabinet = cabinet.Get(locker.CabinetID);
            type = type.Get(cabinet.TypeID);

            textBox38.Text = locker.Id.ToString();
            textBox39.Text = locker.Code;
            textBox40.Text = cabinet.Code;
            textBox41.Text = type.Name;

            //Additional Payment
            TimeSpan timeSpan = endDate.Date.Subtract(DateTime.Now.Date);
            int daysLeft = Convert.ToInt32(timeSpan.Days);
            if (daysLeft >= 0)
                _overdueDays = 0;
            else
            {
                _overdueDays = -daysLeft;
                _totalFine = _totalFine + rentalSettingsList[1].SettingValue + (type.Rate * _overdueDays);
                numericUpDown8.Value = _totalFine;
                checkBox1.Checked = true;   //Assign Overdue Check Box as Ticked
            }
            textBox43.Text = _overdueDays.ToString();
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e) //Key Lost Check Box
        {
            if (_keyLostFineAdded)
            {
                _totalFine = _totalFine - rentalSettingsList[2].SettingValue;
                _keyLostFineAdded = false;
            }
            else
            {
                _totalFine = _totalFine + rentalSettingsList[2].SettingValue;
                _keyLostFineAdded = true;
            }
            numericUpDown8.Value = _totalFine;
        }

        private void CheckBox3_CheckedChanged(object sender, EventArgs e) //Locker Damaged Check Box
        {
            if (_lockerDamagedFineAdded)
            {
                _totalFine = _totalFine - rentalSettingsList[3].SettingValue;
                _lockerDamagedFineAdded = false;
            }
            else
            {
                _totalFine = _totalFine + rentalSettingsList[3].SettingValue;
                _lockerDamagedFineAdded = true;
            }
            numericUpDown8.Value = _totalFine;
        }

        private void Button11_Click(object sender, EventArgs e) //Cancel button in end rental
        {
            this.Close();
        }

        private void Button12_Click(object sender, EventArgs e) //OK button
        {
            this.Close();
        }

        private void Button10_Click(object sender, EventArgs e) //Next button
        {
            if (numericUpDown9.Value < numericUpDown8.Value)
            {
                MessageBox.Show("Input Error: Insufficient Payment." + Environment.NewLine +
                    "Payment amount must be equal or higher than total price.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            numericUpDown10.Value = numericUpDown9.Value - numericUpDown8.Value;

            button11.Hide();
            button10.Hide();
            button12.Show();

            //Delete Rental Log
            var log = new AccessLog()
            {
                User = Login.Username,
                Action = "End",
                Item = "Rental",
                ItemId = selectedRental.Id.ToString()
            };
            log.Insert();
            log = new AccessLog()
            {
                User = "system",
                Action = "Delete from database",
                Item = "Rental",
                ItemId = selectedRental.Id.ToString()
            };
            log.Insert();

            var transactionItem = Transaction.Where(String.Format("rental_id = {0}", selectedRental.Id), 0, 1);
            transactionItem[0].ReturnDate = DateTime.Now.Date;
            transactionItem[0].OverdueTime = _overdueDays;
            transactionItem[0].Fine = numericUpDown8.Value;
            transactionItem[0].Save();

            log = new AccessLog()
            {
                User = "system",
                Action = "Update",
                Item = "Transaction",
                ItemId = transactionItem[0].Id.ToString(),
                Description = "Return Date: " + transactionItem[0].ReturnDate.ToString("dd-MM-yyyy") +  
                "; Overdue Time: " + _overdueDays +  " day; Fine: " + numericUpDown8.Value
            };
            log.Insert();

            //Insert transaction return status
            RentalStatus transReturnStatus = new RentalStatus();
            if (_keyLostFineAdded)
            {
                transReturnStatus.TransactionId = transactionItem[0].Id;
                transReturnStatus.StatusId = 3;
                transReturnStatus.Insert();
                log = new AccessLog()
                {
                    User = "system",
                    Action = "Add",
                    Item = "Rental Status",
                    ItemId = transReturnStatus.TransactionId + ", " + transReturnStatus.StatusId,
                    Description = "Return Status: Key Lost"
                };
                log.Insert();
            }
            if (_lockerDamagedFineAdded)
            {
                transReturnStatus.TransactionId = transactionItem[0].Id;
                transReturnStatus.StatusId = 4;
                transReturnStatus.Insert();
                log = new AccessLog()
                {
                    User = "system",
                    Action = "Add",
                    Item = "Rental Status",
                    ItemId = transReturnStatus.TransactionId + ", " + transReturnStatus.StatusId,
                    Description = "Return Status: Locker Damaged"
                };
                log.Insert();
            }
            if (checkBox1.Checked)
            {
                transReturnStatus.TransactionId = transactionItem[0].Id;
                transReturnStatus.StatusId = 2;
                transReturnStatus.Insert();
                log = new AccessLog()
                {
                    User = "system",
                    Action = "Add",
                    Item = "Rental Status",
                    ItemId = transReturnStatus.TransactionId + ", " + transReturnStatus.StatusId,
                    Description = "Return Status: Overdue"
                };
                log.Insert();
            }

            //Release the occupied / overdue locker
            string lockerStatus = "";
            var locker = new Locker();
            locker = locker.Get(selectedRental.LockerID);

            if (locker.IsOverdued())
                lockerStatus = "Overdue";
            else
                lockerStatus = "Occupied";
            if (!_keyLostFineAdded && !_lockerDamagedFineAdded)
            {
                locker.Reset();
                log = new AccessLog()
                {
                    User = "system",
                    Action = "Update",
                    Item = "Locker",
                    ItemId = locker.Id.ToString(),
                    Description = "Code: " + locker.Code + "; Status: " + lockerStatus + " to Available"
                };
                log.Insert();

                //Check is the cabinet full, if yes, set to available
                var cabinet = new Cabinet();
                cabinet = cabinet.Get(locker.CabinetID);
                if (cabinet.IsFull())
                {
                    cabinet.Restore();

                    log = new AccessLog()
                    {
                        User = "system",
                        Action = "Update",
                        Item = "Cabinet",
                        ItemId = cabinet.Id.ToString(),
                        Description = "Code: " + cabinet.Code + "; Status: Full to Available"
                    };
                    log.Insert();
                }
            }
            else
            {
                locker.NotAvailable();
                string reason = "";
                if (_keyLostFineAdded && !_lockerDamagedFineAdded)
                    reason += "Key Lost";
                else if (!_keyLostFineAdded && _lockerDamagedFineAdded)
                    reason += "Locker Damaged";
                else
                    reason += "Key Lost & Locker Damaged";
                log = new AccessLog()
                {
                    User = "system",
                    Action = "Disable",
                    Item = "Locker",
                    ItemId = locker.Id.ToString(),
                    Description = "Code: " + locker.Code + "; Status: " + lockerStatus + " to Not Available; Reason: " + reason
                };
                log.Insert();
            }

            //Delete the rental 
            selectedRental.Delete();
        }
    }
}
