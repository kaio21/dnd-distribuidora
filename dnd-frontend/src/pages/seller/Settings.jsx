import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { Clock, ToggleLeft, ToggleRight, Save } from 'lucide-react'

export default function SellerSettings() {
  const [form, setForm] = useState({ openTime: '08:00', closeTime: '18:00', isOpen: true })
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    api.get('/settings')
      .then(r => setForm({ openTime: r.data.openTime, closeTime: r.data.closeTime, isOpen: r.data.isOpen }))
      .finally(() => setLoading(false))
  }, [])

  const handleSave = async () => {
    setSaving(true)
    try {
      await api.put('/settings', form)
      toast.success('Configurações salvas!')
    } catch {
      toast.error('Erro ao salvar configurações')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
  )

  return (
    <div className="p-4 md:p-8 max-w-2xl">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Configurações</h1>
        <p className="text-gray-500 text-sm mt-1">Horário de funcionamento da loja</p>
      </div>

      <div className="card space-y-6">
        {/* Toggle status */}
        <div className={`flex items-center justify-between p-4 rounded-xl ${form.isOpen ? 'bg-green-50' : 'bg-red-50'}`}>
          <div>
            <p className="font-semibold text-gray-800">Status da loja</p>
            <p className={`text-sm font-medium mt-0.5 ${form.isOpen ? 'text-green-600' : 'text-red-600'}`}>
              {form.isOpen ? '● Aberta — aceitando pedidos' : '● Fechada — não aceita pedidos'}
            </p>
          </div>
          <button
            onClick={() => setForm(f => ({ ...f, isOpen: !f.isOpen }))}
            className={`transition-colors ${form.isOpen ? 'text-green-500 hover:text-green-600' : 'text-gray-400 hover:text-gray-500'}`}>
            {form.isOpen ? <ToggleRight size={52} /> : <ToggleLeft size={52} />}
          </button>
        </div>

        {/* Horário abertura */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            <Clock size={14} className="inline mr-1" />
            Horário de abertura
          </label>
          <input
            type="time"
            value={form.openTime}
            onChange={e => setForm(f => ({ ...f, openTime: e.target.value }))}
            className="input" />
        </div>

        {/* Horário fechamento */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            <Clock size={14} className="inline mr-1" />
            Horário de fechamento
          </label>
          <input
            type="time"
            value={form.closeTime}
            onChange={e => setForm(f => ({ ...f, closeTime: e.target.value }))}
            className="input" />
        </div>

        {/* Resumo */}
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 text-sm text-blue-800">
          Pedidos serão aceitos das <strong>{form.openTime}</strong> às <strong>{form.closeTime}</strong> (horário de Brasília).
          {!form.isOpen && (
            <p className="mt-1 text-red-600 font-semibold">
              ⚠ Loja fechada manualmente — nenhum pedido será aceito.
            </p>
          )}
        </div>

        <button
          onClick={handleSave}
          disabled={saving}
          className="btn-primary w-full py-3 flex items-center justify-center gap-2">
          <Save size={18} />
          {saving ? 'Salvando...' : 'Salvar configurações'}
        </button>
      </div>
    </div>
  )
}
