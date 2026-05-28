// ============================================================
// game.js — Logica tavolo di gioco + SignalR
// ============================================================

let currentUser = null;
let matchId     = null;
let matchState  = null;  // Ultimo MatchDTO ricevuto
let connection  = null;  // SignalR connection

// Posizione locale del giocatore rispetto al tavolo
// Sarà calcolata quando riceviamo il primo DTO
let myPosition = null;  // 'sud'|'nord'|'est'|'ovest'

// ── Init ──────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', async () => {
  currentUser = requireAuth();
  if (!currentUser) return;

  // Leggi matchId dall'URL o da sessionStorage
  const params = new URLSearchParams(window.location.search);
  matchId = params.get('matchId') || sessionStorage.getItem('marafone_match');
  if (!matchId) { window.location.href = '/lobby.html'; return; }

  // Avvia SignalR
  await initSignalR();

  // Carica stato iniziale
  await refreshMatch();
});

// ── SignalR ───────────────────────────────────────────────────
async function initSignalR() {
  connection = new signalR.HubConnectionBuilder()
    .withUrl('/matchHub')
    .withAutomaticReconnect([0, 1000, 3000, 5000])
    .build();

  connection.on('MatchUpdated', () => {
    refreshMatch();
  });

  connection.on('PlayerJoined', (connId) => {
    console.log('Giocatore connesso:', connId);
  });

  connection.onreconnected(async () => {
    await connection.invoke('JoinMatch', matchId);
    await refreshMatch();
  });

  try {
    await connection.start();
    await connection.invoke('JoinMatch', matchId);
  } catch (e) {
    console.warn('SignalR connection failed, falling back to polling:', e);
    // Fallback: polling ogni 3 secondi
    setInterval(refreshMatch, 3000);
  }
}

// ── Fetch stato partita ───────────────────────────────────────
async function refreshMatch() {
  try {
    const dto = await apiGet(`/api/match/${matchId}/player/${currentUser.id}`);
    if (!dto || dto.error) {
      showError('Partita non trovata o accesso non autorizzato.');
      return;
    }
    matchState = dto;
    renderMatch(dto);
  } catch (e) {
    showError('Errore di connessione al server.');
  }
}

// ── Render principale ─────────────────────────────────────────
function renderMatch(dto) {
  // Risolvi posizioni
  const players = resolvePositions(dto);

  // Header
  document.getElementById('game-target-badge').textContent = `🏆 ${dto.targetPoints} punti`;
  updateStatusText(dto);

  // Nomi e turno
  renderPlayerZone('sud',   players.sud,   dto);
  renderPlayerZone('nord',  players.nord,  dto);
  renderPlayerZone('ovest', players.ovest, dto);
  renderPlayerZone('est',   players.est,   dto);

  // Briscola
  const briscolaEl = document.getElementById('briscola-suit');
  if (dto.briscolaAttuale) {
    briscolaEl.textContent = `${suitSymbol(dto.briscolaAttuale)} ${dto.briscolaAttuale}`;
    briscolaEl.className = `briscola-suit ${suitColor(dto.briscolaAttuale)}`;
  } else {
    briscolaEl.textContent = '—';
    briscolaEl.className = 'briscola-suit';
  }

  // Tavolo
  renderTavolo(dto.tavolo);

  // Punteggi
  document.getElementById('score-name-sq1').textContent = dto.squadra1.name;
  document.getElementById('score-name-sq2').textContent = dto.squadra2.name;
  document.getElementById('hand-pts-sq1').textContent   = dto.squadra1.handPointsReal;
  document.getElementById('hand-pts-sq2').textContent   = dto.squadra2.handPointsReal;
  document.getElementById('match-pts-sq1').textContent  = dto.squadra1.matchPoints;
  document.getElementById('match-pts-sq2').textContent  = dto.squadra2.matchPoints;

  // Mia mano
  renderMyHand(players.sud, dto);

  // Overlays
  handleOverlays(dto, players.sud);
}

