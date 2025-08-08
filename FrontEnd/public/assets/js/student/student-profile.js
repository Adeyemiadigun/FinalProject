import { api, loadComponent, logOut } from "../utils.js";

window.studentProfile = function () {
  return {
    student: { name: "", email: "", batch: "", joinedAt: "" },
    summary: { avgScore: 0, completedAssessments: 0, passRate: 0, rank: 0 },
    history: [],

    logOut() {
      logOut(); // Use the shared logout function
    },

    async init() {
      const token = localStorage.getItem("accessToken");
      if (!token) {
        logOut();
        return;
      }

      try {
        // Load Layout Components
        await Promise.all([
          loadComponent(
            "sidebar-placeholder",
            "/public/components/sidebar-student.html"
          ),
          loadComponent(
            "navbar-placeholder",
            "/public/components/navbar-student.html"
          ),
        ]);

        // Fetch Student Details
        const detailRes = await api.get("/students/details");
        const detailJson = await detailRes.json();
        const s = detailJson.data;
        this.student = {
          name: s.fullName,
          email: s.email,
          batch: s.batchName ?? "N/A",
          joinedAt: new Date(s.dateCreated).toLocaleDateString(),
        };

        // Fetch Student Metrics
        const metricRes = await api.get("/students/metrics/summary");
        const metricJson = await metricRes.json();
        const m = metricJson.data;
        this.summary = {
          avgScore: m.averageScore.toFixed(1),
          completedAssessments: m.submmittedCount,
          passRate: m.passRate.toFixed(1),
          rank: m.rank,
        };

        // Fetch Submission History
        const historyRes = await api.get("/students/history");
        const historyJson = await historyRes.json();
        this.history = historyJson.data.map((x) => ({
          id: x.assessmentId,
          title: x.assessmentTitle,
          date: new Date(x.submittedAt).toLocaleDateString(),
          score: x.totalScore,
          passed: x.totalScore >= 50, // adjust if pass mark varies
        }));
      } catch (err) {
        console.error("Failed to load student profile", err);
        Swal.fire("Error", "Failed to load profile. Try again later.", "error");
      }
    },
  };
};
