﻿using System.Net.Http.Json;
using Api.Dtos;
using Docker.DotNet.Models;
using Domain.Orders;
using Domain.Restaurants;
using Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Xunit;

namespace Api.Tests.Integration.Orders;

public class OrdersControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly Restaurant _mainRestaurant;
    private readonly User _mainUser;
    private readonly Order _mainOrder;

    public OrdersControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _mainRestaurant = RestaurantsData.MainRestaurant();
        _mainUser = UsersData.MainUser("TestPassword123!");
        _mainOrder = OrdersData.MainOrder(_mainRestaurant.Id, _mainUser.Id);
    }

    [Fact]
    public async Task ShouldGetAllOrders()
    {
        // Arrange
        SetTestUser(_mainUser.Id.ToString(), _mainUser.Role.ToString());

        // Act
        var response = await Client.GetAsync("orders");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders!.Count.Should().Be(1);

        var order = orders.First();
        order.Id.Should().Be(_mainOrder.Id.Value);
        order.Name.Should().Be(_mainOrder.Name);
        order.OwnerId.Should().Be(_mainUser.Id.Value);
        order.RestaurantId.Should().Be(_mainRestaurant.Id.Value);
    }

    [Fact]
    public async Task ShouldGetOrderById()
    {
        // Arrange
        SetTestUser(_mainUser.Id.ToString(), _mainUser.Role.ToString());

        var orderId = _mainOrder.Id.Value;

        // Act
        var response = await Client.GetAsync($"orders/{orderId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Id.Should().Be(_mainOrder.Id.Value);
        order.Name.Should().Be(_mainOrder.Name);
        order.OwnerId.Should().Be(_mainUser.Id.Value);
        order.RestaurantId.Should().Be(_mainRestaurant.Id.Value);
    }

    [Fact]
    public async Task ShouldCreateOrder()
    {
        // Arrange
        SetTestUser(_mainUser.Id.ToString(), _mainUser.Role.ToString());

        var request = new OrderDto(
            Id: null,
            Name: "New Order",
            OwnerId: _mainUser.Id.Value,
            CreatedAt: null,
            RestaurantId: _mainRestaurant.Id.Value,
            Restaurant: null,
            State: OrderState.Opened,
            Items: null);

        // Act
        var response = await Client.PostAsJsonAsync("orders", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
        responseOrder.Should().NotBeNull();
        var orderId = new OrderId(responseOrder!.Id!.Value);

        var dbOrder = await Context.Orders.FirstAsync(x => x.Id == orderId);
        dbOrder.Name.Should().Be(request.Name);
        dbOrder.OwnerId.Value.Should().Be(_mainUser.Id.Value);
        dbOrder.RestaurantId.Value.Should().Be(_mainRestaurant.Id.Value);
    }

    [Fact]
    public async Task ShouldUpdateOrder()
    {
        // Arrange
        SetTestUser(_mainUser.Id.ToString(), _mainUser.Role.ToString());

        var updatedName = "Updated Order Name";
        var updatedState = OrderState.Closed;
        var request = new OrderDto(
            Id: _mainOrder.Id.Value,
            Name: updatedName,
            OwnerId: _mainUser.Id.Value,
            CreatedAt: _mainOrder.CreatedAt,
            RestaurantId: _mainRestaurant.Id.Value,
            Restaurant: null,
            State: updatedState,
            Items: null);

        // Act
        var response = await Client.PutAsJsonAsync("orders", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var updatedOrder = await Context.Orders.FirstAsync(x => x.Id == _mainOrder.Id);
        updatedOrder.Name.Should().Be(updatedName);
        updatedOrder.State.Should().Be(updatedState);
    }

    [Fact]
    public async Task ShouldDeleteOrder()
    {
        // Arrange
        SetTestUser(_mainUser.Id.ToString(), _mainUser.Role.ToString());

        var testOrder = Order.New(OrderId.New(), _mainUser.Id, "OBID", _mainRestaurant.Id);
        await Context.Orders.AddAsync(testOrder);
        await SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"orders/{testOrder.Id.Value}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var orderExists = await Context.Orders.AnyAsync(x => x.Id == testOrder.Id);
        orderExists.Should().BeFalse();
    }

    public async Task InitializeAsync()
    {
        await Context.Restaurants.AddAsync(_mainRestaurant);
        await Context.Users.AddAsync(_mainUser);
        await Context.Orders.AddAsync(_mainOrder);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Orders.RemoveRange(Context.Orders);
        Context.Users.RemoveRange(Context.Users);
        Context.Restaurants.RemoveRange(Context.Restaurants);
        await SaveChangesAsync();
    }
}