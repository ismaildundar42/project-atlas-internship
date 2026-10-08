const fs = require('fs');
const path = require('path');

const targetDirs = [
  path.join(__dirname, '..', 'frontend', 'public', 'uploads', 'projects', 'demo'),
  path.join(__dirname, '..', 'backend', 'src', 'DeUygulamaVitrini.API', 'wwwroot', 'uploads', 'projects', 'demo')
];

targetDirs.forEach(dir => {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
});

const svgs = {
  // 1. Ana Konveyör Kestirimci Bakım
  "ana-konveyor-ekipman-sagligi-kestirimci-bakim": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-1" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#0f172a"/>
      <stop offset="50%" stop-color="#1e293b"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
    <linearGradient id="acc-1" x1="0%" y1="0%" x2="100%" y2="0%">
      <stop offset="0%" stop-color="#38bdf8"/>
      <stop offset="50%" stop-color="#818cf8"/>
      <stop offset="100%" stop-color="#c084fc"/>
    </linearGradient>
    <filter id="glow-1" x="-20%" y="-20%" width="140%" height="140%">
      <feGaussianBlur stdDeviation="8" result="blur"/>
      <feMerge><feMergeNode in="blur"/><feMergeNode in="SourceGraphic"/></feMerge>
    </filter>
  </defs>
  <rect width="800" height="450" fill="url(#bg-1)"/>
  <g stroke="#334155" stroke-width="1" opacity="0.25">
    <line x1="0" y1="75" x2="800" y2="75"/><line x1="0" y1="150" x2="800" y2="150"/><line x1="0" y1="225" x2="800" y2="225"/><line x1="0" y1="300" x2="800" y2="300"/><line x1="0" y1="375" x2="800" y2="375"/>
    <line x1="160" y1="0" x2="160" y2="450"/><line x1="320" y1="0" x2="320" y2="450"/><line x1="480" y1="0" x2="480" y2="450"/><line x1="640" y1="0" x2="640" y2="450"/>
  </g>
  <!-- Conveyor & Bearings Graphic -->
  <g transform="translate(150, 100)" filter="url(#glow-1)">
    <!-- Pulley Left -->
    <circle cx="100" cy="140" r="55" fill="none" stroke="#64748b" stroke-width="6"/>
    <circle cx="100" cy="140" r="20" fill="#1e293b" stroke="#38bdf8" stroke-width="4"/>
    <circle cx="100" cy="140" r="6" fill="#38bdf8"/>
    <!-- Pulley Right -->
    <circle cx="400" cy="140" r="55" fill="none" stroke="#64748b" stroke-width="6"/>
    <circle cx="400" cy="140" r="20" fill="#1e293b" stroke="#38bdf8" stroke-width="4"/>
    <circle cx="400" cy="140" r="6" fill="#38bdf8"/>
    <!-- Belt Top & Bottom -->
    <path d="M 100,85 L 400,85" stroke="#94a3b8" stroke-width="10" stroke-linecap="round"/>
    <path d="M 100,195 L 400,195" stroke="#94a3b8" stroke-width="10" stroke-linecap="round"/>
    <!-- Sensor Nodes -->
    <circle cx="100" cy="65" r="8" fill="#ef4444"/>
    <line x1="100" y1="65" x2="100" y2="85" stroke="#ef4444" stroke-width="2" stroke-dasharray="2,2"/>
    <circle cx="400" cy="65" r="8" fill="#10b981"/>
    <line x1="400" y1="65" x2="400" y2="85" stroke="#10b981" stroke-width="2" stroke-dasharray="2,2"/>
  </g>
  <!-- Live Vibration Waveform -->
  <path d="M 50,340 Q 150,340 220,335 T 320,340 T 360,300 T 380,390 T 400,270 T 420,410 T 440,290 T 460,360 T 500,340 T 650,340 T 750,340" fill="none" stroke="url(#acc-1)" stroke-width="4" stroke-linecap="round" filter="url(#glow-1)"/>
  <!-- Status Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#1e293b" stroke="#38bdf8" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#38bdf8"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600" letter-spacing="0.5">VIBRATION CBM</text>
</svg>`,

  // 2. Cevher Harmanlama ve Kalite
  "cevher-harmanlama-kalite-analiz-platformu": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-2" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#18181b"/>
      <stop offset="50%" stop-color="#27272a"/>
      <stop offset="100%" stop-color="#09090b"/>
    </linearGradient>
    <linearGradient id="ore-grad" x1="0%" y1="0%" x2="0%" y2="100%">
      <stop offset="0%" stop-color="#f59e0b"/>
      <stop offset="50%" stop-color="#d97706"/>
      <stop offset="100%" stop-color="#78350f"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-2)"/>
  <!-- Geological strata / Ore layers -->
  <g opacity="0.8">
    <path d="M 50,280 Q 200,240 400,290 T 750,250 L 750,400 L 50,400 Z" fill="#3f3f46" stroke="#71717a" stroke-width="2"/>
    <path d="M 50,310 Q 250,280 480,330 T 750,300 L 750,400 L 50,400 Z" fill="#27272a" stroke="#52525b" stroke-width="2"/>
    <path d="M 50,350 Q 300,320 520,360 T 750,340 L 750,400 L 50,400 Z" fill="url(#ore-grad)" stroke="#f59e0b" stroke-width="2"/>
  </g>
  <!-- Grade Analysis Chart & Histogram -->
  <g transform="translate(180, 70)">
    <!-- Flask / Analysis Motif -->
    <rect x="260" y="40" width="180" height="150" rx="8" fill="#18181b" stroke="#f59e0b" stroke-width="2" opacity="0.9"/>
    <line x1="280" y1="160" x2="420" y2="160" stroke="#52525b" stroke-width="1.5"/>
    <!-- Bars -->
    <rect x="290" y="110" width="20" height="50" fill="#f59e0b" rx="2"/>
    <rect x="320" y="80" width="20" height="80" fill="#10b981" rx="2"/>
    <rect x="350" y="95" width="20" height="65" fill="#38bdf8" rx="2"/>
    <rect x="380" y="130" width="20" height="30" fill="#a855f7" rx="2"/>
    <!-- Target grade curve -->
    <path d="M 50,140 Q 150,60 250,120 T 450,80" fill="none" stroke="#10b981" stroke-width="3" stroke-dasharray="4,4"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#27272a" stroke="#f59e0b" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#f59e0b"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">GRADE BLENDING</text>
</svg>`,

  // 3. Açık Ocak İSG Kamera Güvenlik
  "acik-ocak-yapay-zeka-isg-kamera-guvenlik": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-3" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#022c22"/>
      <stop offset="50%" stop-color="#064e3b"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-3)"/>
  <!-- Video Viewfinder Frame -->
  <g stroke="#10b981" stroke-width="3" fill="none">
    <path d="M 80,100 L 80,60 L 120,60"/>
    <path d="M 720,100 L 720,60 L 680,60"/>
    <path d="M 80,350 L 80,390 L 120,390"/>
    <path d="M 720,350 L 720,390 L 680,390"/>
    <!-- Center Target -->
    <circle cx="400" cy="225" r="30" stroke="#10b981" stroke-width="1.5" stroke-dasharray="6,6"/>
    <line x1="400" y1="180" x2="400" y2="270" stroke="#10b981" stroke-width="1"/>
    <line x1="350" y1="225" x2="450" y2="225" stroke="#10b981" stroke-width="1"/>
  </g>
  <!-- AI Detection Bounding Boxes -->
  <g transform="translate(240, 130)">
    <!-- Worker Box (Compliant) -->
    <rect x="0" y="0" width="100" height="180" rx="4" fill="rgba(16, 185, 129, 0.1)" stroke="#10b981" stroke-width="2"/>
    <rect x="0" y="-22" width="90" height="22" fill="#10b981" rx="2"/>
    <text x="6" y="-6" fill="#ffffff" font-family="sans-serif" font-size="11" font-weight="700">PPE OK: 98%</text>
    <!-- Person Silhouette -->
    <circle cx="50" cy="35" r="16" fill="#10b981"/>
    <path d="M 25,70 C 25,55 75,55 75,70 L 75,130 L 25,130 Z" fill="#10b981" opacity="0.6"/>
  </g>
  <g transform="translate(460, 150)">
    <!-- Warning Box (Restricted Area) -->
    <rect x="0" y="0" width="110" height="150" rx="4" fill="rgba(239, 68, 68, 0.1)" stroke="#ef4444" stroke-width="2" stroke-dasharray="6,4"/>
    <rect x="0" y="-22" width="105" height="22" fill="#ef4444" rx="2"/>
    <text x="6" y="-6" fill="#ffffff" font-family="sans-serif" font-size="11" font-weight="700">ZONE ALERT</text>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#064e3b" stroke="#10b981" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#ef4444"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">AI VISION HSE</text>