// Calcola quale slot (sud/nord/est/ovest) corrisponde a quale player
function resolvePositions(dto) {
  // L'ordine nel DTO è: Sq1P1, Sq1P2, Sq2P1, Sq2P2
  // L'ordine nei _sedie[] è: Sq1P1(0), Sq2P1(1), Sq1P2(2), Sq2P2(3)
  // Sedie: 0=Sud, 1=Ovest, 2=Nord, 3=Est (rispetto a chi ha il turno 0)
  const allPlayers = [
    { ...dto.squadra1.player1, team: dto.squadra1.name, sediaIdx: 0 },
    { ...dto.squadra2.player1, team: dto.squadra2.name, sediaIdx: 1 },
    { ...dto.squadra1.player2, team: dto.squadra1.name, sediaIdx: 2 },
    { ...dto.squadra2.player2, team: dto.squadra2.name, sediaIdx: 3 },
  ];

  // Trova il mio indice di sedia
  const myPlayer = allPlayers.find(p => p.id === currentUser.id);
  if (!myPlayer) {
    // Spettatore: metti Sq1P1 come Sud
    return {
      sud:   allPlayers[0],
      ovest: allPlayers[1],
      nord:  allPlayers[2],
      est:   allPlayers[3],
    };
  }

  const myIdx = myPlayer.sediaIdx;
  // Ruota in modo che io sia sempre a Sud
  const positions = ['sud', 'ovest', 'nord', 'est'];
  const result = {};
  for (let offset = 0; offset < 4; offset++) {
    const sediaIdx = (myIdx + offset) % 4;
    result[positions[offset]] = allPlayers[sediaIdx];
  }
  return result;
}

function renderPlayerZone(pos, player, dto) {
  if (!player) return;

  const nameEl   = document.getElementById(`name-${pos}`);
  const avatarEl = document.getElementById(`avatar-${pos}`);
  const teamEl   = document.getElementById(`team-${pos}`);
  const turnEl   = document.getElementById(`turn-${pos}`);
  const handEl   = document.getElementById(`hand-${pos}`);

  if (nameEl)   nameEl.textContent   = player.name || '—';
  if (avatarEl) avatarEl.textContent = (player.name || '?')[0].toUpperCase();
  if (teamEl)   teamEl.textContent   = player.team || '';

  const isMyTurn = player.id === dto.currentPlayerId;
  if (turnEl)   turnEl.className = `turn-indicator ${isMyTurn ? 'active' : ''}`;

  // Mano avversario (dorsi)
  if (handEl && pos !== 'sud') {
    handEl.innerHTML = (player.hand || [])
      .map(() => `<div class="card-back" title="Carta coperta"></div>`)
      .join('');
  }
}

function renderTavolo(tavolo) {
  const el = document.getElementById('table-cards');
  if (!tavolo || tavolo.length === 0) {
    el.innerHTML = '<div class="table-placeholder">Nessuna carta sul tavolo</div>';
    return;
  }
  el.innerHTML = tavolo.map(pc => {
    const colorCls = suitColor(pc.card.suit);
    return buildCardHTML(pc.card.rank, pc.card.suit, false,
      `title="${pc.playerName}" class="playing-card played-on-table ${colorCls}"`);
  }).join('');
}

function renderMyHand(myPlayer, dto) {
  const handEl = document.getElementById('my-hand');
  if (!myPlayer || !myPlayer.hand) { handEl.innerHTML = ''; return; }

  const isMyTurn = myPlayer.id === dto.currentPlayerId && dto.phase === 'Playing';
  const leadSuit = dto.tavolo && dto.tavolo.length > 0 ? dto.tavolo[0].card.suit : null;

  handEl.innerHTML = myPlayer.hand.map((card, i) => {
    if (card.rank === 'Dorso') {
      return `<div class="playing-card card-back-face"></div>`;
    }

    // Calcola se la carta è giocabile (rispettando l'obbligo di rispondere al seme)
    let playable = isMyTurn;
    let disabled = '';

    if (isMyTurn && leadSuit) {
      const hasLeadSuit = myPlayer.hand.some(c =>
        c.suit?.toLowerCase() === leadSuit?.toLowerCase() && c.rank !== 'Dorso');
      if (hasLeadSuit) {
        const cardSuit = card.suit?.toLowerCase();
        const briscolaLower = dto.briscolaAttuale?.toLowerCase();
        if (cardSuit !== leadSuit?.toLowerCase() && cardSuit !== briscolaLower) {
          playable = false;
          disabled = 'disabled';
        }
      }
    }

    if (!isMyTurn) disabled = 'disabled';

    const colorCls = suitColor(card.suit);
    return `<div class="playing-card ${colorCls} ${disabled}"
                 data-rank="${card.rank}"
                 data-suit="${card.suit}"
                 onclick="${playable ? `playCard('${card.rank}','${card.suit}')` : ''}"
                 title="${card.rank} di ${card.suit}">
      <span class="card-rank-corner-tl">${rankLabel(card.rank)}</span>
      <span class="card-suit-icon">${suitSymbol(card.suit)}</span>
      <span class="card-rank">${rankLabel(card.rank)}</span>
      <span class="card-rank-corner-br">${suitSymbol(card.suit)}</span>
    </div>`;
  }).join('');
}

