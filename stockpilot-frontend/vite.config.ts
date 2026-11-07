import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const pythonApi = 'http://127.0.0.1:8000'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: pythonApi, changeOrigin: true },
      '/Products': { target: pythonApi, changeOrigin: true },
      '/Orders': { target: pythonApi, changeOrigin: true },
      '/Category': { target: pythonApi, changeOrigin: true },
      '/Suppliers': { target: pythonApi, changeOrigin: true }
    }
  }
})
