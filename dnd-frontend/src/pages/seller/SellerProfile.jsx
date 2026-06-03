import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { useAuth } from '../../context/AuthContext'
import { User, Lock, Save, Eye, EyeOff } from 'lucide-react'

export default function SellerProfile() {
  const { login } = useAuth()
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [changingPass, setChangingPass] = useState(false)
  const [showCurrent, setShowCurrent] = useState(false)
  const [showNew, setShowNew] = useState(false)

  const [form, setForm] = useState({ name: '', email: '', role: '' })
  const [passForm, setPassForm] = useState({ currentPassword: '', newPassword: '', confirm: '' })

  useEffect(() => {
    api.get('/sellers/me').then(r => {
      setForm({ name: r.data.name, email: r.data.email, role: r.data.role })
    }).finally(() => setLoading(false))
  }, [])

  const handleSave = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      const res = await api.put('/sellers/me', { name: form.name })
      login({ ...JSON.parse(localStorage.getItem('user') || '{}'), name: res.data.name })
      toast.success('Perfil atualizado!')
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao salvar')
    } finally {
      setSaving(false)
    }
  }

  const handleChangePass = async (e) => {
    e.preventDefault()
    if (passForm.newPassword !== passForm.confirm) {
      toast.error('A nova senha e a confirmação não conferem.')
      return
    }
    setChangingPass(true)
    try {
      await api.patch('/sellers/me/password', {
        currentPassword: passForm.currentPassword,
        newPassword: passForm.newPassword,
      })
      toast.success('Senha alterada com sucesso!')
      setPassForm({ currentPassword: '', newPassword: '', confirm: '' })
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao alterar senha')
    } finally {
      setChangingPass(false)
    }
  }

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
    </div>
  )

  const roleBadge = {
    Admin:    'bg-yellow-100 text-yellow-800',
    Gerente:  'bg-purple-100 text-purple-800',
    Vendedor: 'bg-blue-100 text-blue-800',
  }

  return (
    <div className="p-4 md:p-8 max-w-2xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Meu Perfil</h1>
        <p className="text-gray-500 text-sm mt-1">Edite suas informações de acesso</p>
      </div>

      {/* Dados do perfil */}
      <div className="card">
        <h2 className="text-sm font-bold text-gray-700 mb-5 flex items-center gap-2">
          <User size={16} className="text-blue-500" /> Dados da conta
        </h2>

        <div className="flex items-center gap-3 mb-5 p-3 bg-gray-50 rounded-xl">
          <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center shrink-0">
            <User size={18} className="text-blue-600" />
          </div>
          <div>
            <p className="font-semibold text-gray-900 text-sm">{form.name}</p>
            <p className="text-gray-500 text-xs">{form.email}</p>
          </div>
          <span className={`ml-auto text-xs font-bold px-2 py-0.5 rounded-full ${roleBadge[form.role] ?? 'bg-gray-100 text-gray-700'}`}>
            {form.role}
          </span>
        </div>

        <form onSubmit={handleSave} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Nome</label>
            <input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
              required className="input" placeholder="Seu nome completo" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">E-mail</label>
            <input value={form.email} disabled
              className="input bg-gray-50 text-gray-400 cursor-not-allowed" />
            <p className="text-xs text-gray-400 mt-1">O e-mail não pode ser alterado.</p>
          </div>

          <div className="pt-1">
            <button type="submit" disabled={saving} className="btn-primary flex items-center gap-2">
              <Save size={16} />
              {saving ? 'Salvando...' : 'Salvar alterações'}
            </button>
          </div>
        </form>
      </div>

      {/* Alterar senha */}
      <div className="card">
        <h2 className="text-sm font-bold text-gray-700 mb-5 flex items-center gap-2">
          <Lock size={16} className="text-blue-500" /> Alterar senha
        </h2>
        <form onSubmit={handleChangePass} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Senha atual</label>
            <div className="relative">
              <input
                type={showCurrent ? 'text' : 'password'}
                value={passForm.currentPassword}
                onChange={e => setPassForm(f => ({ ...f, currentPassword: e.target.value }))}
                required className="input pr-10" placeholder="••••••••" />
              <button type="button" onClick={() => setShowCurrent(p => !p)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                {showCurrent ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Nova senha</label>
              <div className="relative">
                <input
                  type={showNew ? 'text' : 'password'}
                  value={passForm.newPassword}
                  onChange={e => setPassForm(f => ({ ...f, newPassword: e.target.value }))}
                  required minLength={6} className="input pr-10" placeholder="Mínimo 6 caracteres" />
                <button type="button" onClick={() => setShowNew(p => !p)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                  {showNew ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Confirmar nova senha</label>
              <input
                type="password"
                value={passForm.confirm}
                onChange={e => setPassForm(f => ({ ...f, confirm: e.target.value }))}
                required className="input" placeholder="Repita a nova senha" />
            </div>
          </div>

          <div className="pt-1">
            <button type="submit" disabled={changingPass} className="btn-primary flex items-center gap-2">
              <Lock size={16} />
              {changingPass ? 'Alterando...' : 'Alterar senha'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
