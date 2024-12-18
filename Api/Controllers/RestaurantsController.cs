﻿using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Restaurants.Commands;
using Domain.Restaurants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("restaurants")]
[ApiController]
public class RestaurantsController(ISender sender, IRestaurantQueries restaurantQueries) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PaginatedData<RestaurantDto>>> GetAll(
        [FromQuery] int pageSize,
        [FromQuery] int page,
        CancellationToken cancellationToken,
        [FromQuery] string searchText = "")
    {
        var skip = (page - 1) * pageSize;

        var (entities, count) = await restaurantQueries.GetAll(skip, pageSize, searchText, cancellationToken);

        return new PaginatedData<RestaurantDto>(entities.Select(RestaurantDto.FromDomainModel).ToList(), count);
    }

    [AuthorizeRoles("Admin")]
    [HttpPost]
    public async Task<ActionResult<RestaurantDto>> Create([FromBody] RestaurantDto request, CancellationToken cancellationToken)
    {
        var input = new CreateRestaurantCommand
        {
            Name = request.Name,
            Description = request.Description
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<RestaurantDto>>(
            r => RestaurantDto.FromDomainModel(r),
            e => e.ToObjectResult());
    }

    [AuthorizeRoles("Admin")]
    [HttpPut]
    public async Task<ActionResult<RestaurantDto>> Update([FromBody] RestaurantDto request, CancellationToken cancellationToken)
    {
        var input = new UpdateRestaurantCommand
        {
            RestaurantId = request.Id!.Value,
            Name = request.Name,
            Description = request.Description
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<RestaurantDto>>(
            r => RestaurantDto.FromDomainModel(r),
            e => e.ToObjectResult());
    }

    [AuthorizeRoles("Admin")]
    [HttpDelete("{restaurantId:guid}")]
    public async Task<ActionResult<RestaurantDto>> Delete([FromRoute] Guid restaurantId, CancellationToken cancellationToken)
    {
        var input = new DeleteRestaurantCommand
        {
            RestaurantId = restaurantId
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<RestaurantDto>>(
            r => RestaurantDto.FromDomainModel(r),
            e => e.ToObjectResult());
    }
}
