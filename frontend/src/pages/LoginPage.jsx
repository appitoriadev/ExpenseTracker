import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import Button from '../components/ui/Button';
import Input from '../components/ui/Input';
import Alert from '../components/ui/Alert';
import Icon from '../components/ui/Icon';

export default function LoginPage() {
  const { login } = useAuth();

  const [form, setForm]       = useState({ username: '', password: '' });
  const [errors, setErrors]   = useState({});
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState('');
  const [sessionMsg, setSessionMsg] = useState(() => {
    const expired = sessionStorage.getItem('session_expired');
    if (expired) sessionStorage.removeItem('session_expired');
    return expired ? 'Your session has expired. Please sign in again.' : '';
  });

  const set = (key, value) => {
    setForm((f) => ({ ...f, [key]: value }));
    if (errors[key]) setErrors((e) => ({ ...e, [key]: '' }));
  };

  const validate = () => {
    const e = {};
    if (!form.username.trim()) e.username = 'Username is required';
    if (!form.password)        e.password  = 'Password is required';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setApiError('');
    if (!validate()) return;
    setLoading(true);
    try {
      await login(form.username, form.password);
    } catch (err) {
      setApiError(err.message ?? 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-dvh flex items-center justify-center bg-gray-50 px-4 py-12">
      <div className="w-full max-w-sm animate-fade-in">
        {/* Logo */}
        <div className="text-center mb-10">
          <div className="w-16 h-16 rounded-2xl bg-primary-500 shadow-primary flex items-center justify-center mx-auto mb-4">
            <Icon name="wallet" size={30} className="text-white" />
          </div>
          <h1 className="text-2xl font-semibold text-gray-900 mb-1">Expense Tracker</h1>
          <p className="text-sm text-gray-500">Sign in to your account</p>
        </div>

        {/* Card */}
        <div className="bg-white rounded-2xl border border-gray-200 shadow-md p-8">
          {sessionMsg && (
            <Alert type="warning" onClose={() => setSessionMsg('')} autoDismiss={4000}>
              {sessionMsg}
            </Alert>
          )}
          {apiError && (
            <Alert type="error" onClose={() => setApiError('')}>
              {apiError}
            </Alert>
          )}

          <form onSubmit={handleSubmit} noValidate>
            <Input
              label="Username"
              required
              placeholder="admin"
              value={form.username}
              onChange={(e) => set('username', e.target.value)}
              error={errors.username}
              autoComplete="username"
              autoFocus
            />
            <Input
              type="password"
              label="Password"
              required
              placeholder="••••••••"
              value={form.password}
              onChange={(e) => set('password', e.target.value)}
              error={errors.password}
              autoComplete="current-password"
            />
            <Button type="submit" loading={loading} fullWidth className="mt-2">
              Sign In
            </Button>
          </form>
        </div>

        {/* Link to registration */}
        <p className="text-center text-sm text-gray-500 mt-6">
          Don&apos;t have an account?{' '}
          <Link to="/register" className="font-medium text-primary-600 hover:text-primary-700">
            Sign up
          </Link>
        </p>
      </div>
    </div>
  );
}
