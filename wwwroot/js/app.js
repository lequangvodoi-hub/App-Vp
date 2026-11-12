// ============================================================================
// FULL 3D WEBGL OFFICE & RIGGED 3D CHIBI CHARACTER ENGINE (THREE.JS)
// No champion names — pure 3D character models styled after the 4 reference designs
// ============================================================================

const state = {
  boss: {
    id: 'claude',
    name: 'Sếp Claude',
    role: 'Giám Đốc Phân Tích & Điều Phối',
    officePos: { x: -7.5, z: -7.2 },
    conferencePos: { x: 0.0, z: -7.2 },
    currentPos: { x: -7.5, z: -7.2 },
    targetPos: { x: -7.5, z: -7.2 },
    pose: 'idle',
    commandsDispatched: 0,
    currentThought: 'Sẵn sàng phân tích yêu cầu và bắn Chưởng Lực 3D giao việc cho 4 AI Agent.',
    speechText: ''
  },
  agents: [
    {
      id: 'galio',
      order: 1,
      name: 'AI Agent PDF',
      role: 'Chuyên Viên Sửa & Chuẩn Hoá PDF (.pdf)',
      fileExtension: '.pdf',
      themeColor: '#e11d48',
      secondaryColor: '#f59e0b',
      starLevel: 3,
      mana: 85,
      homeDesk: { x: -6.5, z: -0.5 },
      currentPos: { x: -6.5, z: -0.5 },
      targetPos: { x: -6.5, z: -0.5 },
      pose: 'idle',
      currentTaskTitle: 'Trực tại Bàn 3D #1 (Khu PDF)',
      quote: 'Mọi trang PDF đều được căn chuẩn A4 và đóng dấu bảo mật 3D!',
      speechText: ''
    },
    {
      id: 'zeri',
      order: 2,
      name: 'AI Agent Word',
      role: 'Chuyên Viên Sửa Word & Hợp Đồng (.docx)',
      fileExtension: '.docx',
      themeColor: '#06b6d4',
      secondaryColor: '#a855f7',
      starLevel: 2,
      mana: 75,
      homeDesk: { x: -2.2, z: -0.5 },
      currentPos: { x: -2.2, z: -0.5 },
      targetPos: { x: -2.2, z: -0.5 },
      pose: 'idle',
      currentTaskTitle: 'Trực tại Bàn 3D #2 (Khu Word)',
      quote: 'Tốc độ soạn thảo 1.000 từ/giây! Mọi lỗi văn bản .docx được xử lý tức thì!',
      speechText: ''
    },
    {
      id: 'jhin',
      order: 3,
      name: 'AI Agent Excel',
      role: 'Chuyên Viên Sửa Excel & Số Liệu (.xlsx)',
      fileExtension: '.xlsx',
      themeColor: '#dc2626',
      secondaryColor: '#fbbf24',
      starLevel: 3,
      mana: 90,
      homeDesk: { x: 2.2, z: -0.5 },
      currentPos: { x: 2.2, z: -0.5 },
      targetPos: { x: 2.2, z: -0.5 },
      pose: 'idle',
      currentTaskTitle: 'Trực tại Bàn 3D #3 (Khu Excel)',
      quote: 'Mọi bảng tính Excel 4 Quý và công thức đều đạt độ chuẩn xác 100%!',
      speechText: ''
    },
    {
      id: 'jinx',
      order: 4,
      name: 'AI Agent PowerPoint',
      role: 'Chuyên Viên Sửa PowerPoint & Slide (.pptx)',
      fileExtension: '.pptx',
      themeColor: '#38bdf8',
      secondaryColor: '#f43f5e',
      starLevel: 2,
      mana: 95,
      homeDesk: { x: 6.5, z: -0.5 },
      currentPos: { x: 6.5, z: -0.5 },
      targetPos: { x: 6.5, z: -0.5 },
      pose: 'idle',
      currentTaskTitle: 'Trực tại Bàn 3D #4 (Khu PPT)',
      quote: 'Thiết kế bộ Slide thuyết trình PowerPoint 16:9 sống động và bùng nổ!',
      speechText: ''
    }
  ],
  selectedAgentId: 'galio',
  documents: [],
  attachedFileName: null,
  attachedContent: null,
  speechTimers: {}
};

// Three.js Main Scene Globals
let scene, camera, renderer, controls, clock;
let floorMesh;
const characterRigs = {}; // id -> { root, head, body, leftArm, rightArm, leftLeg, rightLeg, wingL, wingR, weapon, auraRing, pointLight, labelEl }
const activeProjectiles3D = [];
const activeShockwaves3D = [];

// Inspector 3D Turntable Globals
let inspScene, inspCamera, inspRenderer, inspControls, inspRig;

async function initApp() {
  initMain3DScene();
  initInspector3DScene();
  bindUIEvents();
  await fetchBackendState();
  renderRoster();
  updateInspectorUI();
  appendTimelineLog('👑 Sếp Claude', 'Không gian Văn Phòng 3D WebGL & 4 Model Nhân Vật 3D đã khởi tạo! Giữ chuột trái để xoay góc nhìn 360°, lăn chuột để Zoom hoặc gửi lệnh điều phối.', '#f59e0b');
}

async function fetchBackendState() {
  try {
    const res = await fetch('/api/state');
    if (!res.ok) throw new Error('Offline fallback');
    const data = await res.json();
    if (data.documents) {
      state.documents = data.documents;
      renderVault();
    }
  } catch {
    seedFallbackDocuments();
    renderVault();
  }
}

function seedFallbackDocuments() {
  if (state.documents.length > 0) return;
  createLocalDoc('galio', 'Agent_PDF_Report_Q4.pdf', 'pdf', 'Chuẩn hoá bố cục A4 & đóng dấu bảo mật PDF');
  createLocalDoc('zeri', 'Agent_Word_Contract.docx', 'docx', 'Soạn thảo & biên tập hợp đồng thương mại Word');
  createLocalDoc('jhin', 'Agent_Excel_Finance_4Q.xlsx', 'xlsx', 'Lập bảng tính tài chính & công thức KPI 4 Quý');
  createLocalDoc('jinx', 'Agent_PowerPoint_Deck.pptx', 'pptx', 'Thiết kế bộ Slide thuyết trình 16:9');
}

function createLocalDoc(agentId, fileName, fileType, instruction) {
  const agent = state.agents.find(a => a.id === agentId);
  const doc = {
    fileId: Math.random().toString(36).slice(2, 9),
    fileName,
    fileType,
    createdByAgentId: agentId,
    createdByAgentName: agent ? agent.name : 'AI Agent 3D',
    sizeBytes: 5120 + Math.floor(Math.random() * 4096),
    downloadUrl: `/workspace/${fileName}`,
    previewText: `[TÀI LIỆU ĐÃ XỬ LÝ BỞI ${agent ? agent.name.toUpperCase() : 'AI AGENT'}]\nTên file: ${fileName}\nChỉ thị từ Sếp Claude: ${instruction}\nTrạng thái: Đã chuẩn hoá cấu trúc và xác thực hoàn tất 100%.`
  };
  state.documents.unshift(doc);
  return doc;
}

