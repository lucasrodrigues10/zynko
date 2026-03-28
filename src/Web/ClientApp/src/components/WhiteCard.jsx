export function WhiteCard({ text, selected, onClick, disabled }) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={[
        'bg-white text-zinc-900 rounded-2xl p-4 text-left w-full flex flex-col justify-between min-h-32',
        'border-2 transition-all duration-150',
        selected
          ? 'border-yellow-400 ring-2 ring-yellow-400/40 shadow-lg shadow-yellow-400/20 -translate-y-1'
          : 'border-transparent hover:border-zinc-300 hover:shadow-md',
        disabled && !selected ? 'opacity-40 cursor-not-allowed' : 'cursor-pointer',
      ].join(' ')}
    >
      <p className="text-sm font-semibold leading-snug">{text}</p>
      <span className="text-xs opacity-20 font-bold tracking-widest uppercase mt-2 block">🃏 Zynko</span>
    </button>
  );
}
