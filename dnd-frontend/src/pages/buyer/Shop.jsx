import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import {
  ShoppingCart, Package, Plus, Minus, Trash2, Image, AlertCircle,
  X, Search, Clock, MessageCircle, CheckCircle,
  User, MapPin, Phone
} from 'lucide-react'
import { maskCPF, maskCNPJ, maskPhone } from '../../utils/masks'
import Pagination from '../../components/Pagination'

const fmt = (v) => `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`
const PAGE_SIZE = 24

export default function Shop() {
  const [products, setProducts] = useState([])
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [categoryList, setCategoryList] = useState([])
  const [cart, setCart] = useState([])
  const [loading, setLoading] = useState(true)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [categoryFilter, setCategoryFilter] = useState('')
  const [cartOpen, setCartOpen] = useState(false)
  const [storeSettings, setStoreSettings] = useState(null)
  const [ordering, setOrdering] = useState(false)
  const [reviewOpen, setReviewOpen] = useState(false)
  const [buyerProfile, setBuyerProfile] = useState(null)
  const [loadingProfile, setLoadingProfile] = useState(false)
  const [successOrder, setSuccessOrder] = useState(null)

  useEffect(() => {
    api.get('/settings').then(r => setStoreSettings(r.data)).catch(() => {})
    api.get('/categories').then(r => setCategoryList(r.data)).catch(() => {})
  }, [])

  useEffect(() => {
    const t = setTimeout(() => { setSearch(searchInput); setPage(1) }, 350)
    return () => clearTimeout(t)
  }, [searchInput])

  useEffect(() => { setPage(1) }, [categoryFilter])

  useEffect(() => {
    setLoading(true)
    const params = { activeOnly: true, page, pageSize: PAGE_SIZE }
    if (search) params.search = search
    if (categoryFilter) params.category = categoryFilter

    api.get('/products', { params }).then(({ data }) => {
      setProducts(data.items)
      setTotalCount(data.totalCount)
      setTotalPages(data.totalPages || 1)
    }).finally(() => setLoading(false))
  }, [page, search, categoryFilter])

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

  // O estoque máximo usado aqui vem do snapshot salvo no item do carrinho no
  // momento em que foi adicionado — os produtos carregados na tela mudam de
  // página, então não dá para depender da lista `products` atual para validar.
  // A validação definitiva de qualquer forma acontece no servidor ao fechar o pedido.
  const changeQty = (id, delta) => {
    setCart(prev => prev.map(i => {
      if (i.id !== id) return i
      const newQty = i.qty + delta
      if (newQty < 1) return i
      if (newQty > i.stock) { toast.error(`Estoque máximo: ${i.stock}`); return i }
      return { ...i, qty: newQty }
    }))
  }

  const setQty = (id, value) => {
    const num = parseInt(value, 10)
    if (isNaN(num) || num < 1) return
    setCart(prev => prev.map(i => {
      if (i.id !== id) return i
      if (num > i.stock) { toast.error(`Estoque máximo: ${i.stock}`); return i }
      return { ...i, qty: num }
    }))
  }

  const cartCount = cart.reduce((a, i) => a + i.qty, 0)
  const cartTotal = cart.reduce((a, i) => a + i.salePrice * i.qty, 0)

  const handleOrder = async () => {
    if (!storeOpen) { toast.error('A loja está fechada. Tente durante o horário de funcionamento.'); return }
    if (cart.length === 0) { toast.error('Carrinho vazio'); return }

    if (!buyerProfile) {
      setLoadingProfile(true)
      try {
        const res = await api.get('/buyers/me')
        setBuyerProfile(res.data)
      } catch {
        toast.error('Erro ao carregar dados do perfil')
        return
      } finally {
        setLoadingProfile(false)
      }
    }
    setReviewOpen(true)
    setCartOpen(false)
  }

  const confirmOrder = async () => {
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
        const msg = `Olá! Realizei um pedido na D&D Distribuidora.\n\n*Pedido #${order.id}*\n${lines}\n\n*Total: ${fmt(cartTotal)}*\n\nNome: ${buyerProfile?.name}\nLoja: ${buyerProfile?.storeName}\nTelefone: ${buyerProfile?.phone}\n\nAguardo confirmação. Obrigado!`
        whatsappUrl = `https://wa.me/${num}?text=${encodeURIComponent(msg)}`
      }

      setCart([])
      setReviewOpen(false)
      setSuccessOrder({ id: order.id, whatsappUrl })
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao realizar pedido')
    } finally {
      setOrdering(false)
    }
  }

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
                <div className="flex items-center gap-1.5">
                  <button onClick={() => changeQty(item.id, -1)} className="w-7 h-7 rounded-full bg-gray-200 hover:bg-gray-300 flex items-center justify-center transition">
                    <Minus size={12} />
                  </button>
                  <input
                    type="number" min="1" max={item.stock}
                    value={item.qty}
                    onChange={e => setQty(item.id, e.target.value)}
                    className="w-12 text-center text-sm font-bold border border-gray-200 rounded-lg py-1 focus:outline-none focus:border-blue-400"
                  />
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
          <button onClick={handleOrder} disabled={loadingProfile} className="btn-primary w-full py-3 text-base">
            {loadingProfile ? 'Carregando...' : 'Revisar pedido'}
          </button>
        </div>
      )}
    </>
  )

  if (loading && products.length === 0) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
  )

  return (
    <div className="flex" style={{ height: '100%', minHeight: 0 }}>

      {/* Modal de revisão do pedido */}
      {reviewOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md max-h-[90vh] flex flex-col">
            <div className="p-5 border-b border-gray-100 flex items-center justify-between shrink-0">
              <h2 className="text-lg font-black text-gray-900">Revisão do Pedido</h2>
              <button onClick={() => setReviewOpen(false)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <div className="flex-1 overflow-y-auto p-5 space-y-5">
              {/* Dados do comprador */}
              {buyerProfile && (
                <div className="bg-blue-50 border border-blue-100 rounded-xl p-4 space-y-2">
                  <p className="text-xs font-bold text-blue-700 uppercase tracking-wide mb-3">Seus dados</p>
                  <div className="flex items-start gap-2">
                    <User size={14} className="text-blue-500 mt-0.5 shrink-0" />
                    <div>
                      <p className="text-sm font-semibold text-gray-900">{buyerProfile.name}</p>
                      <p className="text-xs text-gray-500">{buyerProfile.storeName}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Phone size={14} className="text-blue-500 shrink-0" />
                    <p className="text-sm text-gray-700">{buyerProfile.phone ? maskPhone(buyerProfile.phone) : '—'}</p>
                  </div>
                  <div className="flex items-start gap-2">
                    <MapPin size={14} className="text-blue-500 mt-0.5 shrink-0" />
                    <p className="text-sm text-gray-700">{buyerProfile.address}</p>
                  </div>
                  <div className="text-xs text-gray-500 pt-1 border-t border-blue-100 mt-2 flex gap-4">
                    {buyerProfile.cpf  && <span>CPF: {maskCPF(buyerProfile.cpf)}</span>}
                    {buyerProfile.cnpj && <span>CNPJ: {maskCNPJ(buyerProfile.cnpj)}</span>}
                  </div>
                </div>
              )}

              {/* Itens do pedido */}
              <div>
                <p className="text-xs font-bold text-gray-500 uppercase tracking-wide mb-3">Itens do pedido</p>
                <div className="space-y-2">
                  {cart.map(item => (
                    <div key={item.id} className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
                      <div>
                        <p className="text-sm font-semibold text-gray-800">{item.name}</p>
                        <p className="text-xs text-gray-500">{item.qty}x {fmt(item.salePrice)}</p>
                      </div>
                      <p className="text-sm font-bold text-blue-700">{fmt(item.salePrice * item.qty)}</p>
                    </div>
                  ))}
                </div>
              </div>

              {/* Total */}
              <div className="bg-gray-50 rounded-xl p-4 flex justify-between items-center">
                <span className="font-bold text-gray-700 text-lg">Total</span>
                <span className="text-2xl font-black text-gray-900">{fmt(cartTotal)}</span>
              </div>
            </div>

            <div className="p-5 border-t border-gray-100 shrink-0">
              <button
                onClick={confirmOrder}
                disabled={ordering}
                className="btn-primary w-full py-3 text-base font-bold">
                {ordering ? 'Enviando pedido...' : 'Confirmar e enviar pedido'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal de pedido realizado */}
      {successOrder && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 text-center">
            <CheckCircle size={56} className="text-green-500 mx-auto mb-3" />
            <h2 className="text-xl font-black text-gray-900 mb-1">Pedido #{successOrder.id} realizado!</h2>
            <p className="text-gray-500 text-sm mb-6">Seu pedido foi enviado com sucesso. Acompanhe o status na aba Pedidos.</p>

            {successOrder.whatsappUrl && (
              <a
                href={successOrder.whatsappUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center justify-center gap-2 w-full bg-green-500 hover:bg-green-600 text-white font-bold py-3 px-4 rounded-xl transition mb-3">
                <MessageCircle size={20} />
                Enviar pedido pelo WhatsApp
              </a>
            )}

            <button onClick={() => setSuccessOrder(null)} className="btn-secondary w-full">
              Continuar comprando
            </button>
          </div>
        </div>
      )}

      {/* Produtos */}
      <div className="flex-1 overflow-y-auto p-4 md:p-8 pb-24 md:pb-8">
        <div className="mb-4">
          <h1 className="text-2xl font-bold text-gray-900">Loja</h1>
          <p className="text-gray-500 text-sm mt-1">{totalCount} produto(s) disponível(is)</p>
        </div>

        {storeSettings && (
          <div className={`flex items-center gap-3 rounded-xl px-4 py-3 mb-4 text-sm font-medium ${
            storeOpen
              ? 'bg-green-50 border border-green-200 text-green-700'
              : 'bg-red-50 border border-red-200 text-red-700'
          }`}>
            <Clock size={16} className="shrink-0" />
            {storeOpen ? 'Loja aberta — aceitando pedidos' : 'Loja fechada temporariamente'}
          </div>
        )}

        <div className="mb-5 flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1 max-w-md">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input value={searchInput} onChange={e => setSearchInput(e.target.value)}
              placeholder="Buscar produto..." className="input pl-9" />
          </div>
          <select value={categoryFilter} onChange={e => setCategoryFilter(e.target.value)} className="input sm:w-56">
            <option value="">Todas as categorias</option>
            {categoryList.map(c => <option key={c.id} value={c.name}>{c.name}</option>)}
          </select>
        </div>

        {!loading && products.length === 0 ? (
          <div className="text-center py-20">
            <Package size={48} className="text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500">Nenhum produto disponível no momento</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4">
            {products.map(p => (
              <div key={p.id}
                className="bg-white border border-gray-100 rounded-xl overflow-hidden shadow-sm flex flex-col transition-all duration-200 hover:-translate-y-1 hover:shadow-md">
                <div className="aspect-video bg-gray-100 overflow-hidden">
                  {p.imageUrl
                    ? <img src={p.imageUrl} alt={p.name} loading="lazy" className="w-full h-full object-cover transition-transform duration-300 hover:scale-105" />
                    : <div className="flex items-center justify-center h-full text-gray-300"><Image size={40} /></div>}
                </div>
                <div className="p-3 flex flex-col flex-1">
                  <span className="inline-block text-xs bg-blue-100 text-blue-700 font-semibold px-2 py-0.5 rounded-full mb-2 self-start">
                    {p.category || 'Geral'}
                  </span>
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
        )}

        <Pagination page={page} totalPages={totalPages} totalCount={totalCount} onPageChange={setPage} itemLabel="produto(s)" />
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
