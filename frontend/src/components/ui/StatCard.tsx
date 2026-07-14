interface Props {
    label: string
    value: string
    sub?: string
    colour?: string
}

export default function StatCard({ label, value, sub, colour }: Props) {
    return (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
            <p className="text-xs text-gray-400 font-medium uppercase tracking-wide">
                {label}
            </p>
            <p className="text-2xl font-semibold text-gray-900 mt-1">{value}</p>
            {sub && (
                <p className="text-xs mt-1" style={{ color: colour ?? '#6b7280' }}>
                    {sub}
                </p>
            )}
        </div>
    )
}