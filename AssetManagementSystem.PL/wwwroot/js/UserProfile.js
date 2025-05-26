    /* ==========================================
   Professional User Profile JavaScript
   ========================================== */

document.addEventListener('DOMContentLoaded', function() {
    initializeUserProfile();
});

function initializeUserProfile() {
    // Initialize all profile features
    initializeCounterAnimations();
    initializeProfileAnimations();
    initializeAssetCards();
    initializeTooltips();
    initializeScrollAnimations();
    initializeThemeToggle();
    initializeSearchAndFilter();
    initializeProfileActions();
    initializeAccessibility();
}

/* ==========================================
   Counter Animations
   ========================================== */
function initializeCounterAnimations() {
    const counters = document.querySelectorAll('.counter');
    
    // Intersection Observer for counter animation
    const counterObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounter(entry.target);
                counterObserver.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.5
    });

    counters.forEach(counter => {
        counterObserver.observe(counter);
    });
}

function animateCounter(element) {
    const target = parseInt(element.getAttribute('data-target') || element.textContent);
    const duration = 2000; // 2 seconds
    const step = target / (duration / 16); // 60fps
    let current = 0;

    const timer = setInterval(() => {
        current += step;
        if (current >= target) {
            element.textContent = target;
            clearInterval(timer);
            
            // Add completion effect
            element.style.transform = 'scale(1.1)';
            setTimeout(() => {
                element.style.transform = 'scale(1)';
            }, 200);
        } else {
            element.textContent = Math.floor(current);
        }
    }, 16);
}

/* ==========================================
   Profile Animations
   ========================================== */
function initializeProfileAnimations() {
    // Profile avatar hover effect
    const profileAvatar = document.querySelector('.profile-avatar');
    if (profileAvatar) {
        profileAvatar.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.05) rotate(5deg)';
        });
        
        profileAvatar.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1) rotate(0deg)';
        });
    }

    // Card hover effects
    const cards = document.querySelectorAll('.profile-card, .stat-card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-5px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });

    // Info item animations
    const infoItems = document.querySelectorAll('.info-item');
    infoItems.forEach((item, index) => {
        item.style.animationDelay = `${index * 0.1}s`;
        item.classList.add('animate-fade-in');
    });
}

/* ==========================================
   Asset Cards Interactions
   ========================================== */
function initializeAssetCards() {
    const assetCards = document.querySelectorAll('.asset-card');
    
    assetCards.forEach(card => {
        // Add click handler for asset details
        card.addEventListener('click', function() {
            const assetTag = this.querySelector('.asset-tag').textContent;
            showAssetDetails(assetTag);
        });

        // Add keyboard navigation
        card.setAttribute('tabindex', '0');
        card.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });

        // Add loading state simulation
        card.addEventListener('click', function() {
            this.classList.add('loading');
            setTimeout(() => {
                this.classList.remove('loading');
            }, 1000);
        });
    });
}

function showAssetDetails(assetTag) {
    // Create modal for asset details
    const modal = document.createElement('div');
    modal.className = 'asset-details-modal';
    modal.innerHTML = `
        <div class="modal-overlay">
            <div class="modal-content">
                <div class="modal-header">
                    <h3><i class="fas fa-box"></i> تفاصيل الأصل: ${assetTag}</h3>
                    <button class="modal-close" aria-label="إغلاق">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="loading-spinner">
                        <i class="fas fa-spinner fa-spin"></i>
                        <p>جاري تحميل البيانات...</p>
                    </div>
                </div>
            </div>
        </div>
    `;

    document.body.appendChild(modal);
    
    // Add modal styles
    const modalStyles = `
        .asset-details-modal {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
            animation: fadeIn 0.3s ease;
        }
        
        .modal-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.5);
            backdrop-filter: blur(5px);
        }
        
        .modal-content {
            background: white;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            max-width: 600px;
            width: 90vw;
            max-height: 80vh;
            overflow: hidden;
            position: relative;
            z-index: 1;
            animation: slideInUp 0.3s ease;
        }
        
        .modal-header {
            padding: 1.5rem 2rem;
            border-bottom: 1px solid #e9ecef;
            display: flex;
            justify-content: space-between;
            align-items: center;
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
        }
        
        .modal-close {
            background: none;
            border: none;
            font-size: 1.2rem;
            color: #6c757d;
            cursor: pointer;
            padding: 0.5rem;
            border-radius: 50%;
            transition: all 0.3s ease;
        }
        
        .modal-close:hover {
            background: rgba(220, 53, 69, 0.1);
            color: #dc3545;
        }
        
        .modal-body {
            padding: 2rem;
            text-align: center;
        }
        
        .loading-spinner {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 1rem;
            color: #6c757d;
        }
        
        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }
        
        @keyframes slideInUp {
            from { transform: translateY(50px); opacity: 0; }
            to { transform: translateY(0); opacity: 1; }
        }
    `;

    // Add styles to document
    const styleSheet = document.createElement('style');
    styleSheet.textContent = modalStyles;
    document.head.appendChild(styleSheet);

    // Add event listeners
    modal.querySelector('.modal-close').addEventListener('click', closeModal);
    modal.querySelector('.modal-overlay').addEventListener('click', closeModal);

    // Simulate data loading
    setTimeout(() => {
        modal.querySelector('.modal-body').innerHTML = `
            <div class="asset-details">
                <div class="asset-info-grid">
                    <div class="info-row">
                        <span class="label">رمز الأصل:</span>
                        <span class="value">${assetTag}</span>
                    </div>
                    <div class="info-row">
                        <span class="label">الحالة:</span>
                        <span class="value status-active">نشط</span>
                    </div>
                    <div class="info-row">
                        <span class="label">تاريخ الاستلام:</span>
                        <span class="value">2024-01-15</span>
                    </div>
                    <div class="info-row">
                        <span class="label">القسم:</span>
                        <span class="value">قسم تقنية المعلومات</span>
                    </div>
                </div>
                <div class="asset-actions">
                    <button class="btn btn-primary">
                        <i class="fas fa-eye"></i> عرض التفاصيل الكاملة
                    </button>
                </div>
            </div>
        `;
    }, 1500);

    function closeModal() {
        modal.style.animation = 'fadeOut 0.3s ease forwards';
        setTimeout(() => {
            document.body.removeChild(modal);
            document.head.removeChild(styleSheet);
        }, 300);
    }
}

