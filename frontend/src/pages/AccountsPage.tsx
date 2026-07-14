import { useEffect, useState } from 'react'
import { Landmark, RefreshCw } from 'lucide-react'
import AppLayout from '@/components/layout/AppLayout'
import { getAccounts, initiateConnection } from '@/api/endpoints'
import type { AccountDto } from '@/types'
import { formatCurrency, formatDate } from '@/lib/utils'

export default function AccountsPage() {
    const [accounts, setAccounts] = useState<AccountDto[]>([])
    const [loading, setLoading] = useState(true)
    const [connecting, setConnecting] = useState(false)
    const [banner, setBanner] = useState<{ type: 'success' | 'error'; message: string } | null>(null)

    useEffect(() => {
        getAccounts()
            .then(res => setAccounts(res.data))
            .finally(() => setLoading(false))
    }, [])

    useEffect(() => {
        const params = new URLSearchParams(window.location.search)
        const error = params.get('error')
        const connected = params.get('connected')

        if (error) {
            setBanner({ type: 'error', message: 'Failed to connect your bank account. Please try again.' })
        } else if (connected) {
            setBanner({ type: 'success', message: 'Bank account connected successfully.' })
            getAccounts().then(res => setAccounts(res.data))
        }

        if (error || connected) {
            window.history.replaceState({}, '', window.location.pathname)
        }
    }, [])

    const handleConnectBank = async () => {
        setConnecting(true)
        try {
            const res = await initiateConnection()
            window.location.href = res.data.authorisationUrl
        } catch (err) {
            console.error('Failed to initiate connection:', err)
            setConnecting(false)
        }
    }

    const totalBalance = accounts.reduce(
        (sum, a) => sum + (a.balanceCurrent ?? 0), 0
    )

    return (
        <AppLayout>
            <div className="p-8 max-w-4xl">

                <div className="flex items-center justify-between mb-6">
                    <div>
                        <h2 className="text-2xl font-semibold text-gray-900">Accounts</h2>
                        <p className="text-sm text-gray-400 mt-1">
                            {accounts.length} connected account{accounts.length !== 1 ? 's' : ''}
                            {accounts.length > 0 && ` · Total balance ${formatCurrency(totalBalance)}`}
                        </p>
                    </div>
                    <button
                        onClick={handleConnectBank}
                        disabled={connecting}
                        className="flex items-center gap-2 px-4 py-2 bg-gray-900 text-white text-sm rounded-lg hover:bg-gray-800 disabled:opacity-50 transition-colors"
                    >
                        <Landmark size={14} />
                        {connecting ? 'Connecting...' : 'Connect bank'}
                    </button>
                </div>

                {banner && (
                    <div
                        className={`mb-6 px-4 py-3 rounded-lg text-sm ${
                            banner.type === 'success'
                                ? 'bg-green-50 text-green-700 border border-green-200'
                                : 'bg-red-50 text-red-700 border border-red-200'
                        }`}
                    >
                        {banner.message}
                    </div>
                )}

                {loading ? (
                    <p className="text-sm text-gray-400">Loading accounts...</p>
                ) : accounts.length === 0 ? (
                    <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
                        <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <Landmark size={20} className="text-gray-400" />
                        </div>
                        <p className="text-sm font-medium text-gray-900 mb-1">
                            No accounts connected
                        </p>
                        <p className="text-xs text-gray-400 mb-4">
                            Connect your UK bank account via Open Banking to start tracking your finances.
                        </p>
                        <button
                            onClick={handleConnectBank}
                            disabled={connecting}
                            className="px-4 py-2 bg-gray-900 text-white text-sm rounded-lg hover:bg-gray-800 disabled:opacity-50 transition-colors"
                        >
                            {connecting ? 'Connecting...' : 'Connect your first bank'}
                        </button>
                    </div>
                ) : (
                    <div className="space-y-4">
                        {accounts.map(account => (
                            <div
                                key={account.id}
                                className="bg-white rounded-xl border border-gray-200 p-5"
                            >
                                <div className="flex items-start justify-between mb-4">
                                    <div>
                                        <h3 className="text-base font-semibold text-gray-900">
                                            {account.displayName}
                                        </h3>
                                        <p className="text-xs text-gray-400 capitalize mt-0.5">
                                            {account.accountType.toLowerCase()} · {account.currency}
                                        </p>
                                    </div>
                                    <div className="text-right">
                                        <p className="text-xl font-semibold text-gray-900">
                                            {formatCurrency(account.balanceCurrent ?? 0, account.currency)}
                                        </p>
                                        <p className="text-xs text-gray-400 mt-0.5">
                                            {formatCurrency(account.balanceAvailable ?? 0, account.currency)} available
                                        </p>
                                    </div>
                                </div>

                                <div className="grid grid-cols-3 gap-4 pt-4 border-t border-gray-50">
                                    {account.sortCode && (
                                        <div>
                                            <p className="text-xs text-gray-400">Sort code</p>
                                            <p className="text-sm font-medium text-gray-700 mt-0.5">
                                                {account.sortCode}
                                            </p>
                                        </div>
                                    )}
                                    {account.accountNumber && (
                                        <div>
                                            <p className="text-xs text-gray-400">Account number</p>
                                            <p className="text-sm font-medium text-gray-700 mt-0.5">
                                                {account.accountNumber}
                                            </p>
                                        </div>
                                    )}
                                    {account.lastSyncedAt && (
                                        <div>
                                            <p className="text-xs text-gray-400">Last synced</p>
                                            <p className="text-sm font-medium text-gray-700 mt-0.5">
                                                {formatDate(account.lastSyncedAt)}
                                            </p>
                                        </div>
                                    )}
                                </div>
                            </div>
                        ))}

                        <button
                            onClick={handleConnectBank}
                            disabled={connecting}
                            className="w-full py-3 border-2 border-dashed border-gray-200 rounded-xl text-sm text-gray-400 hover:border-gray-300 hover:text-gray-500 transition-colors disabled:opacity-50"
                        >
                            {connecting ? 'Connecting...' : '+ Connect another bank'}
                        </button>
                    </div>
                )}
            </div>
        </AppLayout>
    )
}