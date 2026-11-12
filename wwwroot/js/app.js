// TFT Little Legends AI Agent Office Simulator — Frontend Controller
const state = {
  boss: null,
  agents: [],
  documents: [],
  selectedAgentId: 'galio',
  renderMode: 'sprite', // 'sprite' | 'video_angle'
  attachedFileName: null,
  attachedContent: null,
  speechTimers: {},
  particles: [],
  projectiles: [],
  shockwaves: []
};

const fxCanvas = document.getElementById('fxCanvas');
const fxCtx = fxCanvas.getContext('2d');

async function initApp() {
  setupCanvasLoop();
  bindUIEvents();
  await fetchOfficeState();
  appendTimelineLog('👑 Sếp Claude', 'Hệ thống Văn Phòng Linh Thú ĐTCL (C# .NET 8 Core) đã khởi động! Sẵn sàng nhận lệnh điều phối T1 Galio (PDF), Zeri (Word), Jhin (Excel), Jinx (PowerPoint).', '#f59e0b');
}

async function fetchOfficeState() {
  try {
    const res = await fetch('/api/state');
    const data = await res.json();
    state.boss = data.boss;
    state.agents = data.agents;
    state.documents = data.documents;

    renderBossCard();
    renderRoster();
    renderOfficeEntities();
    renderInspector();
    renderVault();
  } catch (err) {
    console.error('Failed to load state:', err);
  }
}

function renderBossCard() {
  if (!state.boss) return;
  document.getElementById('bossCmdCount').textContent = state.boss.commandsDispatched;
  document.getElementById('bossThoughtBox').textContent = `"${state.boss.currentThought}"`;
}

function renderRoster() {
  const container = document.getElementById('agentRosterList');
  container.innerHTML = '';

  state.agents.forEach(agent => {
    const stars = '★'.repeat(agent.starLevel);
    const activeClass = agent.id === state.selectedAgentId ? 'active' : '';
    const item = document.createElement('div');
    item.className = `roster-item ${activeClass}`;
    item.style.borderLeft = `4px solid ${agent.themeColor}`;
    item.innerHTML = `
      <div class="roster-portrait-box" style="border-color: ${agent.themeColor}">
        <img src="${agent.sprites.portrait}" alt="${agent.name}" />
        <div class="roster-stars">${stars}</div>
      </div>
      <div class="roster-info">
        <div class="roster-name-row">
          <span class="roster-name">#${agent.videoOrder} ${agent.name}</span>
          <span class="ext-badge" style="color: ${agent.themeColor}">${agent.fileExtension.toUpperCase()}</span>
        </div>
        <div class="roster-role">${agent.role}</div>
        <div class="mana-bar-wrap">
          <div class="mana-bar-fill" style="width: ${Math.min(100, agent.mana)}%; background: ${agent.themeColor}"></div>
        </div>
      </div>
    `;
    item.addEventListener('click', () => {
      state.selectedAgentId = agent.id;
      renderRoster();
      renderInspector();
      spawnShockwave(agent.currentPos.x, agent.currentPos.y, agent.themeColor);
      showEntitySpeech(agent.id, agent.quote, 3200);
    });
    container.appendChild(item);
  });
}

function getEntityImageSrc(entity, isBoss = false) {
  if (isBoss) {
    const pose = entity.state === 'working' ? 'work' : entity.state === 'walking' ? 'walk' : entity.state === 'ult' ? 'ult' : 'idle';
    return entity.sprites[pose] || entity.sprites.idle;
  }

  if (state.renderMode === 'video_angle') {
    const angleKey = entity.currentAngle || 'ctrl1';
    return entity.sprites.videoAngles[angleKey] || entity.sprites.videoAngles.ctrl1;
  } else {
    if (entity.state === 'walking') return entity.sprites.walk;
    if (entity.state === 'working') return entity.sprites.work;
    if (entity.state === 'ult') return entity.sprites.ult;
    return entity.sprites.idle;
  }
}

