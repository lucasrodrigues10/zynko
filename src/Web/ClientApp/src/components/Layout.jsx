import { Link } from 'react-router-dom';

export function Layout({ children }) {
  return (
    <div className="min-h-screen bg-zinc-950 text-zinc-100">
      <header className="border-b border-zinc-800 bg-zinc-900/80 backdrop-filter backdrop-blur-sm sticky top-0 z-50">
        <div className="max-w-6xl mx-auto px-6 h-14 flex items-center gap-3">
          <Link to="/" className="flex items-center gap-2 font-black text-xl tracking-tight hover:text-yellow-400 transition-colors">
            <span>🃏</span>
            <span>Zynko</span>
          </Link>
          <span className="text-zinc-600 text-xs font-medium ml-1 border border-zinc-700 px-2 py-0.5 rounded-full">+18</span>
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
        <div className="max-w-6xl mx-auto px-6 h-14 flex items-center gap-3">
          <Link to="/" className="flex items-center gap-2 font-black text-xl tracking-tight hover:text-yellow-400 transition-colors">
            <span>🃏</span>
            <span>Zynko</span>
          </Link>
          <span className="text-zinc-600 text-xs font-medium ml-1 border border-zinc-700 px-2 py-0.5 rounded-full">+18</span>
        </div>
      </header>
      {children}
    </div>
  );
}
