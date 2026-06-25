// ── User / Auth ───────────────────────────────────────────────────────────────

export enum UserRole {
  ApiaryAdmin       = 'ApiaryAdmin',
  SystemAdmin       = 'SystemAdmin',
  OrganizationAdmin = 'OrganizationAdmin',
  Beekeeper         = 'Beekeeper',
}

// ── Enums ─────────────────────────────────────────────────────────────────────

export enum BeehiveType {
  Langstroth   = 1,
  DadantBlatt  = 2,
  Warré        = 3,
  TopBar       = 4,
  Other        = 5,
}

export const BeehiveTypeLabels: Record<BeehiveType, string> = {
  [BeehiveType.Langstroth]:  'LR (Langstroth-Rutova) košnica',
  [BeehiveType.DadantBlatt]: 'DB (Dadan-Blatt) košnica',
  [BeehiveType.Warré]:       'AŽ (Alberti-Žnideršič) košnica',
  [BeehiveType.TopBar]:      'Pološka košnica',
  [BeehiveType.Other]:       'Ostalo',
}

export enum BeehiveMaterial {
  Wood        = 1,
  Plastic     = 2,
  Polystyrene = 3,
}

export const BeehiveMaterialLabels: Record<BeehiveMaterial, string> = {
  [BeehiveMaterial.Wood]:        'Drvo',
  [BeehiveMaterial.Plastic]:     'Plastika',
  [BeehiveMaterial.Polystyrene]: 'Stiropor',
}

export enum HoneyLevel {
  Low    = 1,
  Medium = 2,
  High   = 3,
}

export const HoneyLevelLabels: Record<HoneyLevel, string> = {
  [HoneyLevel.Low]:    'Nisko',
  [HoneyLevel.Medium]: 'Srednje',
  [HoneyLevel.High]:   'Visoko',
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
  createdByName?: string
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
  currentTemperature?: number
  currentApparentTemperature?: number
  currentWeatherCode?: number
  currentWindSpeed?: number
  currentHumidity?: number
  daily: DailyWeather[]
}