function renderOfficeEntities() {
  const layer = document.getElementById('entitiesLayer');

  // Ensure Boss Claude element exists
  if (state.boss) {
    let bossEl = document.getElementById('entity-claude');
    if (!bossEl) {
      bossEl = document.createElement('div');
      bossEl.id = 'entity-claude';
      bossEl.className = 'iso-entity boss-entity';
      bossEl.style.setProperty('--entity-color', '#f59e0b');
      bossEl.innerHTML = `
        <div class="entity-speech hidden" id="speech-claude"></div>
        <div class="entity-hud">
          <div class="entity-hud-top">
            <span class="entity-stars">👑</span>
            <span class="entity-name">SẾP CLAUDE</span>
          </div>
          <div class="entity-role-badge">Giám Đốc Điều Phối AI</div>
        </div>
        <div class="entity-sprite-wrap">
          <img class="entity-sprite-img" id="img-claude" src="${getEntityImageSrc(state.boss, true)}" alt="Sếp Claude" />
        </div>
        <div class="entity-pedestal"></div>
      `;
      bossEl.addEventListener('click', (e) => {
        e.stopPropagation();
        showEntitySpeech('claude', 'Tôi là Sếp Claude! Hãy nhập lệnh bên trái để tôi phân tích và giao việc cho 4 Linh Thú Tí Nị!', 3500);
        spawnShockwave(state.boss.currentPos.x, state.boss.currentPos.y, '#f59e0b');
      });
      layer.appendChild(bossEl);
    }

    bossEl.style.left = `${state.boss.currentPos.x}%`;
    bossEl.style.top = `${state.boss.currentPos.y}%`;
    bossEl.className = `iso-entity boss-entity state-${state.boss.state || 'idle'}`;
    const bossImg = document.getElementById('img-claude');
    if (bossImg) bossImg.src = getEntityImageSrc(state.boss, true);
  }

  // Ensure all 4 TFT Agent elements exist and update positions/sprites
  state.agents.forEach(agent => {
    let el = document.getElementById(`entity-${agent.id}`);
    if (!el) {
      el = document.createElement('div');
      el.id = `entity-${agent.id}`;
      el.className = 'iso-entity';
      el.style.setProperty('--entity-color', agent.themeColor);
      el.innerHTML = `
        <div class="entity-speech hidden" id="speech-${agent.id}"></div>
        <div class="entity-hud">
          <div class="entity-hud-top">
            <span class="entity-stars" id="stars-${agent.id}"></span>
            <span class="entity-name">${agent.name}</span>
            <span class="entity-angle-pill" id="angle-${agent.id}"></span>
          </div>
          <div class="entity-role-badge" id="role-${agent.id}"></div>
        </div>
        <div class="entity-sprite-wrap">
          <img class="entity-sprite-img" id="img-${agent.id}" src="" alt="${agent.name}" />
        </div>
        <div class="entity-pedestal"></div>
      `;
      el.addEventListener('click', (e) => {
        e.stopPropagation();
        state.selectedAgentId = agent.id;
        renderRoster();
        renderInspector();
        spawnShockwave(agent.currentPos.x, agent.currentPos.y, agent.themeColor);
        showEntitySpeech(agent.id, `${agent.name}: "${agent.quote}"`, 3500);
      });
      layer.appendChild(el);
    }

    el.style.left = `${agent.currentPos.x}%`;
    el.style.top = `${agent.currentPos.y}%`;
    el.className = `iso-entity state-${agent.state || 'idle'}`;

    document.getElementById(`stars-${agent.id}`).textContent = '★'.repeat(agent.starLevel);
    document.getElementById(`angle-${agent.id}`).textContent = (agent.currentAngle || 'ctrl1').toUpperCase();
    document.getElementById(`role-${agent.id}`).textContent = `Sửa ${agent.fileExtension.toUpperCase()} • ${agent.currentTaskTitle}`;

    const imgEl = document.getElementById(`img-${agent.id}`);
    if (imgEl) {
      imgEl.src = getEntityImageSrc(agent, false);
      imgEl.className = `entity-sprite-img ${state.renderMode === 'video_angle' ? 'video-angle-mode' : ''}`;
    }
  });
}

function showEntitySpeech(actorId, text, durationMs = 3000) {
  const bubble = document.getElementById(`speech-${actorId}`);
  if (!bubble) return;
  bubble.textContent = text;
  bubble.classList.remove('hidden');

  if (state.speechTimers[actorId]) {
    clearTimeout(state.speechTimers[actorId]);
  }
  state.speechTimers[actorId] = setTimeout(() => {
    bubble.classList.add('hidden');
  }, durationMs);
}

