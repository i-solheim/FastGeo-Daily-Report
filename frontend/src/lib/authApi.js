const API_URL = import.meta.env.VITE_API_URL;

export async function getCurrentUser() {
    const response = await fetch(`${API_URL}/auth/me`, {
        credentials: "include",
    });

    if (!response.ok) {
        return null;
    }

    return response.json();
}

export async function logoutUser() {
    await fetch(`${API_URL}/auth/logout`, {
        method: "POST",
        credentials: "include",
    });
}