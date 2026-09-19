'use strict';
// Regresiones del motor compartido: funciones reales con un adaptador DOM aislado.
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const source=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/pagoFactura.js'),'utf8');
function extract(name){const a=source.search(new RegExp('^function '+name+'\\(','m'));assert(a>=0,name);const end=/^}\r?$/m.exec(source.slice(a));assert(end,name+' cierre');return source.slice(a,a+end.index+1);}
const names=['descartarBorradorPago','consumirBorradorPago','registrarEventosInicioOperacionPago','normalizarTexto','normalizarTextoUpper','convertirNumeroNC','aCentavosMonto','desdeCentavosNC','obtenerTotalNetoCentavos','obtenerTotalOtrosValoresCentavos','obtenerCreditosNCImputados','obtenerTotalCreditosNCCentavos','obtenerCantidadCreditosNCImputados','actualizarBadgeCantidadPagos','consultaNCBloqueaPago','validarSeleccionNCParaFinalizar','actualizarTotalesPago','limpiarTablaFormasPago','eliminarValor','formatearImporteJsonNC','construirJsonUnionesNC','crearEstadoNCVacio','obtenerDescripcionCreditoNC','obtenerObservacionCreditoNC','asegurarFilaSinFormasPago','renderizarCreditosNCEnGrillaPago','clonarObjetoNC','obtenerCreditosBorradorNC','crearBorradorNC','obtenerTotalNcDesdeLista','validarBorradorNC','guardarDetalleCtaCte','actualizarSeleccionadosNC','puedeEditarCreditoNC','guardarImputacionesNC','cambiarImputacionIndividualNC','hayCreditosNCDisponibles','hayCreditosOpcionalesNC','sincronizarDisponibilidadNCConMediosPago','reiniciarImputacionesNC','marcarCreditoNCSeleccionado','aplicarImputacionInicialNC','restaurarSeleccionNC','obtenerMaximoImputableCreditoNC','obtenerTotalNcExceptoCredito','volverACalculoFactura','obtenerIdentidadClientePago','elevarModalCreditoNC','abrirModalTipoMedioPagoDespuesDeNC'];
let rows=[],errors=[],modals=[],closed=[],styles={},selection=[];
const elements=new Map(),handlers=new Map();
function matches(row,selector){return selector.split(',').some(part=>{if(part.includes('fila-credito-nc'))return row.includes('class="fila-credito-nc"');if(part.includes('data-valor-id'))return row.includes(part.match(/data-valor-id="[^"]+"/)[0]);if(part.includes('fila-valor'))return row.includes('class="fila-valor"');if(part.includes('rowSinFormasPago'))return row.includes('id="rowSinFormasPago"');return false;});}
function $(selector){if(typeof selector==='object')return selector;
 if(!elements.has(selector))elements.set(selector,{value:'',props:{},classes:new Set()});const e=elements.get(selector);
 return {length:selector==='#rowSinFormasPago'?rows.filter(r=>matches(r,selector)).length:1,
 html(v){if(selector==='#tbodyFormasPago')rows=v.match(/<tr\b[\s\S]*?<\/tr>/g)||[];return this;},
 append(v){rows.push(...(v.match(/<tr\b[\s\S]*?<\/tr>/g)||[]));return this;},
 find(sel){return {get length(){return rows.filter(r=>matches(r,sel)).length;},remove(){rows=rows.filter(r=>!matches(r,sel));},each(){}};},
 text(v){if(v===undefined)return e.value;e.value=v;return this;},val(v){if(v===undefined)return e.value;e.value=v;return this;},
 prop(k,v){if(v===undefined)return e.props[k];e.props[k]=v;return this;},
 addClass(v){e.classes.add(v);return this;},removeClass(v){e.classes.delete(v);return this;},toggleClass(v,on){if(on)e.classes.add(v);else e.classes.delete(v);return this;},hasClass(v){return e.classes.has(v);},
 modal(action){modals.push([selector,action]);return this;},fadeOut(ms,cb){cb.call(this);return this;},remove(){rows=rows.filter(r=>!matches(r,selector));return this;},
 on(evt,sel,fn){handlers.set(selector+'|'+evt, {selector:typeof sel==='string'?sel:null,callback:fn||sel});return this;},
 off(evt){handlers.delete(selector+'|'+evt);return this;},one(evt,cb){cb.call(this);return this;},last(){return this;},get(){return {style:{setProperty:(k,v,p)=>{styles.backdrop=[k,v,p];}}};}
 };
}
const quiet={log(){},warn(){},error(){}};
const ctx={document:'document',$ ,console:quiet,escapeHtml:String,formatearNumero:n=>Number(n).toFixed(2),formatearMoneda:n=>Number(n).toFixed(2),
 AbrirMensaje:(t,b,cb,confirmation,buttons,kind,exportCallback)=>{if(t==='Volver')assert.equal(exportCallback,null);cb('SI');},actualizarTotalInstrumento(){},mostrarMensajeError:e=>errors.push(e),modalDetalleCtaCteInstance:{hide(){}},
 revisionPago:0,MODAL_ANIMATION_TIMEOUT:300,window:{_coTipoActual:'CD'},pagoParaRetomar:null,setTimeout:cb=>cb(),abrirModalTipoMedioPago:()=>selection.push('opened'),
 ocultarVentanaPago:async sel=>{closed.push(sel);},
 calcularTotalCC:()=>{ctx.recalculosCC=(ctx.recalculosCC||0)+1;},
 RESULTADO_CONSULTA_NC:Object.fromEntries(['PENDIENTE','ERROR','CON_CREDITOS','SIN_CREDITOS','NO_APLICA'].map(s=>[s,s]))};
vm.createContext(ctx);vm.runInContext(names.map(extract).join('\n'),ctx);
function setup(amount=10000000,count=0,mandatory=false,total=227966.09){
 rows=[];errors=[];modals=[];closed=[];styles={};selection=[];elements.clear();ctx.pagoParaRetomar=null;$('#modalPago').addClass('show');
 ctx.conceptosPago={totalPagar:total,recargos:0,descuentos:0};
 ctx.valoresPago=Array.from({length:count},(_,i)=>({id:i+1,tcf_id:'EF',tcf_desc:'Efectivo',ins_id:'ARS',ins_desc:'Pesos',ins_simbolo:'$',importe:1000}));
 const nc={clave:'NC-1',seleccionado:true,obligatorio:mandatory,importeImputadoCentavos:amount,saldoDisponibleCentavos:amount,creditoOriginal:{cta_id:'TEST',tco_id:'NC',dia_movi:'TEST',cm_compte:'TEST',cv_importe:String(-amount/100),cv_importe_ori:'-300000.00',cv_fecha_carga:'2026-09-17'}};
 ctx.estadoNC={...ctx.crearEstadoNCVacio(),cargado:true,tipoMedioPagoNcDisponible:true,resultadoConsulta:amount?'CON_CREDITOS':'SIN_CREDITOS',disponibles:amount?[nc]:[],seleccionados:amount?[nc]:[]};
 ctx.valoresPago.forEach(v=>rows.push('<tr class="fila-valor" data-valor-id="'+v.id+'">EF</tr>'));
 ctx.renderizarCreditosNCEnGrillaPago();ctx.actualizarTotalesPago();
}
let passed=0;
function test(name,body){setup();body();passed++;console.log('OK '+name);}
test('Modificar una NC actualiza saldo, resumen y JSON sin alterar el credito original',()=>{
 const original=JSON.stringify(ctx.estadoNC.disponibles[0].creditoOriginal);
 assert(ctx.cambiarImputacionIndividualNC('NC-1',4000000));assert.equal(ctx.conceptosPago.totalCreditosNC,40000);assert.equal(ctx.conceptosPago.diferencia,187966.09);
 assert.equal(ctx.construirJsonUnionesNC()[0].cv_importe,'-40000.00');assert.equal(JSON.stringify(ctx.estadoNC.disponibles[0].creditoOriginal),original);
 assert.equal(ctx.estadoNC.disponibles[0].saldoDisponibleCentavos,10000000);
});
test('Quitar deja disponible el credito completo y excluye la union',()=>{
 assert(ctx.cambiarImputacionIndividualNC('NC-1',0));assert.equal(ctx.estadoNC.disponibles[0].saldoDisponibleCentavos,10000000);
 assert.equal(ctx.construirJsonUnionesNC().length,0);assert.equal(ctx.conceptosPago.diferencia,227966.09);assert(rows[0].includes('rowSinFormasPago'));
 assert.equal($('#btnCreditosPago').prop('disabled'),false);
});
test('Un credito retirado se puede agregar desde el selector',()=>{
 ctx.cambiarImputacionIndividualNC('NC-1',0);ctx.crearBorradorNC();ctx.estadoNC.borrador[0].seleccionado=true;ctx.estadoNC.borrador[0].importeImputadoCentavos=2500000;
 ctx.guardarDetalleCtaCte();assert.equal(ctx.conceptosPago.totalCreditosNC,25000);assert.equal(ctx.construirJsonUnionesNC().length,1);
});
test('Credito retirado no reaparece al cambiar o reabrir catalogo',()=>{
 ctx.cambiarImputacionIndividualNC('NC-1',0);ctx.sincronizarDisponibilidadNCConMediosPago([]);ctx.sincronizarDisponibilidadNCConMediosPago([{tcf_id:'NC'}]);
 assert.equal(ctx.estadoNC.seleccionados.length,0);assert.equal(ctx.conceptosPago.totalCreditosNC,0);
});
test('Modificacion manual no es reemplazada por la imputacion automatica',()=>{
 ctx.cambiarImputacionIndividualNC('NC-1',2500000);ctx.sincronizarDisponibilidadNCConMediosPago([]);ctx.sincronizarDisponibilidadNCConMediosPago([{tcf_id:'NC'}]);assert.equal(ctx.conceptosPago.totalCreditosNC,25000);
});
test('NC obligatoria no permite modificar ni quitar',()=>{
 setup(10000000,0,true);assert.equal(ctx.cambiarImputacionIndividualNC('NC-1',0),false);assert.equal(ctx.cambiarImputacionIndividualNC('NC-1',100),false);assert(rows[0].includes('Obligatorio'));
 ctx.crearBorradorNC();ctx.estadoNC.borrador[0].seleccionado=false;assert.equal(ctx.guardarImputacionesNC(ctx.estadoNC.borrador),false);
});
test('Restricciones del cliente y catalogo se conservan',()=>{
 ctx.estadoNC.esConsumidorFinal=true;assert.equal(ctx.cambiarImputacionIndividualNC('NC-1',200),false);
 ctx.estadoNC.esConsumidorFinal=false;ctx.estadoNC.tipoMedioPagoNcDisponible=false;assert.equal(ctx.cambiarImputacionIndividualNC('NC-1',200),false);assert.equal(ctx.validarSeleccionNCParaFinalizar().esValido,false);
});
test('No permite superar saldo disponible ni importe pendiente',()=>{
 assert.equal(ctx.cambiarImputacionIndividualNC('NC-1',10000001),false);
 ctx.valoresPago=[{importe:200000,tcf_id:'EF'}];assert.equal(ctx.cambiarImputacionIndividualNC('NC-1',3000000),false);
 assert.equal(ctx.estadoNC.disponibles[0].importeImputadoCentavos,10000000);
});
test('Rechaza duplicados, negativos y fracciones de centavo',()=>{
 ctx.crearBorradorNC();ctx.estadoNC.borrador.push(ctx.estadoNC.borrador[0]);assert.equal(ctx.guardarImputacionesNC(ctx.estadoNC.borrador),false);
 assert.equal(ctx.cambiarImputacionIndividualNC('NC-1',-1),false);assert.equal(ctx.cambiarImputacionIndividualNC('NC-1',1.5),false);
});
test('Con diferencia cero los creditos siguen accesibles',()=>{
 setup(22796609);assert.equal($('#btnAgregarPago').prop('disabled'),true);assert.equal($('#btnCreditosPago').prop('disabled'),false);
 assert.equal($('#btnFinalizarPago').prop('disabled'),false);assert(ctx.cambiarImputacionIndividualNC('NC-1',10000000));assert.equal($('#btnAgregarPago').prop('disabled'),false);
});
test('Eliminar ultimo efectivo conserva fila NC y resumen',()=>{
 setup(22796609,1);ctx.eliminarValor(1);assert.equal(rows.length,1);assert(rows[0].includes('fila-credito-nc'));assert.equal(ctx.conceptosPago.totalCreditosNC,227966.09);assert.equal($('#btnFinalizarPago').prop('disabled'),false);
});
test('Eliminar efectivo con credito parcial permite continuar',()=>{
 setup(10000000,1);ctx.eliminarValor(1);assert.equal(rows.length,1);assert.equal($('#btnAgregarPago').prop('disabled'),false);assert.equal(ctx.conceptosPago.diferencia,127966.09);
});
test('Eliminar efectivo sin NC y con otro efectivo respeta la grilla',()=>{
 setup(0,1);ctx.eliminarValor(1);assert(rows[0].includes('rowSinFormasPago'));assert.equal($('#btnAgregarPago').prop('disabled'),false);
 setup(10000000,2);ctx.eliminarValor(1);assert.equal(rows.length,2);assert.equal(ctx.valoresPago[0].id,2);
});
test('Al retomar preserva retiros y ediciones, e incorpora obligatorios vigentes',()=>{
 const prev=JSON.parse(JSON.stringify(ctx.estadoNC.disponibles));prev[0].seleccionado=false;prev[0].importeImputadoCentavos=0;
 const mandatory={...ctx.estadoNC.disponibles[0],clave:'OB',obligatorio:true};ctx.estadoNC.disponibles.push(mandatory);
 ctx.restaurarSeleccionNC(prev);assert.equal(ctx.estadoNC.disponibles[0].seleccionado,false);assert.equal(mandatory.seleccionado,true);
 ctx.sincronizarDisponibilidadNCConMediosPago([{tcf_id:'NC'}]);assert.equal(ctx.estadoNC.disponibles[0].seleccionado,false);
});
test('Si el saldo vigente bajo, exige revisar la imputacion al retomar',()=>{
 const prev=JSON.parse(JSON.stringify(ctx.estadoNC.disponibles));ctx.estadoNC.disponibles[0].saldoDisponibleCentavos=100;ctx.restaurarSeleccionNC(prev);assert.equal(ctx.validarSeleccionNCParaFinalizar().esValido,false);
});
test('Modal y fondo de NC quedan por encima del pago',()=>{
 ctx.elevarModalCreditoNC({style:{setProperty:(...args)=>styles.modal=args}});assert.deepEqual(styles.modal,['z-index','5100','important']);assert.deepEqual(styles.backdrop,['z-index','5099','important']);
});
test('Con saldo cubierto no abre automaticamente otro medio',()=>{
 setup(22796609);ctx.abrirModalTipoMedioPagoDespuesDeNC();assert.equal(selection.length,0);
 setup(10000000);ctx.abrirModalTipoMedioPagoDespuesDeNC();assert.equal(selection.length,1);
});
test('Una apertura demorada no revive ventanas al volver',()=>{
 const original=ctx.setTimeout;let callback;ctx.setTimeout=cb=>{callback=cb;};ctx.abrirModalTipoMedioPagoDespuesDeNC();ctx.revisionPago++;callback();assert.equal(selection.length,0);ctx.setTimeout=original;
});
(async()=>{
 for(const [co,destino] of [['CD','#modalFacturasPendientes'],['CC','#modalCuentaCorriente'],['CR','#modalCalculoFactura'],['CF','#modalCalculoFactura']]){
  setup(10000000,1);ctx.recalculosCC=0;ctx.window._coTipoActual=co;$('#txtClienteIdPago').val('TEST');ctx.volverACalculoFactura();
  await new Promise(setImmediate);
  assert(modals.some(([s,a])=>s===destino&&a==='show'));assert.equal(closed.at(-1),'#modalPago');assert(closed.includes('#modalDetalleCtaCte'));
  assert.equal(ctx.recalculosCC,co==='CC'?1:0,'Al volver debe recalcular la seleccion y habilitar Seguir en CC');
  assert.equal(ctx.pagoParaRetomar.valores.length,1);assert.equal(ctx.pagoParaRetomar.creditos.length,1);passed++;console.log('OK Volver '+co+' conserva borrador y abre '+destino);
 }
 for(const boundary of ['show','#btnCancelarSeleccionFacturas','#btnCancelarCC','#btnCancelarCliente','#btnCobrarSeleccionVFP']){
  setup(3000,0,false,305267.60);ctx.window._coTipoActual='CD';$('#txtClienteIdPago').val('C0189311');
  const second={...ctx.estadoNC.disponibles[0],clave:'NC-2',saldoDisponibleCentavos:7351535,importeImputadoCentavos:7351535,
   creditoOriginal:{...ctx.estadoNC.disponibles[0].creditoOriginal,cm_compte:'NC-2',cv_importe:'-73515.35'}};
  ctx.estadoNC.disponibles.push(second);
  const serverCredits=ctx.clonarObjetoNC(ctx.estadoNC.disponibles);
  assert(ctx.cambiarImputacionIndividualNC('NC-1',0));
  assert(ctx.cambiarImputacionIndividualNC('NC-2',3000000));
  ctx.volverACalculoFactura();await new Promise(setImmediate);
  assert.equal(ctx.pagoParaRetomar.creditos[1].importeImputadoCentavos,3000000);
  ctx.registrarEventosInicioOperacionPago();
  if(boundary==='show'){
   const modal={};handlers.get('#modalIdentificarCliente|show.bs.modal.inicioOperacionPago').callback.call(modal,{target:modal});
  }else{
   const h=handlers.get('document|click.inicioOperacionPago');assert(h.selector.split(', ').includes(boundary));h.callback();
   assert(!h.selector.includes('#btnVolverPago'),'Volver un paso conserva el borrador');
  }
  const previous=ctx.consumirBorradorPago('CD');assert.equal(previous,null);
  // Datos vigentes de una nueva consulta, sin restaurar la selección abandonada.
  ctx.estadoNC={...ctx.crearEstadoNCVacio(),cargado:true,tipoMedioPagoNcDisponible:true,disponibles:serverCredits};
  assert(ctx.aplicarImputacionInicialNC({incluirOpcionales:true}));
  assert.deepEqual(Array.from(ctx.estadoNC.seleccionados,c=>c.importeImputadoCentavos),[3000,7351535]);
  passed++;console.log('OK Nueva operacion '+boundary+' descarta 30000 y precarga 30 + 73515.35');
 }
 setup();ctx.window._coTipoActual='CD';$('#txtClienteIdPago').val('C0189311');
 ctx.volverACalculoFactura();await new Promise(setImmediate);
 assert(ctx.consumirBorradorPago('CD'),'Volver solo a las facturas permite retomar');
 assert.equal(ctx.consumirBorradorPago('CD'),null,'El borrador se consume una sola vez');
 passed++;console.log('OK Retomar dentro de la misma operacion conserva y consume el borrador una sola vez');
 console.log('TOTAL: '+passed+' escenarios aprobados. Sin transacciones ni servidor.');
})().catch(e=>{console.error(e);process.exitCode=1;});
