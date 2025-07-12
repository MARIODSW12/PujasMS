using MediatR;

using Pujas.Application.DTOs;

namespace Pujas.Infrastructure.Queries
{
    public class GetBidByIdQuery : IRequest<GetBidDto>
    {
        public string Id { get; set; }

        public GetBidByIdQuery(string id)
        {
            Id = id;
        }
    }
}
