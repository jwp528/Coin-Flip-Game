// Coin Flip Game - Service Worker (Production)
// This file is used when the app is published
const CACHE_NAME = 'coin-flip-game-v1.4.1';
const urlsToCache = [
  '/',
  '/index.html',
  '/css/app.css',
  '/css/bootstrap/bootstrap.min.css',
  // Note: Do NOT pre-cache _framework files - let Blazor handle them
  '/js/coinhelpers.js',
  '/js/coinpreviewmodal.js',
  '/js/particles.js',
  '/js/physics.js',
  '/js/audio.js',
  '/img/coins/logo.png',
  '/manifest.json',
  '/favicon.png'
];

// Install event - cache resources
self.addEventListener('install', event => {
  console.log('[ServiceWorker] Install v' + CACHE_NAME);
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        console.log('[ServiceWorker] Caching app shell');
        // Use cache-bust parameter and better error handling
        return cache.addAll(urlsToCache.map(url => new Request(url, {cache: 'reload'})))
          .catch(err => {
            console.error('[ServiceWorker] Failed to cache resources:', err);
            // Continue anyway - don't block installation
            return Promise.resolve();
          });
      })
      .catch(err => {
        console.error('[ServiceWorker] Cache failed:', err);
      })
  );
  // Force the waiting service worker to become the active service worker
  self.skipWaiting();
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
  console.log('[ServiceWorker] Activate v' + CACHE_NAME);
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
  // Take control of all pages immediately
  return self.clients.claim();
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', event => {
  const url = new URL(event.request.url);
  
  // Skip cross-origin requests
  if (!event.request.url.startsWith(self.location.origin)) {
    return;
  }

  // Skip API calls - always fetch fresh
  if (event.request.url.includes('/api/')) {
    event.respondWith(fetch(event.request));
    return;
  }

  // CRITICAL FIX: Network-first for _framework files to prevent integrity errors
  // This prevents "mono_download_assets: RuntimeError: index out of bounds"
  if (url.pathname.includes('/_framework/') || 
      url.pathname.includes('blazor.webassembly.js') ||
      url.pathname.endsWith('.dll') ||
      url.pathname.endsWith('.wasm') ||
      url.pathname.endsWith('.pdb') ||
      url.pathname.endsWith('.dat') ||
      url.pathname.endsWith('blazor.boot.json')) {
    
    console.log('[ServiceWorker] Network-first for framework file:', url.pathname);
    
    // Network-first strategy for Blazor runtime files
    event.respondWith(
      fetch(event.request, { cache: 'no-cache' })
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
              statusText: 'Service Unavailable',
              headers: new Headers({
                'Content-Type': 'text/plain'
              })
            });
          });
        })
    );
    return;
  }

  // Cache-first strategy for static app shell resources
  event.respondWith(
    caches.match(event.request)
      .then(response => {
        // Cache hit - return response
        if (response) {
          // For index.html, also check network for updates in background
          if (event.request.url.endsWith('/') || event.request.url.endsWith('index.html')) {
            // Return cached version immediately, but update cache in background
            fetch(event.request)
              .then(freshResponse => {
                if (freshResponse && freshResponse.status === 200) {
                  caches.open(CACHE_NAME).then(cache => {
                    cache.put(event.request, freshResponse.clone());
                  });
                }
              })
              .catch(() => {
                // Network failed, cached version is being used
              });
          }
          return response;
        }

        // Not in cache - fetch from network
        return fetch(event.request).then(response => {
          // Check if valid response
          if (!response || response.status !== 200 || response.type !== 'basic') {
            return response;
          }

          // Clone the response
          const responseToCache = response.clone();

          // Cache the new resource
          caches.open(CACHE_NAME)
            .then(cache => {
              // Only cache GET requests
              if (event.request.method === 'GET') {
                cache.put(event.request, responseToCache);
              }
            });

          return response;
        }).catch(error => {
          console.error('[ServiceWorker] Fetch failed:', error);
          
          // Return offline page for navigation requests
          if (event.request.mode === 'navigate') {
            return caches.match('/index.html');
          }
          
          // Return a generic offline response for other requests
          return new Response('Offline - content not available', {
            status: 503,
            statusText: 'Service Unavailable',
            headers: new Headers({
              'Content-Type': 'text/plain'
            })
          });
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
    console.log('[ServiceWorker] Background sync: sync-flips');
    // Could sync flip data to cloud here
  }
});

// Push notifications (future feature)
self.addEventListener('push', event => {
  const options = {
    body: event.data ? event.data.text() : 'New coins available!',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/icon-72x72.png',
    vibrate: [200, 100, 200],
    tag: 'coin-flip-notification',
    requireInteraction: false,
    data: {
      dateOfArrival: Date.now(),
      primaryKey: 1
    }
  };

  event.waitUntil(
    self.registration.showNotification('Coin Flip Game', options)
  );
});

// Notification click
self.addEventListener('notificationclick', event => {
  event.notification.close();
  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true })
      .then(clientList => {
        // If app is already open, focus it
        for (let i = 0; i < clientList.length; i++) {
          const client = clientList[i];
          if (client.url === '/' && 'focus' in client) {
            return client.focus();
          }
        }
        // Otherwise, open a new window
        if (clients.openWindow) {
          return clients.openWindow('/');
        }
      })
  );
});
