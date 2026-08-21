import { Suspense, lazy } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'

import Home from './pages/Home'
import Login from './pages/auth/Login'
import BuyerRegister from './pages/auth/BuyerRegister'

import SellerLayout from './components/SellerLayout'
import BuyerLayout from './components/BuyerLayout'

// Carregadas sob demanda — só baixadas depois que o usuário loga e entra
// numa dessas telas, em vez de tudo junto no primeiro acesso ao site.
const Dashboard = lazy(() => import('./pages/seller/Dashboard'))
const Products = lazy(() => import('./pages/seller/Products'))
const Categories = lazy(() => import('./pages/seller/Categories'))
const SellerOrders = lazy(() => import('./pages/seller/SellerOrders'))
const SellerMessages = lazy(() => import('./pages/seller/SellerMessages'))
const SellerSettings = lazy(() => import('./pages/seller/Settings'))
const AdminPanel = lazy(() => import('./pages/seller/AdminPanel'))
const SellerProfile = lazy(() => import('./pages/seller/SellerProfile'))

const Shop = lazy(() => import('./pages/buyer/Shop'))
const BuyerOrders = lazy(() => import('./pages/buyer/BuyerOrders'))
const BuyerMessages = lazy(() => import('./pages/buyer/BuyerMessages'))
const BuyerProfile = lazy(() => import('./pages/buyer/BuyerProfile'))

const Spinner = () => (
  <div className="min-h-screen flex items-center justify-center">
    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
  </div>
)

function ProtectedSeller({ children }) {
  const { user, loading } = useAuth()
  if (loading) return <Spinner />
  if (!user) return <Navigate to="/login" replace />
  if (user.role !== 'Seller') return <Navigate to="/buyer/shop" replace />
  return children
}

function ProtectedBuyer({ children }) {
  const { user, loading } = useAuth()
  if (loading) return <Spinner />
  if (!user) return <Navigate to="/login" replace />
  if (user.role !== 'Buyer') return <Navigate to="/seller/dashboard" replace />
  return children
}

export default function App() {
  return (
    <AuthProvider>
      <Suspense fallback={<Spinner />}>
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
            <Route path="profile" element={<SellerProfile />} />
          </Route>

          <Route path="/buyer" element={<ProtectedBuyer><BuyerLayout /></ProtectedBuyer>}>
            <Route index element={<Navigate to="shop" replace />} />
            <Route path="shop" element={<Shop />} />
            <Route path="orders" element={<BuyerOrders />} />
            <Route path="messages" element={<BuyerMessages />} />
            <Route path="profile" element={<BuyerProfile />} />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Suspense>
    </AuthProvider>
  )
}
