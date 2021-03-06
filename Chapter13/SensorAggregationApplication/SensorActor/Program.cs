﻿using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Threading;

namespace SensorActor
{
    internal static class Program
    {
        /// <summary>
        /// これは、サービス ホスト プロセスのエントリ ポイントです。
        /// </summary>
        private static void Main()
        {
            try
            {
                // この行は、Service Fabric ランタイムにアクター クラスをホストするアクター サービスを登録します。
                // ServiceManifest.xml ファイルと ApplicationManifest.xml ファイルのコンテンツ
                // は、このプロジェクトをビルドするときに自動的に設定されます。
                //詳細については、https://aka.ms/servicefabricactorsplatform を参照してください

                ActorRuntime.RegisterActorAsync<SensorActor>(
                   (context, actorType) => new ActorService(context, actorType)).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
