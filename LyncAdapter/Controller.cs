using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyncAdapter
{
    public class Controller
    {
        private bool _isInitialized = false;
        private LyncManager _lyncManager;
        private CloudQueue _cloudQueue;

        public void Setup()
        {
            if (!_isInitialized)
            {
                _cloudQueue = GetCloudQueue();

                _lyncManager = new LyncManager();
                _lyncManager.AvailabilityChanged += _lyncManager_AvailabilityChanged;
                _lyncManager.Setup();

                _isInitialized = true;
            }
        }

        private void _lyncManager_AvailabilityChanged(object sender, AvailabilityEventArgs e)
        {
            AddStatusToQueue(e.Availability.AvailabilityName);
        }

        private void AddStatusToQueue(string newStatus)
        {
            _cloudQueue.AddMessage(new CloudQueueMessage(newStatus), TimeSpan.FromMinutes(5));
        }

        private CloudQueue GetCloudQueue()
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureQueueConnectionString"]);
            CloudQueueClient cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
            CloudQueue queueReference = cloudQueueClient.GetQueueReference(ConfigurationManager.AppSettings["AzureQueueName"]);
            queueReference.CreateIfNotExists();
            return queueReference;
        }
    }
}
