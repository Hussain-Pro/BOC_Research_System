const fs = require('fs');
const path = require('path');

function patchDirectory(dir) {
  if (!fs.existsSync(dir)) return;
  const files = fs.readdirSync(dir);
  for (const file of files) {
    const filePath = path.join(dir, file);
    const stat = fs.statSync(filePath);
    if (stat.isDirectory()) {
      patchDirectory(filePath);
    } else if (file.endsWith('.js') || file.endsWith('.mjs')) {
      let content = fs.readFileSync(filePath, 'utf8');
      let changed = false;

      // Replace webpackIgnore comments to include @vite-ignore
      if (content.includes('/*webpackIgnore: true*/')) {
        content = content.replace(/\/\*webpackIgnore:\s*true\*\//g, '/* @vite-ignore, webpackIgnore: true */');
        changed = true;
      }
      if (content.includes('/*webpackIgnore:true*/')) {
        content = content.replace(/\/\*webpackIgnore:\s*true\*\//g, '/* @vite-ignore, webpackIgnore: true */');
        changed = true;
      }

      if (changed) {
        fs.writeFileSync(filePath, content, 'utf8');
        console.log(`Patched: ${filePath}`);
      }
    }
  }
}

console.log('Patching pdfjs-dist and ng2-pdf-viewer files to suppress Vite dynamic import warnings...');
const nodeModulesPath = path.join(__dirname, 'node_modules');
patchDirectory(path.join(nodeModulesPath, 'pdfjs-dist'));
patchDirectory(path.join(nodeModulesPath, 'ng2-pdf-viewer'));
console.log('Patching complete.');
