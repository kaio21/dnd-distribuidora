import { useState, useEffect, useRef } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import * as signalR from '@microsoft/signalr'
import { HUB_URL } from '../../api/config'
import { Send, MessageSquare } from 'lucide-react'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import { useAuth } from '../../context/AuthContext'

export default function BuyerMessages() {
  const { user } = useAuth()
  const [messages, setMessages] = useState([])
  const [text, setText] = useState('')
  const [sending, setSending] = useState(false)
  const [connection, setConnection] = useState(null)
  const bottomRef = useRef()

  const loadMessages = () => {
    api.get(`/messages/conversation/${user.userId}`)
      .then(r => setMessages(r.data))
      .catch(() => {})
  }

  useEffect(() => {
    loadMessages()

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
    })

    conn.start().then(() => {
      conn.invoke('JoinGroup', `chat-${user.userId}`)
    }).catch(console.error)

    setConnection(conn)
    return () => conn.stop()
  }, [])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const send = async () => {
    if (!text.trim()) return
    setSending(true)
    try {
      await api.post('/messages', {
        receiverId: 1,
        receiverType: 'Seller',
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
    const phone = '5511999999999'
    const msg = encodeURIComponent(`Olá! Sou ${user.name} e gostaria de falar sobre um pedido na D&D Distribuidora.`)
    window.open(`https://wa.me/${phone}?text=${msg}`, '_blank')
  }

  return (
    <div className="flex flex-col h-screen">
      <div className="bg-white border-b border-gray-200 px-8 py-5 flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-900">Mensagens</h1>
          <p className="text-sm text-gray-500">Fale com a D&D Distribuidora</p>
        </div>
        <button onClick={openWhatsApp}
          className="flex items-center gap-2 bg-green-500 hover:bg-green-600 text-white font-semibold px-4 py-2.5 rounded-xl transition text-sm">
          <MessageSquare size={16} />
          Contato WhatsApp
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-6 space-y-3 bg-gray-50">
        {messages.length === 0 && (
          <div className="text-center py-20 text-gray-400">
            <MessageSquare size={48} className="mx-auto mb-3 opacity-30" />
            <p>Nenhuma mensagem ainda. Diga olá!</p>
          </div>
        )}
        {messages.map(m => (
          <div key={m.id} className={`flex ${m.senderType === 'Buyer' ? 'justify-end' : 'justify-start'}`}>
            <div className={`max-w-xs lg:max-w-md px-4 py-2.5 rounded-2xl text-sm ${
              m.senderType === 'Buyer'
                ? 'bg-blue-600 text-white rounded-br-sm'
                : 'bg-white border border-gray-200 text-gray-800 rounded-bl-sm shadow-sm'}`}>
              {m.senderType === 'Seller' && (
                <p className="text-xs font-bold text-blue-600 mb-1">D&D Distribuidora</p>
              )}
              <p>{m.content}</p>
              <p className={`text-xs mt-1 ${m.senderType === 'Buyer' ? 'text-blue-200' : 'text-gray-400'}`}>
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
    </div>
  )
}
