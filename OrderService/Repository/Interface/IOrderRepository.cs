using Microsoft.AspNetCore.Mvc;

namespace OrderService.Repository.Interface
{
    public interface IOrderRepository
    {
        Task<List<OrderModel>> GetOrder(string UserName);
        Task<bool> AddOrder(OrderModel order);
        Task<Tuple<bool, string>> IsAuthenticated(string username, string password);
        Task<bool> InsertToken(string username, string token, DateTime expiryDate);
    }
}
