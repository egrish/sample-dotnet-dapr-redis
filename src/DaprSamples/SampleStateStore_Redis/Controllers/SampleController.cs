using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleStateStore_Redis.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SampleStateStore_Redis.Controllers
{
    public class SampleController : ControllerBase
    {

        public const string StoreName = "statestore";
        private readonly ILogger<SampleController> logger;

        public SampleController(ILogger<SampleController> logger)
        {
            this.logger = logger;
        }


        //[Topic("pubsub", "client")]
        [HttpPost("addClient")]
        public async Task<ActionResult<string>> AddClient([FromBody] Client client, [FromServices] DaprClient daprClient)
        {
            logger.LogDebug($"Adding client with id {client?.Id} and first name {client?.FirstName}");

            if (client is null)
                return new BadRequestObjectResult(new { error = $"The client is null" });

            var state = await daprClient.GetStateEntryAsync<Client>(StoreName, client.Id);

           
            if (state.Value != null )
            {
                logger.LogError($"The client with id {client.Id} already exists");
                return new BadRequestObjectResult(new { error = $"The client with id {client.Id} already exists" });
            };

            state.Value = new Client() { Id = client.Id, FirstName = client.FirstName, LastName = client.LastName };


            await state.SaveAsync();
            return state.Value.Id;
        }


        //[Topic("pubsub", "client")]
        [HttpGet("getClient/{id}")]
        public async Task<ActionResult<Client>> GetClient(string id, [FromServices] DaprClient daprClient)
        {
            logger.LogDebug($"Retrieving client with id {id}");

            if (string.IsNullOrEmpty(id) )
                return new BadRequestObjectResult(new { error = $"The client id is null" });

            var state = await daprClient.GetStateEntryAsync<Client>(StoreName, id);


            if (state.Value == null)
            {
                logger.LogError($"The client with id {id} does not exists");
                return new BadRequestObjectResult(new { error = $"The client with id {id} does not exists" });
            };

          
            return state.Value;
        }


        //[Topic("pubsub", "client")]
        [HttpDelete("deleteClient/{id}")]
        public async Task<ActionResult<bool>> DeleteClient(string id, [FromServices] DaprClient daprClient)
        {
            logger.LogDebug($"Deleting client with id {id}");

            if (string.IsNullOrEmpty(id))
                return new BadRequestObjectResult(new { error = $"The client id is null" });

           

            try
            {
                await daprClient.DeleteStateAsync(StoreName, id);
                return new OkObjectResult(new { status = $"The client with id {id} has been deleted" });
            }
            catch (Exception)
            {
                return new BadRequestObjectResult(new { error = $"The client with id { id } has NOT been deleted" });
            }
            
           
        }

        /// <summary>
        /// Method for returning a BadRequest result which will cause Dapr sidecar to throw an RpcException
        [HttpPost("throwException")]
        public async Task<ActionResult<Client>> ThrowException(Client client, [FromServices] DaprClient daprClient)
        {
            Console.WriteLine("Enter ThrowException");
            var task = Task.Delay(10);
            await task;
            return BadRequest(new { statusCode = 400, message = "bad request" });
        }

    }
}
