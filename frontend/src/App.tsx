import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import LoginPage from '@/pages/LoginPage'
import RegisterPage from '@/pages/RegisterPage'
import DashboardPage from '@/pages/DashboardPage'
import TransactionsPage from '@/pages/TransactionsPage'
import CategoriesPage from '@/pages/CategoriesPage'
import BudgetsPage from '@/pages/BudgetsPage'
import AccountsPage from '@/pages/AccountsPage'
import ProtectedRoute from '@/components/layout/ProtectedRoute'

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/dashboard" element={
                    <ProtectedRoute><DashboardPage /></ProtectedRoute>
                } />
                <Route path="/transactions" element={
                    <ProtectedRoute><TransactionsPage /></ProtectedRoute>
                } />
                <Route path="/categories" element={
                    <ProtectedRoute><CategoriesPage /></ProtectedRoute>
                } />
                <Route path="/budgets" element={
                    <ProtectedRoute><BudgetsPage /></ProtectedRoute>
                } />
                <Route path="/accounts" element={
                    <ProtectedRoute><AccountsPage /></ProtectedRoute>
                } />
                <Route path="/" element={<Navigate to="/dashboard" replace />} />
            </Routes>
        </BrowserRouter>
    )
}