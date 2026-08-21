import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import { ShoppingCart, Package, Image, AlertCircle, ChevronDown, ChevronRight, Clock, Search, LogIn } from 'lucide-react'
import BarraCep from '../components/BarraCep'

const fmt = (v) => `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`

export default function Home() {
  const [products, setProducts] = useState([])
  const [storeSettings, setStoreSettings] = useState(null)
  const [loading, setLoading] = useState(true)
  const [erro, setErro] = useState(false)
  const [search, setSearch] = useState('')
  const [openCategories, setOpenCategories] = useState({})

  const carregarDados = () => {
    setLoading(true)
    setErro(false)
    Promise.all([
      api.get('/products?activeOnly=true'),
      api.get('/settings')
    ]).then(([prodRes, settingsRes]) => {
      setProducts(prodRes.data)
      setStoreSettings(settingsRes.data)
      const cats = [...new Set(prodRes.data.map(p => p.category || 'Geral'))]
      setOpenCategories(Object.fromEntries(cats.map(c => [c, true])))
    }).catch(() => setErro(true)).finally(() => setLoading(false))
  }

  useEffect(() => { carregarDados() }, [])

  const storeOpen = storeSettings?.isOpen ?? true

  const toggleCategory = (cat) => setOpenCategories(prev => ({ ...prev, [cat]: !prev[cat] }))

  const filtered = products.filter(p => p.name.toLowerCase().includes(search.toLowerCase()))
  const categories = [...new Set(filtered.map(p => p.category || 'Geral'))].sort()
  const byCategory = Object.fromEntries(
    categories.map(cat => [cat, filtered.filter(p => (p.category || 'Geral') === cat)])
  )

  return (
    <div className="min-h-screen flex flex-col" style={{ background: '#eef2f8' }}>
      {/* Header */}
      <header className="bg-blue-900 sticky top-0 z-40 shadow-lg">
        {/* Linha principal */}
        <div className="px-4 md:px-8 py-3 flex items-center justify-between gap-4">
          <div className="flex items-center gap-3 shrink-0">
            <div className="bg-blue-600 rounded-xl w-9 h-9 flex items-center justify-center shrink-0">
              <span className="text-white font-black text-lg">D</span>
            </div>
            <span className="text-white font-bold text-lg hidden sm:block">D&D Distribuidora</span>
            <span className="text-white font-bold text-base sm:hidden">D&D</span>
          </div>

          {/* Barra de CEP — centro */}
          <div className="flex-1 flex justify-center">
            <BarraCep />
          </div>

          <div className="flex items-center gap-2 shrink-0">
            <Link to="/login"
              className="flex items-center gap-1.5 bg-white text-blue-700 font-semibold px-3 py-2 rounded-lg text-sm hover:bg-blue-50 transition">
              <LogIn size={15} />
              <span className="hidden sm:inline">Entrar</span>
              <span className="sm:hidden">Entrar</span>
            </Link>
          </div>
        </div>
      </header>

      {/* Banner da loja */}
      <div className="bg-blue-800 px-4 md:px-8 py-6">
        <h1 className="text-white font-black text-2xl md:text-3xl mb-1">Catálogo de Produtos</h1>
        <p className="text-blue-200 text-sm">Faça login para realizar pedidos e acompanhar suas compras</p>

        {storeSettings && (
          <div className={`inline-flex items-center gap-2 mt-3 px-3 py-1.5 rounded-full text-xs font-semibold ${
            storeOpen ? 'bg-green-500/20 text-green-200' : 'bg-red-500/20 text-red-200'
          }`}>
            <Clock size={12} />
            {storeOpen ? 'Loja aberta' : 'Loja fechada temporariamente'}
          </div>
        )}
      </div>

      {/* Conteúdo */}
      <main className="flex-1 px-4 md:px-8 py-6 max-w-6xl mx-auto w-full">
        <div className="mb-5">
          <div className="relative max-w-lg">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Buscar produto..."
              className="input pl-9" />
          </div>
        </div>

        {loading ? (
          <div className="flex flex-col items-center justify-center py-24 gap-3">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <p className="text-gray-400 text-sm">Carregando catálogo...</p>
          </div>
        ) : erro ? (
          <div className="flex flex-col items-center justify-center py-24 gap-4 text-center">
            <div className="bg-yellow-50 border border-yellow-200 rounded-2xl px-8 py-8 max-w-md w-full">
              <AlertCircle size={40} className="text-yellow-500 mx-auto mb-3" />
              <h3 className="font-bold text-gray-800 mb-1">Serviço temporariamente indisponível</h3>
              <p className="text-gray-500 text-sm mb-4">
                O servidor está acordando. Isso pode levar alguns segundos. Tente novamente.
              </p>
              <button
                onClick={carregarDados}
                className="btn-primary w-full flex items-center justify-center gap-2 py-2.5">
                <Search size={15} />
                Tentar novamente
              </button>
            </div>
          </div>
        ) : filtered.length === 0 ? (
          <div className="text-center py-20">
            <Package size={48} className="text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500">Nenhum produto disponível no momento</p>
          </div>
        ) : (
          <div className="space-y-4">
            {categories.map(cat => (
              <div key={cat} className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
                <button
                  onClick={() => toggleCategory(cat)}
                  className="w-full flex items-center justify-between px-5 py-4 hover:bg-gray-50 transition">
                  <div className="flex items-center gap-3">
                    {openCategories[cat]
                      ? <ChevronDown size={18} className="text-blue-600" />
                      : <ChevronRight size={18} className="text-gray-400" />}
                    <span className="font-bold text-gray-800">{cat}</span>
                    <span className="text-xs bg-blue-100 text-blue-700 font-semibold px-2 py-0.5 rounded-full">
                      {byCategory[cat].length}
                    </span>
                  </div>
                </button>

                {openCategories[cat] && (
                  <div className="border-t border-gray-100">
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 p-4">
                      {byCategory[cat].map(p => (
                        <div key={p.id} className="bg-white border border-gray-100 rounded-xl overflow-hidden shadow-sm flex flex-col transition-all duration-200 hover:-translate-y-1 hover:shadow-md">
                          <div className="aspect-video bg-gray-100 overflow-hidden">
                            {p.imageUrl
                              ? <img src={p.imageUrl} alt={p.name} loading="lazy" className="w-full h-full object-cover transition-transform duration-300 hover:scale-105" />
                              : <div className="flex items-center justify-center h-full text-gray-300"><Image size={40} /></div>}
                          </div>
                          <div className="p-3 flex flex-col flex-1">
                            <h3 className="font-bold text-gray-900 mb-1 text-sm">{p.name}</h3>
                            {p.description && <p className="text-gray-500 text-xs mb-2 line-clamp-2">{p.description}</p>}

                            <div className="flex items-center justify-between mt-auto mb-3">
                              <p className="text-xl font-black text-blue-700">{fmt(p.salePrice)}</p>
                              {p.stock === 0 ? (
                                <span className="flex items-center gap-1 text-red-500 text-xs font-medium">
                                  <AlertCircle size={13} /> Sem estoque
                                </span>
                              ) : (
                                <span className="text-gray-400 text-xs">{p.stock} disp.</span>
                              )}
                            </div>

                            <Link
                              to="/login"
                              className="btn-primary w-full flex items-center justify-center gap-2 text-sm py-2">
                              <ShoppingCart size={14} />
                              {p.stock === 0 ? 'Sem estoque' : 'Entrar para comprar'}
                            </Link>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </main>

      <footer className="text-center py-5 text-gray-400 text-xs border-t border-gray-200">
        © 2025 D&D Distribuidora Atacadista
      </footer>
    </div>
  )
}