// ============================================================================
// 1. BUILD FULL 3D ISOMETRIC OFFICE ARCHITECTURE IN THREE.JS
// ============================================================================
function initMain3DScene() {
  const canvas = document.getElementById('threeCanvas');
  const viewport = document.getElementById('webglViewport');

  scene = new THREE.Scene();
  scene.background = new THREE.Color(0x07101e);
  scene.fog = new THREE.FogExp2(0x07101e, 0.018);

  camera = new THREE.PerspectiveCamera(42, viewport.clientWidth / viewport.clientHeight, 0.1, 200);
  camera.position.set(0, 18.5, 21.5);

  renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
  renderer.setSize(viewport.clientWidth, viewport.clientHeight);
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = THREE.PCFSoftShadowMap;

  controls = new THREE.OrbitControls(camera, renderer.domElement);
  controls.target.set(0, 0.8, -1.5);
  controls.enableDamping = true;
  controls.dampingFactor = 0.06;
  controls.maxPolarAngle = Math.PI / 2 - 0.06;
  controls.minDistance = 7;
  controls.maxDistance = 38;

  // Lighting
  const hemiLight = new THREE.HemisphereLight(0xdbeafe, 0x1e293b, 0.75);
  scene.add(hemiLight);

  const dirLight = new THREE.DirectionalLight(0xfffbeb, 1.15);
  dirLight.position.set(14, 24, 16);
  dirLight.castShadow = true;
  dirLight.shadow.mapSize.width = 2048;
  dirLight.shadow.mapSize.height = 2048;
  dirLight.shadow.camera.near = 1;
  dirLight.shadow.camera.far = 60;
  const d = 16;
  dirLight.shadow.camera.left = -d;
  dirLight.shadow.camera.right = d;
  dirLight.shadow.camera.top = d;
  dirLight.shadow.camera.bottom = -d;
  scene.add(dirLight);

  build3DOfficeEnvironment();

  // Build 3D Character Rigs: Boss Claude + 4 Specialist Models
  createCharacterIntoScene('claude', state.boss.currentPos.x, state.boss.currentPos.z, '#f59e0b');
  state.agents.forEach(agent => {
    createCharacterIntoScene(agent.id, agent.currentPos.x, agent.currentPos.z, agent.themeColor);
  });

  // Raycaster for clicking on 3D floor or 3D models
  const raycaster = new THREE.Raycaster();
  const mouse = new THREE.Vector2();
  let pointerDownPos = { x: 0, y: 0 };

  canvas.addEventListener('pointerdown', (e) => {
    pointerDownPos = { x: e.clientX, y: e.clientY };
  });

  canvas.addEventListener('pointerup', (e) => {
    const dist = Math.hypot(e.clientX - pointerDownPos.x, e.clientY - pointerDownPos.y);
    if (dist > 6) return; // Was dragging camera

    const rect = canvas.getBoundingClientRect();
    mouse.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
    mouse.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;
    raycaster.setFromCamera(mouse, camera);

    // Check if clicked a character model
    const charRoots = Object.values(characterRigs).map(r => r.root);
    const charHits = raycaster.intersectObjects(charRoots, true);
    if (charHits.length > 0) {
      let obj = charHits[0].object;
      while (obj && !obj.userData.actorId && obj.parent) {
        obj = obj.parent;
      }
      if (obj && obj.userData.actorId) {
        const clickedId = obj.userData.actorId;
        if (clickedId !== 'claude') {
          state.selectedAgentId = clickedId;
          renderRoster();
          updateInspectorUI();
          const ag = state.agents.find(a => a.id === clickedId);
          if (ag) {
            spawn3DShockwave(ag.currentPos.x, ag.currentPos.z, ag.themeColor);
            showSpeech3D(ag.id, ag.quote, 3200);
          }
        } else {
          spawn3DShockwave(state.boss.currentPos.x, state.boss.currentPos.z, '#f59e0b');
          showSpeech3D('claude', 'Tôi là Sếp Claude! Hãy ra lệnh để tôi điều phối 4 Model 3D!', 3200);
        }
        return;
      }
    }

    // Check if clicked on floor to walk selected 3D model
    const floorHits = raycaster.intersectObject(floorMesh);
    if (floorHits.length > 0) {
      const pt = floorHits[0].point;
      const agent = state.agents.find(a => a.id === state.selectedAgentId);
      if (agent) {
        agent.targetPos = {
          x: Math.max(-9.8, Math.min(9.8, pt.x)),
          z: Math.max(-8.8, Math.min(7.5, pt.z))
        };
        agent.pose = 'walk';
        spawn3DShockwave(agent.targetPos.x, agent.targetPos.z, agent.themeColor);
      }
    }
  });

  window.addEventListener('resize', () => {
    if (!viewport.clientWidth || !viewport.clientHeight) return;
    camera.aspect = viewport.clientWidth / viewport.clientHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(viewport.clientWidth, viewport.clientHeight);
  });

  clock = new THREE.Clock();
  animateMain3D();
}

function build3DOfficeEnvironment() {
  const officeGroup = new THREE.Group();

  // Checkered/Parquet 3D Office Floor Base
  const floorGeo = new THREE.BoxGeometry(22, 0.5, 18);
  const floorMat = new THREE.MeshStandardMaterial({ color: 0x1e293b, roughness: 0.55, metalness: 0.15 });
  floorMesh = new THREE.Mesh(floorGeo, floorMat);
  floorMesh.position.set(0, -0.25, -1);
  floorMesh.receiveShadow = true;
  officeGroup.add(floorMesh);

  // Subtle Tactical Grid on Floor
  const gridHelper = new THREE.GridHelper(22, 22, 0xc8aa6e, 0x334155);
  gridHelper.position.set(0, 0.01, -1);
  gridHelper.material.opacity = 0.28;
  gridHelper.material.transparent = true;
  officeGroup.add(gridHelper);

  // Outer Low Cutaway Architectural Walls
  const wallMat = new THREE.MeshStandardMaterial({ color: 0x334155, roughness: 0.6 });
  const glassMat = new THREE.MeshPhysicalMaterial({
    color: 0x38bdf8,
    transparent: true,
    opacity: 0.24,
    roughness: 0.1,
    metalness: 0.1
  });

  // Back Wall (North)
  const backWall = new THREE.Mesh(new THREE.BoxGeometry(22.2, 3.2, 0.35), wallMat);
  backWall.position.set(0, 1.6, -10.0);
  backWall.castShadow = true;
  backWall.receiveShadow = true;
  officeGroup.add(backWall);

  // Left Wall (West)
  const leftWall = new THREE.Mesh(new THREE.BoxGeometry(0.35, 3.2, 18.2), wallMat);
  leftWall.position.set(-11.0, 1.6, -1.0);
  leftWall.castShadow = true;
  leftWall.receiveShadow = true;
  officeGroup.add(leftWall);

  // Right Wall (East)
  const rightWall = new THREE.Mesh(new THREE.BoxGeometry(0.35, 2.4, 18.2), wallMat);
  rightWall.position.set(11.0, 1.2, -1.0);
  rightWall.castShadow = true;
  officeGroup.add(rightWall);

  // 1. Boss Claude's Executive Glass Office (Top-Left: x: -10.8 to -4.2, z: -9.8 to -4.6)
  const bossRug = new THREE.Mesh(
    new THREE.BoxGeometry(5.8, 0.04, 4.6),
    new THREE.MeshStandardMaterial({ color: 0x78350f, roughness: 0.8 })
  );
  bossRug.position.set(-7.5, 0.02, -7.2);
  officeGroup.add(bossRug);

  const bossGlassRight = new THREE.Mesh(new THREE.BoxGeometry(0.12, 2.6, 5.2), glassMat);
  bossGlassRight.position.set(-4.3, 1.3, -7.2);
  officeGroup.add(bossGlassRight);

  const bossGlassFront = new THREE.Mesh(new THREE.BoxGeometry(6.5, 2.6, 0.12), glassMat);
  bossGlassFront.position.set(-7.5, 1.3, -4.6);
  officeGroup.add(bossGlassFront);

  // Boss Claude Executive Desk
  officeGroup.add(build3DDeskWorkstation(-7.5, -6.2, 0xf59e0b, true));

  // 2. Strategic Conference Room (Top-Center: x: -4.2 to +4.2, z: -9.8 to -4.6)
  const confRug = new THREE.Mesh(
    new THREE.BoxGeometry(7.4, 0.04, 4.6),
    new THREE.MeshStandardMaterial({ color: 0x1e3a8a, roughness: 0.8 })
  );
  confRug.position.set(0, 0.02, -7.2);
  officeGroup.add(confRug);

  // Conference Table + Hologram Projector in Center
  const confTable = new THREE.Mesh(
    new THREE.CylinderGeometry(1.8, 1.5, 0.75, 24),
    new THREE.MeshStandardMaterial({ color: 0x0f172a, metalness: 0.5, roughness: 0.3 })
  );
  confTable.position.set(0, 0.38, -7.2);
  confTable.castShadow = true;
  officeGroup.add(confTable);

  const holoCore = new THREE.Mesh(
    new THREE.OctahedronGeometry(0.42, 1),
    new THREE.MeshStandardMaterial({ color: 0x38bdf8, emissive: 0x0284c7, emissiveIntensity: 0.9, wireframe: true })
  );
  holoCore.position.set(0, 1.25, -7.2);
  holoCore.name = 'holoCore';
  officeGroup.add(holoCore);

  // 3. Portal & File Vault Archive (Top-Right: x: +4.5 to +10.5, z: -9.8 to -4.6)
  const portalArch = new THREE.Group();
  portalArch.position.set(7.5, 0, -7.2);
  const torus = new THREE.Mesh(
    new THREE.TorusGeometry(1.45, 0.14, 16, 40),
    new THREE.MeshStandardMaterial({ color: 0xc8aa6e, emissive: 0xf59e0b, emissiveIntensity: 0.6, metalness: 0.8 })
  );
  torus.position.y = 1.65;
  torus.name = 'vaultPortalRing';
  portalArch.add(torus);

  const portalDisc = new THREE.Mesh(
    new THREE.CircleGeometry(1.32, 32),
    new THREE.MeshBasicMaterial({ color: 0x38bdf8, transparent: true, opacity: 0.42, side: THREE.DoubleSide })
  );
  portalDisc.position.y = 1.65;
  portalArch.add(portalDisc);
  officeGroup.add(portalArch);

  // 4. Four Specialized 3D Workstations in Main Bullpen for the 4 AI Agent Models
  const deskConfigs = [
    { x: -6.5, z: 0.8, color: 0xe11d48 }, // Desk 1: PDF
    { x: -2.2, z: 0.8, color: 0x06b6d4 }, // Desk 2: Word
    { x: 2.2,  z: 0.8, color: 0xdc2626 }, // Desk 3: Excel
    { x: 6.5,  z: 0.8, color: 0x38bdf8 }  // Desk 4: PowerPoint
  ];
  deskConfigs.forEach(cfg => {
    officeGroup.add(build3DDeskWorkstation(cfg.x, cfg.z, cfg.color, false));
  });

  // Decorative 3D Potted Plants in corners
  [[-9.8, 6.2], [9.8, 6.2], [-9.8, -3.5], [9.8, -3.5]].forEach(([px, pz]) => {
    officeGroup.add(build3DPlant(px, pz));
  });

  scene.add(officeGroup);
}

