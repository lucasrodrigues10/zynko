import { getAvatar } from './AvatarPicker';

export function Scoreboard({ players, scoreLimit }) {
  const sorted = [...players].sort((a, b) => b.score - a.score);
  return (
    <div className="bg-zinc-900 border border-zinc-800 rounded-2xl p-4">
      <p className="text-xs font-semibold text-zinc-500 uppercase tracking-widest mb-3">Placar</p>
      <ul className="space-y-2.5">
        {sorted.map(p => (
          <li key={p.id} className="flex items-center gap-2">
            <span className="text-base leading-none flex-shrink-0">{getAvatar(p.id)}</span>
            {p.isJudge && <span className="text-xs" title="Juiz">⚖️</span>}
            <span className={`text-sm font-semibold truncate flex-1 ${p.isJudge ? 'text-yellow-400' : 'text-zinc-200'}`}>
              {p.name}
            </span>
            <div className="flex items-center gap-1 flex-shrink-0">
              {Array.from({ length: scoreLimit }).map((_, i) => (
                <span
                  key={i}
                  className={`w-2.5 h-2.5 rounded-full transition-colors ${i < p.score ? 'bg-yellow-400' : 'bg-zinc-700'}`}
                />
              ))}
              <span className="text-xs text-zinc-500 ml-1 w-4 text-right">{p.score}</span>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
