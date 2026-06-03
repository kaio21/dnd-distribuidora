import { Outlet, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { LayoutDashboard, Package, ShoppingBag, MessageSquare, LogOut, Store, Settings, Tag, Shield, UserCircle } from 'lucide-react'

export default function SellerLayout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const navItems = [
    { to: '/seller/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
    { to: '/seller/products',  icon: Package,          label: 'Produtos' },
    { to: '/seller/categories', icon: Tag,             label: 'Categorias' },
    { to: '/seller/orders',    icon: ShoppingBag,      label: 'Pedidos' },
    { to: '/seller/messages',  icon: MessageSquare,    label: 'Mensagens' },
    { to: '/seller/settings',  icon: Settings,         label: 'Configurações' },
    { to: '/seller/profile',   icon: UserCircle,       label: 'Meu Perfil' },
    ...(user?.sellerRole === 'Admin' ? [{ to: '/seller/admin', icon: Shield, label: 'Admin' }] : []),
  ]

  const handleLogout = () => { logout(); navigate('/') }

  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      {/* Sidebar — apenas desktop */}
      <aside className="hidden md:flex w-64 bg-blue-900 flex-col shrink-0">
        <div className="p-6 border-b border-blue-800">
          <div className="flex items-center gap-3">
            <div className="bg-blue-600 rounded-lg w-9 h-9 flex items-center justify-center">
              <Store size={18} className="text-white" />
            </div>
            <div>
              <p className="text-white font-bold text-sm leading-tight">D&D Distribuidora</p>
              <p className="text-blue-300 text-xs">Painel do Vendedor</p>
            </div>
          </div>
        </div>

        <div className="px-4 py-4 border-b border-blue-800">
          <p className="text-blue-400 text-xs mb-1">Logado como</p>
          <p className="text-white font-semibold text-sm truncate">{user?.name}</p>
          <p className="text-blue-300 text-xs truncate">{user?.email}</p>
          {user?.sellerRole && (
            <span className={`text-xs font-bold px-2 py-0.5 rounded-full mt-1 inline-block ${
              user.sellerRole === 'Admin' ? 'bg-yellow-400 text-yellow-900' :
              user.sellerRole === 'Gerente' ? 'bg-purple-400 text-purple-900' :
              'bg-blue-400 text-blue-900'
            }`}>{user.sellerRole}</span>
          )}
        </div>

        <nav className="flex-1 p-4 space-y-1">
          {navItems.map(({ to, icon: Icon, label }) => (
            <NavLink key={to} to={to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors ${
                  isActive ? 'bg-blue-600 text-white' : 'text-blue-300 hover:bg-blue-800 hover:text-white'
                }`
              }>
              <Icon size={18} />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="p-4 border-t border-blue-800">
          <button onClick={handleLogout}
            className="flex items-center gap-3 w-full px-4 py-3 text-blue-300 hover:text-red-400 hover:bg-blue-800 rounded-lg text-sm font-medium transition-colors">
            <LogOut size={18} />
            Sair
          </button>
        </div>
      </aside>

      {/* Header mobile */}
      <div className="flex flex-col flex-1 min-w-0">
        <header className="flex md:hidden items-center justify-between bg-blue-900 px-4 py-3 shrink-0">
          <div className="flex items-center gap-2">
            <div className="bg-blue-600 rounded-lg w-7 h-7 flex items-center justify-center">
              <Store size={14} className="text-white" />
            </div>
            <span className="text-white font-bold text-sm">D&D Distribuidora</span>
          </div>
          <span className="text-blue-300 text-xs truncate max-w-[140px]">{user?.name}</span>
        </header>

        <main className="flex-1 overflow-y-auto pb-16 md:pb-0">
          <Outlet />
        </main>
      </div>

      {/* Bottom nav — apenas mobile */}
      <nav className="fixed bottom-0 left-0 right-0 bg-blue-900 border-t border-blue-800 flex md:hidden z-50">
        {navItems.map(({ to, icon: Icon, label }) => (
          <NavLink key={to} to={to}
            className={({ isActive }) =>
              `flex-1 flex flex-col items-center justify-center py-2 text-xs font-medium transition-colors ${
                isActive ? 'text-white' : 'text-blue-400'
              }`
            }>
            <Icon size={20} className="mb-0.5" />
            {label}
          </NavLink>
        ))}
        <button onClick={handleLogout}
          className="flex-1 flex flex-col items-center justify-center py-2 text-xs font-medium text-blue-400 hover:text-red-400 transition-colors">
          <LogOut size={20} className="mb-0.5" />
          Sair
        </button>
      </nav>
    </div>
  )
}
