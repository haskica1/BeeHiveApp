import { useEffect } from 'react'
import { useNavigate, useParams, useLocation } from 'react-router-dom'
import { useFieldArray, useForm } from 'react-hook-form'
import { AlertCircle, Loader2, Plus, Trash2 } from 'lucide-react'
import { useExpense, useCreateExpense, useUpdateExpense } from '../../core/services/expenseQueries'
import { ExpenseSource } from '../../core/models'
import type { CreateExpenseItemPayload } from '../../core/models'
import { FormHeader } from '../../shared/components'

interface FormValues {
  purchaseDate: string
  totalAmount: string
  currency: string
  notes: string
  items: Array<{
    name: string
    quantity: string
    unit: string
    unitPrice: string
    totalPrice: string
  }>
}

const DEFAULT_CURRENCIES = ['BAM', 'EUR', 'USD', 'HRK']

export default function ExpenseFormPage() {
  const { id } = useParams<{ id: string }>()
  const expenseId = id ? parseInt(id) : undefined
  const isEdit = expenseId !== undefined

  const navigate = useNavigate()
  const location = useLocation()
  const prefilled = location.state as { items?: CreateExpenseItemPayload[]; source?: ExpenseSource } | null

  const { data: existing, isLoading: loadingExisting } = useExpense(expenseId ?? 0)
  const createExpense = useCreateExpense()
  const updateExpense = useUpdateExpense(expenseId ?? 0)

  const {
    register,
    control,
    handleSubmit,
    reset,
    watch,
    setValue,
  } = useForm<FormValues>({
    defaultValues: {
      purchaseDate: new Date().toISOString().split('T')[0],
      totalAmount: '',
      currency: 'BAM',
      notes: '',
      items: prefilled?.items?.map(i => ({
        name: i.name,
        quantity: String(i.quantity),
        unit: i.unit ?? '',
        unitPrice: String(i.unitPrice),
        totalPrice: String(i.totalPrice),
      })) ?? [emptyItem()],
    },
  })

  const { fields, append, remove } = useFieldArray({ control, name: 'items' })

  // Populate form when editing
  useEffect(() => {
    if (existing && isEdit) {
      reset({
        purchaseDate: existing.purchaseDate.split('T')[0],
        totalAmount: String(existing.totalAmount),
        currency: existing.currency,
        notes: existing.notes ?? '',
        items: existing.items.map(i => ({
          name: i.name,
          quantity: String(i.quantity),
          unit: i.unit ?? '',
          unitPrice: String(i.unitPrice),
          totalPrice: String(i.totalPrice),
        })),
      })
    }
  }, [existing, isEdit, reset])

  // Auto-compute total from items
  const watchedItems = watch('items')
  useEffect(() => {
    const sum = watchedItems.reduce((acc, item) => {
      const tp = parseFloat(item.totalPrice) || 0
      return acc + tp
    }, 0)
    if (sum > 0) setValue('totalAmount', sum.toFixed(2))
  }, [watchedItems, setValue])

  const isSaving = createExpense.isPending || updateExpense.isPending
  const error = createExpense.error || updateExpense.error

  async function onSubmit(values: FormValues) {
    const items: CreateExpenseItemPayload[] = values.items.map((item, i) => ({
      name: item.name.trim(),
      quantity: parseFloat(item.quantity) || 0,
      unit: item.unit.trim() || undefined,
      unitPrice: parseFloat(item.unitPrice) || 0,
      totalPrice: parseFloat(item.totalPrice) || 0,
      sortOrder: i,
    }))

    if (isEdit && expenseId) {
      await updateExpense.mutateAsync({
        purchaseDate: values.purchaseDate,
        totalAmount: parseFloat(values.totalAmount) || 0,
        currency: values.currency,
        notes: values.notes.trim() || undefined,
        items,
      })
    } else {
      await createExpense.mutateAsync({
        source: prefilled?.source ?? ExpenseSource.Manual,
        purchaseDate: values.purchaseDate,
        totalAmount: parseFloat(values.totalAmount) || 0,
        currency: values.currency,
        notes: values.notes.trim() || undefined,
        items,
      })
    }

    navigate('/expenses')
  }

  if (isEdit && loadingExisting) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-6 h-6 animate-spin text-honey-500" />
      </div>
    )
  }

  return (
    <div className="max-w-2xl mx-auto">
      <FormHeader
        icon="🧾"
        title={isEdit ? 'Edit Expense' : 'New Expense'}
        onBack={() => navigate('/expenses')}
        backLabel="Back to Expenses"
      />

      <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-sm dark:shadow-none border border-honey-100 dark:border-slate-800 px-8 py-8">
        {error && (
          <div className="flex items-start gap-2 bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/30 text-red-700 dark:text-red-300 rounded-xl px-4 py-3 text-sm mb-5">
            <AlertCircle className="w-4 h-4 mt-0.5 shrink-0" />
            Failed to save expense. Please check your inputs and try again.
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Date + Currency row */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1.5">
                Purchase Date <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                {...register('purchaseDate', { required: true })}
                className="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-slate-700 text-sm outline-none bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-2 focus:ring-honey-100 transition-all"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1.5">Currency</label>
              <select
                {...register('currency')}
                className="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-slate-700 text-sm outline-none bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-2 focus:ring-honey-100 transition-all"
              >
                {DEFAULT_CURRENCIES.map(c => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>
            </div>
          </div>

          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Notes</label>
            <input
              type="text"
              placeholder="e.g. Spring supplies, local store"
              {...register('notes')}
              className="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-slate-700 text-sm outline-none bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-2 focus:ring-honey-100 transition-all"
            />
          </div>

          {/* Items */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-300">
                Items <span className="text-red-500">*</span>
              </label>
              <button
                type="button"
                onClick={() => append(emptyItem())}
                className="flex items-center gap-1 text-xs text-honey-600 dark:text-honey-400 hover:text-honey-700 dark:hover:text-honey-300 font-medium transition-colors"
              >
                <Plus className="w-3.5 h-3.5" />
                Add item
              </button>
            </div>

            {/* Column headers */}
            <div className="grid grid-cols-[2fr_1fr_0.8fr_1fr_1fr_auto] gap-2 px-1 mb-1">
              {['Product', 'Qty', 'Unit', 'Unit price', 'Total', ''].map(h => (
                <span key={h} className="text-xs font-medium text-gray-400 dark:text-slate-500">{h}</span>
              ))}
            </div>

            <div className="space-y-2">
              {fields.map((field, index) => (
                <div key={field.id} className="grid grid-cols-[2fr_1fr_0.8fr_1fr_1fr_auto] gap-2 items-center">
                  <input
                    type="text"
                    placeholder="Sugar"
                    {...register(`items.${index}.name`, { required: true })}
                    className="px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 text-sm outline-none bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-1 focus:ring-honey-100 transition-all"
                  />
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="25"
                    {...register(`items.${index}.quantity`, { required: true })}
                    className="px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 text-sm outline-none bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-1 focus:ring-honey-100 transition-all"
                  />
                  <input
                    type="text"
                    placeholder="kg"
                    {...register(`items.${index}.unit`)}
                    className="px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 text-sm outline-none bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-1 focus:ring-honey-100 transition-all"
                  />
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="1.60"
                    {...register(`items.${index}.unitPrice`)}
                    className="px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 text-sm outline-none bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-1 focus:ring-honey-100 transition-all"
                  />
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="40.00"
                    {...register(`items.${index}.totalPrice`)}
                    className="px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 text-sm outline-none bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-1 focus:ring-honey-100 transition-all"
                  />
                  <button
                    type="button"
                    onClick={() => remove(index)}
                    disabled={fields.length === 1}
                    className="p-1.5 rounded-lg text-gray-300 dark:text-slate-600 hover:text-red-400 dark:hover:text-red-400 hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
                    aria-label="Remove item"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>
          </div>

          {/* Total */}
          <div className="flex items-center justify-end gap-3 pt-2 border-t border-gray-100 dark:border-slate-800">
            <span className="text-sm text-gray-500 dark:text-slate-400">Total amount</span>
            <div className="flex items-center gap-2">
              <input
                type="number"
                step="0.01"
                min="0"
                {...register('totalAmount', { required: true })}
                className="w-28 px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 text-sm font-semibold outline-none bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-1 focus:ring-honey-100 transition-all text-right"
              />
              <span className="text-sm font-medium text-gray-600 dark:text-slate-300">{watch('currency')}</span>
            </div>
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={() => navigate('/expenses')}
              className="flex-1 px-4 py-3 rounded-xl border border-gray-200 dark:border-slate-700 text-sm font-medium text-gray-700 dark:text-slate-200 hover:bg-gray-50 dark:hover:bg-slate-800 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSaving}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-xl bg-honey-500 hover:bg-honey-600 text-white text-sm font-semibold disabled:opacity-60 transition-colors"
            >
              {isSaving && <Loader2 className="w-4 h-4 animate-spin" />}
              {isEdit ? 'Save Changes' : 'Save Expense'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

function emptyItem() {
  return { name: '', quantity: '', unit: '', unitPrice: '', totalPrice: '' }
}
