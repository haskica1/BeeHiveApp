import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { format } from 'date-fns'
import { AlertCircle, Camera, Loader2, PencilLine, Plus, ReceiptText, Trash2 } from 'lucide-react'
import { useExpenses, useDeleteExpense } from '../../core/services/expenseQueries'
import { ExpenseSource, ExpenseSourceLabels } from '../../core/models'
import type { Expense } from '../../core/models'

export default function ExpensesPage() {
  const navigate = useNavigate()
  const { data: expenses = [], isLoading } = useExpenses()
  const deleteExpense = useDeleteExpense()
  const [deletingId, setDeletingId] = useState<number | null>(null)

  async function handleDelete(expense: Expense) {
    if (!confirm(`Delete expense of ${expense.totalAmount} ${expense.currency} from ${format(new Date(expense.purchaseDate), 'dd.MM.yyyy')}?`))
      return
    setDeletingId(expense.id)
    try {
      await deleteExpense.mutateAsync(expense.id)
    } finally {
      setDeletingId(null)
    }
  }

  const totalSpent = expenses.reduce((sum, e) => sum + e.totalAmount, 0)

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Expenses</h1>
          {expenses.length > 0 && (
            <p className="text-sm text-gray-500 mt-0.5">
              {expenses.length} record{expenses.length !== 1 ? 's' : ''} · Total:{' '}
              <span className="font-semibold text-gray-700">
                {totalSpent.toFixed(2)} {expenses[0]?.currency ?? 'BAM'}
              </span>
            </p>
          )}
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => navigate('/expenses/scan')}
            className="flex items-center gap-1.5 px-3 py-2 rounded-xl border border-gray-200 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
          >
            <Camera className="w-4 h-4" />
            Scan Receipt
          </button>
          <button
            onClick={() => navigate('/expenses/new')}
            className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-honey-500 hover:bg-honey-600 text-white text-sm font-semibold transition-colors"
          >
            <Plus className="w-4 h-4" />
            Add Expense
          </button>
        </div>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex justify-center py-16">
          <Loader2 className="w-6 h-6 animate-spin text-honey-500" />
        </div>
      )}

      {/* Empty */}
      {!isLoading && expenses.length === 0 && (
        <div className="text-center py-20 bg-white rounded-2xl border border-honey-100 shadow-sm">
          <ReceiptText className="w-12 h-12 text-honey-300 mx-auto mb-3" />
          <p className="text-gray-500 font-medium">No expenses recorded yet.</p>
          <p className="text-sm text-gray-400 mt-1">
            Add your first expense manually or by scanning a receipt.
          </p>
          <div className="flex items-center justify-center gap-3 mt-5">
            <button
              onClick={() => navigate('/expenses/scan')}
              className="flex items-center gap-1.5 px-4 py-2 rounded-xl border border-gray-200 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
            >
              <Camera className="w-4 h-4" />
              Scan Receipt
            </button>
            <button
              onClick={() => navigate('/expenses/new')}
              className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-honey-500 hover:bg-honey-600 text-white text-sm font-semibold transition-colors"
            >
              <Plus className="w-4 h-4" />
              Add Manually
            </button>
          </div>
        </div>
      )}

      {/* Error state */}
      {deleteExpense.isError && (
        <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 rounded-xl px-4 py-3 text-sm">
          <AlertCircle className="w-4 h-4 shrink-0" />
          Failed to delete expense. Please try again.
        </div>
      )}

      {/* Expense list */}
      {!isLoading && expenses.length > 0 && (
        <div className="space-y-3">
          {expenses.map(expense => (
            <ExpenseCard
              key={expense.id}
              expense={expense}
              isDeleting={deletingId === expense.id}
              onEdit={() => navigate(`/expenses/${expense.id}/edit`)}
              onDelete={() => handleDelete(expense)}
            />
          ))}
        </div>
      )}
    </div>
  )
}

// ── Expense Card ──────────────────────────────────────────────────────────────

interface ExpenseCardProps {
  expense: Expense
  isDeleting: boolean
  onEdit: () => void
  onDelete: () => void
}

function ExpenseCard({ expense, isDeleting, onEdit, onDelete }: ExpenseCardProps) {
  const isReceiptScan = expense.source === ExpenseSource.ReceiptScan

  return (
    <div className="bg-white rounded-2xl border border-honey-100 shadow-sm px-5 py-4 flex items-center gap-4">
      {/* Icon */}
      <div className={`w-10 h-10 rounded-xl flex items-center justify-center shrink-0 ${
        isReceiptScan ? 'bg-blue-50 text-blue-500' : 'bg-honey-50 text-honey-600'
      }`}>
        {isReceiptScan ? <Camera className="w-5 h-5" /> : <PencilLine className="w-5 h-5" />}
      </div>

      {/* Info */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          <span className="font-semibold text-gray-900">
            {expense.totalAmount.toFixed(2)} {expense.currency}
          </span>
          <span className="text-xs text-gray-400 bg-gray-100 rounded-full px-2 py-0.5">
            {ExpenseSourceLabels[expense.source]}
          </span>
        </div>
        <div className="flex items-center gap-3 mt-0.5 text-sm text-gray-500">
          <span>{format(new Date(expense.purchaseDate), 'dd.MM.yyyy')}</span>
          <span>·</span>
          <span>{expense.itemCount} item{expense.itemCount !== 1 ? 's' : ''}</span>
          {expense.notes && (
            <>
              <span>·</span>
              <span className="truncate">{expense.notes}</span>
            </>
          )}
        </div>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-1 shrink-0">
        <button
          onClick={onEdit}
          className="p-2 rounded-lg text-gray-400 hover:text-honey-600 hover:bg-honey-50 transition-colors"
          aria-label="Edit expense"
        >
          <PencilLine className="w-4 h-4" />
        </button>
        <button
          onClick={onDelete}
          disabled={isDeleting}
          className="p-2 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 transition-colors disabled:opacity-50"
          aria-label="Delete expense"
        >
          {isDeleting
            ? <Loader2 className="w-4 h-4 animate-spin" />
            : <Trash2 className="w-4 h-4" />
          }
        </button>
      </div>
    </div>
  )
}
