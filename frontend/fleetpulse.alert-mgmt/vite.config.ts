/// <reference types="vitest/config" />
import { fileURLToPath, URL } from 'node:url'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'
import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite' // 1. Import Tailwind
import { federation } from '@module-federation/vite'

const standalone = process.env.VITE_STANDALONE === 'true'
// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    babel({ presets: [reactCompilerPreset()] }),
    tailwindcss(), // 2. Add Tailwind to the Vite plugins
    // Standalone runs skip federation so they don't clobber the remote's shared temp files/dts
    ...(standalone ? [] : [federation({
      name: 'alerts_mfe',
      filename: 'remoteEntry.js',
      exposes: {
        './AlertsDashboard': './src/features/alerts/components/AlertsDashboard.tsx'
      },
      shared: {
        react: { singleton: true },
        'react-dom': { singleton: true },
      },
      dts: {
        tsConfigPath: './tsconfig.app.json'
      },
    })])
  ],
  cacheDir: standalone ? 'node_modules/.vite-standalone' : 'node_modules/.vite',
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: "./src/test/setup.ts",
  },

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
