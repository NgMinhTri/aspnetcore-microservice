using AutoMapper;
using Basket.API.Entities;
using EvenBus.Messages.Events;
using Shared.DTOs.Basket;

namespace Basket.API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<BasketCheckout, BasketCheckoutEvent>();
            CreateMap<CartDto, Cart>().ReverseMap();
            CreateMap<Cart, CartDto>().ReverseMap();
        }
    }
}
