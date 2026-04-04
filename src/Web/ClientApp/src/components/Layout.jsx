import { Link } from 'react-router-dom';

function ZynkoLogo() {
  return (
    <Link to="/" className="flex items-center gap-2.5 group select-none">
      {/* Card icon */}
      <svg width="32" height="32" viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
        {/* Card shadow */}
        <rect x="5" y="5" width="22" height="27" rx="3" fill="#09090b" />
        {/* Card body */}
        <rect x="3" y="3" width="22" height="27" rx="3" fill="#18181b" stroke="#facc15" strokeWidth="1.5" />
        {/* Bold Z */}
        <text x="14" y="22" textAnchor="middle" fontFamily="Arial Black, sans-serif" fontWeight="900" fontSize="16" fill="#facc15" letterSpacing="-1">Z</text>
        {/* Small suit in corner */}
        <text x="6.5" y="10" textAnchor="middle" fontFamily="Arial" fontSize="5" fill="#facc15" opacity="0.7">♠</text>
        <text x="21.5" y="28" textAnchor="middle" fontFamily="Arial" fontSize="5" fill="#facc15" opacity="0.7" transform="rotate(180 21.5 25)">♠</text>
      </svg>

      {/* Wordmark */}
      <span
        className="font-black text-xl tracking-tight leading-none transition-colors group-hover:text-yellow-400"
        style={{
          background: 'linear-gradient(135deg, #ffffff 40%, #facc15 100%)',
          WebkitBackgroundClip: 'text',
          WebkitTextFillColor: 'transparent',
          backgroundClip: 'text',
        }}
      >
        ZYNKO
      </span>

      <span className="text-zinc-600 text-xs font-semibold border border-zinc-700 px-1.5 py-0.5 rounded-full leading-none">
        +18
      </span>
    </Link>
  );
}

export function Layout({ children }) {
  return (
    <div className="min-h-screen bg-zinc-950 text-zinc-100">
      <header className="border-b border-zinc-800 bg-zinc-900/80 backdrop-filter backdrop-blur-sm sticky top-0 z-50">
        <div className="max-w-6xl mx-auto px-6 h-14 flex items-center">
          <ZynkoLogo />
        </div>
      </header>
      <main className="max-w-6xl mx-auto px-6 py-8">
        {children}
      </main>
    </div>
  );
}

export function FullLayout({ children }) {
  return (
    <div className="min-h-screen bg-zinc-950 text-zinc-100">
      <header className="border-b border-zinc-800 bg-zinc-900/80 backdrop-filter backdrop-blur-sm sticky top-0 z-50">
        <div className="max-w-6xl mx-auto px-6 h-14 flex items-center">
          <ZynkoLogo />
        </div>
      </header>
      {children}
    </div>
  );
}
