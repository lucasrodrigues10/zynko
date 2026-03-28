namespace Zynko.Application.Games.Commands.CreateGame;

public class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(x => x.HostName)
            .NotEmpty().WithMessage("Informe seu nome.")
            .MaximumLength(50).WithMessage("Nome não pode ter mais de 50 caracteres.");

        RuleFor(x => x.ScoreLimit)
            .InclusiveBetween(1, 20).WithMessage("O limite de pontos deve estar entre 1 e 20.");
    }
}