</svg>`,

  // 4. Ağır Maden Araçları Telemetri & Filo
  "maden-araclari-telemetri-dinamik-filo-yonetimi": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-4" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#1e1b4b"/>
      <stop offset="50%" stop-color="#312e81"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-4)"/>
  <!-- Pit Navigation Topology -->
  <g stroke="#4338ca" stroke-width="2" fill="none" opacity="0.4">
    <ellipse cx="400" cy="240" rx="320" ry="140"/>
    <ellipse cx="400" cy="240" rx="240" ry="100"/>
    <ellipse cx="400" cy="240" rx="150" ry="60"/>
    <line x1="80" y1="240" x2="720" y2="240" stroke-dasharray="4,4"/>
  </g>
  <!-- Haul Truck Vector Silhouette -->
  <g transform="translate(280, 140)">
    <!-- Dump Bed -->
    <path d="M 20,40 L 150,40 L 180,90 L 40,90 Z" fill="#f59e0b" stroke="#fbbf24" stroke-width="3"/>
    <!-- Cab -->
    <path d="M 185,55 L 220,55 L 235,90 L 185,90 Z" fill="#38bdf8" stroke="#7dd3fc" stroke-width="2"/>
    <!-- Chassis & Huge Wheels -->
    <rect x="30" y="85" width="200" height="15" fill="#334155"/>
    <circle cx="70" cy="115" r="32" fill="#0f172a" stroke="#64748b" stroke-width="8"/>
    <circle cx="70" cy="115" r="12" fill="#e2e8f0"/>
    <circle cx="190" cy="115" r="32" fill="#0f172a" stroke="#64748b" stroke-width="8"/>
    <circle cx="190" cy="115" r="12" fill="#e2e8f0"/>
    <!-- GPS Telemetry Beacon -->
    <circle cx="210" cy="40" r="6" fill="#10b981"/>
    <path d="M 200,30 Q 210,20 220,30" stroke="#10b981" stroke-width="2" fill="none"/>
    <path d="M 195,22 Q 210,10 225,22" stroke="#10b981" stroke-width="2" fill="none"/>
  </g>
  <!-- Telemetry Metrics Bar -->
  <g transform="translate(220, 340)">
    <rect x="0" y="0" width="360" height="40" rx="8" fill="#1e1b4b" stroke="#6366f1" stroke-width="1.5"/>
    <text x="25" y="25" fill="#a5b4fc" font-family="sans-serif" font-size="12">SPEED: <tspan fill="#ffffff" font-weight="700">28 km/h</tspan></text>
    <text x="140" y="25" fill="#a5b4fc" font-family="sans-serif" font-size="12">FUEL: <tspan fill="#ffffff" font-weight="700">42 L/h</tspan></text>
    <text x="250" y="25" fill="#a5b4fc" font-family="sans-serif" font-size="12">CYCLE: <tspan fill="#10b981" font-weight="700">14.2 min</tspan></text>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#312e81" stroke="#818cf8" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#818cf8"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">FLEET TELEMETRY</text>
</svg>`,

  // 5. Tesis Enerji İzleme Portalı
  "kurumsal-tesis-enerji-izleme-portali": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-5" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#172554"/>
      <stop offset="50%" stop-color="#1e3a8a"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-5)"/>
  <!-- High Voltage Transformer & Energy Grid -->
  <g transform="translate(140, 100)">
    <!-- Transformer Coils -->
    <rect x="80" y="40" width="200" height="140" rx="10" fill="#0f172a" stroke="#3b82f6" stroke-width="3"/>
    <circle cx="140" cy="110" r="35" fill="none" stroke="#60a5fa" stroke-width="4"/>
    <circle cx="220" cy="110" r="35" fill="none" stroke="#60a5fa" stroke-width="4"/>
    <!-- Insulators -->
    <rect x="110" y="10" width="16" height="30" fill="#94a3b8" rx="2"/>
    <rect x="172" y="10" width="16" height="30" fill="#94a3b8" rx="2"/>
    <rect x="234" y="10" width="16" height="30" fill="#94a3b8" rx="2"/>
  </g>
  <!-- Sine wave & Power Factor Gauge -->
  <g transform="translate(430, 130)">
    <rect x="0" y="0" width="240" height="130" rx="8" fill="#0f172a" stroke="#3b82f6" stroke-width="1.5"/>
    <path d="M 20,65 Q 45,20 70,65 T 120,65 T 170,65 T 220,65" fill="none" stroke="#fbbf24" stroke-width="3"/>
    <path d="M 20,65 Q 45,35 70,65 T 120,65 T 170,65 T 220,65" fill="none" stroke="#38bdf8" stroke-width="2" stroke-dasharray="3,3"/>
    <text x="30" y="110" fill="#94a3b8" font-family="sans-serif" font-size="12">cos φ = <tspan fill="#10b981" font-weight="700">0.99</tspan></text>
    <text x="130" y="110" fill="#94a3b8" font-family="sans-serif" font-size="12">THD = <tspan fill="#10b981" font-weight="700">1.8%</tspan></text>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#1e3a8a" stroke="#60a5fa" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#fbbf24"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">POWER QUALITY</text>
