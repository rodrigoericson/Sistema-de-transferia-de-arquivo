import { useEffect, useRef } from 'react';

interface Props {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  validacao?: { status: string; mensagem: string; ok: boolean };
  onValidar?: () => void;
}

export default function DiretorioInput({ value, onChange, placeholder, validacao, onValidar }: Props) {
  const timer = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (!onValidar || !value.trim()) return;
    if (timer.current) clearTimeout(timer.current);
    timer.current = setTimeout(() => onValidar(), 500);
    return () => { if (timer.current) clearTimeout(timer.current); };
  }, [value]);

  const borderColor = !validacao || validacao.status === 'idle' ? 'border-gray-700'
    : validacao.status === 'validando' ? 'border-blue-500'
    : validacao.ok ? 'border-green-500'
    : 'border-red-500';

  const icon = !validacao || validacao.status === 'idle' ? null
    : validacao.status === 'validando' ? <span className="text-blue-400 text-xs">⟳</span>
    : validacao.ok ? <span className="text-green-400 text-sm">✓</span>
    : <span className="text-red-400 text-sm">✗</span>;

  return (
    <div>
      <div className="relative">
        <input
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder={placeholder}
          className={`w-full px-3 py-2 pr-8 bg-gray-800 border ${borderColor} rounded text-gray-100 text-sm font-mono focus:outline-none transition-colors`}
        />
        {icon && <span className="absolute right-3 top-1/2 -translate-y-1/2">{icon}</span>}
      </div>
      {validacao && validacao.status !== 'idle' && validacao.status !== 'validando' && (
        <p className={`text-xs mt-1 ${validacao.ok ? 'text-green-500' : 'text-red-400'}`}>
          {validacao.mensagem}
        </p>
      )}
    </div>
  );
}
