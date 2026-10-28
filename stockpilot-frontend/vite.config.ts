import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'http://localhost:5232', changeOrigin: true, secure: false },
      '/Products': { target: 'http://localhost:5232', changeOrigin: true, secure: false },
      '/Orders': { target: 'http://localhost:5232', changeOrigin: true, secure: false },
      '/Category': { target: 'http://localhost:5232', changeOrigin: true, secure: false },
      '/Suppliers': { target: 'http://localhost:5232', changeOrigin: true, secure: false }
    }
  }
})
