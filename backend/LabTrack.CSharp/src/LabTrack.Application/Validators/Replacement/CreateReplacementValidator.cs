using FluentValidation;
using LabTrack.Application.Commands.Replacement;

namespace LabTrack.Application.Validators.Replacement;

public class CreateReplacementValidator : AbstractValidator<CreateReplacementCommand>
{
    public CreateReplacementValidator()
    {
    }
}
