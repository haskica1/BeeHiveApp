import { useState, useRef, useCallback } from 'react'

export type VoiceInputState = 'idle' | 'recording' | 'processing' | 'done' | 'error'

interface UseVoiceInputReturn {
  state: VoiceInputState
  transcript: string
  errorMessage: string | null
  startRecording: () => void
  stopRecording: () => void
  reset: () => void
}

export function useVoiceInput(): UseVoiceInputReturn {
  const [state, setState] = useState<VoiceInputState>('idle')
  const [transcript, setTranscript] = useState('')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const recognitionRef = useRef<SpeechRecognition | null>(null)
  const accumulatedRef = useRef<string>('')
  const hasErrorRef    = useRef(false)

  const startRecording = useCallback(() => {
    const SpeechRecognition =
      window.SpeechRecognition ?? (window as unknown as { webkitSpeechRecognition: typeof window.SpeechRecognition }).webkitSpeechRecognition

    if (!SpeechRecognition) {
      setState('error')
      setErrorMessage('Vaš pretraživač ne podržava prepoznavanje glasa. Koristite Chrome ili Edge.')
      return
    }

    accumulatedRef.current = ''
    hasErrorRef.current    = false
    setTranscript('')
    setErrorMessage(null)

    const recognition = new SpeechRecognition()
    recognition.lang = 'bs-BA'
    recognition.continuous = true
    recognition.interimResults = true
    recognition.maxAlternatives = 1

    recognition.onresult = (event: SpeechRecognitionEvent) => {
      let interim = ''
      let final = accumulatedRef.current

      for (let i = event.resultIndex; i < event.results.length; i++) {
        const result = event.results[i]
        if (result.isFinal) {
          final += result[0].transcript + ' '
          accumulatedRef.current = final
        } else {
          interim += result[0].transcript
        }
      }

      setTranscript((final + interim).trim())
    }

    recognition.onerror = (event: SpeechRecognitionErrorEvent) => {
      if (event.error === 'no-speech') return
      hasErrorRef.current = true
      setState('error')
      const messages: Record<string, string> = {
        'not-allowed': 'Pristup mikrofonu je odbijen. Dozvolite pristup u postavkama pretraživača.',
        'network':     'Greška mreže pri prepoznavanju glasa. Provjerite internet vezu.',
        'audio-capture': 'Mikrofon nije pronađen. Provjerite da li je spojen.',
      }
      setErrorMessage(messages[event.error] ?? `Greška prepoznavanja: ${event.error}`)
    }

    recognition.onend = () => {
      if (!hasErrorRef.current) {
        setState('done')
      }
    }

    recognitionRef.current = recognition
    recognition.start()
    setState('recording')
  }, [])

  const stopRecording = useCallback(() => {
    recognitionRef.current?.stop()
    setState('processing')
  }, [])

  const reset = useCallback(() => {
    recognitionRef.current?.abort()
    recognitionRef.current = null
    accumulatedRef.current = ''
    setTranscript('')
    setErrorMessage(null)
    setState('idle')
  }, [])

  return { state, transcript, errorMessage, startRecording, stopRecording, reset }
}
