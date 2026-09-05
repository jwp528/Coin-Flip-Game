export function subscribeVisibility(dotNetRef) {
    const onChange = () => {
        if (document.visibilityState === "hidden") {
            dotNetRef.invokeMethodAsync("OnVisibilityHidden");
        } else {
            dotNetRef.invokeMethodAsync("OnVisibilityShown");
        }
    };
    const pageHide = () => dotNetRef.invokeMethodAsync("OnVisibilityHidden");
    document.addEventListener("visibilitychange", onChange);
    window.addEventListener("pagehide", pageHide);
    return {
        dispose: () => {
            document.removeEventListener("visibilitychange", onChange);
            window.removeEventListener("pagehide", pageHide);
        }
    };
}
