﻿using JavaProject___Server.NET.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using JavaProject___Server.NET.SQL;

namespace JavaProject___Server
{

    public class Client
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string IPAdress { get; set; }
        public string UID { get; set; }

        public int sendingPacketsPerMinute { get; set; }

        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;

        //Client giriş yapınca Sunucu clientin UID'sini ve username'ini kaydediyor ve sunucu loguna bilgi düşüyor
        public Client(TcpClient client)
        {
            _ = Task.Run(() =>
            {
                while (true)
                {
                    sendingPacketsPerMinute = 0;
                    System.Threading.Thread.Sleep(60000);
                }
            });

            //sql bağlandığında uid, mesajları ve username i sql den çekicez 

            ClientSocket = client;
            _packetReader = new PacketReader(ClientSocket.GetStream());
            try
            {
                IPAdress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                MySqlDataBase sql = new MySqlDataBase();


                bool status = false;
                while (!status)
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        //opcode 0 ise kullanıcı kayıt oluyor
                        case 0:
                            sendingPacketsPerMinute++;
                            if (sendingPacketsPerMinute>10)
                            {
                                Program.SendToManyPackets(this);
                            }
                            else
                            {
                                Username = _packetReader.ReadMessage();
                                Email = _packetReader.ReadMessage();
                                Password = _packetReader.ReadMessage();
                                Console.WriteLine("[" + DateTime.Now + "]: [/" + IPAdress + "] user tried to sign up, checking information...");
                                if (sql.CheckRegisterUser(Username, Email))
                                {
                                    Program.SendRegisterInfo(this, false);
                                    Console.WriteLine("[" + DateTime.Now + "]: [/" + IPAdress + "] user already registered, username: " + Username);
                                }
                                else
                                {
                                    UID = Guid.NewGuid().ToString();
                                    Program._users.Add(this);
                                    Program.sendUserInfo(this, Username, UID);
                                    Console.WriteLine("[" + DateTime.Now + "]: [/" + IPAdress + "] user registered, username: " + Username);
                                    sql.InsertUser(Username, UID, Email, Password);
                                    status = true;
                                    Program.SendRegisterInfo(this, true);

                                    Task.Run(() => Procces(sql));
                                    sql.createUserStorage(this);

                                    Program.sendConnectionInfo(sql);

                                }
                            }  
                            break;
                        //opcode 1 ise kullanıcı giriş yapıyor
                        case 1:
                            sendingPacketsPerMinute++;
                            if (sendingPacketsPerMinute > 10)
                            {
                                Program.SendToManyPackets(this);
                            }
                            else
                            {
                                Email = _packetReader.ReadMessage();
                                Password = _packetReader.ReadMessage();
                                Console.WriteLine("[" + DateTime.Now + "]: [/" + IPAdress + "] user tried to sign in, checking information...");
                                if (sql.CheckLoginUser(Email, Password))
                                {
                                    bool check = false;
                                    foreach (Client c in Program._users)
                                    {
                                        if (c.Email == Email)
                                        {
                                            Program.SendLoginInfo(this, false);
                                            Console.WriteLine("[" + DateTime.Now + "]: [/" + IPAdress + "] user already logged in, username: " + this.Username);
                                            check = true;
                                            break;
                                        }
                                    }
                                    if (check)
                                    {
                                        break;
                                    }
                                    Username = sql.getName(Email);
                                    UID = sql.getUID(Email);
                                    Program._users.Add(this);
                                    Program.sendUserInfo(this, Username, UID);
                                    Console.WriteLine("[" + DateTime.Now + "]: [/" + IPAdress + "] user logged in, username: " + this.Username);
                                    status = true;
                                    Program.SendLoginInfo(this, true);
                                    Task.Run(() => Procces(sql));
                                    Program.sendConnectionInfo(sql);
                                }
                                else
                                {
                                    Program.SendLoginInfo(this, false);
                                    Console.WriteLine("[" + DateTime.Now + "]: [/" + IPAdress + "] user unknown account: " + Email);
                                }
                            }
                            break;
                       
                        //opcode yanlış ise bu hatayı veriyor konsola yazdırıyor
                        default:
                            Console.WriteLine("[" + DateTime.Now + "]: [/" + IPAdress + "] user unknown opcode: " + opcode);
                            break;
                    }
                }
            }
            catch
            {

            }
        }


        //Clientin paketlerini okuyor
        void Procces(MySqlDataBase sql)
        {
            
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var message = _packetReader.ReadMessage();
                            var contactUID = _packetReader.ReadMessage();
                            var firstMessage = _packetReader.ReadMessage();
                            var messageUID = _packetReader.ReadMessage();
                            if (firstMessage == "True")
                            {
                                firstMessage = "1";
                            }
                            else
                            {
                                firstMessage = "0";
                            }
                            sql.InsertMessage(this.Username, this.Username, contactUID, "imagelink", message, DateTime.Now.ToString(), firstMessage, messageUID);
                            sql.InsertMessage(sql.getName(sql.getMail(contactUID)), this.Username, this.UID, "imagelink", message, DateTime.Now.ToString(), firstMessage, messageUID);
                            Program.sendMessage(message,contactUID,UID, messageUID);
                            break;
                        case 6:
                            var groupName = _packetReader.ReadMessage();
                            var groupUsernames = _packetReader.ReadMessage();
                            List<string> usernames = groupUsernames.Split(' ').ToList();
                            string clientUIDS = "";

                            foreach (string username in usernames)
                            {
                                Client client = Program._users.Where(x => x.Username.ToLower() == username).FirstOrDefault();
                                if (client != null)
                                {
                                    clientUIDS+= client.UID + " ";
                                }
                            }
                            clientUIDS.TrimEnd(' ');
                            Program.createGroup(groupName, clientUIDS);
                            break;
                        case 7:
                            var deleteMessageUID = _packetReader.ReadMessage();
                            var deleteMessageContactUID = _packetReader.ReadMessage();
                            
                            if (sql.checkMessage(this.Username, deleteMessageUID))
                            {
                                sql.deleteMessage(this.Username, deleteMessageUID);
                                sql.deleteMessage(sql.getName(sql.getMail(deleteMessageContactUID)), deleteMessageUID);
                                Program.deleteMessage(deleteMessageUID, this.UID, deleteMessageContactUID);
                                Program.deleteMessage(deleteMessageUID, deleteMessageContactUID, this.UID);
                            }
                            break;

                        case 8:
                            var userColor = _packetReader.ReadMessage();
                            Program.BroadcastMutedState(userColor, this.UID);
                            break;
                        //Eğer yanlış bir opcode gelirse bu hatayı veriyor konsola yazdırıyor
                        default:
                            Console.WriteLine("[" + DateTime.Now + "]: Unknown opcode: " + opcode);
                            break;
                    }
                }
                catch
                {
                    //Eğer Client Programı kapatırsa ve ya interneti giderse sunucu kullanıcının bilgilerini siliyor
                    Console.WriteLine("[" + DateTime.Now + "]: " + Username + "[/" + IPAdress + "] has disconnected.");
                    Program.sendDisconnectionInfo(this);
                    Program._users.Remove(this);
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
