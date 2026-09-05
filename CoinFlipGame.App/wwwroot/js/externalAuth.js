function base64Url(bytes) {
    let binary = "";
    bytes.forEach(value => binary += String.fromCharCode(value));
    return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

export async function start(options) {
    if (!options.authorizationEndpoint || !options.clientId || !options.scope || !options.redirectUri)
        throw new Error("external-auth-not-configured");
    const verifier = base64Url(crypto.getRandomValues(new Uint8Array(48)));
    const challenge = base64Url(new Uint8Array(await crypto.subtle.digest("SHA-256", new TextEncoder().encode(verifier))));
    const state = base64Url(crypto.getRandomValues(new Uint8Array(24)));
    sessionStorage.setItem("coinflip.external-auth.v1", JSON.stringify({
        verifier, state, mode: options.mode || "login"
    }));
    const url = new URL(options.authorizationEndpoint);
    url.searchParams.set("client_id", options.clientId);
    url.searchParams.set("response_type", "code");
    url.searchParams.set("redirect_uri", options.redirectUri);
    url.searchParams.set("response_mode", "query");
    url.searchParams.set("scope", options.scope);
    url.searchParams.set("code_challenge", challenge);
    url.searchParams.set("code_challenge_method", "S256");
    url.searchParams.set("state", state);
    // Do not add login_hint. Entra External ID can lose user-flow session context (AADSTS165000).
    if (options.createAccount) url.searchParams.set("prompt", "create");
    // Do not add domain_hint for Google. Some External ID tenants reject issuer acceleration (AADSTS90023).
    location.assign(url.toString());
}

export async function complete(options) {
    try {
        const saved = JSON.parse(sessionStorage.getItem("coinflip.external-auth.v1") || "null");
        const query = new URLSearchParams(location.search);
        if (!saved || query.get("state") !== saved.state) return { success: false, error: "The sign-in response could not be verified." };
        if (query.has("error")) return { success: false, error: query.get("error_description") || "Sign-in was cancelled." };
        const code = query.get("code");
        if (!code) return { success: false, error: "The identity provider returned no authorization code." };
        const body = new URLSearchParams({
            client_id: options.clientId,
            grant_type: "authorization_code",
            code,
            redirect_uri: options.redirectUri,
            code_verifier: saved.verifier,
            scope: ""
        });
        const response = await fetch(options.tokenEndpoint, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body
        });
        const token = await response.json();
        sessionStorage.removeItem("coinflip.external-auth.v1");
        if (!response.ok || !token.access_token) return { success: false, error: token.error_description || "Sign-in token exchange failed." };
        return { success: true, accessToken: token.access_token, mode: saved.mode };
    } catch {
        return { success: false, error: "Social sign-in could not be completed." };
    }
}
