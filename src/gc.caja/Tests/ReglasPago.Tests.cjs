const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const source=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/pagoFactura.js'),'utf8');
const cd=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/pagoDiferido.js'),'utf8');
const extract=(s,n)=>{const m=s.match(new RegExp('^(?:async )?function '+n+'\\([^]*?^}', 'm'));assert(m,n);return fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/siteGen.js'),'utf8').match(/^function esMedioCobroElectronico\([^]*?^}/m)[0]+'\n'+m[0];};
const cases=JSON.parse(fs.readFileSync(path.join(__dirname,'ReglasPago.Casos.json'),'utf8'));
const ctx=vm.createContext({console:{log(){},warn(){},error(){}},window:{_coTipoActual:'CD'},Date,
 formatearMoneda:String,normalizarTextoUpper:x=>String(x||'').trim().toUpperCase()});
for(const n of ['evaluarReglasPago','validarDiferenciaParaFinalizar','esInstrumentoDocumento','fechaMaximaCredito','fechaLocalDocumento','fechaDocumentoValida'])vm.runInContext(extract(source,n),ctx);
for(const c of cases){
 ctx.window._coTipoActual=c.co;ctx.condicionesPagoCliente={registrado:c.registrado,tope:c.tope,dias_cheque:c.diasH,dias_documento:c.diasD};
 ctx.obtenerTotalNetoCentavos=()=>Math.round(c.total*100);ctx.obtenerTotalCreditosNCCentavos=()=>Math.round(c.nc*100);
 ctx.valoresPago=c.values.map(v=>({tcf_id:v.tipo,ins_id:v.tipo==='DO'?'DOC':v.tipo,importe:v.amount,detalle:{fecha_cheque:ctx.fechaMaximaCredito(v.day),fecha_vencimiento:ctx.fechaMaximaCredito(v.day)}}));
 assert.equal(ctx.validarDiferenciaParaFinalizar().permitir,c.ok,c.name);
 if(c.net!==undefined)assert.equal(Math.round(ctx.evaluarReglasPago(ctx.valoresPago).vuelto*100)||0,Math.round((c.values.filter(v=>v.tipo==='EF').reduce((a,v)=>a+v.amount,0)-c.net)*100),c.name);
}
(async()=>{
 for(const pendientes of [true,false,null,'error']) {
  let destination;
  const ctx=vm.createContext({console:{log(){},warn(){},error(){}},reinicioCobranzaDiferidaEnCurso:false,estadoCobranzaDiferida:{tienePendientesCliente:pendientes},
   cobranzaDiferidaInicializaUrl:'/gc/Facturacion/PDiferido',MenuCajaUrl:'/gc/Home',window:{location:{replace:u=>destination=u}},
   cerrarModalesCobranzaDiferida:async()=>{},recargarFacturasPendientesDelDia:async()=>{if(pendientes==='error')throw Error('Sin conexión');}});
  vm.runInContext(extract(cd,'reiniciarCobranzaDiferida'),ctx);await ctx.reiniciarCobranzaDiferida();
  assert.equal(destination,pendientes===false?'/gc/Home':'/gc/Facturacion/PDiferido');
 }
 // Doble envío: antes de contestar sólo se despacha un POST. Tras éxito tampoco se repite.
 let ajax, posts=0;
 const ctx=vm.createContext({console:{log(){},warn(){},error(){}},confirmacionPagoEnCurso:false,confirmacionPagoConfirmada:false,
  window:{_coTipoActual:'CC'},actualizarMensajeLoadingGlobal(){},ocultarLoadingGlobal(){},mostrarResultadoCobranzaCtaCte:()=>true,
  $:{ajax:()=>{posts++;const req={done:f=>{ajax=f;return req;},fail:()=>req};return req;}}});
 vm.runInContext(extract(source,'enviarPayloadAlServidor'),ctx);
 ctx.enviarPayloadAlServidor([],[],'CuentaCorriente',[]);ctx.enviarPayloadAlServidor([],[],'CuentaCorriente',[]);assert.equal(posts,1);
 ajax({ok:true,data:[{rb_compte:'TEST'}]});ctx.enviarPayloadAlServidor([],[],'CuentaCorriente',[]);assert.equal(posts,1);
 vm.runInContext(extract(source,'advertirSalidaDurantePago'),ctx);
 let prevented=0;ctx.confirmacionPagoEnCurso=true;ctx.advertirSalidaDurantePago({preventDefault(){prevented++;}});assert.equal(prevented,1);
 ctx.confirmacionPagoEnCurso=false;ctx.advertirSalidaDurantePago({preventDefault(){prevented++;}});assert.equal(prevented,1);
 console.log(`PASS: ${cases.length} reglas compartidas, 4 regresos CD, doble confirmación y aviso de recarga.`);
})().catch(e=>{console.error(e);process.exitCode=1;});
