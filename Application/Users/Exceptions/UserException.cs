﻿using Domain.Users;

namespace Application.Users.Exceptions;

public abstract class UserException(UserId id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public UserId UserId { get; } = id;
}

public class UserNotFoundException(UserId id)
    : UserException(id, $"User with id: {id} not found");

public class UserAlreadyExistsException(UserId id)
    : UserException(id, $"User with id: {id} already exists");

public class UserUnknownException(UserId id, Exception innerException)
    : UserException(id, $"Unknown exception for the User with id: {id}", innerException);

public class UserUnknownBalanceHistoryException(UserId id, Exception innerException)
    : UserException(id, $"Unknown exception for the User Balance History with id: {id}", innerException);
