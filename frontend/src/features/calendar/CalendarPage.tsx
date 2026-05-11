import { useMemo, useState } from 'react'
import {
  addDays, addMonths, endOfMonth, endOfWeek, format,
  isSameDay, isSameMonth, isToday, isPast, parseISO,
  startOfMonth, startOfWeek, subMonths,
} from 'date-fns'
import { ChevronLeft, ChevronRight as ChevronRightIcon, CheckSquare, Droplets, CalendarDays, AlertCircle, Clock } from 'lucide-react'
import { Link } from 'react-router-dom'
import clsx from 'clsx'
import { useCalendarEvents } from '../../core/services/queries'
import { FeedingEntryStatus, TodoPriority } from '../../core/models'
import type { CalendarTodo, CalendarFeedingEntry } from '../../core/models'
import LoadingSpinner from '../../shared/components/LoadingSpinner'
import ErrorMessage from '../../shared/components/ErrorMessage'

// ── Types ──────────────────────────────────────────────────────────────────────

interface DayEvents {
  todos: CalendarTodo[]
  feedings: CalendarFeedingEntry[]
}

// ── Main Page ──────────────────────────────────────────────────────────────────

export default function CalendarPage() {
  const [currentMonth, setCurrentMonth] = useState(new Date())
  const [selectedDay, setSelectedDay] = useState<Date>(new Date())

  const { data, isLoading, isError } = useCalendarEvents()

  const calendarDays = useMemo(() => {
    const start = startOfWeek(startOfMonth(currentMonth), { weekStartsOn: 1 })
    const end   = endOfWeek(endOfMonth(currentMonth),     { weekStartsOn: 1 })
    const days: Date[] = []
    let cur = start
    while (cur <= end) {
      days.push(cur)
      cur = addDays(cur, 1)
    }
    return days
  }, [currentMonth])

  const eventsByDate = useMemo<Map<string, DayEvents>>(() => {
    const map = new Map<string, DayEvents>()
    if (!data) return map

    for (const todo of data.todos) {
      if (!todo.dueDate) continue
      const key = todo.dueDate.slice(0, 10)
      if (!map.has(key)) map.set(key, { todos: [], feedings: [] })
      map.get(key)!.todos.push(todo)
    }
    for (const entry of data.feedingEntries) {
      const key = entry.scheduledDate.slice(0, 10)
      if (!map.has(key)) map.set(key, { todos: [], feedings: [] })
      map.get(key)!.feedings.push(entry)
    }
    return map
  }, [data])

  const monthSummary = useMemo(() => {
    if (!data) return { todos: 0, feedings: 0 }
    const prefix = format(currentMonth, 'yyyy-MM')
    let todos = 0, feedings = 0
    for (const [key, events] of eventsByDate) {
      if (!key.startsWith(prefix)) continue
      todos    += events.todos.length
      feedings += events.feedings.length
    }
    return { todos, feedings }
  }, [data, eventsByDate, currentMonth])

  const selectedKey    = format(selectedDay, 'yyyy-MM-dd')
  const selectedEvents = eventsByDate.get(selectedKey) ?? { todos: [], feedings: [] }

  if (isLoading) return <LoadingSpinner message="Loading calendar…" />
  if (isError)   return <ErrorMessage message="Failed to load calendar events." />

  return (
    <div className="space-y-6 animate-fade-in">

      {/* ── Page header ─────────────────────────────────────────────────────── */}
      <div className="flex items-start justify-between">
        <div>
          <h1 className="font-display text-2xl font-bold text-honey-900">Calendar</h1>
          <p className="text-sm text-gray-500 mt-0.5">Your tasks and feeding schedules</p>
        </div>
        {/* Month summary pills */}
        <div className="hidden sm:flex items-center gap-2">
          {monthSummary.todos > 0 && (
            <span className="flex items-center gap-1.5 px-3 py-1 rounded-full bg-honey-100 text-honey-800 text-xs font-semibold">
              <CheckSquare className="w-3.5 h-3.5" />
              {monthSummary.todos} task{monthSummary.todos !== 1 ? 's' : ''}
            </span>
          )}
          {monthSummary.feedings > 0 && (
            <span className="flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-100 text-emerald-800 text-xs font-semibold">
              <Droplets className="w-3.5 h-3.5" />
              {monthSummary.feedings} feeding{monthSummary.feedings !== 1 ? 's' : ''}
            </span>
          )}
        </div>
      </div>

      {/* ── Calendar card ────────────────────────────────────────────────────── */}
      <div className="card p-4 sm:p-6">

        {/* Month navigation */}
        <div className="flex items-center justify-between mb-5">
          <button
            onClick={() => setCurrentMonth(m => subMonths(m, 1))}
            className="p-2 rounded-xl text-gray-500 hover:bg-honey-100 hover:text-honey-800 transition-colors"
            aria-label="Previous month"
          >
            <ChevronLeft className="w-5 h-5" />
          </button>
          <h2 className="font-display text-xl font-bold text-gray-800 select-none">
            {format(currentMonth, 'MMMM yyyy')}
          </h2>
          <button
            onClick={() => setCurrentMonth(m => addMonths(m, 1))}
            className="p-2 rounded-xl text-gray-500 hover:bg-honey-100 hover:text-honey-800 transition-colors"
            aria-label="Next month"
          >
            <ChevronRightIcon className="w-5 h-5" />
          </button>
        </div>

        {/* Day-of-week header */}
        <div className="grid grid-cols-7 mb-2">
          {['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'].map(d => (
            <div key={d} className="text-center text-[11px] font-bold text-gray-400 uppercase tracking-wide py-1">
              {d}
            </div>
          ))}
        </div>

        {/* Calendar grid */}
        <div className="grid grid-cols-7 gap-1">
          {calendarDays.map(day => {
            const key          = format(day, 'yyyy-MM-dd')
            const events       = eventsByDate.get(key)
            const todosCount   = events?.todos.length ?? 0
            const feedingCount = events?.feedings.length ?? 0
            const inMonth      = isSameMonth(day, currentMonth)
            const selected     = isSameDay(day, selectedDay)
            const todayDate    = isToday(day)

            const hasOverdue = events?.todos.some(t => !t.isCompleted && isPast(parseISO(t.dueDate!)))
              || events?.feedings.some(f => f.status !== FeedingEntryStatus.Completed && isPast(parseISO(f.scheduledDate)))

            return (
              <button
                key={key}
                onClick={() => setSelectedDay(day)}
                className={clsx(
                  'relative flex flex-col items-center justify-start pt-1.5 pb-2 px-0.5 rounded-xl transition-all min-h-[56px]',
                  !inMonth && 'opacity-25 pointer-events-none',
                  selected
                    ? 'bg-honey-500 shadow-honey shadow-sm'
                    : todayDate
                    ? 'bg-honey-50 ring-2 ring-honey-400 ring-inset'
                    : 'hover:bg-honey-50',
                )}
              >
                {/* Day number */}
                <span className={clsx(
                  'text-sm font-semibold leading-none',
                  selected  ? 'text-white'       :
                  todayDate ? 'text-honey-700'   :
                              'text-gray-700',
                )}>
                  {format(day, 'd')}
                </span>

                {/* Event dots */}
                {(todosCount > 0 || feedingCount > 0) && (
                  <div className="flex items-center justify-center gap-0.5 mt-1.5 flex-wrap">
                    {todosCount > 0 && (
                      <span className={clsx(
                        'w-1.5 h-1.5 rounded-full flex-shrink-0',
                        selected
                          ? 'bg-white/80'
                          : hasOverdue
                          ? 'bg-red-500'
                          : 'bg-honey-500',
                      )} />
                    )}
                    {feedingCount > 0 && (
                      <span className={clsx(
                        'w-1.5 h-1.5 rounded-full flex-shrink-0',
                        selected ? 'bg-white/70' : 'bg-emerald-500',
                      )} />
                    )}
                  </div>
                )}
              </button>
            )
          })}
        </div>

        {/* Legend */}
        <div className="flex items-center gap-5 mt-5 pt-4 border-t border-gray-100">
          <div className="flex items-center gap-1.5">
            <span className="w-2.5 h-2.5 rounded-full bg-honey-500" />
            <span className="text-xs text-gray-500">Tasks</span>
          </div>
          <div className="flex items-center gap-1.5">
            <span className="w-2.5 h-2.5 rounded-full bg-emerald-500" />
            <span className="text-xs text-gray-500">Diet feedings</span>
          </div>
          <div className="flex items-center gap-1.5">
            <span className="w-2.5 h-2.5 rounded-full bg-red-500" />
            <span className="text-xs text-gray-500">Overdue</span>
          </div>
        </div>
      </div>

      {/* ── Selected-day panel ────────────────────────────────────────────────── */}
      <div className="card p-4 sm:p-6 animate-fade-in">
        <div className="flex items-center gap-2 mb-4">
          <CalendarDays className="w-4 h-4 text-honey-600" />
          <h3 className="font-display text-base font-bold text-gray-800">
            {format(selectedDay, 'EEEE, MMMM d, yyyy')}
          </h3>
          {isToday(selectedDay) && (
            <span className="text-xs font-semibold px-2 py-0.5 bg-honey-100 text-honey-700 rounded-full">
              Today
            </span>
          )}
        </div>

        {selectedEvents.todos.length === 0 && selectedEvents.feedings.length === 0 ? (
          <EmptyDay />
        ) : (
          <div className="space-y-5">
            {selectedEvents.todos.length > 0 && (
              <section>
                <SectionLabel icon={<CheckSquare className="w-3.5 h-3.5" />} color="honey" count={selectedEvents.todos.length}>
                  Tasks
                </SectionLabel>
                <div className="space-y-2 mt-2">
                  {selectedEvents.todos.map(todo => <TodoCard key={todo.id} todo={todo} />)}
                </div>
              </section>
            )}
            {selectedEvents.feedings.length > 0 && (
              <section>
                <SectionLabel icon={<Droplets className="w-3.5 h-3.5" />} color="emerald" count={selectedEvents.feedings.length}>
                  Diet Feedings
                </SectionLabel>
                <div className="space-y-2 mt-2">
                  {selectedEvents.feedings.map(f => <FeedingCard key={f.id} entry={f} />)}
                </div>
              </section>
            )}
          </div>
        )}
      </div>

    </div>
  )
}

