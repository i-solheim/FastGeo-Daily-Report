import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Link } from "react-router-dom"

const API_URL = import.meta.env.VITE_API_URL;

function ProjectListPage() {
    const [projects, setProjects] = useState([]);

    useEffect(() => {
        fetch(`${API_URL}/api/projects`)
            .then(response => response.json())
            .then(data => setProjects(data.projects));
    }, []);

    return <>
        {projects.map(project => (
            <Link key={project} to={`/project/${project}`}>
                <Card key={project}>
                    <CardHeader>
                        <CardTitle>{project}</CardTitle>
                    </CardHeader>
                </Card>
            </Link>
        ))}
    </>;
}

export default ProjectListPage;