function build3DDeskWorkstation(x, z, accentHex, isBoss) {
  const g = new THREE.Group();
  g.position.set(x, 0, z);

  // Desk Top
  const topW = isBoss ? 2.8 : 2.3;
  const topD = isBoss ? 1.3 : 1.1;
  const deskMat = new THREE.MeshStandardMaterial({ color: isBoss ? 0x451a03 : 0x1e293b, roughness: 0.4 });
  const topMesh = new THREE.Mesh(new THREE.BoxGeometry(topW, 0.12, topD), deskMat);
  topMesh.position.y = 0.82;
  topMesh.castShadow = true;
  topMesh.receiveShadow = true;
  g.add(topMesh);

  // Glowing Neon Trim on Desk Edge
  const trimMesh = new THREE.Mesh(
    new THREE.BoxGeometry(topW + 0.04, 0.04, topD + 0.04),
    new THREE.MeshBasicMaterial({ color: accentHex })
  );
  trimMesh.position.y = 0.85;
  g.add(trimMesh);

  // Desk Legs
  const legMat = new THREE.MeshStandardMaterial({ color: 0x0f172a, metalness: 0.7 });
  [[-topW / 2 + 0.15, -topD / 2 + 0.15], [topW / 2 - 0.15, -topD / 2 + 0.15],
   [-topW / 2 + 0.15, topD / 2 - 0.15], [topW / 2 - 0.15, topD / 2 - 0.15]].forEach(([lx, lz]) => {
    const leg = new THREE.Mesh(new THREE.CylinderGeometry(0.06, 0.06, 0.82, 8), legMat);
    leg.position.set(lx, 0.41, lz);
    g.add(leg);
  });

  // 3D Dual Monitor Setup on Desk
  const monStand = new THREE.Mesh(new THREE.CylinderGeometry(0.05, 0.12, 0.25, 10), legMat);
  monStand.position.set(0, 0.98, 0.2);
  g.add(monStand);

  const screenFrame = new THREE.Mesh(new THREE.BoxGeometry(1.15, 0.62, 0.06), legMat);
  screenFrame.position.set(0, 1.25, 0.2);
  g.add(screenFrame);

  const screenGlow = new THREE.Mesh(
    new THREE.PlaneGeometry(1.06, 0.54),
    new THREE.MeshBasicMaterial({ color: accentHex, side: THREE.DoubleSide })
  );
  screenGlow.position.set(0, 1.25, 0.16);
  g.add(screenGlow);

  return g;
}

function build3DPlant(x, z) {
  const g = new THREE.Group();
  g.position.set(x, 0, z);
  const pot = new THREE.Mesh(
    new THREE.CylinderGeometry(0.35, 0.26, 0.55, 12),
    new THREE.MeshStandardMaterial({ color: 0x9a3412, roughness: 0.7 })
  );
  pot.position.y = 0.28;
  g.add(pot);

  const foliageMat = new THREE.MeshStandardMaterial({ color: 0x10b981, roughness: 0.5 });
  const leaf1 = new THREE.Mesh(new THREE.DodecahedronGeometry(0.45, 1), foliageMat);
  leaf1.position.y = 0.85;
  g.add(leaf1);
  return g;
}

