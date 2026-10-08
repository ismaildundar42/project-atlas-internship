const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

console.log('=== DEMİR EXPORT PROJECT COVER & ASSET INTEGRITY VALIDATOR ===\n');

// 1. Fetch all projects from database
const sqlOutput = execSync(
  `sqlcmd -S "DESKTOP-496IBCH\\SQLEXPRESS" -d "DeUygulamaVitriniDb" -y 0 -Q "SET NOCOUNT ON; SELECT Id, Name, Slug, CoverImageUrl, IsPublished, ApprovalStatus FROM Projects ORDER BY Id;"`,
  { encoding: 'utf8' }
);

const lines = sqlOutput.trim().split('\n').map(l => l.trim()).filter(l => l && !l.startsWith('Id') && !l.startsWith('-') && !l.includes('rows affected'));

console.log(`Total projects in database: ${lines.length}`);
if (lines.length !== 20) {
  console.error(`FAILED: Expected 20 projects, found ${lines.length}`);
  process.exit(1);
}

const frontendPublic = path.join(__dirname, '..', 'frontend', 'public');
const backendWwwroot = path.join(__dirname, '..', 'backend', 'src', 'DeUygulamaVitrini.API', 'wwwroot');

const seenUrls = new Set();
let passed = 0;
let failed = 0;

console.log('\n%-4s | %-50s | %-45s | %-8s | %-8s | %-6s', 'ID', 'PROJECT NAME', 'COVER IMAGE URL', 'FRONTEND', 'BACKEND', 'STATUS');
console.log('-'.repeat(140));

for (const line of lines) {
  // Regex match: ID, Name, Slug, CoverImageUrl, IsPublished, ApprovalStatus
  const parts = line.split(/\s+/);
  const id = parts[0];
  const coverUrl = parts.find(p => p.startsWith('/uploads/'));
  const slug = parts.find(p => p.includes('-') && !p.startsWith('/'));
  
  if (!coverUrl) {
    console.error(`[FAIL] Project ID ${id}: Missing CoverImageUrl in line: ${line}`);
    failed++;
    continue;
  }

  // Check duplicate
  if (seenUrls.has(coverUrl)) {
    console.error(`[FAIL] Project ID ${id}: Duplicate CoverImageUrl ${coverUrl}`);
    failed++;
    continue;
  }
  seenUrls.add(coverUrl);

  // Check remote
  if (coverUrl.startsWith('http://') || coverUrl.startsWith('https://')) {
    console.error(`[FAIL] Project ID ${id}: Remote URL found ${coverUrl}`);
    failed++;
    continue;
  }

  // Resolve relative files
  const relPath = coverUrl.replace(/^\//, '').replace(/\//g, path.sep);
  const frontFile = path.join(frontendPublic, relPath);
  const backFile = path.join(backendWwwroot, relPath);

  const frontExists = fs.existsSync(frontFile);
  const backExists = fs.existsSync(backFile);

  if (!frontExists || !backExists) {
    console.error(`[FAIL] Project ID ${id}: File missing! Front: ${frontExists}, Back: ${backExists} at ${relPath}`);
    failed++;
    continue;
  }

  // Validate XML/SVG content
  const content = fs.readFileSync(frontFile, 'utf8');
  if (!content.includes('<svg') || !content.includes('</svg>')) {
    console.error(`[FAIL] Project ID ${id}: Invalid SVG content at ${frontFile}`);
    failed++;
    continue;
  }

  passed++;
  console.log(`[PASS] ID ${id.padEnd(5)} | ${parts.slice(1, 4).join(' ').padEnd(35)} | ${coverUrl.padEnd(45)} | OK       | OK       | VALID`);
}

console.log('-'.repeat(140));
console.log(`\nVALIDATION SUMMARY:`);
console.log(`- Total Projects: 20`);
console.log(`- Valid Covers:   ${passed}`);
console.log(`- Failed Covers:  ${failed}`);
console.log(`- Unique SVGs:    ${seenUrls.size}`);

if (failed > 0 || passed !== 20) {
  console.error('\nFAILED: Visual asset integrity checks did not pass 100%.');
  process.exit(1);
} else {
  console.log('\nSUCCESS: All 20 project covers are 100% valid, physical, distinct, and verified!');
}