// ── Helper components ──────────────────────────────────────────────────────────

function EmptyDay() {
  return (
    <div className="flex flex-col items-center justify-center py-10 text-gray-400">
      <CalendarDays className="w-10 h-10 mb-3 opacity-30" />
      <p className="text-sm font-medium">No events on this day</p>
      <p className="text-xs mt-0.5 opacity-70">Select another date to browse your schedule</p>
    </div>
  )
}

function SectionLabel({
  icon, color, count, children,
}: {
  icon: React.ReactNode
  color: 'honey' | 'emerald'
  count: number
  children: React.ReactNode
}) {
  const cls = color === 'honey'
    ? 'text-honey-700 bg-honey-50 border-honey-100'
    : 'text-emerald-700 bg-emerald-50 border-emerald-100'

  return (
    <div className={clsx('inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg border text-xs font-bold uppercase tracking-wide', cls)}>
      {icon}
      {children}
      <span className="ml-0.5 opacity-70">({count})</span>
    </div>
  )
}

function TodoCard({ todo }: { todo: CalendarTodo }) {
  const overdue = !todo.isCompleted && todo.dueDate && isPast(parseISO(todo.dueDate))

  const priorityConfig = {
    [TodoPriority.High]:   { label: 'High',   cls: 'text-red-600 bg-red-50 border-red-100' },
    [TodoPriority.Medium]: { label: 'Medium', cls: 'text-amber-600 bg-amber-50 border-amber-100' },
    [TodoPriority.Low]:    { label: 'Low',    cls: 'text-blue-600 bg-blue-50 border-blue-100' },
  }
  const pc = priorityConfig[todo.priority] ?? priorityConfig[TodoPriority.Medium]

  const context     = todo.beehiveName ?? todo.apiaryName
  const contextKind = todo.beehiveId ? 'Beehive' : 'Apiary'

  return (
    <div className={clsx(
      'flex items-start gap-3 p-3.5 rounded-xl border transition-all',
      todo.isCompleted  ? 'bg-gray-50 border-gray-100 opacity-60' :
      overdue           ? 'bg-red-50 border-red-200' :
                          'bg-white border-honey-100 hover:border-honey-200',
    )}>
      {/* Checkbox visual */}
      <div className={clsx(
        'flex-shrink-0 mt-0.5 w-4 h-4 rounded border-2 flex items-center justify-center',
        todo.isCompleted ? 'bg-green-500 border-green-500' : 'border-gray-300',
      )}>
        {todo.isCompleted && <span className="text-white text-[10px] font-bold">✓</span>}
      </div>

      <div className="min-w-0 flex-1">
        <p className={clsx(
          'text-sm font-semibold leading-snug',
          todo.isCompleted ? 'line-through text-gray-400' :
          overdue          ? 'text-red-800' :
                             'text-gray-800',
        )}>
          {todo.title}
        </p>

        <div className="flex flex-wrap items-center gap-1.5 mt-1.5">
          {context && (
            <span className="text-xs text-gray-500">
              {contextKind}: <span className="font-medium text-gray-700">{context}</span>
            </span>
          )}
          <span className={clsx('text-xs px-2 py-0.5 rounded-full border font-medium', pc.cls)}>
            {pc.label}
          </span>
          {overdue && (
            <span className="flex items-center gap-0.5 text-xs text-red-600 font-semibold">
              <AlertCircle className="w-3 h-3" /> Overdue
            </span>
          )}
          {todo.isCompleted && (
            <span className="text-xs text-green-600 font-medium">Completed</span>
          )}
        </div>

        {todo.notes && (
          <p className="text-xs text-gray-400 mt-1.5 line-clamp-2 leading-relaxed">{todo.notes}</p>
        )}
      </div>
    </div>
  )
}

