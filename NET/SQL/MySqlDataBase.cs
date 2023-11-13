﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JavaProject___Server.NET.SQL
{
    public class MySqlDataBase
    {
        private readonly string _connectionString;
        private readonly string _connectionString2;
        public MySqlDataBase()
        {
            _connectionString = "Server=46.31.77.173,3306;Database=javaproject;Uid=JavaProject;Pwd=JavaProject_ICU123;";
            _connectionString2 = "Server=46.31.77.173,3306;Database=javaproject_user;Uid=JavaProject;Pwd=JavaProject_ICU123;";
        }
        protected MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
        protected MySqlConnection GetConnection2()
        {
            return new MySqlConnection(_connectionString2);
        }

        
        public void createUserStorage(Client client)
        {
            using (MySqlConnection conn = GetConnection2())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS user_" + client.Username + " (Username VARCHAR(255), UID VARCHAR(36), ImageSource VARCHAR(255), Message VARCHAR(1000), Time VARCHAR(255), FirstMessage BOOLEAN)", conn);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                conn.Close();
            }
        }

        public void InsertMessage(Client client ,string username, string uid, string imageSource, string message, string time, string fistMessage)
        {
              using (MySqlConnection conn = GetConnection2())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO user_" + client.Username + " (Username, UID, ImageSource, Message, Time, FirstMessage) VALUES (@Username, @UID, @ImageSource, @Message, @Time, @FirstMessage)", conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@UID", uid);
                cmd.Parameters.AddWithValue("@ImageSource", imageSource);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@Time", time);
                cmd.Parameters.AddWithValue("@FirstMessage", fistMessage);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public string getName(string email)
        {
              using (MySqlConnection conn = GetConnection())
              {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT Username FROM users WHERE Email=@Email", conn);
                    cmd.Parameters.AddWithValue("@Email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            string name = reader.GetString(0);
                            conn.Close();
                            return name;
                        }
                        else
                        {
                            conn.Close();
                            return null;
                        }
                    }
              }
        }

        public string getFriends(string email)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT FriendsUIDS FROM users WHERE Email=@Email", conn);
                cmd.Parameters.AddWithValue("@email", email);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        string friends = reader.GetString(0);
                        conn.Close();
                        return friends;
                    }
                    else
                    {
                        conn.Close();
                        return "";
                    }
                }
            }
        }
        public string getUID(string email)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT UID FROM users WHERE Email=@Email", conn);
                cmd.Parameters.AddWithValue("@email", email);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        string uid = reader.GetString(0);
                        conn.Close();
                        return uid;
                    }
                    else
                    {
                        conn.Close();
                        return null;
                    }
                }
            }
        }
        public bool CheckLoginUser(string email, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM users WHERE Email=@email AND Password=@password", conn);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                using (var reader = cmd.ExecuteReader())
                {
                    
                    if (reader.HasRows)
                    {
                        conn.Close();
                        return true;
                    }
                    else
                    {
                        conn.Close();
                        return false;
                    }
                }
            }
        }

        public bool CheckRegisterUser(string username, string email)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM users WHERE Email=@email OR Username=@username", conn);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@username", username);
                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            conn.Close();
                            return true;
                        }
                        else
                        {
                            conn.Close();
                            return false;
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }
        }
        public void InsertUser(string username, string uid, string email, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO users (Username, UID, Email, Password) VALUES (@username, @uid, @email, @password)", conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@uid", uid);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
