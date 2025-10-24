import React from 'react';
import { Search, X } from 'lucide-react';

interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  onClear?: () => void;
}

const SearchBar: React.FC<SearchBarProps> = ({
  value,
  onChange,
  placeholder = "Search...",
  onClear
}) => {
  return (
    <div style={{ position: 'relative', width: '100%' }}>
      <div style={{ position: 'absolute', left: '0.75rem', top: '50%', transform: 'translateY(-50%)', pointerEvents: 'none' }}>
        <Search size={20} color="#9CA3AF" />
      </div>
      <input
        type="text"
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        style={{
          width: '100%',
          padding: '0.75rem 2.5rem 0.75rem 2.75rem',
          border: '2px solid #D1D5DB',
          borderRadius: '8px',
          fontSize: '1rem',
          outline: 'none',
          transition: 'border-color 0.2s'
        }}
        onFocus={(e) => e.target.style.borderColor = '#3B82F6'}
        onBlur={(e) => e.target.style.borderColor = '#D1D5DB'}
      />
      {value && (
        <button
          onClick={() => {
            onChange('');
            onClear?.();
          }}
          style={{
            position: 'absolute',
            right: '0.75rem',
            top: '50%',
            transform: 'translateY(-50%)',
            background: 'none',
            border: 'none',
            cursor: 'pointer',
            padding: '0.25rem',
            display: 'flex',
            alignItems: 'center',
            color: '#6B7280'
          }}
          title="Clear search"
        >
          <X size={18} />
        </button>
      )}
    </div>
  );
};

export default SearchBar;

