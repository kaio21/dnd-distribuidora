import { Link } from 'react-router-dom'

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-900 via-blue-800 to-blue-700 flex flex-col">
      <header className="px-8 py-6 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="bg-white rounded-xl w-10 h-10 flex items-center justify-center">
            <span className="text-blue-700 font-black text-lg">D</span>
          </div>
          <span className="text-white font-bold text-xl">D&D Distribuidora</span>
        </div>
      </header>

      <main className="flex-1 flex flex-col items-center justify-center px-4 text-center">
        <h1 className="text-5xl font-black text-white mb-4 leading-tight">
          D&D Distribuidora<br />
          <span className="text-blue-200">Atacadista</span>
        </h1>
        <p className="text-blue-100 text-lg mb-12 max-w-md">
          Conectando lojistas e compradores com agilidade, controle de estoque e gestão de lucros em tempo real.
        </p>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 w-full max-w-2xl">
          <div className="bg-white/10 backdrop-blur-sm border border-white/20 rounded-2xl p-8 text-left">
            <div className="text-3xl mb-4">🏪</div>
            <h2 className="text-white font-bold text-2xl mb-2">Sou Vendedor (Logista)</h2>
            <p className="text-blue-200 text-sm mb-6">Gerencie produtos, acompanhe pedidos e veja seu lucro em tempo real.</p>
            <div className="flex flex-col gap-3">
              <Link to="/seller/login"
                className="bg-white text-blue-700 font-bold py-3 px-6 rounded-xl hover:bg-blue-50 transition text-center">
                Entrar como Vendedor
              </Link>
              <Link to="/seller/register"
                className="border border-white/40 text-white font-semibold py-3 px-6 rounded-xl hover:bg-white/10 transition text-center">
                Criar conta Vendedor
              </Link>
            </div>
          </div>

          <div className="bg-white/10 backdrop-blur-sm border border-white/20 rounded-2xl p-8 text-left">
            <div className="text-3xl mb-4">🛒</div>
            <h2 className="text-white font-bold text-2xl mb-2">Sou Comprador</h2>
            <p className="text-blue-200 text-sm mb-6">Encontre produtos, faça pedidos e acompanhe suas compras com facilidade.</p>
            <div className="flex flex-col gap-3">
              <Link to="/buyer/login"
                className="bg-white text-blue-700 font-bold py-3 px-6 rounded-xl hover:bg-blue-50 transition text-center">
                Entrar como Comprador
              </Link>
              <Link to="/buyer/register"
                className="border border-white/40 text-white font-semibold py-3 px-6 rounded-xl hover:bg-white/10 transition text-center">
                Criar conta Comprador
              </Link>
            </div>
          </div>
        </div>
      </main>

      <footer className="text-center py-6 text-blue-300 text-sm">
        © 2024 D&D Distribuidora Atacadista — Todos os direitos reservados
      </footer>
    </div>
  )
}
