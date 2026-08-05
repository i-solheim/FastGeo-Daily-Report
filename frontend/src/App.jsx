import ProjectListPage from "./pages/ProjectListPage";
import ProjectPage from "./pages/ProjectPage";
import { Route, Routes } from "react-router-dom"
import { Toaster } from "@/components/ui/toast";
import AuthCallbackPage from "./pages/AuthCallbackPage";

function App() {
  return (
    <>
    <Toaster>
      <Routes>
        <Route path="/" element={<ProjectListPage />} />
        <Route path="/project/:projectKey" element={<ProjectPage />} />
        <Route path="/auth/callback" element={<AuthCallbackPage />} />
      </Routes>
    </Toaster>
    </>
  );
}

export default App;