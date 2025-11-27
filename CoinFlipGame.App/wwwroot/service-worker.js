// Coin Flip Game - Service Worker
const CACHE_NAME = 'coin-flip-game-v1.4.1';
const urlsToCache = [
  '/',
  '/index.html',
  '/css/app.css',
  '/css/bootstrap/bootstrap.min.css',
  // Note: Do NOT pre-cache _framework files - let Blazor handle them
  '/js/coinhelpers.js',
  '/js/particles.js',
  '/js/physics.js',
  '/js/audio.js',
  '/img/coins/logo.png',
  '/manifest.json'
];

// Install event - cache resources
self.addEventListener('install', event => {
  console.log('[ServiceWorker] Install');
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        console.log('[ServiceWorker] Caching app shell');
        // Use addAll with error handling to prevent partial cache
        return cache.addAll(urlsToCache).catch(err => {
          console.error('[ServiceWorker] Failed to cache resources:', err);
          // Continue anyway - don't block installation
          return Promise.resolve();
        });
      })
      .catch(err => {
        console.log('[ServiceWorker] Cache failed:', err);
      })
  );
  self.skipWaiting();
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
  console.log('[ServiceWorker] Activate');
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          if (cacheName !== CACHE_NAME) {
            console.log('[ServiceWorker] Removing old cache:', cacheName);
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
  return self.clients.claim();
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', event => {
  const url = new URL(event.request.url);
  
  // Skip cross-origin requests
  if (!event.request.url.startsWith(self.location.origin)) {
    return;
  }

  // CRITICAL: Use network-first for _framework files to avoid integrity errors
  // This prevents mono_download_assets RuntimeError: index out of bounds
  if (url.pathname.includes('/_framework/') || 
      url.pathname.includes('blazor.webassembly.js') ||
      url.pathname.endsWith('.dll') ||
      url.pathname.endsWith('.wasm') ||
      url.pathname.endsWith('.pdb') ||
      url.pathname.endsWith('.dat') ||
      url.pathname.endsWith('blazor.boot.json')) {
    // Network-first strategy for Blazor runtime files
    event.respondWith(
      fetch(event.request)
        .then(response => {
          // Only cache valid responses
          if (response && response.status === 200) {
            const responseToCache = response.clone();
            caches.open(CACHE_NAME).then(cache => {
              cache.put(event.request, responseToCache);
            });
          }
          return response;
        })
        .catch(error => {
          console.error('[ServiceWorker] Network fetch failed for framework file:', url.pathname, error);
          // Try cache as fallback
          return caches.match(event.request).then(cachedResponse => {
            if (cachedResponse) {
              console.log('[ServiceWorker] Serving cached framework file:', url.pathname);
              return cachedResponse;
            }
            // Return error response
            return new Response('Network error occurred', {
              status: 503,
              statusText: 'Service Unavailable'
            });
          });
        })
    );
    return;
  }

  // Cache-first strategy for app shell resources
  event.respondWith(
    caches.match(event.request)
      .then(response => {
        // Cache hit - return response
        if (response) {
          return response;
        }

        // Clone the request
        const fetchRequest = event.request.clone();

        return fetch(fetchRequest).then(response => {
          // Check if valid response
          if (!response || response.status !== 200 || response.type !== 'basic') {
            return response;
          }

          // Clone the response
          const responseToCache = response.clone();

          caches.open(CACHE_NAME)
            .then(cache => {
              // Don't cache POST requests or non-GET methods
              if (event.request.method === 'GET') {
                cache.put(event.request, responseToCache);
              }
            });

          return response;
        }).catch(error => {
          console.log('[ServiceWorker] Fetch failed:', error);
          // You could return a custom offline page here
          return caches.match('/index.html');
        });
      })
  );
});

// Message event - for manual cache updates
self.addEventListener('message', event => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
  
  if (event.data && event.data.type === 'CLEAR_CACHE') {
    event.waitUntil(
      caches.keys().then(cacheNames => {
        return Promise.all(
          cacheNames.map(cacheName => caches.delete(cacheName))
        );
      })
    );
  }
});

// Background sync for future features
self.addEventListener('sync', event => {
  if (event.tag === 'sync-flips') {
    console.log('[ServiceWorker] Background sync');
    // Could sync flip data to cloud here
  }
});

// Push notifications for future features
self.addEventListener('push', event => {
  const options = {
    body: event.data ? event.data.text() : 'New coins available!',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/icon-72x72.png',
    vibrate: [200, 100, 200],
    tag: 'coin-flip-notification',
    requireInteraction: false
  };

  event.waitUntil(
    self.registration.showNotification('Coin Flip Game', options)
  );
});

// Notification click
self.addEventListener('notificationclick', event => {
  event.notification.close();
  event.waitUntil(
    clients.openWindow('/')
  );
});

