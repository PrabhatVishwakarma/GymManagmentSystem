import React from 'react';
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  itemsPerPage: number;
  onPageChange: (page: number) => void;
  onItemsPerPageChange: (itemsPerPage: number) => void;
}

const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  totalItems,
  itemsPerPage,
  onPageChange,
  onItemsPerPageChange
}) => {
  const startItem = (currentPage - 1) * itemsPerPage + 1;
  const endItem = Math.min(currentPage * itemsPerPage, totalItems);

  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisiblePages = 5;

    if (totalPages <= maxVisiblePages + 2) {
      // Show all pages if total is small
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Always show first page
      pages.push(1);

      if (currentPage > 3) {
        pages.push('...');
      }

      // Show pages around current page
      const startPage = Math.max(2, currentPage - 1);
      const endPage = Math.min(totalPages - 1, currentPage + 1);

      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }

      if (currentPage < totalPages - 2) {
        pages.push('...');
      }

      // Always show last page
      if (totalPages > 1) {
        pages.push(totalPages);
      }
    }

    return pages;
  };

  return (
    <div style={{
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      padding: '1rem',
      backgroundColor: 'white',
      borderTop: '1px solid #E5E7EB',
      flexWrap: 'wrap',
      gap: '1rem'
    }}>
      {/* Items per page selector */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
        <span style={{ color: '#6B7280', fontSize: '0.875rem' }}>Show:</span>
        <select
          value={itemsPerPage}
          onChange={(e) => onItemsPerPageChange(Number(e.target.value))}
          style={{
            padding: '0.5rem',
            border: '1px solid #D1D5DB',
            borderRadius: '6px',
            fontSize: '0.875rem',
            cursor: 'pointer',
            outline: 'none'
          }}
        >
          <option value={10}>10</option>
          <option value={25}>25</option>
          <option value={50}>50</option>
          <option value={100}>100</option>
          <option value={200}>200</option>
        </select>
        <span style={{ color: '#6B7280', fontSize: '0.875rem' }}>per page</span>
      </div>

      {/* Page info */}
      <div style={{ color: '#6B7280', fontSize: '0.875rem' }}>
        Showing <strong>{startItem}</strong> to <strong>{endItem}</strong> of <strong>{totalItems}</strong> items
      </div>

      {/* Page navigation */}
      <div style={{ display: 'flex', gap: '0.25rem', alignItems: 'center' }}>
        {/* First page */}
        <button
          onClick={() => onPageChange(1)}
          disabled={currentPage === 1}
          style={{
            padding: '0.5rem',
            border: '1px solid #D1D5DB',
            borderRadius: '6px',
            backgroundColor: currentPage === 1 ? '#F3F4F6' : 'white',
            cursor: currentPage === 1 ? 'not-allowed' : 'pointer',
            display: 'flex',
            alignItems: 'center',
            opacity: currentPage === 1 ? 0.5 : 1
          }}
          title="First page"
        >
          <ChevronsLeft size={16} />
        </button>

        {/* Previous page */}
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage === 1}
          style={{
            padding: '0.5rem',
            border: '1px solid #D1D5DB',
            borderRadius: '6px',
            backgroundColor: currentPage === 1 ? '#F3F4F6' : 'white',
            cursor: currentPage === 1 ? 'not-allowed' : 'pointer',
            display: 'flex',
            alignItems: 'center',
            opacity: currentPage === 1 ? 0.5 : 1
          }}
          title="Previous page"
        >
          <ChevronLeft size={16} />
        </button>

        {/* Page numbers */}
        {getPageNumbers().map((page, index) => (
          typeof page === 'number' ? (
            <button
              key={index}
              onClick={() => onPageChange(page)}
              style={{
                padding: '0.5rem 0.75rem',
                border: '1px solid #D1D5DB',
                borderRadius: '6px',
                backgroundColor: currentPage === page ? '#3B82F6' : 'white',
                color: currentPage === page ? 'white' : '#374151',
                cursor: 'pointer',
                fontWeight: currentPage === page ? '600' : '400',
                fontSize: '0.875rem',
                minWidth: '2.5rem',
                transition: 'all 0.2s'
              }}
              onMouseEnter={(e) => {
                if (currentPage !== page) {
                  e.currentTarget.style.backgroundColor = '#F3F4F6';
                }
              }}
              onMouseLeave={(e) => {
                if (currentPage !== page) {
                  e.currentTarget.style.backgroundColor = 'white';
                }
              }}
            >
              {page}
            </button>
          ) : (
            <span
              key={index}
              style={{
                padding: '0.5rem',
                color: '#9CA3AF',
                fontSize: '0.875rem'
              }}
            >
              {page}
            </span>
          )
        ))}

        {/* Next page */}
        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage === totalPages}
          style={{
            padding: '0.5rem',
            border: '1px solid #D1D5DB',
            borderRadius: '6px',
            backgroundColor: currentPage === totalPages ? '#F3F4F6' : 'white',
            cursor: currentPage === totalPages ? 'not-allowed' : 'pointer',
            display: 'flex',
            alignItems: 'center',
            opacity: currentPage === totalPages ? 0.5 : 1
          }}
          title="Next page"
        >
          <ChevronRight size={16} />
        </button>

        {/* Last page */}
        <button
          onClick={() => onPageChange(totalPages)}
          disabled={currentPage === totalPages}
          style={{
            padding: '0.5rem',
            border: '1px solid #D1D5DB',
            borderRadius: '6px',
            backgroundColor: currentPage === totalPages ? '#F3F4F6' : 'white',
            cursor: currentPage === totalPages ? 'not-allowed' : 'pointer',
            display: 'flex',
            alignItems: 'center',
            opacity: currentPage === totalPages ? 0.5 : 1
          }}
          title="Last page"
        >
          <ChevronsRight size={16} />
        </button>
      </div>
    </div>
  );
};

export default Pagination;

