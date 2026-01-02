using System;
using System.Collections.Generic;
using hub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public int Liras { get; set; }
        public int Ounces { get; set; }
        public int Semiliras { get; set; }
        public int CommissionID { get; set; }
        public decimal GoldPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }  // ✅ Added
    }

    public class Transactionsdata
    {
        private string connectionString = ConnectionString.connectionstring;

        private readonly IHubContext<TransactionHub> _hubContext;

        public Transactionsdata()
        {

        }

        public Transactionsdata(IHubContext<TransactionHub> hubContext)
        {
            _hubContext = hubContext;
        }


        public bool AddTransaction(Transaction transaction)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction sqlTran = conn.BeginTransaction();

                try
                {
                    // 1. Insert into Transactions Table
                    string insertQuery = @"INSERT INTO Transactions 
                (db_senderID, db_receiverID, db_liras, db_ounces, db_semiliras, db_commissionID, db_goldprice, db_totalprice, db_date, db_status)
                VALUES (@SenderID, @ReceiverID, @Liras, @Ounces, @Semiliras, @CommissionID, @GoldPrice, @TotalPrice, @Date, @Status)";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn, sqlTran);
                    insertCmd.Parameters.AddWithValue("@SenderID", transaction.SenderID);
                    insertCmd.Parameters.AddWithValue("@ReceiverID", transaction.ReceiverID);
                    insertCmd.Parameters.AddWithValue("@Liras", transaction.Liras);
                    insertCmd.Parameters.AddWithValue("@Ounces", transaction.Ounces);
                    insertCmd.Parameters.AddWithValue("@Semiliras", transaction.Semiliras);
                    insertCmd.Parameters.AddWithValue("@CommissionID", transaction.CommissionID);
                    insertCmd.Parameters.AddWithValue("@GoldPrice", transaction.GoldPrice);
                    insertCmd.Parameters.AddWithValue("@TotalPrice", transaction.TotalPrice);
                    insertCmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    insertCmd.Parameters.AddWithValue("@Status", "Pending");

                    insertCmd.ExecuteNonQuery();

                    // 2. Update Sender Wallet (subtract)
                    string updateSenderWallet = @"UPDATE wallets
                    SET db_liras = db_liras - @Liras,
                        db_ounces = db_ounces - @Ounces,
                        db_semiliras = db_semiliras - @Semiliras
                    WHERE TraderID = @SenderID";

                    SqlCommand senderCmd = new SqlCommand(updateSenderWallet, conn, sqlTran);
                    senderCmd.Parameters.AddWithValue("@Liras", transaction.Liras);
                    senderCmd.Parameters.AddWithValue("@Ounces", transaction.Ounces);
                    senderCmd.Parameters.AddWithValue("@Semiliras", transaction.Semiliras);
                    senderCmd.Parameters.AddWithValue("@SenderID", transaction.SenderID);
                    senderCmd.ExecuteNonQuery();

                    // 3. Update Receiver Wallet (add)
                    string updateReceiverWallet = @"UPDATE wallets
                    SET db_liras = db_liras + @Liras,
                        db_ounces = db_ounces + @Ounces,
                        db_semiliras = db_semiliras + @Semiliras
                    WHERE TraderID = @ReceiverID";

                    SqlCommand receiverCmd = new SqlCommand(updateReceiverWallet, conn, sqlTran);
                    receiverCmd.Parameters.AddWithValue("@Liras", transaction.Liras);
                    receiverCmd.Parameters.AddWithValue("@Ounces", transaction.Ounces);
                    receiverCmd.Parameters.AddWithValue("@Semiliras", transaction.Semiliras);
                    receiverCmd.Parameters.AddWithValue("@ReceiverID", transaction.ReceiverID);
                    receiverCmd.ExecuteNonQuery();

                    // All successful ➔ Commit Transaction
                    sqlTran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    sqlTran.Rollback();
                    Console.WriteLine("Error in AddTransaction: " + ex.Message);
                    return false;
                }
            }
        }


        public Transaction GetTransactionById(int transactionID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Transactions WHERE TransactionID = @TransactionID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TransactionID", transactionID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Transaction
                    {
                        TransactionID = Convert.ToInt32(reader["TransactionID"]),
                        SenderID = reader["db_senderID"].ToString(),
                        ReceiverID = reader["db_receiverID"].ToString(),
                        Liras = Convert.ToInt32(reader["db_liras"]),
                        Ounces = Convert.ToInt32(reader["db_ounces"]),
                        Semiliras = Convert.ToInt32(reader["db_semiliras"]),
                        CommissionID = Convert.ToInt32(reader["db_commissionID"]),
                        GoldPrice = Convert.ToDecimal(reader["db_goldprice"]),
                        TotalPrice = Convert.ToDecimal(reader["db_totalprice"]),
                        Date = Convert.ToDateTime(reader["db_date"]),
                        Status = reader["db_status"].ToString()
                    };
                }
                return null;
            }
        }


        public async Task CancelExpiredTransactions()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string selectQuery = @"SELECT * FROM Transactions 
            WHERE db_status = 'Pending' 
            AND DATEADD(minute, 1, db_date) < GETDATE()"; 

                SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                conn.Open();
                SqlDataReader reader = await selectCmd.ExecuteReaderAsync();

                List<Transaction> expiredTransactions = new List<Transaction>();
                while (await reader.ReadAsync())
                {
                    expiredTransactions.Add(new Transaction
                    {
                        TransactionID = Convert.ToInt32(reader["TransactionID"]),
                        SenderID = reader["db_senderID"].ToString(),
                        ReceiverID = reader["db_receiverID"].ToString(),
                        Liras = Convert.ToInt32(reader["db_liras"]),
                        Ounces = Convert.ToInt32(reader["db_ounces"]),
                        Semiliras = Convert.ToInt32(reader["db_semiliras"])
                    });
                }
                reader.Close();

                foreach (var transaction in expiredTransactions)
                {
                    using (SqlTransaction sqlTran = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Return assets to sender
                            string updateSenderWallet = @"UPDATE wallets
                        SET db_liras = db_liras + @Liras,
                            db_ounces = db_ounces + @Ounces,
                            db_semiliras = db_semiliras + @Semiliras
                        WHERE TraderID = @SenderID";

                            SqlCommand updateSenderCmd = new SqlCommand(updateSenderWallet, conn, sqlTran);
                            updateSenderCmd.Parameters.AddWithValue("@Liras", transaction.Liras);
                            updateSenderCmd.Parameters.AddWithValue("@Ounces", transaction.Ounces);
                            updateSenderCmd.Parameters.AddWithValue("@Semiliras", transaction.Semiliras);
                            updateSenderCmd.Parameters.AddWithValue("@SenderID", transaction.SenderID);
                            await updateSenderCmd.ExecuteNonQueryAsync();

                            // 2. Take back assets from receiver
                            string updateReceiverWallet = @"UPDATE wallets
                        SET db_liras = db_liras - @Liras,
                            db_ounces = db_ounces - @Ounces,
                            db_semiliras = db_semiliras - @Semiliras
                        WHERE TraderID = @ReceiverID";

                            SqlCommand updateReceiverCmd = new SqlCommand(updateReceiverWallet, conn, sqlTran);
                            updateReceiverCmd.Parameters.AddWithValue("@Liras", transaction.Liras);
                            updateReceiverCmd.Parameters.AddWithValue("@Ounces", transaction.Ounces);
                            updateReceiverCmd.Parameters.AddWithValue("@Semiliras", transaction.Semiliras);
                            updateReceiverCmd.Parameters.AddWithValue("@ReceiverID", transaction.ReceiverID);
                            await updateReceiverCmd.ExecuteNonQueryAsync();

                            // 3. Update transaction status to Canceled
                            string updateStatus = @"UPDATE Transactions
                        SET db_status = 'Canceled'
                        WHERE TransactionID = @TransactionID";

                            SqlCommand updateStatusCmd = new SqlCommand(updateStatus, conn, sqlTran);
                            updateStatusCmd.Parameters.AddWithValue("@TransactionID", transaction.TransactionID);
                            await updateStatusCmd.ExecuteNonQueryAsync();

                            // 4. Send live SignalR notification
                            await _hubContext.Clients.All.SendAsync("TransactionCanceled", transaction.TransactionID);

                            sqlTran.Commit(); // 🎯 Commit after everything
                        }
                        catch (Exception ex)
                        {
                            sqlTran.Rollback();
                            Console.WriteLine($"[CancelExpiredTransactions Error] {ex.Message}");
                        }
                    }
                }
            }
        }


        public List<Transaction> GetAllTransactions()
        {
            List<Transaction> list = new List<Transaction>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Transactions";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Transaction
                    {
                        TransactionID = Convert.ToInt32(reader["TransactionID"]),
                        SenderID = reader["db_senderID"].ToString(),
                        ReceiverID = reader["db_receiverID"].ToString(),
                        Liras = Convert.ToInt32(reader["db_liras"]),
                        Ounces = Convert.ToInt32(reader["db_ounces"]),
                        Semiliras = Convert.ToInt32(reader["db_semiliras"]),
                        CommissionID = Convert.ToInt32(reader["db_commissionID"]),
                        GoldPrice = Convert.ToDecimal(reader["db_goldprice"]),
                        TotalPrice = Convert.ToDecimal(reader["db_totalprice"]),
                        Date = Convert.ToDateTime(reader["db_date"]),
                        Status = reader["db_status"].ToString() // ✅ Added
                    });
                }
            }
            return list;
        }

        public List<Transaction> GetTransactionByTraderID(string traderID)
        {
            List<Transaction> transactions = new List<Transaction>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM Transactions 
                        WHERE db_senderID = @TraderID OR db_receiverID = @TraderID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TraderID", traderID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    transactions.Add(new Transaction
                    {
                        TransactionID = Convert.ToInt32(reader["TransactionID"]),
                        SenderID = reader["db_senderID"].ToString(),
                        ReceiverID = reader["db_receiverID"].ToString(),
                        Liras = Convert.ToInt32(reader["db_liras"]),
                        Ounces = Convert.ToInt32(reader["db_ounces"]),
                        Semiliras = Convert.ToInt32(reader["db_semiliras"]),
                        CommissionID = Convert.ToInt32(reader["db_commissionID"]),
                        GoldPrice = Convert.ToDecimal(reader["db_goldprice"]),
                        TotalPrice = Convert.ToDecimal(reader["db_totalprice"]),
                        Date = Convert.ToDateTime(reader["db_date"]),
                        Status = reader["db_status"].ToString() // ✅ Added
                    });
                }
            }
            return transactions;
        }
        public bool ConfirmTransaction(int transactionID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Transactions SET db_status = 'Completed' WHERE TransactionID = @TransactionID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TransactionID", transactionID);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
        }

        public bool CancelTransaction(int transactionID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Transactions SET db_status = 'Canceled' WHERE TransactionID = @TransactionID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TransactionID", transactionID);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
        }

    }
}
