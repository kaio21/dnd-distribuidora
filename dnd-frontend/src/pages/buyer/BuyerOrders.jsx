import { useState, useEffect, useRef } from 'react'
import api from '../../api/axios'
import { Package, CheckCircle, Clock, Truck, XCircle, ChevronDown, ChevronUp, FileText } from 'lucide-react'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import { gerarNotaFiscal } from '../../utils/notaFiscal'
import * as signalR from '@microsoft/signalr'
import { HUB_URL } from '../../api/config'
import toast from 'react-hot-toast'
import Pagination from '../../components/Pagination'

const PAGE_SIZE = 20

const fmt = (v) => `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`

const statusConfig = {
  Pendente:   { icon: Clock,         color: 'text-yellow-600', bg: 'bg-yellow-50',  cls: 'badge-pending',   label: 'Aguardando confirmação' },
  Confirmado: { icon: CheckCircle,   color: 'text-blue-600',   bg: 'bg-blue-50',    cls: 'badge-confirmed', label: 'Confirmado' },
  Pronto:     { icon: Package,       color: 'text-green-600',  bg: 'bg-green-50',   cls: 'badge-ready',     label: 'Pronto para retirada/entrega!' },
  Entregue:   { icon: Truck,         color: 'text-gray-600',   bg: 'bg-gray-50',    cls: 'badge-delivered', label: 'Entregue' },
  Cancelado:  { icon: XCircle,       color: 'text-red-600',    bg: 'bg-red-50',     cls: 'badge-cancelled', label: 'Cancelado' },
}

export default function BuyerOrders() {
  const [orders, setOrders] = useState([])
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [loading, setLoading] = useState(true)
  const [expanded, setExpanded] = useState(null)
  const { userId } = JSON.parse(localStorage.getItem('user') || '{}')

  const load = () => {
    api.get('/orders', { params: { page, pageSize: PAGE_SIZE } }).then(({ data }) => {
      setOrders(data.items)
      setTotalCount(data.totalCount)
      setTotalPages(data.totalPages || 1)
    }).finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [page])

  const loadRef = useRef(load)
  loadRef.current = load

  useEffect(() => {
    const token = localStorage.getItem('token')
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(`${HUB_URL}/hubs/chat?access_token=${token}`)
      .withAutomaticReconnect()
      .build()

    conn.on('OrderStatusUpdated', ({ orderId, status }) => {
      const sc = statusConfig[status]
      toast(sc?.label || `Pedido #${orderId}: ${status}`, { icon: '📦' })
      loadRef.current()
    })

    conn.start().then(() => {
      conn.invoke('JoinGroup', `buyer-${userId}`)
    }).catch(console.error)

    return () => conn.stop()
  }, [])

  if (loading && orders.length === 0) return <div className="flex items-center justify-center h-64"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div></div>

  return (
    <div className="p-4 md:p-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Meus Pedidos</h1>
        <p className="text-gray-500 text-sm mt-1">{totalCount} pedido(s)</p>
      </div>

      {orders.length === 0 ? (
        <div className="card text-center py-20">
          <Package size={48} className="text-gray-300 mx-auto mb-4" />
          <p className="text-gray-500 font-medium mb-2">Você ainda não fez pedidos</p>
          <a href="/buyer/shop" className="btn-primary inline-block">Ver produtos</a>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map(order => {
            const sc = statusConfig[order.status] || statusConfig.Pendente
            const Icon = sc.icon
            const isExpanded = expanded === order.id

            return (
              <div key={order.id} className="card">
                <div className={`flex items-start gap-4 p-4 rounded-xl mb-4 ${sc.bg}`}>
                  <Icon size={24} className={sc.color} />
                  <div>
                    <p className={`font-bold ${sc.color}`}>{sc.label}</p>
                    <p className="text-gray-500 text-sm mt-0.5">Pedido #{order.id}</p>
                  </div>
                  {order.status === 'Pronto' && (
                    <span className="ml-auto animate-pulse bg-green-500 text-white text-xs font-bold px-3 py-1 rounded-full">
                      PRONTO!
                    </span>
                  )}
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-500">
                      {format(new Date(order.createdAt), "dd 'de' MMMM 'às' HH:mm", { locale: ptBR })}
                    </p>
                    <p className="text-lg font-bold text-gray-900 mt-1">{fmt(order.totalAmount)}</p>
                    <p className="text-sm text-gray-500">{order.items?.length} iten(s)</p>
                  </div>
                  <button onClick={() => setExpanded(isExpanded ? null : order.id)}
                    className="btn-secondary flex items-center gap-2 text-sm py-2">
                    {isExpanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
                    {isExpanded ? 'Fechar' : 'Ver detalhes'}
                  </button>
                  <button onClick={() => gerarNotaFiscal(order)}
                    className="flex items-center gap-2 text-sm py-2 px-3 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl font-semibold transition">
                    <FileText size={16} /> Comprovante
                  </button>
                </div>

                {isExpanded && (
                  <div className="mt-4 pt-4 border-t border-gray-100">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="text-left text-xs text-gray-500 uppercase">
                          <th className="pb-2">Produto</th>
                          <th className="pb-2 text-center">Qtd</th>
                          <th className="pb-2 text-right">Preço unit.</th>
                          <th className="pb-2 text-right">Subtotal</th>
                        </tr>
                      </thead>
                      <tbody>
                        {order.items?.map((item, i) => (
                          <tr key={i} className="border-t border-gray-100">
                            <td className="py-2 font-medium text-gray-800">{item.productName}</td>
                            <td className="py-2 text-center text-gray-600">{item.quantity}x</td>
                            <td className="py-2 text-right text-gray-600">{fmt(item.unitPrice)}</td>
                            <td className="py-2 text-right font-semibold text-gray-800">{fmt(item.unitPrice * item.quantity)}</td>
                          </tr>
                        ))}
                        <tr className="border-t-2 border-gray-200">
                          <td colSpan={3} className="pt-3 text-right font-bold text-gray-700">Total:</td>
                          <td className="pt-3 text-right font-black text-blue-700 text-base">{fmt(order.totalAmount)}</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            )
          })}
        </div>
      )}

      <Pagination page={page} totalPages={totalPages} totalCount={totalCount} onPageChange={setPage} itemLabel="pedido(s)" />
    </div>
  )
}
