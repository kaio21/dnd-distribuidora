import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { ShoppingCart, Package, Plus, Minus, Trash2, Image, AlertCircle, X, ChevronDown, ChevronRight, Clock, MessageCircle, CheckCircle } from 'lucide-react'

const fmt = (v) => `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`

export default function Shop() {
  const [products, setProducts] = useState([])
  const [cart, setCart] = useState([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [cartOpen, setCartOpen] = useState(false)
  const [openCategories, setOpenCategories] = useState({})
  const [storeSettings, setStoreSettings] = useState(null)
  const [ordering, setOrdering] = useState(false)
  const [successOrder, setSuccessOrder] = useState(null) // { id, whatsappUrl }

  useEffect(() => {
    Promise.all([
      api.get('/products?activeOnly=true'),
      api.get('/settings')
    ]).then(([prodRes, settingsRes]) => {
      setProducts(prodRes.data)
      setStoreSettings(settingsRes.data)
      const cats = [...new Set(prodRes.data.map(p => p.category || 'Geral'))]
      setOpenCategories(Object.fromEntries(cats.map(c => [c, true])))
    }).finally(() => setLoading(false))
  }, [])

  const isStoreOpen = () => {
    if (!storeSettings) return true
    if (!storeSettings.isOpen) return false
    return true
  }

  const storeOpen = isStoreOpen()

  const addToCart = (product) => {
    if (!storeOpen) { toast.error('A loja está fechada no momento.'); return }
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

  const handleOrder = async () => {
    if (!storeOpen) { toast.error('A loja está fechada. Tente durante o horário de funcionamento.'); return }
    if (cart.length === 0) { toast.error('Carrinho vazio'); return }

    setOrdering(true)
    try {
      const res = await api.post('/orders', {
        items: cart.map(i => ({ productId: i.id, quantity: i.qty }))
      })
      const order = res.data

      let whatsappUrl = null
      const num = storeSettings?.whatsAppNumber?.replace(/\D/g, '')
      if (num) {
        const lines = cart.map(i => `• ${i.name} (${i.qty}x) = ${fmt(i.salePrice * i.qty)}`).join('\n')
        const msg = `Olá! Realizei um pedido na D&D Distribuidora.\n\n*Pedido #${order.id}*\n${lines}\n\n*Total: ${fmt(cartTotal)}*\n\nAguardo confirmação. Obrigado!`
        whatsappUrl = `https://wa.me/${num}?text=${encodeURIComponent(msg)}`
      }

      setCart([])
      setCartOpen(false)
      setSuccessOrder({ id: order.id, whatsappUrl })
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao realizar pedido')
    } finally {
      setOrdering(false)
    }
  }

  const toggleCategory = (cat) => setOpenCategories(prev => ({ ...prev, [cat]: !prev[cat] }))

  const filtered = products.filter(p => p.name.toLowerCase().includes(search.toLowerCase()))
  const categories = [...new Set(filtered.map(p => p.category || 'Geral'))].sort()
  const byCategory = Object.fromEntries(
    categories.map(cat => [cat, filtered.filter(p => (p.category || 'Geral') === cat)])
  )

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
        <div className="p-4 border-t border-gray-200 shrink-0">
          <div className="flex justify-between mb-4">
            <span className="font-semibold text-gray-700">Total</span>
            <span className="text-xl font-black text-gray-900">{fmt(cartTotal)}</span>
          </div>
          <button onClick={handleOrder} disabled={ordering} className="btn-primary w-full py-3 text-base">
            {ordering ? 'Enviando pedido...' : 'Finalizar pedido'}
          </button>
        </div>
      )}
    </>
  )

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
  )

  return (
    <div className="flex" style={{ height: '100%', minHeight: 0 }}>
      {/* Modal de pedido realizado */}
      {successOrder && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 text-center">
            <CheckCircle size={56} className="text-green-500 mx-auto mb-3" />
            <h2 className="text-xl font-black text-gray-900 mb-1">Pedido #{successOrder.id} realizado!</h2>
            <p className="text-gray-500 text-sm mb-6">Seu pedido foi enviado com sucesso. Acompanhe o status na aba Pedidos.</p>

            {successOrder.whatsappUrl ? (
              <a
                href={successOrder.whatsappUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center justify-center gap-2 w-full bg-green-500 hover:bg-green-600 text-white font-bold py-3 px-4 rounded-xl transition mb-3">
                <MessageCircle size={20} />
                Enviar pedido pelo WhatsApp
              </a>
            ) : null}

            <button
              onClick={() => setSuccessOrder(null)}
              className="btn-secondary w-full">
              Continuar comprando
            </button>
          </div>
        </div>
      )}

      {/* Produtos */}
      <div className="flex-1 overflow-y-auto p-4 md:p-8 pb-24 md:pb-8">
        <div className="mb-4">
          <h1 className="text-2xl font-bold text-gray-900">Loja</h1>
          <p className="text-gray-500 text-sm mt-1">{products.length} produto(s) disponível(is)</p>
        </div>

        {/* Banner status */}
        {storeSettings && (
          <div className={`flex items-center gap-3 rounded-xl px-4 py-3 mb-4 text-sm font-medium ${
            storeOpen
              ? 'bg-green-50 border border-green-200 text-green-700'
              : 'bg-red-50 border border-red-200 text-red-700'
          }`}>
            <Clock size={16} className="shrink-0" />
            {storeOpen
              ? 'Loja aberta — aceitando pedidos'
              : 'Loja fechada temporariamente'}
          </div>
        )}

        <div className="mb-5">
          <input value={search} onChange={e => setSearch(e.target.value)}
            placeholder="Buscar produto..." className="input max-w-md" />
        </div>

        {filtered.length === 0 ? (
          <div className="text-center py-20">
            <Package size={48} className="text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500">Nenhum produto disponível no momento</p>
          </div>
        ) : (
          <div className="space-y-3">
            {categories.map(cat => (
              <div key={cat} className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
                <button
                  onClick={() => toggleCategory(cat)}
                  className="w-full flex items-center justify-between px-5 py-4 hover:bg-gray-50 transition">
                  <div className="flex items-center gap-3">
                    {openCategories[cat] ? <ChevronDown size={18} className="text-blue-600" /> : <ChevronRight size={18} className="text-gray-400" />}
                    <span className="font-bold text-gray-800">{cat}</span>
                    <span className="text-xs bg-blue-100 text-blue-700 font-semibold px-2 py-0.5 rounded-full">
                      {byCategory[cat].length}
                    </span>
                  </div>
                </button>

                {openCategories[cat] && (
                  <div className="border-t border-gray-100">
                    <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4 p-4">
                      {byCategory[cat].map(p => (
                        <div key={p.id} className="bg-white border border-gray-100 rounded-xl overflow-hidden shadow-sm flex flex-col">
                          <div className="aspect-video bg-gray-100 overflow-hidden">
                            {p.imageUrl
                              ? <img src={p.imageUrl} alt={p.name} className="w-full h-full object-cover" />
                              : <div className="flex items-center justify-center h-full text-gray-300"><Image size={40} /></div>}
                          </div>
                          <div className="p-3 flex flex-col flex-1">
                            <h3 className="font-bold text-gray-900 mb-1 text-sm">{p.name}</h3>
                            {p.description && <p className="text-gray-500 text-xs mb-2 line-clamp-2">{p.description}</p>}

                            <div className="flex items-center justify-between mb-3 mt-auto">
                              <p className="text-xl font-black text-blue-700">{fmt(p.salePrice)}</p>
                              {p.stock === 0 ? (
                                <span className="flex items-center gap-1 text-red-500 text-xs font-medium">
                                  <AlertCircle size={14} /> Sem estoque
                                </span>
                              ) : (
                                <span className="text-gray-400 text-xs">{p.stock} disp.</span>
                              )}
                            </div>

                            <button
                              onClick={() => addToCart(p)}
                              disabled={p.stock === 0 || !storeOpen}
                              className="btn-primary w-full flex items-center justify-center gap-2 text-sm py-2">
                              <ShoppingCart size={15} />
                              {!storeOpen ? 'Loja fechada' : p.stock === 0 ? 'Sem estoque' : 'Adicionar'}
                            </button>
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
      </div>

      {/* Carrinho — desktop sidebar */}
      <aside className="hidden md:flex w-80 bg-white border-l border-gray-200 flex-col shrink-0">
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

      {/* Botão flutuante — mobile */}
      {cartCount > 0 && !cartOpen && (
        <button onClick={() => setCartOpen(true)}
          className="fixed bottom-20 right-4 md:hidden bg-blue-600 text-white rounded-full px-4 py-3 flex items-center gap-2 shadow-xl z-40">
          <ShoppingCart size={18} />
          <span className="font-bold">{cartCount}</span>
          <span className="font-semibold text-sm">{fmt(cartTotal)}</span>
        </button>
      )}

      {/* Drawer carrinho — mobile */}
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
