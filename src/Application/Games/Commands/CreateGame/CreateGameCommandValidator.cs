namespace Zynko.Application.Games.Commands.CreateGame;

public class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(x => x.RoomName)
            .NotEmpty().WithMessage("Informe o nome da sala.")
            .MaximumLength(100).WithMessage("Nome da sala não pode ter mais de 100 caracteres.");

        RuleFor(x => x.ScoreLimit)
            .InclusiveBetween(1, 20).WithMessage("O limite de pontos deve estar entre 1 e 20.");
    }
}
