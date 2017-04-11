﻿using Common.Lib.Models;
using Player.Interfaces;
using Player.Models;
using System;
using System.Net;

namespace Player.Presenters
{
    class StartGamePresenter : IStartGamePresenter
    {
        private IStartGameModel model;
        private IStartGameView view;
        private DiscoveryClient discoveryClient;
        private Client client;

        public StartGamePresenter(IStartGameModel model, IStartGameView view)
        {
            this.model = model;
            this.view = view;

            model.player = new Common.Lib.Models.Player();

            view.StartGameModel = model;
            view.StartGamePresenter = this;

            client = new Client();

            discoveryClient = new DiscoveryClient();
            discoveryClient.OnDataReceived += discoveryClient_OnDataReceived;
            discoveryClient.Start();
        }

        private void client_OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected");
        }

        private void discoveryClient_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            client.OnConnected += client_OnConnected;
            client.OnDataReceived += client_OnDataReceived;
            //this ipaddress is from the server
            string ipaddress = e.Data.Split(' ')[1].Split(':')[0];
            int port = int.Parse(e.Data.Split(' ')[1].Split(':')[1]);


            string tempIP ="";
            //this ipAddress is from this host
            foreach (IPAddress thisIPAddress in Dns.GetHostEntry(string.Empty).AddressList)
            {
                if (thisIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    Console.WriteLine("This host's IP Address: " + thisIPAddress.ToString());
                    tempIP = thisIPAddress.ToString();
                }
            }
            Console.WriteLine("Dealer IP Address: " + ipaddress);
            if (ipaddress.ToString() == tempIP)
                Console.WriteLine("I am the dealer - Start Game");
            else
                Console.WriteLine("I am not the dealer - Join Game");



            client.Connect(ipaddress, port);
            
            //client.Send(new CommandObject() { Command = Command.Join, Payload = new Common.Lib.Models.Player() { Name = "Tim" } });
        }
        private void client_OnDataReceived(object sender, ClientDataReceivedEventArgs e)
        {
            Console.WriteLine(e.CmdObject.Response.ToString());
        }

        public void OnButton1Click()
        {
            IInGameModel inGameModel = new InGameModel();
            inGameModel.player = model.player;
            IInGameView inGameView = new InGameView(inGameModel);

            IInGamePresenter inGamePresenter = new InGamePresenter(inGameModel, inGameView, client);
            client.OnDataReceived += inGamePresenter.client_OnDataReceived;
            inGamePresenter.ShowDialog();
        }

        public void goButtonClick()
        {
            CommandObject cmdObj = new CommandObject();
            cmdObj.Command = Command.Join;
            cmdObj.Payload = model.player;
            client.Send(cmdObj);
        }
    }
}
