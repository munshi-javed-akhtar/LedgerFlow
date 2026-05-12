import { Link, Outlet } from 'react-router-dom';

export function DashboardLayout() {
  return <div className="min-h-screen grid grid-cols-[240px_1fr]"><aside className="bg-slate-900 text-white p-6 space-y-3"><h1 className="font-semibold text-xl">LedgerFlow</h1><nav className="space-y-2 text-sm"><Link to="/dashboard" className="block">Dashboard</Link><Link to="/wallets" className="block">Wallets</Link><Link to="/transactions" className="block">Transactions</Link></nav></aside><main className="p-6"><Outlet /></main></div>;
}
