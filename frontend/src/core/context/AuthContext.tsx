import { createContext, useCallback, useContext, useState } from 'react'
import { authService, type AuthUser } from '../services/authService'

interface AuthContextValue {
  user: AuthUser | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => authService.getUser())

  const login = useCallback(async (email: string, password: string) => {
    const response = await authService.login(email, password)
    setUser({
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
      role: response.role,
      organizationId: response.organizationId,
      organizationName: response.organizationName,
    })
  }, [])

  const logout = useCallback(() => {
    authService.logout()
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
