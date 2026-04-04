import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../components/Layout';

const SCORE_OPTIONS = [3, 5, 7, 10];

const ROOM_ADJECTIVES = ['Zoada', 'Louca', 'Épica', 'Caótica', 'Maldita', 'Insana', 'Barulhenta', 'Safada', 'Top', 'Noise'];
const RANDOM_NAMES = ['Pato', 'Lobo', 'Urso', 'Tigre', 'Orca', 'Cobra', 'Gato', 'Raposa', 'Dragão', 'Tubarão', 'Leão', 'Panda', 'Corvo', 'Falcão', 'Lince', 'Capivara'];

function randomRoomName() {
  const adj = ROOM_ADJECTIVES[Math.floor(Math.random() * ROOM_ADJECTIVES.length)];
  const num = Math.floor(Math.random() * 99) + 1;
  return `Sala ${adj} ${num}`;
}

function randomPlayerName() {
  const name = RANDOM_NAMES[Math.floor(Math.random() * RANDOM_NAMES.length)];
  const num = Math.floor(Math.random() * 99) + 1;
  return `${name}${num}`;
}

function saveSession(gameCode, gameId, playerId) {
  localStorage.setItem(`zynko_${gameCode}`, JSON.stringify({ gameId, playerId }));
}

export function HomePage() {
  const navigate = useNavigate();

  const [roomName, setRoomName] = useState(() => randomRoomName());
  const [scoreLimit, setScoreLimit] = useState(5);
  const [loadingCreate, setLoadingCreate] = useState(false);
  const [errorCreate, setErrorCreate] = useState(null);

  const [openGames, setOpenGames] = useState([]);
  const [joiningCode, setJoiningCode] = useState(null);
  const [errorJoin, setErrorJoin] = useState(null);

  useEffect(() => {
    const load = () =>
      fetch('/api/games/open').then(r => r.ok ? r.json() : []).then(setOpenGames).catch(() => {});
    load();
    const interval = setInterval(load, 5000);
    return () => clearInterval(interval);
  }, []);

  const handleCreate = async () => {
    if (!roomName.trim()) { setErrorCreate('Informe o nome da sala.'); return; }
    setLoadingCreate(true); setErrorCreate(null);
    try {
      const res = await fetch('/api/games', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ hostName: randomPlayerName(), roomName: roomName.trim(), scoreLimit }),
      });
      if (!res.ok) {
        const d = await res.json();
        setErrorCreate(d?.title || d?.detail || 'Erro ao criar.');
        return;
      }
      const data = await res.json();
      saveSession(data.gameCode, data.gameId, data.playerId);
      navigate(`/lobby/${data.gameCode}`);
    } catch { setErrorCreate('Erro de conexão.'); }
    finally { setLoadingCreate(false); }
  };

  const handleJoin = async (code) => {
    setJoiningCode(code); setErrorJoin(null);
    try {
      const res = await fetch(`/api/games/${code}/join`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ playerName: randomPlayerName() }),
      });
      if (!res.ok) { const d = await res.json(); setErrorJoin(d?.title || 'Erro ao entrar.'); return; }
      const data = await res.json();
      saveSession(code, data.gameId, data.playerId);
      navigate(data.gameInProgress ? `/game/${code}` : `/lobby/${code}`);
    } catch { setErrorJoin('Erro de conexão.'); }
    finally { setJoiningCode(null); }
  };

  return (
    <Layout>
      <div className="max-w-2xl mx-auto py-8">
        {/* Hero */}
        <div className="text-center mb-10">
          <div className="flex justify-center mb-5">
            <svg width="72" height="88" viewBox="0 0 72 88" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="8" y="8" width="56" height="72" rx="7" fill="#09090b" />
              <rect x="4" y="4" width="56" height="72" rx="7" fill="#18181b" stroke="#facc15" strokeWidth="2" />
              <text x="32" y="52" textAnchor="middle" fontFamily="Arial Black, sans-serif" fontWeight="900" fontSize="40" fill="#facc15" letterSpacing="-2">Z</text>
              <text x="13" y="22" textAnchor="middle" fontFamily="Arial" fontSize="12" fill="#facc15" opacity="0.6">♠</text>
              <text x="51" y="70" textAnchor="middle" fontFamily="Arial" fontSize="12" fill="#facc15" opacity="0.6" transform="rotate(180 51 64)">♠</text>
            </svg>
          </div>
          <h1
            className="text-6xl font-black tracking-tight mb-3"
            style={{
              background: 'linear-gradient(135deg, #ffffff 30%, #facc15 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              backgroundClip: 'text',
            }}
          >
            ZYNKO
          </h1>
          <p className="text-zinc-400 text-lg">Jogo de cartas adulto para os sem noção</p>
        </div>

        {/* Create room */}
        <div className="bg-zinc-900 border border-zinc-800 rounded-2xl p-6 mb-6">
          <h2 className="text-base font-bold mb-4">Criar sala</h2>
          <div className="flex flex-col gap-4">
            <div>
              <label className="block text-xs font-semibold text-zinc-400 uppercase tracking-widest mb-1.5">Nome da sala</label>
              <input
                type="text"
                value={roomName}
                onChange={e => { setRoomName(e.target.value); setErrorCreate(null); }}
                onKeyDown={e => e.key === 'Enter' && handleCreate()}
                maxLength={100}
                className="w-full bg-zinc-800 border border-zinc-700 text-zinc-100 placeholder-zinc-500 rounded-lg px-3 py-2.5 text-sm outline-none focus:border-yellow-400 focus:ring-1 focus:ring-yellow-400 transition-colors"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-zinc-400 uppercase tracking-widest mb-1.5">Pontos para vencer</label>
              <div className="flex gap-2">
                {SCORE_OPTIONS.map(opt => (
                  <button
                    key={opt}
                    onClick={() => setScoreLimit(opt)}
                    className={`flex-1 py-2 rounded-lg font-bold text-sm transition-all ${
                      scoreLimit === opt
                        ? 'bg-yellow-400 text-black'
                        : 'bg-zinc-800 text-zinc-400 hover:bg-zinc-700 border border-zinc-700'
                    }`}
                  >
                    {opt}
                  </button>
                ))}
              </div>
            </div>

            {errorCreate && <p className="text-red-400 text-sm">{errorCreate}</p>}

            <button
              onClick={handleCreate}
              disabled={loadingCreate}
              className="w-full py-3 rounded-xl font-bold text-sm bg-yellow-400 text-black hover:bg-yellow-300 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loadingCreate ? 'Criando...' : 'Criar sala →'}
            </button>
          </div>
        </div>

        {/* Rooms list */}
        <div>
          <div className="flex items-center gap-2 mb-3">
            <h2 className="text-sm font-semibold text-zinc-400 uppercase tracking-widest">Salas disponíveis</h2>
            <span className="inline-block w-2 h-2 rounded-full bg-yellow-400 animate-pulse flex-shrink-0" />
          </div>

          {errorJoin && <p className="text-red-400 text-sm mb-2">{errorJoin}</p>}

          {openGames.length === 0 ? (
            <p className="text-zinc-600 text-sm py-4 text-center">Nenhuma sala disponível — crie a primeira!</p>
          ) : (
            <div className="flex flex-col gap-2">
              {openGames.map(game => (
                <div key={game.code} className="bg-zinc-900 border border-zinc-800 rounded-xl px-5 py-4 flex items-center gap-4">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-0.5">
                      <p className="font-semibold text-zinc-100 text-sm">{game.name || game.code}</p>
                      {game.inProgress && (
                        <span className="text-xs font-bold bg-green-500/20 text-green-400 border border-green-500/30 px-2 py-0.5 rounded-full">
                          Em andamento
                        </span>
                      )}
                    </div>
                    <p className="text-zinc-600 text-xs">
                      {game.playerCount} jogador{game.playerCount !== 1 ? 'es' : ''} • {game.scoreLimit} pontos para vencer
                    </p>
                  </div>
                  <button
                    onClick={() => handleJoin(game.code)}
                    disabled={joiningCode === game.code}
                    className="px-5 py-2 rounded-lg font-bold text-sm bg-yellow-400 text-black hover:bg-yellow-300 transition-colors disabled:opacity-40 disabled:cursor-not-allowed flex-shrink-0"
                  >
                    {joiningCode === game.code ? 'Entrando...' : 'Entrar →'}
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        <p className="text-center text-zinc-600 text-xs mt-8">
          Conteúdo adulto (+18) • Jogue com responsabilidade
        </p>
      </div>
    </Layout>
  );
}
