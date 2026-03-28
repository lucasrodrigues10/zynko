export function BlackCard({ text, size = 'md' }) {
  const sizes = {
    sm: 'p-4 min-h-32 text-base',
    md: 'p-6 min-h-44 text-lg',
    lg: 'p-8 min-h-56 text-xl',
  };

  return (
    <div className={`bg-zinc-950 border border-zinc-700 text-white rounded-2xl flex flex-col justify-between ${sizes[size]} w-full select-none`}>
      <p className="font-bold leading-snug">{text}</p>
      <div className="flex items-center gap-1.5 opacity-30 mt-4">
        <span className="text-sm">🃏</span>
        <span className="text-xs font-bold tracking-widest uppercase">Zynko</span>
      </div>
    </div>
  );
}