</svg>`,

  // 6. Su Rejimi ve Çevresel İzleme
  "su-rejimi-cevresel-izleme-telemetri-agi": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-6" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#082f49"/>
      <stop offset="50%" stop-color="#0c4a6e"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-6)"/>
  <!-- Piezometer Well & Water Table -->
  <g transform="translate(180, 80)">
    <!-- Soil strata -->
    <rect x="0" y="80" width="440" height="220" fill="#1e293b" opacity="0.6"/>
    <!-- Water Table Layer -->
    <path d="M 0,180 Q 110,165 220,180 T 440,175 L 440,300 L 0,300 Z" fill="#0284c7" opacity="0.5"/>
    <!-- Well Casing Column -->
    <rect x="190" y="20" width="60" height="260" fill="#0f172a" stroke="#38bdf8" stroke-width="3" rx="4"/>
    <!-- Solar Panel & LoRaWAN Node on top -->
    <polygon points="170,20 270,20 250,5 190,5" fill="#0369a1" stroke="#38bdf8" stroke-width="1.5"/>
    <line x1="220" y1="5" x2="220" y2="-20" stroke="#38bdf8" stroke-width="2"/>
    <circle cx="220" cy="-20" r="4" fill="#38bdf8"/>
    <!-- Radio Waves -->
    <path d="M 205,-30 Q 220,-40 235,-30" stroke="#38bdf8" stroke-width="2" fill="none"/>
    <path d="M 195,-38 Q 220,-52 245,-38" stroke="#38bdf8" stroke-width="2" fill="none"/>
    <!-- Sensor Cable & Probe -->
    <line x1="220" y1="20" x2="220" y2="230" stroke="#f59e0b" stroke-width="2" stroke-dasharray="4,2"/>
    <rect x="210" y="230" width="20" height="35" rx="4" fill="#f59e0b"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#0c4a6e" stroke="#38bdf8" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#38bdf8"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">HYDRO TELEMETRY</text>
</svg>`,

  // 7. 3B Jeolojik Modelleme & Maden Planlama
  "3b-jeolojik-modelleme-sayisal-maden-planlama": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-7" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#1c1917"/>
      <stop offset="50%" stop-color="#292524"/>
      <stop offset="100%" stop-color="#0c0a09"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-7)"/>
  <!-- Isometric 3D Voxel Geological Block Matrix -->
  <g transform="translate(400, 200) scale(1.1)">
    <!-- Iso Cube 1 -->
    <g transform="translate(-100, 0)">
      <polygon points="0,-40 60,-10 0,20 -60,-10" fill="#ef4444" stroke="#7f1d1d" stroke-width="1"/>
      <polygon points="-60,-10 0,20 0,70 -60,40" fill="#b91c1c" stroke="#7f1d1d" stroke-width="1"/>
      <polygon points="0,20 60,-10 60,40 0,70" fill="#991b1b" stroke="#7f1d1d" stroke-width="1"/>
    </g>
    <!-- Iso Cube 2 -->
    <g transform="translate(0, -30)">
      <polygon points="0,-40 60,-10 0,20 -60,-10" fill="#f59e0b" stroke="#78350f" stroke-width="1"/>
      <polygon points="-60,-10 0,20 0,70 -60,40" fill="#d97706" stroke="#78350f" stroke-width="1"/>
      <polygon points="0,20 60,-10 60,40 0,70" fill="#b45309" stroke="#78350f" stroke-width="1"/>
    </g>
    <!-- Iso Cube 3 -->
    <g transform="translate(100, 0)">
      <polygon points="0,-40 60,-10 0,20 -60,-10" fill="#10b981" stroke="#064e3b" stroke-width="1"/>
      <polygon points="-60,-10 0,20 0,70 -60,40" fill="#059669" stroke="#064e3b" stroke-width="1"/>
      <polygon points="0,20 60,-10 60,40 0,70" fill="#047857" stroke="#064e3b" stroke-width="1"/>
    </g>
    <!-- Iso Cube 4 -->
    <g transform="translate(0, 30)">
      <polygon points="0,-40 60,-10 0,20 -60,-10" fill="#3b82f6" stroke="#1e3a8a" stroke-width="1"/>
      <polygon points="-60,-10 0,20 0,70 -60,40" fill="#2563eb" stroke="#1e3a8a" stroke-width="1"/>
      <polygon points="0,20 60,-10 60,40 0,70" fill="#1d4ed8" stroke="#1e3a8a" stroke-width="1"/>
    </g>
    <!-- Fault Plane Line -->
    <line x1="-160" y1="-70" x2="160" y2="100" stroke="#f43f5e" stroke-width="3" stroke-dasharray="6,4"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#292524" stroke="#a8a29e" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#f59e0b"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">3D BLOCK MODEL</text>
