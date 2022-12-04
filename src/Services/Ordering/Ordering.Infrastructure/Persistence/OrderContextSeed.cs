using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Entities;
using Ordering.Domain.Enums;
using Serilog;
namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        private readonly ILogger _logger;
        private readonly OrderContext _orderContext;
        public OrderContextSeed(ILogger logger, OrderContext orderContext)
        {
            _logger = logger;
            _orderContext = orderContext;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                if (_orderContext.Database.IsSqlServer())
                {
                    await _orderContext.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            if (!_orderContext.Orders.Any())
            {
                await _orderContext.Orders.AddRangeAsync(
                    new Order
                    {
                        UserName = "customer1",
                        FirstName = "customer1",
                        LastName = "customer",
                        EmailAddress = "customer1@local.com",
                        ShippingAddress = "Wollongong",
                        InvoiceAddress = "Australia",
                        TotalPrice = 250
                    },
                    new Order
                    {
                        UserName = "customer2",
                        FirstName = "customer2",
                        LastName = "customer",
                        EmailAddress = "customer2@local.com",
                        ShippingAddress = "Wollongong",
                        InvoiceAddress = "Australia",
                        TotalPrice = 250
                    });
            }
        }


        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
                await _orderContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
