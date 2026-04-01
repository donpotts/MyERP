/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    './wwwroot/**/*.html',
    './**/*.razor',
    './**/*.cs',
  ],
  theme: {
    extend: {
      colors: {
        primary: '#4f46e5',
        'dark-bg': '#0f172a',
      }
    }
  },
  safelist: [
    // Dynamic role/avatar colors
    'bg-indigo-500', 'bg-violet-500', 'bg-green-500', 'bg-amber-500', 'bg-slate-500',
    'bg-blue-500', 'bg-red-500', 'bg-pink-500', 'bg-teal-500', 'bg-orange-500', 'bg-cyan-500',
    // Dynamic top bar gradients from ThemeService.TopBarClass()
    'bg-gradient-to-r',
    'from-indigo-600', 'to-purple-600',
    'from-violet-600', 'to-purple-700',
    'from-blue-600',   'to-blue-700',
    'from-emerald-600','to-teal-700',
    'from-rose-600',   'to-pink-700',
    'from-amber-500',  'to-orange-600',
    // Theme toggle slider
    'translate-x-1', 'translate-x-6',
    // Sidebar state classes (applied without lg: prefix — matched by app.css rules)
    'sidebar-collapsed', 'sidebar-expanded',
    // Safelist full color palette for accent remapping and dark mode overrides
    {
      pattern: /^(bg|text|border|ring|from|to|via)-(indigo|violet|blue|emerald|rose|amber|purple|teal|pink|orange|green|red|yellow|gray|slate|cyan)-(50|100|200|300|400|500|600|700|800|900)$/,
      variants: ['hover', 'active', 'focus', 'dark', 'dark:hover'],
    },
  ]
}
