import { useState, useCallback } from 'react';
import api from '../lib/api';

interface ValidacaoResult {
  status: 'idle' | 'validando' | 'existe' | 'criado' | 'sem_permissao' | 'inacessivel' | 'erro';
  mensagem: string;
  ok: boolean;
}

export function useValidarDiretorio() {
  const [resultado, setResultado] = useState<Record<string, ValidacaoResult>>({});

  const validar = useCallback(async (key: string, path: string) => {
    if (!path.trim()) {
      setResultado(prev => ({ ...prev, [key]: { status: 'idle', mensagem: '', ok: false } }));
      return;
    }

    setResultado(prev => ({ ...prev, [key]: { status: 'validando', mensagem: 'Verificando...', ok: false } }));

    try {
      const { data } = await api.post('/diretorios/validar', { path: path.trim() });
      if (data.success && data.data) {
        setResultado(prev => ({ ...prev, [key]: {
          status: data.data.status,
          mensagem: data.data.mensagem,
          ok: data.data.ok,
        }}));
      }
    } catch {
      setResultado(prev => ({ ...prev, [key]: { status: 'erro', mensagem: 'Falha na validação', ok: false } }));
    }
  }, []);

  return { resultado, validar };
}
