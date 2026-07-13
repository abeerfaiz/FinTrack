import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { register } from '@/api/endpoints'

export default function RegisterPage() {
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [displayName, setDisplayName] = useState('')
    const [error, setError] = useState('')
    const [loading, setLoading] = useState(false)
    const navigate = useNavigate()

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setError('')
        setLoading(true)

        try {
            await register(email, password, displayName)
            navigate('/login')
        } catch {
            setError('Registration failed. Email may already be taken.')
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className= "min-h-screen flex items-center justify-center bg-gray-50" >
        <div className="w-full max-w-md bg-white rounded-xl border border-gray-200 p-8" >
            <div className="mb-8" >
                <h1 className="text-2xl font-semibold text-gray-900" > Create account </h1>
                    < p className = "text-sm text-gray-500 mt-1" > Start tracking your finances </p>
                        </div>

                        < form onSubmit = { handleSubmit } className = "space-y-4" >
                            <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1" >
                                Display name
                                    </label>
                                    < input
    type = "text"
    value = { displayName }
    onChange = {(e) => setDisplayName(e.target.value)
}
className = "w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900 focus:border-transparent"
placeholder = "Abeer"
required
    />
    </div>

    < div >
    <label className="block text-sm font-medium text-gray-700 mb-1" >
        Email
        </label>
        < input
type = "email"
value = { email }
onChange = {(e) => setEmail(e.target.value)}
className = "w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900 focus:border-transparent"
placeholder = "you@example.com"
required
    />
    </div>

    < div >
    <label className="block text-sm font-medium text-gray-700 mb-1" >
        Password
        </label>
        < input
type = "password"
value = { password }
onChange = {(e) => setPassword(e.target.value)}
className = "w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-gray-900 focus:border-transparent"
placeholder = "At least 8 characters"
required
minLength = { 8}
    />
    </div>

{
    error && (
        <p className="text-sm text-red-600" > { error } </p>
          )
}

<button
            type="submit"
disabled = { loading }
className = "w-full bg-gray-900 text-white py-2 px-4 rounded-lg text-sm font-medium hover:bg-gray-800 disabled:opacity-50 transition-colors"
    >
    { loading? 'Creating account...': 'Create account' }
    </button>
    </form>

    < p className = "text-sm text-gray-500 mt-6 text-center" >
        Already have an account ? { ' '}
            < Link to = "/login" className = "text-gray-900 font-medium hover:underline" >
                Sign in
                </Link>
                </p>
                </div>
                </div>
  )
}