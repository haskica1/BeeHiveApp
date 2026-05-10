import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { X, AlertCircle } from 'lucide-react'
import { BrowserQRCodeReader, IScannerControls } from '@zxing/browser'

interface Props {
  onClose: () => void
}

function extractUniqueId(text: string): string | null {
  // Handle full URLs: https://example.com/scan/abc123
  try {
    const url = new URL(text)
    const match = url.pathname.match(/\/scan\/([^/]+)/)
    if (match) return match[1]
  } catch {}
  // Handle relative paths: /scan/abc123
  const pathMatch = text.match(/\/scan\/([^/]+)/)
  if (pathMatch) return pathMatch[1]
  // Raw UUID
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(text)) return text
  return null
}

export default function QrScannerModal({ onClose }: Props) {
  const videoRef = useRef<HTMLVideoElement>(null)
  const controlsRef = useRef<IScannerControls | null>(null)
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const reader = new BrowserQRCodeReader()
    let cancelled = false
    let controls: IScannerControls | null = null

    reader
      .decodeFromVideoDevice(undefined, videoRef.current!, (result) => {
        if (cancelled || !result) return
        const uniqueId = extractUniqueId(result.getText())
        if (uniqueId) {
          cancelled = true
          controls?.stop()
          onClose()
          navigate(`/scan/${uniqueId}`)
        }
      })
      .then((c) => {
        controls = c
        controlsRef.current = c
        if (cancelled) c.stop()
      })
      .catch(() => {
        if (!cancelled)
          setError('Camera access denied. Please allow camera permission and try again.')
      })

    return () => {
      cancelled = true
      controls?.stop()
      controlsRef.current?.stop()
    }
  }, [navigate, onClose])

  return (
    <div
      className="fixed inset-0 z-50 flex items-end sm:items-center justify-center bg-black/75"
      onClick={onClose}
    >
      <div
        className="relative w-full max-w-sm mx-4 mb-4 sm:mb-0 rounded-2xl overflow-hidden bg-black shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="absolute top-0 inset-x-0 z-10 flex items-center justify-between px-4 pt-4 pb-2">
          <span className="bg-black/60 text-white text-sm font-medium px-3 py-1 rounded-full backdrop-blur-sm">
            Scan Beehive QR Code
          </span>
          <button
            onClick={onClose}
            className="p-1.5 rounded-full bg-black/60 text-white hover:bg-black/80 transition-colors backdrop-blur-sm"
            aria-label="Close scanner"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {error ? (
          <div className="flex flex-col items-center justify-center gap-4 p-10 text-center min-h-[320px]">
            <AlertCircle className="w-10 h-10 text-red-400" />
            <p className="text-white text-sm leading-relaxed">{error}</p>
            <button
              onClick={onClose}
              className="mt-2 px-5 py-2 rounded-xl bg-honey-500 hover:bg-honey-600 text-white text-sm font-semibold transition-colors"
            >
              Close
            </button>
          </div>
        ) : (
          <>
            <video
              ref={videoRef}
              className="w-full aspect-square object-cover"
              muted
              playsInline
            />

            {/* Scanning frame overlay */}
            <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
              {/* Dark vignette around the frame */}
              <div className="absolute inset-0 bg-black/40" />
              <div className="relative z-10 w-52 h-52 bg-transparent">
                {/* Clear area */}
                <div className="absolute inset-0 bg-transparent mix-blend-normal" />
                {/* Corners */}
                <div className="absolute top-0 left-0 w-10 h-10 border-t-[3px] border-l-[3px] border-honey-400 rounded-tl" />
                <div className="absolute top-0 right-0 w-10 h-10 border-t-[3px] border-r-[3px] border-honey-400 rounded-tr" />
                <div className="absolute bottom-0 left-0 w-10 h-10 border-b-[3px] border-l-[3px] border-honey-400 rounded-bl" />
                <div className="absolute bottom-0 right-0 w-10 h-10 border-b-[3px] border-r-[3px] border-honey-400 rounded-br" />
                {/* Scan line animation */}
                <div className="absolute inset-x-0 h-0.5 bg-honey-400/70 animate-scan-line" />
              </div>
            </div>

            <div className="absolute bottom-4 inset-x-0 flex justify-center pointer-events-none">
              <span className="bg-black/60 text-white/80 text-xs px-4 py-1.5 rounded-full backdrop-blur-sm">
                Point camera at a beehive QR code
              </span>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
