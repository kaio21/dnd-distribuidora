import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { ShoppingBag, TrendingUp, MessageSquare, CheckCircle, Filter } from 'lucide-react'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'

const fmt = (v) => `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`

const statusConfig = {
  Pendente:   { cls: 'badge-pending',   label: 'Pendente' },
  Confirmado: { cls: 'badge-confirmed', label: 'Confirmado' },
  Pronto:     { cls: 'badge-ready',     label: 'Pronto' },
  Entregue:   { cls: 'badge-delivered', label: 'Entregue' },
  Cancelado:  { cls: 'badge-cancelled', label: 'Cancelado' },
}

const nextStatus = {
  Pendente:   'Confirmado',
  Confirmado: 'Pronto',
  Pronto:     'Entregue',
}

export default function SellerOrders() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [filterStatus, setFilterStatus] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [expanded, setExpanded] = useState(null)

  const load = () => {
    const params = new URLSearchParams()
    if (filterStatus) params.append('status', filterStatus)
    if (dateFrom) params.append('from', dateFrom)
    if (dateTo) params.append('to', dateTo)
    api.get(`/orders?${params}`).then(r => setOrders(r.data)).finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [filterStatus, dateFrom, dateTo])

  const advance = async (order) => {
    const next = nextStatus[order.status]
    if (!next) return
    try {
      const res = await api.patch(`/orders/${order.id}/status`, { status: next })
      toast.success(`Pedido marcado como ${next}`)
      if (res.data.whatsappUrl) {
        if (confirm('Pedido pronto! Deseja notificar o cliente pelo WhatsApp?')) {
          window.open(res.data.whatsappUrl, '_blank')
        }
      }
      load()
    } catch {
      toast.error('Erro ao atualizar status')
    }
  }

  const openWhatsApp = (order) => {
    const phone = order.buyerPhone.replace(/\D/g, '')
    const msg = encodeURIComponent(`Olá ${order.buyerName}! Seu pedido #${order.id} no valor de ${fmt(order.totalAmount)}. Precisando de algo mais?`)
    window.open(`https://wa.me/55${phone}?text=${msg}`, '_blank')
  }

  const totalRevenue = orders.reduce((a, o) => a + o.totalAmount, 0)
  const totalProfit  = orders.reduce((a, o) => a + o.totalProfit, 0)

  if (loading) return <div className="flex items-center justify-center h-64"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div></div>

  return (
    <div className="p-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Pedidos</h1>
        <p className="text-gray-500 text-sm mt-1">{orders.length} pedido(s) encontrado(s)</p>
      </div>

      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="card">
          <p className="text-sm text-gray-500">Pedidos filtrados</p>
          <p className="text-2xl font-bold text-gray-900">{orders.length}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-500">Receita</p>
          <p className="text-2xl font-bold text-blue-600">{fmt(totalRevenue)}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-500">Lucro</p>
          <p className="text-2xl font-bold text-green-600">{fmt(totalProfit)}</p>
        </div>
      </div>

      <div className="card mb-6">
        <div className="flex flex-wrap gap-4 items-end">
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Status</label>
            <select value={filterStatus} onChange={e => setFilterStatus(e.target.value)} className="input w-40">
              <option value="">Todos</option>
              {Object.keys(statusConfig).map(s => <option key={s} value={s}>{statusConfig[s].label}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">De</label>
            <input type="date" value={dateFrom} onChange={e => setDateFrom(e.target.value)} className="input" />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Até</label>
            <input type="date" value={dateTo} onChange={e => setDateTo(e.target.value)} className="input" />
          </div>
          <button onClick={() => { setFilterStatus(''); setDateFrom(''); setDateTo('') }} className="btn-secondary">Limpar</button>
        </div>
      </div>

      {orders.length === 0 ? (
        <div className="card text-center py-16">
          <ShoppingBag size={48} className="text-gray-300 mx-auto mb-4" />
          <p className="text-gray-500 font-medium">Nenhum pedido encontrado</p>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map(order => {
            const sc = statusConfig[order.status] || {}
            const isExpanded = expanded === order.id
            return (
              <div key={order.id} className="card">
                <div className="flex items-start justify-between flex-wrap gap-4">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <span className="text-sm font-bold text-gray-400">#{order.id}</span>
                      <span className={sc.cls}>{sc.label}</span>
                    </div>
                    <p className="font-semibold text-gray-900">{order.buyerName}</p>
                    <p className="text-sm text-gray-500">{order.buyerStoreName}</p>
                    <p className="text-xs text-gray-400 mt-1">
                      {format(new Date(order.createdAt), "dd 'de' MMMM 'às' HH:mm", { locale: ptBR })}
                    </p>
                  </div>

                  <div className="text-right">
                    <p className="text-sm text-gray-500">Venda</p>
                    <p className="text-lg font-bold text-gray-900">{fmt(order.totalAmount)}</p>
                    <p className="text-sm text-green-600 font-semibold flex items-center gap-1 justify-end mt-0.5">
                      <TrendingUp size={14} /> Lucro: {fmt(order.totalProfit)}
                    </p>
                  </div>
                </div>

                <div className="flex flex-wrap gap-2 mt-4 pt-4 border-t border-gray-100">
                  <button onClick={() => setExpanded(isExpanded ? null : order.id)}
                    className="btn-secondary text-sm py-1.5 px-3">
                    {isExpanded ? 'Ocultar itens' : `Ver ${order.items?.length ?? 0} iten(s)`}
                  </button>
                  {nextStatus[order.status] && (
                    <button onClick={() => advance(order)} className="btn-primary text-sm py-1.5 px-3 flex items-center gap-1.5">
                      <CheckCircle size={14} /> Marcar como {nextStatus[order.status]}
                    </button>
                  )}
                  <button onClick={() => openWhatsApp(order)}
                    className="flex items-center gap-1.5 text-sm py-1.5 px-3 bg-green-500 hover:bg-green-600 text-white rounded-lg font-semibold transition">
                    <MessageSquare size={14} /> WhatsApp
                  </button>
                </div>

                {isExpanded && (
                  <div className="mt-4 bg-gray-50 rounded-xl p-4">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="text-left text-gray-500 text-xs uppercase">
                          <th className="pb-2">Produto</th>
                          <th className="pb-2 text-center">Qtd</th>
                          <th className="pb-2 text-right">Preço</th>
                          <th className="pb-2 text-right">Lucro</th>
                        </tr>
                      </thead>
                      <tbody>
                        {order.items?.map((item, i) => (
                          <tr key={i} className="border-t border-gray-200">
                            <td className="py-2 font-medium text-gray-800">{item.productName}</td>
                            <td className="py-2 text-center text-gray-600">{item.quantity}x</td>
                            <td className="py-2 text-right text-gray-800">{fmt(item.unitPrice * item.quantity)}</td>
                            <td className="py-2 text-right text-green-600 font-semibold">{fmt(item.profit)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
