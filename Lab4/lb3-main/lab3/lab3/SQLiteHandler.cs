using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace lab3
{
    public class SQLiteHandler : IDisposable
    {
        private readonly string connectionString =
            "Data Source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keywords.db");

        private SqliteConnection connection;

        public bool IsConnected
        {
            get { return connection != null && connection.State == ConnectionState.Open; }
        }

        public void ConnectToDb()
        {
            if (IsConnected)
                return;

            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }

            connection = new SqliteConnection(connectionString);
            connection.Open();

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS Keywords (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Keyword TEXT NOT NULL);";
                command.ExecuteNonQuery();
            }
        }

        public void DisconnectFromDb()
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        public void InsertKeyword(string k)
        {
            EnsureConnected();

            if (string.IsNullOrWhiteSpace(k))
                throw new ArgumentException("The keyword cannot be empty.", "k");

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Keywords (Keyword) VALUES ($keyword);";
                command.Parameters.AddWithValue("$keyword", k.Trim());
                command.ExecuteNonQuery();
            }
        }

        public List<string> GetAllKeywords()
        {
            EnsureConnected();

            List<string> keywords = new List<string>();
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Keyword FROM Keywords ORDER BY Id;";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        keywords.Add(reader.GetString(0));
                }
            }

            return keywords;
        }

        private void EnsureConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Connect to the database first.");
        }

        public void Dispose()
        {
            DisconnectFromDb();
        }
    }
}
