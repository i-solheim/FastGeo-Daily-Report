const API_BASE = import.meta.env.VITE_API_URL;

async function request(path) {
    const response = await fetch(`${API_BASE}${path}`, {
        credentials: "include",
    });

    if (!response.ok) {
        throw new Error(`Request failed: ${response.status}`);
    }

    return response;
}

export async function apiGet(path) {
    return (await request(path)).json();
}

export async function apiGetText(path) {
    return (await request(path)).text();
}