// Tabler Custom JavaScript for Asset Management System
$(document).ready(function () {
    // Load notifications
    loadNotifications();

    // Refresh notifications every 60 seconds
    setInterval(loadNotifications, 60000);

    // Dark mode toggle
    let darkMode = localStorage.getItem('darkMode') === 'enabled';
    applyDarkMode(darkMode);

    $('#darkModeToggle').on('click', function (e) {
        e.preventDefault();
        darkMode = !darkMode;
        localStorage.setItem('darkMode', darkMode ? 'enabled' : 'disabled');
        applyDarkMode(darkMode);
    });

    // Enhanced desktop and mobile sidebar toggle
    $('.navbar-toggler, .sidebar-toggle-btn').on('click', function (e) {
        e.preventDefault();
        toggleSidebar();
    });

    // Close sidebar when clicking overlay (mobile)
    $(document).on('click', '.navbar-overlay', function () {
        closeSidebar();
    });

    // Handle window resize for responsive behavior
    $(window).on('resize', function () {
        if ($(window).width() > 991.98) {
            // Desktop: Remove mobile-specific classes
            $('.navbar-overlay').removeClass('show');
            $('body').removeClass('navbar-vertical-show').css('overflow', '');
        } else {
            // Mobile: Reset to collapsed state
            $('.navbar-vertical').removeClass('navbar-vertical-collapsed');
            $('.page-wrapper').removeClass('sidebar-collapsed');
        }
    });

    // Add smooth scroll behavior for quick actions
    $('.quick-actions-grid a').on('click', function (e) {
        const $btn = $(this);
        $btn.addClass('clicked');

        // Add success feedback animation
        const originalIcon = $btn.find('i').attr('class');
        $btn.find('i').removeClass().addClass('fas fa-check text-success');

        setTimeout(() => {
            $btn.removeClass('clicked');
            $btn.find('i').removeClass().addClass(originalIcon);
        }, 1000);

        // Add ripple effect
        const $ripple = $('<span class="ripple"></span>');
        $btn.append($ripple);

        setTimeout(() => {
            $ripple.remove();
        }, 600);
    });

    // Enhanced click feedback with CSS
    $('<style>')
        .prop('type', 'text/css')
        .html(`
            .ripple {
                position: absolute;
                border-radius: 50%;
                background: rgba(255, 255, 255, 0.6);
                transform: scale(0);
                animation: ripple-animation 0.6s linear;
                pointer-events: none;
                top: 50%;
                left: 50%;
                width: 20px;
                height: 20px;
                margin-top: -10px;
                margin-left: -10px;
            }

            @keyframes ripple-animation {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }
        `)
        .appendTo('head');    // Add current page highlighting
    highlightCurrentPage();

    // Add hover effects for nav items
    $('.navbar-vertical .nav-link').on('mouseenter', function () {
        $(this).find('.nav-link-icon').addClass('animate');
    }).on('mouseleave', function () {
        $(this).find('.nav-link-icon').removeClass('animate');
    });

    // Restore sidebar state from previous session
    restoreSidebarState();

    // Add keyboard shortcut for sidebar toggle (Ctrl + B)
    $(document).on('keydown', function(e) {
        if (e.ctrlKey && e.key === 'b') {
            e.preventDefault();
            toggleSidebar();
        }
    });
});

// Sidebar toggle functionality
function toggleSidebar() {
    const $sidebar = $('.navbar-vertical');
    const $pageWrapper = $('.page-wrapper');
    const $toggler = $('.navbar-toggler, .sidebar-toggle-btn');
    
    if ($(window).width() > 991.98) {
        // Desktop behavior: collapse/expand
        $sidebar.toggleClass('navbar-vertical-collapsed');
        $pageWrapper.toggleClass('sidebar-collapsed');
        
        // Save state to localStorage
        const isCollapsed = $sidebar.hasClass('navbar-vertical-collapsed');
        localStorage.setItem('sidebarCollapsed', isCollapsed);        // Update toggle button icon
        const icon = $toggler.find('i');
        if (isCollapsed) {
            icon.removeClass('fa-bars').addClass('fa-eye');
        } else {
            icon.removeClass('fa-eye').addClass('fa-bars');
        }
    } else {
        // Mobile behavior: show/hide
        $sidebar.toggleClass('navbar-vertical-show');
        $toggler.toggleClass('collapsed');
        
        // Create overlay if it doesn't exist
        if (!$('.navbar-overlay').length) {
            $('body').append('<div class="navbar-overlay"></div>');
        }
        
        if ($sidebar.hasClass('navbar-vertical-show')) {
            $('.navbar-overlay').addClass('show');
            $('body').addClass('navbar-vertical-show').css('overflow', 'hidden');
        } else {
            $('.navbar-overlay').removeClass('show');
            $('body').removeClass('navbar-vertical-show').css('overflow', '');
        }
    }
}

function closeSidebar() {
    $('.navbar-vertical').removeClass('navbar-vertical-show');
    $('.navbar-toggler, .sidebar-toggle-btn').removeClass('collapsed');
    $('.navbar-overlay').removeClass('show');
    $('body').removeClass('navbar-vertical-show').css('overflow', '');
}