// ============================================================================
// 2. PROCEDURAL 3D CHIBI CHARACTER GENERATOR (BASED ON THE 4 VIDEO MODELS)
// ============================================================================
function buildChibi3DCharacterRig(actorId, primaryHex) {
  const root = new THREE.Group();
  root.userData.actorId = actorId;

  const skinMat = new THREE.MeshStandardMaterial({ color: 0xffe4d6, roughness: 0.45 });
  const darkMat = new THREE.MeshStandardMaterial({ color: 0x111827, roughness: 0.4, metalness: 0.4 });
  const goldMat = new THREE.MeshStandardMaterial({ color: 0xf59e0b, roughness: 0.25, metalness: 0.85 });

  // Glowing Tactical Pedestal Ring under feet
  const auraRing = new THREE.Mesh(
    new THREE.RingGeometry(0.55, 0.72, 32),
    new THREE.MeshBasicMaterial({ color: primaryHex, side: THREE.DoubleSide, transparent: true, opacity: 0.85 })
  );
  auraRing.rotation.x = -Math.PI / 2;
  auraRing.position.y = 0.03;
  root.add(auraRing);

  // Body / Torso Group
  const bodyGroup = new THREE.Group();
  bodyGroup.position.y = 0.75;
  root.add(bodyGroup);

  // Big Chibi Head Group (proportionally oversized like TFT Chibi models)
  const headGroup = new THREE.Group();
  headGroup.position.y = 0.72;
  bodyGroup.add(headGroup);

  // Arms & Legs
  const leftArm = new THREE.Group();
  leftArm.position.set(-0.42, 0.28, 0);
  bodyGroup.add(leftArm);

  const rightArm = new THREE.Group();
  rightArm.position.set(0.42, 0.28, 0);
  bodyGroup.add(rightArm);

  const leftLeg = new THREE.Group();
  leftLeg.position.set(-0.18, -0.32, 0);
  bodyGroup.add(leftLeg);

  const rightLeg = new THREE.Group();
  rightLeg.position.set(0.18, -0.32, 0);
  bodyGroup.add(rightLeg);

  let wingL = null, wingR = null, weapon = null;

  if (actorId === 'galio') {
    // MODEL #1 (PDF SPECIALIST): White/Crimson/Gold Armored Chibi Guardian with Giant Golden-Red Mecha Wings & Crown
    const armorMat = new THREE.MeshStandardMaterial({ color: 0xf8fafc, roughness: 0.25, metalness: 0.6 });
    const crimsonMat = new THREE.MeshStandardMaterial({ color: 0xe11d48, roughness: 0.3, metalness: 0.5 });

    // Torso Armor + Gold Chest Crest
    const torso = new THREE.Mesh(new THREE.CylinderGeometry(0.36, 0.28, 0.62, 16), armorMat);
    torso.castShadow = true;
    bodyGroup.add(torso);
    const chestCrest = new THREE.Mesh(new THREE.BoxGeometry(0.38, 0.25, 0.32), goldMat);
    chestCrest.position.set(0, 0.08, 0.14);
    bodyGroup.add(chestCrest);

    // Chibi Head + Golden Horns/Crown + Glowing Blue Eyes
    const headMesh = new THREE.Mesh(new THREE.BoxGeometry(0.76, 0.68, 0.68), armorMat);
    headMesh.castShadow = true;
    headGroup.add(headMesh);

    const visor = new THREE.Mesh(new THREE.BoxGeometry(0.80, 0.18, 0.42), goldMat);
    visor.position.set(0, 0.24, 0.18);
    headGroup.add(visor);

    // Swept-up Golden Crown Horns
    const hornGeo = new THREE.ConeGeometry(0.14, 0.58, 8);
    const hornL = new THREE.Mesh(hornGeo, goldMat);
    hornL.position.set(-0.34, 0.52, 0.08);
    hornL.rotation.z = 0.28;
    headGroup.add(hornL);
    const hornR = new THREE.Mesh(hornGeo, goldMat);
    hornR.position.set(0.34, 0.52, 0.08);
    hornR.rotation.z = -0.28;
    headGroup.add(hornR);

    const eyeMat = new THREE.MeshBasicMaterial({ color: 0x38bdf8 });
    [-0.17, 0.17].forEach(ex => {
      const eye = new THREE.Mesh(new THREE.SphereGeometry(0.075, 10, 10), eyeMat);
      eye.position.set(ex, 0.02, 0.35);
      headGroup.add(eye);
    });

    // Articulated Golden-Crimson Wings
    wingL = new THREE.Group();
    wingL.position.set(-0.25, 0.22, -0.22);
    wingR = new THREE.Group();
    wingR.position.set(0.25, 0.22, -0.22);
    for (let i = 0; i < 3; i++) {
      const featherL = new THREE.Mesh(new THREE.BoxGeometry(0.55 - i * 0.1, 0.16, 0.08), i === 0 ? goldMat : crimsonMat);
      featherL.position.set(-0.32 - i * 0.08, 0.18 - i * 0.18, 0);
      featherL.rotation.z = 0.35 - i * 0.15;
      wingL.add(featherL);

      const featherR = new THREE.Mesh(new THREE.BoxGeometry(0.55 - i * 0.1, 0.16, 0.08), i === 0 ? goldMat : crimsonMat);
      featherR.position.set(0.32 + i * 0.08, 0.18 - i * 0.18, 0);
      featherR.rotation.z = -0.35 + i * 0.15;
      wingR.add(featherR);
    }
    bodyGroup.add(wingL, wingR);

    // Arms + PDF Shield Emblem on Right Arm
    const armMeshL = new THREE.Mesh(new THREE.CylinderGeometry(0.12, 0.1, 0.45, 10), crimsonMat);
    armMeshL.position.y = -0.18;
    leftArm.add(armMeshL);
    const armMeshR = new THREE.Mesh(new THREE.CylinderGeometry(0.12, 0.1, 0.45, 10), crimsonMat);
    armMeshR.position.y = -0.18;
    rightArm.add(armMeshR);

    weapon = new THREE.Mesh(new THREE.CylinderGeometry(0.34, 0.34, 0.08, 6), goldMat);
    weapon.rotation.x = Math.PI / 2;
    weapon.position.set(0.12, -0.24, 0.2);
    rightArm.add(weapon);

    // Legs
    [leftLeg, rightLeg].forEach(leg => {
      const lMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.13, 0.11, 0.42, 10), armorMat);
      lMesh.position.y = -0.2;
      leg.add(lMesh);
    });

  } else if (actorId === 'zeri') {
    // MODEL #2 (WORD SPECIALIST): Tactical Jacket, Spiky Neon-Yellow/Cyan Ponytail, Visor Goggles & Lightning Gun
    const jacketMat = new THREE.MeshStandardMaterial({ color: 0x1e293b, roughness: 0.4 });
    const neonHairMat = new THREE.MeshStandardMaterial({ color: 0xd9f99d, emissive: 0x06b6d4, emissiveIntensity: 0.35 });
    const cyanMat = new THREE.MeshBasicMaterial({ color: 0x06b6d4 });

    const torso = new THREE.Mesh(new THREE.CylinderGeometry(0.30, 0.24, 0.58, 14), jacketMat);
    torso.castShadow = true;
    bodyGroup.add(torso);

    const headMesh = new THREE.Mesh(new THREE.SphereGeometry(0.42, 20, 20), skinMat);
    headMesh.castShadow = true;
    headGroup.add(headMesh);

    // Tactical Holographic Visor across eyes
    const goggles = new THREE.Mesh(
      new THREE.BoxGeometry(0.68, 0.18, 0.25),
      new THREE.MeshStandardMaterial({ color: 0x06b6d4, emissive: 0x06b6d4, emissiveIntensity: 0.6, transparent: true, opacity: 0.88 })
    );
    goggles.position.set(0, 0.04, 0.30);
    headGroup.add(goggles);

    // High Spiky Electric Ponytail
    const hairTop = new THREE.Mesh(new THREE.SphereGeometry(0.44, 16, 16, 0, Math.PI * 2, 0, Math.PI * 0.55), neonHairMat);
    hairTop.position.y = 0.04;
    headGroup.add(hairTop);

    const ponytail = new THREE.Mesh(new THREE.ConeGeometry(0.24, 0.68, 10), neonHairMat);
    ponytail.position.set(0, 0.55, -0.22);
    ponytail.rotation.x = -0.55;
    headGroup.add(ponytail);

    [leftArm, rightArm].forEach(arm => {
      const aMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.10, 0.09, 0.42, 10), jacketMat);
      aMesh.position.y = -0.18;
      arm.add(aMesh);
    });

    // Electric Pulse Blaster in Right Arm
    weapon = new THREE.Mesh(new THREE.BoxGeometry(0.14, 0.18, 0.58), cyanMat);
    weapon.position.set(0, -0.32, 0.24);
    rightArm.add(weapon);

    [leftLeg, rightLeg].forEach(leg => {
      const lMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.11, 0.10, 0.42, 10), darkMat);
      lMesh.position.y = -0.2;
      leg.add(lMesh);
    });

  } else if (actorId === 'jhin') {
    // MODEL #3 (EXCEL SPECIALIST): Crimson/Obsidian Robes, White Sculpted Mask with Glowing Red Eye, High Collar & Precision Cannon
    const robeMat = new THREE.MeshStandardMaterial({ color: 0x991b1b, roughness: 0.35, metalness: 0.3 });
    const maskMat = new THREE.MeshStandardMaterial({ color: 0xf8fafc, roughness: 0.2 });

    const torso = new THREE.Mesh(new THREE.CylinderGeometry(0.32, 0.34, 0.62, 16), robeMat);
    torso.castShadow = true;
    bodyGroup.add(torso);

    // High Shoulder Guard
    const shoulderPad = new THREE.Mesh(new THREE.BoxGeometry(0.86, 0.16, 0.38), goldMat);
    shoulderPad.position.set(0, 0.28, -0.04);
    bodyGroup.add(shoulderPad);

    // Hooded Head + White Artistic Mask
    const hood = new THREE.Mesh(new THREE.SphereGeometry(0.43, 18, 18), darkMat);
    headGroup.add(hood);
    const mask = new THREE.Mesh(new THREE.SphereGeometry(0.39, 18, 18), maskMat);
    mask.position.set(0, -0.02, 0.08);
    headGroup.add(mask);

    // Signature Glowing Red Right Eye & Golden Crest
    const redEye = new THREE.Mesh(new THREE.SphereGeometry(0.085, 10, 10), new THREE.MeshBasicMaterial({ color: 0xff1133 }));
    redEye.position.set(0.15, 0.04, 0.44);
    headGroup.add(redEye);

    const hornCrest = new THREE.Mesh(new THREE.ConeGeometry(0.12, 0.48, 8), robeMat);
    hornCrest.position.set(-0.26, 0.46, 0.05);
    hornCrest.rotation.z = 0.25;
    headGroup.add(hornCrest);

    [leftArm, rightArm].forEach(arm => {
      const aMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.10, 0.09, 0.44, 10), robeMat);
      aMesh.position.y = -0.18;
      arm.add(aMesh);
    });

    // Precision Rifle in Right Hand
    weapon = new THREE.Mesh(new THREE.CylinderGeometry(0.07, 0.11, 0.72, 12), goldMat);
    weapon.rotation.x = Math.PI / 2;
    weapon.position.set(0, -0.32, 0.32);
    rightArm.add(weapon);

    [leftLeg, rightLeg].forEach(leg => {
      const lMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.11, 0.10, 0.42, 10), darkMat);
      lMesh.position.y = -0.2;
      leg.add(lMesh);
    });

  } else if (actorId === 'jinx') {
    // MODEL #4 (POWERPOINT SPECIALIST): Vibrant Cyan/Rose Outfit, Twin Long Braids, Cat-Ear Hat & Heavy Launcher
    const cyanHairMat = new THREE.MeshStandardMaterial({ color: 0x38bdf8, roughness: 0.3 });
    const roseMat = new THREE.MeshStandardMaterial({ color: 0xf43f5e, roughness: 0.35 });

    const torso = new THREE.Mesh(new THREE.CylinderGeometry(0.29, 0.24, 0.58, 14), roseMat);
    torso.castShadow = true;
    bodyGroup.add(torso);

    const headMesh = new THREE.Mesh(new THREE.SphereGeometry(0.41, 20, 20), skinMat);
    headGroup.add(headMesh);

    const eyeMat = new THREE.MeshBasicMaterial({ color: 0xf43f5e });
    [-0.15, 0.15].forEach(ex => {
      const eye = new THREE.Mesh(new THREE.SphereGeometry(0.07, 10, 10), eyeMat);
      eye.position.set(ex, 0.02, 0.38);
      headGroup.add(eye);
    });

    // Stylish Hat + Twin Long Cyan Braids
    const cap = new THREE.Mesh(new THREE.CylinderGeometry(0.43, 0.44, 0.24, 16), darkMat);
    cap.position.y = 0.26;
    headGroup.add(cap);

    wingL = new THREE.Mesh(new THREE.CylinderGeometry(0.09, 0.05, 0.95, 10), cyanHairMat);
    wingL.position.set(-0.34, -0.28, -0.18);
    wingL.rotation.z = 0.22;
    headGroup.add(wingL);

    wingR = new THREE.Mesh(new THREE.CylinderGeometry(0.09, 0.05, 0.95, 10), cyanHairMat);
    wingR.position.set(0.34, -0.28, -0.18);
    wingR.rotation.z = -0.22;
    headGroup.add(wingR);

    [leftArm, rightArm].forEach(arm => {
      const aMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.10, 0.08, 0.42, 10), skinMat);
      aMesh.position.y = -0.18;
      arm.add(aMesh);
    });

    // Heavy Shark Launcher on Right Arm
    weapon = new THREE.Mesh(new THREE.CylinderGeometry(0.16, 0.12, 0.68, 12), cyanHairMat);
    weapon.rotation.x = Math.PI / 2;
    weapon.position.set(0.08, -0.26, 0.28);
    rightArm.add(weapon);

    [leftLeg, rightLeg].forEach(leg => {
      const lMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.11, 0.09, 0.42, 10), darkMat);
      lMesh.position.y = -0.2;
      leg.add(lMesh);
    });

  } else {
    // BOSS CLAUDE 3D MODEL: Executive Tactician Suit, Golden Crown & Floating Hologram Tablet
    const suitMat = new THREE.MeshStandardMaterial({ color: 0x1e293b, roughness: 0.3, metalness: 0.4 });
    const torso = new THREE.Mesh(new THREE.CylinderGeometry(0.35, 0.28, 0.64, 16), suitMat);
    torso.castShadow = true;
    bodyGroup.add(torso);

    const tie = new THREE.Mesh(new THREE.BoxGeometry(0.10, 0.36, 0.06), goldMat);
    tie.position.set(0, 0.05, 0.30);
    bodyGroup.add(tie);

    const headMesh = new THREE.Mesh(new THREE.SphereGeometry(0.42, 20, 20), skinMat);
    headGroup.add(headMesh);

    const crown = new THREE.Mesh(new THREE.CylinderGeometry(0.32, 0.26, 0.22, 8), goldMat);
    crown.position.y = 0.44;
    headGroup.add(crown);

    const glasses = new THREE.Mesh(new THREE.BoxGeometry(0.62, 0.14, 0.12), goldMat);
    glasses.position.set(0, 0.05, 0.38);
    headGroup.add(glasses);

    [leftArm, rightArm].forEach(arm => {
      const aMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.11, 0.09, 0.44, 10), suitMat);
      aMesh.position.y = -0.18;
      arm.add(aMesh);
    });

    weapon = new THREE.Mesh(
      new THREE.BoxGeometry(0.36, 0.05, 0.46),
      new THREE.MeshBasicMaterial({ color: 0xf59e0b })
    );
    weapon.position.set(0, -0.32, 0.25);
    rightArm.add(weapon);

    [leftLeg, rightLeg].forEach(leg => {
      const lMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.12, 0.10, 0.42, 10), suitMat);
      lMesh.position.y = -0.2;
      leg.add(lMesh);
    });
  }

  return {
    root,
    bodyGroup,
    headGroup,
    leftArm,
    rightArm,
    leftLeg,
    rightLeg,
    wingL,
    wingR,
    weapon,
    auraRing
  };
}

