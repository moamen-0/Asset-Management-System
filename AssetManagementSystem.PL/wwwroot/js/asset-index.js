/**
 * Professional Asset Management System - Enhanced UI JavaScript
 * Enhanced with modern features, performance optimizations, and professional animations
 */

// Enhanced application configuration
const AssetManagement = {
    config: {
        animationDuration: 300,
        refreshInterval: 30000, // Auto-refresh every 30 seconds
        debounceDelay: 500,
        maxRetries: 3,
        toastDuration: 3000
    },
    
    // Performance monitoring
    performance: {
        startTime: performance.now(),
        apiCalls: 0,
        errors: 0
    },
    
    // Cache for better performance
    cache: {
        departments: null,
        users: null,
        supervisors: null,
        lastRefresh: null
    },
    
    // Enhanced UI state management
    state: {
        isLoading: false,
        selectedAssets: new Set(),
        currentFilter: null,
        lastSearch: null
    }
};

// Enhanced utility functions
const Utils = {
    // Professional loading overlay with animations
    showLoadingOverlay(message, showProgress = false) {
        const overlay = $(`
            <div class="professional-loading-overlay">
                <div class="loading-container">
                    <div class="loading-spinner">
                        <div class="spinner-ring"></div>
                        <div class="spinner-ring"></div>
                        <div class="spinner-ring"></div>
                    </div>
                    <h4 class="loading-title">${message}</h4>
                    <p class="loading-subtitle">يرجى الانتظار...</p>
                    ${showProgress ? '<div class="progress-bar"><div class="progress-fill"></div></div>' : ''}
                </div>
            </div>
        `);
        $('body').append(overlay);
        
        // Animate in
        requestAnimationFrame(() => {
            overlay.addClass('show');
        });
        
        return overlay;
    },
    
    // Enhanced toast notifications
    showToast(title, message, type = 'success', duration = AssetManagement.config.toastDuration) {
        const toast = $(`
            <div class="professional-toast toast-${type}">
                <div class="toast-icon">
                    <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'}"></i>
                </div>
                <div class="toast-content">
                    <div class="toast-title">${title}</div>
                    <div class="toast-message">${message}</div>
                </div>
                <button class="toast-close">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `);
        
        $('.toast-container').append(toast);
        
        // Animate in
        requestAnimationFrame(() => {
            toast.addClass('show');
        });
        
        // Auto hide
        setTimeout(() => {
            toast.removeClass('show').addClass('hide');
            setTimeout(() => toast.remove(), 300);
        }, duration);
        
        return toast;
    },
    
    // Enhanced error handling
    handleApiError(xhr, operation = 'العملية') {
        AssetManagement.performance.errors++;
        
        let errorMessage = `فشل في تنفيذ ${operation}`;
        
        if (xhr.responseJSON?.error) {
            errorMessage = xhr.responseJSON.error;
        } else if (xhr.status === 0) {
            errorMessage = 'تعذر الاتصال بالخادم';
        } else if (xhr.status === 404) {
            errorMessage = 'الصفحة المطلوبة غير موجودة';
        } else if (xhr.status === 500) {
            errorMessage = 'خطأ داخلي في الخادم';
        }
        
        Swal.fire({
            title: 'خطأ!',
            text: errorMessage,
            icon: 'error',
            confirmButtonText: 'حسناً',
            customClass: {
                popup: 'professional-alert'
            }
        });
        
        console.error(`API Error [${operation}]:`, xhr);
    },
    
    // Debounced function for search
    debounce(func, delay) {
        let timeoutId;
        return function (...args) {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(() => func.apply(this, args), delay);
        };
    },
    
    // Format numbers with animations
    animateCounterTo(element, targetValue, duration = 1000) {
        const $element = $(element);
        const startValue = parseInt($element.text()) || 0;
        const increment = (targetValue - startValue) / (duration / 16);
        let currentValue = startValue;
        
        const timer = setInterval(() => {
            currentValue += increment;
            if ((increment > 0 && currentValue >= targetValue) || 
                (increment < 0 && currentValue <= targetValue)) {
                currentValue = targetValue;
                clearInterval(timer);
            }
            $element.text(Math.floor(currentValue));
        }, 16);
    }
};

// Add toast container to page
if (!$('.toast-container').length) {
    $('body').append('<div class="toast-container"></div>');
}

