import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Loader2, Lock, QrCode, AlertTriangle } from 'lucide-react'
import { useAuth } from '../../core/context/AuthContext'
import { beehiveService, type BeehiveScanInfo } from '../../core/services/beehiveService'

type ScanState = 'loading' | 'not-found' | 'no-access' | 'error'

export default function ScanPage() {
  const { uniqueId } = useParams<{ uniqueId: string }>()
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const [state, setState] = useState<ScanState>('loading')
  const [beehiveName, setBeehiveName] = useState<string>('')

  useEffect(() => {
    if (!uniqueId) {
      setState('not-found')
      return
    }

    let cancelled = false

    async function run() {
      // Step 1: public lookup — resolve uniqueId → { id, name }
      let info: BeehiveScanInfo | null
      try {
        info = await beehiveService.scanLookup(uniqueId!)
      } catch {
        if (!cancelled) setState('error')
        return
      }

      if (cancelled) return

      if (!info) {
        setState('not-found')
        return
      }

      setBeehiveName(info.name)

      // Step 2: if not authenticated, redirect to login and come back
      if (!isAuthenticated) {
        navigate(`/login?returnUrl=${encodeURIComponent(`/scan/${uniqueId}`)}`, { replace: true })
        return
      }

      // Step 3: check access
      let hasAccess: boolean
      try {
        hasAccess = await beehiveService.checkAccess(info.id)
      } catch {
        if (!cancelled) setState('error')
        return
      }

      if (cancelled) return

      if (hasAccess) {
        navigate(`/beehives/${info.id}`, { replace: true })
      } else {
        setState('no-access')
      }
    }

    run()
    return () => { cancelled = true }
  }, [uniqueId, isAuthenticated, navigate])

  if (state === 'loading') {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-honey-50 gap-4">
        <Loader2 className="w-10 h-10 text-honey-500 animate-spin" />
        <p className="text-gray-500 text-sm">Opening beehive…</p>
      </div>
    )
  }

  if (state === 'not-found') {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-honey-50 px-6">
        <div className="bg-white rounded-2xl shadow-xl border border-honey-100 px-8 py-10 max-w-sm w-full text-center">
          <div className="flex items-center justify-center w-16 h-16 rounded-full bg-amber-50 mx-auto mb-4">
            <QrCode className="w-8 h-8 text-amber-400" />
          </div>
          <h1 className="text-xl font-bold text-gray-900 mb-2">Beehive Not Found</h1>
          <p className="text-gray-500 text-sm">
            This QR code doesn't match any beehive in the system. It may have been removed.
          </p>
          <button
            onClick={() => navigate('/', { replace: true })}
            className="mt-6 w-full py-2.5 px-4 rounded-xl bg-honey-500 hover:bg-honey-600 text-white font-semibold text-sm transition-colors"
          >
            Go to Dashboard
          </button>
        </div>
      </div>
    )
  }

  if (state === 'no-access') {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-honey-50 px-6">
        <div className="bg-white rounded-2xl shadow-xl border border-honey-100 px-8 py-10 max-w-sm w-full text-center">
          <div className="flex items-center justify-center w-16 h-16 rounded-full bg-red-50 mx-auto mb-4">
            <Lock className="w-8 h-8 text-red-400" />
          </div>
          <h1 className="text-xl font-bold text-gray-900 mb-2">Access Denied</h1>
          {beehiveName && (
            <p className="text-sm font-medium text-honey-700 mb-2">{beehiveName}</p>
          )}
          <p className="text-gray-500 text-sm">
            You don't have permission to view this beehive. Please contact your administrator to request access.
          </p>
          <button
            onClick={() => navigate('/', { replace: true })}
            className="mt-6 w-full py-2.5 px-4 rounded-xl bg-honey-500 hover:bg-honey-600 text-white font-semibold text-sm transition-colors"
          >
            Go to Dashboard
          </button>
        </div>
      </div>
    )
  }

  // error state
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-honey-50 px-6">
      <div className="bg-white rounded-2xl shadow-xl border border-honey-100 px-8 py-10 max-w-sm w-full text-center">
        <div className="flex items-center justify-center w-16 h-16 rounded-full bg-orange-50 mx-auto mb-4">
          <AlertTriangle className="w-8 h-8 text-orange-400" />
        </div>
        <h1 className="text-xl font-bold text-gray-900 mb-2">Something Went Wrong</h1>
        <p className="text-gray-500 text-sm">
          Unable to open the beehive right now. Please check your connection and try again.
        </p>
        <button
          onClick={() => window.location.reload()}
          className="mt-6 w-full py-2.5 px-4 rounded-xl bg-honey-500 hover:bg-honey-600 text-white font-semibold text-sm transition-colors"
        >
          Try Again
        </button>
      </div>
    </div>
  )
}
