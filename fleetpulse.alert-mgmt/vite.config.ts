import { fileURLToPath, URL } from 'node:url'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'
import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite' // 1. Import Tailwind
import { federation } from '@module-federation/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    babel({ presets: [reactCompilerPreset()] }),
    tailwindcss(), // 2. Add Tailwind to the Vite plugins
    federation({
      name: 'alerts_mfe',
      filename: 'remoteEntry.js',
      exposes: {
        './AlertsDashboard': './src/features/alerts/components/AlertsDashboard.tsx'
      },
      shared: {
        react: { singleton: true, requiredVersion: '^19.2.7' },
        'react-dom': { singleton: true, requiredVersion: '^19.2.7' },
      },
      dts: {
        tsConfigPath: './tsconfig.app.json'
      },
    })
  ],
  build: {
    target: 'esnext',
    minify: false, // Recommended for MF dev builds
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
      //"@": path.resolve(__dirname, "./src"),
    }
  },
  server: {
    port: 5174
  }
})
