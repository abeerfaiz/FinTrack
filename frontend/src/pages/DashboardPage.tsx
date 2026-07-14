import { useEffect, useState } from 'react'
import {
    LineChart, Line, XAxis, YAxis, CartesianGrid,
    Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend
} from 'recharts'
import AppLayout from '@/components/layout/AppLayout'
import StatCard from '@/components/ui/StatCard'
import {
    getAccounts,
    getSpendingTrend,
    getMonthlySpending,
    getTopMerchants,
} from '@/api/endpoints'
import type {
    AccountDto,
    MonthlySpendDto,
    CategorySpendingDto,
    MerchantSpendDto,
} from '@/types'
import { formatCurrency, currentYearMonth } from '@/lib/utils'

export default function DashboardPage() {
    const [accounts, setAccounts] = useState<AccountDto[]>([])
    const [trend, setTrend] = useState<MonthlySpendDto[]>([])
    const [categorySpend, setCategorySpend] = useState<CategorySpendingDto[]>([])
    const [topMerchants, setTopMerchants] = useState<MerchantSpendDto[]>([])
    const [loading, setLoading] = useState(true)

    const { year, month } = currentYearMonth()

    useEffect(() => {
        async function load() {
            try {
                const [accountsRes, trendRes, categoryRes, merchantRes] = await Promise.all([
                    getAccounts(),
                    getSpendingTrend(6),
                    getMonthlySpending(year, month),
                    getTopMerchants(year, month, 5),
                ])
                setAccounts(accountsRes.data)
                setTrend(trendRes.data)
                setCategorySpend(categoryRes.data)
                setTopMerchants(merchantRes.data)
            } catch (err) {
                console.error('Dashboard load error:', err)
            } finally {
                setLoading(false)
            }
        }
        load()
    }, [year, month])

    const totalBalance = accounts.reduce(
        (sum, a) => sum + (a.balanceCurrent ?? 0), 0
    )

    const totalAvailable = accounts.reduce(
        (sum, a) => sum + (a.balanceAvailable ?? 0), 0
    )

    const thisMonthSpend = trend.find(
        t => t.year === year && t.month === month
    )?.totalSpend ?? 0

    if (loading) {
        return (
            <AppLayout>
                <div className="flex items-center justify-center h-64">
                    <p className="text-gray-400 text-sm">Loading dashboard...</p>
                </div>
            </AppLayout>
        )
    }

    return (
        <AppLayout>
            <div className="p-8 max-w-6xl">

                {/* Header */}
                <div className="mb-8">
                    <h2 className="text-2xl font-semibold text-gray-900">Dashboard</h2>
                    <p className="text-sm text-gray-400 mt-1">
                        {new Date().toLocaleDateString('en-GB', {
                            weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
                        })}
                    </p>
                </div>

                {/* Summary cards */}
                <div className="grid grid-cols-3 gap-4 mb-8">
                    <StatCard
                        label="Total balance"
                        value={formatCurrency(totalBalance)}
                        sub={`${accounts.length} connected account${accounts.length !== 1 ? 's' : ''}`}
                    />
                    <StatCard
                        label="Available"
                        value={formatCurrency(totalAvailable)}
                        sub="Across all accounts"
                    />
                    <StatCard
                        label="Spent this month"
                        value={formatCurrency(thisMonthSpend)}
                        sub={new Date(year, month - 1).toLocaleDateString('en-GB', { month: 'long', year: 'numeric' })}
                        colour="#ef4444"
                    />
                </div>

                {/* Charts row */}
                <div className="grid grid-cols-2 gap-6 mb-8">

                    {/* Spending trend */}
                    <div className="bg-white rounded-xl border border-gray-200 p-5">
                        <h3 className="text-sm font-medium text-gray-900 mb-4">
                            Spending trend
                        </h3>
                        {trend.length === 0 ? (
                            <p className="text-sm text-gray-400">No spending data yet.</p>
                        ) : (
                            <ResponsiveContainer width="100%" height={200}>
                                <LineChart data={trend}>
                                    <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                                    <XAxis
                                        dataKey="monthLabel"
                                        tick={{ fontSize: 11, fill: '#9ca3af' }}
                                        axisLine={false}
                                        tickLine={false}
                                    />
                                    <YAxis
                                        tick={{ fontSize: 11, fill: '#9ca3af' }}
                                        axisLine={false}
                                        tickLine={false}
                                        tickFormatter={(v) => `£${v}`}
                                    />
                                    <Tooltip
                                        formatter={(value: number) => [formatCurrency(value), 'Spend']}
                                        contentStyle={{
                                            fontSize: 12,
                                            border: '1px solid #e5e7eb',
                                            borderRadius: 8,
                                        }}
                                    />
                                    <Line
                                        type="monotone"
                                        dataKey="totalSpend"
                                        stroke="#111827"
                                        strokeWidth={2}
                                        dot={{ r: 3, fill: '#111827' }}
                                        activeDot={{ r: 5 }}
                                    />
                                </LineChart>
                            </ResponsiveContainer>
                        )}
                    </div>

                    {/* Category breakdown */}
                    <div className="bg-white rounded-xl border border-gray-200 p-5">
                        <h3 className="text-sm font-medium text-gray-900 mb-4">
                            Spending by category
                        </h3>
                        {categorySpend.length === 0 ? (
                            <p className="text-sm text-gray-400">
                                No categorised transactions this month.
                            </p>
                        ) : (
                            <ResponsiveContainer width="100%" height={200}>
                                <PieChart>
                                    <Pie
                                        data={categorySpend}
                                        dataKey="totalSpend"
                                        nameKey="categoryName"
                                        cx="50%"
                                        cy="50%"
                                        outerRadius={70}
                                        strokeWidth={0}
                                    >
                                        {categorySpend.map((entry, index) => (
                                            <Cell
                                                key={`cell-${index}`}
                                                fill={entry.categoryColour}
                                            />
                                        ))}
                                    </Pie>
                                    <Legend
                                        iconSize={8}
                                        wrapperStyle={{ fontSize: 11 }}
                                        formatter={(value, entry: any) =>
                                            `${value} ${formatCurrency(entry.payload.totalSpend)}`
                                        }
                                    />
                                    <Tooltip
                                        formatter={(value: number) => [formatCurrency(value), 'Spend']}
                                        contentStyle={{
                                            fontSize: 12,
                                            border: '1px solid #e5e7eb',
                                            borderRadius: 8,
                                        }}
                                    />
                                </PieChart>
                            </ResponsiveContainer>
                        )}
                    </div>
                </div>

                {/* Bottom row */}
                <div className="grid grid-cols-2 gap-6">

                    {/* Top merchants */}
                    <div className="bg-white rounded-xl border border-gray-200 p-5">
                        <h3 className="text-sm font-medium text-gray-900 mb-4">
                            Top merchants this month
                        </h3>
                        {topMerchants.length === 0 ? (
                            <p className="text-sm text-gray-400">No transactions this month.</p>
                        ) : (
                            <div className="space-y-3">
                                {topMerchants.map((merchant, i) => (
                                    <div key={i} className="flex items-center justify-between">
                                        <div className="flex items-center gap-3">
                                            <div className="w-7 h-7 rounded-full bg-gray-100 flex items-center justify-center text-xs font-medium text-gray-500">
                                                {i + 1}
                                            </div>
                                            <span className="text-sm text-gray-700 truncate max-w-32">
                                                {merchant.merchantName}
                                            </span>
                                        </div>
                                        <div className="text-right">
                                            <p className="text-sm font-medium text-gray-900">
                                                {formatCurrency(merchant.totalSpend)}
                                            </p>
                                            <p className="text-xs text-gray-400">
                                                {merchant.transactionCount} tx
                                            </p>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Connected accounts */}
                    <div className="bg-white rounded-xl border border-gray-200 p-5">
                        <h3 className="text-sm font-medium text-gray-900 mb-4">
                            Connected accounts
                        </h3>
                        {accounts.length === 0 ? (
                            <p className="text-sm text-gray-400">No accounts connected yet.</p>
                        ) : (
                            <div className="space-y-3">
                                {accounts.map((account) => (
                                    <div
                                        key={account.id}
                                        className="flex items-center justify-between py-2 border-b border-gray-50 last:border-0"
                                    >
                                        <div>
                                            <p className="text-sm font-medium text-gray-900">
                                                {account.displayName}
                                            </p>
                                            <p className="text-xs text-gray-400 capitalize">
                                                {account.accountType.toLowerCase()}
                                            </p>
                                        </div>
                                        <div className="text-right">
                                            <p className="text-sm font-medium text-gray-900">
                                                {formatCurrency(account.balanceCurrent ?? 0, account.currency)}
                                            </p>
                                            <p className="text-xs text-gray-400">
                                                {formatCurrency(account.balanceAvailable ?? 0, account.currency)} available
                                            </p>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </AppLayout>
    )
}