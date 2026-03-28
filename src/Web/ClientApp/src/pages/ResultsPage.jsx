import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Layout } from '../components/Layout';

export function ResultsPage() {
  const { code } = useParams();
  const navigate = useNavigate();
  const [game, setGame] = useState(null);

  useEffect(() => {
    fetch(`/api/games/${code}`)
      .then(r => r.json())
      .then(setGame);
  }, [code]);

  if (!game) {
    return (
      <Layout>
        <div className="flex items-center justify-center h-64">
          <p className="text-zinc-500 animate-pulse">Carregando...</p>
        </div>
      </Layout>
    );
  }

  const sorted = [...game.players].sort((a, b) => b.score - a.score);
  const champion = sorted[0];

  return (
    <Layout>
      <div className="max-w-lg mx-auto py-12 text-center">
        <div className="text-7xl mb-4">🏆</div>
        <p className="text-zinc-400 text-sm mb-1">Campeão do Zynko</p>
        <p className="text-5xl font-black text-yellow-400 mb-10">{champion.name}</p>

        <div className="bg-zinc-900 border border-zinc-800 rounded-2xl p-6 mb-8 text-left">
          <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-4">Placar final</p>
          <ul className="space-y-3">
            {sorted.map((p, i) => (
              <li key={p.id} className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <span className="text-zinc-600 font-bold w-5 text-sm">{i + 1}.</span>
                  <span className={`font-semibold ${i === 0 ? 'text-yellow-400' : 'text-zinc-200'}`}>{p.name}</span>
                </div>
                <span className={`font-black text-lg ${i === 0 ? 'text-yellow-400' : 'text-zinc-400'}`}>
                  {p.score} pts
                </span>
              </li>
            ))}
          </ul>
        </div>

        <button
          onClick={() => navigate('/')}
          className="px-8 py-3 rounded-xl font-bold text-sm bg-yellow-400 text-black hover:bg-yellow-300 transition-colors"
        >
          Nova partida
        </button>
      </div>
    </Layout>
  );
}
