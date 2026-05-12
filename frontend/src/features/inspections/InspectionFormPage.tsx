import { useEffect, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { ArrowLeft, Loader2, Mic, MicOff, X } from 'lucide-react'
import {
  useCreateInspection,
  useUpdateInspection,
} from '../../core/services/queries'
import { inspectionService } from '../../core/services/beehiveService'
import { useQuery } from '@tanstack/react-query'
import { queryKeys } from '../../core/services/queries'
import { LoadingSpinner, ErrorMessage, PageHeader } from '../../shared/components'
import { HoneyLevel, HoneyLevelLabels } from '../../core/models'
import type { CreateInspectionPayload } from '../../core/models'
import { useVoiceInput } from './useVoiceInput'

export default function InspectionFormPage() {
  const { id } = useParams<{ id?: string }>()
  const [searchParams] = useSearchParams()
  const isEditing = Boolean(id)
  const inspectionId = Number(id)
  const beehiveId = Number(searchParams.get('beehiveId') ?? 0)
  const navigate = useNavigate()

  const [voiceOpen, setVoiceOpen] = useState(false)
  const [parseError, setParseError] = useState<string | null>(null)
  const [isParsing, setIsParsing] = useState(false)

  // When editing, load existing inspection
  const { data: inspection, isLoading } = useQuery({
    queryKey: queryKeys.inspection(inspectionId),
    queryFn: () => inspectionService.getById(inspectionId),
    enabled: isEditing && !!inspectionId,
  })

  const resolvedBeehiveId = isEditing ? (inspection?.beehiveId ?? beehiveId) : beehiveId

  const createMutation = useCreateInspection(resolvedBeehiveId)
  const updateMutation = useUpdateInspection(inspectionId, resolvedBeehiveId)

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<CreateInspectionPayload>({
    defaultValues: {
      date: new Date().toISOString().split('T')[0],
      honeyLevel: HoneyLevel.Medium,
      beehiveId: beehiveId || undefined,
    },
  })

  useEffect(() => {
    if (inspection && isEditing) {
      reset({
        date: inspection.date.split('T')[0],
        temperature: inspection.temperature ?? undefined,
        honeyLevel: inspection.honeyLevel,
        broodStatus: inspection.broodStatus ?? '',
        notes: inspection.notes ?? '',
        beehiveId: inspection.beehiveId,
      })
    }
  }, [inspection, isEditing, reset])

  const voice = useVoiceInput()

  const handleVoiceToggle = () => {
    if (!voiceOpen) {
      setVoiceOpen(true)
      setParseError(null)
      voice.reset()
    } else {
      voice.reset()
      setVoiceOpen(false)
      setParseError(null)
    }
  }

  const handleStartStop = () => {
    if (voice.state === 'recording') {
      voice.stopRecording()
    } else {
      voice.startRecording()
    }
  }

  const handleApplyTranscript = async () => {
    if (!voice.transcript.trim()) return
    setIsParsing(true)
    setParseError(null)
    try {
      const result = await inspectionService.parseVoice(voice.transcript)
      if (result.date)        setValue('date', result.date)
      if (result.temperature != null) setValue('temperature', result.temperature)
      if (result.honeyLevel  != null) setValue('honeyLevel', result.honeyLevel)
      if (result.broodStatus) setValue('broodStatus', result.broodStatus)
      if (result.notes)       setValue('notes', result.notes)
      setVoiceOpen(false)
      voice.reset()
    } catch {
      setParseError('Greška pri obradi transkripta. Pokušajte ponovo.')
    } finally {
      setIsParsing(false)
    }
  }

  const onSubmit = async (data: CreateInspectionPayload) => {
    const payload: CreateInspectionPayload = {
      ...data,
      honeyLevel: Number(data.honeyLevel),
      beehiveId: resolvedBeehiveId,
      temperature: data.temperature ? Number(data.temperature) : undefined,
    }

    if (isEditing) {
      await updateMutation.mutateAsync(payload)
    } else {
      await createMutation.mutateAsync(payload)
    }
    navigate(`/beehives/${resolvedBeehiveId}`)
  }

  if (isEditing && isLoading) return <LoadingSpinner message="Loading inspection…" />

  const mutationError = createMutation.error ?? updateMutation.error
  const backUrl = `/beehives/${resolvedBeehiveId}`

  const isRecording = voice.state === 'recording'
  const canApply = (voice.state === 'done' || (voice.state === 'recording' && voice.transcript)) && voice.transcript.trim().length > 0

  return (
    <div className="animate-fade-in max-w-lg mx-auto">
      <PageHeader
        title={isEditing ? 'Edit Inspection' : 'Record Inspection'}
        backButton={
          <button
            onClick={() => navigate(backUrl)}
            className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-honey-600 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" /> Back to Beehive
          </button>
        }
      />

      <div className="card">
        {mutationError && (
          <div className="mb-5">
            <ErrorMessage message={mutationError.message} />
          </div>
        )}

        {/* Voice input panel */}
        {!isEditing && (
          <div className="mb-5">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-600">Unesite glasom</span>
              <button
                type="button"
                onClick={handleVoiceToggle}
                className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                  voiceOpen
                    ? 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                    : 'bg-honey-50 text-honey-700 hover:bg-honey-100 border border-honey-200'
                }`}
              >
                {voiceOpen ? (
                  <><X className="w-3.5 h-3.5" /> Zatvori</>
                ) : (
                  <><Mic className="w-3.5 h-3.5" /> Unos glasom</>
                )}
              </button>
            </div>

            {voiceOpen && (
              <div className="rounded-xl border border-gray-200 bg-gray-50 p-4 space-y-3">
                {/* Mic button */}
                <div className="flex items-center gap-3">
                  <button
                    type="button"
                    onClick={handleStartStop}
                    disabled={voice.state === 'processing' || isParsing}
                    className={`flex items-center justify-center w-11 h-11 rounded-full transition-all shadow-sm ${
                      isRecording
                        ? 'bg-red-500 hover:bg-red-600 text-white animate-pulse'
                        : 'bg-white border-2 border-honey-400 text-honey-600 hover:bg-honey-50'
                    } disabled:opacity-50 disabled:cursor-not-allowed`}
                    title={isRecording ? 'Zaustavi snimanje' : 'Počni snimanje'}
                  >
                    {isRecording ? <MicOff className="w-5 h-5" /> : <Mic className="w-5 h-5" />}
                  </button>

                  <span className="text-sm text-gray-500">
                    {isRecording
                      ? 'Snima... kliknite za zaustavljanje'
                      : voice.state === 'done' || voice.transcript
                      ? 'Snimanje završeno'
                      : 'Kliknite za početak snimanja'}
                  </span>

                  {isRecording && (
                    <span className="ml-auto flex items-center gap-1 text-xs text-red-500 font-medium">
                      <span className="w-2 h-2 rounded-full bg-red-500 animate-pulse" />
                      LIVE
                    </span>
                  )}
                </div>

                {/* Transcript display */}
                {voice.transcript && (
                  <div className="rounded-lg border border-gray-200 bg-white px-3 py-2.5">
                    <p className="text-xs text-gray-400 mb-1 font-medium uppercase tracking-wide">Prepoznat tekst</p>
                    <p className="text-sm text-gray-700 leading-relaxed">{voice.transcript}</p>
                  </div>
                )}

                {/* Error from speech recognition */}
                {voice.errorMessage && (
                  <p className="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{voice.errorMessage}</p>
                )}

                {/* Error from parsing */}
                {parseError && (
                  <p className="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{parseError}</p>
                )}

                {/* Apply button */}
                {canApply && (
                  <button
                    type="button"
                    onClick={handleApplyTranscript}
                    disabled={isParsing}
                    className="w-full btn-primary flex items-center justify-center gap-2 py-2"
                  >
                    {isParsing ? (
                      <><Loader2 className="w-4 h-4 animate-spin" /> Obrađujem...</>
                    ) : (
                      'Popuni polja iz snimka'
                    )}
                  </button>
                )}
              </div>
            )}
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-5">
          {/* Date */}
          <div>
            <label className="form-label" htmlFor="date">
              Inspection Date <span className="text-red-500">*</span>
            </label>
            <input
              id="date"
              type="date"
              className="form-input"
              max={new Date().toISOString().split('T')[0]}
              {...register('date', { required: 'Inspection date is required' })}
            />
            {errors.date && <p className="form-error">{errors.date.message}</p>}
          </div>

          {/* Temperature + Honey Level row */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="form-label" htmlFor="temperature">
                Temperature (°C)
              </label>
              <input
                id="temperature"
                type="number"
                step="0.1"
                min="-50"
                max="60"
                placeholder="e.g. 22.5"
                className="form-input"
                {...register('temperature', {
                  min: { value: -50, message: 'Min -50°C' },
                  max: { value: 60, message: 'Max 60°C' },
                })}
              />
              {errors.temperature && <p className="form-error">{errors.temperature.message}</p>}
            </div>

            <div>
              <label className="form-label" htmlFor="honeyLevel">
                Honey Level <span className="text-red-500">*</span>
              </label>
              <select
                id="honeyLevel"
                className="form-input"
                {...register('honeyLevel', { required: 'Honey level is required' })}
              >
                {Object.entries(HoneyLevelLabels).map(([val, label]) => (
                  <option key={val} value={val}>{label}</option>
                ))}
              </select>
              {errors.honeyLevel && <p className="form-error">{errors.honeyLevel.message}</p>}
            </div>
          </div>

          {/* Brood Status */}
          <div>
            <label className="form-label" htmlFor="broodStatus">Brood Status</label>
            <input
              id="broodStatus"
              type="text"
              placeholder="e.g. Healthy pattern, queen spotted, eggs visible…"
              className="form-input"
              {...register('broodStatus', {
                maxLength: { value: 500, message: 'Max 500 characters' },
              })}
            />
            {errors.broodStatus && <p className="form-error">{errors.broodStatus.message}</p>}
          </div>

          {/* Notes */}
          <div>
            <label className="form-label" htmlFor="notes">Notes</label>
            <textarea
              id="notes"
              rows={3}
              placeholder="Any other observations, treatments applied, honey harvested…"
              className="form-input resize-none"
              {...register('notes', {
                maxLength: { value: 2000, message: 'Notes must not exceed 2000 characters' },
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
              Cancel
            </button>
            <button type="submit" className="btn-primary flex-1" disabled={isSubmitting}>
              {isSubmitting ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : isEditing ? (
                'Save Changes'
              ) : (
                'Record Inspection'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
