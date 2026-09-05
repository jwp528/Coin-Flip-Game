// Coin Flip Game - Service Worker (development)
const CACHE_NAME = 'coin-flip-game-v1.5.14';
const urlsToCache = [
  '/',
  '/index.html',
  '/app.css',
  '/bootstrap/bootstrap.min.css',
  '/CoinFlipGame.App.styles.css',
  '/_framework/blazor.webassembly.js',
  '/js/coinhelpers.js',
  '/js/coinpreviewmodal.js',
  '/js/particles.js',
  '/js/physics.js',
  '/js/audio.js',
  '/js/pwa.js',
  '/js/progressSync.js',
  '/js/externalAuth.js',
  '/img/coins/logo.png',
  '/img/coins/Random.png',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png',
  '/manifest.json',
  '/favicon.png'
];

async function precache(cache, urls) {
  await Promise.all(urls.map(async (url) => {
    try {
      const response = await fetch(new Request(url, { cache: 'reload' }));
      if (response && response.ok) {
        await cache.put(url, response);
      } else {
        console.warn('[ServiceWorker] Skip (not ok):', url, response && response.status);
      }
    } catch (err) {
      console.warn('[ServiceWorker] Skip (failed):', url, err);
    }
  }));
}

self.addEventListener('install', event => {
  console.log('[ServiceWorker] Install', CACHE_NAME);
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => precache(cache, urlsToCache))
  );
  self.skipWaiting();
});

self.addEventListener('activate', event => {
  console.log('[ServiceWorker] Activate', CACHE_NAME);
  event.waitUntil(
    caches.keys().then(cacheNames =>
      Promise.all(
        cacheNames
          .filter(name => name !== CACHE_NAME)
          .map(name => caches.delete(name))
      )
    )
  );
  return self.clients.claim();
});

self.addEventListener('fetch', event => {
  if (event.request.method !== 'GET') {
    return;
  }

  if (!event.request.url.startsWith(self.location.origin)) {
    return;
  }

  const url = new URL(event.request.url);

  if (url.pathname.startsWith('/api/') || url.pathname.startsWith('/.auth/')) {
    return;
  }

  if (event.request.mode === 'navigate') {
    event.respondWith(
      fetch(event.request).catch(() => caches.match('/index.html'))
    );
    return;
  }

  event.respondWith(
    caches.match(event.request).then(cached => {
      const networked = fetch(event.request)
        .then(response => {
          if (response && response.status === 200 && response.type === 'basic') {
            const copy = response.clone();
            caches.open(CACHE_NAME).then(cache => cache.put(event.request, copy));
          }
          return response;
        })
        .catch(() => cached || caches.match('/index.html'));

      return cached || networked;
    })
  );
});

self.addEventListener('message', event => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
});
