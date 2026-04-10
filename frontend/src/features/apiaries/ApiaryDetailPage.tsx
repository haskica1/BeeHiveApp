import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Pencil, Plus, Trash2, MapPin, Wind, Droplets, Thermometer } from 'lucide-react'
import { format, parseISO } from 'date-fns'
import { useApiary, useApiaryWeather, useDeleteBeehive } from '../../core/services/queries'
import {
  LoadingSpinner,
  ErrorMessage,
  EmptyState,
  ConfirmDialog,
  PageHeader,
} from '../../shared/components'
import type { Beehive, DailyWeather } from '../../core/models'

// ── WMO weather code → emoji + label ─────────────────────────────────────────

function wmoToIcon(code?: number): string {
  if (code == null) return '🌡️'
  if (code === 0)                   return '☀️'
  if (code === 1)                   return '🌤️'
  if (code === 2)                   return '⛅'
  if (code === 3)                   return '☁️'
  if (code === 45 || code === 48)   return '🌫️'
  if (code >= 51 && code <= 55)     return '🌦️'
  if (code >= 61 && code <= 65)     return '🌧️'
  if (code >= 71 && code <= 77)     return '🌨️'
  if (code >= 80 && code <= 82)     return '🌧️'
  if (code >= 85 && code <= 86)     return '🌨️'
  if (code >= 95 && code <= 99)     return '⛈️'
  return '🌡️'
}

function wmoToLabel(code?: number): string {
  if (code == null) return 'Unknown'
  if (code === 0)                   return 'Clear sky'
  if (code === 1)                   return 'Mainly clear'
  if (code === 2)                   return 'Partly cloudy'
  if (code === 3)                   return 'Overcast'
  if (code === 45 || code === 48)   return 'Foggy'
  if (code >= 51 && code <= 55)     return 'Drizzle'
  if (code >= 61 && code <= 65)     return 'Rain'
  if (code >= 71 && code <= 77)     return 'Snow'
  if (code >= 80 && code <= 82)     return 'Rain showers'
  if (code >= 85 && code <= 86)     return 'Snow showers'
  if (code >= 95 && code <= 99)     return 'Thunderstorm'
  return 'Unknown'
}

// ── Weather card for a single day ─────────────────────────────────────────────

function DayCard({ day, isToday }: { day: DailyWeather; isToday: boolean }) {
  const date = parseISO(day.date)
  return (
    <div className={`rounded-xl p-3 text-center flex flex-col gap-1 border transition-all ${
      isToday
        ? 'bg-honey-50 border-honey-300 shadow-honey'
        : 'bg-white border-gray-100 hover:border-honey-200'
    }`}>
      <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide">
        {isToday ? 'Today' : format(date, 'EEE')}
      </p>
      <p className="text-[11px] text-gray-400">{format(date, 'MMM d')}</p>
      <span className="text-3xl my-1" title={wmoToLabel(day.weatherCode)}>
        {wmoToIcon(day.weatherCode)}
      </span>
      <p className="text-[11px] text-gray-500 leading-tight">{wmoToLabel(day.weatherCode)}</p>
      <div className="flex justify-center gap-2 mt-1">
        <span className="text-sm font-bold text-red-500">
          {day.maxTemp != null ? `${Math.round(day.maxTemp)}°` : '–'}
        </span>
        <span className="text-sm text-blue-400">
          {day.minTemp != null ? `${Math.round(day.minTemp)}°` : '–'}
        </span>
      </div>
      {day.precipitationProbability != null && (
        <p className="text-[10px] text-blue-500 flex items-center justify-center gap-0.5 mt-0.5">
          <Droplets className="w-2.5 h-2.5" />
          {Math.round(day.precipitationProbability)}%
        </p>
      )}
    </div>
  )
}

// ── Main component ────────────────────────────────────────────────────────────

