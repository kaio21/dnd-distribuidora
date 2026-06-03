import { AlertTriangle, Trash2, X } from 'lucide-react'

export default function ConfirmModal({ open, title, description, confirmLabel = 'Excluir', onConfirm, onCancel, danger = true }) {
  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-sm p-6 animate-[fadeIn_0.15s_ease]">
        <div className="flex items-start gap-4">
          <div className={`flex-shrink-0 w-11 h-11 rounded-full flex items-center justify-center ${danger ? 'bg-red-100' : 'bg-yellow-100'}`}>
            <AlertTriangle size={22} className={danger ? 'text-red-500' : 'text-yellow-500'} />
          </div>
          <div className="flex-1 min-w-0">
            <h3 className="text-base font-bold text-gray-900 leading-snug">{title}</h3>
            {description && (
              <p className="text-sm text-gray-500 mt-1 leading-relaxed">{description}</p>
            )}
          </div>
          <button onClick={onCancel} className="text-gray-300 hover:text-gray-500 transition shrink-0">
            <X size={18} />
          </button>
        </div>

        <div className="flex gap-3 mt-6">
          <button
            onClick={onCancel}
            className="flex-1 py-2.5 rounded-xl border border-gray-200 text-gray-700 font-semibold text-sm hover:bg-gray-50 transition">
            Cancelar
          </button>
          <button
            onClick={onConfirm}
            className={`flex-1 py-2.5 rounded-xl font-semibold text-sm flex items-center justify-center gap-2 transition ${
              danger
                ? 'bg-red-500 hover:bg-red-600 text-white'
                : 'bg-yellow-500 hover:bg-yellow-600 text-white'
            }`}>
            <Trash2 size={15} />
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}