function createCharacterIntoScene(actorId, x, z, colorHex) {
  const rig = buildChibi3DCharacterRig(actorId, colorHex);
  rig.root.position.set(x, 0, z);
  scene.add(rig.root);

  // Create Floating 3D HTML Label for Name/Role & Speech Bubble
  const labelsContainer = document.getElementById('labels3DContainer');
  const labelEl = document.createElement('div');
  labelEl.style.position = 'absolute';
  labelEl.style.transform = 'translate(-50%, -100%)';
  labelEl.style.pointerEvents = 'none';
  labelEl.style.textAlign = 'center';
  labelEl.style.zIndex = '15';

  const isBoss = actorId === 'claude';
  const agentObj = isBoss ? state.boss : state.agents.find(a => a.id === actorId);
  const titleText = isBoss ? '👑 SẾP CLAUDE' : `${'★'.repeat(agentObj.starLevel)} ${agentObj.name}`;
  const subText = isBoss ? 'Giám Đốc Điều Phối' : `Sửa ${agentObj.fileExtension.toUpperCase()}`;

  labelEl.innerHTML = `
    <div id="speech3d-${actorId}" class="entity-speech hidden" style="position:relative; bottom:auto; left:auto; transform:none; margin:0 auto 5px;"></div>
    <div style="background:rgba(6,13,24,0.88); border:1px solid ${colorHex}; border-radius:6px; padding:2px 8px; box-shadow:0 4px 10px rgba(0,0,0,0.6);">
      <div style="font-size:10.5px; font-weight:800; color:#fff; white-space:nowrap;">${titleText}</div>
      <div style="font-size:9.5px; font-weight:600; color:${colorHex}; white-space:nowrap;" id="sublabel3d-${actorId}">${subText}</div>
    </div>
  `;
  labelsContainer.appendChild(labelEl);

  rig.labelEl = labelEl;
  characterRigs[actorId] = rig;
}

function showSpeech3D(actorId, text, durationMs = 3000) {
  const el = document.getElementById(`speech3d-${actorId}`);
  if (!el) return;
  el.textContent = text;
  el.classList.remove('hidden');
  if (state.speechTimers[actorId]) clearTimeout(state.speechTimers[actorId]);
  state.speechTimers[actorId] = setTimeout(() => el.classList.add('hidden'), durationMs);
}

