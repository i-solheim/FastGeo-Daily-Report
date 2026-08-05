const API_BASE = import.meta.env.VITE_API_URL;

function authHeaders() {
    const token = localStorage.getItem("token");

    return token
        ? { Authorization: `Bearer ${token}` }
        : {};
}

async function request(path) {
    const response = await fetch(`${API_BASE}${path}`, {
        headers: authHeaders(),
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