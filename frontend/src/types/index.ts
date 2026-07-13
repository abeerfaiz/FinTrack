export interface AccountDto {
    id: string
    externalAccountId: string
    displayName: string
    accountType: string
    currency: string
    sortCode?: string
    accountNumber?: string
    iban?: string
    balanceCurrent?: number
    balanceAvailable?: number
    balanceOverdraft?: number
    balanceUpdatedAt?: string
    lastSyncedAt?: string
}

export interface TransactionDto {
    id: string
    accountId: string
    externalTxId: string
    status: string
    transactionType: string
    transactionCategory: string
    transactionClassification: string[]
    description: string
    merchantName?: string
    amount: number
    currency: string
    transactionDate: string
    runningBalance?: number
    userCategoryId?: string
    userCategoryName?: string
    userCategoryColour?: string
    isManuallyCategorised: boolean
}

export interface PagedResult<T> {
    items: T[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
    hasNextPage: boolean
    hasPreviousPage: boolean
}

export interface CategoryDto {
    id: string
    name: string
    colourHex: string
    icon: string
    isSystem: boolean
}

export interface BudgetSummaryDto {
    budgetId: string
    categoryId: string
    categoryName: string
    categoryColour: string
    budgetAmount: number
    actualSpend: number
    remaining: number
    percentageUsed: number
}

export interface CategorySpendingDto {
    categoryId: string
    categoryName: string
    categoryColour: string
    totalSpend: number
    transactionCount: number
}

export interface MonthlySpendDto {
    year: number
    month: number
    monthLabel: string
    totalSpend: number
}

export interface MerchantSpendDto {
    merchantName: string
    totalSpend: number
    transactionCount: number
}

export interface LoginResult {
    userId: string
    email: string
    accessToken: string
    refreshToken: string
}