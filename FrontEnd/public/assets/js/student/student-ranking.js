  function loadLayout() {
    fetch("/public/components/sidebar-student.html")
      .then(res => res.text())
      .then(html => document.getElementById("sidebar-placeholder").innerHTML = html);

    fetch("/public/components/navbar-student.html")
      .then(res => res.text())
      .then(html => document.getElementById("navbar-placeholder").innerHTML = html);
  }

  function getToken() {
    return localStorage.getItem("accessToken");
  }

  function studentRanking() {
    return {
      viewMode: "my-batch",
      rankings: [],
      currentPage: 1,
      pageSize: 10,
      totalItems: 0,
      loading: false,

      get filteredRankings() {
        return this.rankings;
      },

      get paginatedRankings() {
        return this.filteredRankings;
      },

      nextPage() {
        if (this.currentPage * this.pageSize < this.totalItems) {
          this.currentPage++;
          this.fetchRankings();
        }
      },

      prevPage() {
        if (this.currentPage > 1) {
          this.currentPage--;
          this.fetchRankings();
        }
      },

      async init() {
        this.fetchRankings();
      },
      logOut() {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("userRole");
        window.location.href = "/public/auth/login.html";
      },

      async fetchRankings() {
        this.loading = true;
        const token = getToken();
        const params = new URLSearchParams({
          pageSize: this.pageSize,
          currentPage: this.currentPage,
        });

        try {
          const res = await fetch(
            `https://localhost:7157/api/v1/Students/batch/leaderboard?${params.toString()}`,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );

          const json = await res.json();

          if (json.status) {
            this.rankings = json.data.items.map((s) => ({
              id: s.id,
              name: s.name,
              score: s.avgScore,
              completed: s.completedAssessments,
              batch: "Batch", // Optional if you don't fetch batch name
              isCurrentUser: s.id === this.getCurrentUserId(),
            }));
            this.totalItems = json.data.totalItems;
          } else {
            alert(json.message || "Failed to load leaderboard.");
          }
        } catch (err) {
          console.error("Error loading leaderboard:", err);
        } finally {
          this.loading = false;
        }
      },

      getCurrentUserId() {
        // Decode JWT from localStorage
        try {
          const token = getToken();
          if (!token) return null;
          const payload = JSON.parse(atob(token.split(".")[1]));
          return payload.sub || payload.id || null;
        } catch (e) {
          return null;
        }
      },
    };
  }
