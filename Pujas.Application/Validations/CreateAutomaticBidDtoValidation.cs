using FluentValidation;
using Pujas.Application.DTOs;

namespace Pujas.Application.Validations
{

    public class CreateAutomaticBidDtoValidation : AbstractValidator<CreateAutomaticBidDto>
    {
        public CreateAutomaticBidDtoValidation()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El ID del usuario es obligatorio.");

            RuleFor(x => x.AuctionId)
                .NotEmpty().WithMessage("El ID de la subasta es obligatorio.");

            RuleFor(x => x.Limit)
                .NotEmpty().WithMessage("El limite no puede ser nulo")
                .GreaterThanOrEqualTo(1);


        }
    }
}
