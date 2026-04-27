import { useState } from 'react';
import { useCategories } from '../hooks/useCategories';
import { useExpenses } from '../contexts/ExpensesContext';
import SummaryCards from '../components/dashboard/SummaryCards';
import CategoryBreakdown from '../components/dashboard/CategoryBreakdown';
import ExpenseRow from '../components/expenses/ExpenseRow';
import ExpenseModal from '../components/expenses/ExpenseModal';
import DeleteModal from '../components/expenses/DeleteModal';
import Button from '../components/ui/Button';
import Skeleton from '../components/ui/Skeleton';
import Alert from '../components/ui/Alert';
import Icon from '../components/ui/Icon';

// Filter expenses to a given month + year
const inMonth = (expenses, month, year) =>
  expenses.filter((e) => {
    const d = new Date(e.date);
    return d.getMonth() === month && d.getFullYear() === year;
  });

export default function DashboardPage() {
  const { expenses, loading, error, fetchExpenses } = useExpenses();
  const { categories } = useCategories();
  const [modal, setModal] = useState(null); // null | { type: 'add'|'edit'|'delete', expense? }

  const now      = new Date();
  const prevDate = new Date(now.getFullYear(), now.getMonth() - 1, 1);

  const thisMonthExpenses = inMonth(expenses, now.getMonth(),      now.getFullYear());
  const lastMonthExpenses = inMonth(expenses, prevDate.getMonth(), prevDate.getFullYear());

  const totalThisMonth = thisMonthExpenses.reduce((s, e) => s + e.amount, 0);
  const totalLastMonth = lastMonthExpenses.reduce((s, e) => s + e.amount, 0);
  const monthName      = now.toLocaleString('en-US', { month: 'long' });

  const recentExpenses = expenses.slice(0, 6);

  return (
    <div className="max-w-4xl mx-auto px-5 py-7 animate-fade-in">
      {/* Page header */}
      <div className="flex items-end justify-between mb-6">
        <div>
          <p className="text-xs text-gray-500 mb-1">{monthName} {now.getFullYear()}</p>
          <h1 className="text-2xl font-semibold text-gray-900">Dashboard</h1>
        </div>
        <Button size="sm" onClick={() => setModal({ type: 'add' })}>
          <Icon name="plus" size={15} />
          Add
        </Button>
      </div>

      {/* Error state */}
      {error && (
        <Alert type="error">
          {error}{' '}
          <button
            onClick={fetchExpenses}
            className="underline font-semibold hover:opacity-80"
          >
            Retry
          </button>
        </Alert>
      )}

      {/* Summary cards */}
      <SummaryCards
        totalThisMonth={totalThisMonth}
        totalLastMonth={totalLastMonth}
        countThisMonth={thisMonthExpenses.length}
        monthName={monthName}
        loading={loading}
        currencySymbol="$"
      />

      {/* Category donut */}
      <CategoryBreakdown
        expenses={thisMonthExpenses}
        total={totalThisMonth}
        loading={loading}
        currencySymbol="$"
        categories={categories}
      />

      {/* Recent expenses */}
      <div className="bg-white rounded-2xl border border-gray-200 p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-sm font-semibold text-gray-900">Recent Expenses</h2>
        </div>

        {loading ? (
          <div className="space-y-2">
            {[1, 2, 3].map((i) => <Skeleton key={i} height="h-16" />)}
          </div>
        ) : recentExpenses.length === 0 ? (
          <div className="text-center py-10">
            <p className="text-sm text-gray-500 mb-3">No expenses yet.</p>
            <Button size="sm" onClick={() => setModal({ type: 'add' })}>
              <Icon name="plus" size={14} /> Add your first
            </Button>
          </div>
        ) : (
          <div className="space-y-1">
            {recentExpenses.map((e) => (
              <ExpenseRow
                key={e.id}
                expense={e}
                onEdit={(exp) => setModal({ type: 'edit', expense: exp })}
                onDelete={(exp) => setModal({ type: 'delete', expense: exp })}
              />
            ))}
          </div>
        )}
      </div>

      {/* Modals */}
      <ExpenseModal
        isOpen={modal?.type === 'add' || modal?.type === 'edit'}
        onClose={() => setModal(null)}
        expense={modal?.expense}
      />
      <DeleteModal
        isOpen={modal?.type === 'delete'}
        onClose={() => setModal(null)}
        expense={modal?.expense}
      />
    </div>
  );
}
