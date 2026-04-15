// ── Enums ─────────────────────────────────────────────────────────────────────

export enum BeehiveType {
  Langstroth   = 1,
  DadantBlatt  = 2,
  Warré        = 3,
  TopBar       = 4,
  Other        = 5,
}

export const BeehiveTypeLabels: Record<BeehiveType, string> = {
  [BeehiveType.Langstroth]:  'Langstroth',
  [BeehiveType.DadantBlatt]: 'Dadant-Blatt',
  [BeehiveType.Warré]:       'Warré',
  [BeehiveType.TopBar]:      'Top Bar',
  [BeehiveType.Other]:       'Other',
}

export enum BeehiveMaterial {
  Wood        = 1,
  Plastic     = 2,
  Polystyrene = 3,
}

export const BeehiveMaterialLabels: Record<BeehiveMaterial, string> = {
  [BeehiveMaterial.Wood]:        'Wood',
  [BeehiveMaterial.Plastic]:     'Plastic',
  [BeehiveMaterial.Polystyrene]: 'Polystyrene',
}

export enum HoneyLevel {
  Low    = 1,
  Medium = 2,
  High   = 3,
}

export const HoneyLevelLabels: Record<HoneyLevel, string> = {
  [HoneyLevel.Low]:    'Low',
  [HoneyLevel.Medium]: 'Medium',
  [HoneyLevel.High]:   'High',
}

// ── Apiary ────────────────────────────────────────────────────────────────────

export interface Apiary {
  id: number
  name: string
  description?: string
  latitude?: number
  longitude?: number
  hasLocation: boolean
  beehiveCount: number
  createdAt: string
}

export interface ApiaryDetail extends Apiary {
  beehives: Beehive[]
}

export interface CreateApiaryPayload {
  name: string
  description?: string
  latitude?: number | null
  longitude?: number | null
}

export interface UpdateApiaryPayload extends CreateApiaryPayload {}

// ── Weather ───────────────────────────────────────────────────────────────────

export interface WeatherForecast {
  latitude: number
  longitude: number
  timezone: string
  daily: DailyWeather[]
}

export interface DailyWeather {
  date: string
  maxTemp?: number
  minTemp?: number
  weatherCode?: number
  precipitationSum?: number
  maxWindSpeed?: number
  precipitationProbability?: number
}

// ── Beehive ───────────────────────────────────────────────────────────────────

export interface Beehive {
  id: number
  name: string
  type: BeehiveType
  typeName: string
  material: BeehiveMaterial
  materialName: string
  dateCreated: string
  notes?: string
  apiaryId: number
  inspectionCount: number
  createdAt: string
  uniqueId?: string
  qrCodeBase64?: string
}

export interface BeehiveDetail extends Beehive {
  inspections: Inspection[]
}

export interface CreateBeehivePayload {
  name: string
  type: BeehiveType
  material: BeehiveMaterial
  dateCreated: string
  notes?: string
  apiaryId: number
}

export interface UpdateBeehivePayload extends CreateBeehivePayload {}

// ── Inspection ────────────────────────────────────────────────────────────────

export interface Inspection {
  id: number
  date: string
  temperature?: number
  honeyLevel: HoneyLevel
  honeyLevelName: string
  broodStatus?: string
  notes?: string
  beehiveId: number
  createdAt: string
}

export interface CreateInspectionPayload {
  date: string
  temperature?: number
  honeyLevel: HoneyLevel
  broodStatus?: string
  notes?: string
  beehiveId: number
}

export interface UpdateInspectionPayload extends CreateInspectionPayload {}

// ── Todo ──────────────────────────────────────────────────────────────────────

export enum TodoPriority {
  Low    = 1,
  Medium = 2,
  High   = 3,
}

export const TodoPriorityLabels: Record<TodoPriority, string> = {
  [TodoPriority.Low]:    'Low',
  [TodoPriority.Medium]: 'Medium',
  [TodoPriority.High]:   'High',
}

export interface Todo {
  id: number
  title: string
  notes?: string
  dueDate?: string
  priority: TodoPriority
  priorityName: string
  isCompleted: boolean
  completedAt?: string
  apiaryId?: number
  beehiveId?: number
  createdAt: string
}

export interface CreateTodoPayload {
  title: string
  notes?: string
  dueDate?: string | null
  priority: TodoPriority
  apiaryId?: number
  beehiveId?: number
}

export interface UpdateTodoPayload {
  title: string
  notes?: string
  dueDate?: string | null
  priority: TodoPriority
  isCompleted: boolean
}

// ── API Error shape ───────────────────────────────────────────────────────────

export interface ApiError {
  type: string
  title: string
  status: number
  errors: Record<string, string[]>
}
