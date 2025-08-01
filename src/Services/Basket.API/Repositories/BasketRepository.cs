﻿using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Basket.API.Services;
using Basket.API.Services.Interfaces;
using Contracts.Common.Interfaces;
using Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs.ScheduledJob;
using ILogger = Serilog.ILogger;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _redisCacheService;
        private readonly ISerializeService _serializeService;
        private readonly ILogger _logger;
        private readonly BackgroundJobHttpService _backgroundJobHttpService;
        private readonly IEmailTemplateService _emailTemplateService;

        public BasketRepository(IDistributedCache redisCacheService,
                                ISerializeService serializeService,
                                ILogger logger,
                                BackgroundJobHttpService backgroundJobHttpService,
                                IEmailTemplateService emailTemplateService)
        {
            _redisCacheService = redisCacheService;
            _serializeService = serializeService;
            _logger = logger;
            _backgroundJobHttpService = backgroundJobHttpService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task<Cart?> GetBasketByUserName(string userName)
        {
            var basket = await _redisCacheService.GetStringAsync(userName);
            return string.IsNullOrEmpty(basket) ? null : _serializeService.Deserialize<Cart>(basket);
        }

        public async Task<Cart?> UpdateBasket(Cart cart, DistributedCacheEntryOptions options = null)
        {
            await DeleteBasketFromUserName(cart.Username);
            if (options != null)
                await _redisCacheService.SetStringAsync(cart.Username,
                    _serializeService.Serialize(cart), options);
            else
                await _redisCacheService.SetStringAsync(cart.Username,
                    _serializeService.Serialize(cart));

            try
            {
                await TriggerSendEmailReminderCheckout(cart);
            }
            catch (Exception)
            {

                throw;
            }
            return await GetBasketByUserName(cart.Username);
        }

        public async Task<bool> DeleteBasketFromUserName(string username)
        {
            await DeleteBasketFromUserName(username);
            try
            {
                await _redisCacheService.RemoveAsync(username);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("Error DeleteBasketFromUserName: " + e.Message);
                throw;
            }
        }


        private async Task TriggerSendEmailReminderCheckout(Cart cart)
        {
            var emailTemplate = _emailTemplateService.GenerateReminderCheckoutOrderEmail(cart.Username);
            var model = new ReminderCheckoutOrderDto(cart.EmailAddress,
                                                     "Reminder checkout",
                                                     emailTemplate,
                                                     DateTimeOffset.UtcNow.AddSeconds(30));
            var uri = $"{_backgroundJobHttpService.ScheduledJobUrl}/send-email-reminder-checkout-order";
            var response = await _backgroundJobHttpService._client.PostAsJson(uri, model);
            if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
            {
                var jobId = await response.ReadContentAs<string>();
                if(!string.IsNullOrEmpty(jobId))
                {
                    cart.JobId = jobId;
                    await _redisCacheService.SetStringAsync(cart.Username, _serializeService.Serialize(cart));
                }
            }
        }

        private async Task DeleteReminderCheckoutOrder(string username)
        {
            var cart = await GetBasketByUserName(username);
            if (cart == null || string.IsNullOrEmpty(cart.JobId)) return;

            var jobId = cart.JobId;
            var uri = $"{_backgroundJobHttpService.ScheduledJobUrl}/delete/jobId/{jobId}";
            await _backgroundJobHttpService._client.DeleteAsync(uri);
            _logger.Information($"DeleteReminderCheckoutOrder: Deleted JobId: {jobId}");

        }


    }
}
