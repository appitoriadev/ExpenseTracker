import { useState } from 'react';
import { useExpenses } from '../contexts/ExpensesContext';
import ExpenseRow from '../components/expenses/ExpenseRow';
import ExpenseModal from '../components/expenses/ExpenseModal';
import DeleteModal from '../components/expenses/DeleteModal';
import Button from '../components/ui/Button';
import Skeleton from '../components/ui/Skeleton';
import Alert from '../components/ui/Alert';
import Icon from '../components/ui/Icon';
import { useCategories } from '../hooks/useCategories';

export default function ExpensesPage() {
  const { expenses, loading, error, fetchExpenses } = useExpenses();
  const { categories: apiCategories } = useCategories();

  const [modal,  setModal]  = useState(null);
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState('All');

  const visible = expenses.filter((e) => {
    if (filter !== 'All' && e.category !== filter) return false;
    if (search && !e.title.toLowerCase().includes(search.toLowerCase())) return false;
    return true;
  });

  const categories = ['All', ...apiCategories];

  return (
    <div className="max-w-4xl mx-auto px-5 py-7 animate-fade-in">
      {/* Header */}
      <div className="flex items-end justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">All Expenses</h1>
        <Button size="sm" onClick={() => setModal({ type: 'add' })}>
          <Icon name="plus" size={15} />
          Add
        </Button>
      </div>

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

      {/* Search */}
      <div className="relative mb-3">
        <Icon
          name="search"
          size={16}
          className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
        />
        <input
          type="search"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search expenses…"
          className="w-full pl-9 pr-4 py-2.5 text-sm bg-white border-2 border-gray-200 rounded-xl focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-50 placeholder:text-gray-400"
        />
      </div>

      {/* Category filter pills */}
      <div className="flex gap-2 flex-wrap mb-5">
        {categories.map((cat) => (
          <button
            key={cat}
            onClick={() => setFilter(cat)}
            className={[
              'px-3 py-1.5 rounded-full text-xs font-semibold transition-colors duration-150',
              filter === cat
                ? 'bg-primary-500 text-white'
                : 'bg-gray-100 text-gray-600 hover:bg-gray-200',
            ].join(' ')}
          >
            {cat}
          </button>
        ))}
      </div>

      {/* List */}
      <div className="bg-white rounded-2xl border border-gray-200 p-4">
        {loading ? (
          <div className="space-y-2">
            {[1, 2, 3, 4, 5].map((i) => <Skeleton key={i} height="h-16" />)}
          </div>
        ) : visible.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-sm text-gray-500">
              {search || filter !== 'All'
                ? 'No expenses match your filters.'
                : 'No expenses yet.'}
            </p>
          </div>
        ) : (
          <div className="space-y-1">
            {visible.map((e) => (
              <ExpenseRow
                key={e.id}
                expense={e}
                onEdit={(exp) => setModal({ type: 'edit', expense: exp })}
                onDelete={(exp) => setModal({ type: 'delete', expense: exp })}
              />
            ))}
          </div>
        )}

        {visible.length > 0 && !loading && (
          <p className="text-xs text-gray-400 text-right mt-4 pr-2">
            {visible.length} expense{visible.length !== 1 ? 's' : ''}
          </p>
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
