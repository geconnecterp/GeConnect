// Verifica que la espera Golden sólo altera el HTML del gestor compartido.
const fs = require('node:fs'), vm = require('node:vm'), assert = require('node:assert/strict');
const { createHash } = require('node:crypto');
const path = require('node:path');
const root = path.resolve(__dirname, '..');
const source = fs.readFileSync(path.join(root, 'gc.sitio/wwwroot/js/app/areas/docmngr/docmanager.js'), 'utf8').replace(/\r\n/g, '\n');
new vm.Script(source); // sintaxis del archivo completo
// Huella del gestor anterior al cambio visual (15/10/2026), independiente de HEAD.
// Una modificación funcional futura requiere revisar y actualizar este contrato.
const baseline = '9dab3e21d3d304c26941ac9c1ba482c6bd11e67efd2249366117dd1077eab0d7';
const change = /    \/\/ Presentación autónoma:[\s\S]*?printWindow\.document\.write\(`([\s\S]*?)`\);/;
const match = source.match(change);
assert.ok(match, 'Bloque visual encontrado');
const original = "    printWindow.document.write('<div style=\"font-family:Arial;padding:24px\">Generando documentos para imprimir...</div>');";
assert.equal(createHash('sha256').update(source.replace(change, original)).digest('hex'), baseline, 'El flujo fuera del bloque visual permanece idéntico a la versión anterior');
const html = match[1];
assert.ok(html.startsWith('<!doctype html>'));
assert.ok(html.includes('<html lang="es">') && html.includes('name="viewport"'));
assert.ok(html.includes('role="status"') && html.includes('aria-hidden="true"'));
assert.ok(html.includes('prefers-reduced-motion: reduce'));
assert.ok(html.includes('animation: print-wait-spin'));
assert.ok(!/\$\{|<script|<link|<img|@import|url\(/i.test(html), 'Sin interpolación, scripts o dependencias de red');
console.log('PASS: sintaxis, flujo de impresión intacto, HTML autónomo, spinner, accesibilidad y movimiento reducido.');