function updateStatusText(dto) {
  const el = document.getElementById('game-status-text');
  if (!el) return;
  if (dto.isGameOver) {
    el.textContent = `🏆 Vince: ${dto.vincitorePartita}`;
  } else if (dto.phase === 'BriscolaSelection') {
    el.textContent = `${dto.currentPlayerName} sceglie la briscola...`;
  } else {
    const isMyTurn = dto.currentPlayerId === currentUser.id;
    el.textContent = isMyTurn ? '⚡ È il tuo turno!' : `Turno di ${dto.currentPlayerName}`;
  }
}

// ── Overlays ──────────────────────────────────────────────────
function handleOverlays(dto, myPlayer) {
  const needsBriscola = dto.phase === 'BriscolaSelection'
    && dto.currentPlayerId === currentUser.id;

  document.getElementById('overlay-briscola').classList.toggle('hidden', !needsBriscola);

  if (dto.isGameOver) {
    showGameOver(dto);
  }

  // Nuova smazzata: mano vuota ma partita non finita
  const myHandEmpty = !myPlayer?.hand?.length || myPlayer.hand.every(c => c.rank === 'Dorso');
  if (myHandEmpty && !dto.isGameOver && dto.phase === 'Playing'
      && dto.tavolo && dto.tavolo.length === 0) {
    showNewHandOverlay(dto);
  }
}

function showGameOver(dto) {
  document.getElementById('gameover-title').textContent =
    dto.vincitorePartita ? `🏆 ${dto.vincitorePartita} vince!` : 'Partita terminata!';

  document.getElementById('gameover-scores').innerHTML = `
    <div class="gameover-score-item">
      <div class="gs-name">${dto.squadra1.name}</div>
      <div class="gs-pts">${dto.squadra1.matchPoints}</div>
    </div>
    <div class="gameover-score-item">
      <div class="gs-name">${dto.squadra2.name}</div>
      <div class="gs-pts">${dto.squadra2.matchPoints}</div>
    </div>`;

  document.getElementById('overlay-gameover').classList.remove('hidden');
}

function showNewHandOverlay(dto) {
  document.getElementById('hand-results').innerHTML = `
    <div class="gameover-scores">
      <div class="gameover-score-item">
        <div class="gs-name">${dto.squadra1.name}</div>
        <div class="gs-pts">${dto.squadra1.matchPoints}</div>
      </div>
      <div class="gameover-score-item">
        <div class="gs-name">${dto.squadra2.name}</div>
        <div class="gs-pts">${dto.squadra2.matchPoints}</div>
      </div>
    </div>`;
  document.getElementById('overlay-new-hand').classList.remove('hidden');
}

function showError(msg) {
  document.getElementById('error-msg-text').textContent = msg;
  document.getElementById('overlay-error').classList.remove('hidden');
}

// ── Azioni di gioco ───────────────────────────────────────────
async function setBriscola(suit) {
  document.getElementById('overlay-briscola').classList.add('hidden');
  try {
    const res = await apiPost(`/api/match/${matchId}/briscola`, {
      playerId: currentUser.id,
      suit
    });
    if (res.error) showError(res.error);
    else await refreshMatch();
  } catch (e) { showError('Errore di rete.'); }
}

async function playCard(rank, suit) {
  try {
    const res = await apiPost(`/api/match/${matchId}/play`, {
      playerId: currentUser.id,
      rank,
      suit
    });
    if (res.error) showError(res.error);
    else await refreshMatch();
  } catch (e) { showError('Errore di rete.'); }
}

async function startNextHand() {
  document.getElementById('overlay-new-hand').classList.add('hidden');
  try {
    const res = await apiPost(`/api/match/${matchId}/next-hand`, {
      playerId: currentUser.id
    });
    if (res.error) showError(res.error);
    else await refreshMatch();
  } catch (e) { showError('Errore di rete.'); }
}

async function forfeit() {
  if (!confirm('Sei sicuro di voler abbandonare? La vittoria andrà agli avversari.')) return;
  try {
    await apiPost(`/api/match/${matchId}/forfeit`, { playerId: currentUser.id });
    await refreshMatch();
  } catch (e) { showError('Errore di rete.'); }
}

function goToLobby() {
  window.location.href = '/lobby.html';
}

function confirmLeave() {
  if (confirm('Vuoi tornare alla lobby? La partita rimane in corso.')) {
    window.location.href = '/lobby.html';
  }
}
