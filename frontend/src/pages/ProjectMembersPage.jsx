import { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { toast } from "@/components/ui/toast";
import {
    getProjectMembers,
    getAvailableUsers,
    addProjectMember,
    updateProjectMemberRole,
    removeProjectMember
} from "@/lib/dashboardApi";
import ErrorState from "@/components/ErrorState";
import { useAuth } from "../context/AuthContext";

function ProjectMembersPage() {
    const { projectKey } = useParams();
    const { user } = useAuth();
    const navigate = useNavigate();

    const [members, setMembers] = useState([]);
    const [availableUsers, setAvailableUsers] = useState([]);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [actionError, setActionError] = useState(null);

    const [showAddMember, setShowAddMember] = useState(false);
    const [selectedUserId, setSelectedUserId] = useState("");
    const [selectedRole, setSelectedRole] = useState("Member");
    const [submitting, setSubmitting] = useState(false);
    const [memberToRemove, setMemberToRemove] = useState(null);

    const isAdmin = user?.isAdmin === true;

    const loadMembers = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            const [membersData, usersData] =
                await Promise.all([
                    getProjectMembers(projectKey),
                    getAvailableUsers(projectKey)
                ]);

            setMembers(membersData.members);
            setAvailableUsers(usersData.users);
        } catch (err) {
            console.error(err);
            setError(err);
        } finally {
            setLoading(false);
        }
    }, [projectKey]);

    useEffect(() => {
        loadMembers();
    }, [loadMembers]);

    async function handleAddMember() {
        if (!selectedUserId) {
            return;
        }

        try {
            setSubmitting(true);

            await addProjectMember(
                projectKey,
                Number(selectedUserId),
                selectedRole
            );

            toast.add({
                title: "Member added",
                description: "The member was added to the project.",
                type: "success",
                duration: 2000
            });

            setShowAddMember(false);
            setSelectedUserId("");
            setSelectedRole("Member");

            await loadMembers();
        } catch (err) {
            console.error(err);
            toast.add({
                title: "Unable to add member",
                description: err.message,
                type: "error",
                duration: 3000
            });
        } finally {
            setSubmitting(false);
        }
    }

    async function handleRoleChange(userId, role) {
        try {
            await updateProjectMemberRole(
                projectKey,
                userId,
                role
            );

            toast.add({
                title: "Role updated",
                description: "The member's project role was updated.",
                type: "success",
                duration: 2000
            });

            await loadMembers();
        } catch (err) {
            console.error(err);
            setActionError(err);
            toast.add({
                title: "Unable to update role",
                description: err.message,
                type: "error",
                duration: 3000
            });
        } finally {
            setSubmitting(false);
        }
    }

    if (loading) {
        return (
            <div className="max-w-[60%] mx-auto">
                <p className="text-sm text-muted-foreground">
                    Loading members...
                </p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="max-w-[60%] mx-auto">
                <ErrorState
                    title="Couldn't load project members"
                    description="Please check your connection and try again."
                    onRetry={loadMembers}
                />
            </div>
        );
    }

    return (
        <div className="max-w-[60%] mx-auto">
            <button
                type="button"
                onClick={() =>
                    navigate(`/project/${projectKey}`)
                }
                className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground mb-6"
            >
                <ArrowLeft className="w-4 h-4" />
                Back to project
            </button>

            <div className="flex items-center justify-between mb-6">
                <div>
                    <h1 className="text-2xl font-semibold">
                        Project Members
                    </h1>

                    <p className="text-sm text-muted-foreground mt-1">
                        Members of {projectKey}
                    </p>
                </div>

                <button
                    type="button"
                    onClick={() => {
                        setSelectedRole("Member");
                        setSelectedUserId("");
                        setShowAddMember(true);
                    }}
                    className="px-4 py-2 rounded-md bg-primary text-primary-foreground text-sm"
                >
                    Add Member
                </button>
            </div>

            {showAddMember && (
                <div className="border rounded-lg p-5 mb-6">
                    <h2 className="font-semibold mb-4">
                        Add Member
                    </h2>

                    <div className="flex flex-col gap-4">
                        <div>
                            <label className="text-sm font-medium">
                                User
                            </label>

                            <select
                                value={selectedUserId}
                                onChange={e =>
                                    setSelectedUserId(e.target.value)
                                }
                                className="mt-1 w-full border rounded-md p-2"
                            >
                                <option value="">
                                    Select a user
                                </option>

                                {availableUsers.map(user => (
                                    <option
                                        key={user.id}
                                        value={user.id}
                                    >
                                        {user.displayName ||
                                            user.username}
                                        {" "}(@{user.username})
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div>
                            <label className="text-sm font-medium">
                                Role
                            </label>

                            <select
                                value={selectedRole}
                                onChange={e =>
                                    setSelectedRole(e.target.value)
                                }
                                className="mt-1 w-full border rounded-md p-2"
                            >
                                <option value="Member">
                                    Member
                                </option>

                                {isAdmin && (
                                    <option value="Leader">
                                        Leader
                                    </option>
                                )}
                            </select>
                        </div>

                        <div className="flex justify-end gap-2">
                            <button
                                type="button"
                                onClick={() => {
                                    setShowAddMember(false);
                                    setSelectedUserId("");
                                    setSelectedRole("Member");
                                }}
                                className="px-4 py-2 border rounded-md text-sm"
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                onClick={handleAddMember}
                                disabled={
                                    !selectedUserId ||
                                    submitting
                                }
                                className="px-4 py-2 rounded-md bg-primary text-primary-foreground text-sm disabled:opacity-50"
                            >
                                {submitting
                                    ? "Adding..."
                                    : "Add Member"}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            <div className="border rounded-lg divide-y">
                {members.length === 0 ? (
                    <div className="p-6 text-sm text-muted-foreground">
                        No members found.
                    </div>
                ) : (
                    members.map(member => (
                        <div
                            key={member.userId}
                            className="p-4 flex items-center justify-between"
                        >
                            <div>
                                <p className="font-medium">
                                    {member.displayName ||
                                        member.username}
                                </p>

                                <p className="text-sm text-muted-foreground">
                                    @{member.username}
                                </p>
                            </div>

                            <div className="flex items-center gap-3">
                                {isAdmin ? (
                                    <select
                                        value={member.role}
                                        onChange={e =>
                                            handleRoleChange(
                                                member.userId,
                                                e.target.value
                                            )
                                        }
                                        className="border rounded-md px-2 py-1 text-sm"
                                    >
                                        <option value="Member">
                                            Member
                                        </option>

                                        <option value="Leader">
                                            Leader
                                        </option>
                                    </select>
                                ) : (
                                    <span className="text-sm">
                                        {member.role}
                                    </span>
                                )}

                                {(isAdmin || member.role === "Member") && (
                                    <button
                                        type="button"
                                        onClick={() => {
                                            setActionError(null);
                                            setMemberToRemove(member);
                                        }}
                                        className="px-3 py-1 rounded-md border text-sm text-destructive hover:bg-destructive/10"
                                    >
                                        Remove
                                    </button>
                                )}
                            </div>
                        </div>
                    ))
                )}
            </div>

            {memberToRemove && (
                <div className="fixed inset-0 z-50 flex items-center justify-center">
                    {/* Backdrop */}
                    <div
                        className="absolute inset-0 bg-black/50"
                        onClick={() => setMemberToRemove(null)}
                    />

                    {/* Modal */}
                    <div className="relative w-full max-w-md rounded-lg bg-background border shadow-lg p-6">
                        <h2 className="text-lg font-semibold">
                            Remove project member?
                        </h2>

                        <p className="text-sm text-muted-foreground mt-2">
                            Are you sure you want to remove{" "}
                            <span className="font-medium text-foreground">
                                {memberToRemove.displayName ||
                                    memberToRemove.username}
                            </span>{" "}
                            from this project?
                        </p>

                        <div className="flex justify-end gap-2 mt-6">
                            <button
                                type="button"
                                onClick={() => setMemberToRemove(null)}
                                className="px-4 py-2 border rounded-md text-sm"
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                disabled={submitting}
                                onClick={async () => {
                                    try {
                                        setSubmitting(true);

                                        await removeProjectMember(
                                            projectKey,
                                            memberToRemove.userId
                                        );

                                        setMemberToRemove(null);

                                        toast.add({
                                            title: "Member removed",
                                            description: "The member was removed from the project.",
                                            type: "success",
                                            duration: 2000
                                        });

                                        await loadMembers();
                                    } catch (err) {
                                        console.error(err);
                                        toast.add({
                                            title: "Unable to remove member",
                                            description: err.message,
                                            type: "error",
                                            duration: 3000
                                        });
                                    } finally {
                                        setSubmitting(false);
                                    }
                                }}
                                className="px-4 py-2 rounded-md bg-destructive text-destructive-foreground text-sm disabled:opacity-50"
                            >
                                {submitting ? "Removing..." : "Remove"}
                            </button>
                        </div>
                    </div>
                </div>
            )}

        </div>
    );
}

export default ProjectMembersPage;