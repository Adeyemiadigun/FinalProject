
async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}

function assessmentDashboard() {
  return {
    selectedBatch: '',
    selectedInstructor: '',
    batches: [],
    instructors: [],

    async init() {
      const token = localStorage.getItem('token');
      this.headers = { 'Authorization': `Bearer ${token}` };

      await loadComponent('sidebar-placeholder', '../components/sidebar.html');
      await loadComponent('navbar-placeholder', '../components/nav.html');

      await this.loadDropdowns();
      await this.reloadData();
    },

    async loadDropdowns() {
      const [batchRes, instructorRes] = await Promise.all([
        fetch('/api/v1/batches', { headers: this.headers }),
        fetch('/api/v1/instructors', { headers: this.headers })
      ]);

      this.batches = await batchRes.json();
      this.instructors = await instructorRes.json();
    },

    async reloadData() {
      const query = new URLSearchParams();
      if (this.selectedBatch) query.append('batchId', this.selectedBatch);
      if (this.selectedInstructor) query.append('instructorId', this.selectedInstructor);

      const [metrics, scoreTrends, createdTrends, recents] = await Promise.all([
        fetch(`/api/v1/admin/assessments/metrics?${query}`, { headers: this.headers }).then(res => res.json()),
        fetch(`/api/v1/admin/analytics/assessments/score-trends?${query}`, { headers: this.headers }).then(res => res.json()),
        fetch(`/api/v1/admin/analytics/assessments/created-trend?${query}`, { headers: this.headers }).then(res => res.json()),
        fetch(`/api/v1/admin/assessments/recents?${query}`, { headers: this.headers }).then(res => res.json())
      ]);

      this.renderMetrics(metrics);
      this.renderCharts(scoreTrends, createdTrends);
      this.renderAssessments(recents.data);
    },

    renderMetrics(metrics) {
      document.getElementById('metrics-cards').innerHTML = `
        <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
          <div class="bg-white p-4 rounded shadow">Total: ${metrics.totalAssessments}</div>
          <div class="bg-white p-4 rounded shadow">Active: ${metrics.activeAssessments}</div>
          <div class="bg-white p-4 rounded shadow">Avg Score: ${metrics.averageScore}%</div>
          <div class="bg-white p-4 rounded shadow">Pass Rate: ${metrics.passRate}%</div>
          <div class="bg-white p-4 rounded shadow">Completion: ${metrics.completionRate}%</div>
        </div>`;
    },

    renderCharts(scoreTrend, createdTrend) {
      new Chart(document.getElementById("scoreTrendsChart"), {
        type: 'line',
        data: {
          labels: scoreTrend.map(d => d.label),
          datasets: [{
            label: 'Avg. Score',
            data: scoreTrend.map(d => d.averageScore),
            borderColor: '#4f46e5',
            tension: 0.4
          }]
        },
        options: { responsive: true, scales: { y: { beginAtZero: true } } }
      });

      new Chart(document.getElementById("createdOverTimeChart"), {
        type: 'line',
        data: {
          labels: createdTrend.map(d => d.label),
          datasets: [{
            label: 'Assessments Created',
            data: createdTrend.map(d => d.count),
            borderColor: '#10b981',
            tension: 0.4
          }]
        },
        options: { responsive: true, scales: { y: { beginAtZero: true } } }
      });
    },

    renderAssessments(data) {
      const tbody = document.getElementById('assessment-table-body');
      tbody.innerHTML = data.map(x => `
        <tr class="border-t">
          <td class="p-2">${x.title}</td>
          <td class="p-2">${x.type ?? 'N/A'}</td>
          <td class="p-2">${x.instructor ?? 'N/A'}</td>
          <td class="p-2">${x.batch ?? 'N/A'}</td>
          <td class="p-2 text-green-600 font-medium">${x.status ?? 'N/A'}</td>
          <td class="p-2 space-x-2">
            <button class="text-blue-600 hover:underline">View</button>
            <button class="text-yellow-600 hover:underline">Edit</button>
            <button class="text-red-600 hover:underline">Delete</button>
          </td>
        </tr>
      `).join('');
    }
  };
}