import { useState, useEffect, useRef } from 'react'
import { MapPin, X, Search, Loader2, ChevronDown, CheckCircle2 } from 'lucide-react'

const CEP_KEY = 'dnd_cep_endereco'

function salvarEndereco(dados) {
  localStorage.setItem(CEP_KEY, JSON.stringify(dados))
}

function carregarEndereco() {
  try {
    return JSON.parse(localStorage.getItem(CEP_KEY))
  } catch {
    return null
  }
}

export default function BarraCep() {
  const [endereco, setEndereco] = useState(carregarEndereco)
  const [aberto, setAberto] = useState(false)
  const [cep, setCep] = useState('')
  const [buscando, setBuscando] = useState(false)
  const [erro, setErro] = useState('')
  const modalRef = useRef(null)
  const inputRef = useRef(null)

  // fecha modal ao clicar fora
  useEffect(() => {
    function handleClick(e) {
      if (modalRef.current && !modalRef.current.contains(e.target)) {
        setAberto(false)
      }
    }
    if (aberto) {
      document.addEventListener('mousedown', handleClick)
      setTimeout(() => inputRef.current?.focus(), 50)
    }
    return () => document.removeEventListener('mousedown', handleClick)
  }, [aberto])

  function formatarCep(valor) {
    return valor.replace(/\D/g, '').slice(0, 8).replace(/^(\d{5})(\d)/, '$1-$2')
  }

  async function buscarCep(e) {
    e.preventDefault()
    const numeros = cep.replace(/\D/g, '')
    if (numeros.length !== 8) {
      setErro('CEP deve ter 8 dígitos.')
      return
    }
    setBuscando(true)
    setErro('')
    try {
      const res = await fetch(`https://viacep.com.br/ws/${numeros}/json/`)
      const dados = await res.json()
      if (dados.erro) {
        setErro('CEP não encontrado. Verifique e tente novamente.')
        return
      }
      const novo = {
        cep: dados.cep,
        logradouro: dados.logradouro,
        bairro: dados.bairro,
        cidade: dados.localidade,
        uf: dados.uf,
        resumo: `${dados.bairro ? dados.bairro + ', ' : ''}${dados.localidade} - ${dados.uf}`
      }
      setEndereco(novo)
      salvarEndereco(novo)
      setAberto(false)
      setCep('')
    } catch {
      setErro('Falha ao consultar o CEP. Tente novamente.')
    } finally {
      setBuscando(false)
    }
  }

  function limpar() {
    localStorage.removeItem(CEP_KEY)
    setEndereco(null)
    setCep('')
    setErro('')
  }

  return (
    <div className="relative">
      {/* Barra de endereço */}
      <button
        onClick={() => setAberto(true)}
        className="flex items-center gap-2 text-white/90 hover:text-white transition-colors group"
      >
        <MapPin size={15} className={`shrink-0 ${endereco ? 'text-green-400' : 'text-blue-300'}`} />

        <span className="text-sm font-medium truncate max-w-[200px] sm:max-w-xs">
          {endereco
            ? endereco.resumo
            : <span className="text-blue-200 text-xs sm:text-sm">Informe seu CEP</span>
          }
        </span>

        <ChevronDown size={13} className="text-blue-300 group-hover:text-white transition-colors shrink-0" />
      </button>

      {/* Modal dropdown */}
      {aberto && (
        <div className="fixed inset-0 z-50 flex items-start justify-center pt-[72px] px-4 bg-black/40">
          <div
            ref={modalRef}
            className="bg-white rounded-2xl shadow-2xl w-full max-w-sm overflow-hidden animate-in fade-in slide-in-from-top-2 duration-200"
          >
            {/* Cabeçalho */}
            <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100">
              <div className="flex items-center gap-2">
                <MapPin size={16} className="text-blue-600" />
                <span className="font-bold text-gray-800">Seu endereço de entrega</span>
              </div>
              <button
                onClick={() => setAberto(false)}
                className="text-gray-400 hover:text-gray-600 transition-colors p-1 rounded-lg hover:bg-gray-100"
              >
                <X size={16} />
              </button>
            </div>

            {/* Endereço salvo */}
            {endereco && (
              <div className="mx-5 mt-4 p-3 bg-green-50 border border-green-200 rounded-xl flex items-start gap-3">
                <CheckCircle2 size={16} className="text-green-500 mt-0.5 shrink-0" />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-semibold text-green-800 truncate">{endereco.resumo}</p>
                  <p className="text-xs text-green-600">CEP {endereco.cep}</p>
                </div>
                <button
                  onClick={limpar}
                  className="text-green-400 hover:text-red-500 transition-colors shrink-0"
                  title="Remover endereço"
                >
                  <X size={14} />
                </button>
              </div>
            )}

            {/* Formulário de CEP */}
            <form onSubmit={buscarCep} className="px-5 py-4">
              <label className="block text-xs font-semibold text-gray-500 mb-2 uppercase tracking-wide">
                {endereco ? 'Alterar CEP' : 'Informe o CEP'}
              </label>

              <div className="flex gap-2">
                <input
                  ref={inputRef}
                  value={cep}
                  onChange={e => {
                    setCep(formatarCep(e.target.value))
                    setErro('')
                  }}
                  placeholder="00000-000"
                  maxLength={9}
                  className="flex-1 border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <button
                  type="submit"
                  disabled={buscando || cep.replace(/\D/g, '').length < 8}
                  className="bg-blue-600 hover:bg-blue-700 disabled:opacity-40 disabled:cursor-not-allowed text-white px-4 py-2.5 rounded-xl transition-colors flex items-center gap-1.5 text-sm font-semibold"
                >
                  {buscando
                    ? <Loader2 size={15} className="animate-spin" />
                    : <Search size={15} />
                  }
                  Buscar
                </button>
              </div>

              {erro && (
                <p className="mt-2 text-xs text-red-500 flex items-center gap-1">
                  <X size={11} />
                  {erro}
                </p>
              )}

              <p className="mt-3 text-xs text-gray-400">
                Usamos o CEP apenas para verificar disponibilidade de entrega na sua região.
              </p>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
