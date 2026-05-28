// ============================================================
// lobby.js — Logica lobby (lobby.html)
// ============================================================

let currentUser = null;
let selectedTarget = 41;
const selectedPlayers = { p2: null, p3: null, p4: null }; // altri 3 slot

document.addEventListener('DOMContentLoaded', async () => {
  currentUser = requireAuth();
  if (!currentUser) return;

  // Aggiorna lo slot "Tu"
  document.getElementById('slot-you').textContent = currentUser.username;

  // Carica utenti e partite
  await Promise.all([loadUsers(), loadMatches()]);

  // Auto-refresh partite ogni 10s
  setInterval(loadMatches, 10000);
});

// ── Selezione modalità di gioco ───────────────────────────────
function selectMode(btn) {
  document.querySelectorAll('.mode-btn').forEach(b => b.classList.remove('active'));
  btn.classList.add('active');
  selectedTarget = parseInt(btn.dataset.target, 10);
}

// ── Carica lista utenti ───────────────────────────────────────
async function loadUsers() {
  try {
    const users = await apiGet('/api/lobby/users');
    renderUsersList(users);
  } catch (e) {
    document.getElementById('users-list').innerHTML =
      '<p style="color:var(--txt-muted);font-size:.85rem">Errore caricamento utenti.</p>';
  }
}

function renderUsersList(users) {
  const container = document.getElementById('users-list');
  if (!users || users.length === 0) {
    container.innerHTML = '<p style="color:var(--txt-muted);font-size:.85rem">Nessun utente disponibile.</p>';
    return;
  }

  container.innerHTML = users.map(u => {
    const isMe = u.id === currentUser.id;
    const isAssigned = Object.values(selectedPlayers).some(p => p && p.id === u.id);
    return `<div class="user-chip ${isMe ? 'chip-you' : ''} ${isAssigned && !isMe ? 'chip-assigned' : ''}"
                 id="chip-${u.id}"
                 onclick="${isMe ? '' : `togglePlayerSelect('${u.id}', '${u.username}')`}">
      <div class="chip-avatar">${u.username[0].toUpperCase()}</div>
      <span>${u.username}</span>
      ${isMe ? '<span style="font-size:.7rem;opacity:.6">(Tu)</span>' : ''}
    </div>`;
  }).join('');
}

function togglePlayerSelect(userId, username) {
  // Se già selezionato, rimuovilo
  for (const slot of ['p2', 'p3', 'p4']) {
    if (selectedPlayers[slot]?.id === userId) {
      selectedPlayers[slot] = null;
      refreshSlots();
      refreshUserChips();
      validateForm();
      return;
    }
  }
  // Aggiungi al primo slot libero
  for (const slot of ['p2', 'p3', 'p4']) {
    if (!selectedPlayers[slot]) {
      selectedPlayers[slot] = { id: userId, username };
      refreshSlots();
      refreshUserChips();
      validateForm();
      return;
    }
  }
  // Tutti i slot pieni
  showSlotFull();
}

function refreshSlots() {
  const slotDefs = {
    p2: { id: 'slot-p2', label: 'Avversario (Pos 2)' },
    p3: { id: 'slot-p3', label: 'Compagno (Pos 3)' },
    p4: { id: 'slot-p4', label: 'Avversario (Pos 4)' },
  };
  for (const [key, def] of Object.entries(slotDefs)) {
    const el = document.getElementById(def.id);
    const player = selectedPlayers[key];
    if (player) {
      el.classList.add('filled');
      el.innerHTML = `<span class="slot-icon">✓</span><span>${player.username}</span>
        <button style="margin-left:auto;background:none;border:none;color:var(--txt-muted);cursor:pointer;font-size:.8rem"
                onclick="removeSlot('${key}')">✕</button>`;
    } else {
      el.classList.remove('filled');
      el.innerHTML = `<span class="slot-icon">➕</span><span class="slot-text">${def.label}</span>`;
    }
  }
}

function removeSlot(slot) {
  selectedPlayers[slot] = null;
  refreshSlots();
  refreshUserChips();
  validateForm();
}

function refreshUserChips() {
  // Ricarica completamente il render
  apiGet('/api/lobby/users').then(renderUsersList).catch(() => {});
}

function showSlotFull() {
  const err = document.getElementById('create-error');
  err.textContent = 'Hai già selezionato 3 giocatori. Rimuovine uno per cambiare.';
  err.classList.remove('hidden');
  setTimeout(() => err.classList.add('hidden'), 3000);
}

function validateForm() {
  const allFilled = selectedPlayers.p2 && selectedPlayers.p3 && selectedPlayers.p4;
  document.getElementById('btn-create-match').disabled = !allFilled;
}

// ── Crea partita ──────────────────────────────────────────────
async function createMatch() {
  const { p2, p3, p4 } = selectedPlayers;
  if (!p2 || !p3 || !p4) return;

  const btn = document.getElementById('btn-create-match');
  btn.disabled = true;
  btn.querySelector('.btn-text').textContent = 'Creazione...';

  try {
    const res = await apiPost('/api/lobby/create', {
      user1Id:      currentUser.id,
      user2Id:      p2.id,
      user3Id:      p3.id,
      user4Id:      p4.id,
      targetPoints: selectedTarget
    });

    if (res.matchId) {
      // Salva l'ID partita e vai al gioco
      sessionStorage.setItem('marafone_match', res.matchId);
      window.location.href = `/game.html?matchId=${res.matchId}`;
    } else {
      const err = document.getElementById('create-error');
      err.textContent = res.error || 'Errore nella creazione della partita.';
      err.classList.remove('hidden');
      btn.disabled = false;
      btn.querySelector('.btn-text').textContent = 'Inizia la Partita!';
    }
  } catch (e) {
    const err = document.getElementById('create-error');
    err.textContent = 'Impossibile connettersi al server.';
    err.classList.remove('hidden');
    btn.disabled = false;
    btn.querySelector('.btn-text').textContent = 'Inizia la Partita!';
  }
}

// ── Carica partite ────────────────────────────────────────────
async function loadMatches() {
  try {
    const matches = await apiGet('/api/lobby/all');
    renderMatches(matches);
  } catch (e) {
    document.getElementById('matches-list').innerHTML =
      '<p class="no-matches">Impossibile caricare le partite.</p>';
  }
}

function renderMatches(matches) {
  const container = document.getElementById('matches-list');
  if (!matches || matches.length === 0) {
    container.innerHTML = '<p class="no-matches">Nessuna partita in corso.<br/>Creane una!</p>';
    return;
  }

  container.innerHTML = matches.map(m => {
    let badgeClass, badgeText;
    if (m.isGameOver)           { badgeClass='badge-over';    badgeText='Finita'; }
    else if (m.phase==='Playing'){ badgeClass='badge-playing'; badgeText='In gioco'; }
    else                         { badgeClass='badge-waiting'; badgeText='In attesa'; }

    return `<div class="match-card" onclick="joinMatch('${m.id}')">
      <div>
        <div class="match-players">Partita <strong>#${m.id.substring(0,8)}</strong></div>
        <div style="font-size:.78rem;color:var(--txt-muted);margin-top:4px">${m.phase ?? ''}</div>
      </div>
      <div style="display:flex;align-items:center;gap:10px">
        <span class="match-target">🏆 ${m.targetPoints}</span>
        <span class="match-badge ${badgeClass}">${badgeText}</span>
      </div>
    </div>`;
  }).join('');
}

function joinMatch(matchId) {
  sessionStorage.setItem('marafone_match', matchId);
  window.location.href = `/game.html?matchId=${matchId}`;
}
