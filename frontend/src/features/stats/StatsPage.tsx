import {
  AreaChart, Area,
  BarChart, Bar,
  PieChart, Pie, Cell,
  LineChart, Line,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
} from 'recharts'
import { Activity, BarChart2, CheckSquare, Leaf } from 'lucide-react'
import { useStats } from '../../core/services/queries'
import { LoadingSpinner, ErrorMessage } from '../../shared/components'
import type { NameValue, MonthTemp, PriorityStats } from '../../core/services/statsService'

// ── Colour palettes ───────────────────────────────────────────────────────────

const HONEY   = ['#f59e0b', '#fbbf24', '#fcd34d', '#fde68a', '#fef3c7']
const MULTI   = ['#f59e0b', '#10b981', '#6366f1', '#f43f5e', '#0ea5e9', '#8b5cf6', '#14b8a6', '#f97316']
const HONEY_GRADIENT_ID = 'honeyGrad'
const TEMP_GRADIENT_ID  = 'tempGrad'

// ── Custom tooltip ─────────────────────────────────────────────────────────────

function CustomTooltip({ active, payload, label }: {
  active?: boolean; payload?: { name: string; value: number | null; color?: string }[]; label?: string
}) {
  if (!active || !payload?.length) return null
  return (
    <div className="bg-white border border-honey-100 rounded-xl shadow-lg px-4 py-3 text-sm">
      {label && <p className="font-semibold text-gray-700 mb-1">{label}</p>}
      {payload.map((p, i) => (
        <p key={i} style={{ color: p.color ?? '#f59e0b' }} className="font-medium">
          {p.name}: {p.value != null ? p.value : '—'}
        </p>
      ))}
    </div>
  )
}

// ── Custom pie label ───────────────────────────────────────────────────────────

function PieLabel({ cx, cy, midAngle, innerRadius, outerRadius, percent }: {
  cx: number; cy: number; midAngle: number; innerRadius: number; outerRadius: number;
  percent: number; name: string
}) {
  if (percent < 0.05) return null
  const RADIAN = Math.PI / 180
  const radius = innerRadius + (outerRadius - innerRadius) * 0.5
  const x = cx + radius * Math.cos(-midAngle * RADIAN)
  const y = cy + radius * Math.sin(-midAngle * RADIAN)
  return (
    <text x={x} y={y} fill="white" textAnchor="middle" dominantBaseline="central"
      fontSize={11} fontWeight="600">
      {`${(percent * 100).toFixed(0)}%`}
    </text>
  )
}

// ── Section wrapper ────────────────────────────────────────────────────────────

function Section({ title, icon, children }: { title: string; icon: string; children: React.ReactNode }) {
  return (
    <div className="card mb-6">
      <div className="flex items-center gap-2 mb-5">
        <span className="text-xl">{icon}</span>
        <h2 className="font-display text-lg font-semibold text-gray-800">{title}</h2>
      </div>
      {children}
    </div>
  )
}

// ── KPI card ───────────────────────────────────────────────────────────────────

function KpiCard({ icon, label, value, color }: {
  icon: React.ReactNode; label: string; value: number; color: string
}) {
  return (
    <div className={`rounded-2xl p-5 flex items-center gap-4 ${color}`}>
      <div className="w-12 h-12 rounded-xl bg-white/30 flex items-center justify-center shrink-0">
        {icon}
      </div>
      <div>
        <p className="text-3xl font-bold font-display">{value}</p>
        <p className="text-sm font-medium opacity-80 mt-0.5">{label}</p>
      </div>
    </div>
  )
}

// ── Empty chart placeholder ────────────────────────────────────────────────────

function EmptyChart({ message = 'Not enough data yet' }: { message?: string }) {
  return (
    <div className="flex items-center justify-center h-48 rounded-xl border-2 border-dashed border-gray-200">
      <p className="text-gray-400 text-sm">{message}</p>
    </div>
  )
}

// ── Main page ─────────────────────────────────────────────────────────────────

