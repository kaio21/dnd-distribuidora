import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'

import Home from './pages/Home'
import Login from './pages/auth/Login'
import BuyerRegister from './pages/auth/BuyerRegister'

import SellerLayout from './components/SellerLayout'
import Dashboard from './pages/seller/Dashboard'
import Products from './pages/seller/Products'
import Categories from './pages/seller/Categories'
import SellerOrders from './pages/seller/SellerOrders'
import SellerMessages from './pages/seller/SellerMessages'
import SellerSettings from './pages/seller/Settings'
import AdminPanel from './pages/seller/AdminPanel'

import BuyerLayout from './components/BuyerLayout'
import Shop from './pages/buyer/Shop'
import BuyerOrders from './pages/buyer/BuyerOrders'
import BuyerMessages from './pages/buyer/BuyerMessages'

function ProtectedSeller({ children }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="min-h-screen flex items-center justify-center"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div></div>
  if (!user) return <Navigate to="/login" replace />
  if (user.role !== 'Seller') return <Navigate to="/buyer/shop" replace />
  return children
}

function ProtectedBuyer({ children }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="min-h-screen flex items-center justify-center"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div></div>
  if (!user) return <Navigate to="/login" replace />
  if (user.role !== 'Buyer') return <Navigate to="/seller/dashboard" replace />
  return children
}

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/" element={<Home />} />

        <Route path="/login" element={<Login />} />
        <Route path="/seller/login" element={<Navigate to="/login" replace />} />
        <Route path="/buyer/login" element={<Navigate to="/login" replace />} />
        <Route path="/buyer/register" element={<BuyerRegister />} />

        <Route path="/seller" element={<ProtectedSeller><SellerLayout /></ProtectedSeller>}>
          <Route index element={<Navigate to="dashboard" replace />} />
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="products" element={<Products />} />
          <Route path="categories" element={<Categories />} />
          <Route path="orders" element={<SellerOrders />} />
          <Route path="messages" element={<SellerMessages />} />
          <Route path="settings" element={<SellerSettings />} />
          <Route path="admin" element={<AdminPanel />} />
        </Route>

        <Route path="/buyer" element={<ProtectedBuyer><BuyerLayout /></ProtectedBuyer>}>
          <Route index element={<Navigate to="shop" replace />} />
          <Route path="shop" element={<Shop />} />
          <Route path="orders" element={<BuyerOrders />} />
          <Route path="messages" element={<BuyerMessages />} />
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AuthProvider>
  )
}
