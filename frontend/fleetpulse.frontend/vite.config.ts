/// <reference types="vitest/config" />
import { fileURLToPath, URL } from 'node:url'
import { defineConfig, loadEnv } from 'vite'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'
import tailwindcss from '@tailwindcss/vite'
import { federation } from '@module-federation/vite'

// https://vite.dev/config/
export default defineConfig(({ command, mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  // Dev server only; unit tests mock the module themselves, and builds never get the fake.
  const useSignalRMock =
    command === 'serve' && !process.env.VITEST && env.VITE_ENABLE_MOCKS === 'true'

  return {
  plugins: [
    tailwindcss(),
    react(),
    babel({ presets: [reactCompilerPreset()] }),
    federation({
      name: 'fleet_host',
      shared: {
        react: { singleton: true, requiredVersion: '^19.2.7' },
        'react-dom': { singleton: true, requiredVersion: '^19.2.7' },
      }
    }),
  ],
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: "./src/test/setup.ts",
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      ...(useSignalRMock && {
        '@microsoft/signalr': fileURLToPath(new URL('./src/mocks/signalr-mock.ts', import.meta.url)),
      }),
    }
  },
  build: {
    rollupOptions: {
      input: {
        main: fileURLToPath(new URL('./index.html', import.meta.url)),
        // MSAL redirect bridge page, must be a separate entry so it ships without the app bundle
        redirect: fileURLToPath(new URL('./redirect.html', import.meta.url)),
      },
    },
  },
  }
})