function renderInspector() {
  const agent = state.agents.find(a => a.id === state.selectedAgentId) || state.agents[0];
  if (!agent) return;

  const container = document.getElementById('agentInspector');
  const stars = '★'.repeat(agent.starLevel);

  container.innerHTML = `
    <div class="insp-top">
      <img src="${agent.sprites.portrait}" alt="${agent.name}" class="insp-portrait" style="border-color: ${agent.themeColor}" />
      <div class="insp-title">
        <h3>#${agent.videoOrder} ${agent.name} <span style="color:#fbbf24">${stars}</span></h3>
        <div class="insp-sub">${agent.role} (${agent.patchVersion})</div>
        <a href="${agent.videoUrl}" target="_blank" rel="noopener" class="insp-patch-link">
          ▶️ Xem Video Gốc ĐTCL Trên YouTube ↗
        </a>
      </div>
    </div>

    <div style="display:flex; gap:5px; flex-wrap:wrap;">
      <button class="btn-mini" onclick="triggerAgentAction('${agent.id}', 'ctrl1')">Góc Ctrl+1</button>
      <button class="btn-mini" onclick="triggerAgentAction('${agent.id}', 'ctrl2')">Góc Ctrl+2</button>
      <button class="btn-mini" onclick="triggerAgentAction('${agent.id}', 'ctrl3')">Góc Ctrl+3</button>
      <button class="btn-mini" onclick="triggerAgentAction('${agent.id}', 'ctrl4')">Góc Ctrl+4 (Ult)</button>
      <button class="btn-mini" onclick="triggerAgentAction('${agent.id}', 'star_up')">⭐ Nâng Sao</button>
      <button class="btn-mini" style="border-color:${agent.themeColor};color:#fde68a;" onclick="playAgentFinisherPip('${agent.id}')">💥 Phát Finisher</button>
    </div>

    <div class="preset-label">6 Góc Hình Ảnh & Hiệu Ứng Cắt Từ Video ĐTCL (Click để áp dụng lên sàn):</div>
    <div class="angles-grid">
      ${['ctrl1', 'ctrl2', 'ctrl3', 'ctrl4', 'boom', 'finisher'].map(key => `
        <div class="angle-card ${agent.currentAngle === key ? 'active' : ''}" onclick="selectAgentVideoAngle('${agent.id}', '${key}')">
          <img src="${agent.sprites.videoAngles[key]}" alt="${key}" />
          <div class="angle-label">${key.toUpperCase()}</div>
        </div>
      `).join('')}
    </div>

    <div class="insp-video-box">
      <div class="insp-video-tabs">
        <button class="vtab-btn active" onclick="switchInspectorVideo('${agent.id}', 'finisher', this)">🎬 Kết Liễu (Finisher)</button>
        <button class="vtab-btn" onclick="switchInspectorVideo('${agent.id}', 'showcase', this)">🎥 4 Góc (Ctrl+1..4)</button>
        <button class="vtab-btn" onclick="switchInspectorVideo('${agent.id}', 'boom', this)">☄️ Chưởng Lực (Boom)</button>
      </div>
      <video id="inspectorVideoPlayer" class="insp-video-player" src="${agent.sprites.videoClips.finisher}" controls autoplay muted loop playsinline></video>
    </div>
  `;
}

window.selectAgentVideoAngle = async function(agentId, angleKey) {
  await triggerAgentAction(agentId, angleKey === 'finisher' || angleKey === 'boom' ? 'ctrl4' : angleKey);
  const agent = state.agents.find(a => a.id === agentId);
  if (agent) {
    agent.currentAngle = angleKey;
    renderOfficeEntities();
    renderInspector();
  }
};

window.switchInspectorVideo = function(agentId, clipType, btnEl) {
  const agent = state.agents.find(a => a.id === agentId);
  if (!agent) return;
  document.querySelectorAll('.vtab-btn').forEach(b => b.classList.remove('active'));
  btnEl.classList.add('active');
  const video = document.getElementById('inspectorVideoPlayer');
  if (video) {
    video.src = agent.sprites.videoClips[clipType];
    video.play().catch(() => {});
  }
};

window.triggerAgentAction = async function(agentId, action, targetX = null, targetY = null) {
  try {
    const res = await fetch(`/api/agent/${agentId}/action`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ action, targetX, targetY })
    });
    if (!res.ok) return;
    const updated = await res.json();
    const idx = state.agents.findIndex(a => a.id === agentId);
    if (idx >= 0) state.agents[idx] = updated;

    renderRoster();
    renderOfficeEntities();
    if (state.selectedAgentId === agentId) renderInspector();

    spawnShockwave(updated.currentPos.x, updated.currentPos.y, updated.themeColor);
    showEntitySpeech(agentId, `${updated.name} kích hoạt [${action.toUpperCase()}]!`, 2400);
  } catch (e) {
    console.error(e);
  }
};

