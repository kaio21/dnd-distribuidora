import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import api from '../../api/axios'
import { useAuth } from '../../context/AuthContext'
import { Store, Eye, EyeOff } from 'lucide-react'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [showPass, setShowPass] = useState(false)
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm()

  const onSubmit = async ({ email, password }) => {
    // Tenta login como vendedor primeiro; se falhar, tenta como comprador
    try {
      const res = await api.post('/auth/seller/login', { email, password })
      login(res.data)
      navigate('/seller/dashboard')
      return
    } catch {
      // não é vendedor — tenta comprador
    }

    try {
      const res = await api.post('/auth/buyer/login', { email, password })
      login(res.data)
      navigate('/buyer/shop')
    } catch (err) {
      toast.error(err.response?.data?.message || 'E-mail ou senha inválidos')
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-900 to-blue-700 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-8">
        <div className="flex items-center gap-3 mb-8">
          <div className="bg-blue-600 rounded-xl w-12 h-12 flex items-center justify-center">
            <Store size={22} className="text-white" />
          </div>
          <div>
            <h1 className="text-xl font-bold text-gray-900">D&D Distribuidora</h1>
            <p className="text-gray-500 text-sm">Acesse sua conta</p>
          </div>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">E-mail</label>
            <input
              {...register('email', { required: 'E-mail obrigatório' })}
              type="email" placeholder="seu@email.com"
              className="input" />
            {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Senha</label>
            <div className="relative">
              <input
                {...register('password', { required: 'Senha obrigatória' })}
                type={showPass ? 'text' : 'password'} placeholder="••••••••"
                className="input pr-10" />
              <button type="button" onClick={() => setShowPass(p => !p)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                {showPass ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password.message}</p>}
          </div>

          <button type="submit" disabled={isSubmitting} className="btn-primary w-full py-3">
            {isSubmitting ? 'Entrando...' : 'Entrar'}
          </button>
        </form>

        <p className="text-center text-sm text-gray-600 mt-6">
          Não tem conta?{' '}
          <Link to="/buyer/register" className="text-blue-600 hover:underline font-medium">Cadastrar empresa</Link>
        </p>
        <p className="text-center text-sm text-gray-500 mt-2">
          <Link to="/" className="hover:underline">← Voltar ao início</Link>
        </p>
      </div>
    </div>
  )
}
