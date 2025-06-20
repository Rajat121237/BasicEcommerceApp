using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Order> _ordersCollection;
    private readonly string _collectionName = "orders";

    public OrdersRepository(IMongoDatabase mongoDatabase)
    {
        _ordersCollection = mongoDatabase.GetCollection<Order>(_collectionName);
    }

    public async Task<Order?> AddOrder(Order order)
    {
        order.OrderID = Guid.NewGuid();
        order._id = order.OrderID; 
        foreach (OrderItem item in order.OrderItems)
        {
            item._id = Guid.NewGuid(); 
        }
        await _ordersCollection.InsertOneAsync(order);
        return order;
    }

    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(o => o.OrderID, orderID);
        return (await _ordersCollection.DeleteOneAsync(filter)).DeletedCount > 0;
    }

    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        return (await _ordersCollection.FindAsync(filter)).FirstOrDefault();
    }

    public async Task<IEnumerable<Order?>> GetOrders()
    {
        return (await _ordersCollection.FindAsync(Builders<Order>.Filter.Empty)).ToList();
    }

    public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        return (await _ordersCollection.FindAsync(filter)).ToList();
    }

    public async Task<Order?> UpdateOrder(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, order.OrderID);
        Order? existingOrder = (await _ordersCollection.FindAsync(filter)).FirstOrDefault();
        if (existingOrder == null) return null;
        order._id = existingOrder._id;
        ReplaceOneResult replaceOneResult = await _ordersCollection.ReplaceOneAsync(filter, order);
        return order;
    }
}
