import { useEffect, useRef, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { Circle, Loader2, Mic, Square } from 'lucide-react'
import {
  useCreateInspection,
  useUpdateInspection,
} from '../../core/services/queries'
import { inspectionService } from '../../core/services/beehiveService'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { queryKeys } from '../../core/services/queries'
import { LoadingSpinner, ErrorMessage, FormHeader } from '../../shared/components'
import { HoneyLevel, HoneyLevelLabels } from '../../core/models'
import type { Beehive, CreateInspectionPayload } from '../../core/models'
import { useVoiceInput } from '../../core/hooks/useVoiceInput'
import { useOnlineStatus } from '../../core/hooks/useOnlineStatus'
import { useAuth } from '../../core/context/AuthContext'
import { useToast } from '../../core/context/ToastContext'
import {
  enqueueInspection,
  getOutboxItem,
  removeOutboxItem,
  updateOutboxItem,
  type OutboxItem,
} from '../../core/offline/outbox'
import { isNetworkError } from '../../core/offline/syncOutbox'

export default function InspectionFormPage() {
  const { id } = useParams<{ id?: string }>()
  const [searchParams] = useSearchParams()
  const isEditing   = Boolean(id)
  const inspectionId = Number(id)
  const beehiveId   = Number(searchParams.get('beehiveId') ?? 0)
  const outboxId    = searchParams.get('outboxId')
  const navigate    = useNavigate()

  // Offline outbox (SPEC-07)
  const online = useOnlineStatus()
  const { user } = useAuth()
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const outboxItemRef = useRef<OutboxItem | null>(null)

  // ── Voice state ──────────────────────────────────────────────────────────
  const [voiceOpen, setVoiceOpen]     = useState(false)
  const [recordedBlob, setRecordedBlob] = useState<Blob | null>(null)
  const [isParsing, setIsParsing]     = useState(false)
  const [parseError, setParseError]   = useState<string | null>(null)
  const blobRef = useRef<Blob | null>(null)   // stable ref alongside state

  const voice = useVoiceInput()
  const isRecording = voice.state === 'recording'
  const isDone      = voice.state === 'done'

  // ── Query for edit mode ──────────────────────────────────────────────────
  const { data: inspection, isLoading } = useQuery({
    queryKey: queryKeys.inspection(inspectionId),
    queryFn: () => inspectionService.getById(inspectionId),
    enabled: isEditing && !!inspectionId,
  })

  const resolvedBeehiveId = isEditing ? (inspection?.beehiveId ?? beehiveId) : beehiveId
  const createMutation    = useCreateInspection(resolvedBeehiveId)
  const updateMutation    = useUpdateInspection(inspectionId, resolvedBeehiveId)
  const backUrl           = outboxId ? '/outbox' : `/beehives/${resolvedBeehiveId}`

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<CreateInspectionPayload>({
    defaultValues: {
      date:       new Date().toISOString().split('T')[0],
      honeyLevel: HoneyLevel.Medium,
      beehiveId:  beehiveId || undefined,
    },
  })

  useEffect(() => {
    if (inspection && isEditing) {
      reset({
        date:        inspection.date.split('T')[0],
        temperature: inspection.temperature ?? undefined,
        honeyLevel:  inspection.honeyLevel,
        broodStatus: inspection.broodStatus ?? '',
        notes:       inspection.notes ?? '',
        beehiveId:   inspection.beehiveId,
      })
    }
  }, [inspection, isEditing, reset])

  // Editing an unsent offline entry: prefill from the outbox item (SPEC-07).
  useEffect(() => {
    if (!outboxId || isEditing) return
    let cancelled = false
    void getOutboxItem(outboxId).then(item => {
      if (cancelled || !item) return
      outboxItemRef.current = item
      reset({
        ...item.payload,
        date: item.payload.date.split('T')[0],
        broodStatus: item.payload.broodStatus ?? '',
        notes: item.payload.notes ?? '',
      })
    })
    return () => { cancelled = true }
  }, [outboxId, isEditing, reset])

  // ── Voice handlers ───────────────────────────────────────────────────────

  const handleOpenVoice = () => {
    voice.reset()
    setRecordedBlob(null)
    blobRef.current = null
    setParseError(null)
    setVoiceOpen(true)
  }

  const handleCloseVoice = () => {
    voice.reset()
    setRecordedBlob(null)
    blobRef.current = null
    setParseError(null)
    setVoiceOpen(false)
  }

  const handleStartRecording = async () => {
    setParseError(null)
    await voice.startRecording()
  }

  const handleStopRecording = async () => {
    const blob = await voice.stopRecording()
    blobRef.current = blob
    setRecordedBlob(blob)
  }

  const handleResetTranscript = () => {
    voice.resetTranscript()
    setRecordedBlob(null)
    blobRef.current = null
    setParseError(null)
  }

  const handleProcess = async () => {
    const blob = blobRef.current
    if (!blob) return

    setIsParsing(true)
    setParseError(null)
    try {
      const result = await inspectionService.parseVoice(blob)
      if (result.date)               setValue('date', result.date)
      if (result.honeyLevel  != null) setValue('honeyLevel', result.honeyLevel)
      if (result.broodStatus)        setValue('broodStatus', result.broodStatus)
      if (result.notes)              setValue('notes', result.notes)
      // Close voice panel and let user review the populated form
      voice.reset()
      setRecordedBlob(null)
      blobRef.current = null
      setVoiceOpen(false)
    } catch {
      setParseError('Greška pri obradi snimka. Pokušajte ponovo ili unesite podatke ručno.')
    } finally {
      setIsParsing(false)
    }
  }

  // ── Form submit ──────────────────────────────────────────────────────────

  /** Saves the inspection to the local outbox (create or refresh of the edited item). */
  const saveToOutbox = async (payload: CreateInspectionPayload) => {
    if (!user?.email) return
    const existing = outboxItemRef.current
    if (existing) {
      await updateOutboxItem({ ...existing, payload, status: 'pending', error: undefined })
    } else {
      const cachedHive = queryClient.getQueryData<Beehive>(queryKeys.beehive(resolvedBeehiveId))
      await enqueueInspection({
        ownerEmail: user.email,
        beehiveId: resolvedBeehiveId,
        beehiveName: cachedHive?.name ?? `Košnica #${resolvedBeehiveId}`,
        payload,
      })
    }
    toast.success('Nema mreže — pregled je sačuvan lokalno i biće poslan automatski.')
    navigate(backUrl)
  }

  const onSubmit = async (data: CreateInspectionPayload) => {
    const payload: CreateInspectionPayload = {
      ...data,
      honeyLevel:  Number(data.honeyLevel),
      beehiveId:   resolvedBeehiveId,
      temperature: undefined, // set automatically on the backend from apiary weather
    }
    if (isEditing) {
      await updateMutation.mutateAsync(payload)
      navigate(backUrl)
      return
    }

    // Offline pre-check: don't even try the request without a network (SPEC-07).
    if (!navigator.onLine) {
      await saveToOutbox(payload)
      return
    }

    try {
      await createMutation.mutateAsync(payload)
    } catch (err) {
      // Network-level failure (no response — airplane mode mid-request included) → outbox.
      // HTTP errors with a response are real rejections and render via createMutation.error.
      if (isNetworkError(err)) await saveToOutbox(payload)
      return
    }

    // Sent for real — an outbox copy being edited is now obsolete.
    if (outboxItemRef.current) await removeOutboxItem(outboxItemRef.current.localId)
    navigate(backUrl)
  }

  if (isEditing && isLoading) return <LoadingSpinner message="Učitavanje pregleda…" />

  const mutationError = createMutation.error ?? updateMutation.error

  return (
    <div className="animate-fade-in max-w-lg mx-auto">
      <FormHeader
        icon="📋"
        title={isEditing ? 'Uredi pregled' : 'Zabilježi pregled'}
        onBack={() => navigate(backUrl)}
        backLabel="Nazad na košnicu"
      />

      <div className="card">
        {mutationError && (
          <div className="mb-5">
            <ErrorMessage message={mutationError.message} />
          </div>
        )}

        {/* ── Voice input panel (replaces form fields while active) ──────── */}
        {!isEditing && voiceOpen ? (
          <div className="space-y-5">

            {isParsing ? (
              /* Processing spinner */
              <div className="flex flex-col items-center justify-center py-12 gap-3">
                <Loader2 className="w-10 h-10 animate-spin text-honey-500" />
                <p className="text-sm font-medium text-gray-600 dark:text-slate-300">
                  Obrađujem snimak…
                </p>
                <p className="text-xs text-gray-400 dark:text-slate-500">
                  Transkribovanje i ekstrakcija podataka
                </p>
              </div>
            ) : (
              <>
                {/* Live transcript display */}
                <div>
                  <label className="form-label">
                    {isRecording ? (
                      <span className="flex items-center gap-2">
                        <span className="w-2 h-2 rounded-full bg-red-500 animate-pulse inline-block" />
                        Snimanje u toku — govorite bosanskim jezikom
                      </span>
                    ) : isDone ? (
                      'Prepoznati tekst'
                    ) : (
                      'Vaš govor će se prikazati ovdje'
                    )}
                  </label>
                  {voice.hasSpeechSupport ? (
                    <>
                      <textarea
                        readOnly
                        rows={6}
                        value={voice.liveTranscript}
                        placeholder={
                          isRecording
                            ? 'Slušam…'
                            : 'Kliknite "Počni snimanje" i govorite na bosanskom jeziku.\n\nPrimjer: "Pregledao sam košnicu danas, leglo je zdravo, matica se vidi, med je na visokom nivou, dodao sam supericu."'
                        }
                        className={`w-full rounded-xl border px-4 py-3 text-sm leading-relaxed resize-none
                          bg-gray-50 dark:bg-slate-800/60 text-gray-800 dark:text-slate-200
                          placeholder:text-gray-400 dark:placeholder:text-slate-500
                          transition-colors
                          ${isRecording
                            ? 'border-red-300 dark:border-red-500/40 ring-2 ring-red-100 dark:ring-red-500/10'
                            : 'border-gray-200 dark:border-slate-700'
                          }`}
                      />
                      {voice.liveTranscript.trim() === '' && isDone && (
                        <p className="text-xs text-amber-600 dark:text-amber-400 mt-1.5">
                          Tekst nije prepoznat. Provjerite dozvole mikrofona ili snimite ponovo.
                        </p>
                      )}
                    </>
                  ) : (
                    <div className={`w-full rounded-xl border px-4 py-5 text-sm
                      bg-gray-50 dark:bg-slate-800/60
                      ${isRecording
                        ? 'border-red-300 dark:border-red-500/40 ring-2 ring-red-100 dark:ring-red-500/10'
                        : 'border-gray-200 dark:border-slate-700'
                      }`}
                    >
                      {isRecording ? (
                        <p className="text-gray-500 dark:text-slate-400 text-center">
                          Snimanje u toku — govorite normalno.
                          <br />
                          <span className="text-xs mt-1 block text-gray-400 dark:text-slate-500">
                            Pregled teksta nije podržan u ovom pretraživaču. Snimak će biti obrađen na serveru.
                          </span>
                        </p>
                      ) : isDone ? (
                        <p className="text-gray-500 dark:text-slate-400 text-center">
                          Snimak je spreman za obradu.
                        </p>
                      ) : (
                        <p className="text-gray-400 dark:text-slate-500">
                          Kliknite "Počni snimanje" i govorite na bosanskom jeziku.{'\n\n'}Primjer: "Pregledao sam košnicu danas, leglo je zdravo, matica se vidi, med je na visokom nivou, dodao sam supericu."
                          <br /><br />
                          <span className="text-xs text-gray-400 dark:text-slate-500">
                            Pregled teksta nije podržan u ovom pretraživaču — snimak će biti automatski transkribovan na serveru.
                          </span>
                        </p>
                      )}
                    </div>
                  )}
                </div>

                {/* Error */}
                {(parseError || voice.errorMessage) && (
                  <p className="text-sm text-red-600 dark:text-red-300 bg-red-50 dark:bg-red-500/10 rounded-lg px-4 py-3">
                    {parseError ?? voice.errorMessage}
                  </p>
                )}

                {/* Action buttons */}
                {!isDone ? (
                  /* Not recording / actively recording */
                  <div className="flex gap-3">
                    <button
                      type="button"
                      onClick={handleCloseVoice}
                      className="btn-secondary flex-1"
                      disabled={isRecording}
                    >
                      Zatvori
                    </button>

                    {isRecording ? (
                      <button
                        type="button"
                        onClick={handleStopRecording}
                        className="flex-1 flex items-center justify-center gap-2 rounded-xl px-4 py-2.5
                          bg-red-500 hover:bg-red-600 text-white font-medium text-sm transition-colors"
                      >
                        <Square className="w-4 h-4 fill-current" />
                        Završi snimanje
                      </button>
                    ) : (
                      <button
                        type="button"
                        onClick={handleStartRecording}
                        className="flex-1 flex items-center justify-center gap-2 btn-primary"
                      >
                        <Circle className="w-3.5 h-3.5 fill-current" />
                        Počni snimanje
                      </button>
                    )}
                  </div>
                ) : (
                  /* Done — waiting for action */
                  <div className="flex gap-3">
                    <button
                      type="button"
                      onClick={handleResetTranscript}
                      className="btn-secondary flex-1"
                    >
                      Poništi unos
                    </button>
                    <button
                      type="button"
                      onClick={handleProcess}
                      disabled={!recordedBlob}
                      className="btn-primary flex-1"
                    >
                      Obradi snimak
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        ) : (
          /* ── Regular inspection form ─────────────────────────────────── */
          <>
            {/* Voice input toggle — only on create; transcription is a server call, so offline
                the button is disabled with a hint (SPEC-07) */}
            {!isEditing && (
              <div className="flex flex-col items-end mb-5">
                <button
                  type="button"
                  onClick={handleOpenVoice}
                  disabled={!online}
                  className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium
                    bg-honey-50 text-honey-700 hover:bg-honey-100 border border-honey-200
                    dark:bg-honey-500/10 dark:text-honey-300 dark:hover:bg-honey-500/20 dark:border-honey-500/30
                    transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <Mic className="w-3.5 h-3.5" />
                  Unos govorom
                </button>
                {!online && (
                  <p className="mt-1 text-xs text-gray-400 dark:text-slate-500">Glasovni unos zahtijeva mrežu.</p>
                )}
              </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-5">
              {/* Date */}
              <div>
                <label className="form-label" htmlFor="date">
                  Datum pregleda <span className="text-red-500">*</span>
                </label>
                <input
                  id="date"
                  type="date"
                  className="form-input"
                  max={new Date().toISOString().split('T')[0]}
                  {...register('date', {
                    required: 'Datum pregleda je obavezan',
                    validate: v => v <= new Date().toISOString().split('T')[0] || 'Datum ne može biti u budućnosti',
                  })}
                />
                {errors.date && <p className="form-error">{errors.date.message}</p>}
              </div>

              {/* Honey Level */}
              <div>
                <label className="form-label" htmlFor="honeyLevel">
                  Nivo meda <span className="text-red-500">*</span>
                </label>
                <select
                  id="honeyLevel"
                  className="form-input"
                  {...register('honeyLevel', { required: 'Nivo meda je obavezan' })}
                >
                  {Object.entries(HoneyLevelLabels).map(([val, label]) => (
                    <option key={val} value={val}>{label}</option>
                  ))}
                </select>
                {errors.honeyLevel && <p className="form-error">{errors.honeyLevel.message}</p>}
              </div>

              {/* Brood Status */}
              <div>
                <label className="form-label" htmlFor="broodStatus">Status legla</label>
                <input
                  id="broodStatus"
                  type="text"
                  placeholder="npr. Zdravo leglo, matica viđena, jaja vidljiva…"
                  className="form-input"
                  {...register('broodStatus', {
                    maxLength: { value: 500, message: 'Maksimalno 500 znakova' },
                  })}
                />
                {errors.broodStatus && <p className="form-error">{errors.broodStatus.message}</p>}
              </div>

              {/* Notes */}
              <div>
                <label className="form-label" htmlFor="notes">Napomene</label>
                <textarea
                  id="notes"
                  rows={3}
                  placeholder="Ostala zapažanja, primijenjeni tretmani, ubrani med…"
                  className="form-input resize-none"
                  {...register('notes', {
                    maxLength: { value: 2000, message: 'Napomene ne smiju prelaziti 2000 znakova' },
                  })}
                />
                {errors.notes && <p className="form-error">{errors.notes.message}</p>}
              </div>

              {/* Actions */}
              <div className="flex gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => navigate(backUrl)}
                  className="btn-secondary flex-1"
                >
                  Otkaži
                </button>
                <button type="submit" className="btn-primary flex-1" disabled={isSubmitting}>
                  {isSubmitting ? (
                    <Loader2 className="w-4 h-4 animate-spin" />
                  ) : isEditing ? (
                    'Spremi promjene'
                  ) : (
                    'Zabilježi pregled'
                  )}
                </button>
              </div>
            </form>
          </>
        )}
      </div>
    </div>
  )
}
