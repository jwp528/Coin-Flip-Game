export function subscribeVisibility(dotNetRef) {
    const hidden = () => {
        if (document.visibilityState === "hidden") {
            dotNetRef.invokeMethodAsync("OnVisibilityHidden");
        }
    };
    const pageHide = () => dotNetRef.invokeMethodAsync("OnVisibilityHidden");
    document.addEventListener("visibilitychange", hidden);
    window.addEventListener("pagehide", pageHide);
    return {
        dispose: () => {
            document.removeEventListener("visibilitychange", hidden);
            window.removeEventListener("pagehide", pageHide);
        }
    };
}
