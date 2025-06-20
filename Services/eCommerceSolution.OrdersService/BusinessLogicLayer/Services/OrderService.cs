using AutoMapper;
using BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace eCommerce.ordersMicroservice.BusinessLogicLayer.Services;
public class OrdersService : IOrdersService
{

    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly UsersMicroserviceClient _usersMicroserviceClient;
    private IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;

    public OrdersService(IOrdersRepository ordersRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator, IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator, UsersMicroserviceClient usersMicroserviceClient)
    {
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _usersMicroserviceClient = usersMicroserviceClient;
        _ordersRepository = ordersRepository;
        _mapper = mapper;
    }

    public async Task<List<OrderResponse?>> GetOrders()
    {
        List<Order?> orders = (await _ordersRepository.GetOrders()).ToList();
        return _mapper.Map<List<OrderResponse?>>(orders);
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        List<Order?> orders = (await _ordersRepository.GetOrdersByCondition(filter)).ToList();
        return _mapper.Map<List<OrderResponse?>>(orders);
    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order? order = await _ordersRepository.GetOrderByCondition(filter);
        if (order is null)
        {
            return null;
        }
        return _mapper.Map<OrderResponse>(order);
    }

    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        //Validation checks
        if (orderAddRequest is null)
        {
            throw new ArgumentNullException(nameof(orderAddRequest), "OrderAddRequest cannot be null");
        }
        ValidationResult orderAddRequestValidationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!orderAddRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errors);
        }
        foreach (var orderItem in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItem);
            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemAddRequestValidationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errors);
            }
        }

        //TODO: Add logic for checking if the user exists in the Users Microservice
        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);
        if (user is null)
        {
            throw new ArgumentException($"User with ID {orderAddRequest.UserID} does not exist.");
        }

        Order orderInput = _mapper.Map<Order>(orderAddRequest);
        foreach (var orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(item => item.TotalPrice);

        Order? addedOrder = await _ordersRepository.AddOrder(orderInput);
        if (addedOrder is null)
        {
            return null;
        }
        return _mapper.Map<OrderResponse>(addedOrder);
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        //Validation checks
        if (orderUpdateRequest is null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest), "OrderUpdateRequest cannot be null");
        }
        ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errors);
        }
        foreach (var orderItem in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItem);
            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errors);
            }
        }

        //TODO: Add logic for checking if the user exists in the Users Microservice
        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);
        if (user is null)
        {
            throw new ArgumentException($"User with ID {orderUpdateRequest.UserID} does not exist.");
        }

        Order orderInput = _mapper.Map<Order>(orderUpdateRequest);
        foreach (var orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(item => item.TotalPrice);

        Order? updatedOrder = await _ordersRepository.UpdateOrder(orderInput);
        if (updatedOrder is null)
        {
            return null;
        }
        return _mapper.Map<OrderResponse>(updatedOrder);
    }

    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        Order? existingOrder = (await _ordersRepository.GetOrderByCondition(filter));
        if (existingOrder is null)
        {
            return false;
        }
        return await _ordersRepository.DeleteOrder(orderID);
    }
}