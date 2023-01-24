namespace Taxually.TechnicalTest
{
    public class TaxuallyQueueClient
    {
        public virtual async Task EnqueueAsync<TPayload>(string queueName, TPayload payload)
        {
            // Code to send to message queue removed for brevity
            return;
        }
    }
}