// ============================================================================
// 3. REAL-TIME 3D ANIMATION LOOP (WALKING, WORKING, CTRL+1..4, BOOM 3D FX)
// ============================================================================
function animateMain3D() {
  requestAnimationFrame(animateMain3D);
  const dt = clock.getDelta();
  const t = clock.getElapsedTime();

  controls.update();

  // Rotate Conference Hologram & Vault Portal Ring
  const holo = scene.getObjectByName('holoCore');
  if (holo) {
    holo.rotation.y = t * 1.4;
    holo.rotation.x = Math.sin(t) * 0.4;
  }
  const portalRing = scene.getObjectByName('vaultPortalRing');
  if (portalRing) {
    portalRing.rotation.z = -t * 1.2;
  }

  // Update Boss & 4 Agent 3D Rigs
  updateSingleRigAnimation('claude', state.boss, t, dt);
  state.agents.forEach(agent => {
    updateSingleRigAnimation(agent.id, agent, t, dt);
  });

  // Update 3D Boom Projectiles (Chưởng Lực 3D)
  for (let i = activeProjectiles3D.length - 1; i >= 0; i--) {
    const p = activeProjectiles3D[i];
    p.progress += dt * 1.65;
    if (p.progress >= 1) {
      spawn3DShockwave(p.to.x, p.to.z, p.colorHex);
      scene.remove(p.mesh);
      activeProjectiles3D.splice(i, 1);
      continue;
    }
    const cx = p.from.x + (p.to.x - p.from.x) * p.progress;
    const cz = p.from.z + (p.to.z - p.from.z) * p.progress;
    const cy = 1.2 + Math.sin(p.progress * Math.PI) * 3.2;
    p.mesh.position.set(cx, cy, cz);
    p.mesh.rotation.y += 0.2;
  }

  // Update 3D Expanding Shockwaves on Floor
  for (let i = activeShockwaves3D.length - 1; i >= 0; i--) {
    const sw = activeShockwaves3D[i];
    sw.scale += dt * 5.5;
    sw.opacity -= dt * 1.2;
    if (sw.opacity <= 0) {
      scene.remove(sw.mesh);
      activeShockwaves3D.splice(i, 1);
      continue;
    }
    sw.mesh.scale.set(sw.scale, sw.scale, 1);
    sw.mesh.material.opacity = sw.opacity;
  }

  renderer.render(scene, camera);

  // Project 3D Character Head Positions to 2D Screen Labels
  const canvas = renderer.domElement;
  Object.entries(characterRigs).forEach(([id, rig]) => {
    const worldPos = new THREE.Vector3();
    rig.root.getWorldPosition(worldPos);
    worldPos.y += 2.35;
    worldPos.project(camera);

    const sx = (worldPos.x * 0.5 + 0.5) * canvas.clientWidth;
    const sy = (-worldPos.y * 0.5 + 0.5) * canvas.clientHeight;
    rig.labelEl.style.left = `${sx}px`;
    rig.labelEl.style.top = `${sy}px`;
  });
}

function updateSingleRigAnimation(id, entityData, t, dt) {
  const rig = characterRigs[id];
  if (!rig) return;

  // Smooth 3D movement toward targetPos
  const dx = entityData.targetPos.x - entityData.currentPos.x;
  const dz = entityData.targetPos.z - entityData.currentPos.z;
  const dist = Math.hypot(dx, dz);

  if (dist > 0.08) {
    const speed = 5.2 * dt;
    const step = Math.min(dist, speed);
    entityData.currentPos.x += (dx / dist) * step;
    entityData.currentPos.z += (dz / dist) * step;
    rig.root.position.set(entityData.currentPos.x, 0, entityData.currentPos.z);

    // Face movement direction
    const targetAngle = Math.atan2(dx, dz);
    rig.root.rotation.y = targetAngle;
    if (entityData.pose === 'idle') entityData.pose = 'walk';
  } else {
    if (entityData.pose === 'walk') {
      entityData.pose = 'idle';
      rig.root.rotation.y = 0; // Face forward/camera
    }
  }

  applyPoseToRig(rig, entityData.pose, t + (entityData.order || 0));
}

function applyPoseToRig(rig, pose, t) {
  // Reset defaults
  rig.bodyGroup.position.y = 0.75;
  rig.bodyGroup.rotation.set(0, 0, 0);
  rig.headGroup.rotation.set(0, 0, 0);

  if (pose === 'walk') {
    rig.bodyGroup.position.y = 0.75 + Math.abs(Math.sin(t * 10)) * 0.14;
    rig.leftLeg.rotation.x = Math.sin(t * 10) * 0.65;
    rig.rightLeg.rotation.x = -Math.sin(t * 10) * 0.65;
    rig.leftArm.rotation.x = -Math.sin(t * 10) * 0.55;
    rig.rightArm.rotation.x = Math.sin(t * 10) * 0.55;
  } else if (pose === 'work' || pose === 'ctrl2') {
    // Rapid typing / document crafting at 3D workstation
    rig.bodyGroup.position.y = 0.75 + Math.sin(t * 8) * 0.04;
    rig.leftArm.rotation.x = -0.95 + Math.sin(t * 18) * 0.25;
    rig.rightArm.rotation.x = -0.95 - Math.sin(t * 18) * 0.25;
    rig.headGroup.rotation.x = 0.18;
    rig.headGroup.rotation.y = Math.sin(t * 4) * 0.22;
  } else if (pose === 'ctrl1') {
    // Taunt / Heroic Greeting Pose
    rig.bodyGroup.position.y = 0.78 + Math.sin(t * 5) * 0.06;
    rig.rightArm.rotation.x = -2.4 + Math.sin(t * 8) * 0.25;
    rig.leftArm.rotation.z = 0.45;
    rig.headGroup.rotation.z = Math.sin(t * 4) * 0.15;
  } else if (pose === 'ctrl3') {
    // 360 Dance / Spin Pose
    rig.bodyGroup.position.y = 0.85 + Math.abs(Math.sin(t * 8)) * 0.18;
    rig.bodyGroup.rotation.y = t * 5.5;
    rig.leftArm.rotation.z = 1.1;
    rig.rightArm.rotation.z = -1.1;
  } else if (pose === 'ult' || pose === 'ctrl4') {
    // Ultimate Finisher Levitation & Wing Flare
    rig.bodyGroup.position.y = 1.25 + Math.sin(t * 10) * 0.16;
    rig.bodyGroup.rotation.y = Math.sin(t * 6) * 0.45;
    rig.leftArm.rotation.x = -2.2;
    rig.rightArm.rotation.x = -2.2;
    if (rig.wingL && rig.wingR) {
      rig.wingL.rotation.y = Math.sin(t * 14) * 0.45;
      rig.wingR.rotation.y = -Math.sin(t * 14) * 0.45;
    }
  } else {
    // Smooth breathing Idle
    rig.bodyGroup.position.y = 0.75 + Math.sin(t * 2.8) * 0.04;
    rig.leftArm.rotation.set(0, 0, 0.12 + Math.sin(t * 2.5) * 0.05);
    rig.rightArm.rotation.set(0, 0, -0.12 - Math.sin(t * 2.5) * 0.05);
    rig.leftLeg.rotation.set(0, 0, 0);
    rig.rightLeg.rotation.set(0, 0, 0);
    rig.headGroup.rotation.y = Math.sin(t * 1.5) * 0.12;
  }

  rig.auraRing.rotation.z = t * 1.5;
}

function spawn3DBoomProjectile(fromPos, toPos, colorHex) {
  const mat = new THREE.MeshBasicMaterial({ color: colorHex });
  const orb = new THREE.Mesh(new THREE.OctahedronGeometry(0.32, 1), mat);
  orb.position.set(fromPos.x, 1.2, fromPos.z);
  scene.add(orb);
  activeProjectiles3D.push({
    mesh: orb,
    from: { ...fromPos },
    to: { ...toPos },
    progress: 0,
    colorHex
  });
}

function spawn3DShockwave(x, z, colorHex) {
  const geo = new THREE.RingGeometry(0.3, 0.55, 32);
  const mat = new THREE.MeshBasicMaterial({ color: colorHex, side: THREE.DoubleSide, transparent: true, opacity: 0.95 });
  const mesh = new THREE.Mesh(geo, mat);
  mesh.rotation.x = -Math.PI / 2;
  mesh.position.set(x, 0.05, z);
  scene.add(mesh);
  activeShockwaves3D.push({ mesh, scale: 1, opacity: 0.95 });
}

