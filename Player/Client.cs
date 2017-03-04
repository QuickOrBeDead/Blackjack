﻿using Common.Lib.Models;
using Newtonsoft.Json;
using Player.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientTCP
{
    /// <summary>
    /// Custom data received event arguments, holds the data that was received
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        public CommandObject CmdObject { get; set; }
    }

    // Delegate for hooking up connected notification
    public delegate void ClientConnectedEventHandler(object sender, EventArgs e);

    // Delegate for hooking up receive notification
    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);

    public class Client
    {

        private Socket clientSocket;       
        private byte[] byteData = new byte[1024];

        public event ClientConnectedEventHandler OnConnected;
        public event DataReceivedEventHandler OnDataReceived;

        public Client() {}

        public void Connect(string ip, int port)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

                //Connect to the server
                clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connect: " + ex.Message);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);

                byteData = new byte[1024];

                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), byteData);

                // Invoke the OnConneted event if one exists
                if (OnConnected != null)
                {
                    EventArgs e = new EventArgs();
                    OnConnected(this, e);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("OnConnect: " + ex.Message);
            }
        }

        public void Send(CommandObject commandObject)
        {
            try
            {
                byteData = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(commandObject));

                clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Send: " + ex.Message);
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnSend: " + ex.Message);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            byte[] data = (byte[])ar.AsyncState;
            try
            {
                clientSocket.EndReceive(ar);
                
                string strData = Encoding.UTF8.GetString(data);
                
               CommandObject commandObject = JsonConvert.DeserializeObject<CommandObject>(strData);

               byteData = new byte[1024];

               clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), byteData);

                // Invoke the OnDataReceived event if one exists
                if (OnDataReceived != null)
                {
                    DataReceivedEventArgs e = new DataReceivedEventArgs();
                    e.CmdObject = commandObject;
                    OnDataReceived(this, e);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("OnReceive: " + ex.Message);
            }
        }

    }
}
