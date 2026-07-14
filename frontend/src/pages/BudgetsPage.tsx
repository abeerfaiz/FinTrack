import { useEffect, useState } from 'react'
import { Plus } from 'lucide-react'
import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid,
    Tooltip, ResponsiveContainer, Cell
} from 'recharts'
import AppLayout from '@/components/layout/AppLayout'
import { getBudgetSummary, getCategories, setBudget } from '@/api/endpoints'
import type { BudgetSummaryDto, CategoryDto } from '@/types'
import { formatCurrency, currentYearMonth } from '@/lib/utils'

export default function BudgetsPage() {
    const [summaries, setSummaries] = useState<BudgetSummaryDto[]>([])
    const [categories, setCategories] = useState<CategoryDto[]>([])
    const [loading, setLoading] = useState(true)
    const [showForm, setShowForm] = useState(false)
    const [categoryId, setCategoryId] = useState('')
    const [amount, setAmount] = useState('')
    const [saving, setSaving] = useState(false)

    const { year, month } = currentYearMonth()

    const load = async () => {
        try {
            const [summaryRes, catRes] = await Promise.all([
                getBudgetSummary(year, month),
                getCategories(),
            ])
            setSummaries(summaryRes.data)
            setCategories(catRes.data)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => { load() }, [])

    const handleSetBudget = async (e: React.FormEvent) => {
        e.preventDefault()
        setSaving(true)
        try {
            const monthStart = `${year}-${String(month).padStart(2, '0')}-01`
            await setBudget(categoryId, parseFloat(amount), monthStart)
            setShowForm(false)
            setCategoryId('')
            setAmount('')
            await load()
        } catch (err) {
            console.error('Failed to set budget:', err)
        } finally {
            setSaving(false)
        }
    }

    const chartData = summaries.map(s => ({
        name: s.categoryName,
        budget: s.budgetAmount,
        spent: s.actualSpend,
        colour: s.categoryColour,
        over: s.actualSpend > s.budgetAmount,
    }))

    const monthLabel = new Date(year, month - 1).toLocaleDateString('en-GB', {
        month: 'long',
        year: 'numeric',
    })

    return (
        <AppLayout>
            <div className="p-8 max-w-4xl">

                <div className="flex items-center justify-between mb-6">
                    <div>
                        <h2 className="text-2xl font-semibold text-gray-900">Budgets</h2>
                        <p className="text-sm text-gray-400 mt-1">{monthLabel}</p>
                    </div>
                    <button
                        onClick={() => setShowForm(!showForm)}
                        className="flex items-center gap-2 px-4 py-2 bg-gray-900 text-white text-sm rounded-lg hover:bg-gray-800 transition-colors"
                    >
                        <Plus size={14} />
                        Set budget
                    </button>
                </div>

                {/* Set budget form */}
                {showForm && (
                    <div className="bg-white rounded-xl border border-gray-200 p-5 mb-6">
                        <h3 className="text-sm font-medium text-gray-900 mb-4">
                            Set monthly budget
                        </h3>
                        <form onSubmit={handleSetBudget} className="flex items-end gap-4">
                            <div className="flex-1">
                                <label className="block text-xs text-gray-500 mb-1">Category</label>
                                <select
                                    value={categoryId}
                                    onChange={e => setCategoryId(e.target.value)}
                                    className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                                    required
                                >
                                    <option value="">Select category</option>
                                    {categories.map(c => (
                                        <option key={c.id} value={c.id}>{c.name}</option>
                                    ))}
                                </select>
                            </div>
                            <div className="flex-1">
                                <label className="block text-xs text-gray-500 mb-1">
                                    Monthly budget (£)
                                </label>
                                <input
                                    type="number"
                                    value={amount}
                                    onChange={e => setAmount(e.target.value)}
                                    className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                                    placeholder="300"
                                    min="1"
                                    step="0.01"
                                    required
                                />
                            </div>
                            <button
                                type="submit"
                                disabled={saving}
                                className="px-4 py-2 bg-gray-900 text-white text-sm rounded-lg hover:bg-gray-800 disabled:opacity-50 transition-colors"
                            >
                                {saving ? 'Saving...' : 'Save'}
                            </button>
                            <button
                                type="button"
                                onClick={() => setShowForm(false)}
                                className="px-4 py-2 border border-gray-200 text-sm rounded-lg hover:bg-gray-50 transition-colors"
                            >
                                Cancel
                            </button>
                        </form>
                    </div>
                )}

                {loading ? (
                    <p className="text-sm text-gray-400">Loading budgets...</p>
                ) : summaries.length === 0 ? (
                    <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
                        <p className="text-sm text-gray-400">No budgets set for {monthLabel}.</p>
                        <p className="text-xs text-gray-300 mt-1">
                            Click "Set budget" to start tracking your spending against targets.
                        </p>
                    </div>
                ) : (
                    <>
                        {/* Bar chart */}
                        <div className="bg-white rounded-xl border border-gray-200 p-5 mb-6">
                            <h3 className="text-sm font-medium text-gray-900 mb-4">
                                Budget vs actual
                            </h3>
                            <ResponsiveContainer width="100%" height={220}>
                                <BarChart data={chartData} barGap={4}>
                                    <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                                    <XAxis
                                        dataKey="name"
                                        tick={{ fontSize: 11, fill: '#9ca3af' }}
                                        axisLine={false}
                                        tickLine={false}
                                    />
                                    <YAxis
                                        tick={{ fontSize: 11, fill: '#9ca3af' }}
                                        axisLine={false}
                                        tickLine={false}
                                        tickFormatter={v => `£${v}`}
                                    />
                                    <Tooltip
                                        formatter={(value: number, name: string) => [
                                            formatCurrency(value),
                                            name === 'budget' ? 'Budget' : 'Spent',
                                        ]}
                                        contentStyle={{
                                            fontSize: 12,
                                            border: '1px solid #e5e7eb',
                                            borderRadius: 8,
                                        }}
                                    />
                                    <Bar dataKey="budget" fill="#e5e7eb" radius={[4, 4, 0, 0]} />
                                    <Bar dataKey="spent" radius={[4, 4, 0, 0]}>
                                        {chartData.map((entry, index) => (
                                            <Cell
                                                key={`cell-${index}`}
                                                fill={entry.over ? '#ef4444' : entry.colour}
                                            />
                                        ))}
                                    </Bar>
                                </BarChart>
                            </ResponsiveContainer>
                        </div>

                        {/* Budget list */}
                        <div className="bg-white rounded-xl border border-gray-200 divide-y divide-gray-50">
                            {summaries.map(s => (
                                <div key={s.budgetId} className="px-5 py-4">
                                    <div className="flex items-center justify-between mb-2">
                                        <div className="flex items-center gap-2">
                                            <div
                                                className="w-3 h-3 rounded-full"
                                                style={{ backgroundColor: s.categoryColour }}
                                            />
                                            <span className="text-sm font-medium text-gray-900">
                                                {s.categoryName}
                                            </span>
                                        </div>
                                        <div className="text-right">
                                            <span className={`text-sm font-medium ${s.actualSpend > s.budgetAmount
                                                    ? 'text-red-600'
                                                    : 'text-gray-900'
                                                }`}>
                                                {formatCurrency(s.actualSpend)}
                                            </span>
                                            <span className="text-sm text-gray-400">
                                                {' '}/ {formatCurrency(s.budgetAmount)}
                                            </span>
                                        </div>
                                    </div>

                                    {/* Progress bar */}
                                    <div className="w-full bg-gray-100 rounded-full h-1.5">
                                        <div
                                            className="h-1.5 rounded-full transition-all"
                                            style={{
                                                width: `${Math.min(s.percentageUsed, 100)}%`,
                                                backgroundColor: s.actualSpend > s.budgetAmount
                                                    ? '#ef4444'
                                                    : s.categoryColour,
                                            }}
                                        />
                                    </div>

                                    <div className="flex justify-between mt-1">
                                        <span className="text-xs text-gray-400">
                                            {s.percentageUsed.toFixed(0)}% used
                                        </span>
                                        <span className={`text-xs ${s.remaining < 0 ? 'text-red-500' : 'text-gray-400'
                                            }`}>
                                            {s.remaining < 0
                                                ? `${formatCurrency(Math.abs(s.remaining))} over`
                                                : `${formatCurrency(s.remaining)} remaining`
                                            }
                                        </span>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </>
                )}
            </div>
        </AppLayout>
    )
}