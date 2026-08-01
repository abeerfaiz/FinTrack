import { useEffect, useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import AppLayout from '@/components/layout/AppLayout'
import {
    getCategories,
    createCategory,
    getCategoryRules,
    createCategoryRule,
    deleteCategoryRule,
} from '@/api/endpoints'
import type { CategoryDto, CategoryRuleDto } from '@/types'

const ICONS = ['shopping-cart', 'car', 'utensils', 'tv', 'file-text', 'bag', 'heart', 'activity', 'plane', 'more-horizontal', 'star', 'home', 'coffee', 'music', 'gift']

export default function CategoriesPage() {
    const [categories, setCategories] = useState<CategoryDto[]>([])
    const [loading, setLoading] = useState(true)
    const [showForm, setShowForm] = useState(false)
    const [name, setName] = useState('')
    const [colour, setColour] = useState('#8B5CF6')
    const [icon, setIcon] = useState('star')
    const [saving, setSaving] = useState(false)
    const [error, setError] = useState('')

    const [rules, setRules] = useState<CategoryRuleDto[]>([])
    const [rulesLoading, setRulesLoading] = useState(true)
    const [ruleKeyword, setRuleKeyword] = useState('')
    const [ruleCategoryId, setRuleCategoryId] = useState('')
    const [rulePriority, setRulePriority] = useState(10)
    const [ruleSaving, setRuleSaving] = useState(false)
    const [ruleError, setRuleError] = useState('')

    const load = async () => {
        try {
            const res = await getCategories()
            setCategories(res.data)
        } finally {
            setLoading(false)
        }
    }

    const loadRules = async () => {
        try {
            const res = await getCategoryRules()
            setRules(res.data)
        } finally {
            setRulesLoading(false)
        }
    }

    useEffect(() => { load() }, [])
    useEffect(() => { loadRules() }, [])

    useEffect(() => {
        if (!ruleCategoryId && categories.length > 0) {
            setRuleCategoryId(categories[0].id)
        }
    }, [categories, ruleCategoryId])

    const handleCreate = async (e: React.FormEvent) => {
        e.preventDefault()
        setError('')
        setSaving(true)
        try {
            await createCategory(name, colour, icon)
            setName('')
            setColour('#8B5CF6')
            setIcon('star')
            setShowForm(false)
            await load()
        } catch {
            setError('Failed to create category.')
        } finally {
            setSaving(false)
        }
    }

    const handleCreateRule = async (e: React.FormEvent) => {
        e.preventDefault()
        setRuleError('')
        setRuleSaving(true)
        try {
            await createCategoryRule(ruleCategoryId, ruleKeyword, rulePriority)
            setRuleKeyword('')
            setRulePriority(10)
            await loadRules()
        } catch {
            setRuleError('Failed to create rule.')
        } finally {
            setRuleSaving(false)
        }
    }

    const handleDeleteRule = async (ruleId: string) => {
        try {
            await deleteCategoryRule(ruleId)
            await loadRules()
        } catch {
            setRuleError('Failed to delete rule.')
        }
    }

    const systemCategories = categories.filter(c => c.isSystem)
    const userCategories = categories.filter(c => !c.isSystem)

    return (
        <AppLayout>
            <div className="p-8 max-w-4xl">

                <div className="flex items-center justify-between mb-6">
                    <div>
                        <h2 className="text-2xl font-semibold text-gray-900">Categories</h2>
                        <p className="text-sm text-gray-400 mt-1">
                            Organise your transactions your way
                        </p>
                    </div>
                    <button
                        onClick={() => setShowForm(!showForm)}
                        className="flex items-center gap-2 px-4 py-2 bg-gray-900 text-white text-sm rounded-lg hover:bg-gray-800 transition-colors"
                    >
                        <Plus size={14} />
                        New category
                    </button>
                </div>

                {/* Create form */}
                {showForm && (
                    <div className="bg-white rounded-xl border border-gray-200 p-5 mb-6">
                        <h3 className="text-sm font-medium text-gray-900 mb-4">
                            Create custom category
                        </h3>
                        <form onSubmit={handleCreate} className="flex items-end gap-4">
                            <div className="flex-1">
                                <label className="block text-xs text-gray-500 mb-1">Name</label>
                                <input
                                    value={name}
                                    onChange={e => setName(e.target.value)}
                                    className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                                    placeholder="e.g. Date night"
                                    required
                                />
                            </div>
                            <div>
                                <label className="block text-xs text-gray-500 mb-1">Colour</label>
                                <input
                                    type="color"
                                    value={colour}
                                    onChange={e => setColour(e.target.value)}
                                    className="h-9 w-16 border border-gray-200 rounded-lg cursor-pointer"
                                />
                            </div>
                            <div className="flex-1">
                                <label className="block text-xs text-gray-500 mb-1">Icon</label>
                                <select
                                    value={icon}
                                    onChange={e => setIcon(e.target.value)}
                                    className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                                >
                                    {ICONS.map(i => (
                                        <option key={i} value={i}>{i}</option>
                                    ))}
                                </select>
                            </div>
                            {error && <p className="text-xs text-red-500">{error}</p>}
                            <button
                                type="submit"
                                disabled={saving}
                                className="px-4 py-2 bg-gray-900 text-white text-sm rounded-lg hover:bg-gray-800 disabled:opacity-50 transition-colors"
                            >
                                {saving ? 'Creating...' : 'Create'}
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
                    <p className="text-sm text-gray-400">Loading categories...</p>
                ) : (
                    <>
                        {/* User categories */}
                        {userCategories.length > 0 && (
                            <div className="mb-6">
                                <h3 className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-3">
                                    Your categories
                                </h3>
                                <div className="bg-white rounded-xl border border-gray-200 divide-y divide-gray-50">
                                    {userCategories.map(cat => (
                                        <div key={cat.id} className="flex items-center justify-between px-5 py-3">
                                            <div className="flex items-center gap-3">
                                                <div
                                                    className="w-8 h-8 rounded-full flex items-center justify-center text-white text-xs font-medium"
                                                    style={{ backgroundColor: cat.colourHex }}
                                                >
                                                    {cat.name.charAt(0).toUpperCase()}
                                                </div>
                                                <span className="text-sm font-medium text-gray-900">
                                                    {cat.name}
                                                </span>
                                            </div>
                                            <button className="text-gray-300 hover:text-red-400 transition-colors">
                                                <Trash2 size={14} />
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* System categories */}
                        <div>
                            <h3 className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-3">
                                System categories
                            </h3>
                            <div className="grid grid-cols-2 gap-3">
                                {systemCategories.map(cat => (
                                    <div
                                        key={cat.id}
                                        className="bg-white rounded-xl border border-gray-200 px-4 py-3 flex items-center gap-3"
                                    >
                                        <div
                                            className="w-8 h-8 rounded-full flex items-center justify-center text-white text-xs font-medium flex-shrink-0"
                                            style={{ backgroundColor: cat.colourHex }}
                                        >
                                            {cat.name.charAt(0).toUpperCase()}
                                        </div>
                                        <div>
                                            <p className="text-sm font-medium text-gray-900">{cat.name}</p>
                                            <p className="text-xs text-gray-400">System</p>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        {/* Category rules */}
                        <div className="mt-8">
                            <h3 className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-3">
                                Category rules
                            </h3>
                            <p className="text-sm text-gray-400 mb-4">
                                Automatically categorise transactions whose merchant name or
                                description contains a keyword. Lower priority number wins
                                when multiple rules match.
                            </p>

                            <div className="bg-white rounded-xl border border-gray-200 p-5 mb-6">
                                <form onSubmit={handleCreateRule} className="flex items-end gap-4">
                                    <div className="flex-1">
                                        <label className="block text-xs text-gray-500 mb-1">Keyword</label>
                                        <input
                                            value={ruleKeyword}
                                            onChange={e => setRuleKeyword(e.target.value)}
                                            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                                            placeholder="e.g. NETFLIX"
                                            required
                                        />
                                    </div>
                                    <div className="flex-1">
                                        <label className="block text-xs text-gray-500 mb-1">Category</label>
                                        <select
                                            value={ruleCategoryId}
                                            onChange={e => setRuleCategoryId(e.target.value)}
                                            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                                            required
                                        >
                                            {categories.map(cat => (
                                                <option key={cat.id} value={cat.id}>{cat.name}</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="w-24">
                                        <label className="block text-xs text-gray-500 mb-1">Priority</label>
                                        <input
                                            type="number"
                                            min={0}
                                            value={rulePriority}
                                            onChange={e => setRulePriority(Number(e.target.value))}
                                            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                                            required
                                        />
                                    </div>
                                    {ruleError && <p className="text-xs text-red-500">{ruleError}</p>}
                                    <button
                                        type="submit"
                                        disabled={ruleSaving || !ruleCategoryId}
                                        className="px-4 py-2 bg-gray-900 text-white text-sm rounded-lg hover:bg-gray-800 disabled:opacity-50 transition-colors"
                                    >
                                        {ruleSaving ? 'Adding...' : 'Add rule'}
                                    </button>
                                </form>
                            </div>

                            {rulesLoading ? (
                                <p className="text-sm text-gray-400">Loading rules...</p>
                            ) : rules.length === 0 ? (
                                <p className="text-sm text-gray-400">No category rules yet.</p>
                            ) : (
                                <div className="bg-white rounded-xl border border-gray-200 divide-y divide-gray-50">
                                    {rules.map(rule => (
                                        <div key={rule.id} className="flex items-center justify-between px-5 py-3">
                                            <div className="flex items-center gap-3">
                                                <span className="text-sm font-medium text-gray-900">
                                                    {rule.keyword}
                                                </span>
                                                <span className="text-xs text-gray-400">→ {rule.categoryName}</span>
                                                <span className="text-xs text-gray-300">priority {rule.priority}</span>
                                            </div>
                                            <button
                                                onClick={() => handleDeleteRule(rule.id)}
                                                className="text-gray-300 hover:text-red-400 transition-colors"
                                            >
                                                <Trash2 size={14} />
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </>
                )}
            </div>
        </AppLayout>
    )
}