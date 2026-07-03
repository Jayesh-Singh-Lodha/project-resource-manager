/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        background: '#0f1117',
        surface: '#1a1d27',
        'surface-hover': '#222638',
        border: '#2d3148',
        'border-hover': '#3d4168',
        accent: {
          DEFAULT: '#6366f1',
          hover: '#7c7ff7',
          muted: '#6366f133',
        },
        violet: {
          DEFAULT: '#8b5cf6',
          hover: '#a78bfa',
        },
        success: {
          DEFAULT: '#10b981',
          muted: '#10b98133',
        },
        warning: {
          DEFAULT: '#f59e0b',
          muted: '#f59e0b33',
        },
        danger: {
          DEFAULT: '#ef4444',
          muted: '#ef444433',
        },
        'text-primary': '#f1f5f9',
        'text-secondary': '#cbd5e1',
        'text-muted': '#94a3b8',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
      },
      boxShadow: {
        'glass': '0 8px 32px 0 rgba(0, 0, 0, 0.36)',
        'glass-sm': '0 4px 16px 0 rgba(0, 0, 0, 0.24)',
        'glow': '0 0 20px rgba(99, 102, 241, 0.15)',
        'glow-sm': '0 0 10px rgba(99, 102, 241, 0.1)',
      },
      backdropBlur: {
        'glass': '16px',
      },
      animation: {
        'fade-in': 'fadeIn 0.3s ease-out',
        'slide-up': 'slideUp 0.3s ease-out',
        'slide-in-right': 'slideInRight 0.3s ease-out',
        'pulse-slow': 'pulse 3s infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideUp: {
          '0%': { opacity: '0', transform: 'translateY(10px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        slideInRight: {
          '0%': { opacity: '0', transform: 'translateX(10px)' },
          '100%': { opacity: '1', transform: 'translateX(0)' },
        },
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
}
