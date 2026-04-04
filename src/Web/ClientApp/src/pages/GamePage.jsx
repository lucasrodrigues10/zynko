import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useGameHub } from '../hooks/useGameHub';
import { PlayerHand } from '../components/PlayerHand';
import { SubmissionPile } from '../components/SubmissionPile';
import { Scoreboard } from '../components/Scoreboard';
import { WinnerBanner } from '../components/WinnerBanner';
import { FullLayout } from '../components/Layout';
import { getAvatar } from '../components/AvatarPicker';

function getSession(code) {
  try { return JSON.parse(localStorage.getItem(`zynko_${code}`)); }
  catch { return null; }
}

export function GamePage() {
  const { code } = useParams();
  const navigate = useNavigate();
  const session = getSession(code);

  const [game, setGame] = useState(null);
  const [hand, setHand] = useState([]);
  const [judgeSubmissions, setJudgeSubmissions] = useState(null);
  const [submissionProgress, setSubmissionProgress] = useState(null);
  const [winnerInfo, setWinnerInfo] = useState(null);
  const [hasSubmitted, setHasSubmitted] = useState(false);
  const [phase, setPhase] = useState('waiting');
  const [showScoreboard, setShowScoreboard] = useState(false);

  const fetchGame = useCallback(async () => {
    const res = await fetch(`/api/games/${code}`);
    if (res.ok) setGame(await res.json());
  }, [code]);

  const fetchHand = useCallback(async (gameId, playerId) => {
    const res = await fetch(`/api/games/${gameId}/hand/${playerId}`);
    if (res.ok) setHand(await res.json());
  }, []);

  useEffect(() => { fetchGame(); }, [fetchGame]);

  useEffect(() => {
    if (!session) return;
    const leaveUrl = `/api/games/${code}/leave/${session.playerId}`;
    const onUnload = () => navigator.sendBeacon(leaveUrl);
    window.addEventListener('beforeunload', onUnload);
    return () => {
      window.removeEventListener('beforeunload', onUnload);
      fetch(leaveUrl, { method: 'POST' }).catch(() => {});
    };
  }, [code, session?.playerId]);

  const isJudge = game?.players?.find(p => p.id === session?.playerId)?.isJudge ?? false;

  useGameHub(code, {
    onRoundStarted: async () => {
      const res = await fetch(`/api/games/${code}`);
      if (!res.ok) return;
      const freshGame = await res.json();
      setGame(freshGame);
      setHasSubmitted(false);
      setJudgeSubmissions(null);
      setSubmissionProgress(null);
      setWinnerInfo(null);
      const freshIsJudge = freshGame.players?.find(p => p.id === session?.playerId)?.isJudge ?? false;
      if (!freshIsJudge && session) {
        const handRes = await fetch(`/api/games/${session.gameId}/hand/${session.playerId}`);
        if (handRes.ok) setHand(await handRes.json());
        setPhase('submitting');
      } else {
        setPhase('waiting');
      }
    },
    onCardSubmitted: ({ submittedCount, totalRequired }) => {
      setSubmissionProgress({ submitted: submittedCount, total: totalRequired });
    },
    onJudgingPhase: (submissions) => {
      setJudgeSubmissions(submissions);
      setPhase('judging');
    },
    onRoundFinished: async (data) => {
      setWinnerInfo(data);
      setPhase('winner');
      await fetchGame();
    },
    onGameFinished: async () => {
      await fetchGame();
      navigate(`/results/${code}`);
    },
  });

  useEffect(() => {
    if (!game || !session) return;
    const round = game.currentRound;
    if (!round) return;
    const judge = game.players.find(p => p.isJudge)?.id === session.playerId;
    if (round.status === 0 && judge) {
      setPhase('waiting');
    } else if (round.status === 0 && !judge) {
      fetchHand(session.gameId, session.playerId);
      setPhase('submitting');
    } else if (round.status === 1 && judge) {
      setPhase('judging');
    }
  }, [game?.currentRound?.id, game?.currentRound?.status]);

  const handleSubmitCard = async (cardId) => {
    if (!session) return;
    await fetch(`/api/games/${session.gameId}/rounds/${game.currentRound.id}/submit`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ playerId: session.playerId, whiteCardId: cardId }),
    });
    setHasSubmitted(true);
    setPhase('waiting');
  };

  const handlePickWinner = async (submissionId) => {
    if (!session) return;
    await fetch(`/api/games/${session.gameId}/rounds/${game.currentRound.id}/winner`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ submissionId }),
    });
  };

  const handleNextRound = async () => {
    if (!session) return;
    setPhase('waiting');
    setWinnerInfo(null);
    await fetch(`/api/games/${session.gameId}/rounds/start`, { method: 'POST' });
  };

  if (!game || !session) {
    return (
      <FullLayout>
        <div className="flex items-center justify-center h-64">
          <p className="text-zinc-500 animate-pulse">Carregando...</p>
        </div>
      </FullLayout>
    );
  }

  const currentRound = game.currentRound;
  const myPlayer = game.players.find(p => p.id === session.playerId);
  const isNewJudge = game.players.find(p => p.isJudge)?.id === session.playerId;
  const isHost = game.hostPlayerId === session.playerId;

  return (
    <FullLayout>
      <div className="flex flex-col md:flex-row md:h-[calc(100vh-3.5rem)]">

        {/* Sidebar — desktop: lateral | mobile: barra compacta + placar colapsável */}
        <aside className="md:w-64 md:flex-shrink-0 md:border-r md:border-zinc-800 md:flex md:flex-col md:gap-4 md:p-5 md:overflow-y-auto border-b border-zinc-800">

          {/* Mobile top bar */}
          <div className="flex md:hidden items-center gap-3 px-4 py-2.5">
            <span className="font-mono font-black text-yellow-400 text-sm flex-shrink-0">R{game.currentRoundIndex + 1}</span>
            {myPlayer && (
              <span className="text-sm font-semibold text-zinc-200 truncate">{myPlayer.name}</span>
            )}
            {isNewJudge && <span className="text-xs font-bold text-yellow-400 flex-shrink-0">⚖️ Juiz</span>}
            {myPlayer && (
              <span className="text-xs text-zinc-400 flex-shrink-0 ml-auto">{myPlayer.score} pts</span>
            )}
            <button
              onClick={() => setShowScoreboard(s => !s)}
              className="flex-shrink-0 text-xs text-zinc-400 border border-zinc-700 px-2 py-1 rounded-lg"
            >
              {showScoreboard ? 'Fechar' : 'Placar'}
            </button>
          </div>

          {/* Mobile collapsible scoreboard */}
          {showScoreboard && (
            <div className="md:hidden px-4 pb-3">
              <Scoreboard players={game.players} scoreLimit={game.scoreLimit} />
            </div>
          )}

          {/* Desktop sidebar content */}
          <div className="hidden md:block">
            <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-1">Sala</p>
            <p className="font-mono font-black text-yellow-400 text-lg tracking-widest">{code}</p>
          </div>
          <div className="hidden md:block">
            <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-1">Rodada</p>
            <p className="font-bold text-zinc-200">{game.currentRoundIndex + 1}</p>
          </div>
          {myPlayer && (
            <div className="hidden md:block">
              <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-1">Você</p>
              <div className="flex items-center gap-2">
                <span className="text-2xl">{getAvatar(myPlayer.id)}</span>
                <div>
                  <p className="font-bold text-zinc-200">{myPlayer.name}</p>
                  {isNewJudge && <p className="text-yellow-400 text-xs font-semibold">⚖️ Juiz desta rodada</p>}
                </div>
              </div>
            </div>
          )}
          <div className="hidden md:block flex-1">
            <Scoreboard players={game.players} scoreLimit={game.scoreLimit} />
          </div>
        </aside>

        {/* Main content */}
        <main className="flex-1 overflow-y-auto p-4 md:p-8">
          {phase === 'winner' && winnerInfo ? (
            <WinnerBanner
              playerName={winnerInfo.winnerName}
              cardText={winnerInfo.whiteCardText}
              scores={winnerInfo.scores}
              scoreLimit={game.scoreLimit}
              onNext={isNewJudge || isHost ? handleNextRound : null}
            />
          ) : isNewJudge && phase === 'judging' && judgeSubmissions ? (
            <SubmissionPile
              submissions={judgeSubmissions}
              blackCardText={currentRound?.blackCardText}
              onPickWinner={handlePickWinner}
            />
          ) : !isNewJudge && phase === 'submitting' && hand.length > 0 ? (
            <PlayerHand
              cards={hand}
              blackCardText={currentRound?.blackCardText}
              onSubmit={handleSubmitCard}
            />
          ) : (
            <WaitingView
              isJudge={isNewJudge}
              hasSubmitted={hasSubmitted}
              submissionProgress={submissionProgress}
              currentRound={currentRound}
            />
          )}
        </main>
      </div>
    </FullLayout>
  );
}

