import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { ShoppingCart, Package, Plus, Minus, Trash2, Image, AlertCircle, X } from 'lucide-react'
import { useNavigate } from 'react-router-dom'

const fmt = (v) => `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`

export default function Shop() {
  const [products, setProducts] = useState([])
  const [cart, setCart] = useState([])
  const [loading, setLoading] = useState(true)
  const [ordering, setOrdering] = useState(false)
  const [search, setSearch] = useState('')
  const [cartOpen, setCartOpen] = useState(false)
  const [ordering] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    api.get('/products?activeOnly=true')
      .then(r => setProducts(r.data))
      .finally(() => setLoading(false))
  }, [])

  const addToCart = (product) => {
    if (product.stock === 0) { toast.error('Produto sem estoque!'); return }
    setCart(prev => {
      const existing = prev.find(i => i.id === product.id)
      if (existing) {
        if (existing.qty >= product.stock) { toast.error(`Estoque máximo: ${product.stock}`); return prev }
        return prev.map(i => i.id === product.id ? { ...i, qty: i.qty + 1 } : i)
      }
      return [...prev, { ...product, qty: 1 }]
    })
  }

  const removeFromCart = (id) => setCart(prev => prev.filter(i => i.id !== id))
  const changeQty = (id, delta) => {
    setCart(prev => prev.map(i => {
      if (i.id !== id) return i
      const newQty = i.qty + delta
      if (newQty < 1) return i
      const product = products.find(p => p.id === id)
      if (newQty > product.stock) { toast.error(`Estoque máximo: ${product.stock}`); return i }
      return { ...i, qty: newQty }
    }))
  }

  const cartCount = cart.reduce((a, i) => a + i.qty, 0)
  const cartTotal = cart.reduce((a, i) => a + i.salePrice * i.qty, 0)

  const handleOrder = () => {
    if (cart.length === 0) { toast.error('Carrinho vazio'); return }
    navigate('/buyer/payment', { state: { cart } })
  }

  const filtered = products.filter(p => p.name.toLowerCase().includes(search.toLowerCase()))

  const CartItems = () => (
    <>
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {cart.length === 0 ? (
          <div className="text-center py-12 text-gray-400">
            <ShoppingCart size={36} className="mx-auto mb-2 opacity-30" />
            <p className="text-sm">Carrinho vazio</p>
          </div>
        ) : (
          cart.map(item => (
            <div key={item.id} className="bg-gray-50 rounded-xl p-3">
              <div className="flex justify-between items-start mb-2">
                <p className="text-sm font-semibold text-gray-800 flex-1 pr-2">{item.name}</p>
                <button onClick={() => removeFromCart(item.id)} className="text-gray-400 hover:text-red-500 transition">
                  <Trash2 size={14} />
                </button>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <button onClick={() => changeQty(item.id, -1)} className="w-7 h-7 rounded-full bg-gray-200 hover:bg-gray-300 flex items-center justify-center transition">
                    <Minus size={12} />
                  </button>
                  <span className="text-sm font-bold w-6 text-center">{item.qty}</span>
                  <button onClick={() => changeQty(item.id, 1)} className="w-7 h-7 rounded-full bg-gray-200 hover:bg-gray-300 flex items-center justify-center transition">
                    <Plus size={12} />
                  </button>
                </div>
                <p className="text-sm font-bold text-blue-700">{fmt(item.salePrice * item.qty)}</p>
              </div>
            </div>
          ))
        )}
      </div>
      {cart.length > 0 && (
        <div className="p-4 border-t border-gray-200">
          <div className="flex justify-between mb-4">
            <span className="font-semibold text-gray-700">Total</span>
            <span className="text-xl font-black text-gray-900">{fmt(cartTotal)}</span>
          </div>
          <button onClick={handleOrder} disabled={ordering} className="btn-primary w-full py-3 text-base">
            {ordering ? 'Realizando pedido...' : 'Finalizar pedido'}
          </button>
        </div>
      )}
    </>
  )

  if (loading) return <div className="flex items-center justify-center h-64"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div></div>

  return (
    <div className="flex h-full overflow-hidden" style={{ height: '100vh' }}>
      {/* Produtos */}
      <div className="flex-1 overflow-y-auto p-4 md:p-8">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Loja</h1>
          <p className="text-gray-500 text-sm mt-1">{products.length} produto(s) disponível(is)</p>
        </div>

        <div className="mb-6">
          <input value={search} onChange={e => setSearch(e.target.value)}
            placeholder="Buscar produto..." className="input max-w-md" />
        </div>

        {filtered.length === 0 ? (
          <div className="text-center py-20">
            <Package size={48} className="text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500">Nenhum produto disponível no momento</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4 md:gap-5">
            {filtered.map(p => (
              <div key={p.id} className="card group">
                <div className="aspect-video bg-gray-100 rounded-lg mb-4 overflow-hidden">
                  {p.imageUrl
                    ? <img src={p.imageUrl} alt={p.name} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
                    : <div className="flex items-center justify-center h-full text-gray-300"><Image size={40} /></div>}
                </div>
                <h3 className="font-bold text-gray-900 mb-1">{p.name}</h3>
                <p className="text-gray-500 text-sm mb-3 line-clamp-2">{p.description}</p>

                <div className="flex items-center justify-between mb-4">
                  <p className="text-2xl font-black text-blue-700">{fmt(p.salePrice)}</p>
                  {p.stock === 0 ? (
                    <span className="flex items-center gap-1 text-red-500 text-sm font-medium">
                      <AlertCircle size={16} /> Sem estoque
                    </span>
                  ) : (
                    <span className="text-gray-500 text-sm">{p.stock} disponível</span>
                  )}
                </div>

                <button onClick={() => addToCart(p)} disabled={p.stock === 0}
                  className="btn-primary w-full flex items-center justify-center gap-2 disabled:opacity-40">
                  <ShoppingCart size={16} /> Adicionar
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Carrinho — desktop sidebar */}
      <aside className="hidden md:flex w-80 bg-white border-l border-gray-200 flex-col">
        <div className="p-5 border-b border-gray-200">
          <h2 className="font-bold text-gray-900 flex items-center gap-2">
            <ShoppingCart size={18} /> Carrinho
            {cartCount > 0 && (
              <span className="bg-blue-600 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center ml-auto">
                {cartCount}
              </span>
            )}
          </h2>
        </div>
        <CartItems />
      </aside>

      {/* Botão flutuante do carrinho — mobile */}
      {cartCount > 0 && (
        <button onClick={() => setCartOpen(true)}
          className="fixed bottom-20 right-4 md:hidden bg-blue-600 text-white rounded-full px-4 py-3 flex items-center gap-2 shadow-xl z-40">
          <ShoppingCart size={18} />
          <span className="font-bold">{cartCount}</span>
          <span className="font-semibold text-sm">{fmt(cartTotal)}</span>
        </button>
      )}

      {/* Drawer do carrinho — mobile */}
      {cartOpen && (
        <div className="fixed inset-0 z-50 md:hidden">
          <div className="absolute inset-0 bg-black/50" onClick={() => setCartOpen(false)} />
          <div className="absolute bottom-0 left-0 right-0 bg-white rounded-t-2xl flex flex-col" style={{ maxHeight: '80vh' }}>
            <div className="p-4 border-b border-gray-200 flex items-center justify-between shrink-0">
              <h2 className="font-bold text-gray-900 flex items-center gap-2">
                <ShoppingCart size={18} /> Carrinho ({cartCount})
              </h2>
              <button onClick={() => setCartOpen(false)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>
            <CartItems />
          </div>
        </div>
      )}
    </div>
  )
}
