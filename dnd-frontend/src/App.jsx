import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'

import Home from './pages/Home'
import SellerLogin from './pages/auth/SellerLogin'
import SellerRegister from './pages/auth/SellerRegister'
import BuyerLogin from './pages/auth/BuyerLogin'
import BuyerRegister from './pages/auth/BuyerRegister'

import SellerLayout from './components/SellerLayout'
import Dashboard from './pages/seller/Dashboard'
import Products from './pages/seller/Products'
import SellerOrders from './pages/seller/SellerOrders'
import SellerMessages from './pages/seller/SellerMessages'

import BuyerLayout from './components/BuyerLayout'
import Shop from './pages/buyer/Shop'
import BuyerOrders from './pages/buyer/BuyerOrders'
import BuyerMessages from './pages/buyer/BuyerMessages'

function ProtectedSeller({ children }) {
  const { user } = useAuth()
  if (!user) return <Navigate to="/seller/login" replace />
  if (user.role !== 'Seller') return <Navigate to="/buyer/shop" replace />
  return children
}

function ProtectedBuyer({ children }) {
  const { user } = useAuth()
  if (!user) return <Navigate to="/buyer/login" replace />
  if (user.role !== 'Buyer') return <Navigate to="/seller/dashboard" replace />
  return children
}

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/" element={<Home />} />

        <Route path="/seller/login" element={<SellerLogin />} />
        <Route path="/seller/register" element={<SellerRegister />} />
        <Route path="/buyer/login" element={<BuyerLogin />} />
        <Route path="/buyer/register" element={<BuyerRegister />} />

        <Route path="/seller" element={<ProtectedSeller><SellerLayout /></ProtectedSeller>}>
          <Route index element={<Navigate to="dashboard" replace />} />
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="products" element={<Products />} />
          <Route path="orders" element={<SellerOrders />} />
          <Route path="messages" element={<SellerMessages />} />
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