export interface DailyWeather {
  date: string
  maxTemp?: number
  minTemp?: number
  apparentTempMax?: number
  apparentTempMin?: number
  weatherCode?: number
  precipitationSum?: number
  maxWindSpeed?: number
  precipitationProbability?: number
  sunrise?: string
  sunset?: string
  uvIndexMax?: number
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
  createdByName?: string
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

export interface ParseVoiceResult {
  transcript?: string | null
  date?: string | null
  temperature?: number | null
  honeyLevel?: HoneyLevel | null
  broodStatus?: string | null
  notes?: string | null
}

// ── Todo ──────────────────────────────────────────────────────────────────────

export enum TodoPriority {
  Low    = 1,
  Medium = 2,
  High   = 3,
}

export const TodoPriorityLabels: Record<TodoPriority, string> = {
  [TodoPriority.Low]:    'Nizak',
  [TodoPriority.Medium]: 'Srednji',
  [TodoPriority.High]:   'Visok',
}

export interface AssignableUser {
  id: number
  fullName: string
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
  createdByName?: string
  assignedToId?: number
  assignedToName?: string
  createdAt: string
}

export interface CreateTodoPayload {
  title: string
  notes?: string
  dueDate?: string | null
  priority: TodoPriority
  assignedToId?: number | null
  apiaryId?: number
  beehiveId?: number
}

export interface UpdateTodoPayload {
  title: string
  notes?: string
  dueDate?: string | null
  priority: TodoPriority
  isCompleted: boolean
  assignedToId?: number | null
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
  [DietReason.LackOfFood]:               'Nedostatak hrane',
  [DietReason.WinterFeeding]:            'Zimsko hranjenje',
  [DietReason.SpringStimulation]:        'Proljetna stimulacija',
  [DietReason.NewSwarmSupport]:          'Podrška novom roju',
  [DietReason.PostHarvestRecovery]:      'Oporavak nakon berbe',
  [DietReason.DroughtConditions]:        'Uvjeti suše',
  [DietReason.WeakColonySupport]:        'Podrška slaboj koloniji',
  [DietReason.QueenIntroductionSupport]: 'Podrška uvođenju matice',
  [DietReason.Custom]:                   'Vlastito',
}

export enum FoodType {
  SugarSyrup     = 1,
  Fondant        = 2,
  Pollen         = 3,
  ProteinPatties = 4,
  Custom         = 5,
}

export const FoodTypeLabels: Record<FoodType, string> = {
  [FoodType.SugarSyrup]:     'Šećerni sirup',
  [FoodType.Fondant]:        'Fondan',
  [FoodType.Pollen]:         'Polen',
  [FoodType.ProteinPatties]: 'Proteinski kolači',
  [FoodType.Custom]:         'Vlastito',
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
  createdByName?: string
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

// ── Calendar ──────────────────────────────────────────────────────────────────

export interface CalendarTodo {
  id: number
  title: string
  notes?: string
  dueDate?: string
  priority: TodoPriority
  priorityName: string
  isCompleted: boolean
  apiaryId?: number
  apiaryName?: string
  beehiveId?: number
  beehiveName?: string
}

export interface CalendarFeedingEntry {
  id: number
  scheduledDate: string
  status: FeedingEntryStatus
  statusName: string
  dietId: number
  dietName: string
  beehiveId: number
  beehiveName: string
  foodTypeName: string
}

export interface CalendarEventsResponse {
  todos: CalendarTodo[]
  feedingEntries: CalendarFeedingEntry[]
}

// ── Admin ─────────────────────────────────────────────────────────────────────

export interface AdminOrganization {
  id: number
  name: string
  description?: string
  userCount: number
  apiaryCount: number
  createdByName?: string
  createdAt: string
}

export interface CreateOrganizationPayload {
  name: string
  description?: string
}

export interface UpdateOrganizationPayload {
  name: string
  description?: string
}

export interface AdminUser {
  id: number
  firstName: string
  lastName: string
  email: string
  role: string
  organizationId?: number
  organizationName?: string
  apiaryId?: number
  apiaryName?: string
  assignedBeehiveIds: number[]
  createdAt: string
}

export interface AdminApiaryListItem {
  id: number
  name: string
}

export interface AdminBeehiveListItem {
  id: number
  name: string
  apiaryName: string
}

export interface CreateAdminUserPayload {
  firstName: string
  lastName: string
  email: string
  password: string
  role: string
  organizationId?: number | null
  apiaryId?: number | null
  assignedBeehiveIds: number[]
}

export interface UpdateAdminUserPayload {
  firstName: string
  lastName: string
  email: string
  role: string
  organizationId?: number | null
  apiaryId?: number | null
  assignedBeehiveIds: number[]
}

// ── Org Management ────────────────────────────────────────────────────────────

export interface OrgMember {
  id: number
  firstName: string
  lastName: string
  email: string
  role: string
  apiaryId?: number
  apiaryName?: string
  assignedBeehiveIds: number[]
  assignedBeehiveNames: string[]
}

export interface OrgAvailableBeehive {
  id: number
  name: string
  apiaryName: string
}

export interface OrgAvailableApiary {
  id: number
  name: string
}

export interface UpdateBeehiveAssignmentsPayload {
  beehiveIds: number[]
}

export interface UpdateApiaryAssignmentPayload {
  apiaryId: number | null
}

export interface CreateOrgMemberPayload {
  firstName: string
  lastName: string
  email: string
  password: string
  role: string
  apiaryId?: number | null
  assignedBeehiveIds: number[]
}

// ── Expenses ──────────────────────────────────────────────────────────────────

export enum ExpenseSource {
  Manual      = 1,
  ReceiptScan = 2,
}

export const ExpenseSourceLabels: Record<ExpenseSource, string> = {
  [ExpenseSource.Manual]:      'Ručno',
  [ExpenseSource.ReceiptScan]: 'Skeniranje računa',
}

export interface ExpenseItem {
  id: number
  name: string
  quantity: number
  unit?: string
  unitPrice: number
  totalPrice: number
  sortOrder: number
}

export interface Expense {
  id: number
  source: ExpenseSource
  sourceName: string
  purchaseDate: string
  totalAmount: number
  currency: string
  notes?: string
  itemCount: number
  createdByName?: string
  createdAt: string
}

export interface ExpenseDetail extends Expense {
  items: ExpenseItem[]
}

export interface CreateExpenseItemPayload {
  name: string
  quantity: number
  unit?: string
  unitPrice: number
  totalPrice: number
  sortOrder: number
}

export interface CreateExpensePayload {
  source: ExpenseSource
  purchaseDate: string
  totalAmount: number
  currency: string
  notes?: string
  items: CreateExpenseItemPayload[]
}

export interface UpdateExpensePayload {
  purchaseDate: string
  totalAmount: number
  currency: string
  notes?: string
  items: CreateExpenseItemPayload[]
}

// ── API Error shape ───────────────────────────────────────────────────────────

export interface ApiError {
  type: string
  title: string
  status: number
  errors: Record<string, string[]>
}
