import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../components/Layout';

const SCORE_OPTIONS = [3, 5, 7, 10];

function saveSession(gameCode, gameId, playerId) {
  localStorage.setItem(`zynko_${gameCode}`, JSON.stringify({ gameId, playerId }));
}

export function HomePage() {
  const navigate = useNavigate();

  const [hostName, setHostName] = useState('');
  const [scoreLimit, setScoreLimit] = useState(5);
  const [joinName, setJoinName] = useState('');
  const [joinCode, setJoinCode] = useState('');
  const [loadingCreate, setLoadingCreate] = useState(false);
  const [loadingJoin, setLoadingJoin] = useState(false);
  const [errorCreate, setErrorCreate] = useState(null);
  const [errorJoin, setErrorJoin] = useState(null);

  const handleCreate = async () => {
    if (!hostName.trim()) { setErrorCreate('Informe seu nome.'); return; }
    setLoadingCreate(true); setErrorCreate(null);
    try {
      const res = await fetch('/api/games', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ hostName: hostName.trim(), scoreLimit }),
      });
      if (!res.ok) { const d = await res.json(); setErrorCreate(d?.title || 'Erro ao criar.'); return; }
      const data = await res.json();
      saveSession(data.gameCode, data.gameId, data.playerId);
      navigate(`/lobby/${data.gameCode}`);
    } catch { setErrorCreate('Erro de conexão.'); }
    finally { setLoadingCreate(false); }
  };

  const handleJoin = async () => {
    if (!joinName.trim()) { setErrorJoin('Informe seu nome.'); return; }
    if (!joinCode.trim()) { setErrorJoin('Informe o código da sala.'); return; }
    setLoadingJoin(true); setErrorJoin(null);
    try {
      const code = joinCode.trim().toUpperCase();
      const res = await fetch(`/api/games/${code}/join`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ playerName: joinName.trim() }),
      });
      if (!res.ok) { const d = await res.json(); setErrorJoin(d?.title || 'Sala não encontrada.'); return; }
      const data = await res.json();
      saveSession(code, data.gameId, data.playerId);
      navigate(`/lobby/${code}`);
    } catch { setErrorJoin('Erro de conexão.'); }
    finally { setLoadingJoin(false); }
  };

  return (
    <Layout>
      <div className="max-w-3xl mx-auto py-8">
        {/* Hero */}
        <div className="text-center mb-12">
          <div className="text-6xl mb-4">🃏</div>
          <h1 className="text-5xl font-black tracking-tight mb-3">Zynko</h1>
          <p className="text-zinc-400 text-lg">Jogo de cartas adulto para os sem noção</p>
        </div>

        {/* Cards side by side */}
        <div className="grid grid-cols-2 gap-6">
          {/* Create */}
          <div className="bg-zinc-900 border border-zinc-800 rounded-2xl p-7 flex flex-col gap-5">
            <div>
              <h2 className="text-lg font-bold mb-1">Criar sala</h2>
              <p className="text-zinc-500 text-sm">Você será o host da partida</p>
            </div>

            <div className="flex flex-col gap-4">
              <div>
                <label className="block text-xs font-semibold text-zinc-400 uppercase tracking-widest mb-1.5">Seu nome</label>
                <input
                  type="text"
                  placeholder="Como te chamam?"
                  value={hostName}
                  onChange={e => setHostName(e.target.value)}
                  onKeyDown={e => e.key === 'Enter' && handleCreate()}
                  maxLength={50}
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
            </div>

            {errorCreate && <p className="text-red-400 text-sm">{errorCreate}</p>}

            <button
              onClick={handleCreate}
              disabled={loadingCreate}
              className="mt-auto w-full py-3 rounded-xl font-bold text-sm bg-yellow-400 text-black hover:bg-yellow-300 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loadingCreate ? 'Criando...' : 'Criar sala →'}
            </button>
          </div>

          {/* Join */}
          <div className="bg-zinc-900 border border-zinc-800 rounded-2xl p-7 flex flex-col gap-5">
            <div>
              <h2 className="text-lg font-bold mb-1">Entrar na sala</h2>
              <p className="text-zinc-500 text-sm">Use o código compartilhado pelo host</p>
            </div>

            <div className="flex flex-col gap-4">
              <div>
                <label className="block text-xs font-semibold text-zinc-400 uppercase tracking-widest mb-1.5">Seu nome</label>
                <input
                  type="text"
                  placeholder="Como te chamam?"
                  value={joinName}
                  onChange={e => setJoinName(e.target.value)}
                  maxLength={50}
                  className="w-full bg-zinc-800 border border-zinc-700 text-zinc-100 placeholder-zinc-500 rounded-lg px-3 py-2.5 text-sm outline-none focus:border-yellow-400 focus:ring-1 focus:ring-yellow-400 transition-colors"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-zinc-400 uppercase tracking-widest mb-1.5">Código da sala</label>
                <input
                  type="text"
                  placeholder="ZYNK42"
                  value={joinCode}
                  onChange={e => setJoinCode(e.target.value.toUpperCase())}
                  onKeyDown={e => e.key === 'Enter' && handleJoin()}
                  maxLength={6}
                  className="w-full bg-zinc-800 border border-zinc-700 text-zinc-100 placeholder-zinc-500 rounded-lg px-3 py-2.5 text-sm outline-none focus:border-yellow-400 focus:ring-1 focus:ring-yellow-400 transition-colors font-mono tracking-widest text-center text-lg uppercase"
                />
              </div>
            </div>

            {errorJoin && <p className="text-red-400 text-sm">{errorJoin}</p>}

            <button
              onClick={handleJoin}
              disabled={loadingJoin}
              className="mt-auto w-full py-3 rounded-xl font-bold text-sm bg-zinc-700 text-zinc-100 hover:bg-zinc-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed border border-zinc-600"
            >
              {loadingJoin ? 'Entrando...' : 'Entrar na sala →'}
            </button>
          </div>
        </div>

        {/* Footer note */}
        <p className="text-center text-zinc-600 text-xs mt-8">
          Conteúdo adulto (+18) • Jogue com responsabilidade
        </p>
      </div>
    </Layout>
  );
}
