import { useState } from 'react';
import { WhiteCard } from './WhiteCard';

export function SubmissionPile({ submissions, blackCardText, onPickWinner }) {
  const [selected, setSelected] = useState(null);

  return (
    <div className="flex flex-col gap-6">
      {/* Black card */}
      <div>
        <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-2">Carta desta rodada</p>
        <div className="bg-zinc-950 border border-zinc-700 text-white rounded-2xl p-6 max-w-sm">
          <p className="text-lg font-bold leading-snug">{blackCardText}</p>
          <div className="flex items-center gap-1.5 opacity-30 mt-4">
            <span className="text-sm">🃏</span>
            <span className="text-xs font-bold tracking-widest uppercase">Zynko</span>
          </div>
        </div>
      </div>

      {/* Submissions */}
      <div>
        <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-3">
          ⚖️ Escolha a melhor resposta
        </p>
        <div className="grid grid-cols-4 gap-3">
          {submissions.map(s => (
            <WhiteCard
              key={s.id}
              text={s.whiteCardText}
              selected={selected === s.id}
              onClick={() => setSelected(s.id)}
            />
          ))}
        </div>
      </div>

      {/* Confirm */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => selected !== null && onPickWinner(selected)}
          disabled={selected === null}
          className={`px-8 py-3 rounded-xl font-bold text-sm transition-colors ${
            selected !== null
              ? 'bg-yellow-400 text-black hover:bg-yellow-300'
              : 'bg-zinc-800 text-zinc-600 cursor-not-allowed border border-zinc-700'
          }`}
        >
          Escolher vencedor 🏆
        </button>
        {selected !== null && (
          <p className="text-zinc-400 text-sm">
            "{submissions.find(s => s.id === selected)?.whiteCardText}"
          </p>
        )}
      </div>
    </div>
  );
}
