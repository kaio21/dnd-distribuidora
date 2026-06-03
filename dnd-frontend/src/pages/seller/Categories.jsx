import { useState, useEffect } from 'react'
import api from '../../api/axios'
import toast from 'react-hot-toast'
import { Plus, Trash2, Tag } from 'lucide-react'
import ConfirmModal from '../../components/ConfirmModal'

export default function Categories() {
  const [categories, setCategories] = useState([])
  const [loading, setLoading] = useState(true)
  const [newName, setNewName] = useState('')
  const [saving, setSaving] = useState(false)
  const [confirmDelete, setConfirmDelete] = useState(null) // categoria a excluir

  const load = () =>
    api.get('/categories')
      .then(r => setCategories(r.data))
      .finally(() => setLoading(false))

  useEffect(() => { load() }, [])

  const handleCreate = async (e) => {
    e.preventDefault()
    const name = newName.trim()
    if (!name) return
    setSaving(true)
    try {
      await api.post('/categories', { name })
      toast.success('Categoria criada!')
      setNewName('')
      load()
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao criar categoria')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async () => {
    const cat = confirmDelete
    setConfirmDelete(null)
    try {
      await api.delete(`/categories/${cat.id}`)
      toast.success('Categoria excluída')
      load()
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao excluir categoria')
    }
  }

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
  )

  return (
    <div className="p-4 md:p-8 max-w-2xl">
      <ConfirmModal
        open={!!confirmDelete}
        title={`Excluir categoria "${confirmDelete?.name}"?`}
        description="Os produtos desta categoria não serão excluídos, mas ficarão sem categoria. Só é possível excluir categorias sem produtos vinculados."
        confirmLabel="Excluir categoria"
        onConfirm={handleDelete}
        onCancel={() => setConfirmDelete(null)}
      />
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Categorias</h1>
        <p className="text-gray-500 text-sm mt-1">Crie categorias para organizar seus produtos</p>
      </div>

      {/* Formulário nova categoria */}
      <div className="card mb-6">
        <h2 className="text-sm font-semibold text-gray-700 mb-3">Nova categoria</h2>
        <form onSubmit={handleCreate} className="flex gap-3">
          <input
            value={newName}
            onChange={e => setNewName(e.target.value)}
            placeholder="Ex: Açaí, Alho, Açúcar..."
            className="input flex-1"
            maxLength={100}
          />
          <button type="submit" disabled={saving || !newName.trim()} className="btn-primary flex items-center gap-2">
            <Plus size={16} />
            {saving ? 'Salvando...' : 'Criar'}
          </button>
        </form>
      </div>

      {/* Lista de categorias */}
      {categories.length === 0 ? (
        <div className="card text-center py-12">
          <Tag size={40} className="text-gray-300 mx-auto mb-3" />
          <p className="text-gray-500 font-medium">Nenhuma categoria criada</p>
          <p className="text-gray-400 text-sm mt-1">Crie categorias para organizar seus produtos</p>
        </div>
      ) : (
        <div className="card p-0 overflow-hidden">
          <div className="px-5 py-3 bg-gray-50 border-b border-gray-100">
            <span className="text-sm font-semibold text-gray-600">{categories.length} categoria(s)</span>
          </div>
          {categories.map((cat, i) => (
            <div key={cat.id} className={`flex items-center justify-between px-5 py-4 ${i > 0 ? 'border-t border-gray-100' : ''}`}>
              <div className="flex items-center gap-3">
                <Tag size={16} className="text-blue-500" />
                <span className="font-semibold text-gray-800">{cat.name}</span>
              </div>
              <button
                onClick={() => setConfirmDelete(cat)}
                className="text-gray-400 hover:text-red-500 transition p-1.5 rounded-lg hover:bg-red-50">
                <Trash2 size={16} />
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
