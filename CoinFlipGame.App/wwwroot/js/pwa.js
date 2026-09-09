// Coin Flip Game — PWA install helpers + silent boot update gate
(function () {
    const DISMISS_KEY = 'pwaInstallDismissed';
    const UPDATED_KEY = 'pwaUpdated';
    const SW_URL = '/service-worker.js?v=1.5.26';
    const listeners = [];
    let reloading = false;
    let watchingController = false;

    function notify(method) {
        listeners.forEach(function (ref) {
            try {
                ref.invokeMethodAsync(method);
            } catch (err) {
                console.warn('[PWA] notify failed', method, err);
            }
        });
    }

    function reloadOnce() {
        if (reloading) return;
        reloading = true;
        try {
            sessionStorage.setItem(UPDATED_KEY, '1');
        } catch {
            // ignore
        }
        window.location.reload();
    }

    function waitForState(worker) {
        return new Promise(function (resolve) {
            if (!worker) {
                resolve();
                return;
            }
            if (worker.state === 'installed' || worker.state === 'activated' || worker.state === 'redundant') {
                resolve();
                return;
            }
            worker.addEventListener('statechange', function onChange() {
                if (worker.state === 'installed' || worker.state === 'activated' || worker.state === 'redundant') {
                    worker.removeEventListener('statechange', onChange);
                    resolve();
                }
            });
        });
    }

    const pwa = {
        deferredPrompt: null,
        swUrl: SW_URL,

        register: function (dotNetRef) {
            if (dotNetRef && listeners.indexOf(dotNetRef) === -1) {
                listeners.push(dotNetRef);
            }
            if (this.deferredPrompt) {
                notify('OnPwaInstallAvailable');
            }
        },

        canInstall: function () {
            return !!this.deferredPrompt;
        },

        isStandalone: function () {
            return window.matchMedia('(display-mode: standalone)').matches
                || window.navigator.standalone === true
                || document.referrer.includes('android-app://');
        },

        isIos: function () {
            return /iphone|ipad|ipod/i.test(window.navigator.userAgent)
                && !window.MSStream;
        },

        needsIosInstallHint: function () {
            return this.isIos() && !this.isStandalone();
        },

        isInstallDismissed: function () {
            try {
                return window.localStorage.getItem(DISMISS_KEY) === '1';
            } catch {
                return false;
            }
        },

        dismissInstall: function () {
            try {
                window.localStorage.setItem(DISMISS_KEY, '1');
            } catch {
                // ignore quota / private mode
            }
        },

        promptInstall: async function () {
            if (!this.deferredPrompt) {
                return false;
            }
            this.deferredPrompt.prompt();
            const choice = await this.deferredPrompt.userChoice;
            this.deferredPrompt = null;
            return choice && choice.outcome === 'accepted';
        },

        watchControllerChange: function () {
            if (watchingController || !('serviceWorker' in navigator)) return;
            watchingController = true;
            // Reload only when replacing an existing controller — not on first SW install.
            if (navigator.serviceWorker.controller) {
                navigator.serviceWorker.addEventListener('controllerchange', reloadOnce);
            }
        },

        activateWaiting: function (worker) {
            if (!worker) return;
            try {
                sessionStorage.setItem(UPDATED_KEY, '1');
            } catch {
                // ignore
            }
            worker.postMessage({ type: 'SKIP_WAITING' });
        },

        /**
         * Block boot until a SW update is applied (and the page reloads),
         * no update is available, or the check times out.
         * Returns: 'ready' | 'just-updated' | 'reloading' | 'timeout' | 'no-sw'
         */
        gateOnLaunch: async function (opts) {
            const timeoutMs = (opts && opts.timeoutMs) || 4000;
            const onUpdating = (opts && opts.onUpdating) || function () {};

            this.watchControllerChange();

            if (!('serviceWorker' in navigator)) return 'no-sw';

            let justUpdated = false;
            try {
                justUpdated = sessionStorage.getItem(UPDATED_KEY) === '1';
            } catch {
                justUpdated = false;
            }
            if (justUpdated) {
                try {
                    sessionStorage.removeItem(UPDATED_KEY);
                } catch {
                    // ignore
                }
                navigator.serviceWorker.register(SW_URL).catch(function () {});
                return 'just-updated';
            }

            const deadline = Date.now() + timeoutMs;
            function remaining() {
                return Math.max(0, deadline - Date.now());
            }
            function timed(promise) {
                return Promise.race([
                    promise,
                    new Promise(function (_, reject) {
                        setTimeout(function () {
                            reject(new Error('timeout'));
                        }, remaining() || 1);
                    })
                ]);
            }

            try {
                const registration = await timed(navigator.serviceWorker.register(SW_URL));

                registration.addEventListener('updatefound', function () {
                    if (navigator.serviceWorker.controller) {
                        onUpdating();
                    }
                });

                if (registration.installing && navigator.serviceWorker.controller) {
                    onUpdating();
                    await Promise.race([
                        waitForState(registration.installing),
                        new Promise(function (resolve) { setTimeout(resolve, remaining()); })
                    ]);
                }

                try {
                    await timed(registration.update());
                } catch {
                    // offline / slow network — do not block play
                }

                if (reloading) return 'reloading';

                if (registration.installing && navigator.serviceWorker.controller) {
                    onUpdating();
                    await Promise.race([
                        waitForState(registration.installing),
                        new Promise(function (resolve) { setTimeout(resolve, remaining()); })
                    ]);
                }

                if (reloading) return 'reloading';

                const waiting = registration.waiting;
                if (waiting && navigator.serviceWorker.controller) {
                    onUpdating();
                    this.activateWaiting(waiting);
                    await new Promise(function (resolve) {
                        const waitMs = Math.max(remaining(), 1200);
                        const t = setTimeout(resolve, waitMs);
                        navigator.serviceWorker.addEventListener('controllerchange', function () {
                            clearTimeout(t);
                            resolve();
                        });
                    });
                    if (!reloading) {
                        reloadOnce();
                    }
                    return 'reloading';
                }

                return reloading ? 'reloading' : 'ready';
            } catch {
                navigator.serviceWorker.register(SW_URL).catch(function () {});
                return reloading ? 'reloading' : 'timeout';
            }
        }
    };

    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        pwa.deferredPrompt = e;
        notify('OnPwaInstallAvailable');
    });

    window.addEventListener('appinstalled', function () {
        pwa.deferredPrompt = null;
        try {
            window.localStorage.removeItem(DISMISS_KEY);
        } catch {
            // ignore
        }
        notify('OnPwaInstalled');
    });

    window.pwa = pwa;

    window.checkForServiceWorkerUpdate = async function () {
        if (!('serviceWorker' in navigator)) return;
        try {
            const registration = await navigator.serviceWorker.getRegistration();
            if (registration) {
                await registration.update();
            }
        } catch (err) {
            console.error('Error checking for updates:', err);
        }
    };
})();
