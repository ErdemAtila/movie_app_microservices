const API_BASE = 'https://localhost:7219/api';
const MOVIES_BASE = 'http://localhost:5033/api';

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

// DOM Elements
const mainDisplay = document.getElementById('main-display');
const navItems = document.querySelectorAll('.nav-item');
const globalSearch = document.getElementById('global-search');
const addItemBtn = document.getElementById('add-item-btn');
const modalContainer = document.getElementById('modal-container');

// Initial Load
document.addEventListener('DOMContentLoaded', () => {
    init();
});

async function init() {
    setupEventListeners();
    await loadData('users');
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

    modalContainer.addEventListener('click', (e) => {
        if (e.target === modalContainer) closeModal();
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
        url = `${API_BASE}/${type}`;
        const moviesTypes = ['movies', 'directors', 'genres'];
        // Capitalize for case-sensitive FS compatibility
        if (moviesTypes.some(t => type.startsWith(t))) {
            const capitalizedType = type.charAt(0).toUpperCase() + type.slice(1);
            url = `${MOVIES_BASE}/${capitalizedType}`;
        }

        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        
        // Handle 204 No Content:
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
        renderGeneric(data, '🔑 Role');
    } else if (type === 'groups') {
        renderGeneric(data, '📁 Group');
    } else if (type === 'directors') {
        currentState.directors = data;
        renderGeneric(data, '📽️ Director');
    } else if (type === 'genres') {
        currentState.genres = data;
        renderGeneric(data, '🏷️ Genre');
    } else if (type === 'movies') {
        currentState.movies = data;
        renderMovies(data);
    }
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
            <div class="card-header">
                <img src="https://ui-avatars.com/api/?name=${user.firstName}+${user.lastName}&background=random" class="avatar-large" alt="Avatar">
                <div class="header-text">
                    <h3 class="user-fullname">${user.firstName ?? ''} ${user.lastName ?? ''}</h3>
                    <p class="user-handle">@${user.userName}</p>
                </div>
            </div>
            <div class="card-body">
                <div class="info-item">
                    <p class="info-label">Score</p>
                    <p class="info-value">★ ${user.score.toFixed(1)}</p>
                </div>
                <div class="info-item">
                    <p class="info-label">Join Date</p>
                    <p class="info-value">${new Date(user.registrationDate).toLocaleDateString()}</p>
                </div>
                <div class="info-item">
                    <p class="info-label">Location</p>
                    <p class="info-value">${user.address || 'Not set'}</p>
                </div>
            </div>
            <div class="card-footer">
                <button class="btn btn-primary" onclick="alert('Viewing profile for ${user.userName}')">View Full Profile</button>
            </div>
        `;
        mainDisplay.appendChild(card);
    });
}

function renderGeneric(items, prefix) {
    mainDisplay.innerHTML = '';
    items.forEach(item => {
        const card = document.createElement('div');
        card.className = 'user-card'; // Reuse styling
        card.innerHTML = `
            <div class="card-header">
                <div class="logo-icon" style="background: #1e293b; color: white">${prefix.charAt(0)}</div>
                <div class="header-text">
                    <h3 class="user-fullname">${item.fullName || item.name || item.title}</h3>
                    <p class="user-handle">ID: ${item.id}</p>
                </div>
            </div>
            <div class="card-footer">
                <button class="btn btn-primary">Edit Settings</button>
            </div>
        `;
        mainDisplay.appendChild(card);
    });
}

function updateStats(users) {
    document.getElementById('stat-total-users').textContent = users.length;
    document.getElementById('stat-active-users').textContent = users.filter(u => u.isActive).length;
    
    const avgScore = users.length > 0 
        ? (users.reduce((acc, u) => acc + u.score, 0) / users.length).toFixed(1)
        : '0.0';
    document.getElementById('stat-avg-score').textContent = avgScore;
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
            d.fullName.toLowerCase().includes(q)
        );
        renderGeneric(filtered, '📽️ Director');
        return;
    }

    if (currentState.currentView === 'genres') {
        const filtered = currentState.genres.filter(g => 
            g.name.toLowerCase().includes(q)
        );
        renderGeneric(filtered, '🏷️ Genre');
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
            <button class="btn btn-primary" onclick="location.reload()">Retry Connection</button>
        </div>
    `;
}

function renderMovies(movies) {
    mainDisplay.innerHTML = '';
    
    if (!movies || movies.length === 0) {
        mainDisplay.innerHTML = '<div class="no-data">No movies found. Add them via Swagger (Port 7134).</div>';
        return;
    }

    movies.forEach(movie => {
        const card = document.createElement('div');
        card.className = 'user-card'; 
        
        const genres = movie.movieGenres && movie.movieGenres.length > 0 
            ? movie.movieGenres.map(mg => mg.genre.name).join(', ') 
            : 'No Genre';

        card.innerHTML = `
            <span class="status-badge status-active" style="background: rgba(34, 211, 238, 0.1); color: var(--accent)">
                🎬 MOVIE
            </span>
            <div class="card-header">
                <div class="logo-icon" style="background: linear-gradient(135deg, #f59e0b, #ef4444)">🎬</div>
                <div class="header-text">
                    <h3 class="user-fullname">${movie.name}</h3>
                    <p class="user-handle">${new Date(movie.releaseDate).getFullYear() || 'N/A'}</p>
                </div>
            </div>
            <div class="card-body">
                <div class="info-item">
                    <p class="info-label">Director</p>
                    <p class="info-value">${movie.directorF}</p>
                </div>
                <div class="info-item">
                    <p class="info-label">Genres</p>
                    <p class="info-value" style="font-size: 0.85rem">${movie.genresF.join(', ')}</p>
                </div>
                <div class="info-item">
                    <p class="info-label">Revenue</p>
                    <p class="info-value" style="color: #10b981">${movie.totalRevenueF}</p>
                </div>
            </div>
            <div class="card-footer">
                <button class="btn btn-primary" onclick="alert('Viewing trailer for ${movie.name}')">Watch Trailer</button>
            </div>
        `;
        mainDisplay.appendChild(card);
    });
}

// Modal and Form Logic
function openModal(view) {
    let title = "";
    let formHtml = "";

    if (view === 'movies') {
        title = "Create New Movie";
        renderMovieForm();
        return; // handleMovieForm is async
    } else if (view === 'directors') {
        title = "Create New Director";
        formHtml = renderDirectorForm();
    } else if (view === 'genres') {
        title = "Create New Genre";
        formHtml = renderGenreForm();
    } else {
        alert(`Creation for ${view} view is not implemented yet.`);
        return;
    }

    showModalHtml(title, formHtml);
}

function showModalHtml(title, html) {
    modalContainer.innerHTML = `
        <div class="modal-card">
            <div class="modal-header">
                <h2>${title}</h2>
                <button class="modal-close" onclick="closeModal()">&times;</button>
            </div>
            <form id="creation-form">
                ${html}
                <div class="form-actions">
                    <button type="button" class="btn btn-secondary" onclick="closeModal()">Cancel</button>
                    <button type="submit" class="btn btn-primary">Save Changes</button>
                </div>
            </form>
        </div>
    `;
    modalContainer.style.display = 'flex';
    
    document.getElementById('creation-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const formData = new FormData(e.target);
        const data = Object.fromEntries(formData.entries());
        
        // Custom handling for checkboxes and arrays
        if (data.isRetired !== undefined) data.isRetired = e.target.isRetired.checked;
        if (e.target.genreIds) {
            data.genreIds = Array.from(e.target.genreIds.selectedOptions).map(opt => parseInt(opt.value));
        }
        if (data.directorId) data.directorId = parseInt(data.directorId);
        if (data.totalRevenue) data.totalRevenue = parseFloat(data.totalRevenue);

        await saveItem(currentState.currentView, data);
    });
}

