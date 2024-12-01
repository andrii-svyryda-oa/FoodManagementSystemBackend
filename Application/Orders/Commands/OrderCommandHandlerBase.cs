﻿using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.OrderItems.Exceptions;
using Application.Orders.Exceptions;
using Domain.OrderItems;
using Domain.Orders;
using Domain.Users;
using MediatR;

namespace Application.OrderItems.Commands;

public class OrderCommandHandlerBase(
    IUserRepository userRepository,
    IOrderRepository orderRepository)
{
    protected async Task<Result<Order, OrderException>> ReadDbOrder(
        OrderId orderId,
        UserId authorId,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetById(orderId, cancellationToken);

        return await order.Match(
            async order =>
            {
                var author = await userRepository.GetById(authorId, cancellationToken);

                return author.Match<Result<Order, OrderException>>(
                    author => {
                        if (author.Role == UserRole.Admin)
                        {
                            return order;
                        }

                        if (author.Id == order.OwnerId)
                        {
                            return order;
                        }

                        return new OrderOperationForbiddenException(orderId);
                    },
                    () => new OrderAuthorNotFoundException(authorId));
            },
            () => Task.FromResult<Result<Order, OrderException>>(new OrderNotFoundException(orderId)));
    }
}