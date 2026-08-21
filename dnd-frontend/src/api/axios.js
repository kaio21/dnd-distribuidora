import axios from 'axios'
import toast from 'react-hot-toast'
import { API_URL } from './config'

const api = axios.create({
  baseURL: `${API_URL}/api`,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30000
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// O backend fica hospedado num plano gratuito que hiberna após ociosidade.
// Um timeout/erro de rede sem resposta do servidor é o sintoma típico dele
// "acordando" — em vez de mostrar erro de cara, tenta de novo por conta própria.
const MAX_RETRIES = 3
const RETRY_DELAY_MS = 6000
let wakingToastId = null

const pareceServidorAcordando = (err) =>
  !err.response && (err.code === 'ECONNABORTED' || err.message === 'Network Error')

api.interceptors.response.use(
  res => {
    if (wakingToastId) {
      toast.dismiss(wakingToastId)
      wakingToastId = null
    }
    return res
  },
  async err => {
    const config = err.config || {}

    if (pareceServidorAcordando(err)) {
      config.__retryCount = (config.__retryCount || 0) + 1
      if (config.__retryCount <= MAX_RETRIES) {
        if (!wakingToastId) {
          wakingToastId = toast.loading(
            'Conectando ao servidor... pode levar até 1 minuto no primeiro acesso.',
            { duration: 60000 }
          )
        }
        await new Promise(resolve => setTimeout(resolve, RETRY_DELAY_MS))
        return api(config)
      }
    }

    if (wakingToastId) {
      toast.dismiss(wakingToastId)
      wakingToastId = null
    }

    const url = config.url || ''
    // Não redireciona em endpoints de autenticação — o login testa credenciais intencionalmente
    if (err.response?.status === 401 && !url.includes('/auth/')) {
      localStorage.clear()
      window.location.href = '/'
    }
    return Promise.reject(err)
  }
)

export default api