</svg>`,

  // 8. Otonom İHA / Drone Hacim Hesaplama
  "otonom-iha-sayisal-hacim-hesaplama-stok-takip": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-8" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#042f2e"/>
      <stop offset="50%" stop-color="#115e59"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-8)"/>
  <!-- Topo Contour & Point Cloud Grid -->
  <g stroke="#2dd4bf" stroke-width="1.5" fill="none" opacity="0.4">
    <ellipse cx="400" cy="280" rx="260" ry="90"/>
    <ellipse cx="400" cy="280" rx="190" ry="65"/>
    <ellipse cx="400" cy="280" rx="110" ry="40"/>
  </g>
  <!-- Quadcopter Drone -->
  <g transform="translate(350, 90)">
    <!-- Frame X -->
    <line x1="-50" y1="-20" x2="150" y2="60" stroke="#94a3b8" stroke-width="6" stroke-linecap="round"/>
    <line x1="-50" y1="60" x2="150" y2="-20" stroke="#94a3b8" stroke-width="6" stroke-linecap="round"/>
    <!-- Center Body -->
    <circle cx="50" cy="20" r="24" fill="#0f172a" stroke="#2dd4bf" stroke-width="3"/>
    <circle cx="50" cy="20" r="8" fill="#2dd4bf"/>
    <!-- Rotors -->
    <ellipse cx="-50" cy="-20" rx="28" ry="6" fill="rgba(45, 212, 191, 0.4)" stroke="#2dd4bf" stroke-width="1.5"/>
    <ellipse cx="150" cy="-20" rx="28" ry="6" fill="rgba(45, 212, 191, 0.4)" stroke="#2dd4bf" stroke-width="1.5"/>
    <ellipse cx="-50" cy="60" rx="28" ry="6" fill="rgba(45, 212, 191, 0.4)" stroke="#2dd4bf" stroke-width="1.5"/>
    <ellipse cx="150" cy="60" rx="28" ry="6" fill="rgba(45, 212, 191, 0.4)" stroke="#2dd4bf" stroke-width="1.5"/>
    <!-- Laser / Photogrammetry Cone -->
    <polygon points="50,44 10,190 90,190" fill="rgba(45, 212, 191, 0.15)" stroke="#2dd4bf" stroke-width="1" stroke-dasharray="3,3"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#115e59" stroke="#2dd4bf" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#2dd4bf"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">DRONE SURVEYING</text>
</svg>`,

  // 9. SCADA Merkezi Operasyon Kokpiti
  "zenginlestirme-tesisi-scada-merkezi-operasyon-kokpiti": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-9" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#18181b"/>
      <stop offset="50%" stop-color="#27272a"/>
      <stop offset="100%" stop-color="#09090b"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-9)"/>
  <!-- SCADA Industrial Process Flow Lines -->
  <g transform="translate(100, 110)">
    <!-- Pipe 1 -->
    <path d="M 50,120 L 200,120 L 200,60 L 380,60" fill="none" stroke="#0ea5e9" stroke-width="8" stroke-linecap="round"/>
    <!-- Pipe 2 -->
    <path d="M 200,120 L 380,120 L 380,180 L 550,180" fill="none" stroke="#10b981" stroke-width="8" stroke-linecap="round"/>
    <!-- Tank 1 -->
    <rect x="160" y="20" width="80" height="120" rx="12" fill="#0f172a" stroke="#0ea5e9" stroke-width="3"/>
    <line x1="170" y1="90" x2="230" y2="90" stroke="#0ea5e9" stroke-width="2" stroke-dasharray="4,2"/>
    <text x="182" y="130" fill="#38bdf8" font-family="sans-serif" font-size="11" font-weight="700">TK-101</text>
    <!-- Tank 2 (Flotation) -->
    <rect x="340" y="80" width="80" height="120" rx="12" fill="#0f172a" stroke="#10b981" stroke-width="3"/>
    <line x1="350" y1="140" x2="410" y2="140" stroke="#10b981" stroke-width="2" stroke-dasharray="4,2"/>
    <text x="362" y="190" fill="#34d399" font-family="sans-serif" font-size="11" font-weight="700">FL-204</text>
    <!-- Valve Symbols -->
    <polygon points="280,50 300,60 280,70" fill="#f59e0b"/>
    <polygon points="300,50 280,60 300,70" fill="#f59e0b"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#27272a" stroke="#0ea5e9" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#10b981"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">SCADA COCKPIT</text>
