﻿using Domain.Orders;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IOrderQueries
{
    Task<IReadOnlyList<Order>> GetByUserId(UserId userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetAll(CancellationToken cancellationToken);
    Task<Option<Order>> GetById(OrderId orderId, CancellationToken cancellationToken);
}
