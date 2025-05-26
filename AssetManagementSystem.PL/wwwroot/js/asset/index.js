// Enterprise Asset Management System - Modern Implementation

class AssetManagementSystem {
    constructor() {
        this.initializeComponents();
        this.setupEventListeners();
        this.loadInitialData();
    }

    initializeComponents() {
        // Initialize charts
        this.initializeCharts();
        
        // Initialize tooltips
        this.initializeTooltips();
        
        // Initialize animations
        this.initializeAnimations();
        
        // Initialize toast notifications
        this.initializeToastContainer();
    }

    initializeCharts() {
        // Total Assets Trend Chart
        this.initializeTotalAssetsChart();
        
        // Asset Status Distribution Chart
        this.initializeStatusChart();
        
        // Asset Utilization Chart
        this.initializeUtilizationChart();
    }

    initializeTotalAssetsChart() {
        const ctx = document.getElementById('totalAssetsChart');
        if (!ctx) return;

        this.totalAssetsChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Total Assets',
                    data: [],
                    borderColor: '#3498db',
                    backgroundColor: 'rgba(52, 152, 219, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        backgroundColor: 'rgba(255, 255, 255, 0.9)',
                        titleColor: '#2c3e50',
                        bodyColor: '#2c3e50',
                        borderColor: '#e9ecef',
                        borderWidth: 1,
                        padding: 12,
                        boxPadding: 6,
                        usePointStyle: true,
                        callbacks: {
                            label: function(context) {
                                return `Total Assets: ${context.parsed.y}`;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            display: true,
                            drawBorder: false,
                            color: 'rgba(0, 0, 0, 0.05)'
                        },
                        ticks: {
                            font: {
                                family: "'Inter', sans-serif"
                            }
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            font: {
                                family: "'Inter', sans-serif"
                            }
                        }
                    }
                }
            }
        });
    }

    initializeStatusChart() {
        const ctx = document.getElementById('statusChart');
        if (!ctx) return;

        this.statusChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Active', 'Inactive', 'Maintenance'],
                datasets: [{
                    data: [],
                    backgroundColor: ['#2ecc71', '#e74c3c', '#e67e22'],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '75%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            font: {
                                family: "'Inter', sans-serif"
                            }
                        }
                    }
                }
            }
        });
    }

    initializeUtilizationChart() {
        const ctx = document.getElementById('utilizationChart');
        if (!ctx) return;

        this.utilizationChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['In Use', 'Available', 'Reserved'],
                datasets: [{
                    data: [],
                    backgroundColor: ['#3498db', '#2ecc71', '#e67e22'],
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            display: false
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }

    initializeTooltips() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl, {
                trigger: 'hover',
                placement: 'top',
                animation: true,
                template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>'
            });
        });
    }

    initializeAnimations() {
        // Animate stats cards on scroll
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-fade-in');
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.1
        });

        document.querySelectorAll('.stats-card').forEach(card => {
            observer.observe(card);
        });
    }

    initializeToastContainer() {
        const container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    setupEventListeners() {
        // Refresh Stats Button
        const refreshBtn = document.getElementById('refreshStats');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.handleRefresh());
        }

        // Search Input
        const searchInput = document.getElementById('assetSearch');
        if (searchInput) {
            searchInput.addEventListener('input', debounce((e) => this.handleSearch(e.target.value), 300));
        }

        // Quick Action Buttons
        document.querySelectorAll('.action-button').forEach(btn => {
            btn.addEventListener('click', (e) => this.handleQuickAction(e));
        });

        // Bulk Actions
        const bulkSelect = document.getElementById('bulkSelect');
        if (bulkSelect) {
            bulkSelect.addEventListener('change', () => this.handleBulkSelection());
        }

        // Export Buttons
        document.querySelectorAll('.export-btn').forEach(btn => {
            btn.addEventListener('click', (e) => this.handleExport(e));
        });
    }

    async loadInitialData() {
        try {
            // Show loading state
            this.showLoadingState();
            
            // Fetch data from server
            const [stats, assets] = await Promise.all([
                this.fetchAssetStats(),
                this.fetchAssets()
            ]);
            
            // Update UI with data
            if (stats) {
                this.updateStats(stats);
                this.updateCharts(stats);
            }
            
            if (assets) {
                this.updateAssetsTable(assets);
            }
            
            // Hide loading state
            this.hideLoadingState();
        } catch (error) {
            console.error('Error loading initial data:', error);
            this.showNotification('Error loading data. Please try again.', 'error');
            this.hideLoadingState();
        }
    }

    async fetchAssetStats() {
        try {
            const response = await fetch('/Asset/GetAssetStats', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching asset stats:', error);
            this.showNotification('Failed to load asset statistics', 'error');
            return null;
        }
    }

    async fetchAssets() {
        try {
            const response = await fetch('/Asset/GetAssets', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching assets:', error);
            this.showNotification('Failed to load assets', 'error');
            return null;
        }
    }

    updateStats(stats) {
        if (!stats) return;

        // Update stats cards with animation
        Object.entries(stats).forEach(([key, value]) => {
            const element = document.querySelector(`[data-stat="${key}"]`);
            if (element) {
                const currentValue = parseInt(element.textContent.replace(/,/g, '')) || 0;
                this.animateNumber(element, currentValue, value, 1500);
            }
        });

        // Update trends if available
        if (stats.trends) {
            this.updateTrends(stats.trends);
        }
    }

    updateAssetsTable(assets) {
        const tbody = document.querySelector('tbody');
        if (!tbody) return;

        tbody.innerHTML = assets.map(asset => this.createAssetRow(asset)).join('');
    }

    createAssetRow(asset) {
        return `
            <tr class="animate-fade-in">
                <td>
                    <div class="d-flex align-items-center">
                        <input type="checkbox" class="form-check-input me-3" value="${asset.id}">
                        <div>
                            <div class="fw-bold">${asset.assetTag}</div>
                            <div class="text-muted small">${asset.assetDescription}</div>
                        </div>
                    </div>
                </td>
                <td>${asset.brand}</td>
                <td>${asset.model}</td>
                <td>
                    <span class="status-badge status-${asset.status.toLowerCase()}">
                        <i class="fas fa-circle"></i>
                        ${asset.status}
                    </span>
                </td>
                <td>${asset.location}</td>
                <td>
                    <div class="btn-group">
                        <button class="btn btn-light" data-bs-toggle="tooltip" title="View Details">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-light" data-bs-toggle="tooltip" title="Edit Asset">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-light" data-bs-toggle="tooltip" title="Delete Asset">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }

    updateCharts(stats) {
        // Update Total Assets Chart
        if (this.totalAssetsChart) {
            this.totalAssetsChart.data.labels = stats.timeline.labels;
            this.totalAssetsChart.data.datasets[0].data = stats.timeline.data;
            this.totalAssetsChart.update();
        }

        // Update Status Chart
        if (this.statusChart) {
            this.statusChart.data.datasets[0].data = [
                stats.active,
                stats.inactive,
                stats.maintenance
            ];
            this.statusChart.update();
        }

        // Update Utilization Chart
        if (this.utilizationChart) {
            this.utilizationChart.data.datasets[0].data = [
                stats.inUse,
                stats.available,
                stats.reserved
            ];
            this.utilizationChart.update();
        }
    }

    animateNumber(element, start, end, duration) {
        const range = end - start;
        const increment = range / (duration / 16);
        let current = start;

        const animate = () => {
            current += increment;
            element.textContent = Math.round(current).toLocaleString();

            if ((increment > 0 && current < end) || (increment < 0 && current > end)) {
                requestAnimationFrame(animate);
            } else {
                element.textContent = end.toLocaleString();
            }
        };

        requestAnimationFrame(animate);
    }

    updateTrends(trends) {
        Object.entries(trends).forEach(([key, value]) => {
            const element = document.querySelector(`[data-trend="${key}"]`);
            if (element) {
                const trend = value > 0 ? 'up' : value < 0 ? 'down' : 'neutral';
                element.className = `trend-indicator trend-${trend}`;
                element.innerHTML = `
                    <i class="fas fa-arrow-${trend === 'neutral' ? 'right' : trend}"></i>
                    ${Math.abs(value)}%
                `;
            }
        });
    }

    showLoadingState() {
        // Show loading placeholders for stats
        document.querySelectorAll('.stats-number').forEach(el => {
            el.innerHTML = '<div class="loading-placeholder"></div>';
        });

        // Show loading placeholders for table
        const tbody = document.querySelector('tbody');
        if (tbody) {
            tbody.innerHTML = Array(5).fill(`
                <tr>
                    <td><div class="loading-placeholder"></div></td>
                    <td><div class="loading-placeholder"></div></td>
                    <td><div class="loading-placeholder"></div></td>
                    <td><div class="loading-placeholder"></div></td>
                    <td><div class="loading-placeholder"></div></td>
                    <td><div class="loading-placeholder"></div></td>
                </tr>
            `).join('');
        }

        // Disable refresh button
        const refreshBtn = document.getElementById('refreshStats');
        if (refreshBtn) {
            refreshBtn.disabled = true;
            refreshBtn.classList.add('fa-spin');
        }
    }

    hideLoadingState() {
        // Remove loading placeholders
        document.querySelectorAll('.loading-placeholder').forEach(el => {
            el.remove();
        });

        // Enable refresh button
        const refreshBtn = document.getElementById('refreshStats');
        if (refreshBtn) {
            refreshBtn.disabled = false;
            refreshBtn.classList.remove('fa-spin');
        }
    }

    handleRefresh() {
        const refreshBtn = document.getElementById('refreshStats');
        if (refreshBtn) {
            refreshBtn.classList.add('fa-spin');
            this.loadInitialData().finally(() => {
                refreshBtn.classList.remove('fa-spin');
            });
        }
    }

    handleSearch(query) {
        const rows = document.querySelectorAll('tbody tr');
        const searchTerm = query.toLowerCase();

        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(searchTerm) ? '' : 'none';
        });
    }

    handleQuickAction(event) {
        const action = event.currentTarget.dataset.action;
        switch (action) {
            case 'add':
                window.location.href = '/Asset/Create';
                break;
            case 'import':
                document.getElementById('importFile').click();
                break;
            case 'export':
                this.showExportMenu();
                break;
            default:
                console.log('Unknown action:', action);
        }
    }

    handleBulkSelection() {
        const selectedAssets = document.querySelectorAll('input[name="selectedAssets"]:checked');
        const bulkActions = document.getElementById('bulkActions');
        
        if (bulkActions) {
            bulkActions.style.display = selectedAssets.length > 0 ? 'flex' : 'none';
        }
    }

    async handleExport(event) {
        event.preventDefault();
        const format = event.currentTarget.dataset.format;
        const selectedAssets = Array.from(document.querySelectorAll('input[name="selectedAssets"]:checked'))
            .map(checkbox => checkbox.value);

        if (selectedAssets.length === 0) {
            this.showNotification('Please select assets to export', 'warning');
            return;
        }

        try {
            this.showNotification('Exporting assets...', 'info');
            
            const response = await fetch('/Asset/Export', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    format,
                    assetIds: selectedAssets
                })
            });

            if (!response.ok) throw new Error('Export failed');

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `assets-export.${format}`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            a.remove();

            this.showNotification(`Assets exported successfully in ${format} format`, 'success');
        } catch (error) {
            console.error('Export error:', error);
            this.showNotification('Error exporting assets', 'error');
        }
    }

    showExportMenu() {
        const menu = document.getElementById('exportMenu');
        if (menu) {
            menu.classList.toggle('show');
        }
    }

    showNotification(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');
        
        toast.innerHTML = `
            <div class="toast-header">
                <strong class="me-auto">${this.getNotificationTitle(type)}</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        `;
        
        const container = document.querySelector('.toast-container');
        container.appendChild(toast);
        
        const bsToast = new bootstrap.Toast(toast, {
            animation: true,
            autohide: true,
            delay: 3000
        });
        
        bsToast.show();
        
        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    getNotificationTitle(type) {
        switch (type) {
            case 'success': return 'Success';
            case 'error': return 'Error';
            case 'warning': return 'Warning';
            default: return 'Information';
        }
    }
}

// Utility Functions
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Initialize the system when the DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.assetManagementSystem = new AssetManagementSystem();
}); 