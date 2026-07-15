import { useEffect, useState } from 'react';
import api from '../lib/api';
import type { ApiResponse, WorkerStatus } from '../types';
import { useAuth } from '../hooks/useAuth';

export default function Dashboard() {
  const [status, setStatus] = useState<WorkerStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const logout = useAuth((s) => s.logout);
  const username = useAuth((s) => s.username);

  useEffect(() => {
    fetchStatus();
    const interval = setInterval(fetchStatus, 30000);
    return () => clearInterval(interval);
  }, []);

  const fetchStatus = async () => {
    try {
      const { data } = await api.get<ApiResponse<WorkerStatus>>('/worker/status');
      if (data.success && data.data) setStatus(data.data);
    } catch { /* handled by interceptor */ }
    setLoading(false);
  };

  if (loading) return <div className="p-8 text-gray-400">Carregando...</div>;

  return (
    <div className="min-h-screen bg-gray-950 text-gray-100 p-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-2xl font-mono text-green-400">STA Dashboard</h1>
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-500">{username}</span>
            <button onClick={logout} className="text-sm text-red-400 hover:text-red-300">Sair</button>
          </div>
        </div>

        {status && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <StatCard
              label="Status"
              value={status.status}
              color={status.status === 'rodando' ? 'green' : 'yellow'}
            />
            <StatCard
              label="Arquivos Hoje"
              value={String(status.arquivosHoje)}
              color="blue"
            />
            <StatCard
              label="Erros Hoje"
              value={String(status.errosHoje)}
              color={status.errosHoje > 0 ? 'red' : 'green'}
            />
            <StatCard
              label="Último Ciclo"
              value={status.ultimoCicloStatus === 'O' ? 'Sucesso' : status.ultimoCicloStatus ?? '-'}
              color={status.ultimoCicloStatus === 'O' ? 'green' : 'yellow'}
            />
          </div>
        )}

        <div className="mt-8 flex gap-4">
          <a href="/etapas" className="px-4 py-2 bg-gray-800 hover:bg-gray-700 rounded text-sm">Etapas</a>
          <a href="/logs" className="px-4 py-2 bg-gray-800 hover:bg-gray-700 rounded text-sm">Logs</a>
        </div>
      </div>
    </div>
  );
}

function StatCard({ label, value, color }: { label: string; value: string; color: string }) {
  const colors: Record<string, string> = {
    green: 'border-green-500 text-green-400',
    yellow: 'border-yellow-500 text-yellow-400',
    red: 'border-red-500 text-red-400',
    blue: 'border-blue-500 text-blue-400',
  };

  return (
    <div className={`p-4 bg-gray-900 rounded-lg border-l-4 ${colors[color]}`}>
      <p className="text-xs text-gray-500 uppercase">{label}</p>
      <p className="text-xl font-mono mt-1">{value}</p>
    </div>
  );
}
