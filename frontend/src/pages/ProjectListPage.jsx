import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Link } from "react-router-dom"
import { getProjects } from "@/lib/dashboardApi";
import { useAuth } from "../context/AuthContext";
import ProjectCardSkeleton from "@/components/ProjectCardSkeleton";

function ProjectListPage() {
    const [projects, setProjects] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {

        async function loadProjects() {

            try {
                setLoading(true);
                await new Promise(resolve => setTimeout(resolve, 2000));
                const data = await getProjects();
                setProjects(data.projects);
            } catch (error) {
                console.error(error);
                setError(error);
            } finally {
                setLoading(false);
            }
        }

        loadProjects();
    }, []);

    if (loading) {
        return (
            <div className="space-y-4">
                {[...Array(5)].map((_, index) => (
                    <ProjectCardSkeleton key={index} />
                ))}
            </div>
        );
    }

    if (error) {
        return (
            <p className="text-red-500">
                Failed to load projects.
            </p>
        );
    }

    return <>
        <div className="space-y-4">
            {projects.map(project => (
                <Link key={project} to={`/project/${project}`}>
                    <Card key={project}>
                        <CardHeader>
                            <CardTitle>{project}</CardTitle>
                        </CardHeader>
                    </Card>
                </Link>
            ))}
        </div>
    </>;
}

export default ProjectListPage;