/**
 * SVG icons used across the app. Lucide-style: 24x24 viewBox, currentColor stroke.
 * Apply size via Tailwind (w-4 h-4 etc.) and color via text-* classes.
 */

interface IconProps extends React.SVGProps<SVGSVGElement> {
  className?: string;
}

const baseProps = {
  xmlns: 'http://www.w3.org/2000/svg',
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 2,
  strokeLinecap: 'round' as const,
  strokeLinejoin: 'round' as const,
};

export function LockIcon({ className = 'w-4 h-4', ...rest }: IconProps) {
  return (
    <svg {...baseProps} className={className} {...rest} aria-hidden="true">
      <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
      <path d="M7 11V7a5 5 0 0 1 10 0v4" />
    </svg>
  );
}

export function TrophyIcon({ className = 'w-4 h-4', ...rest }: IconProps) {
  return (
    <svg {...baseProps} className={className} {...rest} aria-hidden="true">
      <path d="M6 9H4.5a2.5 2.5 0 0 1 0-5H6" />
      <path d="M18 9h1.5a2.5 2.5 0 0 0 0-5H18" />
      <path d="M4 22h16" />
      <path d="M10 14.66V17c0 .55-.47.98-.97 1.21C7.85 18.75 7 20.24 7 22" />
      <path d="M14 14.66V17c0 .55.47.98.97 1.21C16.15 18.75 17 20.24 17 22" />
      <path d="M18 2H6v7a6 6 0 0 0 12 0V2Z" />
    </svg>
  );
}

export function FlameIcon({ className = 'w-4 h-4', ...rest }: IconProps) {
  return (
    <svg {...baseProps} className={className} {...rest} aria-hidden="true">
      <path d="M8.5 14.5A2.5 2.5 0 0 0 11 12c0-1.38-.5-2-1-3-1.072-2.143-.224-4.054 2-6 .5 2.5 2 4.9 4 6.5 2 1.6 3 3.5 3 5.5a7 7 0 1 1-14 0c0-1.153.433-2.294 1-3a2.5 2.5 0 0 0 2.5 2.5z" />
    </svg>
  );
}

export function ClockIcon({ className = 'w-4 h-4', ...rest }: IconProps) {
  return (
    <svg {...baseProps} className={className} {...rest} aria-hidden="true">
      <circle cx="12" cy="12" r="10" />
      <polyline points="12 6 12 12 16 14" />
    </svg>
  );
}

export function CheckCircleIcon({ className = 'w-4 h-4', ...rest }: IconProps) {
  return (
    <svg {...baseProps} className={className} {...rest} aria-hidden="true">
      <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
      <polyline points="22 4 12 14.01 9 11.01" />
    </svg>
  );
}

export function XCircleIcon({ className = 'w-4 h-4', ...rest }: IconProps) {
  return (
    <svg {...baseProps} className={className} {...rest} aria-hidden="true">
      <circle cx="12" cy="12" r="10" />
      <line x1="15" y1="9" x2="9" y2="15" />
      <line x1="9" y1="9" x2="15" y2="15" />
    </svg>
  );
}

export function ChevronUpIcon({ className = 'w-4 h-4', ...rest }: IconProps) {
  return (
    <svg {...baseProps} className={className} {...rest} aria-hidden="true">
      <polyline points="18 15 12 9 6 15" />
    </svg>
  );
}

export function ChevronDownIcon({ className = 'w-4 h-4', ...rest }: IconProps) {
  return (
    <svg {...baseProps} className={className} {...rest} aria-hidden="true">
      <polyline points="6 9 12 15 18 9" />
    </svg>
  );
}
