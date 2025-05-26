$(document).ready(function () {
    // Initialize variables
    let selectedRows = new Set();
    let table;
    let isInitialLoad = true;

    // Show loading overlay
    function showLoading() {
        $('#tableLoader').removeClass('d-none');
    }

    // Hide loading overlay
    function hideLoading() {
        $('#tableLoader').addClass('d-none');
    }

    // Initialize DataTable with enhanced features
    table = $('#assetTransfersTable').DataTable({
        processing: true,
        serverSide: true,
        language: {
            processing: "<div class='spinner-border text-primary'></div>",
            search: "بحث:",
            lengthMenu: "عرض _MENU_ سجلات",
            info: "عرض _START_ الى _END_ من _TOTAL_ سجل",
            infoEmpty: "لا توجد سجلات متاحة",
            infoFiltered: "(تمت تصفيتها من _MAX_ سجل)",
            emptyTable: "لا توجد بيانات متاحة في الجدول",
            zeroRecords: "لم يتم العثور على سجلات مطابقة",
            paginate: {
                first: "الأول",
                previous: "السابق",
                next: "التالي",
                last: "الأخير"
            }
        },
        ajax: {
            url: '/AssetTransfer/GetAssetTransfers',
            type: 'POST',
            data: function(d) {
                // Add custom filters
                d.startDate = $('#dateFromFilter').val();
                d.endDate = $('#dateToFilter').val();
                d.department = $('#departmentFilter').val();
                d.transferType = $('#transferTypeFilter').val();
                return d;
            },
            beforeSend: function() {
                showLoading();
            },
            complete: function() {
                hideLoading();
                if (isInitialLoad) {
                    animateStatistics();
                    isInitialLoad = false;
                }
            }
        },
        columns: [
            { 
                data: 'id',
                render: function(data) {
                    return `<span class="badge bg-primary">#${data}</span>`;
                }
            },
            { 
                data: 'transferType',
                render: function(data) {
                    const badgeClass = data === 'داخلي' ? 'bg-info' : 'bg-warning';
                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },
            { 
                data: 'transferDate',
                render: function(data) {
                    return moment(data).format('YYYY/MM/DD');
                }
            },
            { data: 'fromDepartment' },
            { data: 'toDepartment' },
            { 
                data: 'assetTag',
                render: function(data) {
                    return `<span class="text-primary fw-bold">${data}</span>`;
                }
            },
            { data: 'assetDescription' },
            {
                data: 'id',
                render: function (data, type, row) {
                    return `
                        <div class="d-flex justify-content-center gap-2">
                            <a href="/AssetTransfer/GenerateTransferPdf/${data}" 
                               class="btn btn-outline-danger btn-sm btn-icon-circle" 
                               title="تنزيل PDF"
                               data-bs-toggle="tooltip"
                               data-bs-placement="top">
                                <i class="bi bi-file-earmark-pdf"></i>
                            </a>
                            <a href="/AssetTransfer/Edit/${data}" 
                               class="btn btn-outline-warning btn-sm btn-icon-circle" 
                               title="تعديل"
                               data-bs-toggle="tooltip"
                               data-bs-placement="top">
                                <i class="bi bi-pencil"></i>
                            </a>
                            <a href="/AssetTransfer/Delete/${data}" 
                               class="btn btn-outline-danger btn-sm btn-icon-circle" 
                               title="حذف"
                               data-bs-toggle="tooltip"
                               data-bs-placement="top">
                                <i class="bi bi-trash"></i>
                            </a>
                        </div>`;
                }
            }
        ],
        order: [[2, 'desc']], // Sort by transfer date descending by default
        drawCallback: function() {
            updateStatistics();
            initializeTooltips();
        },
        initComplete: function() {
            // Add custom classes to DataTables elements
            $('.dataTables_length select').addClass('form-select');
            $('.dataTables_filter input').addClass('form-control');
            $('.dataTables_info').addClass('text-muted');
            $('.dataTables_paginate .paginate_button').addClass('btn btn-sm btn-outline-primary');
        }
    });

    // Initialize tooltips
    function initializeTooltips() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Animate statistics numbers
    function animateStatistics() {
        $('.stat-value').each(function() {
            const $this = $(this);
            const countTo = parseInt($this.text()) || 0;
            
            $({ countNum: 0 }).animate({
                countNum: countTo
            }, {
                duration: 1500,
                easing: 'swing',
                step: function() {
                    $this.text(Math.floor(this.countNum));
                },
                complete: function() {
                    $this.text(this.countNum);
                }
            });
        });
    }

    // Update statistics
    function updateStatistics() {
        const today = new Date().toISOString().split('T')[0];
        const now = new Date();
        const startOfWeek = new Date(now.setDate(now.getDate() - now.getDay())).toISOString().split('T')[0];
        const endOfWeek = new Date(now.setDate(now.getDate() - now.getDay() + 6)).toISOString().split('T')[0];
        const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0];
        const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().split('T')[0];

        let totalCount = 0;
        let monthlyCount = 0;
        let pendingCount = 0;
        let completedCount = 0;

        table.rows().data().each(function(data) {
            totalCount++;
            const transferDate = new Date(data.transferDate).toISOString().split('T')[0];
            
            if (transferDate >= startOfMonth && transferDate <= endOfMonth) {
                monthlyCount++;
            }
            
            if (data.status === 'pending') {
                pendingCount++;
            } else if (data.status === 'completed') {
                completedCount++;
            }
        });

        $('#totalTransfers').text(totalCount);
        $('#monthlyTransfers').text(monthlyCount);
        $('#pendingTransfers').text(pendingCount);
        $('#completedTransfers').text(completedCount);
    }

    // Handle filter buttons with animation
    $('#applyFilters').on('click', function() {
        const $button = $(this);
        $button.prop('disabled', true);
        $button.html('<span class="spinner-border spinner-border-sm me-2"></span>جاري التصفية...');
        
        table.ajax.reload(function() {
            $button.prop('disabled', false);
            $button.html('<i class="bi bi-funnel me-2"></i>تطبيق المرشحات');
        });
    });

    $('#clearFilters').on('click', function() {
        const $button = $(this);
        $button.prop('disabled', true);
        
        // Animate clearing filters
        $('.form-select, .form-control').each(function() {
            $(this).val('').trigger('change');
        });
        
        table.ajax.reload(function() {
            $button.prop('disabled', false);
        });
    });

    // Handle export buttons with loading states
    $('#exportExcel').on('click', function() {
        const $button = $(this);
        $button.prop('disabled', true);
        $button.html('<span class="spinner-border spinner-border-sm me-2"></span>جاري التصدير...');
        
        window.location.href = '/AssetTransfer/ExportToExcel';
        
        setTimeout(function() {
            $button.prop('disabled', false);
            $button.html('<i class="bi bi-file-earmark-excel me-2"></i>تصدير Excel');
        }, 2000);
    });

    $('#exportPdf').on('click', function() {
        if (confirm('هل تريد تصدير جميع عمليات النقل إلى PDF؟')) {
            const $button = $(this);
            $button.prop('disabled', true);
            $button.html('<span class="spinner-border spinner-border-sm me-2"></span>جاري التصدير...');
            
            let startDate = $('#dateFromFilter').val();
            let endDate = $('#dateToFilter').val();

            if (startDate && endDate) {
                window.location.href = `/AssetTransfer/GenerateTransfersPdfByDateRange?startDate=${startDate}&endDate=${endDate}`;
            } else {
                window.location.href = '/AssetTransfer/GenerateAllTransfersPdf';
            }
            
            setTimeout(function() {
                $button.prop('disabled', false);
                $button.html('<i class="bi bi-file-earmark-pdf me-2"></i>تصدير PDF');
            }, 2000);
        }
    });

    // Load departments with loading state
    function loadDepartments() {
        const $dropdown = $('#departmentFilter');
        $dropdown.prop('disabled', true);
        $dropdown.html('<option>جاري التحميل...</option>');

        $.ajax({
            url: '/Asset/GetDepartments',
            type: 'GET',
            success: function(data) {
                $dropdown.empty();
                $dropdown.append('<option value="">جميع الأقسام</option>');

                $.each(data, function(i, dept) {
                    $dropdown.append($('<option></option>').val(dept.id).text(dept.name));
                });
            },
            error: function() {
                console.error('Failed to load departments');
                $dropdown.html('<option value="">خطأ في تحميل الأقسام</option>');
            },
            complete: function() {
                $dropdown.prop('disabled', false);
            }
        });
    }

    // Initialize date pickers with custom styling
    $('#dateFromFilter, #dateToFilter').on('focus', function() {
        $(this).addClass('border-primary');
    }).on('blur', function() {
        $(this).removeClass('border-primary');
    });

    // Load initial data
    loadDepartments();
}); 