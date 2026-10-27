// Errores de emisión posteriores a un cobro aprobado. Sin red, SQL, POS ni AFIP.
const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
const pago=fs.readFileSync(__dirname+'/../wwwroot/js/app/pagoFactura.js','utf8');
const comun=fs.readFileSync(__dirname+'/../wwwroot/js/app/siteGen.js','utf8');
function extraer(s,n){const m=s.match(new RegExp('^function '+n+'\\([^]*?^}', 'm'));assert(m,n);return m[0];}
const errorAfip='Error Solicitar CAECode: 10048 El campo Importe Total ImpTotal, debe ser igual a la suma de ImpTotConc + ImpNeto + ImpOpEx + ImpTrib + ImpIVA.Code: 10061 La suma de los campos BaseImp en AlicIva debe ser igual al valor ingresado en ImpNeto.';
let casos=0;
function escenario(valores){
 const campos=new Map(),mensajes=[];
 const $=s=>{if(!campos.has(s))campos.set(s,{texto:'',clases:new Set(['d-none'])});const e=campos.get(s);return {text(v){if(v===undefined)return e.texto;e.texto=v;return this;},toggleClass(c,b){b?e.clases.add(c):e.clases.delete(c);return this;},addClass(c){e.clases.add(c);return this;},removeClass(c){e.clases.delete(c);return this;},modal(){return this;}};};
 const ctx={valoresPago:valores,$,formatearNumero:n=>Number(n).toFixed(2),console:{log(){},warn(){},error(){}},escapeHtml:s=>String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;'),AbrirMensaje:(...a)=>mensajes.push(a)};
 vm.createContext(ctx);vm.runInContext(['esMedioCobroElectronico','contieneCobroElectronico'].map(n=>extraer(comun,n)).join('\n')+'\n'+extraer(pago,'procesarErrorPago'),ctx);
 return {ctx,campos,mensajes};
}
for(const tipo of ['T1','MP','TC']){
 const valores=[{id:1,tcf_id:tipo,importe:11862.32,clover:tipo==='T1'?{rb_rec:19.17,rb_aux:1908.2,id_externo:'CLOVER-PAGO-TEST'}:undefined}];const original=JSON.stringify(valores);const t=escenario(valores);
 t.ctx.procesarErrorPago({ok:false,resultado:-2,mensaje:errorAfip});
 assert.equal(t.mensajes.length,1);assert.equal(t.mensajes[0][0],'No se pudo completar la operación de Caja');
 assert(t.mensajes[0][1].includes('El cobro electrónico ya está aprobado'));assert(t.mensajes[0][1].includes(errorAfip));assert(!t.mensajes[0][1].includes('No se pudo procesar el pago'));
 assert.equal(t.mensajes[0][5],'error!');assert(!t.campos.get('#alertaRegistroPago').clases.has('d-none'));assert.equal(t.campos.get('#detalleRegistroPago').texto,errorAfip);
 assert.equal(JSON.stringify(valores),original);casos++;console.log('OK '+tipo+': error de registración no se presenta como rechazo del cobro ni borra valores');
}
{
 const t=escenario([{tcf_id:'EF',importe:100}]);t.ctx.procesarErrorPago({ok:false,mensaje:'Error de registro'});
 assert.equal(t.mensajes[0][0],'Error al Procesar Pago');assert(t.mensajes[0][1].includes('No se pudo procesar el pago'));assert(t.campos.get('#alertaRegistroPago').clases.has('d-none'));casos++;
}
{
 const t=escenario([{tcf_id:'T1',importe:100}]);t.ctx.procesarErrorPago({mensaje:'<img src=x onerror=alert(1)> Fallo CAE'});assert(t.mensajes[0][1].includes('&lt;img'));assert(!t.mensajes[0][1].includes('<img'));casos++;
}
{
 const t=escenario([{tcf_id:'T1',importe:100}]);t.ctx.procesarErrorPago(null);assert(t.mensajes[0][1].includes('Ocurrió un error al procesar el pago'));assert.equal(t.ctx.valoresPago.length,1);casos++;
}
{
 const t=escenario([{tcf_id:'T1',importe:11862.32,clover:{rb_rec:19.17,rb_aux:1908.2,id_externo:'PAGO-TEST'}}]);const original=JSON.stringify(t.ctx.valoresPago);
 Object.assign(t.ctx,{window:{_coTipoActual:'CR'},confirmacionPagoEnCurso:false,confirmacionPagoConfirmada:false,preparandoConfirmacionPago:false,
 actualizarMensajeLoadingGlobal(){},ocultarLoadingGlobal(){},mostrarResultadoCobranzaCtaCte:()=>false,procesarPagoExitoso:()=>exitos++});
 let done,envios=0,exitos=0;t.ctx.$.ajax=()=>{envios++;return {done(f){done=f;return this;},fail(){return this;}};};
 vm.runInContext(extraer(pago,'enviarPayloadAlServidor'),t.ctx);
 t.ctx.enviarPayloadAlServidor([{ins_id:'G00',rb_importe:11862.32,rb_rec:19.17,rb_aux:1908.20}],[],'Facturacion',null);
 done({ok:false,resultado:-2,mensaje:errorAfip});assert.equal(exitos,0);assert.equal(t.ctx.confirmacionPagoConfirmada,false);assert.equal(t.ctx.confirmacionPagoEnCurso,false);assert.equal(JSON.stringify(t.ctx.valoresPago),original);assert(!t.campos.get('#alertaRegistroPago').clases.has('d-none'));
 t.ctx.enviarPayloadAlServidor([{ins_id:'G00',rb_importe:11862.32,rb_rec:19.17,rb_aux:1908.20}],[],'Facturacion',null);
 done({ok:true,debe_imprimir:false,data:[{tco_letra:'A',tco_id:'FA',cm_compte:'0001-TEST'}]});assert.equal(envios,2);assert.equal(exitos,1);assert.equal(t.ctx.confirmacionPagoConfirmada,true);
 assert(t.campos.get('#alertaRegistroPago').clases.has('d-none'));assert.equal(t.campos.get('#detalleRegistroPago').texto,'');
 t.ctx.enviarPayloadAlServidor([],[],'Facturacion',null);assert.equal(envios,2);casos++;console.log('OK Error -2 -> completar registro: conserva pagos, no informa éxito anticipado y limpia aviso sólo al confirmar');
}
{
 const t=escenario([{tcf_id:'T1',importe:100}]);t.ctx.procesarErrorPago({mensaje:errorAfip});t.ctx.conceptosPago={};
 vm.runInContext(extraer(pago,'cargarConceptosPago'),t.ctx);t.ctx.cargarConceptosPago({totalPagar:100});
 assert(t.campos.get('#alertaRegistroPago').clases.has('d-none'));assert.equal(t.campos.get('#detalleRegistroPago').texto,'');casos++;
}
console.log('PASS: '+casos+' escenarios de error de registro con cobro previo aprobado; sin transacciones.');