function WaitingView({ isJudge, hasSubmitted, submissionProgress, currentRound }) {
  return (
    <div className="flex flex-col gap-6 max-w-lg">
      {currentRound && (
        <div>
          <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-2">Carta desta rodada</p>
          <div className="bg-zinc-950 border border-zinc-700 text-white rounded-2xl p-6">
            <p className="text-xl font-bold leading-snug">{currentRound.blackCardText}</p>
            <div className="flex items-center gap-1.5 opacity-30 mt-4">
              <span className="text-sm">🃏</span>
              <span className="text-xs font-bold tracking-widest uppercase">Zynko</span>
            </div>
          </div>
        </div>
      )}

      <div className="bg-zinc-900 border border-zinc-800 rounded-2xl p-6">
        {isJudge ? (
          <>
            <p className="font-bold text-yellow-400 mb-1">⚖️ Você é o juiz</p>
            {submissionProgress ? (
              <>
                <p className="text-zinc-400 text-sm mb-3">
                  {submissionProgress.submitted} de {submissionProgress.total} respostas recebidas
                </p>
                <div className="flex gap-1.5">
                  {Array.from({ length: submissionProgress.total }).map((_, i) => (
                    <div
                      key={i}
                      className={`flex-1 h-1.5 rounded-full transition-colors ${
                        i < submissionProgress.submitted ? 'bg-yellow-400' : 'bg-zinc-700'
                      }`}
                    />
                  ))}
                </div>
              </>
            ) : (
              <p className="text-zinc-500 text-sm animate-pulse">Aguardando as respostas dos jogadores...</p>
            )}
          </>
        ) : hasSubmitted ? (
          <>
            <p className="font-bold text-green-400 mb-1">✓ Resposta enviada</p>
            <p className="text-zinc-500 text-sm">Aguardando o juiz escolher a melhor resposta...</p>
            {submissionProgress && (
              <p className="text-zinc-600 text-xs mt-2">
                {submissionProgress.submitted} de {submissionProgress.total} jogadores enviaram
              </p>
            )}
          </>
        ) : (
          <p className="text-zinc-500 text-sm animate-pulse">Escolha uma carta da sua mão...</p>
        )}
      </div>
    </div>
  );
}
