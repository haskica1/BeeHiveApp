import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.ico', 'apple-touch-icon.png', 'masked-icon.svg'],
      manifest: {
        name: 'BeeHive App',
        short_name: 'BeeHive',
        description: 'Manage your beekeeping operations',
        theme_color: '#d97706',
        background_color: '#fffbeb',
        display: 'standalone',
        icons: [
          { src: 'pwa-192x192.png', sizes: '192x192', type: 'image/png' },
          { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png' },
          { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png', purpose: 'any maskable' }
        ]
      },
      workbox: {
        // Cache API responses for offline use (network-first strategy)
        runtimeCaching: [
          {
            urlPattern: /^https?:\/\/localhost:\d+\/api\/.*/i,
            handler: 'NetworkFirst',
            options: {
              cacheName: 'beehive-api-cache',
              expiration: { maxEntries: 100, maxAgeSeconds: 60 * 60 * 24 }
            }
          }
        ]
      }
    })
  ],
  server: {
    port: 5173,
    proxy: {
      // Proxy API calls to the .NET backend during development
      '/api': {
        target: 'https://beehiveapp-y076.onrender.com',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
