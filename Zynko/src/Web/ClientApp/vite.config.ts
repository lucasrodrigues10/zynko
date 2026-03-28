import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

const target =
  process.env['services__webapi__https__0'] ||
  process.env['services__webapi__http__0'] ||
  'http://localhost:5256';

const proxyOptions = { target, secure: false, changeOrigin: true };

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: parseInt(process.env.PORT!),
    proxy: {
      '/api': proxyOptions,
      '/openapi': proxyOptions,
      '/scalar': proxyOptions,
      '/hubs': { ...proxyOptions, ws: true },
    },
  },
  build: {
    outDir: 'build',
  },
});
