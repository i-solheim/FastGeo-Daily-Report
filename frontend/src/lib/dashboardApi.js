import { apiGet, apiGetText, apiPost, apiPatch, apiDelete } from "./api";

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

export function getUsers() {
    return apiGet("/api/admin/users");
}

export function getProjectMembership(projectKey) {
    return apiGet(
        `/api/projects/${projectKey}/membership`
    );
}

export function getProjectMembers(projectKey) {
    return apiGet(
        `/api/projects/${projectKey}/members`
    );
}

export function addProjectMember(
    projectKey,
    userId,
    role
) {
    return apiPost(
        `/api/projects/${projectKey}/members`,
        {
            userId,
            role
        }
    );
}

export function updateProjectMemberRole(
    projectKey,
    userId,
    role
) {
    return apiPatch(
        `/api/admin/projects/${projectKey}/members/${userId}`,
        {
            role
        }
    );
}

export function removeProjectMember(
    projectKey,
    userId
) {
    return apiDelete(
        `/api/projects/${projectKey}/members/${userId}`
    );
}

export function getAvailableUsers(projectKey) {
    return apiGet(
        `/api/projects/${projectKey}/members/available-users`
    );
}