import { apiGet, apiGetText } from "./api";

export function getProjects() {
    return apiGet("/api/projects");
}

export function getChanges(projectKey, date) {
    return apiGet(
        `/api/projects/${projectKey}/changes?date=${date}`
    );
}

export function getSummary(projectKey, date) {
    return apiGet(
        `/api/projects/${projectKey}/summary?date=${date}`
    );
}

export function getReport(projectKey, date) {
    return apiGetText(
        `/api/projects/${projectKey}/report?date=${date}`
    );
}