window.playAgentFinisherPip = function(agentId, customCaption = null) {
  const agent = state.agents.find(a => a.id === agentId);
  if (!agent) return;

  const pip = document.getElementById('finisherPip');
  const video = document.getElementById('finisherPipVideo');
  const title = document.getElementById('finisherPipTitle');
  const caption = document.getElementById('finisherPipCaption');

  title.textContent = `💥 ${agent.name.toUpperCase()} — ${agent.patchVersion}`;
  caption.textContent = customCaption || `${agent.finisherName} (${agent.boomName})`;
  video.src = agent.sprites.videoClips.finisher;
  pip.classList.remove('hidden');
  video.currentTime = 0;
  video.play().catch(() => {});
};

function renderVault() {
  const list = document.getElementById('vaultList');
  const badge = document.getElementById('docCountBadge');
  badge.textContent = `${state.documents.length} files`;
  list.innerHTML = '';

  state.documents.forEach(doc => {
    const agent = state.agents.find(a => a.id === doc.createdByAgentId);
    const color = agent ? agent.themeColor : '#c8aa6e';
    const item = document.createElement('div');
    item.className = 'vault-item';
    item.style.borderLeft = `3px solid ${color}`;
    item.innerHTML = `
      <div class="vault-item-top">
        <span class="vault-filename">${doc.fileName}</span>
        <span class="ext-badge" style="color:${color}">.${doc.fileType.toUpperCase()}</span>
      </div>
      <div class="vault-agent-tag">Xử lý bởi: <strong>${doc.createdByAgentName}</strong> • ${(doc.sizeBytes / 1024).toFixed(1)} KB</div>
      <div class="vault-actions">
        <button class="btn-vault" onclick="openDocPreview('${doc.fileId}')">👁️ Xem Nội Dung</button>
        <a href="${doc.downloadUrl}" download="${doc.fileName}" class="btn-vault">⬇️ Tải File (.${doc.fileType})</a>
      </div>
    `;
    list.appendChild(item);
  });
}

window.openDocPreview = function(fileId) {
  const doc = state.documents.find(d => d.fileId === fileId);
  if (!doc) return;
  document.getElementById('previewModalTitle').textContent = `📄 ${doc.fileName}`;
  document.getElementById('previewModalMeta').textContent = `Phụ trách: ${doc.createdByAgentName} | Định dạng: .${doc.fileType.toUpperCase()} | Dung lượng: ${doc.sizeBytes} bytes`;
  document.getElementById('previewModalContent').textContent = doc.previewText;
  const dlBtn = document.getElementById('previewModalDownloadBtn');
  dlBtn.href = doc.downloadUrl;
  dlBtn.setAttribute('download', doc.fileName);
  document.getElementById('docPreviewModal').classList.remove('hidden');
};

async function sendCommandToBossClaude(customCommand = null, customTarget = null) {
  const cmdInput = document.getElementById('commandInput');
  const commandText = customCommand ?? cmdInput.value.trim();
  const targetVal = customTarget ?? document.getElementById('targetAgentSelect').value;
  const triggerFinisher = document.getElementById('chkFinisherVideo').checked;

  const btn = document.getElementById('btnSendCommand');
  btn.disabled = true;
  btn.innerHTML = '<span>⏳ SẾP CLAUDE ĐANG PHÂN TÍCH & ĐIỀU PHỐI LINH THÚ...</span>';
  document.getElementById('timelineStatusBadge').textContent = 'Đang thực thi chiến thuật...';

  try {
    const res = await fetch('/api/command', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        command: commandText,
        targetAgent: targetVal,
        attachedFileName: state.attachedFileName,
        attachedContent: state.attachedContent,
        triggerFinisher
      })
    });

    const orchestration = await res.json();
    appendTimelineLog('👑 Sếp Claude', orchestration.claudeAnalysis, '#f59e0b');

    // Play the interactive 3D Isometric Office timeline!
    await playOrchestrationTimeline(orchestration, triggerFinisher);

    // Refresh documents & state
    await fetchOfficeState();
    document.getElementById('timelineStatusBadge').textContent = `Hoàn tất (${orchestration.generatedFiles.length} tài liệu mới)`;
  } catch (err) {
    console.error('Command error:', err);
    appendTimelineLog('⚠️ Hệ thống', 'Có lỗi khi gửi lệnh tới C# Core.', '#ef4444');
  } finally {
    btn.disabled = false;
    btn.innerHTML = '<span>🚀 GỬI LỆNH CHO SẾP CLAUDE PHÂN TÍCH & ĐIỀU PHỐI</span>';
  }
}

