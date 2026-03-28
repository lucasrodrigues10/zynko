using Zynko.Application.Games.Commands.CreateGame;
using Zynko.Application.Games.Commands.JoinGame;
using Zynko.Application.Games.Commands.PickWinner;
using Zynko.Application.Games.Commands.StartGame;
using Zynko.Application.Games.Commands.StartRound;
using Zynko.Application.Games.Commands.SubmitCard;
using Zynko.Application.Games.Queries.GetGame;
using Zynko.Application.Games.Queries.GetPlayerHand;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Zynko.Web.Endpoints;

public class Games : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGame);
        groupBuilder.MapPost(JoinGame, "{code}/join");
        groupBuilder.MapPost(StartGame, "{id}/start");
        groupBuilder.MapGet(GetGame, "{code}");
        groupBuilder.MapGet(GetPlayerHand, "{id}/hand/{playerId}");
        groupBuilder.MapPost(StartRound, "{id}/rounds/start");
        groupBuilder.MapPost(SubmitCard, "{id}/rounds/{roundId}/submit");
        groupBuilder.MapPost(PickWinner, "{id}/rounds/{roundId}/winner");
    }

    [EndpointSummary("Criar partida")]
    public static async Task<Created<CreateGameResult>> CreateGame(ISender sender, CreateGameCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Created($"/api/games/{result.GameCode}", result);
    }

    [EndpointSummary("Entrar na partida via código")]
    public static async Task<Ok<JoinGameResult>> JoinGame(ISender sender, string code, JoinGameCommand command)
    {
        var result = await sender.Send(command with { GameCode = code });
        return TypedResults.Ok(result);
    }

    [EndpointSummary("Host inicia o jogo")]
    public static async Task<NoContent> StartGame(ISender sender, int id, StartGameCommand command)
    {
        await sender.Send(command with { GameId = id });
        return TypedResults.NoContent();
    }

    [EndpointSummary("Estado da partida")]
    public static async Task<Ok<GameVm>> GetGame(ISender sender, string code)
    {
        var vm = await sender.Send(new GetGameQuery(code));
        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Mão do jogador")]
    public static async Task<Ok<IList<CardDto>>> GetPlayerHand(ISender sender, int id, int playerId)
    {
        var hand = await sender.Send(new GetPlayerHandQuery(id, playerId));
        return TypedResults.Ok(hand);
    }

    [EndpointSummary("Iniciar próxima rodada")]
    public static async Task<NoContent> StartRound(ISender sender, int id)
    {
        await sender.Send(new StartRoundCommand(id));
        return TypedResults.NoContent();
    }

    [EndpointSummary("Submeter carta branca")]
    public static async Task<NoContent> SubmitCard(ISender sender, int id, int roundId, SubmitCardCommand command)
    {
        await sender.Send(command with { GameId = id });
        return TypedResults.NoContent();
    }

    [EndpointSummary("Escolher vencedor da rodada")]
    public static async Task<NoContent> PickWinner(ISender sender, int id, int roundId, PickWinnerCommand command)
    {
        await sender.Send(command with { GameId = id, RoundId = roundId });
        return TypedResults.NoContent();
    }
}
