export type Product = { id: number; name: string; description: string | null; sku: string; category: string; price: number; stockQuantity: number; isActive: boolean; createdAtUtc: string; updatedAtUtc: string | null }
export type ProductInput = Omit<Product, 'id' | 'createdAtUtc' | 'updatedAtUtc'>
export type OrderItem = { productId: number; productName: string; quantity: number; unitPrice: number; lineTotal: number }
export type Order = { id: number; createdAtUtc: string; status: string; totalAmount: number; items: OrderItem[] }
export type User = { userId: string; businessName: string; email: string }

const base = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? ''
export class ApiError extends Error {
  status: number
  constructor(status: number, message: string) { super(message); this.status = status }
}
export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  if (init.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json')
  const response = await fetch(`${base}${path}`, {
    ...init, credentials: 'include',
    headers,
  })
  if (!response.ok) {
    if (response.status === 401) window.dispatchEvent(new Event('stockpilot:unauthorized'))
    let message = response.status === 401 ? 'Your session has expired. Please sign in again.' : `Request failed (${response.status}).`
    try { const body = await response.json() as { message?: string; title?: string; errors?: Record<string, string[]> }; message = body.message ?? body.title ?? (Object.values(body.errors ?? {}).flat().join(' ') || message) } catch { /* Keep the readable status message. */ }
    throw new ApiError(response.status, message)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}
export const money = (amount: number) => new Intl.NumberFormat('en-NG', { style: 'currency', currency: 'NGN', maximumFractionDigits: 2 }).format(amount)
export const date = (value: string) => new Intl.DateTimeFormat('en-NG', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))
