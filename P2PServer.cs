﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace kTmCoin
{
    public class P2PServer : WebSocketBehavior
    {
        private bool _chainSynched;
        private WebSocketServer _wss;

        public void Start()
        {
            _wss = new WebSocketServer($"ws://127.0.0.1:{Program.Port}");
            _wss.AddWebSocketService<P2PServer>("/Blockchain");
            _wss.Start();
            Console.WriteLine($"Started server at ws://127.0.0.1:{Program.Port}");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data == "Hi Server")
            {
                Console.WriteLine(e.Data);
                Send("Hi Client");
            }
            else
            {
                Blockchain newChain = JsonConvert.DeserializeObject<Blockchain>(e.Data);

                if (newChain.IsValid() && newChain.Chain.Count > Program.kTmCoin.Chain.Count)
                {
                    List<Transaction> newTransactions = new List<Transaction>();
                    newTransactions.AddRange(newChain.PendingTransactions);
                    newTransactions.AddRange(Program.kTmCoin.PendingTransactions);

                    newChain.PendingTransactions = newTransactions;
                    Program.kTmCoin = newChain;
                }

                if (!_chainSynched)
                {
                    Send(JsonConvert.SerializeObject(Program.kTmCoin));
                    _chainSynched = true;
                }
            }
        }
    }
}