/* ==========================================
   Tooltips and Popovers
   ========================================== */
function initializeTooltips() {
    // Create tooltip element
    const tooltip = document.createElement('div');
    tooltip.className = 'custom-tooltip';
    tooltip.style.cssText = `
        position: absolute;
        background: rgba(0, 0, 0, 0.8);
        color: white;
        padding: 0.5rem 0.75rem;
        border-radius: 6px;
        font-size: 0.875rem;
        pointer-events: none;
        z-index: 9999;
        opacity: 0;
        transition: opacity 0.3s ease;
        backdrop-filter: blur(10px);
    `;
    document.body.appendChild(tooltip);

    // Add tooltips to elements with title or data-tooltip
    const tooltipElements = document.querySelectorAll('[title], [data-tooltip]');
    
    tooltipElements.forEach(element => {
        const tooltipText = element.getAttribute('data-tooltip') || element.getAttribute('title');
        element.removeAttribute('title'); // Prevent default tooltip

        element.addEventListener('mouseenter', function(e) {
            tooltip.textContent = tooltipText;
            tooltip.style.opacity = '1';
            updateTooltipPosition(e, tooltip);
        });

        element.addEventListener('mousemove', function(e) {
            updateTooltipPosition(e, tooltip);
        });

        element.addEventListener('mouseleave', function() {
            tooltip.style.opacity = '0';
        });
    });

    function updateTooltipPosition(e, tooltip) {
        const rect = tooltip.getBoundingClientRect();
        const x = e.clientX + 10;
        const y = e.clientY - rect.height - 10;

        tooltip.style.left = x + 'px';
        tooltip.style.top = y + 'px';
    }
}

/* ==========================================
   Scroll Animations
   ========================================== */
function initializeScrollAnimations() {
    const animatedElements = document.querySelectorAll('.stat-card, .profile-card, .asset-card');
    
    const scrollObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.animation = 'fadeInUp 0.6s ease forwards';
                scrollObserver.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    });

    animatedElements.forEach((element, index) => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(30px)';
        element.style.animationDelay = `${index * 0.1}s`;
        scrollObserver.observe(element);
    });
}

/* ==========================================
   Theme Toggle (Removed)
   ========================================== */
function initializeThemeToggle() {
    // Theme toggle functionality removed for cleaner profile header
    // Dark mode can be implemented elsewhere if needed
}

/* ==========================================
   Search and Filter
   ========================================== */
