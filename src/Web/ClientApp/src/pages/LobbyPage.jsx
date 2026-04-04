import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useGameHub } from '../hooks/useGameHub';
import { Layout } from '../components/Layout';
import { getAvatar } from '../components/AvatarPicker';

export function LobbyPage() {
  const { code } = useParams();
  const navigate = useNavigate();
  const session = getSession(code);

  const [game, setGame] = useState(null);
  const [error, setError] = useState(null);
  const [starting, setStarting] = useState(false);

  const fetchGame = async () => {
    const res = await fetch(`/api/games/${code}`);
    if (!res.ok) { setError('Sala não encontrada.'); return; }
    setGame(await res.json());
  };

  useEffect(() => { fetchGame(); }, [code]);

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

  useGameHub(code, {
    onPlayerJoined: () => fetchGame(),
    onPlayerLeft: () => fetchGame(),
    onGameStarted: () => navigate(`/game/${code}`),
  });

  const isHost = session && game &&
    game.players.find(p => p.id === session.playerId) &&
    game.hostPlayerId === session.playerId;

  const handleStart = async () => {
    if (!session) return;
    setStarting(true);
    const res = await fetch(`/api/games/${session.gameId}/start`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ hostPlayerId: session.playerId }),
    });
    if (!res.ok) {
      const d = await res.json();
      setError(d?.title || 'Erro ao iniciar.');
      setStarting(false);
    }
  };

  if (error) return (
    <Layout>
      <div className="flex items-center justify-center py-24">
        <p className="text-red-400">{error}</p>
      </div>
    </Layout>
  );

  if (!game) return (
    <Layout>
      <div className="flex items-center justify-center py-24">
        <p className="text-zinc-500 animate-pulse">Carregando...</p>
      </div>
    </Layout>
  );

  return (
    <Layout>
      <div className="max-w-2xl mx-auto py-6">
        <h1 className="text-2xl font-black mb-6">Sala de espera</h1>

        <div className="bg-zinc-900 border border-zinc-800 rounded-2xl p-6 mb-5">
          <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-3">Configurações</p>
          <div className="flex gap-8 text-sm">
            <div className="flex justify-between items-center gap-2">
              <span className="text-zinc-400">Pontos para vencer</span>
              <span className="font-bold text-yellow-400">{game.scoreLimit}</span>
            </div>
            <div className="flex justify-between items-center gap-2">
              <span className="text-zinc-400">Jogadores</span>
              <span className="font-bold">{game.players.length} / 12</span>
            </div>
            <div className="flex justify-between items-center gap-2">
              <span className="text-zinc-400">Host</span>
              <span className="font-bold">{game.players.find(p => p.id === game.hostPlayerId)?.name}</span>
            </div>
          </div>
        </div>

        {/* Players */}
        <div className="bg-zinc-900 border border-zinc-800 rounded-2xl p-6 mb-6">
          <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-4">
            Jogadores ({game.players.length})
          </p>
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
            {game.players.map(p => (
              <div key={p.id} className="flex items-center gap-3 bg-zinc-800 rounded-xl px-4 py-3">
                <div className="w-9 h-9 bg-zinc-700 rounded-full flex items-center justify-center text-xl flex-shrink-0">
                  {getAvatar(p.id)}
                </div>
                <div className="min-w-0">
                  <p className="font-semibold text-sm truncate">{p.name}</p>
                  {game.hostPlayerId === p.id && (
                    <p className="text-yellow-400 text-xs">host</p>
                  )}
                </div>
              </div>
            ))}
            {game.players.length < 2 && Array.from({ length: 2 - game.players.length }).map((_, i) => (
              <div key={`empty-${i}`} className="flex items-center gap-3 bg-zinc-800/40 border border-dashed border-zinc-700 rounded-xl px-4 py-3">
                <div className="w-8 h-8 bg-zinc-700 rounded-full flex-shrink-0" />
                <p className="text-zinc-600 text-sm">Aguardando...</p>
              </div>
            ))}
          </div>
        </div>

        {/* Action */}
        {isHost ? (
          <div className="flex items-center gap-4">
            <button
              onClick={handleStart}
              disabled={starting || game.players.length < 2}
              className={`px-8 py-3 rounded-xl font-bold text-sm transition-colors ${
                game.players.length >= 2
                  ? 'bg-yellow-400 text-black hover:bg-yellow-300'
                  : 'bg-zinc-800 text-zinc-600 cursor-not-allowed border border-zinc-700'
              } disabled:opacity-60`}
            >
              {starting ? 'Iniciando...' : `Iniciar jogo com ${game.players.length} jogadores →`}
            </button>
            {game.players.length < 2 && (
              <p className="text-zinc-500 text-sm">Precisa de ao menos 2 jogadores</p>
            )}
          </div>
        ) : (
          <div className="flex items-center gap-2 text-zinc-500 text-sm">
            <span className="inline-block w-2 h-2 bg-yellow-400 rounded-full animate-pulse" />
            Aguardando o host iniciar o jogo...
          </div>
        )}
      </div>
    </Layout>
  );
}

function getSession(gameCode) {
  try { return JSON.parse(localStorage.getItem(`zynko_${gameCode}`)); }
  catch { return null; }
}
