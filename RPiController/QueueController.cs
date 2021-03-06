﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPiController
{
    internal class QueueController
    {
        public event EventHandler<NewMessageEventArgs> NewMessageReceived;
        public event EventHandler<StatusChangeEventArgs> StatusChange;

        private CloudQueue _queue;

        protected void OnNewMessageReceived(string presence)
        {
            if (NewMessageReceived != null)
                NewMessageReceived(this, new NewMessageEventArgs(presence));
        }

        protected void OnStatusChange(QueueStatus newStatus)
        {
            if (StatusChange != null)
                StatusChange(this, new StatusChangeEventArgs(newStatus));
        }

        public void StartLoop()
        {
            try
            {
                _queue = GetCloudQueue(ConfigurationManager.AppSettings["AzureQueueName"]);
            }
            catch
            {
                // Maybe the network is not yet available, so try again in 5 seconds
                OnStatusChange(QueueStatus.WaitingForNetwork);
                Thread.Sleep(5000);
                StartLoop();
                return;
            }

            var lastStatus = QueueStatus.Unknown;

            try
            {
                var debugQueue = GetCloudQueue(ConfigurationManager.AppSettings["AzureDebugQueueName"]);
                var addresses = Dns.GetHostAddresses(Dns.GetHostName());
                for (int i = 0; i < addresses.Length; i++)
                {
                    var ip = addresses[i];
                    debugQueue.AddMessage(new CloudQueueMessage(string.Format("IP address ({0}/{1}): {2}", i + 1, addresses.Length, ip.ToString())), TimeSpan.FromMinutes(60));
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.ToString()); }

            while (true)
            {
                try
                {
                    if (lastStatus != QueueStatus.Ready)
                    {
                        OnStatusChange(QueueStatus.Ready);
                        lastStatus = QueueStatus.Ready;
                    }

                    var message = _queue.GetMessage();
                    if (message != null)
                    {
                        OnNewMessageReceived(message.AsString);
                        _queue.DeleteMessage(message);
                    }

                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (lastStatus != QueueStatus.Error)
                    {
                        OnStatusChange(QueueStatus.Error);
                        lastStatus = QueueStatus.Error;
                    }
                }
            }
        }

        private CloudQueue GetCloudQueue(string name)
        {
            OperationContext.GlobalSendingRequest += OperationContext_GlobalSendingRequest;

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureQueueConnectionString"]);
            CloudQueueClient cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();

            CloudQueue queueReference = cloudQueueClient.GetQueueReference(name);
            queueReference.CreateIfNotExists();
            return queueReference;
        }

        void OperationContext_GlobalSendingRequest(object sender, RequestEventArgs e)
        {
            if (e.Request.ContentLength < 0)
                e.Request.ContentLength = 0;
        }
    }

    public class NewMessageEventArgs : EventArgs
    {
        public string Presence { get; set; }

        public NewMessageEventArgs(string presence)
        {
            Presence = presence;
        }
    }

    public class StatusChangeEventArgs : EventArgs
    {
        public QueueStatus Status { get; set; }

        public StatusChangeEventArgs(QueueStatus status)
        {
            Status = status;
        }
    }

    public enum QueueStatus : int
    {
        Unknown = 0,
        Ready = 1,
        Error = 2,
        WaitingForNetwork = 3,
    }
}
