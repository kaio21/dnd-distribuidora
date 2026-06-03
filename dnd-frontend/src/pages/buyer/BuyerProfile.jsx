import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { useAuth } from '../../context/AuthContext'
import { User, Store, Phone, MapPin, Lock, Save, Eye, EyeOff } from 'lucide-react'
import { maskPhone, stripMask, maskCPF, maskCNPJ } from '../../utils/masks'

export default function BuyerProfile() {
  const { user, login } = useAuth()
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [changingPass, setChangingPass] = useState(false)
  const [showCurrent, setShowCurrent] = useState(false)
  const [showNew, setShowNew] = useState(false)

  const [form, setForm] = useState({ name: '', storeName: '', phone: '', address: '' })
  const [passForm, setPassForm] = useState({ currentPassword: '', newPassword: '', confirm: '' })

  useEffect(() => {
    api.get('/buyers/me').then(r => {
      const d = r.data
      setForm({
        name: d.name || '',
        storeName: d.storeName || '',
        phone: d.phone ? maskPhone(d.phone) : '',
        address: d.address || '',
      })
    }).finally(() => setLoading(false))
  }, [])

  const handleSave = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      const res = await api.put('/buyers/me', {
        name: form.name,
        storeName: form.storeName,
        phone: stripMask(form.phone),
        address: form.address,
      })
      // atualiza o nome no contexto de auth
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
      await api.patch('/buyers/me/password', {
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

  return (
    <div className="p-4 md:p-8 max-w-2xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Meu Perfil</h1>
        <p className="text-gray-500 text-sm mt-1">Edite suas informações de cadastro</p>
      </div>

      {/* Dados do perfil */}
      <div className="card">
        <h2 className="text-sm font-bold text-gray-700 mb-5 flex items-center gap-2">
          <User size={16} className="text-blue-500" /> Dados pessoais e da empresa
        </h2>
        <form onSubmit={handleSave} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Nome completo</label>
              <input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                required className="input" placeholder="Seu nome" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                <Store size={13} className="inline mr-1" />Nome da loja
              </label>
              <input value={form.storeName} onChange={e => setForm(f => ({ ...f, storeName: e.target.value }))}
                required className="input" placeholder="Nome da sua empresa" />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              <Phone size={13} className="inline mr-1" />Telefone / WhatsApp
            </label>
            <input
              value={form.phone}
              onChange={e => setForm(f => ({ ...f, phone: maskPhone(e.target.value) }))}
              className="input" placeholder="(00) 00000-0000" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              <MapPin size={13} className="inline mr-1" />Endereço completo
            </label>
            <input value={form.address} onChange={e => setForm(f => ({ ...f, address: e.target.value }))}
              required className="input" placeholder="Rua, número, bairro, cidade/UF" />
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
