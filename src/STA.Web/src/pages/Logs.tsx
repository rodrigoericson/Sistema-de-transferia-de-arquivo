import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../lib/api';
import type { ApiResponse, PaginatedResponse, LogArquivo } from '../types';
import Header from '../components/layout/Header';

export default function Logs() {
  const [logs, setLogs] = useState<LogArquivo[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState('');
  const [arquivo, setArquivo] = useState('');
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const pageSize = 15;

  useEffect(() => { fetchLogs(); }, [page, status]);

  const fetchLogs = async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      if (status) params.set('status', status);
      if (arquivo) params.set('arquivo', arquivo);

      const { data } = await api.get<ApiResponse<PaginatedResponse<LogArquivo>>>(`/logs/arquivos?${params}`);
      if (data.success && data.data) {
        setLogs(data.data.items);
        setTotal(data.data.total);
      }
    } catch { /* interceptor */ }
    setLoading(false);
  };

  const handleSearch = () => { setPage(1); fetchLogs(); };
  const pageCount = Math.ceil(total / pageSize);

  return (
    <div className="min-h-screen bg-gray-950 text-gray-100">
      <Header />
      <div className="max-w-6xl mx-auto p-8">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-mono text-green-400">Logs de Transferência</h1>
          <button onClick={() => navigate('/')} className="px-3 py-1.5 text-sm bg-gray-800 hover:bg-gray-700 rounded">Voltar</button>
        </div>

        <div className="flex gap-3 mb-4">
          <select value={status} onChange={(e) => { setStatus(e.target.value); setPage(1); }}
            className="px-3 py-1.5 bg-gray-800 border border-gray-700 rounded text-sm text-gray-100">
            <option value="">Todos</option>
            <option value="S">Sucesso</option>
            <option value="E">Erro</option>
          </select>

          <input value={arquivo} onChange={(e) => setArquivo(e.target.value)}
            placeholder="Buscar arquivo..."
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            className="px-3 py-1.5 bg-gray-800 border border-gray-700 rounded text-sm text-gray-100 w-64 focus:outline-none focus:border-green-500" />

          <button onClick={handleSearch} className="px-3 py-1.5 text-sm bg-gray-700 hover:bg-gray-600 rounded">Buscar</button>
        </div>

        {loading ? <p className="text-gray-500">Carregando...</p> : (
          <>
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-gray-500 border-b border-gray-800">
                  <th className="py-2 px-2">Arquivo</th>
                  <th className="py-2 px-2">Origem</th>
                  <th className="py-2 px-2">Destino</th>
                  <th className="py-2 px-2">Tamanho</th>
                  <th className="py-2 px-2">Status</th>
                  <th className="py-2 px-2">Data</th>
                </tr>
              </thead>
              <tbody>
                {logs.map((l) => (
                  <tr key={l.cnLogArquivo} className="border-b border-gray-800/50 hover:bg-gray-900/50">
                    <td className="py-2 px-2 font-mono text-xs">{l.nmArquivo}</td>
                    <td className="py-2 px-2 text-xs text-gray-400 truncate max-w-[200px]" title={l.dsDiretorioOrigem}>{l.dsDiretorioOrigem.split('/').pop()}</td>
                    <td className="py-2 px-2 text-xs text-gray-400 truncate max-w-[200px]" title={l.dsDiretorioDestino}>{l.dsDiretorioDestino.split('/').pop()}</td>
                    <td className="py-2 px-2 text-xs">{formatBytes(l.nrTamanhoBytes)}</td>
                    <td className="py-2 px-2">
                      <span className={`px-2 py-0.5 rounded text-xs ${l.idStatus === 'S' ? 'bg-green-900 text-green-300' : 'bg-red-900 text-red-300'}`}>
                        {l.idStatus === 'S' ? 'Sucesso' : 'Erro'}
                      </span>
                    </td>
                    <td className="py-2 px-2 text-xs text-gray-400">{new Date(l.dtInicio).toLocaleString('pt-BR')}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="flex justify-between items-center mt-4 text-sm text-gray-500">
              <span>{total} registro(s)</span>
              <div className="flex gap-2">
                <button disabled={page <= 1} onClick={() => setPage(page - 1)}
                  className="px-3 py-1 bg-gray-800 rounded disabled:opacity-30">Anterior</button>
                <span className="px-3 py-1">{page} / {pageCount}</span>
                <button disabled={page >= pageCount} onClick={() => setPage(page + 1)}
                  className="px-3 py-1 bg-gray-800 rounded disabled:opacity-30">Próximo</button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