</svg>`,

  // 10. Mobil Saha Denetim & Vardiya
  "mobil-saha-denetim-vardiya-yonetim-uygulamasi": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-10" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#1e293b"/>
      <stop offset="50%" stop-color="#334155"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-10)"/>
  <!-- Rugged Tablet Vector Frame -->
  <g transform="translate(260, 70)">
    <!-- Rugged Bumper Case -->
    <rect x="-10" y="-10" width="300" height="280" rx="20" fill="#0f172a" stroke="#64748b" stroke-width="4"/>
    <!-- Screen -->
    <rect x="10" y="10" width="260" height="240" rx="8" fill="#1e293b"/>
    <!-- Checklist Items -->
    <g transform="translate(30, 40)">
      <!-- Item 1 Checked -->
      <circle cx="15" cy="15" r="10" fill="#10b981"/>
      <polyline points="10,15 14,19 20,11" stroke="#ffffff" stroke-width="2" fill="none"/>
      <rect x="35" y="10" width="160" height="10" rx="3" fill="#94a3b8"/>
      <!-- Item 2 Checked -->
      <circle cx="15" cy="55" r="10" fill="#10b981"/>
      <polyline points="10,55 14,59 20,51" stroke="#ffffff" stroke-width="2" fill="none"/>
      <rect x="35" y="50" width="130" height="10" rx="3" fill="#94a3b8"/>
      <!-- Item 3 Pending / Photo -->
      <circle cx="15" cy="95" r="10" fill="#f59e0b"/>
      <rect x="35" y="90" width="180" height="10" rx="3" fill="#94a3b8"/>
      <!-- Sync Status Pill -->
      <rect x="35" y="140" width="120" height="24" rx="12" fill="#0f172a" stroke="#38bdf8" stroke-width="1.5"/>
      <text x="50" y="156" fill="#38bdf8" font-family="sans-serif" font-size="10" font-weight="700">OFFLINE SYNC OK</text>
    </g>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#334155" stroke="#38bdf8" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#38bdf8"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">MOBILE FIELD OPS</text>
</svg>`,

  // 11. Kurumsal SAP ERP Entegrasyon Katmanı
  "kurumsal-sap-erp-saha-uretim-entegrasyonu": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-11" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#0f172a"/>
      <stop offset="50%" stop-color="#1e1b4b"/>
      <stop offset="100%" stop-color="#020617"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-11)"/>
  <!-- Microservices & API Gateway Mesh Nodes -->
  <g transform="translate(150, 100)">
    <!-- Central Message Hub -->
    <circle cx="250" cy="120" r="45" fill="#1e1b4b" stroke="#818cf8" stroke-width="3"/>
    <text x="228" y="125" fill="#ffffff" font-family="sans-serif" font-size="14" font-weight="800">ERP</text>
    <!-- Satellite Nodes -->
    <!-- Scale Node -->
    <circle cx="70" cy="50" r="28" fill="#0f172a" stroke="#38bdf8" stroke-width="2"/>
    <text x="52" y="54" fill="#38bdf8" font-family="sans-serif" font-size="11" font-weight="600">SCALE</text>
    <line x1="95" y1="65" x2="210" y2="100" stroke="#38bdf8" stroke-width="2" stroke-dasharray="4,4"/>
    <!-- SAP MM Node -->
    <circle cx="430" cy="50" r="28" fill="#0f172a" stroke="#10b981" stroke-width="2"/>
    <text x="410" y="54" fill="#10b981" font-family="sans-serif" font-size="11" font-weight="600">SAP MM</text>
    <line x1="405" y1="65" x2="290" y2="100" stroke="#10b981" stroke-width="2" stroke-dasharray="4,4"/>
    <!-- SAP PM Node -->
    <circle cx="70" cy="190" r="28" fill="#0f172a" stroke="#f59e0b" stroke-width="2"/>
    <text x="50" y="194" fill="#f59e0b" font-family="sans-serif" font-size="11" font-weight="600">SAP PM</text>
    <line x1="95" y1="175" x2="210" y2="140" stroke="#f59e0b" stroke-width="2" stroke-dasharray="4,4"/>
    <!-- SAP SD Node -->
    <circle cx="430" cy="190" r="28" fill="#0f172a" stroke="#c084fc" stroke-width="2"/>
    <text x="412" y="194" fill="#c084fc" font-family="sans-serif" font-size="11" font-weight="600">SAP SD</text>
    <line x1="405" y1="175" x2="290" y2="140" stroke="#c084fc" stroke-width="2" stroke-dasharray="4,4"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#1e1b4b" stroke="#818cf8" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#818cf8"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">SAP INTEGRATION</text>
