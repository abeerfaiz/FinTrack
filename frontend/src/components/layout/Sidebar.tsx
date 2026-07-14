import { Link, useLocation, useNavigate } from 'react-router-dom'
import {
  LayoutDashboard,
  ArrowLeftRight,
  Tag,
  PiggyBank,
  Landmark,
  LogOut,
} from 'lucide-react'

const navItems = [
  { path: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { path: '/transactions', label: 'Transactions', icon: ArrowLeftRight },
  { path: '/categories', label: 'Categories', icon: Tag },
  { path: '/budgets', label: 'Budgets', icon: PiggyBank },
  { path: '/accounts', label: 'Accounts', icon: Landmark },
]

export default function Sidebar() {
  const location = useLocation()
  const navigate = useNavigate()

  const handleLogout = () => {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    navigate('/login')
  }

  return (
    <div className="w-56 min-h-screen bg-white border-r border-gray-200 flex flex-col">
      <div className="px-4 py-6 border-b border-gray-100">
        <h1 className="text-lg font-semibold text-gray-900">FinTrack</h1>
        <p className="text-xs text-gray-400 mt-0.5">Personal finance</p>
      </div>

      <nav className="flex-1 px-3 py-4 space-y-1">
        {navItems.map(({ path, label, icon: Icon }) => {
          const active = location.pathname === path
          return (
            <Link
              key={path}
              to={path}
              className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors ${
                active
                  ? 'bg-gray-900 text-white'
                  : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
              }`}
            >
              <Icon size={16} />
              {label}
            </Link>
          )
        })}
      </nav>

      <div className="px-3 py-4 border-t border-gray-100">
        <button
          onClick={handleLogout}
          className="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-gray-600 hover:bg-gray-50 hover:text-gray-900 transition-colors w-full"
        >
          <LogOut size={16} />
          Sign out
        </button>
      </div>
    </div>
  )
}