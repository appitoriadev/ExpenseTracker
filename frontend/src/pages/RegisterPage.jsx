import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import Button from '../components/ui/Button';
import Input from '../components/ui/Input';
import Alert from '../components/ui/Alert';
import Icon from '../components/ui/Icon';

export default function RegisterPage() {
  const { register } = useAuth();

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [errors, setErrors]     = useState({});
  const [loading, setLoading]   = useState(false);
  const [apiError, setApiError] = useState('');

  const set = (key, value) => {
    setForm((f) => ({ ...f, [key]: value }));
    if (errors[key]) setErrors((e) => ({ ...e, [key]: '' }));
  };

  const validate = () => {
    const e = {};
    if (!form.firstName.trim())       e.firstName       = 'First name is required';
    if (!form.lastName.trim())        e.lastName        = 'Last name is required';
    if (!form.username.trim())        e.username        = 'Username is required';
    if (!form.email.trim())           e.email           = 'Email is required';
    else if (!/\S+@\S+\.\S+/.test(form.email)) e.email = 'Enter a valid email address';
    if (!form.password)               e.password        = 'Password is required';
    else if (form.password.length < 6) e.password       = 'Password must be at least 6 characters';
    if (!form.confirmPassword)        e.confirmPassword = 'Please confirm your password';
    else if (form.password !== form.confirmPassword) e.confirmPassword = 'Passwords do not match';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setApiError('');
    if (!validate()) return;
    setLoading(true);
    try {
      await register(form.username, form.email, form.password, form.firstName, form.lastName);
    } catch (err) {
      setApiError(err.message ?? 'Registration failed. Please try again.');
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
          <p className="text-sm text-gray-500">Create your account</p>
        </div>

        {/* Card */}
        <div className="bg-white rounded-2xl border border-gray-200 shadow-md p-8">
          {apiError && (
            <Alert type="error" onClose={() => setApiError('')}>
              {apiError}
            </Alert>
          )}

          <form onSubmit={handleSubmit} noValidate>
            <div className="flex gap-3">
              <Input
                label="First Name"
                required
                placeholder="Jane"
                value={form.firstName}
                onChange={(e) => set('firstName', e.target.value)}
                error={errors.firstName}
                autoComplete="given-name"
                autoFocus
              />
              <Input
                label="Last Name"
                required
                placeholder="Doe"
                value={form.lastName}
                onChange={(e) => set('lastName', e.target.value)}
                error={errors.lastName}
                autoComplete="family-name"
              />
            </div>
            <Input
              label="Username"
              required
              placeholder="janedoe"
              value={form.username}
              onChange={(e) => set('username', e.target.value)}
              error={errors.username}
              autoComplete="username"
            />
            <Input
              type="email"
              label="Email"
              required
              placeholder="jane@example.com"
              value={form.email}
              onChange={(e) => set('email', e.target.value)}
              error={errors.email}
              autoComplete="email"
            />
            <Input
              type="password"
              label="Password"
              required
              placeholder="••••••••"
              value={form.password}
              onChange={(e) => set('password', e.target.value)}
              error={errors.password}
              autoComplete="new-password"
            />
            <Input
              type="password"
              label="Confirm Password"
              required
              placeholder="••••••••"
              value={form.confirmPassword}
              onChange={(e) => set('confirmPassword', e.target.value)}
              error={errors.confirmPassword}
              autoComplete="new-password"
            />
            <Button type="submit" loading={loading} fullWidth className="mt-2">
              Create Account
            </Button>
          </form>
        </div>

        {/* Back to login */}
        <p className="text-center text-sm text-gray-500 mt-6">
          Already have an account?{' '}
          <Link to="/login" className="font-medium text-primary-600 hover:text-primary-700">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
