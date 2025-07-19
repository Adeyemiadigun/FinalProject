function loadLayout() {
  fetch("/public/components/sidebar-student.html")
    .then((res) => res.text())
    .then(
      (html) =>
        (document.getElementById("sidebar-placeholder").innerHTML = html)
    );
  fetch("/public/components/navbar-student.html")
    .then((res) => res.text())
    .then(
      (html) => (document.getElementById("navbar-placeholder").innerHTML = html)
    );
}

function studentProfile() {
  return {
    student: { name: "", email: "", batch: "", joinedAt: "" },
    summary: { avgScore: 0, completedAssessments: 0, passRate: 0, rank: 0 },
    history: [],
    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
    async init() {
      const token = localStorage.getItem("accessToken");
    
console.log(token)
      try {
        // Get Student Info
        const detailRes = await fetch(
          "https://localhost:7157/api/v1/students/details",
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        const detailJson = await detailRes.json();
        const s = detailJson.data;
        this.student = {
          name: s.fullName,
          email: s.email,
          batch: s.batchName ?? "N/A",
          joinedAt: new Date(s.dateCreated).toLocaleDateString(),
        };

        // Get Student Metrics
        const metricRes = await fetch(
          "https://localhost:7157/api/v1/students/metrics/summary",
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        const metricJson = await metricRes.json();
        const m = metricJson.data;
        this.summary = {
          avgScore: m.averageScore.toFixed(1),
          completedAssessments: m.submmittedCount,
          passRate: m.passRate.toFixed(1),
          rank: m.rank,
        };

        // Get History
        const historyRes = await fetch(
          "https://localhost:7157/api/v1/students/history",
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        const historyJson = await historyRes.json();
        this.history = historyJson.data.map((x) => ({
          id: x.assessmentId,
          title: x.assessmentTitle,
          date: x.submittedAt,
          score: x.totalScore,
          passed: x.totalScore >= 50, // or compare with passing score if available
        }));
      } catch (err) {
        console.error("Failed to load student profile", err);
        alert("Failed to load profile. Try again later.");
      }
    },
  };
}