function initializeSearchAndFilter() {
    const assetsGrid = document.querySelector('.assets-grid');
    if (!assetsGrid) return;

    // Add search input to assets section
    const assetsHeader = document.querySelector('.profile-card-header .header-content');
    if (assetsHeader) {
        const searchContainer = document.createElement('div');
        searchContainer.className = 'search-container';
        searchContainer.innerHTML = `
            <div class="search-input-group">
                <i class="fas fa-search search-icon"></i>
                <input type="text" class="form-control search-input" placeholder="البحث في الأصول..." />
                <button class="btn btn-outline-secondary btn-sm filter-btn" data-tooltip="تصفية الأصول">
                    <i class="fas fa-filter"></i>
                </button>
            </div>
        `;

        // Add search styles
        const searchStyles = `
            .search-container {
                min-width: 300px;
            }
            
            .search-input-group {
                position: relative;
                display: flex;
                align-items: center;
                background: white;
                border: 1px solid #dee2e6;
                border-radius: 25px;
                padding: 0.5rem 1rem;
                transition: all 0.3s ease;
            }
            
            .search-input-group:focus-within {
                border-color: #007bff;
                box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
            }
            
            .search-icon {
                color: #6c757d;
                margin-right: 0.5rem;
            }
            
            .search-input {
                border: none;
                outline: none;
                flex: 1;
                background: transparent;
                font-size: 0.9rem;
            }
            
            .filter-btn {
                margin-right: 0.5rem;
                border-radius: 50%;
                width: 32px;
                height: 32px;
                display: flex;
                align-items: center;
                justify-content: center;
            }
        `;

        const styleSheet = document.createElement('style');
        styleSheet.textContent = searchStyles;
        document.head.appendChild(styleSheet);

        assetsHeader.appendChild(searchContainer);

        // Add search functionality
        const searchInput = searchContainer.querySelector('.search-input');
        const filterBtn = searchContainer.querySelector('.filter-btn');
        
        searchInput.addEventListener('input', function() {
            filterAssets(this.value);
        });

        filterBtn.addEventListener('click', function() {
            showFilterOptions();
        });
    }

    function filterAssets(searchTerm) {
        const assetCards = document.querySelectorAll('.asset-card');
        
        assetCards.forEach(card => {
            const title = card.querySelector('.asset-title').textContent.toLowerCase();
            const tag = card.querySelector('.asset-tag').textContent.toLowerCase();
            const department = card.querySelector('.asset-department').textContent.toLowerCase();
            
            const matches = title.includes(searchTerm.toLowerCase()) ||
                           tag.includes(searchTerm.toLowerCase()) ||
                           department.includes(searchTerm.toLowerCase());
            
            if (matches) {
                card.style.display = 'block';
                card.style.animation = 'fadeIn 0.3s ease';
            } else {
                card.style.display = 'none';
            }
        });

        // Show no results message if needed
        const visibleCards = document.querySelectorAll('.asset-card[style*="display: block"], .asset-card:not([style*="display: none"])');
        if (visibleCards.length === 0 && searchTerm) {
            showNoResultsMessage();
        } else {
            hideNoResultsMessage();
        }
    }

    function showNoResultsMessage() {
        let noResults = document.querySelector('.no-results-message');
        if (!noResults) {
            noResults = document.createElement('div');
            noResults.className = 'no-results-message';
            noResults.innerHTML = `
                <div class="text-center py-4">
                    <i class="fas fa-search fa-3x text-muted mb-3"></i>
                    <h4 class="text-muted">لم يتم العثور على نتائج</h4>
                    <p class="text-muted">جرب تعديل مصطلح البحث أو استخدم مرشحات مختلفة</p>
                </div>
            `;
            assetsGrid.appendChild(noResults);
        }
    }

    function hideNoResultsMessage() {
        const noResults = document.querySelector('.no-results-message');
        if (noResults) {
            noResults.remove();
        }
    }

    function showFilterOptions() {
        console.log('Show filter options - implementation needed');
    }
}

/* ==========================================
   Profile Actions
   ========================================== */
function initializeProfileActions() {
    // Add loading states to action buttons
    const actionButtons = document.querySelectorAll('.btn-action');
    
    actionButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (!this.classList.contains('theme-toggle')) {
                e.preventDefault();
                showLoadingState(this);
                
                // Simulate action completion
                setTimeout(() => {
                    hideLoadingState(this);
                    // Redirect to actual page
                    window.location.href = this.getAttribute('href');
                }, 1000);
            }
        });
    });

    function showLoadingState(button) {
        const originalContent = button.innerHTML;
        button.setAttribute('data-original-content', originalContent);
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i><span>جاري التحميل...</span>';
        button.disabled = true;
    }

    function hideLoadingState(button) {
        const originalContent = button.getAttribute('data-original-content');
        button.innerHTML = originalContent;
        button.disabled = false;
        button.removeAttribute('data-original-content');
    }
}

/* ==========================================
   Accessibility Enhancements
   ========================================== */
function initializeAccessibility() {
    // Add keyboard navigation for cards
    const cards = document.querySelectorAll('.stat-card, .asset-card, .info-item');
    
    cards.forEach(card => {
        card.setAttribute('tabindex', '0');
        card.setAttribute('role', 'button');
        
        card.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    });

    // Add ARIA labels
    const counters = document.querySelectorAll('.counter');
    counters.forEach(counter => {
        const value = counter.textContent;
        const label = counter.closest('.stat-card')?.querySelector('.stat-label')?.textContent;
        counter.setAttribute('aria-label', `${label}: ${value}`);
    });

    // Announce dynamic content changes
    const announcer = document.createElement('div');
    announcer.setAttribute('aria-live', 'polite');
    announcer.setAttribute('aria-atomic', 'true');
    announcer.style.cssText = 'position: absolute; left: -10000px; width: 1px; height: 1px; overflow: hidden;';
    document.body.appendChild(announcer);

    window.announceToScreenReader = function(message) {
        announcer.textContent = message;
    };
}

// Export functions for global access
window.UserProfile = {
    showAssetDetails,
    toggleTheme: () => document.querySelector('.theme-toggle')?.click()
};