// Dashboard JavaScript

// Global chart instances
let assetStatusChart = null;
let assetActivityChart = null;

document.addEventListener('DOMContentLoaded', function() {
    // Initialize loading state
    const loadingOverlay = document.getElementById('loadingOverlay');
    if (loadingOverlay) {
        setTimeout(() => {
            loadingOverlay.classList.add('d-none');
        }, 500);
    }

    // Animate stats cards on scroll
    const statsCards = document.querySelectorAll('.stats-card');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-fade-in');
            }
        });
    }, { threshold: 0.1 });

    statsCards.forEach(card => observer.observe(card));

    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Handle quick action buttons
    const actionButtons = document.querySelectorAll('.action-btn');
    actionButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (this.getAttribute('href') === '#') {
                e.preventDefault();
                showNotification('هذا القسم قيد التطوير', 'info');
            }
        });
    });

    // Notification system
    window.showNotification = function(message, type = 'info') {
        const toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) return;

        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type} border-0`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        toastContainer.appendChild(toast);
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();

        // Remove toast after it's hidden
        toast.addEventListener('hidden.bs.toast', function () {
            toast.remove();
        });
    };

    // Refresh dashboard data
    const refreshButton = document.querySelector('.refresh-btn');
    if (refreshButton) {
        refreshButton.addEventListener('click', function() {
            this.classList.add('fa-spin');
            setTimeout(() => {
                this.classList.remove('fa-spin');
                showNotification('تم تحديث البيانات بنجاح', 'success');
            }, 1000);
        });
    }

    // Handle card collapse/expand
    const collapseButtons = document.querySelectorAll('.collapse-btn');
    collapseButtons.forEach(button => {
        button.addEventListener('click', function() {
            const icon = this.querySelector('i');
            if (icon.classList.contains('fa-minus')) {
                icon.classList.replace('fa-minus', 'fa-plus');
            } else {
                icon.classList.replace('fa-plus', 'fa-minus');
            }
        });
    });
});

// Initialize dashboard charts
function initializeCharts() {
    console.log('Starting chart initialization...');

    try {
        // Asset Status Chart
        const assetStatusCtx = document.getElementById('assetStatusChartCanvas');
        if (!assetStatusCtx) {
            console.error('Asset status chart canvas not found');
            return;
        }

        console.log('Found asset status chart canvas');
        
        // Get stats values
        const statsCards = document.querySelectorAll('.stats-card');
        if (statsCards.length < 4) {
            console.error('Not enough stats cards found');
            return;
        }

        const availableAssets = parseInt(statsCards[1].querySelector('.stats-value').textContent) || 0;
        const assignedAssets = parseInt(statsCards[2].querySelector('.stats-value').textContent) || 0;
        const maintenanceAssets = parseInt(statsCards[3].querySelector('.stats-value').textContent) || 0;

        console.log('Asset stats:', { availableAssets, assignedAssets, maintenanceAssets });

        // Destroy existing chart if it exists
        if (assetStatusChart) {
            assetStatusChart.destroy();
        }

        // Create new chart
        assetStatusChart = new Chart(assetStatusCtx, {
            type: 'doughnut',
            data: {
                labels: ['متاح', 'مخصص', 'قيد الصيانة'],
                datasets: [{
                    data: [availableAssets, assignedAssets, maintenanceAssets],
                    backgroundColor: ['#2fb344', '#f59f00', '#d63939'],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        rtl: true,
                        labels: {
                            font: {
                                family: 'Tajawal',
                                size: 14
                            },
                            padding: 20
                        }
                    },
                    tooltip: {
                        rtl: true,
                        titleFont: {
                            family: 'Tajawal',
                            size: 14
                        },
                        bodyFont: {
                            family: 'Tajawal',
                            size: 14
                        }
                    }
                },
                cutout: '70%'
            }
        });

        // Asset Activity Chart
        const assetActivityCtx = document.getElementById('assetActivityChartCanvas');
        if (!assetActivityCtx) {
            console.error('Asset activity chart canvas not found');
            return;
        }

        console.log('Found asset activity chart canvas');

        // Get the last 6 months
        const months = [];
        const currentDate = new Date();
        for (let i = 5; i >= 0; i--) {
            const date = new Date(currentDate.getFullYear(), currentDate.getMonth() - i, 1);
            months.push(date.toLocaleDateString('ar-SA', { month: 'long' }));
        }

        // Generate random data for demonstration
        const activityData = months.map(() => Math.floor(Math.random() * 30) + 10);

        console.log('Activity data:', { months, activityData });

        // Destroy existing chart if it exists
        if (assetActivityChart) {
            assetActivityChart.destroy();
        }

        // Create new chart
        assetActivityChart = new Chart(assetActivityCtx, {
            type: 'line',
            data: {
                labels: months,
                datasets: [{
                    label: 'نشاط الأصول',
                    data: activityData,
                    borderColor: '#206bc4',
                    backgroundColor: 'rgba(32, 107, 196, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        rtl: true,
                        labels: {
                            font: {
                                family: 'Tajawal',
                                size: 14
                            },
                            padding: 20
                        }
                    },
                    tooltip: {
                        rtl: true,
                        titleFont: {
                            family: 'Tajawal',
                            size: 14
                        },
                        bodyFont: {
                            family: 'Tajawal',
                            size: 14
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            font: {
                                family: 'Tajawal',
                                size: 12
                            }
                        }
                    },
                    x: {
                        ticks: {
                            font: {
                                family: 'Tajawal',
                                size: 12
                            }
                        }
                    }
                }
            }
        });

        console.log('Charts initialized successfully');
    } catch (error) {
        console.error('Error initializing charts:', error);
    }
} 