export default function StatsPage() {
  const { data: stats, isLoading, error } = useStats()

  if (isLoading) return <LoadingSpinner message="Loading statistics…" />
  if (error)     return <ErrorMessage message={error.message} />
  if (!stats)    return null

  const hasInspectionData   = stats.inspectionsByMonth.some(m => m.count > 0)
  const hasTempData         = stats.temperatureByMonth.some(m => m.avgTemp != null)
  const hasHiveData         = stats.beehivesByType.length > 0
  const hasDietData         = stats.dietsByStatus.length > 0
  const hasApiaryData       = stats.apiariesByBeehiveCount.length > 0
  const hasTodoData         = stats.todosByPriority.length > 0
  const hasTopBeehivesData  = stats.topBeehivesByInspections.some(b => b.value > 0)

  return (
    <div className="animate-fade-in">
      {/* Page header */}
      <div className="mb-8">
        <h1 className="font-display text-3xl font-bold text-gray-800 flex items-center gap-3">
          <BarChart2 className="w-8 h-8 text-honey-500" />
          Statistics
        </h1>
        <p className="text-gray-500 mt-1 text-sm">Overview of your apiary activity and hive health</p>
      </div>

      {/* ── KPI Summary ──────────────────────────────────────────────────────── */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-8">
        <KpiCard
          icon={<span className="text-2xl">🏡</span>}
          label="Apiaries"
          value={stats.totalApiaries}
          color="bg-gradient-to-br from-honey-400 to-honey-600 text-white"
        />
        <KpiCard
          icon={<span className="text-2xl">🐝</span>}
          label="Beehives"
          value={stats.totalBeehives}
          color="bg-gradient-to-br from-amber-400 to-orange-500 text-white"
        />
        <KpiCard
          icon={<Activity className="w-6 h-6 text-white" />}
          label="Inspections"
          value={stats.totalInspections}
          color="bg-gradient-to-br from-emerald-400 to-teal-600 text-white"
        />
        <KpiCard
          icon={<span className="text-2xl">🌿</span>}
          label="Active Diets"
          value={stats.activeDiets}
          color="bg-gradient-to-br from-violet-400 to-indigo-600 text-white"
        />
      </div>

      {/* ── Inspections over time ─────────────────────────────────────────────── */}
      <Section title="Inspections — Last 12 Months" icon="📋">
        {!hasInspectionData ? <EmptyChart /> : (
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={stats.inspectionsByMonth} margin={{ top: 5, right: 10, left: -10, bottom: 0 }}>
              <defs>
                <linearGradient id={HONEY_GRADIENT_ID} x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%"  stopColor="#f59e0b" stopOpacity={0.35} />
                  <stop offset="95%" stopColor="#f59e0b" stopOpacity={0.02} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
              <YAxis allowDecimals={false} tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
              <Tooltip content={<CustomTooltip />} />
              <Area
                type="monotone"
                dataKey="count"
                name="Inspections"
                stroke="#f59e0b"
                strokeWidth={2.5}
                fill={`url(#${HONEY_GRADIENT_ID})`}
                dot={{ fill: '#f59e0b', r: 3 }}
                activeDot={{ r: 5 }}
              />
            </AreaChart>
          </ResponsiveContainer>
        )}
      </Section>

      {/* ── Temperature trend ────────────────────────────────────────────────── */}
      <Section title="Temperature Trend — Last 12 Months (°C)" icon="🌡️">
        {!hasTempData ? <EmptyChart message="No temperature data recorded yet" /> : (
          <ResponsiveContainer width="100%" height={220}>
            <LineChart
              data={stats.temperatureByMonth as MonthTemp[]}
              margin={{ top: 5, right: 10, left: -10, bottom: 0 }}
            >
              <defs>
                <linearGradient id={TEMP_GRADIENT_ID} x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%"  stopColor="#ef4444" stopOpacity={0.15} />
                  <stop offset="95%" stopColor="#ef4444" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
              <Tooltip content={<CustomTooltip />} />
              <Legend wrapperStyle={{ fontSize: 12 }} />
              <Line type="monotone" dataKey="maxTemp" name="Max °C" stroke="#ef4444" strokeWidth={2} dot={false} connectNulls />
              <Line type="monotone" dataKey="avgTemp" name="Avg °C" stroke="#f97316" strokeWidth={2.5} dot={{ r: 3 }} connectNulls />
              <Line type="monotone" dataKey="minTemp" name="Min °C" stroke="#3b82f6" strokeWidth={2} dot={false} connectNulls />
            </LineChart>
          </ResponsiveContainer>
        )}
      </Section>

      {/* ── Two-column row: beehive type + material ───────────────────────────── */}
      <div className="grid sm:grid-cols-2 gap-6 mb-6">
        <div className="card">
          <div className="flex items-center gap-2 mb-5">
            <span className="text-xl">🏠</span>
            <h2 className="font-display text-lg font-semibold text-gray-800">Hive Types</h2>
          </div>
          {!hasHiveData ? <EmptyChart /> : (
            <ResponsiveContainer width="100%" height={200}>
              <PieChart>
                <Pie
                  data={stats.beehivesByType as NameValue[]}
                  dataKey="value"
                  nameKey="name"
                  cx="50%"
                  cy="50%"
                  innerRadius={50}
                  outerRadius={80}
                  paddingAngle={3}
                  labelLine={false}
                  label={PieLabel}
                >
                  {stats.beehivesByType.map((_, i) => (
                    <Cell key={i} fill={MULTI[i % MULTI.length]} />
                  ))}
                </Pie>
                <Tooltip formatter={(v, n) => [v, n]} />
                <Legend wrapperStyle={{ fontSize: 12 }} />
              </PieChart>
            </ResponsiveContainer>
          )}
        </div>

        <div className="card">
          <div className="flex items-center gap-2 mb-5">
            <span className="text-xl">🪵</span>
            <h2 className="font-display text-lg font-semibold text-gray-800">Hive Materials</h2>
          </div>
          {stats.beehivesByMaterial.length === 0 ? <EmptyChart /> : (
            <ResponsiveContainer width="100%" height={200}>
              <PieChart>
                <Pie
                  data={stats.beehivesByMaterial as NameValue[]}
                  dataKey="value"
                  nameKey="name"
                  cx="50%"
                  cy="50%"
                  innerRadius={50}
                  outerRadius={80}
                  paddingAngle={3}
                  labelLine={false}
                  label={PieLabel}
                >
                  {stats.beehivesByMaterial.map((_, i) => (
                    <Cell key={i} fill={HONEY[i % HONEY.length]} />
                  ))}
                </Pie>
                <Tooltip />
                <Legend wrapperStyle={{ fontSize: 12 }} />
              </PieChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>

      {/* ── Honey level distribution ──────────────────────────────────────────── */}
      <Section title="Honey Level Distribution" icon="🍯">
        {stats.honeyLevelDistribution.length === 0 ? <EmptyChart /> : (
          <ResponsiveContainer width="100%" height={200}>
            <BarChart
              data={stats.honeyLevelDistribution as NameValue[]}
              margin={{ top: 5, right: 10, left: -10, bottom: 0 }}
            >
              <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" vertical={false} />
              <XAxis dataKey="name" tick={{ fontSize: 12, fill: '#6b7280' }} axisLine={false} tickLine={false} />
              <YAxis allowDecimals={false} tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
              <Tooltip content={<CustomTooltip />} />
              <Bar dataKey="value" name="Inspections" radius={[6, 6, 0, 0]}>
                {stats.honeyLevelDistribution.map((entry, i) => (
                  <Cell
                    key={i}
                    fill={entry.name === 'High' ? '#10b981' : entry.name === 'Medium' ? '#f59e0b' : '#ef4444'}
                  />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        )}
      </Section>

      {/* ── Beehives per apiary ───────────────────────────────────────────────── */}
      {hasApiaryData && (
        <Section title="Beehives per Apiary" icon="🏡">
          <ResponsiveContainer width="100%" height={Math.max(160, stats.apiariesByBeehiveCount.length * 44)}>
            <BarChart
              data={stats.apiariesByBeehiveCount as NameValue[]}
              layout="vertical"
              margin={{ top: 0, right: 20, left: 10, bottom: 0 }}
            >
              <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" horizontal={false} />
              <XAxis type="number" allowDecimals={false} tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
              <YAxis type="category" dataKey="name" tick={{ fontSize: 12, fill: '#6b7280' }} axisLine={false} tickLine={false} width={120} />
              <Tooltip content={<CustomTooltip />} />
              <Bar dataKey="value" name="Beehives" fill="#f59e0b" radius={[0, 6, 6, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Section>
      )}

      {/* ── Top beehives by inspection count ─────────────────────────────────── */}
      {hasTopBeehivesData && (
        <Section title="Most Inspected Beehives" icon="🔍">
          <ResponsiveContainer width="100%" height={Math.max(160, stats.topBeehivesByInspections.length * 44)}>
            <BarChart
              data={stats.topBeehivesByInspections as NameValue[]}
              layout="vertical"
              margin={{ top: 0, right: 20, left: 10, bottom: 0 }}
            >
              <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" horizontal={false} />
              <XAxis type="number" allowDecimals={false} tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
              <YAxis type="category" dataKey="name" tick={{ fontSize: 12, fill: '#6b7280' }} axisLine={false} tickLine={false} width={120} />
              <Tooltip content={<CustomTooltip />} />
              <Bar dataKey="value" name="Inspections" radius={[0, 6, 6, 0]}>
                {stats.topBeehivesByInspections.map((_, i) => (
                  <Cell key={i} fill={MULTI[i % MULTI.length]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </Section>
      )}

      {/* ── Diet section ─────────────────────────────────────────────────────── */}
      {hasDietData && (
        <div className="grid sm:grid-cols-2 gap-6 mb-6">
          <div className="card">
            <div className="flex items-center gap-2 mb-5">
              <span className="text-xl">🌿</span>
              <h2 className="font-display text-lg font-semibold text-gray-800">Diet Status</h2>
            </div>
            <ResponsiveContainer width="100%" height={200}>
              <PieChart>
                <Pie
                  data={stats.dietsByStatus as NameValue[]}
                  dataKey="value"
                  nameKey="name"
                  cx="50%"
                  cy="50%"
                  innerRadius={50}
                  outerRadius={80}
                  paddingAngle={3}
                  labelLine={false}
                  label={PieLabel}
                >
                  {stats.dietsByStatus.map((entry, i) => {
                    const c = entry.name === 'Completed' ? '#10b981'
                      : entry.name === 'In Progress' ? '#6366f1'
                      : entry.name === 'Stopped Early' ? '#ef4444'
                      : '#9ca3af'
                    return <Cell key={i} fill={c} />
                  })}
                </Pie>
                <Tooltip />
                <Legend wrapperStyle={{ fontSize: 12 }} />
              </PieChart>
            </ResponsiveContainer>
          </div>

          <div className="card">
            <div className="flex items-center gap-2 mb-5">
              <Leaf className="w-5 h-5 text-honey-500" />
              <h2 className="font-display text-lg font-semibold text-gray-800">Food Types Used</h2>
            </div>
            <ResponsiveContainer width="100%" height={200}>
              <BarChart
                data={stats.dietsByFoodType as NameValue[]}
                margin={{ top: 5, right: 10, left: -10, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" vertical={false} />
                <XAxis dataKey="name" tick={{ fontSize: 11, fill: '#6b7280' }} axisLine={false} tickLine={false} />
                <YAxis allowDecimals={false} tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
                <Tooltip content={<CustomTooltip />} />
                <Bar dataKey="value" name="Diets" radius={[6, 6, 0, 0]}>
                  {stats.dietsByFoodType.map((_, i) => (
                    <Cell key={i} fill={MULTI[i % MULTI.length]} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}

      {/* ── Todos by priority ────────────────────────────────────────────────── */}
      {hasTodoData && (
        <Section title="Tasks by Priority" icon="✅">
          <ResponsiveContainer width="100%" height={200}>
            <BarChart
              data={stats.todosByPriority as PriorityStats[]}
              margin={{ top: 5, right: 20, left: -10, bottom: 0 }}
            >
              <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" vertical={false} />
              <XAxis dataKey="priority" tick={{ fontSize: 12, fill: '#6b7280' }} axisLine={false} tickLine={false} />
              <YAxis allowDecimals={false} tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
              <Tooltip content={<CustomTooltip />} />
              <Legend wrapperStyle={{ fontSize: 12 }} />
              <Bar dataKey="total" name="Total" fill="#e5e7eb" radius={[6, 6, 0, 0]} />
              <Bar dataKey="completed" name="Completed" fill="#10b981" radius={[6, 6, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
          {/* Completion rate chips */}
          <div className="flex flex-wrap gap-3 mt-4">
            {(stats.todosByPriority as PriorityStats[]).map((p) => {
              const pct = p.total > 0 ? Math.round((p.completed / p.total) * 100) : 0
              const color = p.priority === 'High' ? 'bg-red-100 text-red-700'
                : p.priority === 'Medium' ? 'bg-honey-100 text-honey-700'
                : 'bg-gray-100 text-gray-600'
              return (
                <div key={p.priority} className={`flex items-center gap-2 px-3 py-1.5 rounded-xl text-sm font-medium ${color}`}>
                  <CheckSquare className="w-3.5 h-3.5" />
                  {p.priority}: {pct}% done
                </div>
              )
            })}
          </div>
        </Section>
      )}
    </div>
  )
}
