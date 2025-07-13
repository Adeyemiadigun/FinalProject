function instructorDashboard() {
  return {
    summary: {
      totalAssessments: 0,
      totalStudents: 0,
      avgScore: 0,
      passRate: 0,
    },
    batches: [],
    recentAssessments: [],
    assessmentScoreTrends: [],

    async loadData() {
      const token = localStorage.getItem("accessToken");
      console.log("Token:", token);
    
      const headers = {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      };
    try{
      this.summary = await fetch(
        "https://localhost:7157/api/v1/Dashboard/instructor/metrics/overview",
        {
          method: "GET",
          headers: headers,
        }
      ).then((r) => r.json());
    
      this.batches = await fetch(
        "https://localhost:7157/api/v1/Dashboard/batch-distribution",
        {
          method: "GET",
          headers: headers,
        }
      ).then((r) => r.json());
    
      this.recentAssessments = await fetch(
        "https://localhost:7157/api/v1/Instructors/assessment/recents",
        {
          method: "GET",
          headers: headers,
        }
      ).then((r) => r.json());
    
      this.assessmentScoreTrends = await fetch(
        "https://localhost:7157/api/v1/Assessments/assessment-scores",
        {
          method: "GET",
          headers: headers,
        }
      ).then((r) => r.json());
    
      this.drawCharts();
    }
    catch (error) {
      console.error("Error loading data:", error);
      alert("Failed to load data. Please try again later.");
    }
    },

    drawCharts() {
      new Chart(document.getElementById("assessmentScoreChart"), {
        type: "bar",
        data: {
          labels: this.assessmentScoreTrends.map((x) => x.assessmentTitle),
          datasets: [
            {
              label: "Average Score (%)",
              data: this.assessmentScoreTrends.map((x) => x.averageScore),
              backgroundColor: "#3b82f6",
            },
          ],
        },
      });

      new Chart(document.getElementById("studentBatchChart"), {
        type: "pie",
        data: {
          labels: this.batches.map((b) => b.name),
          datasets: [
            {
              data: this.batches.map((b) => b.studentCount),
              backgroundColor: ["#3b82f6", "#f59e0b", "#10b981"],
            },
          ],
        },
      });
    },
  };
}

async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