</svg>`,

  // 12. İSG Risk Analizi ve Ramak Kala
  "isg-risk-analizi-ramak-kala-dijital-bildirim": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-12" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#450a0a"/>
      <stop offset="50%" stop-color="#7f1d1d"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-12)"/>
  <!-- 5x5 Risk Matrix & Safety Shield -->
  <g transform="translate(180, 90)">
    <!-- Matrix Grid -->
    <g opacity="0.8">
      <rect x="0" y="0" width="30" height="30" fill="#10b981"/>
      <rect x="35" y="0" width="30" height="30" fill="#10b981"/>
      <rect x="70" y="0" width="30" height="30" fill="#f59e0b"/>
      <rect x="105" y="0" width="30" height="30" fill="#ef4444"/>
      <rect x="140" y="0" width="30" height="30" fill="#7f1d1d"/>
      <rect x="0" y="35" width="30" height="30" fill="#10b981"/>
      <rect x="35" y="35" width="30" height="30" fill="#f59e0b"/>
      <rect x="70" y="35" width="30" height="30" fill="#ef4444"/>
      <rect x="105" y="35" width="30" height="30" fill="#ef4444"/>
      <rect x="140" y="35" width="30" height="30" fill="#7f1d1d"/>
      <rect x="0" y="70" width="30" height="30" fill="#f59e0b"/>
      <rect x="35" y="70" width="30" height="30" fill="#ef4444"/>
      <rect x="70" y="70" width="30" height="30" fill="#ef4444"/>
      <rect x="105" y="70" width="30" height="30" fill="#7f1d1d"/>
      <rect x="140" y="70" width="30" height="30" fill="#7f1d1d"/>
      <rect x="0" y="105" width="30" height="30" fill="#ef4444"/>
      <rect x="35" y="105" width="30" height="30" fill="#ef4444"/>
      <rect x="70" y="105" width="30" height="30" fill="#7f1d1d"/>
      <rect x="105" y="105" width="30" height="30" fill="#7f1d1d"/>
      <rect x="140" y="105" width="30" height="30" fill="#7f1d1d"/>
    </g>
    <!-- Safety Shield on the right -->
    <g transform="translate(260, 10)">
      <path d="M 60,0 L 120,25 L 120,80 C 120,130 60,165 60,165 C 60,165 0,130 0,80 L 0,25 Z" fill="#0f172a" stroke="#ef4444" stroke-width="4"/>
      <polygon points="60,35 75,70 110,75 85,100 90,135 60,118 30,135 35,100 10,75 45,70" fill="#ef4444"/>
    </g>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#7f1d1d" stroke="#f87171" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#ef4444"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">RISK &amp; NEAR-MISS</text>
</svg>`,

  // 13. RFID ve Otomatik Kantar
  "rfid-otomatik-kantar-sevkiyat-sistemi": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-13" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#0f172a"/>
      <stop offset="50%" stop-color="#1e293b"/>
      <stop offset="100%" stop-color="#334155"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-13)"/>
  <!-- Weighbridge Platform & RFID Gantry -->
  <g transform="translate(150, 120)">
    <!-- Scale Platform Base -->
    <rect x="40" y="140" width="420" height="20" fill="#475569" stroke="#94a3b8" stroke-width="2"/>
    <!-- Load Cells under platform -->
    <rect x="80" y="160" width="30" height="15" fill="#f59e0b"/>
    <rect x="235" y="160" width="30" height="15" fill="#f59e0b"/>
    <rect x="390" y="160" width="30" height="15" fill="#f59e0b"/>
    <!-- RFID Gantry Pole & Antenna -->
    <rect x="120" y="10" width="12" height="130" fill="#64748b"/>
    <rect x="100" y="10" width="52" height="18" fill="#0f172a" stroke="#38bdf8" stroke-width="2" rx="4"/>
    <!-- RFID Waves propagating down -->
    <path d="M 110,40 Q 126,55 142,40" stroke="#38bdf8" stroke-width="2.5" fill="none"/>
    <path d="M 100,52 Q 126,72 152,52" stroke="#38bdf8" stroke-width="2.5" fill="none"/>
    <path d="M 90,64 Q 126,89 162,64" stroke="#38bdf8" stroke-width="2.5" fill="none"/>
    <!-- Automatic Barrier -->
    <rect x="360" y="50" width="16" height="90" fill="#64748b"/>
    <line x1="368" y1="65" x2="480" y2="40" stroke="#ef4444" stroke-width="6" stroke-linecap="round"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#1e293b" stroke="#38bdf8" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#38bdf8"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">AUTO WEIGHBRIDGE</text>
</svg>`,

  // 14. Yedek Parça Stok Optimizasyonu
  "yedek-parca-ambar-stok-optimizasyonu": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-14" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#1e1b4b"/>
      <stop offset="50%" stop-color="#312e81"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-14)"/>
  <!-- Warehouse Racks & Spare Gears Motif -->
  <g transform="translate(180, 100)">
    <!-- Rack frame -->
    <rect x="40" y="20" width="200" height="180" fill="none" stroke="#6366f1" stroke-width="3" rx="6"/>
    <line x1="40" y1="80" x2="240" y2="80" stroke="#6366f1" stroke-width="2"/>
    <line x1="40" y1="140" x2="240" y2="140" stroke="#6366f1" stroke-width="2"/>
    <!-- Boxes on shelves -->
    <rect x="60" y="40" width="35" height="35" fill="#f59e0b" rx="3"/>
    <rect x="110" y="40" width="45" height="35" fill="#38bdf8" rx="3"/>
    <rect x="60" y="100" width="55" height="35" fill="#10b981" rx="3"/>
    <rect x="130" y="100" width="35" height="35" fill="#ec4899" rx="3"/>
    <!-- Big Industrial Gear -->
    <g transform="translate(330, 110)">
      <circle cx="0" cy="0" r="50" fill="#0f172a" stroke="#a855f7" stroke-width="6"/>
      <circle cx="0" cy="0" r="20" fill="#312e81"/>
      <path d="M 0,-60 L 0,-45 M 0,60 L 0,45 M -60,0 L -45,0 M 60,0 L 45,0" stroke="#a855f7" stroke-width="8" stroke-linecap="round"/>
    </g>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#312e81" stroke="#a855f7" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#a855f7"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">INVENTORY OPTIM</text>
</svg>`,

  // 15. Sondaj Karot Görüntülerinden Otomatik Mineral ve Çatlak Sınıflandırma
  "sondaj-karot-otomatik-mineral-catlak-siniflandirma": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-15" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#022c22"/>
      <stop offset="50%" stop-color="#064e3b"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
    <linearGradient id="core-grad" x1="0%" y1="0%" x2="100%" y2="0%">
      <stop offset="0%" stop-color="#78716c"/>
      <stop offset="50%" stop-color="#d6d3d1"/>
      <stop offset="100%" stop-color="#57534e"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-15)"/>
  <!-- Core Tray with Cylindrical Core Sticks -->
  <g transform="translate(140, 90)">
    <!-- Core Box Wooden Frame -->
    <rect x="0" y="0" width="520" height="220" rx="8" fill="#1c1917" stroke="#78716c" stroke-width="4"/>
    <!-- Row 1 -->
    <rect x="20" y="20" width="480" height="40" rx="6" fill="url(#core-grad)"/>
    <line x1="120" y1="20" x2="128" y2="60" stroke="#dc2626" stroke-width="3"/>
    <line x1="280" y1="20" x2="275" y2="60" stroke="#dc2626" stroke-width="3"/>
    <line x1="410" y1="20" x2="415" y2="60" stroke="#dc2626" stroke-width="3"/>
    <!-- Row 2 -->
    <rect x="20" y="85" width="480" height="40" rx="6" fill="url(#core-grad)"/>
    <line x1="190" y1="85" x2="185" y2="125" stroke="#dc2626" stroke-width="3"/>
    <line x1="330" y1="85" x2="335" y2="125" stroke="#dc2626" stroke-width="3"/>
    <!-- Row 3 -->
    <rect x="20" y="150" width="480" height="40" rx="6" fill="url(#core-grad)"/>
    <line x1="150" y1="150" x2="155" y2="190" stroke="#dc2626" stroke-width="3"/>
    <line x1="390" y1="150" x2="385" y2="190" stroke="#dc2626" stroke-width="3"/>
    <!-- Segmentation Overlay Box -->
    <rect x="18" y="18" width="100" height="44" fill="none" stroke="#10b981" stroke-width="2" stroke-dasharray="3,3"/>
    <text x="25" y="12" fill="#10b981" font-family="sans-serif" font-size="10" font-weight="700">RQD: 88%</text>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#064e3b" stroke="#10b981" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#10b981"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">CORE LOGGING AI</text>