async function playOrchestrationTimeline(orchestration, showFinisherVideo) {
  let prevDelay = 0;

  for (const step of orchestration.timeline) {
    const waitTime = Math.max(250, step.delayMs - prevDelay);
    prevDelay = step.delayMs;
    await sleep(waitTime);

    appendTimelineLog(step.actorId.toUpperCase(), step.message, step.effectColor || '#c8aa6e');

    if (step.actorId === 'claude') {
      if (step.toPos) {
        state.boss.currentPos = step.toPos;
      }
      state.boss.state = step.pose === 'work' ? 'working' : step.pose === 'ult' ? 'ult' : 'idle';
      renderOfficeEntities();
      showEntitySpeech('claude', step.message, 2800);

      if (step.eventType === 'boom_dispatch' && step.fromPos && step.toPos) {
        spawnBoomProjectile(step.fromPos.x, step.fromPos.y, step.toPos.x, step.toPos.y, step.effectColor || '#f59e0b');
      }
    } else {
      const agent = state.agents.find(a => a.id === step.actorId);
      if (agent) {
        if (step.toPos) agent.currentPos = step.toPos;
        agent.state = step.pose === 'walk' ? 'walking' : step.pose === 'work' ? 'working' : step.pose === 'ult' ? 'ult' : 'idle';
        agent.currentAngle = step.angle || 'ctrl1';
        renderOfficeEntities();
        showEntitySpeech(agent.id, step.message, 2800);
        spawnShockwave(agent.currentPos.x, agent.currentPos.y, agent.themeColor);

        if (step.eventType === 'agent_finisher') {
          spawnBoomProjectile(agent.currentPos.x, agent.currentPos.y, 79.0, 19.5, agent.themeColor);
          if (showFinisherVideo) {
            playAgentFinisherPip(agent.id, step.message);
          }
        }
      }
    }
  }
}

function appendTimelineLog(actor, message, color = '#c8aa6e') {
  const feed = document.getElementById('timelineFeed');
  const now = new Date().toLocaleTimeString('vi-VN', { hour12: false });
  const entry = document.createElement('div');
  entry.className = 'log-entry';
  entry.style.borderLeftColor = color;
  entry.innerHTML = `
    <span class="log-time">[${now}]</span>
    <div><strong style="color:${color}">${actor}:</strong> ${message}</div>
  `;
  feed.prepend(entry);
}

// Canvas Particle & TFT Boom Projectile Engine
function setupCanvasLoop() {
  const resize = () => {
    const rect = fxCanvas.parentElement.getBoundingClientRect();
    fxCanvas.width = rect.width || 960;
    fxCanvas.height = rect.height || 640;
  };
  window.addEventListener('resize', resize);
  setTimeout(resize, 100);

  function animate() {
    fxCtx.clearRect(0, 0, fxCanvas.width, fxCanvas.height);

    // Update & draw projectiles (Chưởng Lực)
    for (let i = state.projectiles.length - 1; i >= 0; i--) {
      const p = state.projectiles[i];
      p.progress += 0.045;
      if (p.progress >= 1) {
        spawnShockwave(p.toXPercent, p.toYPercent, p.color);
        state.projectiles.splice(i, 1);
        continue;
      }

      const curX = (p.fromX + (p.toX - p.fromX) * p.progress);
      const curY = (p.fromY + (p.toY - p.fromY) * p.progress) - Math.sin(p.progress * Math.PI) * 48;

      // Trail particles
      state.particles.push({
        x: curX,
        y: curY,
        vx: (Math.random() - 0.5) * 2.2,
        vy: (Math.random() - 0.5) * 2.2,
        radius: Math.random() * 4 + 2,
        alpha: 1,
        color: p.color
      });

      fxCtx.save();
      fxCtx.beginPath();
      fxCtx.arc(curX, curY, 8, 0, Math.PI * 2);
      fxCtx.fillStyle = '#ffffff';
      fxCtx.shadowColor = p.color;
      fxCtx.shadowBlur = 18;
      fxCtx.fill();
      fxCtx.restore();
    }

    // Update & draw shockwaves
    for (let i = state.shockwaves.length - 1; i >= 0; i--) {
      const s = state.shockwaves[i];
      s.radius += 2.6;
      s.alpha -= 0.032;
      if (s.alpha <= 0) {
        state.shockwaves.splice(i, 1);
        continue;
      }
      fxCtx.save();
      fxCtx.beginPath();
      fxCtx.ellipse(s.x, s.y, s.radius, s.radius * 0.55, 0, 0, Math.PI * 2);
      fxCtx.strokeStyle = s.color;
      fxCtx.globalAlpha = s.alpha;
      fxCtx.lineWidth = 3;
      fxCtx.stroke();
      fxCtx.restore();
    }

    // Update & draw spark particles
    for (let i = state.particles.length - 1; i >= 0; i--) {
      const pt = state.particles[i];
      pt.x += pt.vx;
      pt.y += pt.vy;
      pt.alpha -= 0.04;
      if (pt.alpha <= 0) {
        state.particles.splice(i, 1);
        continue;
      }
      fxCtx.save();
      fxCtx.beginPath();
      fxCtx.arc(pt.x, pt.y, pt.radius, 0, Math.PI * 2);
      fxCtx.fillStyle = pt.color;
      fxCtx.globalAlpha = pt.alpha;
      fxCtx.fill();
      fxCtx.restore();
    }

    requestAnimationFrame(animate);
  }
  requestAnimationFrame(animate);
}