// ============================================================================
// 4. RIGHT PANEL: DEDICATED 3D CHARACTER MODEL TURNTABLE INSPECTOR
// ============================================================================
function initInspector3DScene() {
  const canvas = document.getElementById('inspector3DCanvas');
  const box = canvas.parentElement;

  inspScene = new THREE.Scene();
  inspCamera = new THREE.PerspectiveCamera(38, box.clientWidth / box.clientHeight, 0.1, 50);
  inspCamera.position.set(0, 1.6, 3.6);

  inspRenderer = new THREE.WebGLRenderer({ canvas, antialias: true, alpha: true });
  inspRenderer.setSize(box.clientWidth, box.clientHeight);
  inspRenderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

  inspControls = new THREE.OrbitControls(inspCamera, inspRenderer.domElement);
  inspControls.target.set(0, 0.95, 0);
  inspControls.enablePan = false;
  inspControls.autoRotate = true;
  inspControls.autoRotateSpeed = 2.8;

  const hLight = new THREE.HemisphereLight(0xffffff, 0x1e293b, 0.95);
  inspScene.add(hLight);
  const dLight = new THREE.DirectionalLight(0xfffbeb, 1.1);
  dLight.position.set(3, 6, 4);
  inspScene.add(dLight);

  rebuildInspectorModel();

  function loopInspector() {
    requestAnimationFrame(loopInspector);
    const t = clock ? clock.getElapsedTime() : 0;
    inspControls.update();
    if (inspRig) {
      const ag = state.agents.find(a => a.id === state.selectedAgentId) || state.agents[0];
      applyPoseToRig(inspRig, ag.pose, t);
    }
    inspRenderer.render(inspScene, inspCamera);
  }
  loopInspector();
}

function rebuildInspectorModel() {
  if (!inspScene) return;
  if (inspRig && inspRig.root) {
    inspScene.remove(inspRig.root);
  }
  const ag = state.agents.find(a => a.id === state.selectedAgentId) || state.agents[0];
  inspRig = buildChibi3DCharacterRig(ag.id, ag.themeColor);
  inspScene.add(inspRig.root);
}

function updateInspectorUI() {
  const ag = state.agents.find(a => a.id === state.selectedAgentId) || state.agents[0];
  document.getElementById('inspModelTitle').textContent = `🧊 ${ag.name.toUpperCase()} (${'★'.repeat(ag.starLevel)})`;
  const badge = document.getElementById('inspExtBadge');
  badge.textContent = ag.fileExtension.toUpperCase();
  badge.style.color = ag.themeColor;
  document.getElementById('inspRoleSub').textContent = ag.role;
  document.getElementById('inspPoseLabel').textContent = `POSE: ${ag.pose.toUpperCase()}`;
  rebuildInspectorModel();
}

window.setSelectedModelPose = function(pose) {
  const ag = state.agents.find(a => a.id === state.selectedAgentId);
  if (!ag) return;
  ag.pose = pose;
  document.getElementById('inspPoseLabel').textContent = `POSE: ${pose.toUpperCase()}`;
  spawn3DShockwave(ag.currentPos.x, ag.currentPos.z, ag.themeColor);
  showSpeech3D(ag.id, `${ag.name} thực hiện động tác 3D [${pose.toUpperCase()}]!`, 2500);
};

// ============================================================================
// 5. BOSS CLAUDE 3D ORCHESTRATION & COMMAND DISPATCH
// ============================================================================
function determineTargetAgents(command, explicitTarget) {
  if (explicitTarget && explicitTarget !== 'auto' && explicitTarget !== 'all') return [explicitTarget];
  if (explicitTarget === 'all') return ['galio', 'zeri', 'jhin', 'jinx'];

  const lower = (command + ' ' + (state.attachedFileName || '')).toLowerCase();
  const matched = [];
  if (lower.includes('pdf') || lower.includes('đóng dấu') || lower.includes('bảo mật')) matched.push('galio');
  if (lower.includes('word') || lower.includes('wowrd') || lower.includes('docx') || lower.includes('hợp đồng') || lower.includes('văn bản')) matched.push('zeri');
  if (lower.includes('excel') || lower.includes('xlsx') || lower.includes('bảng tính') || lower.includes('tài chính') || lower.includes('số liệu') || lower.includes('kpi')) matched.push('jhin');
  if (lower.includes('pp') || lower.includes('pptx') || lower.includes('powerpoint') || lower.includes('slide') || lower.includes('thuyết trình')) matched.push('jinx');

  if (matched.length === 0 || lower.includes('tất cả') || lower.includes('toàn bộ') || lower.includes('cả 4')) {
    return ['galio', 'zeri', 'jhin', 'jinx'];
  }
  return matched;
}

async function sendCommandToBossClaude(customCmd = null, customTarget = null) {
  const cmdInput = document.getElementById('commandInput');
  const userCmd = (customCmd ?? cmdInput.value.trim()) || 'Tổng hợp báo cáo Quý 4: sửa PDF bảo mật, biên tập hợp đồng Word, lập bảng tính Excel 4 Quý và thiết kế Slide PowerPoint!';
  const targetVal = customTarget ?? document.getElementById('targetAgentSelect').value;
  const autoCam = document.getElementById('chkAutoCamera').checked;

  const selectedIds = determineTargetAgents(userCmd, targetVal);
  const btn = document.getElementById('btnSendCommand');
  btn.disabled = true;
  btn.innerHTML = '<span>⏳ SẾP CLAUDE ĐANG ĐIỀU PHỐI CÁC MODEL 3D...</span>';

  // Also trigger backend C# document generation asynchronously
  fetch('/api/command', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      command: userCmd,
      targetAgent: targetVal,
      attachedFileName: state.attachedFileName,
      attachedContent: state.attachedContent,
      triggerFinisher: true
    })
  }).then(r => r.json()).then(data => {
    if (data && data.generatedFiles) {
      fetchBackendState();
    }
  }).catch(() => {});

  state.boss.commandsDispatched++;
  state.boss.currentThought = `Đang điều phối ${selectedIds.length} Model 3D thực thi: "${userCmd}"`;
  document.getElementById('bossCmdCount').textContent = state.boss.commandsDispatched;
  document.getElementById('bossThoughtBox').textContent = `"${state.boss.currentThought}"`;

  // 1. Boss Claude walks to 3D Conference Room
  state.boss.targetPos = { ...state.boss.conferencePos };
  state.boss.pose = 'walk';
  if (autoCam) focusCameraOn(0, -6.5);
  showSpeech3D('claude', `Triệu tập ${selectedIds.length} AI Agent tại Phòng Họp Chiến Thuật 3D!`, 2800);
  appendTimelineLog('👑 Sếp Claude', `Phân tích lệnh & triệu tập ${selectedIds.map(id => state.agents.find(a => a.id === id).name).join(', ')} tại Phòng Họp 3D.`, '#f59e0b');
  await sleep(1100);

  const stamp = new Date().toTimeString().slice(0, 8).replace(/:/g, '');

  for (const id of selectedIds) {
    const agent = state.agents.find(a => a.id === id);
    state.boss.pose = 'ult';
    spawn3DBoomProjectile(state.boss.currentPos, agent.currentPos, agent.themeColor);
    appendTimelineLog(`👑 Sếp Claude -> ${agent.name}`, `Bắn Chưởng Lực 3D giao việc sửa ${agent.fileExtension.toUpperCase()}: "${userCmd}"`, agent.themeColor);
    await sleep(650);

    // Agent walks to Conference Room spot
    agent.targetPos = { x: (agent.order - 2.5) * 1.8, z: -5.0 };
    agent.pose = 'walk';
    showSpeech3D(agent.id, `Nhận lệnh xử lý ${agent.fileExtension.toUpperCase()}!`, 2200);
    await sleep(1100);

    // Agent returns to 3D Desk & works
    agent.targetPos = { ...agent.homeDesk };
    agent.pose = 'walk';
    if (autoCam) focusCameraOn(agent.homeDesk.x, agent.homeDesk.z);
    await sleep(1050);

    agent.pose = 'work';
    spawn3DShockwave(agent.homeDesk.x, agent.homeDesk.z, agent.themeColor);
    showSpeech3D(agent.id, `Đang sửa & chuẩn hoá file ${agent.fileExtension}...`, 2200);
    await sleep(950);

    // Agent executes 3D Ultimate Finisher & sends file orb to Portal Vault
    agent.pose = 'ult';
    spawn3DShockwave(agent.homeDesk.x, agent.homeDesk.z, agent.secondaryColor);
    spawn3DBoomProjectile(agent.homeDesk, { x: 7.5, z: -7.2 }, agent.themeColor);

    const extClean = agent.fileExtension.replace('.', '');
    const outName = `Agent_${extClean.toUpperCase()}_${stamp}.${extClean}`;
    createLocalDoc(agent.id, outName, extClean, userCmd);
    agent.currentTaskTitle = `Hoàn tất ${outName}`;

    const subLbl = document.getElementById(`sublabel3d-${agent.id}`);
    if (subLbl) subLbl.textContent = `✔ ${outName}`;

    renderRoster();
    renderVault();
    showSpeech3D(agent.id, `💥 Hoàn tất xuất bản file ${outName}!`, 3000);
    appendTimelineLog(`💥 ${agent.name}`, `Thực thi Tuyệt Kỹ 3D hoàn tất file \`${outName}\`!`, agent.themeColor);
    await sleep(950);
    agent.pose = 'idle';
  }

  // Boss Claude returns to Executive Office
  state.boss.targetPos = { ...state.boss.officePos };
  state.boss.pose = 'walk';
  if (autoCam) focusCameraOn(0, -1.5);
  appendTimelineLog('✅ Hệ Thống 3D', `Hoàn tất 100%! Cả ${selectedIds.length} tài liệu đã sẵn sàng trong Kho Tài Liệu bên phải.`, '#10b981');

  btn.disabled = false;
  btn.innerHTML = '<span>🚀 GỬI LỆNH CHO SẾP CLAUDE PHÂN TÍCH & ĐIỀU PHỐI 3D</span>';
}

