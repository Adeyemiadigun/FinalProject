
  function adminDashboard() {
    
    return {
      statCards: [
        { label: 'Total Students', value: 248, bg: 'bg-blue-600', icon: '<path d=\"M17 20h5v-2a4 4 0 00-5-4H7a4 4 0 00-5 4v2h5\"/>' },
        { label: 'Total Assessments', value: 34, bg: 'bg-indigo-600', icon: '<path d=\"M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01\"/>' },
        { label: 'Active Assessments', value: 18, bg: 'bg-yellow-500', icon: '<path d=\"M5 13l4 4L19 7\"/>' },
        { label: 'Total Batches', value: 12, bg: 'bg-green-600', icon: '<path d=\"M3 7h18M3 12h18M3 17h18\"/>' },
        { label: 'Average Score', value: '72%', bg: 'bg-emerald-600', icon: '<path d=\"M9 12l2 2l4 -4\"/>' },
        { label: 'Completion Rate', value: '89%', bg: 'bg-purple-600', icon: '<path d=\"M3 3v18h18\"/>' }
      ],
      topStudents: [
        { id: 1, name: 'Grace Hill', averageScore: 95 },
        { id: 2, name: 'John Doe', averageScore: 91 },
        { id: 3, name: 'Alice King', averageScore: 89 }
      ],
      lowestStudents: [
{ name: 'Henry Ford', email: 'henry@example.com', averageScore: 42 },
{ name: 'Eliot Page', email: 'eliot@example.com', averageScore: 47 },
{ name: 'Maya Lin', email: 'maya@example.com', averageScore: 49 }
],
      initDashboard() {
        loadComponent('sidebar-placeholder', '../components/sidebar.html');
        loadComponent('navbar-placeholder', '../components/nav.html');
        this.renderCharts();
      },

      renderCharts() {
        new Chart(document.getElementById('topAssessmentChart'), {
          type: 'bar',
          data: {
            labels: ['Java Basics', 'Data Structures', 'HTML Quiz'],
            datasets: [{
              label: 'Average % Score',
              data: [92, 88, 85],
              backgroundColor: '#3b82f6'
            }]
          },
          options: {
            responsive: true,
            scales: { y: { beginAtZero: true, max: 100 } }
          }
        });

        new Chart(document.getElementById('lowAssessmentChart'), {
          type: 'bar',
          data: {
            labels: ['NodeJS', 'Python Intro', 'Git Basics'],
            datasets: [{
              label: 'Average % Score',
              data: [48, 52, 58],
              backgroundColor: '#ef4444'
            }]
          },
          options: {
            responsive: true,
            scales: { y: { beginAtZero: true, max: 100 } }
          }
        });

        new Chart(document.getElementById('batchChart'), {
          type: 'bar',
          data: {
            labels: ['Batch 1', 'Batch 2', 'Batch 3'],
            datasets: [{
              label: 'Student Count',
              data: [25, 32, 18],
              backgroundColor: '#10b981'
            }]
          },
          options: {
            responsive: true,
            scales: { y: { beginAtZero: true } }
          }
        });
      }
    }
  }

  async function loadComponent(id, path) {
    const res = await fetch(path);
    document.getElementById(id).innerHTML = await res.text();
  }