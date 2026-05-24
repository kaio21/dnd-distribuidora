import { useState, useEffect, useRef } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { Plus, Edit, Trash2, Pause, Play, Package, Search, Image } from 'lucide-react'

const fmt = (v) => `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`

const emptyForm = { name: '', description: '', costPrice: '', salePrice: '', stock: '', isActive: true }

export default function Products() {
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editing, setEditing] = useState(null)
  const [form, setForm] = useState(emptyForm)
  const [imageFile, setImageFile] = useState(null)
  const [imagePreview, setImagePreview] = useState(null)
  const [saving, setSaving] = useState(false)
  const fileRef = useRef()

  const load = () => {
    api.get('/products').then(r => setProducts(r.data)).finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [])

  const openCreate = () => {
    setEditing(null)
    setForm(emptyForm)
    setImageFile(null)
    setImagePreview(null)
    setShowModal(true)
  }

  const openEdit = (p) => {
    setEditing(p)
    setForm({ name: p.name, description: p.description, costPrice: p.costPrice, salePrice: p.salePrice, stock: p.stock, isActive: p.isActive })
    setImagePreview(p.imageUrl || null)
    setImageFile(null)
    setShowModal(true)
  }

  const handleImageChange = (e) => {
    const file = e.target.files[0]
    if (!file) return
    setImageFile(file)
    setImagePreview(URL.createObjectURL(file))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      const fd = new FormData()
      Object.entries(form).forEach(([k, v]) => fd.append(k, v))
      if (imageFile) fd.append('image', imageFile)

      if (editing) {
        await api.put(`/products/${editing.id}`, fd, { headers: { 'Content-Type': 'multipart/form-data' } })
        toast.success('Produto atualizado!')
      } else {
        await api.post('/products', fd, { headers: { 'Content-Type': 'multipart/form-data' } })
        toast.success('Produto criado!')
      }
      setShowModal(false)
      load()
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao salvar produto')
    } finally {
      setSaving(false)
    }
  }

  const handleToggle = async (p) => {
    await api.patch(`/products/${p.id}/toggle`)
    toast.success(p.isActive ? 'Produto pausado' : 'Produto ativado')
    load()
  }

  const handleDelete = async (p) => {
    if (!confirm(`Excluir "${p.name}"?`)) return
    await api.delete(`/products/${p.id}`)
    toast.success('Produto excluído')
    load()
  }

  const filtered = products.filter(p =>
    p.name.toLowerCase().includes(search.toLowerCase()))

  if (loading) return <div className="flex items-center justify-center h-64"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div></div>

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Produtos</h1>
          <p className="text-gray-500 text-sm mt-1">{products.length} produto(s) cadastrado(s)</p>
        </div>
        <button onClick={openCreate} className="btn-primary flex items-center gap-2">
          <Plus size={18} /> Novo Produto
        </button>
      </div>

      <div className="card mb-6">
        <div className="relative">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input value={search} onChange={e => setSearch(e.target.value)}
            placeholder="Buscar produto..." className="input pl-9" />
        </div>
      </div>

      {filtered.length === 0 ? (
        <div className="card text-center py-16">
          <Package size={48} className="text-gray-300 mx-auto mb-4" />
          <p className="text-gray-500 font-medium">Nenhum produto encontrado</p>
          <button onClick={openCreate} className="btn-primary mt-4">Cadastrar primeiro produto</button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5">
          {filtered.map(p => (
            <div key={p.id} className={`card relative ${!p.isActive ? 'opacity-60' : ''}`}>
              {!p.isActive && (
                <div className="absolute top-3 right-3 bg-gray-200 text-gray-600 text-xs font-bold px-2 py-0.5 rounded">PAUSADO</div>
              )}
              <div className="aspect-video bg-gray-100 rounded-lg mb-4 overflow-hidden">
                {p.imageUrl
                  ? <img src={p.imageUrl} alt={p.name} className="w-full h-full object-cover" />
                  : <div className="flex items-center justify-center h-full text-gray-300"><Image size={40} /></div>}
              </div>
              <h3 className="font-bold text-gray-900 mb-1">{p.name}</h3>
              <p className="text-gray-500 text-sm mb-3 line-clamp-2">{p.description}</p>
              <div className="grid grid-cols-3 gap-2 mb-4 text-center">
                <div className="bg-red-50 rounded-lg p-2">
                  <p className="text-xs text-red-500 font-medium">Custo</p>
                  <p className="text-sm font-bold text-red-700">{fmt(p.costPrice)}</p>
                </div>
                <div className="bg-blue-50 rounded-lg p-2">
                  <p className="text-xs text-blue-500 font-medium">Venda</p>
                  <p className="text-sm font-bold text-blue-700">{fmt(p.salePrice)}</p>
                </div>
                <div className="bg-green-50 rounded-lg p-2">
                  <p className="text-xs text-green-500 font-medium">Lucro</p>
                  <p className="text-sm font-bold text-green-700">{fmt(p.profit)}</p>
                </div>
              </div>
              <p className="text-sm text-gray-600 mb-4">Estoque: <span className={`font-bold ${p.stock === 0 ? 'text-red-600' : 'text-gray-800'}`}>{p.stock} un.</span></p>
              <div className="flex gap-2">
                <button onClick={() => openEdit(p)} className="btn-secondary flex-1 flex items-center justify-center gap-1.5 text-sm py-2">
                  <Edit size={14} /> Editar
                </button>
                <button onClick={() => handleToggle(p)} className={`flex-1 flex items-center justify-center gap-1.5 text-sm py-2 rounded-lg font-semibold transition ${p.isActive ? 'bg-yellow-100 text-yellow-700 hover:bg-yellow-200' : 'bg-green-100 text-green-700 hover:bg-green-200'}`}>
                  {p.isActive ? <><Pause size={14} /> Pausar</> : <><Play size={14} /> Ativar</>}
                </button>
                <button onClick={() => handleDelete(p)} className="p-2 rounded-lg bg-red-50 text-red-500 hover:bg-red-100 transition">
                  <Trash2 size={16} />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg p-6 max-h-screen overflow-y-auto">
            <h2 className="text-lg font-bold text-gray-900 mb-6">{editing ? 'Editar Produto' : 'Novo Produto'}</h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Nome do produto</label>
                <input value={form.name} onChange={e => setForm(f => ({...f, name: e.target.value}))}
                  required placeholder="Ex: Camiseta Básica" className="input" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Descrição</label>
                <textarea value={form.description} onChange={e => setForm(f => ({...f, description: e.target.value}))}
                  rows={3} placeholder="Descrição do produto..." className="input resize-none" />
              </div>
              <div className="grid grid-cols-3 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Preço de custo</label>
                  <input value={form.costPrice} onChange={e => setForm(f => ({...f, costPrice: e.target.value}))}
                    required type="number" step="0.01" min="0" placeholder="0,00" className="input" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Preço de venda</label>
                  <input value={form.salePrice} onChange={e => setForm(f => ({...f, salePrice: e.target.value}))}
                    required type="number" step="0.01" min="0" placeholder="0,00" className="input" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Estoque</label>
                  <input value={form.stock} onChange={e => setForm(f => ({...f, stock: e.target.value}))}
                    required type="number" min="0" placeholder="0" className="input" />
                </div>
              </div>
              {form.costPrice && form.salePrice && (
                <div className="bg-green-50 border border-green-200 rounded-lg p-3 text-sm">
                  <span className="text-green-700 font-medium">Lucro por unidade: </span>
                  <span className="text-green-800 font-bold">{fmt(form.salePrice - form.costPrice)}</span>
                  <span className="text-green-600 ml-2">({(((form.salePrice - form.costPrice) / form.costPrice) * 100).toFixed(0)}%)</span>
                </div>
              )}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Imagem do produto</label>
                <div className="border-2 border-dashed border-gray-200 rounded-xl p-4 text-center cursor-pointer hover:border-blue-400 transition" onClick={() => fileRef.current?.click()}>
                  {imagePreview
                    ? <img src={imagePreview} alt="Preview" className="h-32 mx-auto object-contain rounded-lg" />
                    : <><Image size={32} className="text-gray-300 mx-auto mb-2" /><p className="text-sm text-gray-400">Clique para selecionar imagem</p></>}
                </div>
                <input ref={fileRef} type="file" accept="image/*" onChange={handleImageChange} className="hidden" />
              </div>
              {editing && (
                <label className="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" checked={form.isActive} onChange={e => setForm(f => ({...f, isActive: e.target.checked}))} className="rounded" />
                  <span className="text-sm font-medium text-gray-700">Produto ativo (visível para compradores)</span>
                </label>
              )}
              <div className="flex gap-3 pt-2">
                <button type="button" onClick={() => setShowModal(false)} className="btn-secondary flex-1">Cancelar</button>
                <button type="submit" disabled={saving} className="btn-primary flex-1">{saving ? 'Salvando...' : 'Salvar'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
