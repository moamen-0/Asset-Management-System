/**
 * Professional Location Statistics Enhancement
 * Handles animations, interactions, and data visualization for location statistics cards
 */

class LocationStatistics {
    constructor() {
        this.cards = document.querySelectorAll('.stat-card-professional');
        this.isAnimated = false;
        this.observerOptions = {
            threshold: 0.3,
            rootMargin: '0px 0px -100px 0px'
        };
        
        this.init();
    }

    init() {
        this.setupIntersectionObserver();
        this.setupCardInteractions();
        this.setupTooltips();
        this.setupCounterAnimations();
        this.setupProgressBars();
        this.addAccessibilityFeatures();
    }

    setupIntersectionObserver() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && !this.isAnimated) {
                    this.animateCards();
                    this.isAnimated = true;
                }
            });
        }, this.observerOptions);

        const statsContainer = document.querySelector('.location-stats-container');
        if (statsContainer) {
            observer.observe(statsContainer);
        }
    }

    setupCardInteractions() {
        this.cards.forEach((card, index) => {
            // Add ripple effect on click
            card.addEventListener('click', (e) => {
                this.createRippleEffect(e, card);
                this.showCardDetails(card);
            });

            // Add keyboard navigation
            card.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    card.click();
                }
            });

            // Add focus styles
            card.addEventListener('focus', () => {
                card.style.outline = '3px solid rgba(59, 130, 246, 0.5)';
                card.style.outlineOffset = '2px';
            });

            card.addEventListener('blur', () => {
                card.style.outline = 'none';
            });

            // Staggered animation on load
            setTimeout(() => {
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, index * 150);
        });
    }

    createRippleEffect(event, element) {
        const rect = element.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = event.clientX - rect.left - size / 2;
        const y = event.clientY - rect.top - size / 2;

        const ripple = document.createElement('div');
        ripple.style.cssText = `
            position: absolute;
            left: ${x}px;
            top: ${y}px;
            width: ${size}px;
            height: ${size}px;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.6);
            transform: scale(0);
            animation: ripple 0.6s ease-out;
            pointer-events: none;
            z-index: 1;
        `;

        element.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 600);
    }

    setupTooltips() {
        this.cards.forEach(card => {
            const cardType = this.getCardType(card);
            const tooltipText = this.getTooltipText(cardType);
            
            const tooltip = document.createElement('div');
            tooltip.className = 'stat-tooltip';
            tooltip.textContent = tooltipText;
            tooltip.setAttribute('role', 'tooltip');
            
            card.appendChild(tooltip);
        });
    }

    getCardType(card) {
        if (card.classList.contains('facilities')) return 'facilities';
        if (card.classList.contains('buildings')) return 'buildings';
        if (card.classList.contains('rooms')) return 'rooms';
        if (card.classList.contains('departments')) return 'departments';
        return 'general';
    }

    getTooltipText(type) {
        const tooltips = {
            facilities: 'عدد المنشآت المسجلة في النظام',
            buildings: 'إجمالي المباني في جميع المنشآت',
            rooms: 'العدد الكلي للغرف المتاحة',
            departments: 'الأقسام الموزعة على المنشآت',
            general: 'إحصائية عامة'
        };
        return tooltips[type] || tooltips.general;
    }

    setupCounterAnimations() {
        this.cards.forEach(card => {
            const numberElement = card.querySelector('.stat-number-professional');
            if (numberElement) {
                const finalValue = parseInt(numberElement.textContent) || 0;
                this.animateCounter(numberElement, 0, finalValue, 2000);
            }
        });
    }

    animateCounter(element, start, end, duration) {
        const startTime = performance.now();
        const range = end - start;

        const step = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            
            // Easing function for smooth animation
            const easeOutQuart = 1 - Math.pow(1 - progress, 4);
            const current = start + (range * easeOutQuart);
            
            element.textContent = Math.floor(current).toLocaleString('ar-SA');
            
            if (progress < 1) {
                requestAnimationFrame(step);
            } else {
                element.textContent = end.toLocaleString('ar-SA');
            }
        };

        requestAnimationFrame(step);
    }

    setupProgressBars() {
        this.cards.forEach(card => {
            const progressContainer = document.createElement('div');
            progressContainer.className = 'stat-progress-container';
            
            const progressBar = document.createElement('div');
            progressBar.className = 'stat-progress';
            
            const progressFill = document.createElement('div');
            progressFill.className = 'stat-progress-bar';
            
            // Calculate progress based on card type and value
            const progress = this.calculateProgress(card);
            progressFill.style.setProperty('--progress-width', `${progress}%`);
            
            progressBar.appendChild(progressFill);
            progressContainer.appendChild(progressBar);
            
            const contentWrapper = card.querySelector('.stat-content-wrapper');
            if (contentWrapper) {
                contentWrapper.appendChild(progressContainer);
            }
        });
    }

    calculateProgress(card) {
        const numberElement = card.querySelector('.stat-number-professional');
        const value = parseInt(numberElement?.textContent) || 0;
        
        // Simple progress calculation - can be enhanced based on business logic
        const maxValues = {
            facilities: 20,
            buildings: 100,
            rooms: 500,
            departments: 50
        };
        
        const cardType = this.getCardType(card);
        const maxValue = maxValues[cardType] || 100;
        
        return Math.min((value / maxValue) * 100, 100);
    }

    animateCards() {
        this.cards.forEach((card, index) => {
            setTimeout(() => {
                card.style.animation = 'slideInUp 0.6s ease-out forwards';
                
                // Trigger counter animation
                const numberElement = card.querySelector('.stat-number-professional');
                if (numberElement) {
                    const finalValue = parseInt(numberElement.dataset.value || numberElement.textContent) || 0;
                    numberElement.textContent = '0';
                    this.animateCounter(numberElement, 0, finalValue, 1500 + (index * 200));
                }
            }, index * 100);
        });
    }

    showCardDetails(card) {
        const cardType = this.getCardType(card);
        const value = card.querySelector('.stat-number-professional')?.textContent || '0';
        
        // Create a simple notification or modal - can be enhanced
        this.showNotification(`${this.getCardTitle(cardType)}: ${value}`, cardType);
    }

    getCardTitle(type) {
        const titles = {
            facilities: 'المنشآت',
            buildings: 'المباني',
            rooms: 'الغرف',
            departments: 'الأقسام'
        };
        return titles[type] || 'إحصائية';
    }

    showNotification(message, type) {
        // Remove existing notifications
        const existingNotifications = document.querySelectorAll('.stat-notification');
        existingNotifications.forEach(notification => notification.remove());

        const notification = document.createElement('div');
        notification.className = `stat-notification ${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas fa-info-circle"></i>
                <span>${message}</span>
            </div>
        `;
        
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: var(--card-gradient);
            color: white;
            padding: 1rem 1.5rem;
            border-radius: 12px;
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
            z-index: 9999;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            font-weight: 500;
        `;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Remove after 3 seconds
        setTimeout(() => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    addAccessibilityFeatures() {
        this.cards.forEach(card => {
            // Add ARIA labels
            const cardType = this.getCardType(card);
            const value = card.querySelector('.stat-number-professional')?.textContent || '0';
            const title = this.getCardTitle(cardType);
            
            card.setAttribute('role', 'button');
            card.setAttribute('tabindex', '0');
            card.setAttribute('aria-label', `${title}: ${value}. اضغط للمزيد من التفاصيل`);
            
            // Add live region for screen readers
            const liveRegion = document.createElement('div');
            liveRegion.setAttribute('aria-live', 'polite');
            liveRegion.setAttribute('aria-atomic', 'true');
            liveRegion.className = 'sr-only';
            card.appendChild(liveRegion);
        });
    }

    // Public method to update card values
    updateCardValue(cardType, newValue) {
        const card = document.querySelector(`.stat-card-professional.${cardType}`);
        if (card) {
            const numberElement = card.querySelector('.stat-number-professional');
            if (numberElement) {
                const oldValue = parseInt(numberElement.textContent) || 0;
                this.animateCounter(numberElement, oldValue, newValue, 1000);
                
                // Update progress bar
                const progressBar = card.querySelector('.stat-progress-bar');
                if (progressBar) {
                    const progress = this.calculateProgress(card);
                    progressBar.style.setProperty('--progress-width', `${progress}%`);
                }
            }
        }
    }

    // Public method to refresh all statistics
    refreshStatistics() {
        this.cards.forEach(card => {
            card.classList.add('stat-card-loading');
        });

        // Simulate API call - replace with actual data fetching
        setTimeout(() => {
            this.cards.forEach(card => {
                card.classList.remove('stat-card-loading');
            });
            this.animateCards();
        }, 1000);
    }

    // Public method for action bar refresh
    refreshStats() {
        this.refreshStatistics();
    }
}

/**
 * Departments Overview Enhancement
 * Handles interactions for department badges and facility items
 */
class DepartmentsOverview {
    constructor() {
        this.facilityItems = document.querySelectorAll('.facility-item-professional');
        this.departmentBadges = document.querySelectorAll('.department-badge-professional');
        
        this.init();
    }

    init() {
        this.setupFacilityInteractions();
        this.setupDepartmentBadgeInteractions();
        this.setupAccessibility();
        this.animateOnLoad();
    }

    setupFacilityInteractions() {
        this.facilityItems.forEach(item => {
            item.addEventListener('mouseenter', () => {
                this.highlightFacilityBadges(item);
            });

            item.addEventListener('mouseleave', () => {
                this.resetBadgeHighlight(item);
            });
        });
    }

    setupDepartmentBadgeInteractions() {
        this.departmentBadges.forEach(badge => {
            badge.addEventListener('click', (e) => {
                this.handleBadgeClick(e, badge);
            });

            badge.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.handleBadgeClick(e, badge);
                }
            });

            // Add hover effect with ripple
            badge.addEventListener('mousedown', (e) => {
                this.createBadgeRipple(e, badge);
            });
        });
    }

    handleBadgeClick(event, badge) {
        const departmentName = badge.textContent.trim();
        
        if (badge.classList.contains('more')) {
            this.showAllDepartments(badge);
        } else {
            this.showDepartmentDetails(departmentName);
        }
    }

    createBadgeRipple(event, badge) {
        const rect = badge.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = event.clientX - rect.left - size / 2;
        const y = event.clientY - rect.top - size / 2;

        const ripple = document.createElement('div');
        ripple.style.cssText = `
            position: absolute;
            left: ${x}px;
            top: ${y}px;
            width: ${size}px;
            height: ${size}px;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.6);
            transform: scale(0);
            animation: badgeRipple 0.6s ease-out;
            pointer-events: none;
            z-index: 1;
        `;

        badge.style.position = 'relative';
        badge.style.overflow = 'hidden';
        badge.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 600);
    }

    highlightFacilityBadges(facilityItem) {
        const badges = facilityItem.querySelectorAll('.department-badge-professional');
        badges.forEach((badge, index) => {
            setTimeout(() => {
                badge.style.transform = 'scale(1.05) translateY(-2px)';
                badge.style.boxShadow = '0 6px 20px rgba(102, 126, 234, 0.4)';
            }, index * 50);
        });
    }

    resetBadgeHighlight(facilityItem) {
        const badges = facilityItem.querySelectorAll('.department-badge-professional');
        badges.forEach(badge => {
            badge.style.transform = '';
            badge.style.boxShadow = '';
        });
    }

    showDepartmentDetails(departmentName) {
        // Create and show department details modal or notification
        this.showDepartmentNotification(departmentName);
    }

    showAllDepartments(moreButton) {
        const facilityItem = moreButton.closest('.facility-item-professional');
        const facilityTitle = facilityItem.querySelector('.facility-title-professional').textContent.trim();
        
        this.showFacilityNotification(facilityTitle);
    }

    showDepartmentNotification(departmentName) {
        this.createNotification(
            `قسم: ${departmentName}`,
            'تم تحديد قسم ' + departmentName,
            'info'
        );
    }

    showFacilityNotification(facilityName) {
        this.createNotification(
            `منشأة: ${facilityName}`,
            'عرض جميع أقسام ' + facilityName,
            'info'
        );
    }

    createNotification(title, message, type) {
        // Remove existing notifications
        const existingNotifications = document.querySelectorAll('.department-notification');
        existingNotifications.forEach(notification => notification.remove());

        const notification = document.createElement('div');
        notification.className = `department-notification ${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <div class="notification-header">
                    <i class="fas fa-info-circle"></i>
                    <strong>${title}</strong>
                </div>
                <div class="notification-body">${message}</div>
            </div>
        `;
        
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 1rem 1.5rem;
            border-radius: 12px;
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
            z-index: 9999;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            font-weight: 500;
            max-width: 350px;
        `;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Remove after 3 seconds
        setTimeout(() => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    setupAccessibility() {
        this.departmentBadges.forEach(badge => {
            if (!badge.getAttribute('role')) {
                badge.setAttribute('role', 'button');
            }
            if (!badge.getAttribute('tabindex')) {
                badge.setAttribute('tabindex', '0');
            }
        });
    }

    animateOnLoad() {
        this.facilityItems.forEach((item, index) => {
            item.style.opacity = '0';
            item.style.transform = 'translateY(20px)';
            
            setTimeout(() => {
                item.style.transition = 'all 0.6s ease-out';
                item.style.opacity = '1';
                item.style.transform = 'translateY(0)';
            }, index * 150);
        });
    }
}

// Action Bar Manager Class


// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInUp {
        from {
            opacity: 0;
            transform: translateY(30px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    @keyframes ripple {
        to {
            transform: scale(2);
            opacity: 0;
        }
    }

    @keyframes badgeRipple {
        to {
            transform: scale(2);
            opacity: 0;
        }
    }

    .sr-only {
        position: absolute;
        width: 1px;
        height: 1px;
        padding: 0;
        margin: -1px;
        overflow: hidden;
        clip: rect(0, 0, 0, 0);
        white-space: nowrap;
        border: 0;
    }

    .stat-card-professional {
        opacity: 0;
        transform: translateY(30px);
        transition: all 0.3s ease;
    }

    .notification-content {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .notification-header {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        font-weight: 600;
    }

    .notification-body {
        font-size: 0.9rem;
        opacity: 0.9;
    }
`;

document.head.appendChild(style);

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.locationStats = new LocationStatistics();
    
    // Initialize departments overview
    if (document.querySelector('.departments-overview-professional')) {
        window.departmentsOverview = new DepartmentsOverview();
    }
    
    // Initialize action bar manager
    window.actionBarManager = new ActionBarManager();
});

// Export for global access
window.LocationStatistics = LocationStatistics;
window.DepartmentsOverview = DepartmentsOverview;
window.ActionBarManager = ActionBarManager;
