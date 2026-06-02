import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { Users, XCircle, Shield, UserPlus, Eye, EyeOff } from 'lucide-react'
import { maskCPF, stripMask } from '../../utils/masks'

const roleLabel = { Admin: 'Admin', Gerente: 'Gerente', Vendedor: 'Vendedor' }
const roleBadge = {
  Admin:   'bg-yellow-100 text-yellow-800',
  Gerente: 'bg-purple-100 text-purple-800',
  Vendedor:'bg-blue-100 text-blue-700',
}

const emptyForm = { name: '', cpf: '', email: '', password: '', role: 'Vendedor' }

export default function AdminPanel() {
  const [sellers, setSellers] = useState([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState(emptyForm)
  const [showPass, setShowPass] = useState(false)
  const [saving, setSaving] = useState(false)

  const load = () =>
    api.get('/admin/sellers')
      .then(r => setSellers(r.data))
      .catch(() => toast.error('Sem permissão ou erro ao carregar'))
      .finally(() => setLoading(false))

  useEffect(() => { load() }, [])

  const handleCreate = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      await api.post('/admin/sellers', { ...form, cpf: stripMask(form.cpf) })
      toast.success(`${form.role} criado com sucesso!`)
      setForm(emptyForm)
      setShowForm(false)
      load()
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao criar conta')
    } finally {
      setSaving(false)
    }
  }

  const handleRevoke = async (s) => {
    if (!confirm(`Revogar acesso de "${s.name}"?`)) return
    try {
      await api.patch(`/admin/sellers/${s.id}/revoke`)
      toast.success('Acesso revogado')
      load()
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao revogar')
    }
  }

  const handleRestore = async (s) => {
    try {
      await api.patch(`/admin/sellers/${s.id}/approve`)
      toast.success('Acesso restaurado')
      load()
    } catch {
      toast.error('Erro ao restaurar acesso')
    }
  }

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
  )

  const active   = sellers.filter(s => s.isApproved)
  const revoked  = sellers.filter(s => !s.isApproved)

  return (
    <div className="p-4 md:p-8">
      <div className="flex items-center justify-between mb-8 flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Painel Administrativo</h1>
          <p className="text-gray-500 text-sm mt-1">Gerencie os perfis da plataforma</p>
        </div>
        <button onClick={() => setShowForm(true)}
          className="btn-primary flex items-center gap-2">
          <UserPlus size={18} /> Criar conta
        </button>
      </div>

      {/* Lista de perfis ativos */}
      <div className="mb-6">
        <h2 className="text-sm font-bold text-gray-600 mb-3 flex items-center gap-2">
          <Users size={16} /> Perfis ativos ({active.length})
        </h2>
        {active.length === 0 ? (
          <div className="card text-center py-10">
            <p className="text-gray-400 text-sm">Nenhum perfil ativo</p>
          </div>
        ) : (
          <div className="card p-0 overflow-hidden">
            {active.map((s, i) => (
              <div key={s.id} className={`flex items-center justify-between px-5 py-4 gap-3 ${i > 0 ? 'border-t border-gray-100' : ''}`}>
                <div className="flex items-center gap-3 min-w-0">
                  {s.role === 'Admin' && <Shield size={15} className="text-yellow-500 shrink-0" />}
                  <div className="min-w-0">
                    <p className="font-semibold text-gray-900 flex items-center gap-2 flex-wrap">
                      {s.name}
                      <span className={`text-xs px-2 py-0.5 rounded-full font-bold ${roleBadge[s.role] ?? 'bg-gray-100 text-gray-700'}`}>
                        {roleLabel[s.role] ?? s.role}
                      </span>
                    </p>
                    <p className="text-gray-500 text-sm truncate">{s.email}</p>
                  </div>
                </div>
                {s.role !== 'Admin' && (
                  <button onClick={() => handleRevoke(s)}
                    className="flex items-center gap-1.5 bg-red-50 hover:bg-red-100 text-red-600 font-semibold px-3 py-1.5 rounded-lg text-xs transition shrink-0">
                    <XCircle size={13} /> Revogar
                  </button>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Perfis com acesso revogado */}
      {revoked.length > 0 && (
        <div>
          <h2 className="text-sm font-bold text-gray-500 mb-3">Acesso revogado ({revoked.length})</h2>
          <div className="card p-0 overflow-hidden opacity-60">
            {revoked.map((s, i) => (
              <div key={s.id} className={`flex items-center justify-between px-5 py-4 gap-3 ${i > 0 ? 'border-t border-gray-100' : ''}`}>
                <div>
                  <p className="font-semibold text-gray-700">{s.name}</p>
                  <p className="text-gray-400 text-sm">{s.email}</p>
                </div>
                <button onClick={() => handleRestore(s)}
                  className="text-xs bg-green-50 hover:bg-green-100 text-green-700 font-semibold px-3 py-1.5 rounded-lg transition shrink-0">
                  Restaurar
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Modal criar conta */}
      {showForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
            <h2 className="text-lg font-bold text-gray-900 mb-5">Criar nova conta</h2>
            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Perfil</label>
                <select value={form.role} onChange={e => setForm(f => ({...f, role: e.target.value}))} className="input">
                  <option value="Vendedor">Vendedor</option>
                  <option value="Gerente">Gerente</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Nome completo</label>
                <input value={form.name} onChange={e => setForm(f => ({...f, name: e.target.value}))}
                  required placeholder="Nome do vendedor" className="input" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  CPF <span className="text-gray-400 font-normal">(opcional)</span>
                </label>
                <input value={form.cpf} onChange={e => setForm(f => ({...f, cpf: maskCPF(e.target.value)}))}
                  placeholder="000.000.000-00" className="input" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">E-mail</label>
                <input value={form.email} onChange={e => setForm(f => ({...f, email: e.target.value}))}
                  required type="email" placeholder="email@exemplo.com" className="input" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Senha</label>
                <div className="relative">
                  <input value={form.password} onChange={e => setForm(f => ({...f, password: e.target.value}))}
                    required type={showPass ? 'text' : 'password'} placeholder="Mínimo 6 caracteres"
                    minLength={6} className="input pr-10" />
                  <button type="button" onClick={() => setShowPass(p => !p)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
                    {showPass ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>
              <div className="flex gap-3 pt-2">
                <button type="button" onClick={() => { setShowForm(false); setForm(emptyForm) }}
                  className="btn-secondary flex-1">Cancelar</button>
                <button type="submit" disabled={saving} className="btn-primary flex-1">
                  {saving ? 'Criando...' : 'Criar conta'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
