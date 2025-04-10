using AdminProvider.OrdersManagement.Interfaces;
using AdminProvider.OrdersManagement.Models.DTOs;
using AdminProvider.OrdersManagement.Models.Responses;
using AdminProvider.UsersManagement.Data.Entities;
using AdminProvider.UsersManagement.Factories;
using AdminProvider.UsersManagement.Interfaces;
using AdminProvider.UsersManagement.Models.DTOs;
using AdminProvider.UsersManagement.Models.Requests;

namespace AdminProvider.UsersManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<UserService> _logger;
    public UserService(IUserRepository userRepository, IOrderRepository orderRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<(List<UserDto> Users, int TotalCount, int CompanyCount)> GetAllUsers(int pageNumber, int pageSize)
    {
        var (users, totalCount, companyCount) = await _userRepository.GetAllUsersAsync(pageNumber, pageSize);

        var usersDto = UsersDtoFactory.CreateList(users);

        // Get all customer IDs from users to optimize fetching order counts
        var customerIds = usersDto.Select(u => u.UserId).ToList();

        // Fetch all order counts for these customer IDs in bulk
        var orderCounts = await _orderRepository.GetOrderCountsForCustomerIdsAsync(customerIds);

        // Assign the order count to each user DTO
        foreach (var userDto in usersDto)
        {
            if (orderCounts.ContainsKey(userDto.UserId))
            {
                userDto.OrderCount = orderCounts[userDto.UserId];
            }
            else
            {
                userDto.OrderCount = 0; // No orders found for this customer
            }
        }

        return (usersDto, totalCount, companyCount);
    }


    public async Task<List<UserDto>> GetUsersByQueryAsync(string searchQuery)
    {
        // Get the list of UserEntity objects based on the search query
        var users = await _userRepository.GetUsersByQueryAsync(searchQuery);

        // Convert UserEntity objects to UsersDto
        var usersDto = users.Select(UsersDtoFactory.Create).ToList();

        return usersDto;
    }

    public async Task<UserDetailsDto> GetUserAsync(string userId)
    {
        // Try to parse the userId to a Guid
        if (Guid.TryParse(userId, out var userGuid))
        {
            // Fetch the user details from the repository
            var user = await _userRepository.GetByIdAsync(userGuid);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Fetch the order count for the user
            var orderCount = await GetOrderCountByCustomerIdAsync(userGuid.ToString());

            // Convert the user entity to a UserDetailsDto
            var userDetailsDto = UserDetailsDtoFactory.Create(user);

            // Add the order count to the user details DTO
            userDetailsDto.OrderCount = orderCount;

            return userDetailsDto;
        }

        throw new ArgumentException("Invalid user ID format", nameof(userId));
    }

    public async Task<List<OrderDto>> GetOrdersByCustomerIdAsync(string customerId)
    {
        if (!Guid.TryParse(customerId, out Guid customerGuid))
        {
            throw new ArgumentException("Invalid customer ID format", nameof(customerId));
        }

        var orderEntities = await _orderRepository.GetByCustomerIdAsync(customerGuid);

        return orderEntities.Select(o => new OrderDto
        {
            OrderId = o.OrderId,
            CustomerId = o.CustomerId,
            CreatedAt = o.CreatedAt,
            Quantity = o.Quantity,
            // map more fields if needed
        }).ToList();
    }

    public async Task<int> GetOrderCountByCustomerIdAsync(string customerId)
{
    if (!Guid.TryParse(customerId, out Guid customerGuid))
    {
        throw new ArgumentException("Invalid customer ID format", nameof(customerId));
    }

    return await _orderRepository.GetCountByCustomerIdAsync(customerGuid);
}


    public async Task<string> UpdateAdminNote(UserNoteUpdateRequest userNoteUpdateRequest)
    {
        if (userNoteUpdateRequest == null || userNoteUpdateRequest.UserId == null || userNoteUpdateRequest.AdminNote == null)
        {
            _logger.LogError("There is nothing to update.");
        }
        var userId = userNoteUpdateRequest.UserId;
        string adminNote = userNoteUpdateRequest.AdminNote;

        var updatedNote = await _userRepository.UpdateAdminNoteAsync(userId, adminNote);

        return updatedNote;
    }

    public async Task DeleteUserAsync(UserRequest userRequest)
    {
        if (userRequest == null)
        {
            throw new ArgumentNullException(nameof(userRequest), "User request cannot be null.");
        }

        if (!Guid.TryParse(userRequest.SearchQuery, out Guid userId))
        {
            throw new ArgumentException("Invalid User ID format.", nameof(userRequest.SearchQuery));
        }

        await _userRepository.DeleteOneAsync(userId);
    }
}