function spawnBoomProjectile(fromXPercent, fromYPercent, toXPercent, toYPercent, color) {
  const w = fxCanvas.width;
  const h = fxCanvas.height;
  state.projectiles.push({
    fromX: (fromXPercent / 100) * w,
    fromY: (fromYPercent / 100) * h,
    toX: (toXPercent / 100) * w,
    toY: (toYPercent / 100) * h,
    toXPercent,
    toYPercent,
    progress: 0,
    color
  });
}

function spawnShockwave(xPercent, yPercent, color) {
  const w = fxCanvas.width;
  const h = fxCanvas.height;
  state.shockwaves.push({
    x: (xPercent / 100) * w,
    y: (yPercent / 100) * h,
    radius: 8,
    alpha: 0.95,
    color
  });
}

function bindUIEvents() {
  document.getElementById('btnSendCommand').addEventListener('click', () => sendCommandToBossClaude());

  document.getElementById('btnDispatchAllQuick').addEventListener('click', () => {
    const cmd = 'Sếp Claude hãy phân tích và điều phối toàn bộ 4 Linh Thú Tí Nị: T1 Galio sửa & đóng dấu PDF, Zeri sửa hợp đồng Word, Jhin lập bảng tính Excel 4 Quý, và Jinx thiết kế Slide PowerPoint thuyết trình!';
    document.getElementById('commandInput').value = cmd;
    document.getElementById('targetAgentSelect').value = 'all';
    sendCommandToBossClaude(cmd, 'all');
  });

  document.querySelectorAll('.preset-chip').forEach(btn => {
    btn.addEventListener('click', () => {
      const cmd = btn.getAttribute('data-cmd');
      const target = btn.getAttribute('data-target');
      document.getElementById('commandInput').value = cmd;
      document.getElementById('targetAgentSelect').value = target;
      sendCommandToBossClaude(cmd, target);
    });
  });

  // Render Mode Toggle (Sprite vs Video Angles)
  document.querySelectorAll('#renderModeToggle .seg-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('#renderModeToggle .seg-btn').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      state.renderMode = btn.getAttribute('data-mode');
      renderOfficeEntities();
    });
  });

  // Cycle Ctrl+1..4 for all agents
  const angles = ['ctrl1', 'ctrl2', 'ctrl3', 'ctrl4'];
  let angleCounter = 0;
  document.getElementById('btnAllEmoteCycle').addEventListener('click', () => {
    angleCounter = (angleCounter + 1) % angles.length;
    const nextAngle = angles[angleCounter];
    state.agents.forEach(a => triggerAgentAction(a.id, nextAngle));
  });

  // Reset positions to desks
  document.getElementById('btnResetPositions').addEventListener('click', () => {
    state.agents.forEach(a => triggerAgentAction(a.id, 'reset_desk'));
  });

  // Click anywhere on the 3D Isometric Office Map to walk the selected agent there
  const mapContainer = document.getElementById('officeMapContainer');
  mapContainer.addEventListener('click', (e) => {
    const rect = mapContainer.getBoundingClientRect();
    const xPct = ((e.clientX - rect.left) / rect.width) * 100;
    const yPct = ((e.clientY - rect.top) / rect.height) * 100;
    if (state.selectedAgentId) {
      triggerAgentAction(state.selectedAgentId, 'walk', xPct, yPct);
    }
  });

  // File attachment handler
  document.getElementById('fileAttachInput').addEventListener('change', async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    state.attachedFileName = file.name;
    document.getElementById('attachedFileLabel').textContent = `Đã đính kèm: ${file.name}`;
    try {
      const text = await file.text();
      state.attachedContent = text.slice(0, 600);
    } catch {
      state.attachedContent = `[Binary File Attached: ${file.name} (${file.size} bytes)]`;
    }
  });

  // Close modals & PIP
  document.getElementById('btnClosePip').addEventListener('click', () => {
    document.getElementById('finisherPip').classList.add('hidden');
    document.getElementById('finisherPipVideo').pause();
  });

  document.getElementById('btnCloseDocModal').addEventListener('click', () => {
    document.getElementById('docPreviewModal').classList.add('hidden');
  });
  document.getElementById('btnCloseDocModal2').addEventListener('click', () => {
    document.getElementById('docPreviewModal').classList.add('hidden');
  });

  document.getElementById('btnShowPlaylistModal').addEventListener('click', openPlaylistGalleryModal);
  document.getElementById('btnCloseGalleryModal').addEventListener('click', () => {
    document.getElementById('playlistGalleryModal').classList.add('hidden');
  });
}