export default function ApiaryDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const apiaryId = Number(id)

  const { data: apiary, isLoading, error } = useApiary(apiaryId)
  const { data: weather, isLoading: weatherLoading } = useApiaryWeather(
    apiaryId,
    apiary?.hasLocation ?? false,
  )
  const deleteMutation = useDeleteBeehive(apiaryId)

  const [deleteTarget, setDeleteTarget] = useState<{ id: number; name: string } | null>(null)

  const handleDeleteBeehive = async () => {
    if (!deleteTarget) return
    await deleteMutation.mutateAsync(deleteTarget.id)
    setDeleteTarget(null)
  }

  if (isLoading) return <LoadingSpinner message="Loading apiary…" />
  if (error) return <ErrorMessage message={error.message} />
  if (!apiary) return null

  const today = new Date().toISOString().slice(0, 10)

  return (
    <div className="animate-fade-in">
      <PageHeader
        title={apiary.name}
        subtitle={apiary.description ?? undefined}
        backButton={
          <button
            onClick={() => navigate('/apiaries')}
            className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-honey-600 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" /> All Apiaries
          </button>
        }
        actions={
          <>
            <Link to={`/apiaries/${apiaryId}/edit`} className="btn-secondary text-sm">
              <Pencil className="w-4 h-4" /> Edit
            </Link>
            <Link
              to={`/beehives/new?apiaryId=${apiaryId}`}
              className="btn-primary text-sm"
            >
              <Plus className="w-4 h-4" /> Add Beehive
            </Link>
          </>
        }
      />

      {/* Stats strip */}
      <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 mb-6">
        <StatCard icon="🐝" label="Beehives" value={apiary.beehiveCount} />
        <StatCard
          icon="📋"
          label="Total Inspections"
          value={apiary.beehives?.reduce((s, b) => s + b.inspectionCount, 0) ?? 0}
        />
        <StatCard
          icon="📅"
          label="Since"
          value={format(new Date(apiary.createdAt), 'MMM yyyy')}
        />
      </div>

      {/* Weather forecast */}
      <section className="mb-8">
        <div className="flex items-center justify-between mb-3">
          <h2 className="font-display text-xl font-semibold text-gray-800 flex items-center gap-2">
            🌤️ 7-Day Weather Forecast
          </h2>
          {apiary.hasLocation && (
            <a
              href={`https://maps.google.com/?q=${apiary.latitude},${apiary.longitude}`}
              target="_blank"
              rel="noreferrer"
              className="inline-flex items-center gap-1 text-xs text-honey-600 hover:underline"
            >
              <MapPin className="w-3 h-3" />
              {apiary.latitude?.toFixed(4)}, {apiary.longitude?.toFixed(4)}
            </a>
          )}
        </div>

        {!apiary.hasLocation ? (
          <div className="card text-center py-6 border-dashed border-2 border-gray-200">
            <MapPin className="w-8 h-8 text-gray-300 mx-auto mb-2" />
            <p className="text-gray-500 text-sm">No location set for this apiary.</p>
            <Link
              to={`/apiaries/${apiaryId}/edit`}
              className="inline-flex items-center gap-1 mt-3 text-sm text-honey-600 hover:underline"
            >
              <Pencil className="w-3.5 h-3.5" /> Add location
            </Link>
          </div>
        ) : weatherLoading ? (
          <div className="card py-6 text-center">
            <LoadingSpinner message="Fetching forecast…" />
          </div>
        ) : weather ? (
          <>
            <div className="grid grid-cols-7 gap-2">
              {weather.daily.map((day) => (
                <DayCard key={day.date} day={day} isToday={day.date === today} />
              ))}
            </div>
            {/* Summary row */}
            {weather.daily[0] && (
              <div className="mt-3 card flex flex-wrap gap-4 py-3 text-sm text-gray-600">
                <span className="flex items-center gap-1.5">
                  <Thermometer className="w-4 h-4 text-red-400" />
                  Today: <strong className="text-red-500">{Math.round(weather.daily[0].maxTemp ?? 0)}°C</strong>
                  {' / '}
                  <strong className="text-blue-400">{Math.round(weather.daily[0].minTemp ?? 0)}°C</strong>
                </span>
                {weather.daily[0].precipitationProbability != null && (
                  <span className="flex items-center gap-1.5">
                    <Droplets className="w-4 h-4 text-blue-400" />
                    Rain chance: <strong>{Math.round(weather.daily[0].precipitationProbability)}%</strong>
                  </span>
                )}
                {weather.daily[0].maxWindSpeed != null && (
                  <span className="flex items-center gap-1.5">
                    <Wind className="w-4 h-4 text-gray-400" />
                    Wind: <strong>{Math.round(weather.daily[0].maxWindSpeed)} km/h</strong>
                  </span>
                )}
                <span className="ml-auto text-xs text-gray-400">
                  via Open-Meteo · {weather.timezone}
                </span>
              </div>
            )}
          </>
        ) : (
          <div className="card text-center py-4 text-gray-400 text-sm">
            Weather data unavailable.
          </div>
        )}
      </section>

      {/* Beehive list */}
      <h2 className="font-display text-xl font-semibold text-gray-800 mb-4">Beehives</h2>

      {!apiary.beehives?.length ? (
        <EmptyState
          title="No beehives yet"
          description="Add your first beehive to this apiary."
          action={
            <Link to={`/beehives/new?apiaryId=${apiaryId}`} className="btn-primary text-sm">
              <Plus className="w-4 h-4" /> Add Beehive
            </Link>
          }
        />
      ) : (
        <div className="grid gap-3 sm:grid-cols-2">
          {apiary.beehives.map((beehive: Beehive) => (
            <div
              key={beehive.id}
              className="card hover:shadow-honey hover:-translate-y-0.5 transition-all duration-200 group cursor-pointer"
              onClick={() => navigate(`/beehives/${beehive.id}`)}
            >
              <div className="flex items-start gap-3">
                <span className="text-2xl shrink-0 mt-0.5">🏠</span>
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between gap-2">
                    <h3 className="font-semibold text-gray-800 truncate group-hover:text-honey-700 transition-colors">
                      {beehive.name}
                    </h3>
                    <div
                      className="flex gap-1 shrink-0"
                      onClick={e => e.stopPropagation()}
                    >
                      <Link
                        to={`/beehives/${beehive.id}/edit`}
                        className="p-1.5 rounded-lg text-gray-400 hover:text-honey-600 hover:bg-honey-50 transition-colors"
                      >
                        <Pencil className="w-3.5 h-3.5" />
                      </Link>
                      <button
                        onClick={() => setDeleteTarget({ id: beehive.id, name: beehive.name })}
                        className="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 transition-colors"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>

                  <div className="flex flex-wrap gap-1.5 mt-1.5">
                    <span className="badge bg-honey-100 text-honey-700">{beehive.typeName}</span>
                    <span className="badge bg-gray-100 text-gray-600">{beehive.materialName}</span>
                  </div>

                  <p className="mt-2 text-xs text-gray-500">
                    📋 {beehive.inspectionCount} inspection{beehive.inspectionCount !== 1 ? 's' : ''}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <ConfirmDialog
        isOpen={!!deleteTarget}
        title="Delete Beehive"
        message={`Delete "${deleteTarget?.name}"? All inspection records will also be removed.`}
        onConfirm={handleDeleteBeehive}
        onCancel={() => setDeleteTarget(null)}
        isLoading={deleteMutation.isPending}
      />
    </div>
  )
}

function StatCard({ icon, label, value }: { icon: string; label: string; value: string | number }) {
  return (
    <div className="card text-center py-4">
      <div className="text-2xl mb-1">{icon}</div>
      <div className="font-display text-xl font-bold text-honey-700">{value}</div>
      <div className="text-xs text-gray-500 mt-0.5">{label}</div>
    </div>
  )
}
