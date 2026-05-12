import { useState, useRef, useCallback } from 'react'

export type VoiceInputState = 'idle' | 'recording' | 'error'

interface UseVoiceInputReturn {
  state: VoiceInputState
  errorMessage: string | null
  startRecording: () => Promise<void>
  stopRecording: () => Promise<Blob>
  reset: () => void
}

export function useVoiceInput(): UseVoiceInputReturn {
  const [state, setState]               = useState<VoiceInputState>('idle')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const recorderRef = useRef<MediaRecorder | null>(null)
  const streamRef   = useRef<MediaStream | null>(null)
  const chunksRef   = useRef<Blob[]>([])

  const startRecording = useCallback(async () => {
    setErrorMessage(null)

    let stream: MediaStream
    try {
      stream = await navigator.mediaDevices.getUserMedia({ audio: true })
    } catch {
      setState('error')
      setErrorMessage('Pristup mikrofonu je odbijen. Dozvolite pristup u postavkama pretraživača.')
      return
    }

    streamRef.current = stream
    chunksRef.current = []

    const mimeType =
      MediaRecorder.isTypeSupported('audio/webm;codecs=opus') ? 'audio/webm;codecs=opus' :
      MediaRecorder.isTypeSupported('audio/webm')             ? 'audio/webm' :
      MediaRecorder.isTypeSupported('audio/mp4')              ? 'audio/mp4' :
      ''

    const recorder = new MediaRecorder(stream, mimeType ? { mimeType } : undefined)
    recorder.ondataavailable = (e) => { if (e.data.size > 0) chunksRef.current.push(e.data) }
    recorderRef.current = recorder
    recorder.start(250)
    setState('recording')
  }, [])

  const stopRecording = useCallback((): Promise<Blob> => {
    return new Promise((resolve) => {
      const recorder = recorderRef.current
      if (!recorder) { resolve(new Blob()); return }

      recorder.onstop = () => {
        const blob = new Blob(chunksRef.current, { type: recorder.mimeType || 'audio/webm' })
        streamRef.current?.getTracks().forEach(t => t.stop())
        resolve(blob)
      }

      recorder.stop()
    })
  }, [])

  const reset = useCallback(() => {
    recorderRef.current?.stop()
    streamRef.current?.getTracks().forEach(t => t.stop())
    recorderRef.current = null
    streamRef.current   = null
    chunksRef.current   = []
    setErrorMessage(null)
    setState('idle')
  }, [])

  return { state, errorMessage, startRecording, stopRecording, reset }
}
