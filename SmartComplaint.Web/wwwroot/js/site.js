// ── Toastr Global Config ──────────────────────────
if (typeof toastr !== 'undefined') {
    toastr.options = {
        positionClass: "toast-top-right",
        timeOut: 3500,
        closeButton: true,
        progressBar: true,
        preventDuplicates: true,
        newestOnTop: true,
    };
}

// ── Document Ready ────────────────────────────────
$(document).ready(function () {
    // ── Dark Mode ─────────────────────────────────────
    initDarkMode();

    $('#darkToggle').on('click', function () {
        toggleDarkMode();
    });

    // Sidebar Toggle
    $('#sidebarToggle').on('click', function () {
        if ($(window).width() <= 768) {
            $('#sidebar').toggleClass('open');
            $('#sidebarOverlay').toggleClass('show');
        } else {
            $('#sidebar').toggleClass('collapsed');
            $('#mainContent').toggleClass('expanded');
        }
    });

    // Sidebar overlay click — close on mobile
    $('#sidebarOverlay').on('click', function () {
        $('#sidebar').removeClass('open');
        $(this).removeClass('show');
    });

    // Load notification count
    loadNotifCount();

    // Auto dismiss alerts after 4 seconds
    setTimeout(function () {
        $('.alert-dismissible').fadeOut(500, function () {
            $(this).remove();
        });
    }, 4000);

    // Loading overlay on form submit
    $('form').on('submit', function () {
        const btn = $(this).find('[type=submit]');
        if (btn.length && !$(this).hasClass('no-loading')) {
            showLoading();
        }
    });

    // Highlight active sidebar link
    highlightActiveNav();
});

// ── Notification Badge ────────────────────────────
function loadNotifCount() {
    $.get('/Notifications/UnreadCount')
        .done(function (data) {
            if (data.count > 0) {
                $('#notifBadge').removeClass('d-none').text(data.count);
            } else {
                $('#notifBadge').addClass('d-none');
            }
        })
        .fail(function () {
            $('#notifBadge').addClass('d-none');
        });
}

// ── Loading Overlay ───────────────────────────────
function showLoading(msg) {
    $('#loadingText').text(msg || 'Please wait...');
    $('#loadingOverlay').addClass('show');
}

function hideLoading() {
    $('#loadingOverlay').removeClass('show');
}

// ── SweetAlert Confirm Delete ─────────────────────
function confirmDelete(id, url, name) {
    Swal.fire({
        title: 'Delete ' + (name || 'this item') + '?',
        text: 'This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#EF4444',
        cancelButtonColor: '#6B7280',
        confirmButtonText: 'Yes, delete!',
        cancelButtonText: 'Cancel',
        borderRadius: '12px',
    }).then((result) => {
        if (result.isConfirmed) {
            showLoading('Deleting...');
            window.location.href = url;
        }
    });
}

// ── SweetAlert Confirm Action ─────────────────────
function confirmAction(title, text, url, btnText, btnColor) {
    Swal.fire({
        title: title,
        text: text,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: btnColor || '#4F46E5',
        cancelButtonColor: '#6B7280',
        confirmButtonText: btnText || 'Confirm',
    }).then((result) => {
        if (result.isConfirmed) {
            showLoading();
            window.location.href = url;
        }
    });
}

// ── Active Nav Highlight ──────────────────────────
function highlightActiveNav() {
    const path = window.location.pathname.toLowerCase();
    $('.sidebar-nav .nav-link').each(function () {
        const href = $(this).attr('href')?.toLowerCase();
        if (href && path.startsWith(href) && href !== '/') {
            $(this).addClass('active');
        }
    });
}

// ── Status Badge HTML ─────────────────────────────
function statusBadge(status) {
    const map = {
        'Open': 'badge-open',
        'InProgress': 'badge-inprogress',
        'OnHold': 'badge-onhold',
        'Resolved': 'badge-resolved',
        'Closed': 'badge-closed',
    };
    return `<span class="badge ${map[status] || 'bg-secondary'}">${status}</span>`;
}

// ── Priority Badge HTML ───────────────────────────
function priorityBadge(priority) {
    const map = {
        'High': 'badge-high',
        'Medium': 'badge-medium',
        'Low': 'badge-low',
    };
    return `<span class="badge ${map[priority] || 'bg-secondary'}">${priority}</span>`;
}

// ── Toast Helpers ─────────────────────────────────
function showSuccess(msg) { toastr.success(msg); }
function showError(msg) { toastr.error(msg); }
function showInfo(msg) { toastr.info(msg); }
function showWarning(msg) { toastr.warning(msg); }

// ── Session Expired Handler ───────────────────────
$(document).ajaxError(function (event, jqxhr) {
    if (jqxhr.status === 401) {
        Swal.fire({
            title: 'Session Expired',
            text: 'Please login again to continue.',
            icon: 'warning',
            confirmButtonColor: '#4F46E5',
            confirmButtonText: 'Login',
        }).then(() => {
            window.location.href = '/Auth/Login';
        });
    }
});


// ── Dark Mode Functions ───────────────────────────
function initDarkMode() {
    const saved = localStorage.getItem('theme') || 'light';
    applyTheme(saved);
}

function toggleDarkMode() {
    const current = document.documentElement.getAttribute('data-theme');
    const next = current === 'dark' ? 'light' : 'dark';
    applyTheme(next);
    localStorage.setItem('theme', next);
}

function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    const icon = document.getElementById('darkIcon');
    if (!icon) return;

    if (theme === 'dark') {
        icon.textContent = '☀️';
        document.getElementById('darkToggle').title = 'Switch to Light Mode';
    } else {
        icon.textContent = '🌙';
        document.getElementById('darkToggle').title = 'Switch to Dark Mode';
    }
}