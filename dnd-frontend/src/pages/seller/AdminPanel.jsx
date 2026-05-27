import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { Users, CheckCircle, XCircle, Shield } from 'lucide-react'

export default function AdminPanel() {
  const [sellers, setSellers] = useState([])
  const [loading, setLoading] = useState(true)

  const load = () =>
    api.get('/admin/sellers')
      .then(r => setSellers(r.data))
      .catch(() => toast.error('Sem permissão ou erro ao carregar'))
      .finally(() => setLoading(false))

  useEffect(() => { load() }, [])

  const handleApprove = async (id) => {
    try {
      await api.patch(`/admin/sellers/${id}/approve`)
      toast.success('Vendedor aprovado!')
      load()
    } catch {
      toast.error('Erro ao aprovar')
    }
  }

  const handleRevoke = async (id) => {
    if (!confirm('Revogar acesso deste vendedor?')) return
    try {
      await api.patch(`/admin/sellers/${id}/revoke`)
      toast.success('Acesso revogado')
      load()
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao revogar')
    }
  }

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
  )

  const pending = sellers.filter(s => !s.isApproved)
  const approved = sellers.filter(s => s.isApproved)

  return (
    <div className="p-4 md:p-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Painel Administrativo</h1>
        <p className="text-gray-500 text-sm mt-1">Gerencie os vendedores cadastrados na plataforma</p>
      </div>

      {/* Pendentes */}
      {pending.length > 0 && (
        <div className="mb-6">
          <h2 className="text-sm font-bold text-yellow-700 mb-3 flex items-center gap-2">
            <XCircle size={16} />
            Aguardando aprovação ({pending.length})
          </h2>
          <div className="space-y-3">
            {pending.map(s => (
              <div key={s.id} className="bg-yellow-50 border border-yellow-200 rounded-xl p-4 flex items-center justify-between gap-3">
                <div>
                  <p className="font-semibold text-gray-900">{s.name}</p>
                  <p className="text-gray-500 text-sm">{s.email}</p>
                  <p className="text-gray-400 text-xs mt-0.5">Cadastrado em {new Date(s.createdAt).toLocaleDateString('pt-BR')}</p>
                </div>
                <button
                  onClick={() => handleApprove(s.id)}
                  className="flex items-center gap-1.5 bg-green-500 hover:bg-green-600 text-white font-semibold px-4 py-2 rounded-lg text-sm transition shrink-0">
                  <CheckCircle size={15} />
                  Aprovar
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Aprovados */}
      <div>
        <h2 className="text-sm font-bold text-gray-600 mb-3 flex items-center gap-2">
          <Users size={16} />
          Vendedores ativos ({approved.length})
        </h2>
        {approved.length === 0 ? (
          <div className="card text-center py-10">
            <p className="text-gray-400 text-sm">Nenhum vendedor ativo</p>
          </div>
        ) : (
          <div className="card p-0 overflow-hidden">
            {approved.map((s, i) => (
              <div key={s.id} className={`flex items-center justify-between px-5 py-4 gap-3 ${i > 0 ? 'border-t border-gray-100' : ''}`}>
                <div className="flex items-center gap-3 min-w-0">
                  {s.isAdmin && <Shield size={15} className="text-blue-500 shrink-0" />}
                  <div className="min-w-0">
                    <p className="font-semibold text-gray-900 flex items-center gap-2 flex-wrap">
                      {s.name}
                      {s.isAdmin && <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full font-bold">Admin</span>}
                    </p>
                    <p className="text-gray-500 text-sm truncate">{s.email}</p>
                  </div>
                </div>
                {!s.isAdmin && (
                  <button
                    onClick={() => handleRevoke(s.id)}
                    className="flex items-center gap-1.5 bg-red-50 hover:bg-red-100 text-red-600 font-semibold px-3 py-1.5 rounded-lg text-xs transition shrink-0">
                    <XCircle size={13} />
                    Revogar
                  </button>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
