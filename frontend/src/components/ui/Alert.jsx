import { useEffect } from 'react';
import Icon from './Icon';
import { cn } from '../../utils/cn';

const STYLES = {
  success: 'bg-success-light border-success text-success',
  error:   'bg-error-light   border-error   text-error',
  warning: 'bg-warning-light border-warning text-warning',
  info:    'bg-info-light    border-info    text-info',
};

/**
 * Left-bordered alert / feedback message.
 *
 * @param {{
 *   type?: 'success'|'error'|'warning'|'info',
 *   children: React.ReactNode,
 *   onClose?: () => void,
 *   autoDismiss?: number,
 *   className?: string,
 * }} props
 */
export default function Alert({ type = 'info', children, onClose, autoDismiss, className = '' }) {
  useEffect(() => {
    if (!autoDismiss || !onClose) return;
    const id = setTimeout(onClose, autoDismiss);
    return () => clearTimeout(id);
  }, [autoDismiss, onClose]);

  return (
    <div
      role="alert"
      className={cn(
        'flex items-start justify-between gap-3',
        'border-l-4 rounded-r-lg px-4 py-3 mb-4',
        'animate-fade-in',
        STYLES[type],
        className,
      )}
    >
      <p className="text-sm font-semibold">{children}</p>
      {onClose && (
        <button
          onClick={onClose}
          aria-label="Dismiss"
          className="shrink-0 opacity-70 hover:opacity-100 transition-opacity"
        >
          <Icon name="x" size={16} />
        </button>
      )}
    </div>
  );
}