function openPlaylistGalleryModal() {
  const body = document.getElementById('galleryModalBody');
  body.innerHTML = `
    <p style="font-size:12.5px; color:#cbd5e1; margin-bottom:12px;">
      Toàn bộ 4 AI Agent được trích xuất hình ảnh các góc (<strong>Ctrl+1, Ctrl+2, Ctrl+3, Ctrl+4</strong>), <strong>Chưởng Lực (Boom)</strong> và <strong>Hiệu Ứng Kết Liễu (Finisher)</strong> từ 4 video đầu tiên trong danh sách phát YouTube Đấu Trường Chân Lý:
    </p>
    <div class="gallery-grid">
      ${state.agents.map(a => `
        <div class="gallery-agent-card" style="border-color:${a.themeColor}">
          <div style="display:flex; justify-content:space-between; align-items:center;">
            <strong style="color:#fde68a; font-size:14px;">Video #${a.videoOrder}: ${a.name} — ${a.role}</strong>
            <a href="${a.videoUrl}" target="_blank" style="color:#38bdf8; font-size:11px;">Xem YouTube ↗</a>
          </div>
          <div style="font-size:11px; color:#94a3b8;">${a.boomName} • ${a.finisherName}</div>
          <div class="gallery-angles-row">
            <div><img src="${a.sprites.videoAngles.ctrl1}" alt="Ctrl+1"/><div class="angle-label">Góc Ctrl+1</div></div>
            <div><img src="${a.sprites.videoAngles.ctrl2}" alt="Ctrl+2"/><div class="angle-label">Góc Ctrl+2</div></div>
            <div><img src="${a.sprites.videoAngles.ctrl3}" alt="Ctrl+3"/><div class="angle-label">Góc Ctrl+3</div></div>
            <div><img src="${a.sprites.videoAngles.ctrl4}" alt="Ctrl+4"/><div class="angle-label">Góc Ctrl+4</div></div>
          </div>
          <div style="display:grid; grid-template-columns: 1fr 1fr; gap:8px; margin-top:4px;">
            <div>
              <div class="angle-label">Sprite 4 Góc Isometric (Tách nền)</div>
              <img src="${a.sprites.sheet}" style="width:100%; height:130px; object-fit:cover; border-radius:6px;" />
            </div>
            <div>
              <div class="angle-label">Video Kết Liễu Thực Tế (Finisher MP4)</div>
              <video src="${a.sprites.videoClips.finisher}" controls muted loop style="width:100%; height:130px; object-fit:cover; border-radius:6px; background:#000;"></video>
            </div>
          </div>
        </div>
      `).join('')}
    </div>
  `;
  document.getElementById('playlistGalleryModal').classList.remove('hidden');
}

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

document.addEventListener('DOMContentLoaded', initApp);
