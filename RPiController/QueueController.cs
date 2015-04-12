using Microsoft.WindowsAzure.Storage;
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
                _queue = GetCloudQueue();
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
                catch
                {
                    if (lastStatus != QueueStatus.Error)
                    {
                        OnStatusChange(QueueStatus.Error);
                        lastStatus = QueueStatus.Error;
                    }
                }
            }
        }

        private CloudQueue GetCloudQueue()
        {
            OperationContext.GlobalSendingRequest += OperationContext_GlobalSendingRequest;
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureQueueConnectionString"]);
            CloudQueueClient cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();

            CloudQueue queueReference = cloudQueueClient.GetQueueReference(ConfigurationManager.AppSettings["AzureQueueName"]);
            queueReference.CreateIfNotExists();
            return queueReference;
        }

        void OperationContext_GlobalSendingRequest(object sender, RequestEventArgs e)
        {
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
