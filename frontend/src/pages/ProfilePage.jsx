import { useAuth } from '../contexts/AuthContext';
import Button from '../components/ui/Button';
import Icon from '../components/ui/Icon';

const INFO_ROWS = [
  ['Backend',      'ExpenseTracker .NET 10 API'],
  ['Architecture', 'Clean Architecture (4 layers)'],
  ['Auth',         'JWT Bearer token'],
  ['Database',     'PostgreSQL + Npgsql ADO.NET'],
  ['Frontend',     'React 18 + React Router v6 + Tailwind CSS'],
];

export default function ProfilePage() {
  const { user, logout } = useAuth();

  return (
    <div className="max-w-lg mx-auto px-5 py-7 animate-fade-in">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Profile</h1>

      {/* User card */}
      <div className="bg-white rounded-2xl border border-gray-200 p-6 mb-4">
        {/* Avatar + identity */}
        <div className="flex items-center gap-4 mb-6">
          <div className="w-14 h-14 rounded-full bg-primary-100 flex items-center justify-center shrink-0">
            <Icon name="user" size={26} className="text-primary-700" />
          </div>
          <div>
            <p className="text-base font-semibold text-gray-900">{user?.username ?? 'admin'}</p>
            <p className="text-sm text-gray-500">Expense Tracker</p>
          </div>
        </div>

        {/* Stack info */}
        <div className="border-t border-gray-100 pt-5 space-y-4">
          {INFO_ROWS.map(([label, value]) => (
            <div key={label}>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-0.5">
                {label}
              </p>
              <p className="text-sm text-gray-900 font-mono">{value}</p>
            </div>
          ))}
        </div>
      </div>

      <Button variant="danger" fullWidth onClick={logout}>
        <Icon name="logout" size={16} />
        Sign Out
      </Button>
    </div>
  );
}