function closeModal() {
    modalContainer.style.display = 'none';
    modalContainer.innerHTML = '';
}

function renderDirectorForm() {
    return `
        <div class="form-group">
            <label class="form-label">First Name</label>
            <input type="text" name="firstName" class="form-input" required placeholder="e.g. Christopher">
        </div>
        <div class="form-group">
            <label class="form-label">Last Name</label>
            <input type="text" name="lastName" class="form-input" required placeholder="e.g. Nolan">
        </div>
        <div class="form-group" style="display: flex; align-items: center; gap: 10px;">
            <input type="checkbox" name="isRetired" id="isRetired" class="form-checkbox">
            <label for="isRetired" class="form-label" style="margin-bottom: 0">Is Retired?</label>
        </div>
    `;
}

function renderGenreForm() {
    return `
        <div class="form-group">
            <label class="form-label">Genre Name</label>
            <input type="text" name="name" class="form-input" required placeholder="e.g. Sci-Fi">
        </div>
    `;
}

async function renderMovieForm() {
    const directors = await fetch(`${MOVIES_BASE}/directors`).then(res => res.ok ? res.json() : []);
    const genres = await fetch(`${MOVIES_BASE}/genres`).then(res => res.ok ? res.json() : []);

    const directorOptions = directors.map(d => `<option value="${d.id}">${d.firstName} ${d.lastName}</option>`).join('');
    const genreOptions = genres.map(g => `<option value="${g.id}">${g.name}</option>`).join('');

    const html = `
        <div class="form-group">
            <label class="form-label">Movie Title</label>
            <input type="text" name="name" class="form-input" required placeholder="e.g. Inception">
        </div>
        <div class="form-group">
            <label class="form-label">Release Date</label>
            <input type="date" name="releaseDate" class="form-input">
        </div>
        <div class="form-group">
            <label class="form-label">Total Revenue ($)</label>
            <input type="number" name="totalRevenue" class="form-input" step="0.01" placeholder="0.00">
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
    showModalHtml("Create New Movie", html);
}

async function saveItem(view, data) {
    let endpoint = view === 'movies' ? 'movies' : view;
    let url = `${MOVIES_BASE}/${endpoint}`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const result = await response.json();
        if (response.ok && result.isSuccessful) {
            alert(result.message || "Saved successfully!");
            closeModal();
            loadData(view);
        } else {
            alert("Error: " + (result.message || "Failed to save item."));
        }
    } catch (error) {
        console.error("Save Error:", error);
        alert("Exception: Could not connect to microservice.");
    }
}
