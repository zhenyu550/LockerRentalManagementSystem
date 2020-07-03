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
    public partial class RentalSettingsForm : Form
    {
        private bool _insertComplete;
        private List<RentalSettings> _rentalSettingsList;

        public bool InsertComplete { get { return _insertComplete; } }

        public RentalSettingsForm(bool setting_done)
        {
            InitializeComponent();
            _insertComplete = setting_done;
            if (!_insertComplete)
            {
                button3.Hide();
                button4.Hide();
            }
            else
            {
                button1.Hide();
                button2.Hide();
                _rentalSettingsList = RentalSettings.All(0, 10);
                LoadSettingsValue();
                LockInput();
            }

        }

        private void Button1_Click(object sender, EventArgs e) //Cancel or Back button
        {
            if (!_insertComplete)
                this.Close();
            else
            {
                button1.Hide();
                button2.Hide();
                button3.Show();
                button4.Show();
                LoadSettingsValue();
                LockInput();
            }
        }

        private void Button2_Click(object sender, EventArgs e) //Save button
        {
            decimal maxOverdueDays;
            decimal overdueFine;
            decimal keyLostFine;
            decimal lockerDamagedFine;

            maxOverdueDays = numericUpDown1.Value;
            overdueFine = numericUpDown2.Value;
            keyLostFine = numericUpDown3.Value;
            lockerDamagedFine = numericUpDown4.Value;


            if (!_insertComplete)
            {
                var rSetting = new RentalSettings();
                rSetting.Name = "Max Overdue Days";
                rSetting.SettingValue = maxOverdueDays;
                rSetting.Save();

                rSetting = new RentalSettings();
                rSetting.Name = "Overdue Fine";
                rSetting.SettingValue = overdueFine;
                rSetting.Save();

                rSetting = new RentalSettings();
                rSetting.Name = "Key Lost Fine";
                rSetting.SettingValue = keyLostFine;
                rSetting.Save();

                rSetting = new RentalSettings();
                rSetting.Name = "Locker Damage Fine";
                rSetting.SettingValue = lockerDamagedFine;
                rSetting.Save();

                _insertComplete = true;

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Add",
                    Item = "Rental Settings"
                };
                log.Insert();

                this.Close();
            }
            else
            {
                int noOfChangedAttr = 0;
                string updatedAttr = "";
                if (_rentalSettingsList[0].SettingValue != maxOverdueDays)
                {
                    if (noOfChangedAttr == 0)
                        updatedAttr += "Max Overdue Days: " + Convert.ToInt32(_rentalSettingsList[0].SettingValue).ToString()
                            + " day to " + Convert.ToInt32(maxOverdueDays).ToString() + " day";
                    else
                        updatedAttr += "; Max Overdue Days: " + Convert.ToInt32(_rentalSettingsList[0].SettingValue).ToString() 
                            + " day to " + Convert.ToInt32(maxOverdueDays).ToString() + " day";

                    _rentalSettingsList[0].SettingValue = maxOverdueDays;
                    _rentalSettingsList[0].Save();
                    noOfChangedAttr++;
                }

                if (_rentalSettingsList[1].SettingValue != overdueFine)
                {
                    if (noOfChangedAttr == 0)
                        updatedAttr += "Overdue Fine: " + _rentalSettingsList[1].SettingValue + " to " + overdueFine;
                    else
                        updatedAttr += "; Overdue Fine: " + _rentalSettingsList[1].SettingValue + " to " + overdueFine;

                    _rentalSettingsList[1].SettingValue = overdueFine;
                    _rentalSettingsList[1].Save();
                    noOfChangedAttr++;
                }

                if (_rentalSettingsList[2].SettingValue != keyLostFine)
                {
                    if (noOfChangedAttr == 0)
                        updatedAttr += "Key Lost Fine: " + _rentalSettingsList[2].SettingValue + " to " + keyLostFine;
                    else
                        updatedAttr += "; Key Lost Fine: " + _rentalSettingsList[2].SettingValue + " to " + keyLostFine;

                    _rentalSettingsList[2].SettingValue = keyLostFine;
                    _rentalSettingsList[2].Save();
                    noOfChangedAttr++;
                }

                if (_rentalSettingsList[3].SettingValue != lockerDamagedFine)
                {
                    if (noOfChangedAttr == 0)
                        updatedAttr += "Locker Damage Fine: " + _rentalSettingsList[3].SettingValue + " to " + lockerDamagedFine;
                    else
                        updatedAttr += "; Locker Damage Fine: " + _rentalSettingsList[3].SettingValue + " to " + lockerDamagedFine;

                    _rentalSettingsList[3].SettingValue = lockerDamagedFine;
                    _rentalSettingsList[3].Save();
                    noOfChangedAttr++;
                }

                var log = new AccessLog()
                {
                    User = Login.Username,
                    Action = "Update",
                    Item = "Rental Settings",
                    Description = updatedAttr
                };
                log.Insert();

                button1.Hide();
                button2.Hide();
                button3.Show();
                button4.Show();
                LoadSettingsValue();
                LockInput();
            }
        }

        private void Button3_Click(object sender, EventArgs e) //Close button
        {
            this.Close();
        }

        private void Button4_Click(object sender, EventArgs e) //Edit button
        {
            button3.Hide();
            button4.Hide();
            button1.Text = "Back";
            button1.Show();
            button2.Show();
            UnlockInput();
        }

        private void LoadSettingsValue()
        {
            numericUpDown1.Value = _rentalSettingsList[0].SettingValue;
            numericUpDown2.Value = _rentalSettingsList[1].SettingValue;
            numericUpDown3.Value = _rentalSettingsList[2].SettingValue;
            numericUpDown4.Value = _rentalSettingsList[3].SettingValue;
        }

        private void LockInput()
        {
            numericUpDown1.ReadOnly = true;
            numericUpDown2.ReadOnly = true;
            numericUpDown3.ReadOnly = true;
            numericUpDown4.ReadOnly = true;
        }

        private void UnlockInput()
        {
            numericUpDown1.ReadOnly = false;
            numericUpDown2.ReadOnly = false;
            numericUpDown3.ReadOnly = false;
            numericUpDown4.ReadOnly = false;
        }

        private void RentalSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_insertComplete)
            {
                MessageBox.Show("Exit Error: Rental Settings Not Completed." + Environment.NewLine +
                    "You must complete the set up for rental settings.", "Exit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }
    }
}