</svg>`,

  // 16. Atık Barajı ve Liç Sahası Jeoteknik Kararlılık
  "atik-baraji-lic-sahasi-jeoteknik-erken-uyari": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-16" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#1e1b4b"/>
      <stop offset="50%" stop-color="#0f172a"/>
      <stop offset="100%" stop-color="#020617"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-16)"/>
  <!-- Dam Embankment Cross-Section -->
  <g transform="translate(140, 110)">
    <!-- Dam Wall Profile -->
    <polygon points="50,220 200,60 350,60 500,220" fill="#334155" stroke="#64748b" stroke-width="3"/>
    <!-- Impounded Tailings Slurry -->
    <polygon points="0,120 200,60 50,220 0,220" fill="#0284c7" opacity="0.6"/>
    <!-- Inclinometer Depth String -->
    <line x1="275" y1="60" x2="275" y2="220" stroke="#ef4444" stroke-width="3" stroke-dasharray="6,4"/>
    <circle cx="275" cy="60" r="6" fill="#ef4444"/>
    <circle cx="275" cy="110" r="5" fill="#f59e0b"/>
    <circle cx="275" cy="160" r="5" fill="#10b981"/>
    <!-- Displacement Vectors -->
    <line x1="350" y1="60" x2="385" y2="70" stroke="#f43f5e" stroke-width="3"/>
    <polygon points="385,70 375,64 378,75" fill="#f43f5e"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#1e293b" stroke="#f43f5e" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#f43f5e"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">DAM STABILITY</text>
</svg>`,

  // 17. Ağır İş Makineleri Yağ Spektrometri
  "agir-ekipman-yag-spektrometri-analiz-platformu": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-17" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#18181b"/>
      <stop offset="50%" stop-color="#27272a"/>
      <stop offset="100%" stop-color="#09090b"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-17)"/>
  <!-- Oil Drop & Spectral Element Breakdown -->
  <g transform="translate(180, 90)">
    <!-- Oil Droplet -->
    <path d="M 120,40 C 120,40 60,130 60,170 C 60,205 85,230 120,230 C 155,230 180,205 180,170 C 180,130 120,40 120,40 Z" fill="#d97706" stroke="#fbbf24" stroke-width="4"/>
    <!-- Elemental Wear Bars on right -->
    <g transform="translate(230, 40)">
      <!-- Fe (Iron) -->
      <text x="0" y="25" fill="#94a3b8" font-family="sans-serif" font-size="12" font-weight="700">Fe (Iron)</text>
      <rect x="75" y="12" width="140" height="16" rx="4" fill="#334155"/>
      <rect x="75" y="12" width="85" height="16" rx="4" fill="#3b82f6"/>
      <!-- Cu (Copper) -->
      <text x="0" y="65" fill="#94a3b8" font-family="sans-serif" font-size="12" font-weight="700">Cu (Copper)</text>
      <rect x="75" y="52" width="140" height="16" rx="4" fill="#334155"/>
      <rect x="75" y="52" width="45" height="16" rx="4" fill="#f59e0b"/>
      <!-- Pb (Lead) -->
      <text x="0" y="105" fill="#94a3b8" font-family="sans-serif" font-size="12" font-weight="700">Pb (Lead)</text>
      <rect x="75" y="92" width="140" height="16" rx="4" fill="#334155"/>
      <rect x="75" y="92" width="115" height="16" rx="4" fill="#ef4444"/>
      <!-- Si (Silicon) -->
      <text x="0" y="145" fill="#94a3b8" font-family="sans-serif" font-size="12" font-weight="700">Si (Dust)</text>
      <rect x="75" y="132" width="140" height="16" rx="4" fill="#334155"/>
      <rect x="75" y="132" width="30" height="16" rx="4" fill="#10b981"/>
    </g>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#27272a" stroke="#fbbf24" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#fbbf24"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">OIL SPECTROMETRY</text>
