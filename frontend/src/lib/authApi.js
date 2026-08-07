import { request } from "./api";

export async function getCurrentUser() {
    return (await request("/auth/me")).json();
}

export async function logoutUser() {
    await request("/auth/logout", {
        method: "POST",
    });
}