import { useState, useEffect } from 'react'

export function useAuth() {
    const [isAuthenticated, setIsAuthenticated] = useState(false)
    const [isLoading, setIsLoading] = useState(true)

    useEffect(() => {
        const token = localStorage.getItem('accessToken')
        setIsAuthenticated(!!token)
        setIsLoading(false)
    }, [])

    const login = (accessToken: string, refreshToken: string) => {
        localStorage.setItem('accessToken', accessToken)
        localStorage.setItem('refreshToken', refreshToken)
        setIsAuthenticated(true)
    }

    const logout = () => {
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
        setIsAuthenticated(false)
    }

    return { isAuthenticated, isLoading, login, logout }
}