// Coin Flip Game — PWA install + update helpers for Blazor
(function () {
    const DISMISS_KEY = 'pwaInstallDismissed';
    const listeners = [];

    const pwa = {
        deferredPrompt: null,
        waitingWorker: null,
        updateAvailable: false,

        register: function (dotNetRef) {
            if (dotNetRef && listeners.indexOf(dotNetRef) === -1) {
                listeners.push(dotNetRef);
            }
            if (this.deferredPrompt) {
                notify('OnPwaInstallAvailable');
            }
            if (this.updateAvailable) {
                notify('OnPwaUpdateAvailable');
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

        applyUpdate: function () {
            const worker = this.waitingWorker;
            if (worker) {
                worker.postMessage({ type: 'SKIP_WAITING' });
            }
            let reloading = false;
            navigator.serviceWorker.addEventListener('controllerchange', function () {
                if (reloading) return;
                reloading = true;
                window.location.reload();
            });
            // If the worker was already active (skipWaiting in install), just reload.
            setTimeout(function () {
                if (!reloading) {
                    window.location.reload();
                }
            }, 400);
        }
    };

    function notify(method) {
        listeners.forEach(function (ref) {
            try {
                ref.invokeMethodAsync(method);
            } catch (err) {
                console.warn('[PWA] notify failed', method, err);
            }
        });
    }

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

    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.addEventListener('controllerchange', function () {
            // Handled in applyUpdate when user opts in
        });
    }

    window.pwa = pwa;
    window.pwaOnUpdateFound = function (worker) {
        pwa.waitingWorker = worker;
        pwa.updateAvailable = true;
        notify('OnPwaUpdateAvailable');
    };
})();
