export const AVATARS = [
  '🐱', '🐶', '🦊', '🐸', '🐼', '🐨',
  '🐯', '🦁', '🐮', '🐷', '🐙', '🦋',
  '🦄', '🐲', '🦖', '🤖', '👾', '🎃',
  '💀', '👻', '🤡', '😈', '🤠', '🧙',
  '🧟', '🧛', '🧜', '🦸', '🍕', '🌮',
];

export function getAvatar(playerId) {
  if (!playerId || typeof playerId !== 'string') return '🐱';
  const stored = localStorage.getItem(`zynko_avatar_${playerId}`);
  if (stored) return stored;
  const hash = playerId.split('').reduce((acc, c) => acc + c.charCodeAt(0), 0);
  return AVATARS[hash % AVATARS.length];
}

export function saveAvatar(playerId, avatar) {
  localStorage.setItem(`zynko_avatar_${playerId}`, avatar);
}

export function AvatarPicker({ selected, onSelect }) {
  return (
    <div>
      <label className="block text-xs font-semibold text-zinc-400 uppercase tracking-widest mb-2">
        Seu avatar
      </label>
      <div className="grid grid-cols-6 gap-1.5">
        {AVATARS.map(avatar => (
          <button
            key={avatar}
            type="button"
            onClick={() => onSelect(avatar)}
            className={`text-xl p-1.5 rounded-lg transition-all leading-none ${
              selected === avatar
                ? 'bg-yellow-400/20 ring-2 ring-yellow-400 scale-110'
                : 'bg-zinc-800 hover:bg-zinc-700 hover:scale-105'
            }`}
          >
            {avatar}
          </button>
        ))}
      </div>
    </div>
  );
}
