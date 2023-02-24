import { defineConfig } from 'vite';
import solidPlugin from 'vite-plugin-solid';
import autoprefixer from 'autoprefixer';

const postcssNested = require('postcss-nested');
const postcssUrl = require('postcss-url');

export default defineConfig({
  plugins: [solidPlugin()],
  server: {
    port: 3000,
  },
  build: {
    target: 'esnext',
  },
  css: {
    postcss: {
        parser: false,
        map: false,
        plugins: [
            autoprefixer,
            postcssNested,
            postcssUrl
        ]
    }
  }
});
