const API_BASE = import.meta.env.VITE_API_URL;

class ApiError extends Error {
    constructor(status, message) {
        super(message);

        this.name = "ApiError";
        this.status = status;
    }
}

export async function request(path, options = {}) {
    try {
        const response = await fetch(`${API_BASE}${path}`, {
            credentials: "include",
            ...options
        });

        if (response.status === 401) {

            throw new ApiError(
                401,
                "Unauthorized"
            );
        }

        if (!response.ok) {
            throw new ApiError(
                response.status,
                `Request failed: ${response.status}`
            );
        }

        return response;
    } catch (err) {
        if (err instanceof ApiError) {
            throw err;
        }

        throw new ApiError(
            0,
            "Unable to reach the server."
        );
    }
}

export async function apiGet(path) {
    return (await request(path)).json();
}

export async function apiGetText(path) {
    return (await request(path)).text();
}