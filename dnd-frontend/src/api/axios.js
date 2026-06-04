import axios from 'axios'
import { API_URL } from './config'

const api = axios.create({
  baseURL: `${API_URL}/api`,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  res => res,
  err => {
    const url = err.config?.url || ''
    // Não redireciona em endpoints de autenticação — o login testa credenciais intencionalmente
    if (err.response?.status === 401 && !url.includes('/auth/')) {
      localStorage.clear()
      window.location.href = '/'
    }
    return Promise.reject(err)
  }
)

export default api