function focusCameraOn(x, z) {
  controls.target.set(x, 0.8, z);
}

// ============================================================================
// 6. UI RENDERERS & EVENT BINDINGS
// ============================================================================
function renderRoster() {
  const container = document.getElementById('agentRosterList');
  container.innerHTML = '';

  const icons = { galio: '🛡️', zeri: '⚡', jhin: '🎭', jinx: '🔥' };
  state.agents.forEach(agent => {
    const stars = '★'.repeat(agent.starLevel);
    const activeClass = agent.id === state.selectedAgentId ? 'active' : '';
    const item = document.createElement('div');
    item.className = `roster-item ${activeClass}`;
    item.style.borderLeft = `4px solid ${agent.themeColor}`;
    item.innerHTML = `
      <div class="roster-portrait-box" style="border-color:${agent.themeColor}; display:flex; align-items:center; justify-content:center; font-size:22px;">
        <span>${icons[agent.id] || '🧊'}</span>
        <div class="roster-stars">${stars}</div>
      </div>
      <div class="roster-info">
        <div class="roster-name-row">
          <span class="roster-name">#${agent.order} ${agent.name}</span>
          <span class="ext-badge" style="color:${agent.themeColor}">${agent.fileExtension.toUpperCase()}</span>
        </div>
        <div class="roster-role">${agent.role}</div>
        <div class="mana-bar-wrap">
          <div class="mana-bar-fill" style="width:${agent.mana}%; background:${agent.themeColor}"></div>
        </div>
      </div>
    `;
    item.addEventListener('click', () => {
      state.selectedAgentId = agent.id;
      renderRoster();
      updateInspectorUI();
      focusCameraOn(agent.currentPos.x, agent.currentPos.z);
      spawn3DShockwave(agent.currentPos.x, agent.currentPos.z, agent.themeColor);
      showSpeech3D(agent.id, agent.quote, 3000);
    });
    container.appendChild(item);
  });
}

function renderVault() {
  const list = document.getElementById('vaultList');
  document.getElementById('docCountBadge').textContent = `${state.documents.length} files`;
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
      <div class="vault-agent-tag">Model 3D: <strong>${doc.createdByAgentName}</strong> • ${(doc.sizeBytes / 1024).toFixed(1)} KB</div>
      <div class="vault-actions">
        <button class="btn-vault" onclick="openDocPreview('${doc.fileId}')">👁️ Xem Nội Dung</button>
        <a href="${doc.downloadUrl || '#'}" download="${doc.fileName}" class="btn-vault">⬇️ Tải File</a>
      </div>
    `;
    list.appendChild(item);
  });
}

window.openDocPreview = function(fileId) {
  const doc = state.documents.find(d => d.fileId === fileId);
  if (!doc) return;
  document.getElementById('previewModalTitle').textContent = `📄 ${doc.fileName}`;
  document.getElementById('previewModalMeta').textContent = `Xử lý bởi: ${doc.createdByAgentName} | Định dạng: .${doc.fileType.toUpperCase()}`;
  document.getElementById('previewModalContent').textContent = doc.previewText;
  const dlBtn = document.getElementById('previewModalDownloadBtn');
  dlBtn.href = doc.downloadUrl || '#';
  dlBtn.setAttribute('download', doc.fileName);
  document.getElementById('docPreviewModal').classList.remove('hidden');
};

function appendTimelineLog(actor, message, color = '#c8aa6e') {
  const feed = document.getElementById('timelineFeed');
  const now = new Date().toLocaleTimeString('vi-VN', { hour12: false });
  const entry = document.createElement('div');
  entry.className = 'log-entry';
  entry.style.borderLeftColor = color;
  entry.innerHTML = `<span class="log-time">[${now}]</span><div><strong style="color:${color}">${actor}:</strong> ${message}</div>`;
  feed.prepend(entry);
}

function bindUIEvents() {
  document.getElementById('btnSendCommand').addEventListener('click', () => sendCommandToBossClaude());
  document.getElementById('btnDispatchAllQuick').addEventListener('click', () => {
    const cmd = 'Sếp Claude hãy điều phối cả 4 Model 3D: AI Agent PDF đóng dấu bảo mật, AI Agent Word sửa hợp đồng, AI Agent Excel lập bảng tính 4 Quý, và AI Agent PowerPoint thiết kế Slide thuyết trình!';
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

  document.getElementById('btnCamIso').addEventListener('click', () => {
    camera.position.set(0, 18.5, 21.5);
    controls.target.set(0, 0.8, -1.5);
  });

  document.getElementById('btnCamTop').addEventListener('click', () => {
    camera.position.set(0, 27.0, 4.0);
    controls.target.set(0, 0, -2.0);
  });

  document.getElementById('bossFocusBtn').addEventListener('click', () => {
    focusCameraOn(state.boss.currentPos.x, state.boss.currentPos.z);
    spawn3DShockwave(state.boss.currentPos.x, state.boss.currentPos.z, '#f59e0b');
  });

  ['ctrl1', 'ctrl2', 'ctrl3', 'ctrl4'].forEach((poseKey, idx) => {
    const btn = document.getElementById(`btnPoseCtrl${idx + 1}`);
    if (btn) {
      btn.addEventListener('click', () => {
        state.agents.forEach(a => {
          a.pose = poseKey === 'ctrl4' ? 'ult' : poseKey;
          spawn3DShockwave(a.currentPos.x, a.currentPos.z, a.themeColor);
        });
        updateInspectorUI();
      });
    }
  });

  document.getElementById('btnResetPositions').addEventListener('click', () => {
    state.boss.targetPos = { ...state.boss.officePos };
    state.boss.pose = 'idle';
    state.agents.forEach(a => {
      a.targetPos = { ...a.homeDesk };
      a.pose = 'idle';
    });
    focusCameraOn(0, -1.5);
  });

  document.getElementById('fileAttachInput').addEventListener('change', async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    state.attachedFileName = file.name;
    document.getElementById('attachedFileLabel').textContent = `Đã đính kèm: ${file.name}`;
  });

  document.getElementById('btnCloseDocModal').addEventListener('click', () => {
    document.getElementById('docPreviewModal').classList.add('hidden');
  });
  document.getElementById('btnCloseDocModal2').addEventListener('click', () => {
    document.getElementById('docPreviewModal').classList.add('hidden');
  });
}

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

document.addEventListener('DOMContentLoaded', initApp);
