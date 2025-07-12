using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pujas.Infrastructure.Queries;
using Pujas.Application.Commands;
using Pujas.Application.DTOs;
using Pujas.Domain.Aggregates;
using Pujas.Application.Events;
using RestSharp;
using Pujas.Domain.ValueObjects;
using System;
using System.Text.Json;
using Pujas.Infrastructure.Interfaces;

namespace Pujas.API.Controllers
{
    [ApiController]
    [Route("api/bid")]
    public class BidsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRestClient _restClient;
        private readonly ICronJobService _cronJobService;
        public BidsController(IMediator mediator, IPublishEndpoint publishEndpoint, IRestClient restClient, ICronJobService cronJobService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _cronJobService = cronJobService ?? throw new ArgumentNullException(nameof(cronJobService));
            _restClient = restClient;
        }

        #region createAutomaticBid
        [HttpPost("createAutomaticBid")]
        public async Task<IActionResult> CreateAutomaticBid([FromBody] CreateAutomaticBidDto automaticBidDto)
        {

            try
            {
                if ( automaticBidDto.Minimum > automaticBidDto.Limit)
                {
                    return BadRequest("El limite no puede ser menor al inicio de la puja automatica.");
                }
                var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("SUBASTA_MS_URL") + "/getById/" + automaticBidDto.AuctionId, Method.Get);

                var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                if (!APIResponse.IsSuccessful)
                {
                    return BadRequest("No existe una subasta con ese id.");
                }

                var auction = JsonDocument.Parse(APIResponse.Content);

                if (auction.RootElement.GetProperty("basePrice").GetDecimal() + auction.RootElement.GetProperty("minimumIncrease").GetDecimal() > automaticBidDto.Limit)
                {
                    return BadRequest("El precio base de la subasta es mayor al limite de la puja automatica.");
                }
                if (auction.RootElement.GetProperty("basePrice").GetDecimal() > automaticBidDto.Minimum)
                {
                    return BadRequest("El precio base de la subasta es mayor al inicio de la puja automatica.");
                }
                if (auction.RootElement.GetProperty("minimumIncrease").GetDecimal() > automaticBidDto.Increment)
                {
                    return BadRequest("El incremento no puede ser menor al incremento minimo de la subasta.");
                }
                if (auction.RootElement.GetProperty("status").GetString() != "pending")
                {
                    return BadRequest("No puedes colocar una puja automatica en una subasta no disponible.");
                }
                var automaticBid = await _mediator.Send(new GetUserAutomaticBidQuery(automaticBidDto.UserId, automaticBidDto.AuctionId));

                if (automaticBid != null)
                {
                    return BadRequest("No puedes tener 2 pujas automaticas a la vez.");
                }

                var automaticBidEvent = new AutomaticBidCreatedEvent(
                    automaticBidDto.UserId,
                    automaticBidDto.Limit,
                    DateTime.UtcNow,
                    automaticBidDto.AuctionId,
                    automaticBidDto.Minimum,
                    automaticBidDto.Increment
                );
                await _publishEndpoint.Publish(automaticBidEvent);
                return CreatedAtAction("CreateAutomaticBid", new { result = "puja automatica creada con exito" }, new
                {
                    result = "puja automatica creada con exito"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetAuctionBids
        [HttpGet("getAuctionBids/{auctionId}")]
        public async Task<IActionResult> GetAuctionBids([FromRoute] string auctionId)
        {
            try
            {
                var auctions = await _mediator.Send(new GetAuctionBidsQuery(auctionId));

                return Ok(auctions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetAuctionPostors
        [HttpGet("getAuctionPostors/{auctionId}")]
        public async Task<IActionResult> GetAuctionPostors([FromRoute] string auctionId)
        {
            try
            {
                var bids = await _mediator.Send(new GetAuctionBidsQuery(auctionId));

                var usersIds = bids.Select(b => b.UserId).Distinct().ToList();

                var users = new List<string>();

                foreach (var userId in usersIds)
                {
                    var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("USER_MS_URL") + "/getuserbyid/" + userId, Method.Get);
                    var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                    if (!APIResponse.IsSuccessful)
                    {
                        users.Add("anonimo");
                    }
                    else
                    {
                        var user = JsonDocument.Parse(APIResponse.Content);
                        users.Add(user.RootElement.GetProperty("name").GetString() + " " + user.RootElement.GetProperty("lastName"));
                    }
                }

                return Ok(users.Distinct());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetAuctionLastBid
        [HttpGet("getAuctionLastBid/{auctionId}")]
        public async Task<IActionResult> GetAuctionLastBid([FromRoute] string auctionId)
        {
            try
            {
                var auctions = await _mediator.Send(new GetAuctionLastBidQuery(auctionId));
                return Ok(auctions);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Puja no encontrada.")
                    return NotFound("Puja no encontrada.");
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetUserParticipateAuctionBids
        [HttpGet("GetUserParticipateAuctionBids/{userId}")]
        public async Task<IActionResult> GetUserParticipateAuctionBids([FromRoute] string userId, [FromQuery]List<string>? auctionSearchIds,
            [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                if (from.HasValue && to.HasValue && from.Value > to.Value)
                {
                    return BadRequest("La fecha de inicio no puede ser mayor que la fecha de fin.");
                }
                if ((from.HasValue && !to.HasValue) || (!from.HasValue && to.HasValue))
                {
                    return BadRequest("Debe proporcionar ambas fechas o ninguna para filtrar por fecha.");
                }
                var bids = await _mediator.Send(new GetUserParticipateAuctionsQuery(userId, auctionSearchIds, from, to));
                var auctionIds = bids.Select(b => b.AuctionId).Distinct().ToList();
                var auctionsLastBid = new List<GetBidDto>();
                foreach (var auctionId in auctionIds)
                {
                    var lastBid = await _mediator.Send(new GetAuctionLastBidQuery(auctionId));
                    if (lastBid != null)
                    {
                        auctionsLastBid.Add(lastBid);
                    }
                }
                return Ok(new
                {
                    userBids = bids,
                    auctionsLastBid = auctionsLastBid
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region AuctionInitiated
        [HttpPost("AuctionInitiated/{id}/{minimumIncrease}/{initialPrice}")]
        public async Task<IActionResult> AuctionInitiated([FromRoute] string id, [FromRoute] decimal minimumIncrease, [FromRoute] decimal initialPrice)
        {
            try
            {
                _cronJobService.CreateCronJob(id, initialPrice, minimumIncrease);
                return Ok(new { result = "Subasta iniciada con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region AuctionEnded
        [HttpPost("AuctionEnded/{id}")]
        public async Task<IActionResult> AuctionEnded([FromRoute] string id)
        {
            try
            {
                _cronJobService.DeleteCronJob(id);
                return Ok(new { result = "Subasta terminada con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion
    }
}