$(document).ready(function () {
    console.log('🚀 Asset Management System - Professional UI Initialized');
    
    // Enhanced initialization with performance tracking
    AssetManagement.performance.startTime = performance.now();
    
    // Initialize Dashboard Stats with animations
    fetchAssetStats();

    // Initialize the main assets DataTable with enhanced configuration
    let table = $('#assetsTable').DataTable({
        processing: true,
        serverSide: true,
        responsive: true,
        stateSave: true, // Save table state
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "الكل"]],
        ajax: {
            url: '/Asset/GetAssets',
            type: 'POST',
            data: function(d) {
                AssetManagement.performance.apiCalls++;
                console.log('📊 Sending DataTable request:', d);
                return d;
            },
            dataSrc: function(json) {
                console.log('📥 Received DataTable response:', json);
                if (!json || !json.data) {
                    console.error('❌ Invalid response format:', json);
                    Utils.showToast('خطأ', 'تنسيق البيانات غير صحيح', 'error');
                    return [];
                }
                
                // Add smooth fade-in animation for new data
                setTimeout(() => {
                    $('#assetsTable tbody tr').addClass('fadeInUp');
                }, 100);
                
                return json.data;
            },
            error: function (xhr, error, thrown) {
                console.error('❌ DataTable error:', error, thrown);
                Utils.handleApiError(xhr, 'تحميل البيانات');
            }
        },
        scrollX: true,
        order: [[2, 'asc']], // Sort by Asset Tag by default
        language: {
            url: "//cdn.datatables.net/plug-ins/1.10.25/i18n/Arabic.json",
            search: "البحث:",
            lengthMenu: "عرض _MENU_ عنصر",
            info: "عرض _START_ إلى _END_ من أصل _TOTAL_ عنصر",
            infoEmpty: "لا توجد عناصر للعرض",
            infoFiltered: "(مفلتر من _MAX_ عنصر إجمالي)",
            paginate: {
                first: "الأول",
                last: "الأخير", 
                next: "التالي",
                previous: "السابق"
            },
            processing: "جاري المعالجة..."
        },
        dom: '<"table-header"<"row"<"col-md-6"l><"col-md-6"f>>>rt<"table-footer"<"row"<"col-md-6"i><"col-md-6"p>>>',
        columns: [
            {
                data: null,
                orderable: false,
                className: "text-center selection-column",
                width: "50px",
                render: function () {
                    return `<input type="checkbox" class="row-checkbox form-check-input professional-checkbox">`;
                }
            },
            {
                data: null,
                orderable: false,
                className: "text-center actions-column",
                width: "120px",
                render: function (data, type, row) {
                    return `
                        <div class="btn-group professional-actions">
                            <button type="button" class="btn btn-sm btn-primary dropdown-toggle action-btn" 
                                    data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-cog"></i>
                            </button>
                            <ul class="dropdown-menu professional-dropdown">
                                <li><a href="/Asset/Edit/${row.assetTag}" class="dropdown-item">
                                    <i class="fas fa-edit text-warning me-2"></i> تعديل
                                </a></li>
                                <li><a href="/Asset/Delete/${row.assetTag}" class="dropdown-item">
                                    <i class="fas fa-trash-alt text-danger me-2"></i> حذف
                                </a></li>
                                <li><hr class="dropdown-divider"></li>
                                <li><a href="#" class="dropdown-item dispose-btn" data-asset-tag="${row.assetTag}">
                                    <i class="fas fa-info-circle text-info me-2"></i> تكهين
                                </a></li>
                            </ul>
                        </div>`;
                }
            },
            {
                data: 'assetTag',
                className: "asset-tag-column",
                render: function (data) {
                    return `<span class="professional-badge asset-tag-badge" data-bs-toggle="tooltip" 
                                  title="Asset Tag: ${data}">${data}</span>`;
                }
            },
            { 
                data: 'cluster', 
                className: "cluster-column",
                render: function(data) {
                    return data ? `<span class="cluster-text">${data}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            { 
                data: 'facility.name',
                className: "facility-column",
                render: function(data) {
                    return data ? `<span class="facility-text">${data}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            { 
                data: 'building.name',
                className: "building-column",
                render: function(data) {
                    return data ? `<span class="building-text">${data}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            { 
                data: 'floor.name',
                className: "floor-column",
                render: function(data) {
                    return data ? `<span class="floor-text">${data}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            { 
                data: 'room.name',
                className: "room-column",
                render: function(data) {
                    return data ? `<span class="room-text">${data}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            { 
                data: 'department.name',
                className: "department-column",
                render: function(data) {
                    return data ? `<span class="department-text">${data}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            { 
                data: 'assetDescription',
                className: "description-column",
                render: function(data) {
                    const truncated = data && data.length > 50 ? data.substring(0, 50) + '...' : data;
                    return data ? `<span class="description-text" data-bs-toggle="tooltip" 
                                        title="${data}">${truncated}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            { 
                data: 'brand',
                className: "brand-column",
                render: function(data) {
                    return data ? `<span class="brand-text">${data}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            { 
                data: 'model',
                className: "model-column",
                render: function(data) {
                    return data ? `<span class="model-text">${data}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            { 
                data: 'assetType',
                className: "type-column",
                render: function(data) {
                    return data ? `<span class="type-text">${data}</span>` : '<span class="text-muted">غير محدد</span>';
                }
            },
            {
                data: 'status',
                className: "status-column",
                render: function (data) {
                    const statusConfig = {
                        'Available': { class: 'status-available', icon: 'check-circle', text: 'متاح' },
                        'In Use': { class: 'status-inuse', icon: 'user', text: 'قيد الاستخدام' },
                        'Under Maintenance': { class: 'status-maintenance', icon: 'tools', text: 'تحت الصيانة' },
                        'Disposed': { class: 'status-disposed', icon: 'trash', text: 'مكهن' }
                    };
                    
                    const config = statusConfig[data] || { class: 'status-unknown', icon: 'question', text: data };
                    
                    return `<span class="professional-status ${config.class}">
                              <i class="fas fa-${config.icon} me-1"></i>${config.text}
                            </span>`;
                }
            }
        ],
        initComplete: function () {
            console.log('✅ DataTable initialization completed successfully');
            
            // Add professional styling to DataTable elements
            $('#assetsTable_filter input')
                .addClass('form-control professional-search')
                .attr('placeholder', 'البحث في الأصول...');
            
            $('#assetsTable_length select')
                .addClass('form-select professional-select');
            
            // Initialize tooltips
            $('[data-bs-toggle="tooltip"]').tooltip();
            
            // Add loading complete animation
            $('#assetsTable').addClass('table-loaded');
            
            Utils.showToast('نجح', 'تم تحميل جدول الأصول بنجاح', 'success', 2000);
        },
        
        // Enhanced drawing callback for animations
        drawCallback: function() {
            // Re-initialize tooltips for new rows
            $('[data-bs-toggle="tooltip"]').tooltip();
            
            // Add stagger animation to rows
            $('#assetsTable tbody tr').each(function(index) {
                $(this).css('animation-delay', (index * 50) + 'ms').addClass('fadeInUp');
            });
        }
    });

    // Initialize the filtered assets table with enhanced configuration
    let filteredTable = $('#filteredAssetsTable').DataTable({
        responsive: true,
        pageLength: 50,
        stateSave: true,
        dom: '<"filtered-table-header"<"row"<"col-12 text-center export-container my-3"B>><"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>>rt<"filtered-table-footer"<"row"<"col-sm-12 col-md-6"i><"col-sm-12 col-md-6"p>>>',
        language: {
            url: "//cdn.datatables.net/plug-ins/1.10.25/i18n/Arabic.json",
            search: "البحث في النتائج:",
            lengthMenu: "عرض _MENU_ نتيجة",
            info: "عرض _START_ إلى _END_ من أصل _TOTAL_ نتيجة",
            infoEmpty: "لا توجد نتائج للعرض",
            paginate: {
                first: "الأول",
                last: "الأخير",
                next: "التالي", 
                previous: "السابق"
            }
        },
        buttons: [
            {
                extend: 'collection',
                text: '<i class="fas fa-download me-2"></i>تصدير النتائج',
                className: 'btn btn-primary professional-export-btn',
                buttons: [
                    {
                        extend: 'copy',
                        text: '<i class="fas fa-copy me-2"></i>نسخ للحافظة',
                        className: 'btn btn-sm export-option'
                    },
                    {
                        extend: 'excel',
                        text: '<i class="fas fa-file-excel me-2"></i>تصدير Excel',
                        className: 'btn btn-sm export-option',
                        filename: function() {
                            return 'تصفية_الأصول_' + new Date().toISOString().slice(0, 10);
                        }
                    },
                    {
                        extend: 'pdf',
                        text: '<i class="fas fa-file-pdf me-2"></i>تصدير PDF',
                        className: 'btn btn-sm export-option',
                        filename: function() {
                            return 'تصفية_الأصول_' + new Date().toISOString().slice(0, 10);
                        },
                        orientation: 'landscape',
                        pageSize: 'A4'
                    },
                    {
                        extend: 'print',
                        text: '<i class="fas fa-print me-2"></i>طباعة',
                        className: 'btn btn-sm export-option'
                    }
                ]
            }
        ],
        columns: [
            { data: 'assetTag' },
            { data: 'assetDescription' },
            { data: 'department.name' },
            { data: 'brand' },
            { data: 'model' },
            { data: 'status' },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return `<div class="btn-group">
                        <a href="/Asset/Edit/${row.assetTag}" class="btn btn-sm btn-primary" title="تعديل">
                            <i class="fas fa-edit"></i>
                        </a>
                        <a href="/Asset/Details/${row.assetTag}" class="btn btn-sm btn-info" title="تفاصيل">
                            <i class="fas fa-info-circle"></i>
                        </a>
                        <a href="#" class="btn btn-sm btn-danger dispose-btn" title="تكهين" data-asset-tag="${row.assetTag}">
                            <i class="fas fa-trash-alt"></i>
                        </a>
                    </div>`;
                }
            }
        ]
    });

    // Fetch asset statistics
    // Fetch asset statistics
    function fetchAssetStats() {
        $.ajax({
            url: '/Asset/GetStats',
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    // Update statistics cards
                    $('#totalAssets').text(response.totalCount);
                    $('#activeAssets').text(response.activeCount);
                    $('#maintenanceAssets').text(response.maintenanceCount);
                    $('#disposedAssets').text(response.disposedCount);
                    
                    // Update header total count
                    updateTotalAssetsCount(response.totalCount);

                    console.log('Asset stats loaded successfully:', response);
                } else {
                    console.error('Failed to load asset stats:', response.error);
                    // Set default values if stats loading fails
                    $('#totalAssets, #activeAssets, #maintenanceAssets, #disposedAssets').text('--');
                    $('#totalAssetsCount').text('--');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error fetching asset stats:', error);
                // Set default values if stats loading fails
                $('#totalAssets, #activeAssets, #maintenanceAssets, #disposedAssets').text('--');
                $('#totalAssetsCount').text('--');
            }
        });
    }

    // Update tag counter as user types
    $('#assetTagsInput').on('input', function () {
        const tags = processAssetTags($(this).val());
        const badge = $(this).closest('.form-group').find('.badge');
        badge.text(tags.length);

        if (tags.length > 0) {
            badge.removeClass('bg-secondary').addClass('bg-primary');
        } else {
            badge.removeClass('bg-primary').addClass('bg-secondary');
        }
    });

    // Clear tags button
    $('#clearTagsBtn').click(function () {
        $('#assetTagsInput').val('').trigger('input');
    });

    // Handle clipboard paste for Excel data
    $('#assetTagsInput, #bulkAssetTags').on('paste', function (e) {
        setTimeout(() => {
            let input = $(this).val();
            if (input.includes('\t')) {
                input = input.replace(/\t/g, '\n');
                $(this).val(input);
                $(this).trigger('input');
            }
        }, 0);
    });

    // Function to process and clean asset tags
    function processAssetTags(input) {
        return input.split(/[\n,\t]+/)
            .map(tag => tag.trim())
            .filter(tag => tag.length > 0)
            .filter((tag, index, self) => self.indexOf(tag) === index); // Remove duplicates
    }

    // Handle the filter button click
    $('#filterAssets').click(function () {
        const tagsInput = $('#assetTagsInput').val();
        const tags = processAssetTags(tagsInput);

        if (tags.length === 0) {
            Swal.fire({
                title: 'خطأ!',
                text: 'الرجاء إدخال Asset Tag واحد على الأقل',
                icon: 'error',
                confirmButtonText: 'حسناً'
            });
            return;
        }

        // Show loading indicator
        const loadingOverlay = showLoadingOverlay("جاري تصفية الأصول...");

        // Call the API to get filtered assets
        $.ajax({
            url: '/Asset/GetFilteredAssets',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(tags),
            success: function (response) {
                loadingOverlay.remove();

                if (response.success) {
                    // Show the filter results section
                    $('#filterResultsSection').fadeIn(300);

                    // Update filter stats
                    $('#filterCount').text(response.data.length);

                    // Clear and reload the filtered table
                    filteredTable.clear().draw();
                    filteredTable.rows.add(response.data.map(asset => [
                        asset.assetTag,
                        asset.assetDescription || 'غير محدد',
                        asset.department?.name || 'غير محدد',
                        asset.brand || 'غير محدد',
                        asset.model || 'غير محدد',
                        asset.status || 'غير محدد',
                        `<div class="btn-group">
                                    <a href="/Asset/Edit/${asset.assetTag}" class="btn btn-sm btn-primary" title="تعديل">
                                        <i class="fas fa-edit"></i>
                                    </a>
                                    <a href="/Asset/Details/${asset.assetTag}" class="btn btn-sm btn-info" title="تفاصيل">
                                        <i class="fas fa-info-circle"></i>
                                    </a>
                                    <a href="#" class="btn btn-sm btn-danger dispose-btn" title="تكهين" data-asset-tag="${asset.assetTag}">
                                        <i class="fas fa-trash-alt"></i>
                                    </a>
                                </div>`
                    ])).draw();

                    // Display matched and missing tags
                    $('#matchedTags').empty();
                    $('#missingTags').empty();

                    // Get the list of found asset tags
                    const foundTags = response.data.map(a => a.assetTag);

                    // Display matched tags
                    foundTags.forEach(tag => {
                        $('#matchedTags').append(`
                                    <span class="badge bg-primary p-2 me-1">${tag}</span>
                                `);
                    });

                    // Check for missing tags
                    const missingTags = tags.filter(tag => !foundTags.includes(tag));
                    if (missingTags.length > 0) {
                        $('#missingTagsAlert').show();
                        $('#missingTags').empty();
                        missingTags.forEach(tag => {
                            $('#missingTags').append(`
                                        <span class="badge bg-warning text-dark p-2 me-1">${tag}</span>
                                    `);
                        });
                    } else {
                        $('#missingTagsAlert').hide();
                    }

                    // Scroll to results
                    $('html, body').animate({
                        scrollTop: $("#filterResultsSection").offset().top - 20
                    }, 500);
                } else {
                    Swal.fire({
                        title: 'خطأ!',
                        text: response.error || 'حدث خطأ أثناء تصفية الأصول',
                        icon: 'error',
                        confirmButtonText: 'حسناً'
                    });
                }
            },
            error: function (xhr) {
                loadingOverlay.remove();
                Swal.fire({
                    title: 'خطأ!',
                    text: 'حدث خطأ أثناء تصفية الأصول: ' + (xhr.responseJSON?.error || 'خطأ غير معروف'),
                    icon: 'error',
                    confirmButtonText: 'حسناً'
                });
            }
        });
    });

    // Show loading overlay
    function showLoadingOverlay(message) {
        const overlay = $(`
                    <div class="downloading-overlay">
                        <div class="downloading-content">
                            <div class="spinner-border text-primary mb-3" role="status"></div>
                            <h5>${message}</h5>
                            <p class="text-muted mb-0">يرجى الانتظار...</p>
                        </div>
                    </div>
                `);
        $('body').append(overlay);
        return overlay;
    }

    // Close filter results
    $('#closeFilterResults').click(function () {
        $('#filterResultsSection').fadeOut(300);
    });

    // Handle checkbox selection and bulk operations
    let selectedAssets = new Set();

    $(document).on('change', '.row-checkbox', function () {
        const row = $(this).closest('tr');
        const assetTag = table.row(row).data().assetTag;

        if (this.checked) {
            selectedAssets.add(assetTag);
        } else {
            selectedAssets.delete(assetTag);
        }

        updateBulkControls();
    });

    // "Select All" checkbox
    $('#selectAll').on('change', function () {
        $('.row-checkbox').prop('checked', this.checked);

        if (this.checked) {
            table.rows().data().each(function (row) {
                selectedAssets.add(row.assetTag);
            });
        } else {
            selectedAssets.clear();
        }

        updateBulkControls();
    });

    // Process bulk asset tags from textarea
    $('#processAssetTags').click(function () {
        const tagsInput = $('#bulkAssetTags').val();
        const tags = processAssetTags(tagsInput);

        if (tags.length === 0) {
            Swal.fire({
                title: 'تنبيه!',
                text: 'الرجاء إدخال Asset Tag واحد على الأقل',
                icon: 'warning',
                confirmButtonText: 'حسناً'
            });
            return;
        }

        // Show loading indicator
        Swal.fire({
            title: 'جاري التحقق...',
            text: 'يتم البحث عن الأصول في قاعدة البيانات',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        // Make AJAX call to verify tags against the full database
        $.ajax({
            url: '/Asset/VerifyAssetTags',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(tags),
            success: function (response) {
                Swal.close();

                if (response.success) {
                    // Clear existing selections
                    $('.row-checkbox').prop('checked', false);
                    selectedAssets.clear();

                    // Add the found assets to our selection
                    if (response.foundTags && response.foundTags.length > 0) {
                        response.foundTags.forEach(tag => {
                            selectedAssets.add(tag);
                        });

                        // Select checkboxes for assets that are currently visible in the table
                        table.rows().every(function () {
                            const rowData = this.data();
                            if (selectedAssets.has(rowData.assetTag)) {
                                $(this.node()).find('.row-checkbox').prop('checked', true);
                            }
                        });
                    }

                    // Format the result message
                    const found = response.foundTags.length;
                    const notFoundTags = response.notFoundTags || [];
                    const notFound = notFoundTags.length;

                    let messageHtml = '';
                    if (found > 0) {
                        messageHtml += `<p>✅ تم العثور وتحديد ${found} أصل</p>`;
                    }

                    if (notFound > 0) {
                        messageHtml += `<p>⚠️ ${notFound} أصل غير موجود في قاعدة البيانات:</p>`;
                        messageHtml += '<div class="not-found-tags-container" style="max-height: 150px; overflow-y: auto; margin-top: 10px; margin-bottom: 10px; text-align: left;">';
                        messageHtml += '<ul style="padding-left: 20px;">';

                        notFoundTags.forEach(tag => {
                            messageHtml += `<li style="margin-bottom: 5px;"><code>${tag}</code></li>`;
                        });

                        messageHtml += '</ul></div>';
                    }

                    // Show results to user
                    Swal.fire({
                        icon: found > 0 ? (notFound > 0 ? 'info' : 'success') : 'error',
                        title: found > 0 ? 'تم تحديد الأصول' : 'لم يتم العثور على أي أصول',
                        html: messageHtml,
                        confirmButtonText: 'حسناً',
                        width: notFound > 0 ? '500px' : '400px'
                    });

                    // Update bulk operation controls
                    updateBulkControls();

                    // Clear the textarea
                    $('#bulkAssetTags').val('');
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'خطأ!',
                        text: response.error || 'حدث خطأ أثناء التحقق من الأصول',
                        confirmButtonText: 'حسناً'
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: 'error',
                    title: 'خطأ!',
                    text: 'فشل الاتصال بالخادم، يرجى المحاولة مرة أخرى',
                    confirmButtonText: 'حسناً'
                });
                console.error("Error verifying asset tags:", error);
            }
        });
    });

    // Clear Selection Button
    $('#clearSelectionBtn').click(function () {
        selectedAssets.clear();
        $('.row-checkbox').prop('checked', false);
        $('#selectAll').prop('checked', false);
        updateBulkControls();
    });

    // Update bulk controls
    function updateBulkControls() {
        const count = selectedAssets.size;
        $('#selectedCount').text(count);
        $('.bulk-badge').text(count);
        // Update this line to include the supervisor button
        $('#bulkTransferBtn, #bulkDisposeBtn, #bulkReturnBtn, #bulkSupervisorBtn, #clearSelectionBtn').prop('disabled', count === 0);

        if (count > 0) {
            $('.selection-counter').addClass('animate__animated animate__pulse');
            setTimeout(() => {
                $('.selection-counter').removeClass('animate__animated animate__pulse');
            }, 1000);
        }
    }

    // Handle bulk transfer button click
    $('#bulkTransferBtn').click(function () {
        loadDepartmentsAndUsers();
        $('#bulkTransferModal').modal('show');
    });

    // Handle bulk dispose button click
    $('#bulkDisposeBtn').click(function () {
        $('#bulkDisposalModal').modal('show');
    });

    // Function to load departments and users
    function loadDepartmentsAndUsers() {
        const departmentSelect = $('#targetDepartment');
        const userSelect = $('#targetUser');

        // Show loading indicators
        departmentSelect.prop('disabled', true).html('<option>جاري تحميل الأقسام...</option>');
        userSelect.prop('disabled', true).html('<option>جاري تحميل المستلمين...</option>');

        // Load departments
        $.get('/Asset/GetDepartments')
            .done(function (departments) {
                departmentSelect.prop('disabled', false)
                    .empty()
                    .append('<option value="">-- اختر القسم --</option>');

                departments.forEach(dept => {
                    departmentSelect.append(
                        `<option value="${dept.id}">${dept.name}</option>`
                    );
                });
            })
            .fail(function (error) {
                console.error('Failed to load departments:', error);
                departmentSelect.prop('disabled', false)
                    .empty()
                    .append('<option value="">خطأ في تحميل الأقسام</option>');
            });

        // Load users
        $.get('/Asset/GetUsers')
            .done(function (response) {
                userSelect.prop('disabled', false).empty();
                userSelect.append(`<option value="">-- غير محدد --</option>`);

                if (response.success && Array.isArray(response.data)) {
                    if (response.data.length === 0) {
                        userSelect.append(`<option disabled>لا يوجد مستخدمين في النظام</option>`);
                    } else {
                        response.data.forEach(user => {
                            userSelect.append(`<option value="${user.id}">${user.name}</option>`);
                        });
                    }
                } else {
                    userSelect.append(`<option disabled>خطأ: تنسيق بيانات غير صالح</option>`);
                }
            })
            .fail(function (xhr) {
                userSelect.prop('disabled', false)
                    .empty()
                    .append(`<option disabled>خطأ في تحميل المستلمين</option>`);
            });
    }

    // Bulk transfer form submission
    $('#bulkTransferForm').on('submit', function (e) {
        e.preventDefault();

        const targetDepartmentId = parseInt($('#targetDepartment').val());
        const targetUserId = $('#targetDepartmentUser').val() || $('#targetUser').val();

        if (!targetDepartmentId) {
            Swal.fire({
                title: 'خطأ!',
                text: 'الرجاء اختيار القسم الهدف',
                icon: 'error',
                confirmButtonText: 'حسناً'
            });
            return;
        }

        const request = {
            assetTags: Array.from(selectedAssets),
            targetDepartmentId: targetDepartmentId,
            targetUserId: targetUserId || null
        };

        // Show loading overlay
        const loadingOverlay = showLoadingOverlay("جاري نقل الأصول...");

        $.ajax({
            url: '/Asset/BulkTransfer',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(request),
            success: function (response) {
                loadingOverlay.remove();

                $('#bulkTransferModal').modal('hide');
                selectedAssets.clear();
                updateBulkControls();
                table.ajax.reload();

                Swal.fire({
                    title: 'تم بنجاح!',
                    text: 'تم نقل الأصول بنجاح',
                    icon: 'success',
                    confirmButtonText: 'حسناً'
                });
            },
            error: function (xhr, status, error) {
                loadingOverlay.remove();

                Swal.fire({
                    title: 'خطأ!',
                    text: 'فشل نقل الأصول: ' + (xhr.responseJSON?.error || 'خطأ غير معروف'),
                    icon: 'error',
                    confirmButtonText: 'حسناً'
                });
            }
        });
    });

    $('#targetUser').on('change', function () {
        if ($(this).val()) {
            $('#targetDepartmentUser').val('');
        }
    });

    $('#targetDepartmentUser').on('change', function () {
        if ($(this).val()) {
            $('#targetUser').val('');
        }
    });
    $('#targetDepartment').on('change', function () {
        const departmentId = $(this).val();
        const deptUserSelect = $('#targetDepartmentUser');

        if (!departmentId) {
            // If no department selected, clear and disable department user dropdown
            deptUserSelect.prop('disabled', true)
                .empty()
                .append('<option value="">-- غير محدد --</option>');
            return;
        }

        // Show loading indicator
        deptUserSelect.prop('disabled', true)
            .empty()
            .append('<option>جاري تحميل المستلمين...</option>');

        // Fetch users for the selected department
        $.ajax({
            url: '/Asset/GetUsersByDepartment',
            type: 'GET',
            data: { departmentId: departmentId },
            success: function (response) {
                deptUserSelect.empty().prop('disabled', false);
                deptUserSelect.append('<option value="">-- غير محدد --</option>');

                if (response && response.length > 0) {
                    response.forEach(user => {
                        deptUserSelect.append(`<option value="${user.id}">${user.name}</option>`);
                    });
                } else {
                    deptUserSelect.append('<option disabled>لا يوجد مستخدمين في هذا القسم</option>');
                }
            },
            error: function (xhr) {
                deptUserSelect.prop('disabled', false)
                    .empty()
                    .append('<option value="">-- غير محدد --</option>')
                    .append('<option disabled>خطأ في تحميل المستلمين</option>');

                console.error('Error loading department users:', xhr);
            }
        });
    });
    // Bulk disposal form submission
    $('#bulkDisposalForm').on('submit', function (e) {
        e.preventDefault();

        const request = {
            assetTags: Array.from(selectedAssets),
            disposalType: $('#disposalType').val(),
            saleValue: parseFloat($('#saleValue').val()) || 0
        };

        if (!request.disposalType) {
            Swal.fire({
                title: 'خطأ!',
                text: 'الرجاء اختيار نوع التكهين',
                icon: 'error',
                confirmButtonText: 'حسناً'
            });
            return;
        }

        // Show confirmation dialog
        Swal.fire({
            title: 'تأكيد التكهين',
            html: `هل أنت متأكد من رغبتك في تكهين <strong>${request.assetTags.length}</strong> أصول؟<br>هذه العملية لا يمكن التراجع عنها.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'نعم، قم بالتكهين',
            cancelButtonText: 'إلغاء'
        }).then((result) => {
            if (result.isConfirmed) {
                // Show loading overlay
                const loadingOverlay = showLoadingOverlay("جاري تكهين الأصول وإنشاء المستند...");

                // Send the request
                $.ajax({
                    url: '/Asset/BulkDispose',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(request),
                    xhrFields: {
                        responseType: 'blob' // Expect binary data (PDF) in response
                    },
                    success: function (blob) {
                        loadingOverlay.remove();

                        // Handle PDF download
                        const url = window.URL.createObjectURL(blob);
                        const a = document.createElement('a');
                        a.href = url;
                        a.download = `مستند_التكهين_${new Date().toISOString().slice(0, 10)}.pdf`;
                        document.body.appendChild(a);
                        a.click();
                        window.URL.revokeObjectURL(url);

                        // Clean up and update UI
                        $('#bulkDisposalModal').modal('hide');
                        selectedAssets.clear();
                        updateBulkControls();
                        table.ajax.reload();

                        // Show success message
                        Swal.fire({
                            title: 'تم بنجاح!',
                            html: `تم تكهين الأصول بنجاح.<br>جاري تنزيل مستند التكهين...`,
                            icon: 'success',
                            confirmButtonText: 'حسناً'
                        });

                        // Refresh asset stats
                        fetchAssetStats();
                    },
                    error: function (xhr) {
                        loadingOverlay.remove();

                        // Try to parse error message
                        let errorMessage = 'حدث خطأ أثناء تنفيذ عملية التكهين';
                        if (xhr.responseType !== 'blob') {
                            try {
                                const errorResponse = JSON.parse(xhr.responseText);
                                errorMessage = errorResponse.error || errorMessage;
                            } catch (e) {
                                console.error('Error parsing error response:', e);
                            }
                        }

                        Swal.fire({
                            title: 'خطأ!',
                            text: errorMessage,
                            icon: 'error',
                            confirmButtonText: 'حسناً'
                        });
                    }
                });
            }
        });
    });

    // Handle individual dispose button click
    $(document).on('click', '.dispose-btn', function () {
        const assetTag = $(this).data('asset-tag');
        selectedAssets.clear();
        selectedAssets.add(assetTag);
        updateBulkControls();

        // Trigger the bulk disposal modal
        $('#bulkDisposalModal').modal('show');
    });

    // Handle the Open in New Page button
    $('#filterNewPage').click(function () {
        const tagsInput = $('#assetTagsInput').val();
        const tags = processAssetTags(tagsInput);

        if (tags.length === 0) {
            Swal.fire({
                title: 'خطأ!',
                text: 'الرجاء إدخال Asset Tag واحد على الأقل',
                icon: 'error',
                confirmButtonText: 'حسناً'
            });
            return;
        }

        // Create a form to submit to the new page
        const form = $('<form></form>').attr({
            id: 'newPageForm',
            action: '/Asset/FilteredAssets',
            method: 'post'
        });

        // Add inputs for each tag
        tags.forEach(tag => {
            form.append(`<input type="hidden" name="assetTags" value="${tag}">`);
        });

        // Add form to body and submit
        $('body').append(form);
        $('#newPageForm').submit();
    });

    // Return Document Button
    $('#bulkReturnBtn').on('click', function () {
        if (selectedAssets.size === 0) {
            Swal.fire({
                title: 'خطأ!',
                text: 'الرجاء تحديد أصول أولاً',
                icon: 'error',
                confirmButtonText: 'حسناً'
            });
            return;
        }

        // Redirect to the return document creation page with selected assets
        const assetTags = Array.from(selectedAssets);

        // Create a form to submit
        const form = $('<form></form>').attr({
            method: 'post',
            action: '@Url.Action("Create", "ReturnDocument")'
        });

        // Add asset tags as hidden inputs
        assetTags.forEach(tag => {
            form.append($('<input>').attr({
                type: 'hidden',
                name: 'AssetTags',
                value: tag
            }));
        });

        // Add CSRF token
        form.append($('<input>').attr({
            type: 'hidden',
            name: '__RequestVerificationToken',
            value: $('input[name="__RequestVerificationToken"]').val()
        }));

        // Append form to body and submit
        $('body').append(form);
        form.submit();
    });

    // Refresh table button
    $('#refreshTable').click(function () {
        table.ajax.reload();
        fetchAssetStats();

        // Show toast notification
        Swal.fire({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            icon: 'success',
            title: 'تم تحديث البيانات'
        });
    });

    // Download template button
    $('#downloadTemplate').click(function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'تحميل النموذج',
            text: 'سيتم تحميل نموذج Excel فارغ لاستيراد الأصول',
            icon: 'info',
            showCancelButton: true,
            confirmButtonText: 'تحميل',
            cancelButtonText: 'إلغاء'
        }).then((result) => {
            if (result.isConfirmed) {
                // Here you'd typically trigger the actual download
                // This would be a link to an action on your controller
                window.location.href = '@Url.Action("DownloadTemplate", "Asset")';
            }
        });
    });
    // Supervisor button click handler
    $('#bulkSupervisorBtn').click(function () {
        $('#supervisorAssetCount').text(selectedAssets.size);
        loadSupervisors();
        $('#bulkSupervisorModal').modal('show');
    });

    // Function to load supervisors for the dropdown
    function loadSupervisors() {
        $.ajax({
            url: '/Asset/GetSupervisors',
            type: 'GET',
            success: function (response) {
                const select = $('#supervisorSelect');
                select.empty().append('<option value="">-- اختر المشرف --</option>');

                if (response.success && Array.isArray(response.data)) {
                    response.data.forEach(supervisor => {
                        select.append(`<option value="${supervisor.id}">${supervisor.name}</option>`);
                    });
                } else {
                    select.append('<option value="" disabled>خطأ في تحميل المشرفين</option>');
                }
            },
            error: function (error) {
                console.error('Error loading supervisors:', error);
                Swal.fire({
                    title: 'خطأ!',
                    text: 'فشل تحميل قائمة المشرفين. الرجاء المحاولة مرة أخرى.',
                    icon: 'error',
                    confirmButtonText: 'حسناً'
                });
            }
        });
    }

    // Handle supervisor form submission
    $('#bulkSupervisorForm').on('submit', function (e) {
        e.preventDefault();

        const supervisorId = $('#supervisorSelect').val();
        if (!supervisorId) {
            Swal.fire({
                title: 'تنبيه!',
                text: 'الرجاء اختيار مشرف',
                icon: 'warning',
                confirmButtonText: 'حسناً'
            });
            return;
        }

        // Show loading indicator
        const loadingOverlay = showLoadingOverlay("جاري تعيين المشرف...");

        // Send request to assign supervisor
        $.ajax({
            url: '/Asset/BulkAssignSupervisor',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                assetTags: Array.from(selectedAssets),
                supervisorId: supervisorId
            }),
            success: function (response) {
                loadingOverlay.remove();
                $('#bulkSupervisorModal').modal('hide');

                // Clear selections
                selectedAssets.clear();
                updateBulkControls();

                // Reload the table
                table.ajax.reload();

                // Show success message
                Swal.fire({
                    title: 'تم بنجاح!',
                    text: 'تم تعيين المشرف بنجاح',
                    icon: 'success',
                    confirmButtonText: 'حسناً'
                });
            },
            error: function (xhr) {
                loadingOverlay.remove();
                Swal.fire({
                    title: 'خطأ!',
                    text: 'فشل تعيين المشرف: ' + (xhr.responseJSON?.error || 'خطأ غير معروف'),
                    icon: 'error',
                    confirmButtonText: 'حسناً'
                });
            }
        });
    });

    // ================================
    // Enhanced Header Functionality
    // ================================

    // Enhanced Header Initialization
    function initializeEnhancedHeader() {
        console.log('🎨 Initializing enhanced header functionality');
        
        // Initialize tooltips for header buttons
        $('[data-bs-toggle="tooltip"]').tooltip({
            placement: 'bottom',
            trigger: 'hover',
            delay: { show: 300, hide: 100 }
        });
        
        // Global search functionality
        $('#globalSearch').on('input', function() {
            const searchTerm = $(this).val().toLowerCase();
            if (table && table.search) {
                table.search(searchTerm).draw();
            }
            updateSearchIndicator(searchTerm);
        });
        
        // Clear search on escape key
        $('#globalSearch').on('keydown', function(e) {
            if (e.key === 'Escape') {
                $(this).val('');
                if (table && table.search) {
                    table.search('').draw();
                }
                updateSearchIndicator('');
            }
        });
        
        // Refresh data button
        $('#refreshDataBtn').on('click', function() {
            refreshAssetData();
        });
        
        // Update connection status
        updateConnectionStatus();
        
        // Update last refresh time
        updateLastRefreshTime();
        
        // Initialize auto-refresh timer
        setInterval(updateLastRefreshTime, 30000); // Update every 30 seconds
    }

    // Update search indicator
    function updateSearchIndicator(searchTerm) {
        const searchInput = $('#globalSearch');
        const inputGroup = searchInput.closest('.input-group');
        
        if (searchTerm.length > 0) {
            inputGroup.addClass('searching');
            searchInput.addClass('is-valid');
        } else {
            inputGroup.removeClass('searching');
            searchInput.removeClass('is-valid');
        }
    }

    // Refresh asset data
    function refreshAssetData() {
        const refreshBtn = $('#refreshDataBtn');
        const originalHtml = refreshBtn.html();
        
        // Show loading state
        refreshBtn.html('<i class="fas fa-spinner fa-spin"></i>').prop('disabled', true);
        
        try {
            // Reload DataTable if it exists
            if (table && table.ajax) {
                table.ajax.reload(function() {
                    // Refresh completed
                    refreshBtn.html(originalHtml).prop('disabled', false);
                    updateLastRefreshTime();
                    updateDisplayedCount();
                    
                    Utils.showToast('تم', 'تم تحديث البيانات بنجاح', 'success', 2000);
                });
            } else {
                // Fallback: refresh page
                setTimeout(() => {
                    location.reload();
                }, 1000);
            }
            
            // Also refresh statistics
            fetchAssetStats();
            
        } catch (error) {
            console.error('Error refreshing data:', error);
            refreshBtn.html(originalHtml).prop('disabled', false);
            Utils.showToast('خطأ', 'فشل في تحديث البيانات', 'error');
        }
    }

    // Update connection status
    function updateConnectionStatus() {
        const statusElement = $('.connection-status small');
        
        // Simple connectivity check
        if (navigator.onLine) {
            statusElement.html('<i class="fas fa-circle me-1 text-success" style="font-size: 8px;"></i>متصل')
                        .removeClass('text-danger')
                        .addClass('text-success');
        } else {
            statusElement.html('<i class="fas fa-circle me-1 text-danger" style="font-size: 8px;"></i>غير متصل')
                        .removeClass('text-success')
                        .addClass('text-danger');
        }
    }

    // Update last refresh time
    function updateLastRefreshTime() {
        const now = new Date();
        const timeString = now.toLocaleTimeString('ar-SA', {
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
        $('#lastUpdateTime').text(timeString);
    }

    // Update displayed count
    function updateDisplayedCount() {
        if (table && table.rows) {
            const displayedCount = table.rows({ page: 'current' }).count();
            $('#displayedCount').text(displayedCount);
        }
    }

    // Update total assets count
    function updateTotalAssetsCount(count) {
        $('#totalAssetsCount').text(count || 0);
        
        // Animate the count update
        $('#totalAssetsCount').addClass('text-primary fw-bold')
                              .fadeOut(200)
                              .fadeIn(400)
                              .removeClass('text-primary fw-bold', 2000);
    }

    // Enhanced search with suggestions (future enhancement)
    function initializeSmartSearch() {
        const searchInput = $('#globalSearch');
        const suggestionsContainer = $('<div class="search-suggestions position-absolute bg-white border rounded shadow-sm d-none"></div>');
        
        searchInput.parent().append(suggestionsContainer);
        
        searchInput.on('focus', function() {
            $(this).parent().find('.search-suggestions').removeClass('d-none');
        });
        
        searchInput.on('blur', function() {
            setTimeout(() => {
                $(this).parent().find('.search-suggestions').addClass('d-none');
            }, 200);
        });
    }

    // Listen for online/offline events
    window.addEventListener('online', updateConnectionStatus);
    window.addEventListener('offline', updateConnectionStatus);

    // ================================
    // Enhanced Header Integration
    // ================================

    // Add to main initialization
    $(document).ready(function() {
        // Initialize enhanced header after a short delay to ensure other components are ready
        setTimeout(initializeEnhancedHeader, 500);
        setTimeout(initializeSmartSearch, 600);
    });

    // ...existing code...
    
    // ============================================================================
    // PROFESSIONAL ENHANCEMENTS - Advanced Features
    // ============================================================================

    // Advanced keyboard shortcuts
    $(document).on('keydown', function(e) {
        // Ctrl+A: Select All Assets
        if (e.ctrlKey && e.key === 'a' && !$(e.target).is('input, textarea')) {
            e.preventDefault();
            $('#selectAll').prop('checked', true).trigger('change');
            Utils.showToast('تحديد', 'تم تحديد جميع الأصول', 'info', 2000);
        }

        // Ctrl+R: Refresh Data
        if (e.ctrlKey && e.key === 'r') {
            e.preventDefault();
            $('#refreshTable').trigger('click');
        }

        // Escape: Clear Selection
        if (e.key === 'Escape') {
            $('#clearSelectionBtn').trigger('click');
        }

        // Ctrl+F: Focus Search
        if (e.ctrlKey && e.key === 'f') {
            e.preventDefault();
            $('#assetsTable_filter input').focus();
        }
    });

    // Professional search with autocomplete enhancement
    let searchTimeout;
    $('#assetsTable_filter input').on('input', function() {
        const searchTerm = $(this).val();
        clearTimeout(searchTimeout);
        
        // Add visual feedback for searching
        $(this).addClass('searching');
        
        searchTimeout = setTimeout(() => {
            $(this).removeClass('searching');
            
            if (searchTerm.length > 2) {
                // Store last search for analytics
                AssetManagement.state.lastSearch = {
                    term: searchTerm,
                    timestamp: new Date()
                };
            }
        }, 500);
    });

    // Advanced tooltips with custom content
    function initializeAdvancedTooltips() {
        // Asset tag tooltips with additional info
        $(document).on('mouseenter', '.asset-tag-badge', function() {
            const assetTag = $(this).text();
            const tooltip = $(`
                <div class="professional-tooltip">
                    <div class="tooltip-header">
                        <strong>رقم الأصل: ${assetTag}</strong>
                    </div>
                    <div class="tooltip-body">
                        <small>انقر للنسخ • مزدوج للتفاصيل</small>
                    </div>
                </div>
            `);
            
            $('body').append(tooltip);
            
            const rect = this.getBoundingClientRect();
            tooltip.css({
                top: rect.bottom + 5,
                left: rect.left,
                display: 'block'
            });
        }).on('mouseleave', '.asset-tag-badge', function() {
            $('.professional-tooltip').remove();
        });

        // Click to copy asset tag
        $(document).on('click', '.asset-tag-badge', function() {
            const assetTag = $(this).text();
            navigator.clipboard.writeText(assetTag).then(() => {
                Utils.showToast('نسخ', `تم نسخ رقم الأصل: ${assetTag}`, 'success', 2000);
                $(this).addClass('copied').delay(1000).queue(() => $(this).removeClass('copied').dequeue());
            });
        });

        // Double click for details
        $(document).on('dblclick', '.asset-tag-badge', function() {
            const assetTag = $(this).text();
            window.open(`/Asset/Details/${assetTag}`, '_blank');
        });
    }

    // Advanced data export with custom options
    function initializeAdvancedExport() {
        $('#advancedExportBtn').on('click', function() {
            const exportModal = $(`
                <div class="modal fade" id="advancedExportModal" tabindex="-1">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content professional-modal">
                            <div class="modal-header">
                                <h5 class="modal-title">
                                    <i class="fas fa-download me-2"></i>
                                    تصدير متقدم للبيانات
                                </h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                            </div>
                            <div class="modal-body">
                                <form id="advancedExportForm">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <h6>نوع التصدير</h6>
                                            <div class="form-check">
                                                <input class="form-check-input" type="radio" name="exportType" value="all" id="exportAll" checked>
                                                <label class="form-check-label" for="exportAll">جميع الأصول</label>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="radio" name="exportType" value="selected" id="exportSelected">
                                                <label class="form-check-label" for="exportSelected">الأصول المحددة فقط</label>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="radio" name="exportType" value="filtered" id="exportFiltered">
                                                <label class="form-check-label" for="exportFiltered">النتائج المفلترة</label>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <h6>تنسيق الملف</h6>
                                            <div class="form-check">
                                                <input class="form-check-input" type="radio" name="fileFormat" value="excel" id="formatExcel" checked>
                                                <label class="form-check-label" for="formatExcel">Excel (.xlsx)</label>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="radio" name="fileFormat" value="csv" id="formatCsv">
                                                <label class="form-check-label" for="formatCsv">CSV (.csv)</label>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="radio" name="fileFormat" value="pdf" id="formatPdf">
                                                <label class="form-check-label" for="formatPdf">PDF (.pdf)</label>
                                            </div>
                                        </div>
                                    </div>
                                    <hr>
                                    <div class="row">
                                        <div class="col-12">
                                            <h6>الأعمدة المراد تصديرها</h6>
                                            <div class="row">
                                                <div class="col-md-4">
                                                    <div class="form-check">
                                                        <input class="form-check-input column-check" type="checkbox" value="assetTag" id="colAssetTag" checked>
                                                        <label class="form-check-label" for="colAssetTag">رقم الأصل</label>
                                                    </div>
                                                    <div class="form-check">
                                                        <input class="form-check-input column-check" type="checkbox" value="description" id="colDescription" checked>
                                                        <label class="form-check-label" for="colDescription">الوصف</label>
                                                    </div>
                                                    <div class="form-check">
                                                        <input class="form-check-input column-check" type="checkbox" value="department" id="colDepartment" checked>
                                                        <label class="form-check-label" for="colDepartment">القسم</label>
                                                    </div>
                                                </div>
                                                <div class="col-md-4">
                                                    <div class="form-check">
                                                        <input class="form-check-input column-check" type="checkbox" value="brand" id="colBrand" checked>
                                                        <label class="form-check-label" for="colBrand">الماركة</label>
                                                    </div>
                                                    <div class="form-check">
                                                        <input class="form-check-input column-check" type="checkbox" value="model" id="colModel" checked>
                                                        <label class="form-check-label" for="colModel">الموديل</label>
                                                    </div>
                                                    <div class="form-check">
                                                        <input class="form-check-input column-check" type="checkbox" value="status" id="colStatus" checked>
                                                        <label class="form-check-label" for="colStatus">الحالة</label>
                                                    </div>
                                                </div>
                                                <div class="col-md-4">
                                                    <div class="form-check">
                                                        <input class="form-check-input column-check" type="checkbox" value="facility" id="colFacility">
                                                        <label class="form-check-label" for="colFacility">المنشأة</label>
                                                    </div>
                                                    <div class="form-check">
                                                        <input class="form-check-input column-check" type="checkbox" value="building" id="colBuilding">
                                                        <label class="form-check-label" for="colBuilding">المبنى</label>
                                                    </div>
                                                    <div class="form-check">
                                                        <input class="form-check-input column-check" type="checkbox" value="location" id="colLocation">
                                                        <label class="form-check-label" for="colLocation">الموقع الكامل</label>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="mt-2">
                                                <button type="button" class="btn btn-sm btn-outline-primary" id="selectAllColumns">تحديد الكل</button>
                                                <button type="button" class="btn btn-sm btn-outline-secondary" id="clearAllColumns">إلغاء الكل</button>
                                            </div>
                                        </div>
                                    </div>
                                </form>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">إلغاء</button>
                                <button type="button" class="btn btn-primary" id="executeExport">
                                    <i class="fas fa-download me-2"></i>تصدير البيانات
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `);

            $('body').append(exportModal);
            $('#advancedExportModal').modal('show');

            // Handle column selection
            $('#selectAllColumns').on('click', () => $('.column-check').prop('checked', true));
            $('#clearAllColumns').on('click', () => $('.column-check').prop('checked', false));

            // Handle export execution
            $('#executeExport').on('click', function() {
                const formData = {
                    exportType: $('input[name="exportType"]:checked').val(),
                    fileFormat: $('input[name="fileFormat"]:checked').val(),
                    columns: $('.column-check:checked').map(function() { return this.value; }).get(),
                    selectedAssets: Array.from(selectedAssets)
                };

                if (formData.columns.length === 0) {
                    Utils.showToast('خطأ', 'يجب تحديد عمود واحد على الأقل', 'error');
                    return;
                }

                executeAdvancedExport(formData);
            });

            // Clean up modal when closed
            $('#advancedExportModal').on('hidden.bs.modal', function() {
                $(this).remove();
            });
        });
    }

    function executeAdvancedExport(options) {
        const loadingOverlay = Utils.showLoadingOverlay("جاري تحضير ملف التصدير...", true);

        // Simulate progress
        let progress = 0;
        const progressInterval = setInterval(() => {
            progress += Math.random() * 15;
            if (progress > 90) progress = 90;
            $('.progress-fill').css('width', progress + '%');
        }, 200);

        $.ajax({
            url: '/Asset/AdvancedExport',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(options),
            xhrFields: {
                responseType: 'blob'
            },
            success: function(blob, status, xhr) {
                clearInterval(progressInterval);
                $('.progress-fill').css('width', '100%');

                setTimeout(() => {
                    loadingOverlay.remove();

                    // Extract filename from response headers
                    const contentDisposition = xhr.getResponseHeader('Content-Disposition');
                    let filename = 'تصدير_الأصول';
                    if (contentDisposition) {
                        const matches = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                        if (matches && matches[1]) {
                            filename = matches[1].replace(/['"]/g, '');
                        }
                    }

                    // Add file extension based on format
                    const extension = {
                        'excel': '.xlsx',
                        'csv': '.csv',
                        'pdf': '.pdf'
                    }[options.fileFormat] || '.xlsx';

                    if (!filename.includes('.')) {
                        filename += extension;
                    }

                    // Download file
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = filename;
                    document.body.appendChild(a);
                    a.click();
                    window.URL.revokeObjectURL(url);
                    document.body.removeChild(a);

                    $('#advancedExportModal').modal('hide');
                    Utils.showToast('نجح', 'تم تصدير البيانات بنجاح', 'success');
                }, 500);
            },
            error: function(xhr) {
                clearInterval(progressInterval);
                loadingOverlay.remove();
                Utils.handleApiError(xhr, 'تصدير البيانات');
            }
        });
    }

    // Performance monitoring and analytics
    function initializePerformanceMonitoring() {
        // Track page load time
        window.addEventListener('load', function() {
            const loadTime = performance.now() - AssetManagement.performance.startTime;
            console.log(`📊 Page loaded in ${loadTime.toFixed(2)}ms`);
            
            if (loadTime > 5000) {
                console.warn('⚠️ Slow page load detected');
            }
        });

        // Track DataTable performance
        $('#assetsTable').on('processing.dt', function(e, settings, processing) {
            if (processing) {
                AssetManagement.performance.lastRequestStart = performance.now();
            } else {
                const duration = performance.now() - AssetManagement.performance.lastRequestStart;
                console.log(`📊 DataTable request completed in ${duration.toFixed(2)}ms`);
            }
        });

        // Memory usage monitoring (if available)
        if (performance.memory) {
            setInterval(() => {
                const memInfo = performance.memory;
                if (memInfo.usedJSHeapSize > memInfo.jsHeapSizeLimit * 0.9) {
                    console.warn('⚠️ High memory usage detected');
                }
            }, 30000);
        }
    }

    // Real-time notifications system
    function initializeRealTimeNotifications() {
        // Check for updates every 30 seconds
        setInterval(() => {
            if (!document.hidden && !AssetManagement.state.isLoading) {
                checkForUpdates();
            }
        }, AssetManagement.config.refreshInterval);

        // Handle visibility change
        document.addEventListener('visibilitychange', function() {
            if (!document.hidden) {
                // Page became visible, check for updates
                checkForUpdates();
            }
        });
    }

    function checkForUpdates() {
        $.ajax({
            url: '/Asset/CheckUpdates',
            type: 'GET',
            data: { lastUpdate: AssetManagement.cache.lastRefresh },
            success: function(response) {
                if (response.hasUpdates) {
                    showUpdateNotification(response.updateCount);
                }
            },
            error: function() {
                // Silently fail for update checks
            }
        });
    }

    function showUpdateNotification(count) {
        const notification = $(`
            <div class="update-notification">
                <div class="notification-content">
                    <i class="fas fa-sync-alt"></i>
                    <span>تم العثور على ${count} تحديث جديد</span>
                    <button class="btn btn-sm btn-primary ms-2" onclick="refreshDataWithNotification()">تحديث</button>
                    <button class="btn btn-sm btn-outline-secondary ms-1" onclick="dismissNotification()">×</button>
                </div>
            </div>
        `);

        $('.toast-container').append(notification);
        setTimeout(() => notification.addClass('show'), 100);
    }

    window.refreshDataWithNotification = function() {
        table.ajax.reload();
        fetchAssetStats();
        $('.update-notification').removeClass('show');
        setTimeout(() => $('.update-notification').remove(), 300);
        Utils.showToast('تحديث', 'تم تحديث البيانات', 'success');
    };

    window.dismissNotification = function() {
        $('.update-notification').removeClass('show');
        setTimeout(() => $('.update-notification').remove(), 300);
    };

    // Initialize all professional enhancements
    initializeAdvancedTooltips();
    initializeAdvancedExport();
    initializePerformanceMonitoring();
    initializeRealTimeNotifications();

    // Professional UI state persistence
    const uiState = {
        save: function() {
            localStorage.setItem('assetManagement_uiState', JSON.stringify({
                tablePageLength: table.page.len(),
                selectedColumns: table.columns().visible().toArray(),
                lastRefresh: AssetManagement.cache.lastRefresh,
                userPreferences: {
                    autoRefresh: true,
                    showAnimations: true,
                    compactView: false
                }
            }));
        },
        
        load: function() {
            try {
                const saved = localStorage.getItem('assetManagement_uiState');
                if (saved) {
                    const state = JSON.parse(saved);
                    // Apply saved state
                    if (state.tablePageLength) {
                        table.page.len(state.tablePageLength);
                    }
                    return state;
                }
            } catch (e) {
                console.warn('Failed to load UI state:', e);
            }
            return null;
        }
    };

    // Save state on page unload
    window.addEventListener('beforeunload', uiState.save);

    // Load saved state
    const savedState = uiState.load();
    if (savedState) {
        console.log('✅ UI state restored from previous session');
    }

    // Final initialization message
    console.log('🎉 Professional Asset Management System fully loaded and enhanced!');
    
    // Load asset statistics on page load
    fetchAssetStats();
    
    // Show welcome message for first-time users
    if (!localStorage.getItem('assetManagement_welcomed')) {
        setTimeout(() => {
            Utils.showToast(
                'مرحباً بك', 
                'استخدم Ctrl+F للبحث، Ctrl+A لتحديد الكل، Escape لإلغاء التحديد', 
                'info', 
                5000
            );
            localStorage.setItem('assetManagement_welcomed', 'true');
        }, 2000);
    }

});