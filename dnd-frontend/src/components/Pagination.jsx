import { ChevronLeft, ChevronRight } from 'lucide-react'

export default function Pagination({ page, totalPages, totalCount, onPageChange, itemLabel = 'item(ns)' }) {
  if (totalPages <= 1) return null

  return (
    <div className="flex items-center justify-between flex-wrap gap-3 mt-6">
      <p className="text-sm text-gray-500">
        {totalCount} {itemLabel} — página {page} de {totalPages}
      </p>
      <div className="flex items-center gap-2">
        <button
          onClick={() => onPageChange(page - 1)}
          disabled={page <= 1}
          className="btn-secondary flex items-center gap-1 text-sm py-1.5 px-3 disabled:opacity-40 disabled:cursor-not-allowed">
          <ChevronLeft size={16} /> Anterior
        </button>
        <button
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
          className="btn-secondary flex items-center gap-1 text-sm py-1.5 px-3 disabled:opacity-40 disabled:cursor-not-allowed">
          Próxima <ChevronRight size={16} />
        </button>
      </div>
    </div>
  )
}
