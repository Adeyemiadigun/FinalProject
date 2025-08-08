// ✅ Use relative path to shared utils
import { api, loadComponent, logOut } from "../shared/utils.js";

window.adminAssessmentsPage = function () {
  return {
    assessments: [],
    batches: [],
    filters: { batchId: "", startDate: "", endDate: "", search: "" },
    pagination: {
      currentPage: 1,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    },
    pageSize: 5,
    logOut,
    async init() {
      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");
      await this.fetchBatches();
      await this.fetchAssessments();
    },

    async fetchBatches() {
      const res = await api.get("/Batches/all");
      const data = await res.json();
      this.batches = data.data || [];
    },

    async fetchAssessments() {
      const params = new URLSearchParams({
        currentPage: this.pagination.currentPage,
        pageSize: this.pageSize,
      });
      if (this.filters.batchId) params.append("batchId", this.filters.batchId);
      if (this.filters.startDate)
        params.append("startDate", this.filters.startDate);
      if (this.filters.endDate) params.append("endDate", this.filters.endDate);
      if (this.filters.search) params.append("search", this.filters.search);

      const res = await api.get(`/assessments?${params}`);
      const result = await res.json();

      if (res.ok) {
        this.assessments = result.data.items;
        this.pagination = {
          currentPage: result.data.currentPage,
          totalPages: result.data.totalPages,
          hasNextPage: result.data.hasNextPage,
          hasPreviousPage: result.data.hasPreviousPage,
        };
      } else {
        this.assessments = [];
      }
    },

    applyFilters() {
      this.pagination.currentPage = 1;
      this.fetchAssessments();
    },

    prevPage() {
      if (this.pagination.hasPreviousPage) {
        this.pagination.currentPage--;
        this.fetchAssessments();
      }
    },

    nextPage() {
      if (this.pagination.hasNextPage) {
        this.pagination.currentPage++;
        this.fetchAssessments();
      }
    },
  };
};
