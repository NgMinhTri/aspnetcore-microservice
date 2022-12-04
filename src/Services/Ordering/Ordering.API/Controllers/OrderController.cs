using Contracts.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders;
using Shared.Services.Email;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Ordering.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IEmailSmtpService _emailSmtpService;
        public OrderController(IMediator mediator, IEmailSmtpService emailSmtpService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _emailSmtpService = emailSmtpService;
        }

        private static class RouteNames
        {
            public const string GetOrders = nameof(GetOrders);
        }

        [HttpGet("{username}", Name = RouteNames.GetOrders)]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrderByUserName([Required] string username)
        {
            var query = new GetOrdersQuery(username);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        //[HttpGet("test-email")]
        //public async Task<IActionResult> TestEmail()
        //{
        //    var message = new MailRequest
        //    {
        //        Body = "<h1> Hello Tri",
        //        Subject = "Test Email",
        //        ToAddress = "tringuyenm4@nashtechglobal.com"
        //    };
        //    await _emailSmtpService.SendEmailAsync(message);
        //    return Ok();

        //}

    }
}
