﻿using IoTHubParitionMap.Interfaces;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace IoTHubParitionMap
{
internal class IoTHubParitionMap : StatefulActor<IoTHubParitionMap.ActorState>, IIoTHubPartitionMap
{
    IActorTimer mTimer;

    [DataContract]
    internal sealed class ActorState
    {
        [DataMember]
        public List<string> PartitionNames { get; set; }
        [DataMember]
        public Dictionary<string, DateTime> PartitionLeases { get; set; }
    }
    protected override Task OnActivateAsync()
    {
        if (this.State == null)
        {
            this.State = new ActorState { PartitionNames = new List<string>(), PartitionLeases = new Dictionary<string, DateTime>() };
            ResetPartitionNames();
            mTimer = RegisterTimer(CheckLease, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)); 
        }
        return Task.FromResult(true);
    }
    private Task CheckLease(Object state)
    {
        List<string> keys = this.State.PartitionLeases.Keys.ToList();

        foreach(string key in keys)
        {
            if (DateTime.Now - this.State.PartitionLeases[key] >= TimeSpan.FromSeconds(60))
                this.State.PartitionLeases.Remove(key);
        }
        return Task.FromResult(1);
    }
    protected override Task OnDeactivateAsync()
    {
        if (mTimer != null)
            UnregisterTimer(mTimer);
        return base.OnDeactivateAsync();
    }

Task<string> IIoTHubPartitionMap.LeaseTHubPartitionAsync()
    {
        string ret = "";
        foreach(string partition in this.State.PartitionNames)
        {
            if (!this.State.PartitionLeases.ContainsKey(partition))
            {
                this.State.PartitionLeases.Add(partition, DateTime.Now);
                ret = partition;
                break;
            }
        }
        return Task.FromResult(ret);

    }
    Task<string> IIoTHubPartitionMap.RenewIoTHubPartitionLeaseAsync(string partition)
    {
        string ret = "";
        if (this.State.PartitionLeases.ContainsKey(partition))
        {
            this.State.PartitionLeases[partition] = DateTime.Now;
            ret = partition;
        }
        return Task.FromResult(ret);
    }
    void ResetPartitionNames()
    {
        var eventHubClient = EventHubClient.CreateFromConnectionString("HostName=iote2e.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=++Fev//TfVOkRUYa4s2bIJA4gad2UvPHQ2K+razIYP8=", "messages/events");
        var partitions = eventHubClient.GetRuntimeInformation().PartitionIds;
        foreach (string partition in partitions)
        {
            this.State.PartitionNames.Add(partition);
        }
    }
}
}
