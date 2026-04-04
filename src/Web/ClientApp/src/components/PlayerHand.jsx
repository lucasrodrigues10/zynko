import { useState } from 'react';
import { WhiteCard } from './WhiteCard';

export function PlayerHand({ cards, blackCardText, onSubmit }) {
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

      {/* Hand */}
      <div>
        <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-3">Sua mão — escolha 1 carta</p>
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
          {cards.map(card => (
            <WhiteCard
              key={card.id}
              text={card.text}
              selected={selected === card.id}
              onClick={() => setSelected(card.id)}
            />
          ))}
        </div>
      </div>

      {/* Confirm */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center gap-3">
        <button
          onClick={() => selected !== null && onSubmit(selected)}
          disabled={selected === null}
          className={`w-full sm:w-auto px-8 py-3 rounded-xl font-bold text-sm transition-colors ${
            selected !== null
              ? 'bg-yellow-400 text-black hover:bg-yellow-300'
              : 'bg-zinc-800 text-zinc-600 cursor-not-allowed border border-zinc-700'
          }`}
        >
          Confirmar escolha →
        </button>
        {selected !== null && (
          <p className="text-zinc-400 text-sm">
            "{cards.find(c => c.id === selected)?.text}"
          </p>
        )}
      </div>
    </div>
  );
}
