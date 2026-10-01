const http=require('node:http'),fs=require('node:fs'),path=require('node:path');
const rutas={'/':path.join(__dirname,'Respaldo.Navegador.Tests.html'),'/respaldoProductos.js':path.join(__dirname,'../wwwroot/js/app/respaldoProductos.js')};
http.createServer((req,res)=>{const f=rutas[req.url];if(!f){res.writeHead(404);res.end();return;}res.setHeader('Content-Type',f.endsWith('.js')?'application/javascript':'text/html; charset=utf-8');res.setHeader('Cache-Control','no-store');res.end(fs.readFileSync(f));}).listen(8099,'127.0.0.1',()=>console.log('Pruebas locales: http://127.0.0.1:8099'));
