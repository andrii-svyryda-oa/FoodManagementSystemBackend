﻿using Application.OrderItems.Exceptions;
using Application.Orders.Exceptions;
using Application.Restaurants.Exceptions;
using Application.Users.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class OrderItemErrorHandler
{
    public static ObjectResult ToObjectResult(this OrderItemException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                OrderItemNotFoundException or 
                    OrderItemOrderNotFoundException or 
                    OrderItemUserNotFoundException => StatusCodes.Status404NotFound,
                OrderItemAuthorNotFoundException or
                    OrderItemOperationForbiddenException => StatusCodes.Status403Forbidden,
                OrderItemOrderAlreadyClosedException => StatusCodes.Status405MethodNotAllowed,
                OrderItemUnknownException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("User error handler does not implemented")
            }
        };
    }
}