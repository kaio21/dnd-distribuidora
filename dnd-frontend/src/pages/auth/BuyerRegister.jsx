import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import api from '../../api/axios'
import { useAuth } from '../../context/AuthContext'
import { ShoppingCart, Eye, EyeOff } from 'lucide-react'
import { maskCPF, maskCNPJ, maskPhone, stripMask } from '../../utils/masks'

export default function BuyerRegister() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [showPass, setShowPass] = useState(false)
  const [cpf, setCpf] = useState('')
  const [cnpj, setCnpj] = useState('')
  const [phone, setPhone] = useState('')
  const { register, handleSubmit, setValue, formState: { errors, isSubmitting } } = useForm()

  const onSubmit = async (data) => {
    try {
      const payload = {
        ...data,
        cpf: stripMask(cpf),
        cnpj: stripMask(cnpj),
        phone: stripMask(phone),
      }
      const res = await api.post('/auth/buyer/register', payload)
      login(res.data)
      toast.success('Conta criada com sucesso!')
      navigate('/buyer/shop')
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao cadastrar. Verifique os dados.')
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 to-gray-800 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg p-8">
        <div className="flex items-center gap-3 mb-8">
          <div className="bg-blue-600 rounded-xl w-12 h-12 flex items-center justify-center">
            <ShoppingCart size={22} className="text-white" />
          </div>
          <div>
            <h1 className="text-xl font-bold text-gray-900">Cadastro de Comprador</h1>
            <p className="text-gray-500 text-sm">D&D Distribuidora</p>
          </div>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Nome completo</label>
              <input {...register('name', { required: 'Obrigatório' })}
                placeholder="João Silva" className="input" />
              {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name.message}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Nome da loja</label>
              <input {...register('storeName', { required: 'Obrigatório' })}
                placeholder="Loja do João" className="input" />
              {errors.storeName && <p className="text-red-500 text-xs mt-1">{errors.storeName.message}</p>}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">CPF</label>
              <input value={cpf} onChange={e => setCpf(maskCPF(e.target.value))}
                placeholder="000.000.000-00" className="input" required />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">CNPJ</label>
              <input value={cnpj} onChange={e => setCnpj(maskCNPJ(e.target.value))}
                placeholder="00.000.000/0001-00" className="input" required />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">E-mail</label>
            <input {...register('email', { required: 'Obrigatório' })}
              type="email" placeholder="loja@email.com" className="input" />
            {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Telefone / WhatsApp</label>
            <input value={phone} onChange={e => setPhone(maskPhone(e.target.value))}
              placeholder="(11) 99999-9999" className="input" required />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Endereço completo</label>
            <input {...register('address', { required: 'Obrigatório' })}
              placeholder="Rua, número, bairro, cidade/UF" className="input" />
            {errors.address && <p className="text-red-500 text-xs mt-1">{errors.address.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Senha</label>
            <div className="relative">
              <input {...register('password', { required: 'Obrigatório', minLength: { value: 6, message: 'Mínimo 6 caracteres' } })}
                type={showPass ? 'text' : 'password'} placeholder="••••••••" className="input pr-10" />
              <button type="button" onClick={() => setShowPass(!showPass)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
                {showPass ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password.message}</p>}
          </div>

          <button type="submit" disabled={isSubmitting} className="btn-primary w-full py-3">
            {isSubmitting ? 'Criando conta...' : 'Criar conta'}
          </button>
        </form>

        <p className="text-center text-sm text-gray-600 mt-5">
          Já tem conta?{' '}
          <Link to="/buyer/login" className="text-blue-600 hover:underline font-medium">Entrar</Link>
        </p>
        <p className="text-center text-sm text-gray-500 mt-2">
          <Link to="/" className="hover:underline">← Voltar ao início</Link>
        </p>
      </div>
    </div>
  )
}
