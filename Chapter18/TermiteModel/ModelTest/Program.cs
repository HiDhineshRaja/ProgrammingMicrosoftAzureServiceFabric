﻿using Box.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Threading;
using Termite.Interfaces;

namespace ModelTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rand = new Random();
            int size = 100;
            int termites = 175;
            IBox boxClient = ServiceProxy.Create<IBox>(new Uri("fabric:/TermiteModel/Box"), new ServicePartitionKey(0));
            boxClient.ResetBox().Wait();

            ITermite[] proxies = new ITermite[termites];
            for (int i = 0; i < proxies.Length; i++)
            {
                proxies[i] = ActorProxy.Create<ITermite>(new ActorId(i), new Uri("fabric:/TermiteModel/TermiteActorService"));
                proxies[i].GetStateAsync();
            }

            Console.BackgroundColor = ConsoleColor.White;
            while (true)
            {
                var box = boxClient.ReadBox().Result;
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                for (int y = 0; y < size; y++)
                {
                    Console.CursorTop = y;
                    for (int x = 0; x < size; x++)
                    {
                        Console.CursorLeft = x;
                        if (box[y * size + x] == 0)
                            Console.Write(" ");
                        else
                            Console.Write("#");
                    }
                }
                Console.ForegroundColor = ConsoleColor.DarkRed;
                for (int i = 0; i < proxies.Length; i++)
                {
                    var state = proxies[i].GetStateAsync().Result;
                    Console.CursorLeft = state.X;
                    Console.CursorTop = state.Y;
                    Console.Write("T");
                }
                Thread.Sleep(500);
            }
        }
    }
}
