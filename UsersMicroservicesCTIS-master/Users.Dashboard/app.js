const API_BASE = 'https://localhost:7219/api';
const MOVIES_BASE = 'https://localhost:7134/api';

// State Management
let currentState = {
    users: [],
    roles: [],
    groups: [],
    movies: [],
    directors: [],
    genres: [],
    currentView: 'users'
};

let jwtToken = localStorage.getItem('jwtToken') || null;
let refreshToken = localStorage.getItem('refreshToken') || null;

// DOM Elements
const mainDisplay = document.getElementById('main-display');
const navItems = document.querySelectorAll('.nav-item');
const globalSearch = document.getElementById('global-search');
const addItemBtn = document.getElementById('add-item-btn');
const modalContainer = document.getElementById('modal-container');
const loginBtn = document.getElementById('login-btn');
const registerBtn = document.getElementById('register-btn');

// Initial Load
document.addEventListener('DOMContentLoaded', () => {
    init();
});

async function init() {
    setupEventListeners();
    updateAuthUI();
    await loadData('users');
}

function updateAuthUI() {
    if (jwtToken) {
        loginBtn.textContent = 'Logout';
        document.getElementById('user-profile').style.display = 'block';
        registerBtn.style.display = 'none'; // hide Register when logged in
    } else {
        loginBtn.textContent = 'Login';
        document.getElementById('user-profile').style.display = 'none';
        registerBtn.style.display = '';
    }
}

function setTokens(token, refresh) {
    jwtToken = token;
    refreshToken = refresh;
    if (token) {
        localStorage.setItem('jwtToken', token);
        localStorage.setItem('refreshToken', refresh);
    } else {
        localStorage.removeItem('jwtToken');
        localStorage.removeItem('refreshToken');
    }
    updateAuthUI();
}

// Custom API fetcher to handle tokens
async function apiCall(url, options = {}) {
    if (!options.headers) options.headers = {};
    if (!options.headers['Content-Type'] && !(options.body instanceof FormData)) {
        options.headers['Content-Type'] = 'application/json';
    }
    if (jwtToken && jwtToken !== 'null') {
        let cleanToken = jwtToken.replace(/^(Bearer\s+)+/i, '').trim();
        options.headers['Authorization'] = `Bearer ${cleanToken}`;
    }
    
    let response = await fetch(url, options);
    
    // Auto refresh logic based on 401
    if (response.status === 401 && refreshToken) {
    try {
        let cleanToken = jwtToken ? jwtToken.replace(/^(Bearer\s+)+/i, '').trim() : '';
        const refreshRes = await fetch(`${API_BASE}/RefreshToken`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ token: cleanToken, refreshToken: refreshToken })
        });
            if (refreshRes.ok) {
                const data = await refreshRes.json();
                let cleanToken = data.token.replace(/^(Bearer\s+)+/i, '').trim();
                options.headers['Authorization'] = `Bearer ${cleanToken}`;
                response = await fetch(url, options); // retry
            } else {
                setTokens(null, null);
            }
        } catch(e) {
            setTokens(null, null);
        }
    }
    return response;
}

