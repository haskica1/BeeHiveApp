import { useCallback, useEffect, useRef, useState } from 'react'

export type SpeechStatus = 'idle' | 'speaking' | 'paused'

/**
 * Browser text-to-speech via `speechSynthesis` (SPEC-06 "Poslušaj") — no backend, no cost.
 * Voice pick order: bs-* → hr-* → sr-* → browser default (quality then depends on the device).
 * Long texts are queued as paragraph-sized utterances (single long utterances get cut off in
 * some Chrome versions). Speech is cancelled on unmount so navigation always stops playback.
 */
export function useSpeech() {
  const isSupported = typeof window !== 'undefined' && 'speechSynthesis' in window
  const [status, setStatus] = useState<SpeechStatus>('idle')
  const queueRef = useRef<SpeechSynthesisUtterance[]>([])

  const stop = useCallback(() => {
    if (!isSupported) return
    queueRef.current = []
    window.speechSynthesis.cancel()
    setStatus('idle')
  }, [isSupported])

  // Navigation away must stop the audio.
  useEffect(() => stop, [stop])

  const speak = useCallback((text: string) => {
    if (!isSupported) return
    window.speechSynthesis.cancel()

    const voice = pickVoice(window.speechSynthesis.getVoices())
    const chunks = toChunks(text)
    if (chunks.length === 0) return

    const utterances = chunks.map(chunk => {
      const u = new SpeechSynthesisUtterance(chunk)
      if (voice) u.voice = voice
      u.lang = voice?.lang ?? 'hr'
      u.rate = 0.95
      return u
    })

    const last = utterances[utterances.length - 1]
    last.onend = () => setStatus('idle')
    last.onerror = () => setStatus('idle')

    queueRef.current = utterances
    for (const u of utterances) window.speechSynthesis.speak(u)
    setStatus('speaking')
  }, [isSupported])

  const pause = useCallback(() => {
    if (!isSupported || status !== 'speaking') return
    window.speechSynthesis.pause()
    setStatus('paused')
  }, [isSupported, status])

  const resume = useCallback(() => {
    if (!isSupported || status !== 'paused') return
    window.speechSynthesis.resume()
    setStatus('speaking')
  }, [isSupported, status])

  return { isSupported, status, speak, pause, resume, stop }
}

/** bs → hr → sr → null (browser default). */
function pickVoice(voices: SpeechSynthesisVoice[]): SpeechSynthesisVoice | null {
  for (const prefix of ['bs', 'hr', 'sr']) {
    const match = voices.find(v => v.lang.toLowerCase().startsWith(prefix))
    if (match) return match
  }
  return null
}

/** Paragraph-sized utterance chunks; very long paragraphs are split on sentence boundaries. */
function toChunks(text: string): string[] {
  const chunks: string[] = []
  for (const paragraph of text.split(/\n{2,}/)) {
    const p = paragraph.trim()
    if (!p) continue
    if (p.length <= 400) {
      chunks.push(p)
      continue
    }
    let current = ''
    for (const sentence of p.split(/(?<=[.!?])\s+/)) {
      if (current && current.length + sentence.length > 400) {
        chunks.push(current)
        current = sentence
      } else {
        current = current ? `${current} ${sentence}` : sentence
      }
    }
    if (current) chunks.push(current)
  }
  return chunks
}