</svg>`,

  // 18. Konveyör Bant Termal Kamera Sıcaklık
  "konveyor-bant-termal-kamera-sicaklik-izleme": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-18" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#450a0a"/>
      <stop offset="50%" stop-color="#18181b"/>
      <stop offset="100%" stop-color="#09090b"/>
    </linearGradient>
    <linearGradient id="thermal-grad" x1="0%" y1="0%" x2="100%" y2="0%">
      <stop offset="0%" stop-color="#3b82f6"/>
      <stop offset="35%" stop-color="#10b981"/>
      <stop offset="70%" stop-color="#f59e0b"/>
      <stop offset="100%" stop-color="#ef4444"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-18)"/>
  <!-- Thermal Camera & Belt Hotspot View -->
  <g transform="translate(160, 110)">
    <!-- Conveyor Belt with Heat Map -->
    <rect x="40" y="100" width="420" height="70" rx="8" fill="url(#thermal-grad)" stroke="#ffffff" stroke-width="2"/>
    <!-- Hotspot Target -->
    <circle cx="360" cy="135" r="25" fill="none" stroke="#ffffff" stroke-width="3" stroke-dasharray="4,2"/>
    <text x="340" y="90" fill="#fca5a5" font-family="sans-serif" font-size="13" font-weight="800">88.4 °C</text>
    <!-- Thermal Camera Icon mounted above -->
    <polygon points="200,20 240,20 250,50 190,50" fill="#334155" stroke="#f87171" stroke-width="2"/>
    <line x1="220" y1="50" x2="360" y2="135" stroke="#f87171" stroke-width="1.5" stroke-dasharray="3,3"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#7f1d1d" stroke="#f87171" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#ef4444"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">THERMAL VISION</text>
</svg>`,

  // 19. Maden Rehabilitasyonu Uydu Takip
  "maden-rehabilitasyon-bitkilendirme-uydu-takip": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-19" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#052e16"/>
      <stop offset="50%" stop-color="#14532d"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-19)"/>
  <!-- Satellite Orbit & Multispectral NDVI Grid -->
  <g transform="translate(150, 90)">
    <!-- Land parcel grids -->
    <polygon points="50,220 180,140 320,160 200,240" fill="#15803d" stroke="#22c55e" stroke-width="2"/>
    <polygon points="180,140 290,70 420,90 320,160" fill="#84cc16" stroke="#a3e635" stroke-width="2"/>
    <polygon points="320,160 420,90 520,130 430,210" fill="#ca8a04" stroke="#eab308" stroke-width="2"/>
    <!-- Orbiting Satellite on top right -->
    <g transform="translate(420, 20)">
      <rect x="0" y="0" width="40" height="24" rx="4" fill="#0f172a" stroke="#22c55e" stroke-width="2"/>
      <!-- Solar Wings -->
      <rect x="-35" y="4" width="30" height="16" fill="#0284c7" stroke="#38bdf8" stroke-width="1"/>
      <rect x="45" y="4" width="30" height="16" fill="#0284c7" stroke="#38bdf8" stroke-width="1"/>
      <!-- Beam to ground -->
      <line x1="20" y1="24" x2="-120" y2="140" stroke="#22c55e" stroke-width="1.5" stroke-dasharray="4,4"/>
    </g>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#14532d" stroke="#22c55e" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#22c55e"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">SATELLITE NDVI</text>
</svg>`,

  // 20. Tesis Döner Fırın ve Kırıcı Titreşim Anomali Tespiti (Ar-Ge - Taslak)
  "doner-firin-kirici-titresim-anomali-tespiti": `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 450" width="100%" height="100%">
  <defs>
    <linearGradient id="bg-20" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#431407"/>
      <stop offset="50%" stop-color="#7c2d12"/>
      <stop offset="100%" stop-color="#0f172a"/>
    </linearGradient>
  </defs>
  <rect width="800" height="450" fill="url(#bg-20)"/>
  <!-- Rotary Kiln Cylinder with Laser Beams -->
  <g transform="translate(150, 100)">
    <!-- Kiln Cylinder Tube -->
    <rect x="40" y="70" width="420" height="100" rx="10" fill="#1c1917" stroke="#ea580c" stroke-width="4"/>
    <!-- Riding Rings (Tyres) -->
    <rect x="120" y="55" width="25" height="130" rx="4" fill="#44403c" stroke="#fed7aa" stroke-width="2"/>
    <rect x="340" y="55" width="25" height="130" rx="4" fill="#44403c" stroke="#fed7aa" stroke-width="2"/>
    <!-- Laser Distance Beams -->
    <line x1="20" y1="20" x2="132" y2="55" stroke="#ef4444" stroke-width="2.5"/>
    <circle cx="20" cy="20" r="5" fill="#ef4444"/>
    <line x1="480" y1="20" x2="352" y2="55" stroke="#ef4444" stroke-width="2.5"/>
    <circle cx="480" cy="20" r="5" fill="#ef4444"/>
    <!-- DSP Waveform below -->
    <path d="M 40,210 Q 145,170 250,210 T 460,210" fill="none" stroke="#ea580c" stroke-width="3"/>
  </g>
  <!-- Badge -->
  <rect x="50" y="40" width="160" height="28" rx="14" fill="#7c2d12" stroke="#ea580c" stroke-width="1.5"/>
  <circle cx="66" cy="54" r="5" fill="#ea580c"/>
  <text x="80" y="58" fill="#f8fafc" font-family="system-ui, sans-serif" font-size="12" font-weight="600">KILN LASER DSP</text>
</svg>`
};

Object.entries(svgs).forEach(([slug, content]) => {
  targetDirs.forEach(dir => {
    const filePath = path.join(dir, `${slug}.svg`);
    fs.writeFileSync(filePath, content.trim(), 'utf8');
    console.log(`Wrote: ${filePath}`);
  });
});

console.log('Successfully generated all 20 distinct project cover SVGs!');
