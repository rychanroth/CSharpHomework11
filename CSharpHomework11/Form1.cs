using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CSharpHomework11
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Declare
            bool isConnected;

            //Input
            // No input needed for load

            //Process
            isConnected = DatabaseHelper.TestConnection();
            if (!isConnected)
            {
                MessageBox.Show("Database connection failed! Check your connection string.");
            }

            //Output
            // Form is now loaded and ready
        }

        private void btnChoosePhoto_Click(object sender, EventArgs e)
        {
            //Declare
            OpenFileDialog openFile;
            DialogResult result;
            string photoPath;

            //Input
            openFile = new OpenFileDialog();
            openFile.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFile.Title = "Choose Employee Photo";

            //Process
            result = openFile.ShowDialog();

            if (result == DialogResult.OK)
            {
                try
                {
                    photoPath = openFile.FileName;
                    pictureBox1.Image = Image.FromFile(photoPath);
                    txtPhotoPath.Text = photoPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading photo: " + ex.Message);
                    return;
                }
            }

            //Output
            // Photo is now displayed in pictureBox1 and path in txtPhotoPath
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //Declare
            int empNo;
            bool isValidNumber;
            DataTable employeeData;
            DataRow employee;

            //Input
            isValidNumber = int.TryParse(txtEmpNo.Text, out empNo);

            //Process
            if (!isValidNumber || empNo <= 0)
            {
                MessageBox.Show("Please enter a valid Employee Number!");
                return;
            }

            try
            {
                employeeData = DatabaseHelper.GetEmployee(empNo);

                if (employeeData.Rows.Count > 0)
                {
                    employee = employeeData.Rows[0];

                    // Fill employee information
                    txtEmpName.Text = employee["EmpName"].ToString();
                    txtSex.Text = employee["Sex"].ToString();
                    dtpDOB.Value = Convert.ToDateTime(employee["DOB"]);
                    txtAge.Text = employee["Age"].ToString();
                    txtTitle.Text = employee["Title"].ToString();

                    // Load photo if exists
                    string photoPath = employee["PhotoPath"].ToString();
                    if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
                    {
                        txtPhotoPath.Text = photoPath;
                        pictureBox1.Image = Image.FromFile(photoPath);
                    }
                    else
                    {
                        txtPhotoPath.Text = "";
                        pictureBox1.Image = null;
                    }

                    // Load time records for this employee
                    LoadEmployeeTimeRecords(empNo);
                }
                else
                {
                    MessageBox.Show("Employee not found!");
                    ClearEmployeeFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching employee: " + ex.Message);
            }

            //Output
            // Employee data is now loaded or error message shown
        }

        private void txtTimeIn_TextChanged(object sender, EventArgs e)
        {
            CalculateDuration();
        }

        private void txtTimeOut_TextChanged(object sender, EventArgs e)
        {
            CalculateDuration();
        }

        private void txtRate_TextChanged(object sender, EventArgs e)
        {
            CalculateAmount();
        }

        private void CalculateDuration()
        {
            //Declare
            TimeSpan timeIn, timeOut;
            bool validTimeIn, validTimeOut;
            double durationHours;

            //Input
            validTimeIn = TimeSpan.TryParse(txtTimeIn.Text, out timeIn);
            validTimeOut = TimeSpan.TryParse(txtTimeOut.Text, out timeOut);

            //Process
            if (validTimeIn && validTimeOut && timeOut > timeIn)
            {
                durationHours = (timeOut - timeIn).TotalHours;
                txtDuration.Text = durationHours.ToString("F2");

                // Also calculate amount when duration changes
                CalculateAmount();
            }
            else if (!string.IsNullOrEmpty(txtTimeIn.Text) && !string.IsNullOrEmpty(txtTimeOut.Text))
            {
                txtDuration.Text = "0.00";
            }

            //Output
            // Duration is now calculated and displayed
        }

        private void CalculateAmount()
        {
            //Declare
            decimal duration, rate, amount;
            bool validDuration, validRate;

            //Input
            validDuration = decimal.TryParse(txtDuration.Text, out duration);
            validRate = decimal.TryParse(txtRate.Text, out rate);

            //Process
            if (validDuration && validRate)
            {
                amount = duration * rate;
                txtAmount.Text = amount.ToString("F2");
            }
            else
            {
                txtAmount.Text = "0.00";
            }

            //Output
            // Amount is now calculated and displayed
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //Declare
            int empNo;
            DateTime currentDate;
            TimeSpan timeIn, timeOut;
            decimal duration, rate, amount;
            bool isValid;
            DataTable timeRecords;
            DataRow newRecord;

            //Input
            isValid = ValidateTimeInput(out empNo, out timeIn, out timeOut, out duration, out rate, out amount);

            //Process
            if (!isValid)
            {
                return; // Error message already shown in validation
            }

            try
            {
                currentDate = DateTime.Now.Date;

                // Get current DataGridView data or create new table
                if (dataGridView1.DataSource == null)
                {
                    timeRecords = CreateEmptyTimeRecordsTable();
                }
                else
                {
                    timeRecords = (DataTable)dataGridView1.DataSource;
                }

                // Add new record
                newRecord = timeRecords.NewRow();
                newRecord["EmpNo"] = empNo;
                newRecord["Dates"] = currentDate;
                newRecord["TimeIn"] = timeIn;
                newRecord["TimeOut"] = timeOut;
                newRecord["Duration"] = duration;
                newRecord["Rate"] = rate;
                newRecord["Amount"] = amount;
                timeRecords.Rows.Add(newRecord);

                // Update DataGridView
                dataGridView1.DataSource = timeRecords;

                // Clear input fields
                ClearTimeFields();

                // Update total salary
                UpdateTotalSalary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding record: " + ex.Message);
            }

            //Output
            MessageBox.Show("Record added successfully! Click Save to save to database.");
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            //Declare
            int empNo;
            TimeSpan timeIn, timeOut;
            decimal duration, rate, amount;
            bool isValid;
            DataGridViewRow selectedRow;

            //Input
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a record to modify!");
                return;
            }

            selectedRow = dataGridView1.SelectedRows[0];
            isValid = ValidateTimeInput(out empNo, out timeIn, out timeOut, out duration, out rate, out amount);

            //Process
            if (!isValid)
            {
                return;
            }

            try
            {
                selectedRow.Cells["Dates"].Value = DateTime.Now.Date;
                selectedRow.Cells["TimeIn"].Value = timeIn;
                selectedRow.Cells["TimeOut"].Value = timeOut;
                selectedRow.Cells["Duration"].Value = duration;
                selectedRow.Cells["Rate"].Value = rate;
                selectedRow.Cells["Amount"].Value = amount;

                ClearTimeFields();
                UpdateTotalSalary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error modifying record: " + ex.Message);
            }

            //Output
            MessageBox.Show("Record modified successfully! Click Save to save to database.");
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            //Declare
            DialogResult confirmResult;

            //Input
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a record to remove!");
                return;
            }

            confirmResult = MessageBox.Show("Are you sure you want to remove this record?",
                                          "Confirm Remove", MessageBoxButtons.YesNo);

            //Process
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                    UpdateTotalSalary();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing record: " + ex.Message);
                    return;
                }
            }

            //Output
            MessageBox.Show("Record removed successfully! Click Save to save to database.");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Declare
            int empNo;
            bool validEmpNo;
            DataTable existingRecords;

            //Input
            validEmpNo = int.TryParse(txtEmpNo.Text, out empNo);

            //Process
            if (!validEmpNo || empNo <= 0)
            {
                MessageBox.Show("Please search for an employee first!");
                return;
            }

            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("No records to save!");
                return;
            }

            try
            {
                // Save photo path if changed
                if (!string.IsNullOrEmpty(txtPhotoPath.Text))
                {
                    DatabaseHelper.UpdateEmployeePhoto(empNo, txtPhotoPath.Text);
                }

                // Clear existing time records for this employee
                existingRecords = DatabaseHelper.GetTimeRecordsByEmployee(empNo);
                foreach (DataRow row in existingRecords.Rows)
                {
                    DatabaseHelper.DeleteTimeRecord(Convert.ToInt32(row["RecordID"]));
                }

                // Save all records from DataGridView
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    DateTime date = Convert.ToDateTime(row.Cells["Dates"].Value);
                    TimeSpan timeIn = (TimeSpan)row.Cells["TimeIn"].Value;
                    TimeSpan timeOut = (TimeSpan)row.Cells["TimeOut"].Value;
                    decimal duration = Convert.ToDecimal(row.Cells["Duration"].Value);
                    decimal rate = Convert.ToDecimal(row.Cells["Rate"].Value);
                    decimal amount = Convert.ToDecimal(row.Cells["Amount"].Value);

                    DatabaseHelper.AddTimeRecord(empNo, date, timeIn, timeOut, duration, rate, amount);
                }

                // Reload data from database
                LoadEmployeeTimeRecords(empNo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving to database: " + ex.Message);
                return;
            }

            //Output
            MessageBox.Show("All records saved to database successfully!");
        }

        private void btnReceipt_Click(object sender, EventArgs e)
        {
            //Declare
            string receiptText;
            Form receiptForm;
            TextBox receiptDisplay;

            //Input
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("No records to print receipt!");
                return;
            }

            //Process
            try
            {
                receiptText = GenerateReceiptText();

                // Create receipt display form
                receiptForm = new Form();
                receiptForm.Text = "Employee Pay Receipt";
                receiptForm.Size = new Size(600, 700);
                receiptForm.StartPosition = FormStartPosition.CenterParent;

                receiptDisplay = new TextBox();
                receiptDisplay.Multiline = true;
                receiptDisplay.ScrollBars = ScrollBars.Vertical;
                receiptDisplay.ReadOnly = true;
                receiptDisplay.Dock = DockStyle.Fill;
                receiptDisplay.Font = new Font("Courier New", 10);
                receiptDisplay.Text = receiptText;

                receiptForm.Controls.Add(receiptDisplay);
                receiptForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating receipt: " + ex.Message);
            }

            //Output
            // Receipt is now displayed in new window
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            //Declare
            DataGridViewRow selectedRow;
            TimeSpan timeIn, timeOut;

            //Input
            if (dataGridView1.SelectedRows.Count == 0) return;
            selectedRow = dataGridView1.SelectedRows[0];

            //Process
            if (selectedRow.Cells["TimeIn"].Value != null)
            {
                try
                {
                    timeIn = (TimeSpan)selectedRow.Cells["TimeIn"].Value;
                    timeOut = (TimeSpan)selectedRow.Cells["TimeOut"].Value;

                    txtTimeIn.Text = timeIn.ToString(@"hh\:mm");
                    txtTimeOut.Text = timeOut.ToString(@"hh\:mm");
                    txtDuration.Text = selectedRow.Cells["Duration"].Value.ToString();
                    txtRate.Text = selectedRow.Cells["Rate"].Value.ToString();
                    txtAmount.Text = selectedRow.Cells["Amount"].Value.ToString();
                }
                catch (Exception ex)
                {
                    // Ignore selection errors
                }
            }

            //Output
            // Time fields are now populated with selected row data
        }

        // Helper Methods
        private bool ValidateTimeInput(out int empNo, out TimeSpan timeIn, out TimeSpan timeOut,
                                     out decimal duration, out decimal rate, out decimal amount)
        {
            //Declare
            bool isValid = true;
            string errorMessage = "";

            // Initialize out parameters
            empNo = 0;
            timeIn = TimeSpan.Zero;
            timeOut = TimeSpan.Zero;
            duration = 0;
            rate = 0;
            amount = 0;

            //Input & Process
            // Validate Employee Number
            if (!int.TryParse(txtEmpNo.Text, out empNo) || empNo <= 0)
            {
                errorMessage = "Please enter a valid Employee Number!";
                isValid = false;
            }
            // Validate Time In
            else if (!TimeSpan.TryParse(txtTimeIn.Text, out timeIn))
            {
                errorMessage = "Please enter valid Time In (HH:mm format)!";
                isValid = false;
            }
            // Validate Time Out
            else if (!TimeSpan.TryParse(txtTimeOut.Text, out timeOut))
            {
                errorMessage = "Please enter valid Time Out (HH:mm format)!";
                isValid = false;
            }
            // Validate Time Logic
            else if (timeOut <= timeIn)
            {
                errorMessage = "Time Out must be later than Time In!";
                isValid = false;
            }
            // Validate Rate
            else if (!decimal.TryParse(txtRate.Text, out rate) || rate <= 0)
            {
                errorMessage = "Please enter a valid Rate per Hour!";
                isValid = false;
            }
            // Calculate duration and amount if all valid
            else
            {
                duration = (decimal)(timeOut - timeIn).TotalHours;
                amount = duration * rate;
            }

            //Output
            if (!isValid)
            {
                MessageBox.Show(errorMessage);
            }
            return isValid;
        }

        private void LoadEmployeeTimeRecords(int empNo)
        {
            //Declare
            DataTable timeRecords;

            //Input
            // empNo parameter

            //Process
            try
            {
                timeRecords = DatabaseHelper.GetTimeRecordsByEmployee(empNo);
                dataGridView1.DataSource = timeRecords;
                UpdateTotalSalary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading time records: " + ex.Message);
            }

            //Output
            // Time records are now displayed in DataGridView
        }

        private DataTable CreateEmptyTimeRecordsTable()
        {
            //Declare
            DataTable table;

            //Input
            // No input needed

            //Process
            table = new DataTable();
            table.Columns.Add("EmpNo", typeof(int));
            table.Columns.Add("Dates", typeof(DateTime));
            table.Columns.Add("TimeIn", typeof(TimeSpan));
            table.Columns.Add("TimeOut", typeof(TimeSpan));
            table.Columns.Add("Duration", typeof(decimal));
            table.Columns.Add("Rate", typeof(decimal));
            table.Columns.Add("Amount", typeof(decimal));

            //Output
            return table;
        }

        private void UpdateTotalSalary()
        {
            //Declare
            decimal totalSalary = 0;

            //Input
            // DataGridView rows

            //Process
            if (dataGridView1.DataSource is DataTable dt)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Amount"] != DBNull.Value)
                    {
                        totalSalary += Convert.ToDecimal(row["Amount"]);
                    }
                }
            }
            else
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    if (row.Cells["Amount"].Value != null)
                    {
                        totalSalary += Convert.ToDecimal(row.Cells["Amount"].Value);
                    }
                }
            }

            //Output
            txtTotalSalary.Text = totalSalary.ToString("F2");
        }

        private string GenerateReceiptText()
        {
            //Declare
            string receipt = "";
            decimal totalAmount = 0;

            //Input
            // DataGridView data and employee info

            //Process
            receipt += "===============================================\n";
            receipt += "              EMPLOYEE PAY RECEIPT            \n";
            receipt += "===============================================\n";
            receipt += $"Employee: {txtEmpName.Text} (#{txtEmpNo.Text})\n";
            receipt += $"Title: {txtTitle.Text}\n";
            receipt += $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
            receipt += "===============================================\n\n";

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                DateTime date = Convert.ToDateTime(row.Cells["Dates"].Value);
                TimeSpan timeIn = (TimeSpan)row.Cells["TimeIn"].Value;
                TimeSpan timeOut = (TimeSpan)row.Cells["TimeOut"].Value;
                decimal duration = Convert.ToDecimal(row.Cells["Duration"].Value);
                decimal rate = Convert.ToDecimal(row.Cells["Rate"].Value);
                decimal amount = Convert.ToDecimal(row.Cells["Amount"].Value);
                totalAmount += amount;

                receipt += $"Date: {date:yyyy-MM-dd}\n";
                receipt += $"Time: {timeIn:hh\\:mm} - {timeOut:hh\\:mm} ({duration:F2} hrs)\n";
                receipt += $"Rate: ${rate:F2}/hr  Amount: ${amount:F2}\n";
                receipt += "-----------------------------------------------\n";
            }

            receipt += $"\nTOTAL AMOUNT: ${totalAmount:F2}\n";
            receipt += "===============================================\n";

            //Output
            return receipt;
        }

        private void ClearEmployeeFields()
        {
            //Process
            txtEmpName.Clear();
            txtSex.Clear();
            dtpDOB.Value = DateTime.Now;
            txtAge.Clear();
            txtTitle.Clear();
            txtPhotoPath.Clear();
            pictureBox1.Image = null;
            dataGridView1.DataSource = null;
            txtTotalSalary.Text = "0.00";
        }

        private void ClearTimeFields()
        {
            //Process
            txtTimeIn.Clear();
            txtTimeOut.Clear();
            txtDuration.Clear();
            txtRate.Clear();
            txtAmount.Clear();
        }
    }
}