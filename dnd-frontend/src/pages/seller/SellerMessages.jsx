import { useState, useEffect, useRef } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import * as signalR from '@microsoft/signalr'
import { HUB_URL } from '../../api/config'
import { Send, MessageSquare, ArrowLeft } from 'lucide-react'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'

export default function SellerMessages() {
  const [conversations, setConversations] = useState([])
  const [selected, setSelected] = useState(null)
  const [messages, setMessages] = useState([])
  const [text, setText] = useState('')
  const [sending, setSending] = useState(false)
  const [connection, setConnection] = useState(null)
  const bottomRef = useRef()

  const loadConversations = () => {
    api.get('/messages/conversations').then(r => setConversations(r.data)).catch(() => {})
  }

  useEffect(() => {
    loadConversations()
    const interval = setInterval(loadConversations, 15000)

    const token = localStorage.getItem('token')
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(`${HUB_URL}/hubs/chat?access_token=${token}`)
      .withAutomaticReconnect()
      .build()

    conn.on('ReceiveMessage', (msg) => {
      setMessages(prev => {
        if (prev.some(m => m.id === msg.id)) return prev
        return [...prev, msg]
      })
      loadConversations()
    })

    conn.start().then(() => {
      conn.invoke('JoinGroup', 'sellers')
    }).catch(console.error)

    setConnection(conn)
    return () => { conn.stop(); clearInterval(interval) }
  }, [])

  useEffect(() => {
    if (!selected) return
    api.get(`/messages/conversation/${selected.buyerId}`)
      .then(r => setMessages(r.data))
      .catch(() => toast.error('Erro ao carregar mensagens'))
    connection?.invoke('JoinGroup', `chat-${selected.buyerId}`).catch(() => {})
  }, [selected])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const send = async () => {
    if (!text.trim() || !selected) return
    setSending(true)
    try {
      await api.post('/messages', {
        receiverId: selected.buyerId,
        receiverType: 'Buyer',
        content: text.trim()
      })
      setText('')
    } catch {
      toast.error('Erro ao enviar mensagem')
    } finally {
      setSending(false)
    }
  }

  const openWhatsApp = () => {
    if (!selected) return
    const conv = conversations.find(c => c.buyerId === selected.buyerId)
    if (!conv) return
    const msg = encodeURIComponent(`Olá ${conv.buyerName}! D&D Distribuidora entrando em contato.`)
    window.open(`https://wa.me/55${conv.buyerPhone?.replace(/\D/g, '')}?text=${msg}`, '_blank')
  }

  return (
    <div className="flex h-full" style={{ height: 'calc(100vh - 0px)' }}>
      <aside className={`${selected ? 'hidden md:flex' : 'flex'} w-full md:w-72 border-r border-gray-200 bg-white flex-col`}>
        <div className="p-4 border-b border-gray-200">
          <h2 className="font-bold text-gray-900">Mensagens</h2>
          <p className="text-xs text-gray-500 mt-0.5">{conversations.length} conversa(s)</p>
        </div>
        <div className="flex-1 overflow-y-auto">
          {conversations.length === 0 && (
            <div className="p-8 text-center text-gray-400">
              <MessageSquare size={32} className="mx-auto mb-2 opacity-40" />
              <p className="text-sm">Sem conversas ainda</p>
            </div>
          )}
          {conversations.map(c => (
            <button key={c.buyerId} onClick={() => setSelected(c)}
              className={`w-full text-left px-4 py-3 border-b border-gray-100 hover:bg-gray-50 transition ${selected?.buyerId === c.buyerId ? 'bg-blue-50 border-l-4 border-l-blue-500' : ''}`}>
              <div className="flex items-center justify-between mb-1">
                <p className="font-semibold text-sm text-gray-900 truncate">{c.buyerName}</p>
                {c.unreadCount > 0 && (
                  <span className="bg-blue-600 text-white text-xs font-bold rounded-full w-5 h-5 flex items-center justify-center shrink-0">
                    {c.unreadCount}
                  </span>
                )}
              </div>
              <p className="text-xs text-gray-500 truncate">{c.buyerStoreName}</p>
              <p className="text-xs text-gray-400 truncate mt-0.5">{c.lastMessage}</p>
            </button>
          ))}
        </div>
      </aside>

      <div className={`${selected ? 'flex' : 'hidden md:flex'} flex-1 flex-col bg-gray-50`}>
        {!selected ? (
          <div className="flex-1 flex items-center justify-center text-gray-400">
            <div className="text-center">
              <MessageSquare size={48} className="mx-auto mb-3 opacity-30" />
              <p>Selecione uma conversa</p>
            </div>
          </div>
        ) : (
          <>
            <div className="bg-white border-b border-gray-200 px-4 md:px-6 py-4 flex items-center justify-between gap-3">
              <div className="flex items-center gap-3 min-w-0">
                <button onClick={() => setSelected(null)} className="md:hidden text-gray-500 hover:text-gray-700 shrink-0">
                  <ArrowLeft size={20} />
                </button>
                <div className="min-w-0">
                  <p className="font-bold text-gray-900 truncate">{selected.buyerName}</p>
                  <p className="text-xs text-gray-500 truncate">{selected.buyerStoreName}</p>
                </div>
              </div>
              <button onClick={openWhatsApp}
                className="flex items-center gap-2 bg-green-500 hover:bg-green-600 text-white text-sm font-semibold px-4 py-2 rounded-lg transition">
                WhatsApp
              </button>
            </div>

            <div className="flex-1 overflow-y-auto p-6 space-y-3">
              {messages.map(m => (
                <div key={m.id} className={`flex ${m.senderType === 'Seller' ? 'justify-end' : 'justify-start'}`}>
                  <div className={`max-w-xs lg:max-w-md px-4 py-2.5 rounded-2xl text-sm ${
                    m.senderType === 'Seller'
                      ? 'bg-blue-600 text-white rounded-br-sm'
                      : 'bg-white border border-gray-200 text-gray-800 rounded-bl-sm shadow-sm'}`}>
                    <p>{m.content}</p>
                    <p className={`text-xs mt-1 ${m.senderType === 'Seller' ? 'text-blue-200' : 'text-gray-400'}`}>
                      {format(new Date(m.createdAt), 'HH:mm', { locale: ptBR })}
                    </p>
                  </div>
                </div>
              ))}
              <div ref={bottomRef} />
            </div>

            <div className="bg-white border-t border-gray-200 p-4 flex gap-3">
              <input value={text} onChange={e => setText(e.target.value)}
                onKeyDown={e => e.key === 'Enter' && !e.shiftKey && send()}
                placeholder="Digite uma mensagem..." className="input flex-1" />
              <button onClick={send} disabled={sending || !text.trim()}
                className="btn-primary px-4 flex items-center gap-2">
                <Send size={16} />
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
