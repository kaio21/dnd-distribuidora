import { Outlet, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { ShoppingCart, Package, MessageSquare, LogOut, User } from 'lucide-react'

const navItems = [
  { to: '/buyer/shop',     icon: ShoppingCart,  label: 'Loja' },
  { to: '/buyer/orders',   icon: Package,       label: 'Meus Pedidos' },
  { to: '/buyer/messages', icon: MessageSquare, label: 'Mensagens' },
]

export default function BuyerLayout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => { logout(); navigate('/') }

  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      <aside className="w-64 bg-gray-900 flex flex-col shrink-0">
        <div className="p-6 border-b border-gray-800">
          <div className="flex items-center gap-3">
            <div className="bg-blue-600 rounded-lg w-9 h-9 flex items-center justify-center">
              <ShoppingCart size={18} className="text-white" />
            </div>
            <div>
              <p className="text-white font-bold text-sm leading-tight">D&D Distribuidora</p>
              <p className="text-gray-400 text-xs">Portal do Comprador</p>
            </div>
          </div>
        </div>

        <div className="px-4 py-4 border-b border-gray-800">
          <p className="text-gray-500 text-xs mb-1">Logado como</p>
          <p className="text-white font-semibold text-sm truncate">{user?.name}</p>
          <p className="text-gray-400 text-xs truncate">{user?.email}</p>
        </div>

        <nav className="flex-1 p-4 space-y-1">
          {navItems.map(({ to, icon: Icon, label }) => (
            <NavLink key={to} to={to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-blue-600 text-white'
                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                }`
              }>
              <Icon size={18} />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="p-4 border-t border-gray-800">
          <button onClick={handleLogout}
            className="flex items-center gap-3 w-full px-4 py-3 text-gray-400 hover:text-red-400 hover:bg-gray-800 rounded-lg text-sm font-medium transition-colors">
            <LogOut size={18} />
            Sair
          </button>
        </div>
      </aside>

      <main className="flex-1 overflow-y-auto">
        <Outlet />
      </main>
    </div>
  )
}
