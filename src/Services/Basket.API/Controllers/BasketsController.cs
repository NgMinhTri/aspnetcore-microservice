using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcService;
using Basket.API.Repositories.Interfaces;
using Basket.API.Services.Interfaces;
using EvenBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.ComponentModel.DataAnnotations;
using System.Net;
using ILogger = Serilog.ILogger;

namespace Basket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketsController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly StockItemGrpcService _stockItemGrpcService;

        public BasketsController(IBasketRepository basketRepository, 
                                IMapper mapper, 
                                IPublishEndpoint publishEndpoint,
                                StockItemGrpcService stockItemGrpcService)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _stockItemGrpcService = stockItemGrpcService ?? throw new ArgumentNullException(nameof(stockItemGrpcService));
        }

        [HttpGet("{username}", Name = "GetBasket")]
        [ProducesResponseType(typeof(Cart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Cart>> GetBasket([Required] string username)
        {
            //_logger.Information($"BEGIN: GetBasketByUserName {username}");
            var result = await _basketRepository.GetBasketByUserName(username);
            //_logger.Information($"END: GetBasketByUserName {username}");

            return Ok(result ?? new Cart(username));
        }

        [HttpPost(Name = "UpdateBasket")]
        [ProducesResponseType(typeof(Cart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Cart>> UpdateBasket([FromBody] Cart cart)
        {
            // Communicate with Inventory.Product.Grpc and check quantity available of products
            foreach (var item in cart.Items)
            {
                var stock = await _stockItemGrpcService.GetStock(item.ItemNo);
                item.SetAvailableQuantity(stock.Quantity);
            }

            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.UtcNow.AddMinutes(10))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            var result = await _basketRepository.UpdateBasket(cart, options);
            return Ok(result);
        }

        [HttpDelete("{username}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<bool>> DeleteBasket([Required] string username)
        {
            _logger.Information($"BEGIN: DeleteBasket {username}");
            var result = await _basketRepository.DeleteBasketFromUserName(username);
            _logger.Information($"END: DeleteBasket {username}");
            return Ok(result);
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            var basket = await _basketRepository.GetBasketByUserName(basketCheckout.UserName);
            if (basket == null) return NotFound();

            //publish checkout event to EventBus Message
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;
            await _publishEndpoint.Publish(eventMessage);
            //remove the basket
            await _basketRepository.DeleteBasketFromUserName(basket.Username);

            return Accepted();
        }


        
    }
}
