import client from './client'
import type {
    AccountDto,
    BudgetSummaryDto,
    CategoryDto,
    CategorySpendingDto,
    LoginResult,
    MonthlySpendDto,
    MerchantSpendDto,
    PagedResult,
    TransactionDto,
} from '../types'

// Auth
export const login = (email: string, password: string) =>
    client.post<LoginResult>('/auth/login', { email, password })

export const register = (email: string, password: string, displayName: string) =>
    client.post<{ userId: string }>('/auth/register', { email, password, displayName })

// Accounts
export const getAccounts = () =>
    client.get<AccountDto[]>('/accounts')

// Transactions
export const getTransactions = (params: {
    page?: number
    pageSize?: number
    accountId?: string
    categoryId?: string
    status?: string
    from?: string
    to?: string
}) => client.get<PagedResult<TransactionDto>>('/transactions', { params })

export const syncTransactions = (bankConnectionId: string) =>
    client.post(`/transactions/sync/${bankConnectionId}`)

export const categoriseTransaction = (transactionId: string, categoryId: string) =>
    client.patch(`/transactions/${transactionId}/categorise`, { categoryId })

// Categories
export const getCategories = () =>
    client.get<CategoryDto[]>('/categories')

export const createCategory = (name: string, colourHex: string, icon: string) =>
    client.post<{ id: string }>('/categories', { name, colourHex, icon })

// Budgets
export const getBudgetSummary = (year: number, month: number) =>
    client.get<BudgetSummaryDto[]>(`/budgets/${year}/${month}`)

export const setBudget = (categoryId: string, amount: number, monthStart: string) =>
    client.post<{ id: string }>('/budgets', { categoryId, amount, monthStart })

// Spending analytics
export const getMonthlySpending = (year: number, month: number) =>
    client.get<CategorySpendingDto[]>(`/spending/${year}/${month}/categories`)

export const getSpendingTrend = (months = 6) =>
    client.get<MonthlySpendDto[]>('/spending/trend', { params: { months } })

export const getTopMerchants = (year: number, month: number, top = 5) =>
    client.get<MerchantSpendDto[]>(`/spending/${year}/${month}/top-merchants`, { params: { top } })

// Bank connections
export const initiateConnection = () =>
    client.get<{ authorisationUrl: string; state: string }>('/bank-connections/initiate')