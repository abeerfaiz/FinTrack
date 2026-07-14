import { useEffect, useState, useCallback } from 'react'
import { RefreshCw, Filter } from 'lucide-react'
import AppLayout from '@/components/layout/AppLayout'
import {
    getTransactions,
    getCategories,
    getAccounts,
    syncTransactions,
    categoriseTransaction,
} from '@/api/endpoints'
import type { TransactionDto, CategoryDto, AccountDto, PagedResult } from '@/types'
import { formatCurrency, formatDate } from '@/lib/utils'

export default function TransactionsPage() {
    const [data, setData] = useState<PagedResult<TransactionDto> | null>(null)
    const [categories, setCategories] = useState<CategoryDto[]>([])
    const [accounts, setAccounts] = useState<AccountDto[]>([])
    const [loading, setLoading] = useState(true)
    const [syncing, setSyncing] = useState(false)
    const [page, setPage] = useState(1)

    // Filters
    const [accountFilter, setAccountFilter] = useState('')
    const [categoryFilter, setCategoryFilter] = useState('')
    const [statusFilter, setStatusFilter] = useState('')
    const [fromFilter, setFromFilter] = useState('')
    const [toFilter, setToFilter] = useState('')

    // Category assignment
    const [assigningId, setAssigningId] = useState<string | null>(null)

    const loadTransactions = useCallback(async () => {
        setLoading(true)
        try {
            const res = await getTransactions({
                page,
                pageSize: 20,
                accountId: accountFilter || undefined,
                categoryId: categoryFilter || undefined,
                status: statusFilter || undefined,
                from: fromFilter || undefined,
                to: toFilter || undefined,
            })
            setData(res.data)
        } catch (err) {
            console.error('Failed to load transactions:', err)
        } finally {
            setLoading(false)
        }
    }, [page, accountFilter, categoryFilter, statusFilter, fromFilter, toFilter])

    useEffect(() => {
        async function loadSupporting() {
            const [catRes, accRes] = await Promise.all([
                getCategories(),
                getAccounts(),
            ])
            setCategories(catRes.data)
            setAccounts(accRes.data)
        }
        loadSupporting()
    }, [])

    useEffect(() => {
        loadTransactions()
    }, [loadTransactions])

    const handleSync = async () => {
        if (accounts.length === 0) return
        setSyncing(true)
        try {
            // Sync all connections — use first account's connection for simplicity
            // In a full app you'd sync per bank connection
            await Promise.all(
                accounts.map(a =>
                    syncTransactions(a.id).catch(() => null)
                )
            )
            await loadTransactions()
        } finally {
            setSyncing(false)
        }
    }

    const handleCategorise = async (transactionId: string, categoryId: string) => {
        setAssigningId(transactionId)
        try {
            await categoriseTransaction(transactionId, categoryId)
            await loadTransactions()
        } catch (err) {
            console.error('Failed to categorise:', err)
        } finally {
            setAssigningId(null)
        }
    }

    const resetFilters = () => {
        setAccountFilter('')
        setCategoryFilter('')
        setStatusFilter('')
        setFromFilter('')
        setToFilter('')
        setPage(1)
    }

    return (
        <AppLayout>
            <div className="p-8">

                {/* Header */}
                <div className="flex items-center justify-between mb-6">
                    <div>
                        <h2 className="text-2xl font-semibold text-gray-900">Transactions</h2>
                        {data && (
                            <p className="text-sm text-gray-400 mt-1">
                                {data.totalCount.toLocaleString()} transactions
                            </p>
                        )}
                    </div>
                    <button
                        onClick={handleSync}
                        disabled={syncing || accounts.length === 0}
                        className="flex items-center gap-2 px-4 py-2 bg-gray-900 text-white text-sm rounded-lg hover:bg-gray-800 disabled:opacity-50 transition-colors"
                    >
                        <RefreshCw size={14} className={syncing ? 'animate-spin' : ''} />
                        {syncing ? 'Syncing...' : 'Sync now'}
                    </button>
                </div>

                {/* Filters */}
                <div className="bg-white rounded-xl border border-gray-200 p-4 mb-6">
                    <div className="flex items-center gap-2 mb-3">
                        <Filter size={14} className="text-gray-400" />
                        <span className="text-sm font-medium text-gray-700">Filters</span>
                        <button
                            onClick={resetFilters}
                            className="ml-auto text-xs text-gray-400 hover:text-gray-600"
                        >
                            Reset
                        </button>
                    </div>
                    <div className="grid grid-cols-5 gap-3">
                        <select
                            value={accountFilter}
                            onChange={e => { setAccountFilter(e.target.value); setPage(1) }}
                            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 text-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-900"
                        >
                            <option value="">All accounts</option>
                            {accounts.map(a => (
                                <option key={a.id} value={a.id}>{a.displayName}</option>
                            ))}
                        </select>

                        <select
                            value={categoryFilter}
                            onChange={e => { setCategoryFilter(e.target.value); setPage(1) }}
                            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 text-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-900"
                        >
                            <option value="">All categories</option>
                            {categories.map(c => (
                                <option key={c.id} value={c.id}>{c.name}</option>
                            ))}
                        </select>

                        <select
                            value={statusFilter}
                            onChange={e => { setStatusFilter(e.target.value); setPage(1) }}
                            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 text-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-900"
                        >
                            <option value="">All statuses</option>
                            <option value="Settled">Settled</option>
                            <option value="Pending">Pending</option>
                        </select>

                        <input
                            type="date"
                            value={fromFilter}
                            onChange={e => { setFromFilter(e.target.value); setPage(1) }}
                            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 text-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-900"
                            placeholder="From"
                        />

                        <input
                            type="date"
                            value={toFilter}
                            onChange={e => { setToFilter(e.target.value); setPage(1) }}
                            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 text-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-900"
                            placeholder="To"
                        />
                    </div>
                </div>

                {/* Transaction table */}
                <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
                    {loading ? (
                        <div className="flex items-center justify-center h-48">
                            <p className="text-sm text-gray-400">Loading transactions...</p>
                        </div>
                    ) : !data || data.items.length === 0 ? (
                        <div className="flex flex-col items-center justify-center h-48 gap-2">
                            <p className="text-sm text-gray-400">No transactions found.</p>
                            <p className="text-xs text-gray-300">
                                Connect a bank account and sync to see your transactions.
                            </p>
                        </div>
                    ) : (
                        <>
                            <table className="w-full">
                                <thead>
                                    <tr className="border-b border-gray-100">
                                        <th className="text-left text-xs font-medium text-gray-400 px-5 py-3">Date</th>
                                        <th className="text-left text-xs font-medium text-gray-400 px-5 py-3">Description</th>
                                        <th className="text-left text-xs font-medium text-gray-400 px-5 py-3">Category</th>
                                        <th className="text-left text-xs font-medium text-gray-400 px-5 py-3">Status</th>
                                        <th className="text-right text-xs font-medium text-gray-400 px-5 py-3">Amount</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-50">
                                    {data.items.map((tx) => (
                                        <tr key={tx.id} className="hover:bg-gray-50 transition-colors">
                                            <td className="px-5 py-3 text-sm text-gray-400 whitespace-nowrap">
                                                {formatDate(tx.transactionDate)}
                                            </td>
                                            <td className="px-5 py-3">
                                                <p className="text-sm font-medium text-gray-900">
                                                    {tx.merchantName || tx.description}
                                                </p>
                                                {tx.merchantName && (
                                                    <p className="text-xs text-gray-400">{tx.description}</p>
                                                )}
                                            </td>
                                            <td className="px-5 py-3">
                                                {assigningId === tx.id ? (
                                                    <span className="text-xs text-gray-400">Saving...</span>
                                                ) : (
                                                    <select
                                                        value={tx.userCategoryId ?? ''}
                                                        onChange={e => handleCategorise(tx.id, e.target.value)}
                                                        className="text-xs border border-gray-200 rounded-md px-2 py-1 focus:outline-none focus:ring-1 focus:ring-gray-900 max-w-36"
                                                        style={{
                                                            color: tx.userCategoryColour ?? '#6b7280',
                                                        }}
                                                    >
                                                        <option value="">Uncategorised</option>
                                                        {categories.map(c => (
                                                            <option key={c.id} value={c.id}>{c.name}</option>
                                                        ))}
                                                    </select>
                                                )}
                                            </td>
                                            <td className="px-5 py-3">
                                                <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${tx.status === 'Settled'
                                                        ? 'bg-green-50 text-green-700'
                                                        : 'bg-amber-50 text-amber-700'
                                                    }`}>
                                                    {tx.status}
                                                </span>
                                            </td>
                                            <td className="px-5 py-3 text-right">
                                                <span className={`text-sm font-medium ${tx.amount >= 0 ? 'text-green-600' : 'text-gray-900'
                                                    }`}>
                                                    {tx.amount >= 0 ? '+' : ''}
                                                    {formatCurrency(Math.abs(tx.amount), tx.currency)}
                                                </span>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>

                            {/* Pagination */}
                            <div className="flex items-center justify-between px-5 py-3 border-t border-gray-100">
                                <p className="text-xs text-gray-400">
                                    Page {data.page} of {data.totalPages} —{' '}
                                    {data.totalCount.toLocaleString()} transactions
                                </p>
                                <div className="flex gap-2">
                                    <button
                                        onClick={() => setPage(p => Math.max(1, p - 1))}
                                        disabled={!data.hasPreviousPage}
                                        className="px-3 py-1 text-xs border border-gray-200 rounded-md disabled:opacity-40 hover:bg-gray-50 transition-colors"
                                    >
                                        Previous
                                    </button>
                                    <button
                                        onClick={() => setPage(p => p + 1)}
                                        disabled={!data.hasNextPage}
                                        className="px-3 py-1 text-xs border border-gray-200 rounded-md disabled:opacity-40 hover:bg-gray-50 transition-colors"
                                    >
                                        Next
                                    </button>
                                </div>
                            </div>
                        </>
                    )}
                </div>

            </div>
        </AppLayout>
    )
}