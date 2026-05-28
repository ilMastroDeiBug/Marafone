// ============================================================
// app.js — Stato globale, auth e utilitiy condivise
// ============================================================

const API_BASE = window.location.origin; // Punta all'ASP.NET Core che serve i file

// ── Stato utente (persiste in sessionStorage) ────────────────
const SESSION_KEY = 'marafone_user';

function saveUser(user) {
  sessionStorage.setItem(SESSION_KEY, JSON.stringify(user));
}

function loadUser() {
  const raw = sessionStorage.getItem(SESSION_KEY);
  return raw ? JSON.parse(raw) : null;
}

function clearUser() {
  sessionStorage.removeItem(SESSION_KEY);
}

function logout() {
  clearUser();
  window.location.href = '/';
}

// ── Richiede autenticazione (redirect se non loggato) ────────
function requireAuth() {
  const user = loadUser();
  if (!user) {
    window.location.href = '/';
    return null;
  }
  return user;
}

// ── Imposta l'username nell'header se presente ───────────────
function setupHeader() {
  const el = document.getElementById('header-username');
  const user = loadUser();
  if (el && user) el.textContent = '👤 ' + user.username;
}

// ── API helpers ───────────────────────────────────────────────
async function apiPost(path, body) {
  const res = await fetch(`${API_BASE}${path}`, {
    method:  'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(body)
  });
  return res.json();
}

async function apiGet(path) {
  const res = await fetch(`${API_BASE}${path}`);
  return res.json();
}

// ── Toast / feedback ──────────────────────────────────────────
function showMessage(id, msg, type = 'success') {
  const el = document.getElementById(id);
  if (!el) return;
  el.textContent = msg;
  el.className = `auth-message ${type}`;
  el.classList.remove('hidden');
  setTimeout(() => el.classList.add('hidden'), 4000);
}

// ── Suit helpers ──────────────────────────────────────────────
function suitSymbol(suit) {
  const map = { coppe:'♥', denara:'♦', bastoni:'♣', spade:'♠' };
  return map[suit?.toLowerCase()] ?? suit ?? '?';
}

function suitColor(suit) {
  const map = { coppe:'suit-coppe', denara:'suit-denara', bastoni:'suit-bastoni', spade:'suit-spade' };
  return map[suit?.toLowerCase()] ?? '';
}

function rankLabel(rank) {
  const map = {
    tre:'3', due:'2', asso:'A', re:'R', cavallo:'C', fante:'F',
    sette:'7', sei:'6', cinque:'5', quattro:'4', dorso:'?'
  };
  return map[rank?.toLowerCase()] ?? rank;
}

// ── Genera HTML di una carta ──────────────────────────────────
function buildCardHTML(rank, suit, clickable = false, extra = '') {
  const r = rankLabel(rank);
  const sym = suitSymbol(suit);
  const colorCls = suitColor(suit);
  const isCover = rank?.toLowerCase() === 'dorso';

  if (isCover) {
    return `<div class="playing-card card-back-face" ${extra}></div>`;
  }

  return `<div class="playing-card ${clickable ? '' : 'played-on-table'} ${colorCls}" ${extra}>
    <span class="card-rank-corner-tl">${r}</span>
    <span class="card-suit-icon">${sym}</span>
    <span class="card-rank">${r}</span>
    <span class="card-rank-corner-br">${sym}</span>
  </div>`;
}

// ============================================================
// AUTH — Login / Register (pagina index.html)
// ============================================================
function switchTab(tab) {
  const formLogin    = document.getElementById('form-login');
  const formRegister = document.getElementById('form-register');
  const tabLogin     = document.getElementById('tab-login');
  const tabRegister  = document.getElementById('tab-register');
  if (!formLogin) return;

  if (tab === 'login') {
    formLogin.classList.remove('hidden');
    formRegister.classList.add('hidden');
    tabLogin.classList.add('active');
    tabRegister.classList.remove('active');
  } else {
    formLogin.classList.add('hidden');
    formRegister.classList.remove('hidden');
    tabLogin.classList.remove('active');
    tabRegister.classList.add('active');
  }
}

async function handleLogin(e) {
  e.preventDefault();
  const username = document.getElementById('login-username').value.trim();
  if (!username) return;

  try {
    const res = await apiPost('/api/user/login', { username });
    if (res.id) {
      saveUser({ id: res.id, username: res.username, email: res.email });
      window.location.href = '/lobby.html';
    } else {
      showMessage('auth-message', res.error || 'Errore login', 'error');
    }
  } catch (err) {
    showMessage('auth-message', 'Impossibile connettersi al server.', 'error');
  }
}

async function handleRegister(e) {
  e.preventDefault();
  const username = document.getElementById('reg-username').value.trim();
  const email    = document.getElementById('reg-email').value.trim();
  if (!username) return;

  try {
    const res = await apiPost('/api/user/register', { username, email });
    if (res.id) {
      saveUser({ id: res.id, username: res.username, email: res.email });
      showMessage('auth-message', '✓ ' + (res.message ?? 'Benvenuto!'), 'success');
      setTimeout(() => { window.location.href = '/lobby.html'; }, 900);
    } else {
      showMessage('auth-message', res.error || 'Errore registrazione', 'error');
    }
  } catch (err) {
    showMessage('auth-message', 'Impossibile connettersi al server.', 'error');
  }
}

// ── Auto-redirect se già loggato (solo sulla index) ──────────
if (window.location.pathname === '/' || window.location.pathname.endsWith('index.html')) {
  if (loadUser()) window.location.href = '/lobby.html';
}

// Setup header su tutte le pagine
document.addEventListener('DOMContentLoaded', setupHeader);
