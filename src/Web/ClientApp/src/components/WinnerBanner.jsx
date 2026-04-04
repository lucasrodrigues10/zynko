import { Scoreboard } from './Scoreboard';

export function WinnerBanner({ playerName, cardText, scores, scoreLimit, onNext }) {
  const players = scores?.map(s => ({
    id: s.playerId, name: s.name, score: s.score, isJudge: s.isJudge,
  })) ?? [];

  return (
    <div className="flex flex-col gap-6">
      <div className="bg-zinc-900 border border-zinc-800 rounded-2xl p-5 sm:p-8 text-center">
        <div className="text-4xl sm:text-5xl mb-3 sm:mb-4">🏆</div>
        <p className="text-zinc-400 text-sm mb-1">Melhor resposta de</p>
        <p className="text-2xl sm:text-3xl font-black text-yellow-400 mb-4 sm:mb-6">{playerName}</p>
        <div className="bg-white text-zinc-900 rounded-2xl p-6 max-w-sm mx-auto shadow-lg">
          <p className="text-lg font-bold leading-snug">{cardText}</p>
          <span className="text-xs opacity-20 font-bold tracking-widest uppercase mt-3 block">🃏 Zynko</span>
        </div>
      </div>

      {players.length > 0 && scoreLimit && (
        <Scoreboard players={players} scoreLimit={scoreLimit} />
      )}

      {onNext ? (
        <div>
          <button
            onClick={onNext}
            className="px-8 py-3 rounded-xl font-bold text-sm bg-yellow-400 text-black hover:bg-yellow-300 transition-colors"
          >
            Próxima rodada →
          </button>
        </div>
      ) : (
        <div className="flex items-center gap-2 text-zinc-500 text-sm">
          <span className="inline-block w-2 h-2 bg-yellow-400 rounded-full animate-pulse" />
          Aguardando o juiz iniciar a próxima rodada...
        </div>
      )}
    </div>
  );
}
