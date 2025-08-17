using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace CSharpHomework11
{
    public class DatabaseHelper
    {
        // Updated connection string for LocalDB
        private static string connectionString = @"Data Source=127.0.0.1,1666\IDENTITYACCESS;
Initial Catalog=EmployeeTimeTracker;
User ID=root;
Password=SQL.rcr2009";
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // Employee Management Methods
        public static DataTable GetEmployee(int empNo)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT * FROM Employees WHERE EmpNo = @EmpNo";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpNo", empNo);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public static void UpdateEmployeePhoto(int empNo, string photoPath)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "UPDATE Employees SET PhotoPath = @PhotoPath WHERE EmpNo = @EmpNo";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PhotoPath", photoPath);
                    cmd.Parameters.AddWithValue("@EmpNo", empNo);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Time Records Management Methods
        public static void AddTimeRecord(int empNo, DateTime date, TimeSpan timeIn, TimeSpan timeOut,
                                       decimal duration, decimal rate, decimal amount)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = @"INSERT INTO TimeRecords (EmpNo, Dates, TimeIn, TimeOut, Duration, Rate, Amount) 
                                VALUES (@EmpNo, @Dates, @TimeIn, @TimeOut, @Duration, @Rate, @Amount)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpNo", empNo);
                    cmd.Parameters.AddWithValue("@Dates", date.Date);
                    cmd.Parameters.AddWithValue("@TimeIn", timeIn);
                    cmd.Parameters.AddWithValue("@TimeOut", timeOut);
                    cmd.Parameters.AddWithValue("@Duration", duration);
                    cmd.Parameters.AddWithValue("@Rate", rate);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateTimeRecord(int recordId, DateTime date, TimeSpan timeIn, TimeSpan timeOut,
                                          decimal duration, decimal rate, decimal amount)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = @"UPDATE TimeRecords SET Dates = @Dates, TimeIn = @TimeIn, TimeOut = @TimeOut, 
                                Duration = @Duration, Rate = @Rate, Amount = @Amount WHERE RecordID = @RecordID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RecordID", recordId);
                    cmd.Parameters.AddWithValue("@Dates", date.Date);
                    cmd.Parameters.AddWithValue("@TimeIn", timeIn);
                    cmd.Parameters.AddWithValue("@TimeOut", timeOut);
                    cmd.Parameters.AddWithValue("@Duration", duration);
                    cmd.Parameters.AddWithValue("@Rate", rate);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteTimeRecord(int recordId)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM TimeRecords WHERE RecordID = @RecordID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RecordID", recordId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable GetTimeRecordsByEmployee(int empNo)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT RecordID, EmpNo, Dates, TimeIn, TimeOut, Duration, Rate, Amount 
                                FROM TimeRecords WHERE EmpNo = @EmpNo ORDER BY Dates DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpNo", empNo);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public static DataTable GetAllEmployees()
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT * FROM Employees";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}