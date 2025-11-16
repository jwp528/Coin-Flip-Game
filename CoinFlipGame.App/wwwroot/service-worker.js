// Coin Flip Game - Service Worker
const CACHE_NAME = 'coin-flip-game-v1';
const urlsToCache = [
  '/',
  '/index.html',
  '/css/app.css',
  '/css/bootstrap/bootstrap.min.css',
  '/_framework/blazor.webassembly.js',
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
        return cache.addAll(urlsToCache);
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
  // Skip cross-origin requests
  if (!event.request.url.startsWith(self.location.origin)) {
    return;
  }

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
