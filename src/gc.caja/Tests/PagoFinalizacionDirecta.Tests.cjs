const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
const pago=fs.readFileSync(__dirname+'/../wwwroot/js/app/pagoFactura.js','utf8');
const comun=fs.readFileSync(__dirname+'/../wwwroot/js/app/siteGen.js','utf8');
const cambio=fs.readFileSync(__dirname+'/../wwwroot/js/app/cambioValores.js','utf8');
const extraer=(s,n)=>{const m=s.match(new RegExp('^function '+n+'\\([^]*?^}', 'm'));assert(m,n);return m[0];};
let casos=0;
function contexto(valores){
 const mensajes=[],envios=[];
 const ctx=vm.createContext({console:{log(){},warn(){},error(){}},window:{},
 valoresPago:valores,preparandoConfirmacionPago:false,confirmacionPagoEnCurso:false,confirmacionPagoConfirmada:false,
 conceptosPago:{totalPagar:100,totalCreditosNC:0,totalOtrosValores:100,totalAplicado:100},
 obtenerCantidadCreditosNCImputados:()=>0,validarSeleccionNCParaFinalizar:()=>({esValido:true}),
 validarDiferenciaParaFinalizar:()=>({permitir:true}),construirJsonValores:()=>[{ins_id:'TEST',rb_importe:100}],
 construirJsonUnionesNC:()=>[],formatearMoneda:String,escapeHtml:String,toastr:{error(){}},
 mostrarMensajeError:mensaje=>mensajes.push(mensaje),AbrirMensaje:(...a)=>mensajes.push(a),
 enviarPagoAlServidor:(...a)=>{envios.push(a);ctx.confirmacionPagoEnCurso=true;},
 $:()=>({modal(){}}),setTimeout:f=>f()});
 vm.runInContext(extraer(comun,'esMedioCobroElectronico')+'\n'+extraer(comun,'contieneCobroElectronico')+'\n'+extraer(pago,'finalizarPago'),ctx);
 return {ctx,mensajes,envios};
}
const medios=[
 [{tcf_id:'TC',importe:100},true], [{tcf_id:' td ',importe:100},true],
 [{tcf_id:'MP',importe:100},true], [{tcf_id:'OT',ins_desc:'Clover POS',importe:100},true],
 [{tcf_id:'OT',tcf_desc:'Mercado Pago QR',importe:100},true],
 [{tcf_id:'OT',ins_desc:'Mercado Libre',importe:100},true],
 [{tcf_id:'OT',tcf_desc:'Tarjeta de crédito VISA',importe:100},true],
 [{tcf_id:'BA',ins_desc:'Transferencia bancaria',importe:100},false],
 [{tcf_id:'EF',importe:100},false], [{tcf_id:'CH',importe:100},false],
 [{tcf_id:'DO',importe:100},false], [{tcf_id:'VA',importe:100},false],
 [{tcf_id:'MU',importe:100},false], [{tcf_id:'TC',importe:0},false],
 [{tcf_id:'MP',importe:-1},false], [{tcf_id:'OT',ins_desc:'Otro medio',importe:100},false]
];
for(const co of ['CR','CF','CD','CC'])for(const [valor,directo] of medios){
 const {ctx,mensajes,envios}=contexto([valor]);ctx.window._coTipoActual=co;
 ctx.finalizarPago();assert.equal(envios.length,directo?1:0,`${co}/${valor.tcf_id}`);
 assert.equal(mensajes.length,directo?0:1);
 if(directo){ctx.finalizarPago();assert.equal(envios.length,1);}
 else {mensajes[0][2]('NO');assert.equal(envios.length,0);}
 casos++;
}
for(const fallo of ['NC','diferencia','payload']){
 const {ctx,mensajes,envios}=contexto([{tcf_id:'TC',importe:100}]);
 if(fallo==='NC')ctx.validarSeleccionNCParaFinalizar=()=>({esValido:false,errores:['NC inválida']});
 if(fallo==='diferencia')ctx.validarDiferenciaParaFinalizar=()=>({permitir:false,mensaje:'Saldo pendiente'});
 if(fallo==='payload')ctx.construirJsonValores=()=>{throw Error('Valor inválido');};
 ctx.finalizarPago();assert.equal(envios.length,0);assert.equal(mensajes.length,1);casos++;
}
{
 const {ctx,envios}=contexto([{tcf_id:'EF',importe:40},{tcf_id:'MP',importe:60}]);
 ctx.construirJsonUnionesNC=()=>[{cv_importe:10}];ctx.finalizarPago();
 assert.equal(envios.length,1);assert.equal(envios[0][1][0].cv_importe,10);casos++;
}
// CD: bloquear también la consulta previa, antes del POST que registra el cobro.
for(const resultado of ['OK','rechazo','conexion']){
 let consulta,consultas=0,posts=0;
 const ctx=vm.createContext({console:{log(){},warn(){},error(){}},window:{_coTipoActual:'CD',_contextoOperacionActual:'COBRANZA'},
 preparandoConfirmacionPago:false,confirmacionPagoEnCurso:false,confirmacionPagoConfirmada:false,
 conceptosPago:{totalValores:100},formatearMoneda:String,mostrarLoadingGlobal(){},actualizarMensajeLoadingGlobal(){},ocultarLoadingGlobal(){},AbrirMensaje(){},
 construirArrayCancelar:x=>x,$:Object.assign(()=>({modal(){}}),{ajax:o=>{consulta=o;consultas++;}}),
 enviarPayloadAlServidor:()=>{ctx.preparandoConfirmacionPago=false;ctx.confirmacionPagoEnCurso=true;posts++;}});
 vm.runInContext(extraer(pago,'enviarPagoAlServidor'),ctx);
 ctx.enviarPagoAlServidor([],[]);ctx.enviarPagoAlServidor([],[]);assert.equal(consultas,1);
 if(resultado==='OK'){consulta.success({ok:true,lista:[{}]});assert.equal(posts,1);ctx.enviarPagoAlServidor([],[]);assert.equal(consultas,1);}
 else{if(resultado==='rechazo')consulta.success({ok:false});else consulta.error({status:0},'error','Sin conexión');assert.equal(posts,0);assert.equal(ctx.preparandoConfirmacionPago,false);}
 casos++;
}
// CV e IV reutilizan el mismo criterio y conservan su envío y controles originales.
for(const [valor,directo] of medios){
 let envios=0,resumenes=0;
 const ctx=vm.createContext({valoresCambioValores:[{...valor,rb_importe:valor.importe}],finalizandoCambioValores:false,
 confirmarOperacionCambioValores:()=>{envios++;ctx.finalizandoCambioValores=true;},renderizarConfirmacionCambioValores:()=>resumenes++,
 $:()=>({modal(){}}),setTimeout:f=>f(),mostrarMensajeCambioValores(){}});
 vm.runInContext(extraer(comun,'esMedioCobroElectronico')+'\n'+extraer(comun,'contieneCobroElectronico')+'\n'+extraer(cambio,'prepararConfirmacionCambioValores'),ctx);
 ctx.prepararConfirmacionCambioValores();assert.equal(envios,directo?1:0);assert.equal(resumenes,directo?0:1);
 if(directo){ctx.prepararConfirmacionCambioValores();assert.equal(envios,1);}casos++;
}
for(const archivo of ['Inicio','PDiferido','CobranzaCtaCte']){
 const vista=fs.readFileSync(__dirname+`/../Areas/Facturacion/Views/${archivo}/Index.cshtml`,'utf8');
 assert(vista.includes('pagoFactura.js'));casos++;
}
// Ruta de captura temporal y contrato al SP: conservar instrumento, fecha y referencia.
for(const [valor,directo] of medios.filter(([v,d])=>d)){
 let abierto=0;
 const ctx=vm.createContext({console:{log(){},warn(){},error(){}},
 esInstrumentoDocumento:()=>false,abrirModalDetalleTransferencia:()=>abierto++});
 vm.runInContext(extraer(comun,'esMedioCobroElectronico')+'\n'+extraer(pago,'abrirModalDetalleSegunTipo'),ctx);
 ctx.abrirModalDetalleSegunTipo({ins_id:'VISA',ins_desc:valor.ins_desc||'VISA'},valor);assert.equal(abierto,1);casos++;
}
{
 const ctx=vm.createContext({console:{log(){},warn(){},error(){}},Date,
 valoresPago:[{tcf_id:'TC',tcf_desc:'Tarjeta crédito',ins_id:'VISA',ins_desc:'VISA',importe:100,detalle:{nro_transferencia:'ABC123',fecha_transferencia:'2026-10-09'}}],
 formatearNumero:String,esInstrumentoDocumento:()=>false,evaluarReglasPago:()=>({esValido:true,vuelto:0}),
 obtenerVueltoEfectivoCentavos:()=>0});
 vm.runInContext(extraer(comun,'esMedioCobroElectronico')+'\n'+extraer(pago,'construirJsonValores'),ctx);
 const json=ctx.construirJsonValores();assert.equal(json[0].ins_id,'VISA');assert.equal(json[0].rb_importe,100);
 assert.equal(json[0].rb_fecha_valor,'2026-10-09');assert.equal(json[0].rb_dato3_valor,'000000000ABC123');casos++;
 ctx.valoresPago[0].detalle=null;assert.throws(()=>ctx.construirJsonValores(),/referencia y fecha/);casos++;
}
const layout=fs.readFileSync(__dirname+'/../Views/Shared/_Layout.cshtml','utf8');assert(layout.includes('siteGen.js'));
console.log(`PASS: ${casos} escenarios de finalización directa, confirmación tradicional, validaciones y doble envío; sin cobros reales.`);
