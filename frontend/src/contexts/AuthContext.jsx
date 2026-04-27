import { createContext, useContext, useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/auth.service';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser]         = useState(null);
  const [isLoading, setLoading] = useState(true);
  const navigate                = useNavigate();

  // On mount, restore session from stored token.
  // Decode the JWT payload (base64url) to read the username claim without a /me endpoint.
  useEffect(() => {
    const token = authService.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
        const username = payload['unique_name'] || payload['sub'] || 'unknown';
        setUser({ username });
      } catch {
        authService.logout();
      }
    }
    setLoading(false);
  }, []);

  /**
   * POST /api/Auth/login — stores the JWT and redirects to /dashboard.
   * @param {string} username
   * @param {string} password
   */
  const login = async (username, password) => {
    await authService.login(username, password);
    setUser({ username });
    navigate('/dashboard');
  };

  const register = async (username, email, password, firstName, lastName) => {
    await authService.register(username, email, password, firstName, lastName);
    setUser({ username });
    navigate('/dashboard');
  };

  const logout = () => {
    authService.logout();
    setUser(null);
    navigate('/login');
  };

  return (
    <AuthContext.Provider value={{ user, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>');
  return ctx;
}