function restoreSidebarState() {
    // Restore sidebar state from localStorage on page load
    if ($(window).width() > 991.98) {
        const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (isCollapsed) {
            $('.navbar-vertical').addClass('navbar-vertical-collapsed');
            $('.page-wrapper').addClass('sidebar-collapsed');
            $('.navbar-toggler i, .sidebar-toggle-btn i').removeClass('fa-bars').addClass('fa-eye');
        }
    }
}

function highlightCurrentPage() {
    const currentPath = window.location.pathname;
    $('.navbar-vertical .nav-link').each(function () {
        const linkPath = $(this).attr('href');
        if (linkPath && currentPath.includes(linkPath.split('/').pop())) {
            $(this).addClass('active').siblings().removeClass('active');
        }
    });
}

function applyDarkMode(enabled) {
    const body = document.body;
    const navbar = document.querySelector('.navbar-vertical');
    const pageWrapper = document.querySelector('.page-wrapper');
    const pageHeader = document.querySelector('.page-header');
    
    if (enabled) {
        // Apply dark theme
        document.documentElement.setAttribute('data-bs-theme', 'dark');
        body.classList.add('dark-mode');
        
        // Update icon
        $('#darkModeToggle i').removeClass('fa-moon').addClass('fa-sun');
        
        // Apply dark styles to main components
        if (navbar) {
            navbar.style.background = 'linear-gradient(180deg, #1a1a1a 0%, #2d2d2d 50%, #3a3a3a 100%)';
        }
        if (pageWrapper) {
            pageWrapper.style.background = '#1a1a1a';
            pageWrapper.style.color = '#e9ecef';
        }
        if (pageHeader) {
            pageHeader.style.background = '#2d2d2d';
            pageHeader.style.borderBottom = '1px solid #495057';
        }
        
        // Update cards and other elements
        document.querySelectorAll('.card').forEach(card => {
            card.style.background = '#2d2d2d';
            card.style.borderColor = '#495057';
            card.style.color = '#e9ecef';
        });
        
        // Update dropdowns
        document.querySelectorAll('.dropdown-menu').forEach(dropdown => {
            dropdown.style.background = '#2d2d2d';
            dropdown.style.borderColor = '#495057';
        });
        
    } else {
        // Apply light theme
        document.documentElement.setAttribute('data-bs-theme', 'light');
        body.classList.remove('dark-mode');
        
        // Update icon
        $('#darkModeToggle i').removeClass('fa-sun').addClass('fa-moon');
        
        // Reset to light styles
        if (navbar) {
            navbar.style.background = 'linear-gradient(180deg, #1e3a5f 0%, #2c4a6b 50%, #3a5a77 100%)';
        }
        if (pageWrapper) {
            pageWrapper.style.background = '';
            pageWrapper.style.color = '';
        }
        if (pageHeader) {
            pageHeader.style.background = '';
            pageHeader.style.borderBottom = '';
        }
        
        // Reset cards and other elements
        document.querySelectorAll('.card').forEach(card => {
            card.style.background = '';
            card.style.borderColor = '';
            card.style.color = '';
        });
        
        // Reset dropdowns
        document.querySelectorAll('.dropdown-menu').forEach(dropdown => {
            dropdown.style.background = '';
            dropdown.style.borderColor = '';
        });
    }
}

function loadNotifications() {
    // Get the notification URL from a data attribute that will be set in the layout
    const notificationUrl = document.querySelector('[data-notification-url]')?.getAttribute('data-notification-url');
    if (!notificationUrl) return;

    $.ajax({
        url: notificationUrl,
        type: 'GET',
        success: function (response) {
            if (response.success) {
                // Update the notification badge
                const badgeElem = $('#notification-badge');
                const count = response.count;

                if (count > 0) {
                    badgeElem.text(count).show().addClass('animate-pulse');
                } else {
                    badgeElem.hide().removeClass('animate-pulse');
                }

                // Update the notification list
                const listElem = $('#notification-list');
                listElem.empty();

                if (response.notifications.length === 0) {
                    listElem.html('<div class="notification-item text-center text-muted">لا توجد إشعارات جديدة</div>');
                } else {
                    response.notifications.forEach(notification => {
                        let actionButton = '';
                        if (notification.actionUrl) {
                            actionButton = `<a href="${notification.actionUrl}" class="btn btn-sm btn-primary ms-2">عرض</a>`;
                        }

                        listElem.append(`
                            <div class="notification-item">
                                <div class="d-flex align-items-start">
                                    <div class="flex-grow-1">
                                        <div class="fw-bold">${notification.title}</div>
                                        <div class="text-muted small">${notification.message}</div>
                                        <div class="text-muted small">${notification.timeAgo}</div>
                                    </div>
                                    <div class="d-flex gap-1">
                                        <form action="/Notification/MarkAsRead/${notification.id}" method="post" class="d-inline">
                                            <button type="submit" class="btn btn-sm btn-ghost-secondary">
                                                <i class="fas fa-check"></i>
                                            </button>
                                        </form>
                                        ${actionButton}
                                    </div>
                                </div>
                            </div>
                        `);
                    });
                }
            } else {
                console.error('Failed to load notifications:', response.error);
            }
        },
        error: function () {
            console.error('Error fetching notifications');
            $('#notification-list').html('<div class="notification-item text-center text-danger">خطأ في تحميل الإشعارات</div>');
        }
    });
}