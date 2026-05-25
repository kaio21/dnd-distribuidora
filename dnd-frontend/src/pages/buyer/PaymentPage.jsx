import { useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { CreditCard, Smartphone, ArrowLeft, Copy, Check, Lock } from 'lucide-react'
import { maskCard, maskExpiry } from '../../utils/masks'

const fmt = (v) => `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`

const PIX_KEY = '11999999999'
const PIX_NAME = 'D&D Distribuidora'

export default function PaymentPage() {
  const { state } = useLocation()
  const navigate = useNavigate()
  const [method, setMethod] = useState('pix')
  const [copied, setCopied] = useState(false)
  const [ordering, setOrdering] = useState(false)
  const [card, setCard] = useState({ number: '', name: '', expiry: '', cvv: '' })
  const [installments, setInstallments] = useState('1')

  const cart = state?.cart || []
  const total = cart.reduce((a, i) => a + i.salePrice * i.qty, 0)

  if (cart.length === 0) {
    navigate('/buyer/shop')
    return null
  }

  const copyPix = () => {
    navigator.clipboard.writeText(PIX_KEY)
    setCopied(true)
    toast.success('Chave Pix copiada!')
    setTimeout(() => setCopied(false), 2000)
  }

  const handlePay = async () => {
    if (method !== 'pix') {
      if (!card.number || !card.name || !card.expiry || !card.cvv) {
        toast.error('Preencha todos os dados do cartão')
        return
      }
    }
    setOrdering(true)
    try {
      await api.post('/orders', {
        items: cart.map(i => ({ productId: i.id, quantity: i.qty })),
        paymentMethod: method,
      })
      toast.success('Pedido realizado com sucesso! 🎉')
      navigate('/buyer/orders')
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao realizar pedido')
    } finally {
      setOrdering(false)
    }
  }

  const tabs = [
    { id: 'pix', label: 'Pix', icon: Smartphone },
    { id: 'credit', label: 'Crédito', icon: CreditCard },
    { id: 'debit', label: 'Débito', icon: CreditCard },
  ]

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      <div className="max-w-2xl mx-auto">
        <button onClick={() => navigate('/buyer/shop')}
          className="flex items-center gap-2 text-gray-500 hover:text-gray-700 mb-6 text-sm">
          <ArrowLeft size={16} /> Voltar à loja
        </button>

        <h1 className="text-2xl font-bold text-gray-900 mb-6">Pagamento</h1>

        {/* Resumo do pedido */}
        <div className="card mb-6">
          <h2 className="font-bold text-gray-800 mb-3">Resumo do pedido</h2>
          <div className="space-y-2">
            {cart.map(item => (
              <div key={item.id} className="flex justify-between text-sm">
                <span className="text-gray-600">{item.name} <span className="text-gray-400">x{item.qty}</span></span>
                <span className="font-medium text-gray-800">{fmt(item.salePrice * item.qty)}</span>
              </div>
            ))}
          </div>
          <div className="border-t border-gray-100 mt-3 pt-3 flex justify-between">
            <span className="font-bold text-gray-700">Total</span>
            <span className="text-xl font-black text-blue-700">{fmt(total)}</span>
          </div>
        </div>

        {/* Método de pagamento */}
        <div className="card">
          <h2 className="font-bold text-gray-800 mb-4">Forma de pagamento</h2>

          <div className="flex gap-2 mb-6">
            {tabs.map(({ id, label, icon: Icon }) => (
              <button key={id} onClick={() => setMethod(id)}
                className={`flex-1 flex flex-col items-center gap-1 py-3 rounded-xl border-2 text-sm font-medium transition ${
                  method === id ? 'border-blue-600 bg-blue-50 text-blue-700' : 'border-gray-200 text-gray-500 hover:border-gray-300'
                }`}>
                <Icon size={20} />
                {label}
              </button>
            ))}
          </div>

          {/* PIX */}
          {method === 'pix' && (
            <div className="space-y-4">
              <div className="bg-green-50 border border-green-200 rounded-xl p-4 text-center">
                <div className="w-40 h-40 mx-auto bg-white border-2 border-green-300 rounded-xl flex items-center justify-center mb-3">
                  <div className="grid grid-cols-5 gap-0.5">
                    {Array.from({ length: 25 }).map((_, i) => (
                      <div key={i} className={`w-5 h-5 rounded-sm ${Math.random() > 0.5 ? 'bg-gray-900' : 'bg-white'}`} />
                    ))}
                  </div>
                </div>
                <p className="text-green-700 font-semibold text-sm">QR Code Pix</p>
              </div>

              <div className="bg-gray-50 rounded-xl p-4">
                <p className="text-xs text-gray-500 mb-1">Chave Pix (Telefone)</p>
                <div className="flex items-center justify-between gap-3">
                  <span className="font-mono font-bold text-gray-800">{PIX_KEY}</span>
                  <button onClick={copyPix}
                    className="flex items-center gap-1.5 bg-green-500 hover:bg-green-600 text-white text-xs font-semibold px-3 py-1.5 rounded-lg transition">
                    {copied ? <Check size={14} /> : <Copy size={14} />}
                    {copied ? 'Copiado!' : 'Copiar'}
                  </button>
                </div>
                <p className="text-xs text-gray-500 mt-1">Beneficiário: <strong>{PIX_NAME}</strong></p>
              </div>

              <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-3 text-xs text-yellow-800">
                Após realizar o Pix, clique em "Confirmar pagamento". Seu pedido será processado após a confirmação.
              </div>
            </div>
          )}

          {/* CRÉDITO / DÉBITO */}
          {(method === 'credit' || method === 'debit') && (
            <div className="space-y-4">
              <div className="bg-gradient-to-r from-blue-600 to-blue-800 rounded-2xl p-5 text-white">
                <p className="text-xs opacity-70 mb-4">D&D Distribuidora</p>
                <p className="font-mono text-lg tracking-widest mb-4">
                  {card.number || '0000 0000 0000 0000'}
                </p>
                <div className="flex justify-between text-sm">
                  <span>{card.name || 'NOME DO TITULAR'}</span>
                  <span>{card.expiry || 'MM/AA'}</span>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Número do cartão</label>
                <input value={card.number}
                  onChange={e => setCard(p => ({ ...p, number: maskCard(e.target.value) }))}
                  placeholder="0000 0000 0000 0000" className="input font-mono" maxLength={19} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Nome do titular</label>
                <input value={card.name}
                  onChange={e => setCard(p => ({ ...p, name: e.target.value.toUpperCase() }))}
                  placeholder="COMO ESTÁ NO CARTÃO" className="input uppercase" />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Validade</label>
                  <input value={card.expiry}
                    onChange={e => setCard(p => ({ ...p, expiry: maskExpiry(e.target.value) }))}
                    placeholder="MM/AA" className="input" maxLength={5} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">CVV</label>
                  <input value={card.cvv}
                    onChange={e => setCard(p => ({ ...p, cvv: e.target.value.replace(/\D/g, '').slice(0, 4) }))}
                    placeholder="000" className="input" maxLength={4} type="password" />
                </div>
              </div>

              {method === 'credit' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Parcelas</label>
                  <select value={installments} onChange={e => setInstallments(e.target.value)} className="input">
                    {[1,2,3,4,5,6,10,12].map(n => (
                      <option key={n} value={n}>
                        {n}x de {fmt(total / n)}{n === 1 ? ' (sem juros)' : ''}
                      </option>
                    ))}
                  </select>
                </div>
              )}
            </div>
          )}

          <button onClick={handlePay} disabled={ordering}
            className="btn-primary w-full py-3 mt-6 flex items-center justify-center gap-2 text-base">
            <Lock size={16} />
            {ordering ? 'Processando...' : `Confirmar pagamento • ${fmt(total)}`}
          </button>

          <p className="text-center text-xs text-gray-400 mt-3 flex items-center justify-center gap-1">
            <Lock size={12} /> Pagamento seguro e criptografado
          </p>
        </div>
      </div>
    </div>
  )
}