// UI Helpers
function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    container.appendChild(toast);
    
    setTimeout(() => {
        toast.style.animation = 'fadeOut 0.3s ease forwards';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

function setupEventListeners() {
    navItems.forEach(item => {
        item.addEventListener('click', async (e) => {
            e.preventDefault();
            const view = item.getAttribute('data-view');
            
            navItems.forEach(i => i.classList.remove('active'));
            item.classList.add('active');
            
            currentState.currentView = view;
            updateUIHeader();
            await loadData(view);
        });
    });

    globalSearch.addEventListener('input', (e) => {
        handleSearch(e.target.value);
    });

    addItemBtn.addEventListener('click', () => {
        openModal(currentState.currentView);
    });

    loginBtn.addEventListener('click', () => {
        if (jwtToken) {
            setTokens(null, null);
            showToast('Logged out successfully', 'success');
            loadData(currentState.currentView);
        } else {
            showLoginModal();
        }
    });

    registerBtn.addEventListener('click', () => {
        showRegisterModal();
    });

    modalContainer.addEventListener('click', (e) => {
        // if (e.target === modalContainer) closeModal(); // Disabled by user request
    });
}

function updateUIHeader() {
    const btn = document.getElementById('add-item-btn');
    const view = currentState.currentView;
    let viewName = "";
    
    if (view === 'movies') viewName = "Movie";
    else if (view === 'directors') viewName = "Director";
    else if (view === 'genres') viewName = "Genre";
    else viewName = view.charAt(0).toUpperCase() + view.slice(1, -1);
    
    btn.textContent = `+ New ${viewName}`;
}

async function loadData(type) {
    showLoading();
    let url = ""; 
    try {
        const capitalizedType = type.charAt(0).toUpperCase() + type.slice(1);
        url = `${API_BASE}/${capitalizedType}`;
        const moviesTypes = ['movies', 'directors', 'genres'];
        if (moviesTypes.some(t => type.startsWith(t))) {
            url = `${MOVIES_BASE}/${capitalizedType}`;
        }

        const response = await apiCall(url);
        
        if (response.status === 401) {
            showError("Authentication required. Please login.");
            return;
        }
        if (response.status === 403) {
            showError("Access Denied: You do not have permission to view this.");
            return;
        }
        
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        
        if (response.status === 204) {
            handleViewData(type, []);
            return;
        }

        const data = await response.json();
        handleViewData(type, data);
    } catch (error) {
        console.error('Fetch Error:', error);
        showError(`Failed to connect to ${type} microservice. Ensure API is running at ${url}`);
    }
}

function handleViewData(type, data) {
    const cleanType = type.split('?')[0];
    if (cleanType === 'users') {
        currentState.users = data;
        renderUsers(data);
        updateStats(data);
    } else if (type === 'roles') {
        currentState.roles = data;
        renderGeneric(data, '🔑 Role', 'roles');
    } else if (type === 'groups') {
        currentState.groups = data;
        renderGeneric(data, '📁 Group', 'groups');
    } else if (type === 'directors') {
        currentState.directors = data;
        renderGeneric(data, '📽️ Director', 'directors');
    } else if (type === 'genres') {
        currentState.genres = data;
        renderGeneric(data, '🏷️ Genre', 'genres');
    } else if (type === 'movies') {
        currentState.movies = data;
        renderMovies(data);
    }
}

function getActionButtonsHTML(id, type) {
    return `
        <div class="card-actions">
            <button class="btn-icon edit" onclick="editItem('${type}', '${id}')" title="Edit">✎</button>
            <button class="btn-icon delete" onclick="deleteItem('${type}', '${id}')" title="Delete">🗑</button>
        </div>
    `;
}

function renderUsers(users) {
    mainDisplay.innerHTML = '';
    
    if (!users || users.length === 0) {
        mainDisplay.innerHTML = '<div class="no-data">No users found.</div>';
        return;
    }

    users.forEach(user => {
        const card = document.createElement('div');
        card.className = 'user-card';
        card.innerHTML = `
            <span class="status-badge ${user.isActive ? 'status-active' : 'status-inactive'}">
                ${user.isActive ? 'ACTIVE' : 'INACTIVE'}
            </span>
            <div class="card-header" style="padding-right: 90px;">
                <img src="https://ui-avatars.com/api/?name=${user.firstName || 'User'}+${user.lastName || ''}&background=random" class="avatar-large" alt="Avatar">
                <div class="header-text">
                    <h3 class="user-fullname">${user.firstName ?? ''} ${user.lastName ?? ''}</h3>
                    <p class="user-handle">@${user.userName}</p>
                </div>
            </div>
            <div class="card-body">
                <div class="info-item">
                    <p class="info-label">Join Date</p>
                    <p class="info-value">${user.registrationDate && user.registrationDate !== '0001-01-01T00:00:00' ? new Date(user.registrationDate).toLocaleDateString() : 'N/A'}</p>
                </div>
                <div class="info-item">
                    <p class="info-label">Group</p>
                    <p class="info-value">${user.groupF || 'None'}</p>
                </div>
            </div>
            <div class="card-footer" style="display: flex; justify-content: space-between; align-items: center;">
                ${getActionButtonsHTML(user.id, 'users')}
            </div>
        `;
        mainDisplay.appendChild(card);
    });
}

function renderGeneric(items, prefix, type) {
    mainDisplay.innerHTML = '';
    
    if (!items || items.length === 0) {
        mainDisplay.innerHTML = `<div class="no-data">No ${type} found.</div>`;
        return;
    }

    let icon = "❓";
    let bg = "linear-gradient(135deg, #64748b, #334155)";
    if (type === 'directors') { icon = "🎭"; bg = "linear-gradient(135deg, #a855f7, #3b82f6)"; }
    if (type === 'genres') { icon = "🏷️"; bg = "linear-gradient(135deg, #34d399, #059669)"; }
    if (type === 'groups') { icon = "👥"; bg = "linear-gradient(135deg, #fbbf24, #d97706)"; }
    if (type === 'roles') { icon = "🔑"; bg = "linear-gradient(135deg, #f87171, #dc2626)"; }

    items.forEach(item => {
        const card = document.createElement('div');
        card.className = 'user-card'; 
        const name = item.fullName || item.name || item.title || 'Unnamed';
        card.innerHTML = `
            <div class="card-header">
                <div class="logo-icon" style="background: ${bg}; color: white; display: flex; align-items: center; justify-content: center; width: 48px; height: 48px; border-radius: 12px; font-size: 1.5rem; box-shadow: 0 4px 10px rgba(0,0,0,0.2); flex-shrink: 0;">${icon}</div>
                <div class="header-text">
                    <h3 class="user-fullname">${name}</h3>
                    <p class="user-handle">ID: ${item.id}</p>
                </div>
            </div>
            <div class="card-footer" style="display: flex; justify-content: space-between; align-items: center;">
                ${getActionButtonsHTML(item.id, type)}
            </div>
        `;
        mainDisplay.appendChild(card);
    });
}

function updateStats(users) {
    document.getElementById('stat-total-users').textContent = users.length;
    document.getElementById('stat-active-users').textContent = users.filter(u => u.isActive).length;
    
    document.getElementById('stat-avg-score').textContent = users.filter(u => !u.isActive).length;
    const avgScoreLabel = document.querySelector('#stat-avg-score').previousElementSibling;
    if (avgScoreLabel) avgScoreLabel.textContent = 'Inactive Users';
}

function handleSearch(query) {
    const q = query.toLowerCase();
    
    if (currentState.currentView === 'movies') {
        const filtered = currentState.movies.filter(m => 
            m.name.toLowerCase().includes(q) || 
            (m.directorF && m.directorF.toLowerCase().includes(q))
        );
        renderMovies(filtered);
        return;
    }

    if (currentState.currentView === 'directors') {
        const filtered = currentState.directors.filter(d => 
            (d.fullName || d.firstName + ' ' + d.lastName).toLowerCase().includes(q)
        );
        renderGeneric(filtered, '📽️ Director', 'directors');
        return;
    }

    if (currentState.currentView === 'genres') {
        const filtered = currentState.genres.filter(g => 
            g.name.toLowerCase().includes(q)
        );
        renderGeneric(filtered, '🏷️ Genre', 'genres');
        return;
    }

    if (currentState.currentView === 'groups') {
        const filtered = currentState.groups.filter(g => 
            g.title.toLowerCase().includes(q)
        );
        renderGeneric(filtered, '📁 Group', 'groups');
        return;
    }

    const filtered = currentState.users.filter(u => 
        u.userName.toLowerCase().includes(q) ||
        (u.firstName && u.firstName.toLowerCase().includes(q)) ||
        (u.lastName && u.lastName.toLowerCase().includes(q))
    );
    renderUsers(filtered);
}

function showLoading() {
    mainDisplay.innerHTML = `
        <div class="loading-overlay">
            <div class="spinner"></div>
            <p>Syncing with cloud microservice...</p>
        </div>
    `;
}

function showError(message) {
    mainDisplay.innerHTML = `
        <div class="loading-overlay" style="color: var(--danger)">
            <span style="font-size: 3rem">⚠️</span>
            <p>${message}</p>
            <button class="btn btn-primary" onclick="loadData(currentState.currentView)">Retry Connection</button>
        </div>
    `;
}

function renderMovies(movies) {
    mainDisplay.innerHTML = '';
    
    if (!movies || movies.length === 0) {
        mainDisplay.innerHTML = '<div class="no-data">No movies found. Add them via Swagger or Add Button.</div>';
        return;
    }

    movies.forEach(movie => {
        const card = document.createElement('div');
        card.className = 'user-card'; 
        
        card.innerHTML = `
            <span class="status-badge status-active" style="background: rgba(34, 211, 238, 0.1); color: var(--accent)">
                🎬 MOVIE
            </span>
            <div class="card-header" style="padding-right: 90px;">
                <div class="logo-icon" style="background: linear-gradient(135deg, #f59e0b, #ef4444); color: white; display: flex; align-items: center; justify-content: center; width: 48px; height: 48px; border-radius: 12px; font-size: 1.5rem; flex-shrink: 0;">🎬</div>
                <div class="header-text">
                    <h3 class="user-fullname">${movie.name}</h3>
                    <p class="user-handle">${movie.releaseDate ? new Date(movie.releaseDate).getFullYear() : 'N/A'}</p>
                </div>
            </div>
            <div class="card-body">
                <div class="info-item">
                    <p class="info-label">Director</p>
                    <p class="info-value">${movie.directorF || 'Unknown'}</p>
                </div>
                <div class="info-item">
                    <p class="info-label">Genres</p>
                    <p class="info-value" style="font-size: 0.85rem">${(movie.genresF || []).join(', ') || 'No Genre'}</p>
                </div>
                <div class="info-item">
                    <p class="info-label">Revenue</p>
                    <p class="info-value" style="color: #10b981">${movie.totalRevenueF || '$0'}</p>
                </div>
            </div>
            <div class="card-footer" style="display: flex; justify-content: space-between; align-items: center;">
                ${getActionButtonsHTML(movie.id, 'movies')}
            </div>
        `;
        mainDisplay.appendChild(card);
    });
}

// Modal and Form Logic
async function showRegisterModal() {
    let roles = [];
    try {
        const res = await fetch(`${API_BASE}/Roles`);
        if (res.ok) roles = await res.json();
    } catch(e) {
        showToast('Warning: Could not fetch roles for selection', 'error');
    }

    const roleOptions = roles.map(r => `<option value="${r.id}">${r.name}</option>`).join('');

    const html = `
        <div class="form-group">
            <label class="form-label">Username <span style="color:var(--danger)">*</span></label>
            <input type="text" name="userName" class="form-input" required placeholder="Choose a username" maxlength="30">
        </div>
        <div class="form-group">
            <label class="form-label">Password <span style="color:var(--danger)">*</span></label>
            <input type="password" name="password" class="form-input" required placeholder="Choose a password" maxlength="15">
        </div>
        <div class="form-group">
            <label class="form-label">First Name</label>
            <input type="text" name="firstName" class="form-input" placeholder="Optional" maxlength="50">
        </div>
        <div class="form-group">
            <label class="form-label">Last Name</label>
            <input type="text" name="lastName" class="form-input" placeholder="Optional" maxlength="50">
        </div>
        <div class="form-group">
            <label class="form-label">Select Role <span style="color:var(--danger)">*</span></label>
            <select name="roleId" class="form-select" required>
                <option value="">Select a role...</option>
                ${roleOptions}
            </select>
        </div>
    `;

    showModalHtml("Create an Account", html, async (data) => {
        try {
            const res = await fetch(`${API_BASE}/Users/Register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            const result = await res.json().catch(() => ({}));
            if (res.ok && result.isSuccessful) {
                showToast('Registered successfully! You can now log in.', 'success');
                closeModal();
            } else {
                showToast('Registration failed: ' + (result.message || 'Unknown error'), 'error');
            }
        } catch (e) {
            showToast('Error connecting to the registration service.', 'error');
        }
    }, false);
}

function showLoginModal() {
    const html = `
        <div class="form-group">
            <label class="form-label">Username</label>
            <input type="text" name="userName" class="form-input" required placeholder="Enter username">
        </div>
        <div class="form-group">
            <label class="form-label">Password</label>
            <input type="password" name="password" class="form-input" required placeholder="Enter password">
        </div>
    `;
    
    showModalHtml("Login", html, async (data) => {
        try {
            const res = await fetch(`${API_BASE}/Token`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            if (res.ok) {
                const tokenData = await res.json();
                if (tokenData.token) {
                    setTokens(tokenData.token, tokenData.refreshToken);
                    showToast('Logged in successfully', 'success');
                    closeModal();
                    loadData(currentState.currentView);
                } else {
                    showToast('Login failed: ' + (tokenData.message || 'No token'), 'error');
                }
            } else {
                showToast('Login failed: Invalid credentials', 'error');
            }
        } catch (e) {
            showToast('Error connecting to authentication service', 'error');
        }
    });
}

async function openModal(view, itemData = null) {
    let title = itemData ? `Edit ${view.slice(0, -1)}` : `Create New ${view.slice(0, -1)}`;

    if (view === 'movies') {
        await renderMovieForm(itemData);
        return; 
    } else if (view === 'users') {
        await renderUserFormAsync(itemData);
        return;
    }

    let formHtml = "";
    if (view === 'directors') {
        formHtml = renderDirectorForm(itemData);
    } else if (view === 'genres') {
        formHtml = renderGenreForm(itemData);
    } else if (view === 'groups') {
        formHtml = renderGroupForm(itemData);
    } else if (view === 'roles') {
        formHtml = renderRoleForm(itemData);
    } else {
        showToast(`Creation for ${view} is not implemented.`, 'error');
        return;
    }

    showModalHtml(title, formHtml, async (data) => {
        let finalData = itemData ? { ...itemData, ...data } : { ...data };
        if (itemData && itemData.id) finalData.id = itemData.id;
        await saveItem(view, finalData, !!itemData);
    }, !!itemData);
}

function showModalHtml(title, html, onSubmit, isEdit = true) {
    modalContainer.innerHTML = `
        <div class="modal-card" style="max-height: 90vh; display: flex; flex-direction: column;">
            <div class="modal-header" style="flex-shrink: 0;">
                <h2>${title}</h2>
                <button class="modal-close" onclick="closeModal()">&times;</button>
            </div>
            <form id="creation-form" style="display: flex; flex-direction: column; overflow: hidden;">
                <div style="overflow-y: auto; padding-right: 8px; margin-bottom: 16px;">
                    ${html}
                </div>
                <div class="form-actions" style="flex-shrink: 0; margin-top: 0;">
                    <button type="button" class="btn btn-secondary" onclick="closeModal()">Cancel</button>
                    <button type="submit" class="btn btn-primary">${isEdit ? 'Save Changes' : 'Create'}</button>
                </div>
            </form>
        </div>
    `;
    modalContainer.style.display = 'flex';
    
    document.getElementById('creation-form').addEventListener('submit', (e) => {
        e.preventDefault();
        const formData = new FormData(e.target);
        const data = Object.fromEntries(formData.entries());
        
        // Parse specific field types correctly
        if (data.isRetired !== undefined) data.isRetired = e.target.isRetired.checked;
        if (e.target.isActive) data.isActive = e.target.isActive.checked; // always read checkbox directly
        if (e.target.genreIds) {
            data.genreIds = Array.from(e.target.genreIds.selectedOptions).map(opt => parseInt(opt.value));
        }
        if (e.target.roleIds) {
            data.roleIds = Array.from(e.target.roleIds.selectedOptions).map(opt => parseInt(opt.value));
        }
        if (data.directorId !== undefined) data.directorId = parseInt(data.directorId) || null;
        if (data.totalRevenue !== undefined) data.totalRevenue = data.totalRevenue !== '' ? parseFloat(data.totalRevenue) : null;
        if (data.score !== undefined) data.score = data.score !== '' ? parseFloat(data.score) : 0;
        if (data.gender !== undefined) data.gender = parseInt(data.gender) || 0;
        if (data.roleId !== undefined) data.roleId = parseInt(data.roleId) || 0;
        // BUG FIX: 0 means "none selected" for optional FKs — must send null, not 0
        // Sending 0 causes EF Core to look for a Group/Country/City with Id=0 → 500 error
        if (data.groupId !== undefined) data.groupId = parseInt(data.groupId) || null;
        if (data.countryId !== undefined) data.countryId = parseInt(data.countryId) || null;
        if (data.cityId !== undefined) data.cityId = parseInt(data.cityId) || null;

        // Convert empty string dates to null to avoid backend parse errors
        for (let key in data) {
            if (data[key] === '') data[key] = null;
        }

        if (onSubmit) onSubmit(data);
    });
}

function closeModal() {
    modalContainer.style.display = 'none';
    modalContainer.innerHTML = '';
}

function renderDirectorForm(data = null) {
    return `
        <div class="form-group">
            <label class="form-label">First Name</label>
            <input type="text" name="firstName" class="form-input" required placeholder="e.g. Christopher" value="${data && data.firstName ? data.firstName : ''}">
        </div>
        <div class="form-group">
            <label class="form-label">Last Name</label>
            <input type="text" name="lastName" class="form-input" required placeholder="e.g. Nolan" value="${data && data.lastName ? data.lastName : ''}">
        </div>
        <div class="form-group" style="display: flex; align-items: center; gap: 10px;">
            <input type="checkbox" name="isRetired" id="isRetired" class="form-checkbox" ${data && data.isRetired ? 'checked' : ''}>
            <label for="isRetired" class="form-label" style="margin-bottom: 0">Is Retired?</label>
        </div>
    `;
}

function renderGenreForm(data = null) {
    return `
        <div class="form-group">
            <label class="form-label">Genre Name</label>
            <input type="text" name="name" class="form-input" required placeholder="e.g. Sci-Fi" value="${data && data.name ? data.name : ''}">
        </div>
    `;
}

function renderGroupForm(data = null) {
    return `
        <div class="form-group">
            <label class="form-label">Group Title</label>
            <input type="text" name="title" class="form-input" required placeholder="e.g. Admins" value="${data && data.title ? data.title : ''}">
        </div>
    `;
}

function renderRoleForm(data = null) {
    return `
        <div class="form-group">
            <label class="form-label">Role Name</label>
            <input type="text" name="name" class="form-input" required placeholder="e.g. Moderator" value="${data && data.name ? data.name : ''}">
        </div>
    `;
}

async function renderUserFormAsync(itemData = null) {
    let groups = [];
    let roles = [];
    try {
        const gRes = await apiCall(`${API_BASE}/Groups`);
        if (gRes.ok) groups = await gRes.json();
        
        const rRes = await apiCall(`${API_BASE}/Roles`);
        if (rRes.ok) roles = await rRes.json();
    } catch(e) {
        showToast('Warning: Could not fetch groups or roles', 'error');
    }

    const groupOptions = groups.map(g => `<option value="${g.id}" ${itemData && itemData.groupId === g.id ? 'selected' : ''}>${g.title}</option>`).join('');
    
    let selectedRoleIds = [];
    if (itemData && itemData.roleIds) {
        // API GET response returns roleIds directly as int[]
        selectedRoleIds = itemData.roleIds;
    }
    const roleOptions = roles.map(r => `<option value="${r.id}" ${selectedRoleIds.includes(r.id) ? 'selected' : ''}>${r.name}</option>`).join('');

    const html = `
        <div class="form-group">
            <label class="form-label">Username</label>
            <input type="text" name="userName" class="form-input" required value="${itemData && itemData.userName ? itemData.userName : ''}">
        </div>
        <div class="form-group">
            <label class="form-label">First Name</label>
            <input type="text" name="firstName" class="form-input" value="${itemData && itemData.firstName ? itemData.firstName : ''}">
        </div>
        <div class="form-group">
            <label class="form-label">Last Name</label>
            <input type="text" name="lastName" class="form-input" value="${itemData && itemData.lastName ? itemData.lastName : ''}">
        </div>
        <div class="form-group">
            <label class="form-label">Gender</label>
            <select name="gender" class="form-select">
                <option value="0" ${itemData && itemData.gender === 0 ? 'selected' : ''}>Unknown</option>
                <option value="1" ${itemData && itemData.gender === 1 ? 'selected' : ''}>Male</option>
                <option value="2" ${itemData && itemData.gender === 2 ? 'selected' : ''}>Female</option>
            </select>
        </div>
        <div class="form-group">
            <label class="form-label">Birth Date</label>
            <input type="date" name="birthDate" class="form-input" value="${itemData && itemData.birthDate && itemData.birthDate.includes('T') ? itemData.birthDate.split('T')[0] : ''}">
        </div>
        <div class="form-group">
            <label class="form-label">Score</label>
            <input type="number" name="score" class="form-input" step="0.1" value="${itemData && itemData.score !== undefined ? itemData.score : 0}">
        </div>
        <div class="form-group">
            <label class="form-label">Address</label>
            <input type="text" name="address" class="form-input" value="${itemData && itemData.address ? itemData.address : ''}">
        </div>
        <div class="form-group" style="display: flex; gap: 10px;">
            <div style="flex: 1;">
                <label class="form-label">Country ID</label>
                <input type="number" name="countryId" class="form-input" value="${itemData && itemData.countryId !== undefined ? itemData.countryId : 0}">
            </div>
            <div style="flex: 1;">
                <label class="form-label">City ID</label>
                <input type="number" name="cityId" class="form-input" value="${itemData && itemData.cityId !== undefined ? itemData.cityId : 0}">
            </div>
        </div>
        <div class="form-group">
            <label class="form-label">Group</label>
            <select name="groupId" class="form-select">
                <option value="0">No Group...</option>
                ${groupOptions}
            </select>
        </div>
        <div class="form-group">
            <label class="form-label">Roles (Hold Ctrl/Cmd to select multiple)</label>
            <select name="roleIds" class="form-select" multiple style="height: 100px">
                ${roleOptions}
            </select>
        </div>
        ${!itemData ? `
        <div class="form-group">
            <label class="form-label">Password</label>
            <input type="password" name="password" class="form-input" required>
        </div>` : `
        <div class="form-group">
            <label class="form-label">Password</label>
            <input type="password" name="password" class="form-input" required placeholder="Re-enter password to confirm changes">
        </div>`}
        <div class="form-group" style="display: flex; align-items: center; gap: 10px;">
            <input type="checkbox" name="isActive" id="isActive" class="form-checkbox" ${itemData && itemData.isActive !== false ? 'checked' : 'checked'}>
            <label for="isActive" class="form-label" style="margin-bottom: 0">Is Active?</label>
        </div>
    `;

    const title = itemData ? "Edit User" : "Create New User";
    showModalHtml(title, html, async (data) => {
        let finalData = itemData ? { ...itemData, ...data } : { ...data };
        if (itemData && itemData.id) finalData.id = itemData.id;
        
        // Preserve registrationDate: keep existing for edits, set current time for new users
        if (!itemData) {
            finalData.registrationDate = new Date().toISOString();
        } else if (!finalData.registrationDate) {
            finalData.registrationDate = itemData.registrationDate;
        }
        
        await saveItem('users', finalData, !!itemData);
    }, !!itemData);
}

async function renderMovieForm(itemData = null) {
    let directors = [];
    let genres = [];
    try {
        const dRes = await apiCall(`${MOVIES_BASE}/Directors`);
        if (dRes.ok) directors = await dRes.json();
        
        const gRes = await apiCall(`${MOVIES_BASE}/Genres`);
        if (gRes.ok) genres = await gRes.json();
    } catch(e) {
        showToast('Warning: Could not fetch directors or genres for dropdowns', 'error');
    }

    const directorOptions = directors.map(d => `<option value="${d.id}" ${itemData && itemData.directorId === d.id ? 'selected' : ''}>${d.firstName} ${d.lastName}</option>`).join('');
    
    let selectedGenreIds = [];
    if (itemData && itemData.movieGenres) {
        selectedGenreIds = itemData.movieGenres.map(mg => mg.genreId);
    }
    const genreOptions = genres.map(g => `<option value="${g.id}" ${selectedGenreIds.includes(g.id) ? 'selected' : ''}>${g.name}</option>`).join('');

    const html = `
        <div class="form-group">
            <label class="form-label">Movie Title</label>
            <input type="text" name="name" class="form-input" required placeholder="e.g. Inception" value="${itemData && itemData.name ? itemData.name : ''}">
        </div>
        <div class="form-group">
            <label class="form-label">Release Date</label>
            <input type="date" name="releaseDate" class="form-input" value="${itemData && itemData.releaseDate && itemData.releaseDate.includes('T') ? itemData.releaseDate.split('T')[0] : ''}">
        </div>
        <div class="form-group">
            <label class="form-label">Total Revenue ($)</label>
            <input type="number" name="totalRevenue" class="form-input" step="0.01" placeholder="0.00" value="${itemData && itemData.totalRevenue !== undefined ? itemData.totalRevenue : ''}">
        </div>
        <div class="form-group">
            <label class="form-label">Director</label>
            <select name="directorId" class="form-select" required>
                <option value="">Select Director...</option>
                ${directorOptions}
            </select>
        </div>
        <div class="form-group">
            <label class="form-label">Genres (Hold Ctrl/Cmd to select multiple)</label>
            <select name="genreIds" class="form-select" multiple style="height: 100px">
                ${genreOptions}
            </select>
        </div>
    `;
    const title = itemData ? "Edit Movie" : "Create New Movie";
    showModalHtml(title, html, async (data) => {
        let finalData = itemData ? { ...itemData, ...data } : { ...data };
        if (itemData && itemData.id) finalData.id = itemData.id;
        await saveItem('movies', finalData, !!itemData);
    }, !!itemData);
}

async function saveItem(view, data, isEdit) {
    const moviesTypes = ['movies', 'directors', 'genres'];
    let baseUrl = API_BASE;
    let endpoint = view;
    
    if (moviesTypes.includes(view)) {
        baseUrl = MOVIES_BASE;
        endpoint = view.charAt(0).toUpperCase() + view.slice(1);
    } else {
        endpoint = view.charAt(0).toUpperCase() + view.slice(1);
    }

    const url = `${baseUrl}/${endpoint}`;
    const method = isEdit ? 'PUT' : 'POST';

    try {
        const response = await apiCall(url, {
            method: method,
            body: JSON.stringify(data)
        });

        const result = await response.json().catch(() => ({}));
        
        if (response.ok && (result.isSuccessful || result.isSuccessful === undefined)) {
            showToast(result.message || "Saved successfully!");
            closeModal();
            loadData(view);
        } else {
            showToast("Error: " + (result.message || "Failed to save item."), "error");
        }
    } catch (error) {
        showToast("Exception: Could not connect to microservice.", "error");
    }
}

async function editItem(type, id) {
    // Ensure ID comparison works accurately even if ID is string/number mismatch
    const item = currentState[type].find(x => x.id == id);
    if (item) {
        openModal(type, item);
    } else {
        showToast("Error: Item not found", "error");
    }
}

async function deleteItem(type, id) {
    if (!confirm(`Are you sure you want to delete this item?`)) return;

    const moviesTypes = ['movies', 'directors', 'genres'];
    let baseUrl = API_BASE;
    let endpoint = type.charAt(0).toUpperCase() + type.slice(1);
    
    if (moviesTypes.includes(type)) {
        baseUrl = MOVIES_BASE;
    }

    const url = `${baseUrl}/${endpoint}/${id}`;

    try {
        const response = await apiCall(url, { method: 'DELETE' });
        
        if (response.ok) {
            showToast("Deleted successfully!");
            loadData(type);
        } else {
            const result = await response.json().catch(() => ({}));
            showToast("Error: " + (result.message || "Failed to delete item."), "error");
        }
    } catch (error) {
        showToast("Exception: Could not connect to microservice.", "error");
    }
}
