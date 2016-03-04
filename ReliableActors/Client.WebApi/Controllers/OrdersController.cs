namespace Client.WebApi.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Mvc;
    using Microsoft.AspNet.Http.Extensions;
    using Microsoft.ServiceBus.Messaging;
    using Models;
    using Contracts;
    using Contracts.Orders;
    

    [Route("api/client/orders")]
    public class OrdersController : Controller
    {
        private readonly QueueClient queue_client;

        public OrdersController(QueueClient queueClient)
        {
            if (queueClient == null)
                throw new ArgumentNullException(nameof(queueClient));

            queue_client = queueClient;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            var orderId = Guid.NewGuid();
            var command = new BrokeredMessage(new PlaceOrder(orderId, order.CustomerId, order.BagId).ToJson())
            {
                ContentType = "application/json"
            };

            await queue_client.SendAsync(command);
            
            return Created(new Uri($"{Request.GetEncodedUrl()}/{orderId}"), null);
        }
    }
}

