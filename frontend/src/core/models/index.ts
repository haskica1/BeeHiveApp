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

// ── Diet ──────────────────────────────────────────────────────────────────────

export enum DietStatus {
  NotStarted   = 1,
  InProgress   = 2,
  Completed    = 3,
  StoppedEarly = 4,
}

export enum FeedingEntryStatus {
  Pending   = 1,
  Completed = 2,
}

export enum DietReason {
  LackOfFood               = 1,
  WinterFeeding            = 2,
  SpringStimulation        = 3,
  NewSwarmSupport          = 4,
  PostHarvestRecovery      = 5,
  DroughtConditions        = 6,
  WeakColonySupport        = 7,
  QueenIntroductionSupport = 8,
  Custom                   = 9,
}

export const DietReasonLabels: Record<DietReason, string> = {
  [DietReason.LackOfFood]:               'Lack of Food',
  [DietReason.WinterFeeding]:            'Winter Feeding',
  [DietReason.SpringStimulation]:        'Spring Stimulation',
  [DietReason.NewSwarmSupport]:          'New Swarm Support',
  [DietReason.PostHarvestRecovery]:      'Post-Harvest Recovery',
  [DietReason.DroughtConditions]:        'Drought Conditions',
  [DietReason.WeakColonySupport]:        'Weak Colony Support',
  [DietReason.QueenIntroductionSupport]: 'Queen Introduction Support',
  [DietReason.Custom]:                   'Custom',
}

export enum FoodType {
  SugarSyrup     = 1,
  Fondant        = 2,
  Pollen         = 3,
  ProteinPatties = 4,
  Custom         = 5,
}

export const FoodTypeLabels: Record<FoodType, string> = {
  [FoodType.SugarSyrup]:     'Sugar Syrup',
  [FoodType.Fondant]:        'Fondant',
  [FoodType.Pollen]:         'Pollen',
  [FoodType.ProteinPatties]: 'Protein Patties',
  [FoodType.Custom]:         'Custom',
}

export interface FeedingEntry {
  id: number
  scheduledDate: string
  status: FeedingEntryStatus
  statusName: string
  completionDate?: string
  dietId: number
}

export interface Diet {
  id: number
  name: string
  startDate: string
  reason: DietReason
  reasonName: string
  customReason?: string
  durationDays: number
  frequencyDays: number
  foodType: FoodType
  foodTypeName: string
  customFoodType?: string
  status: DietStatus
  statusName: string
  earlyCompletionComment?: string
  beehiveId: number
  totalEntries: number
  completedEntries: number
  createdAt: string
}

export interface DietDetail extends Diet {
  feedingEntries: FeedingEntry[]
}

export interface CreateDietPayload {
  name: string
  startDate: string
  reason: DietReason
  customReason?: string
  durationDays: number
  frequencyDays: number
  foodType: FoodType
  customFoodType?: string
  beehiveId: number
}

export interface UpdateDietPayload {
  name: string
  startDate: string
  reason: DietReason
  customReason?: string
  durationDays: number
  frequencyDays: number
  foodType: FoodType
  customFoodType?: string
}

export interface CompleteEarlyPayload {
  comment: string
}

// ── API Error shape ───────────────────────────────────────────────────────────

export interface ApiError {
  type: string
  title: string
  status: number
  errors: Record<string, string[]>
}