function FeedingCard({ entry }: { entry: CalendarFeedingEntry }) {
  const completed = entry.status === FeedingEntryStatus.Completed
  const overdue   = !completed && isPast(parseISO(entry.scheduledDate))

  return (
    <Link
      to={`/diets/${entry.dietId}`}
      className={clsx(
        'flex items-start gap-3 p-3.5 rounded-xl border transition-all group',
        completed ? 'bg-gray-50 border-gray-100 opacity-60' :
        overdue   ? 'bg-red-50 border-red-200 hover:border-red-300' :
                    'bg-emerald-50 border-emerald-200 hover:border-emerald-300',
      )}
    >
      <Droplets className={clsx(
        'w-4 h-4 mt-0.5 flex-shrink-0 transition-transform group-hover:scale-110',
        completed ? 'text-gray-400' :
        overdue   ? 'text-red-500'  :
                    'text-emerald-600',
      )} />

      <div className="min-w-0 flex-1">
        <p className={clsx(
          'text-sm font-semibold leading-snug',
          completed ? 'line-through text-gray-400' :
          overdue   ? 'text-red-800' :
                      'text-emerald-900',
        )}>
          {entry.dietName}
        </p>

        <div className="flex flex-wrap items-center gap-1.5 mt-1.5">
          <span className="text-xs text-gray-500">
            Beehive: <span className="font-medium text-gray-700">{entry.beehiveName}</span>
          </span>
          <span className="text-xs text-gray-400">·</span>
          <span className="text-xs text-gray-500">{entry.foodTypeName}</span>
        </div>

        <div className="flex items-center gap-1.5 mt-1.5">
          {completed ? (
            <span className="flex items-center gap-1 text-xs text-green-600 font-semibold">
              <span className="w-1.5 h-1.5 rounded-full bg-green-500" />
              Completed
            </span>
          ) : overdue ? (
            <span className="flex items-center gap-1 text-xs text-red-600 font-semibold">
              <AlertCircle className="w-3 h-3" /> Overdue
            </span>
          ) : (
            <span className="flex items-center gap-1 text-xs text-emerald-700 font-semibold">
              <Clock className="w-3 h-3" /> Pending
            </span>
          )}
        </div>
      </div>

      <ChevronRightIcon className="w-4 h-4 text-gray-300 group-hover:text-gray-400 flex-shrink-0 mt-0.5 transition-colors" />
    </Link>
  )
}
