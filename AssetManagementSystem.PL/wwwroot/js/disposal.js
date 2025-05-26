$(document).ready(function () {
    let disposalsTable;
    let allDisposalsData = [];

    // Initialize the DataTable
    function initializeDataTable() {
        $('#tableLoader').removeClass('d-none');

        disposalsTable = $('#disposalsTable').DataTable({
            processing: true,
            serverSide: true,
            ajax: {
                url: '/Disposal/GetDisposals',
                type: 'POST',
                data: function(d) {
                    // Add custom filter parameters
                    d.disposalType = $('#disposalTypeFilter').val();
                    d.dateFrom = $('#dateFromFilter').val();
                    d.dateTo = $('#dateToFilter').val();
                    d.valueRange = $('#valueRangeFilter').val();
                },
                dataSrc: function(json) {
                    // Store data for statistics
                    allDisposalsData = json.data;
                    updateStatistics(json.data);
                    $('#tableLoader').addClass('d-none');
                    return json.data;
                }
            },
            order: [[3, 'desc']], // Order by date
            language: {
                url: "//cdn.datatables.net/plug-ins/1.10.25/i18n/Arabic.json",
                emptyTable: "لا توجد بيانات متاحة في الجدول",
                info: "عرض _START_ إلى _END_ من أصل _TOTAL_ سجل",
                infoEmpty: "عرض 0 إلى 0 من أصل 0 سجل",
                infoFiltered: "(مرشح من _MAX_ إجمالي السجلات)",
                lengthMenu: "عرض _MENU_ سجل في الصفحة",
                loadingRecords: "جاري التحميل...",
                processing: "جاري المعالجة...",
                search: "البحث:",
                zeroRecords: "لم يتم العثور على نتائج مطابقة",
                paginate: {
                    first: "الأول",
                    last: "الأخير",
                    next: "التالي",
                    previous: "السابق"
                }
            },
            responsive: true,
            pageLength: 25,
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "الكل"]],
            columns: [
                {
                    data: 'id',
                    className: "text-center",
                    width: "8%"
                },
                {
                    data: 'assetTag',
                    className: "text-center",
                    width: "12%",
                    render: function(data, type, row) {
                        return `<span class="badge bg-light text-dark">${data}</span>`;
                    }
                },
                {
                    data: 'disposalType',
                    className: "text-center",
                    width: "15%",
                    render: function(data, type, row) {
                        let badgeClass = 'bg-secondary';
                        switch(data.toLowerCase()) {
                            case 'بيع': badgeClass = 'bg-success'; break;
                            case 'إتلاف': badgeClass = 'bg-danger'; break;
                            case 'تبرع': badgeClass = 'bg-info'; break;
                            default: badgeClass = 'bg-secondary';
                        }
                        return `<span class="badge ${badgeClass}">${data}</span>`;
                    }
                },
                {
                    data: 'disposalDate',
                    className: "text-center",
                    width: "12%",
                    render: function(data, type, row) {
                        if (type === 'display') {
                            const date = new Date(data);
                            return `<span class="text-muted">${date.toLocaleDateString('ar-SA')}</span>`;
                        }
                        return data;
                    }
                },
                {
                    data: 'saleValue',
                    className: "text-center",
                    width: "12%",
                    render: function(data, type, row) {
                        if (type === 'display') {
                            const value = parseFloat(data) || 0;
                            let badgeClass = 'badge-low-value';
                            if (value > 10000) badgeClass = 'badge-high-value';
                            else if (value > 1000) badgeClass = 'badge-medium-value';

                            return `<span class="badge-status ${badgeClass}">${value.toLocaleString('ar-SA')} ر.س</span>`;
                        }
                        return data;
                    }
                },
                {
                    data: 'assetDescription',
                    className: "text-center",
                    width: "25%",
                    render: function(data, type, row) {
                        if (data && data.length > 50) {
                            return `<span title="${data}">${data.substring(0, 50)}...</span>`;
                        }
                        return data || '<span class="text-muted">غير محدد</span>';
                    }
                },
                {
                    data: 'id',
                    className: "text-center",
                    width: "16%",
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                            <div class="action-buttons">
                                <a href="/Disposal/Edit/${data}"
                                   class="btn-action btn-edit"
                                   title="تعديل">
                                    <i class="bi bi-pencil-square"></i>
                                    <span class="d-none d-md-inline">تعديل</span>
                                </a>
                                <a href="/Disposal/Delete/${data}"
                                   class="btn-action btn-delete"
                                   title="حذف">
                                    <i class="bi bi-trash"></i>
                                    <span class="d-none d-md-inline">حذف</span>
                                </a>
                            </div>
                        `;
                    }
                }
            ],
            drawCallback: function(settings) {
                // Add hover effects and animations
                $('tbody tr').hover(
                    function() {
                        $(this).addClass('table-hover-effect');
                    },
                    function() {
                        $(this).removeClass('table-hover-effect');
                    }
                );
            }
        });
    }

    // Initialize statistics with loading state
    function initializeStatistics() {
        $('#totalDisposals').html('<i class="fas fa-spinner fa-spin"></i>');
        $('#monthlyDisposals').html('<i class="fas fa-spinner fa-spin"></i>');
        $('#totalValue').html('<i class="fas fa-spinner fa-spin"></i>');
        $('#recentDisposals').html('<i class="fas fa-spinner fa-spin"></i>');
    }

    // Update statistics based on current data
    function updateStatistics(data) {
        const currentDate = new Date();
        const currentMonth = currentDate.getMonth();
        const currentYear = currentDate.getFullYear();
        const lastWeek = new Date(currentDate.getTime() - 7 * 24 * 60 * 60 * 1000);

        let totalDisposals = data.length;
        let monthlyDisposals = 0;
        let totalValue = 0;
        let recentDisposals = 0;

        data.forEach(disposal => {
            const disposalDate = new Date(disposal.disposalDate);
            const saleValue = parseFloat(disposal.saleValue) || 0;

            // Monthly count
            if (disposalDate.getMonth() === currentMonth && disposalDate.getFullYear() === currentYear) {
                monthlyDisposals++;
            }

            // Recent count (last week)
            if (disposalDate >= lastWeek) {
                recentDisposals++;
            }

            // Total value
            totalValue += saleValue;
        });

        // Animate counter updates
        animateCounter('#totalDisposals', totalDisposals);
        animateCounter('#monthlyDisposals', monthlyDisposals);
        animateValue('#totalValue', totalValue, 'ر.س');
        animateCounter('#recentDisposals', recentDisposals);
    }

    // Animate counter with counting effect
    function animateCounter(selector, finalValue) {
        const element = $(selector);
        let currentValue = 0;
        const increment = Math.ceil(finalValue / 20);
        const timer = setInterval(() => {
            currentValue += increment;
            if (currentValue >= finalValue) {
                currentValue = finalValue;
                clearInterval(timer);
            }
            element.text(currentValue.toLocaleString('ar-SA'));
        }, 50);
    }

    // Animate value with formatting
    function animateValue(selector, finalValue, suffix = '') {
        const element = $(selector);
        let currentValue = 0;
        const increment = Math.ceil(finalValue / 20);
        const timer = setInterval(() => {
            currentValue += increment;
            if (currentValue >= finalValue) {
                currentValue = finalValue;
                clearInterval(timer);
            }
            element.text(currentValue.toLocaleString('ar-SA') + ' ' + suffix);
        }, 50);
    }

    // Filter functionality
    $('#applyFilters').on('click', function() {
        $('#tableLoader').removeClass('d-none');
        disposalsTable.ajax.reload(function() {
            $('#tableLoader').addClass('d-none');
        });

        // Show success message
        showNotification('تم تطبيق المرشحات بنجاح', 'success');
    });

    $('#clearFilters').on('click', function() {
        // Clear all filter inputs
        $('#disposalTypeFilter').val('');
        $('#dateFromFilter').val('');
        $('#dateToFilter').val('');
        $('#valueRangeFilter').val('');

        // Reload table
        $('#tableLoader').removeClass('d-none');
        disposalsTable.ajax.reload(function() {
            $('#tableLoader').addClass('d-none');
        });

        showNotification('تم إعادة تعيين المرشحات', 'info');
    });

    // Export functionality
    $('#exportExcel').on('click', function() {
        showNotification('جاري تصدير البيانات إلى Excel...', 'info');
        // Here you can implement actual export functionality
        setTimeout(() => {
            showNotification('تم تصدير البيانات بنجاح', 'success');
        }, 2000);
    });

    $('#exportPdf').on('click', function() {
        showNotification('جاري تصدير البيانات إلى PDF...', 'info');
        // Here you can implement actual export functionality
        setTimeout(() => {
            showNotification('تم تصدير البيانات بنجاح', 'success');
        }, 2000);
    });

    // Notification system
    function showNotification(message, type = 'info') {
        const alertClass = {
            'success': 'alert-success',
            'error': 'alert-danger',
            'warning': 'alert-warning',
            'info': 'alert-info'
        }[type] || 'alert-info';

        const icon = {
            'success': 'bi-check-circle-fill',
            'error': 'bi-exclamation-triangle-fill',
            'warning': 'bi-exclamation-triangle-fill',
            'info': 'bi-info-circle-fill'
        }[type] || 'bi-info-circle-fill';

        const notification = $(`
            <div class="alert ${alertClass} alert-dismissible fade show position-fixed"
                 style="top: 20px; left: 20px; z-index: 9999; min-width: 300px;" role="alert">
                <i class="bi ${icon} me-2"></i>${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `);

        $('body').append(notification);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            notification.alert('close');
        }, 5000);
    }

    // Enhanced search functionality
    $('#disposalsTable_filter input').on('keyup', function() {
        const searchValue = this.value;
        if (searchValue.length > 2 || searchValue.length === 0) {
            disposalsTable.search(searchValue).draw();
        }
    });

    // Initialize everything
    initializeStatistics();
    initializeDataTable();

    // Keyboard shortcuts
    $(document).on('keydown', function(e) {
        // Ctrl+F for focus on search
        if (e.ctrlKey && e.keyCode === 70) {
            e.preventDefault();
            $('#disposalsTable_filter input').focus();
        }

        // Ctrl+N for new disposal
        if (e.ctrlKey && e.keyCode === 78) {
            e.preventDefault();
            window.location.href = '/Disposal/Create';
        }
    });

    // Responsive table handling
    $(window).on('resize', function() {
        if (disposalsTable) {
            disposalsTable.columns.adjust().draw();
        }
    });

    // Add loading state to action buttons
    $(document).on('click', '.btn-action', function() {
        const btn = $(this);
        const originalHtml = btn.html();
        btn.html('<i class="fas fa-spinner fa-spin"></i>');

        // Restore after navigation (this is just for visual feedback)
        setTimeout(() => {
            btn.html(originalHtml);
        }, 1000);
    });
}); 