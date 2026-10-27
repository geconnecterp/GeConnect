const fs = require('node:fs'), vm = require('node:vm'), assert = require('node:assert/strict');
const modelo = require('../wwwroot/js/app/cloverPago.js'), fixtures = require('./Clover.Fixtures.cjs');
const contexto = { instrumento: { ins_id: 'G00', ins_desc: 'Clover de prueba' }, coTipo: 'CR', saldo: 50000.23 };
let casos = 0;
function check(nombre, fn) { fn(); casos++; console.log('OK ' + nombre); }
async function caso(nombre, fn) { await fn(); casos++; console.log('OK ' + nombre); }
function sesion(escenario = 'aprobado', aprobar = () => {}) {
    const transporte = fixtures.crearTransporte(() => escenario), s = new modelo.Sesion(transporte, () => {}, aprobar);
    return { s, transporte };
}
(async () => {
    for (const [base,p,saldo,rec,total] of [
        ['50000.23',12,50000.23,6000.03,56000.26], ['0.05',10,1,0.01,0.06], ['1.05',-10,2,-0.11,0.94],
        ['100',0,100,0,100], ['100',-5,100,-5,95], ['4,35',12,10,0.52,4.87]
    ]) check('Cálculo decimal '+base+'/'+p, () => { const r=modelo.calcular(base,{recargo_porcentaje:p},saldo);assert.equal(r.recargo,rec);assert.equal(r.total,total); });
    for (const base of ['', '0','-1','1e3','1abc','1.001','1,2,3','99999999999999']) check('Monto inválido '+base, () => assert.throws(() => modelo.calcular(base,{recargo_porcentaje:0},100)));
    check('No supera saldo', () => assert.throws(() => modelo.calcular(101,{recargo_porcentaje:0},100)));
    check('No permite descuento que elimine el total', () => assert.throws(() => modelo.calcular(100,{recargo_porcentaje:-100},100)));
    for (const id of ['AB','ABC','AB-1','A B1','A'.repeat(31),'']) check('ID no llega al transporte '+id, () => assert.throws(() => modelo.normalizarId(id)));
    check('ID manual uppercase sin relleno', () => assert.equal(modelo.normalizarId(' ab12 '),'AB12'));
    check('Clover se identifica por G00, no descripción', () => { assert(modelo.esInstrumento({ins_id:'g00'}));assert(!modelo.esInstrumento({ins_id:'OT',ins_desc:'Clover'})); });
    await caso('T1 abre sin instrumento y utiliza G0 devuelto por el servidor', async () => {
        let enviada;
        const t=fixtures.crearTransporte();t.opciones=async()=>({ok:true,opciones:fixtures.opciones.map(o=>({...o,ins_id:'G0'}))});const enviar=t.procesar;t.procesar=async r=>{enviada=r;return enviar(r);};
        const s=new modelo.Sesion(t);await s.abrir({...contexto,instrumento:{ins_id:'DEF'},medio:{tcf_id:'T1',tcf_desc:'POS Clover'}});
        assert.equal(s.estado,'PREPARANDO');assert.equal(s.contexto.instrumento.ins_id,'G0');await s.enviar('POS');assert.equal(enviada.ins_id,'G0');
    });
    for (const [indice,recargo,total] of [[0,0,100],[1,13.25,113.25],[2,19.17,119.17]]) await caso('Cuotas del analista: plan '+(indice+1), async () => {
        const t=fixtures.crearTransporte();t.opciones=async()=>({ok:true,simulacion:true,opciones:fixtures.opcionesAnalista});
        const s=new modelo.Sesion(t);await s.abrir({...contexto,saldo:100,instrumento:{tcf_id:'T1',ins_id:''}});s.actualizar({plan:String(indice)});
        assert.equal(s.cotizacion().recargo,recargo);assert.equal(s.cotizacion().total,total);await s.enviar('POS');
        assert.equal(t.llamadas[0].ins_id,'G00');assert.equal(t.llamadas[0].co_tipo,'CR');assert.equal(t.llamadas[0].pos_plan,indice+1);assert.equal(t.llamadas[0].cuotas_reales,indice+1);assert.equal(t.llamadas[0].recargo,recargo);assert.equal(t.llamadas[0].monto_total,total);
    });
    await caso('T1 sin catálogo queda en Clover sin ejecutar pagos', async () => {
        let pagos=0;const s=new modelo.Sesion({opciones:async()=>({ok:true,opciones:[]}),procesar:()=>pagos++});await s.abrir({...contexto,instrumento:{tcf_id:'T1',ins_id:''}});assert.equal(s.estado,'BLOQUEADO');assert.equal(pagos,0);assert(s.puedeCerrar());
    });
    await caso('POS: plan 12 no se confunde con 5 cuotas; QR y total con recargo', async () => {
        let resultado; const {s,transporte}=sesion('aprobado',r=>resultado=r);await s.abrir(contexto);s.actualizar({plan:'1',qr:true});
        assert(await s.enviar('POS'));const req=transporte.llamadas[0];assert.equal(req.paymentid,'');assert.equal(req.pos_plan,12);assert.equal(req.cuotas_reales,5);assert.equal(req.monto_total,56000.26);assert.equal(req.recargo,6000.03);assert(req.qr);assert.equal(resultado.accion,'FINALIZAR');assert(resultado.paymentId.startsWith('SIMULADO'));assert(resultado.simulacion);
        assert(!s.actualizar({base:'1'}));assert(!s.cerrar());assert(!await s.enviar('POS'));assert.equal(transporte.llamadas.length,1);
    });
    await caso('Recuperación manual conserva ID/lote/cupón y no usa pago nuevo', async () => {
        let r;const {s,transporte}=sesion('aprobado',x=>r=x);await s.abrir(contexto);s.actualizar({paymentid:'ab1234'});await s.enviar('MANUAL');
        assert.equal(transporte.llamadas[0].paymentid,'AB1234');assert.equal(r.datosSP.rb_dato1_valor,'AB1234');assert.equal(r.datosSP.rb_dato2_valor,'000012');assert.equal(r.datosSP.rb_dato3_valor,'001234');assert(Object.isFrozen(s.resultado.datosSP));
    });
    await caso('ID de exactamente 3 no dispara el SP que lo cobraría', async () => {const {s,transporte}=sesion();await s.abrir(contexto);s.actualizar({paymentid:'abc'});assert(!await s.enviar('MANUAL'));assert.equal(transporte.llamadas.length,0);});
    await caso('Pago parcial solicita completar, no finalizar', async () => {let r;const {s}=sesion('aprobado',x=>r=x);await s.abrir({...contexto,saldo:150});s.actualizar({base:50});await s.enviar('POS');assert.equal(r.accion,'COMPLETAR');assert.equal(r.saldoRestante,100);});
    await caso('Cobranza Diferida filtra recargos y descuentos antes del POS', async () => {const {s}=sesion();await s.abrir({...contexto,coTipo:'CD'});assert.equal(s.opciones.length,1);assert.equal(s.opciones[0].recargo_porcentaje,0);s.actualizar({plan:'1'});assert(!await s.enviar('POS'));});
    for (const escenario of ['rechazado','cancelado','imputado','inexistente']) await caso('Resultado conocido '+escenario, async () => {let aprobaciones=0;const {s}=sesion(escenario,()=>aprobaciones++);await s.abrir(contexto);assert(!await s.enviar('POS'));assert.equal(aprobaciones,0);assert(s.puedeCerrar());});
    for (const escenario of ['timeout','sin-datos','json-invalido']) await caso('Resultado incierto '+escenario+' no permite otro cobro', async () => {const {s,transporte}=sesion(escenario);await s.abrir(contexto);await s.enviar('POS');assert.equal(s.estado,'INCIERTO');assert(!s.cerrar());assert(!await s.enviar('POS'));assert.equal(transporte.llamadas.length,1);});
    await caso('Espera: doble clic, editar, volver o abrir otra operación no altera solicitud', async () => {
        const {s,transporte}=sesion('espera');await s.abrir(contexto);const pendiente=s.enviar('POS');assert.equal(s.estado,'ESPERANDO');assert(!await s.enviar('POS'));assert(!s.actualizar({base:1}));assert(!s.cerrar());assert(!await s.abrir({...contexto,saldo:1}));assert.equal(transporte.llamadas.length,1);transporte.estado.resolver();assert(await pendiente);
    });
    await caso('Cerrar durante carga descarta una respuesta tardía', async () => {let resolver;const s=new modelo.Sesion({opciones:()=>new Promise(r=>resolver=r)});const p=s.abrir(contexto);assert(s.cerrar());resolver({ok:true,opciones:fixtures.opciones});await p;assert.equal(s.estado,'CERRADO');});
    await caso('No sustituye G00 solicitado por G0 devuelto', async () => {const s=new modelo.Sesion({opciones:async()=>({ok:true,opciones:fixtures.opciones.map(x=>({...x,ins_id:'G0'}))})});await s.abrir(contexto);assert.equal(s.estado,'BLOQUEADO');});
    for(const lista of [null,[],[{ins_id:'G00',cuota:1,pos_plan:undefined,recargo_porcentaje:0,pos_desc:'Una'}]]) await caso('Catálogo inválido '+JSON.stringify(lista), async()=>{const s=new modelo.Sesion({opciones:async()=>({ok:true,opciones:lista})});await s.abrir(contexto);assert.equal(s.estado,'BLOQUEADO');});
    await caso('Fallo de registración conserva aprobado y no repite débito', async()=>{const {s,transporte}=sesion('aprobado',()=>{throw Error('Sin registro');});await s.abrir(contexto);await s.enviar('POS');assert.equal(s.estado,'APROBADO');assert(s.resultado);assert(!await s.enviar('POS'));assert.equal(transporte.llamadas.length,1);});
    await caso('RespuestaDto del SP decide el rechazo aunque traiga datos', async()=>{
        const t=fixtures.crearTransporte();t.procesar=async req=>({...fixtures.respuesta(req),resultado:2,resultado_msj:'Rechazo del SP'});
        const s=new modelo.Sesion(t);await s.abrir(contexto);assert(!await s.enviar('POS'));assert.equal(s.estado,'RECHAZADO');assert.equal(s.mensaje,'Rechazo del SP');assert.equal(s.resultado,null);
    });
    await caso('Varios pagos consecutivos no consultan el estado del POS', async()=>{
        const t=fixtures.crearTransporte();t.estado=()=>{throw Error('No consultar estado');};let total=0;
        const s=new modelo.Sesion(t,()=>{},v=>{total+=v.total;return true;});
        await s.abrir({...contexto,saldo:150});s.actualizar({base:50});assert(await s.enviar('POS'));assert.equal(s.estado,'CERRADO');
        await s.abrir({...contexto,saldo:100});assert(await s.enviar('POS'));assert.equal(total,150);assert.equal(t.llamadas.length,2);
    });
    // Probar la vista real con un adaptador DOM mínimo: no hay navegador ni servidores remotos.
    class Elemento {
        constructor(){this.value='';this.disabled=false;this.children=[];this.events={};this.textContent='';}
        addEventListener(n,f){this.events[n]=f;}setAttribute(n,v){this[n]=v;}replaceChildren(){this.children=[];}appendChild(e){this.children.push(e);}focus(){this.focused=true;}select(){this.selected=true;}
    }
    const ids=Object.fromEntries(['formDetalleClover','lblInstrumentoClover','lblSaldoClover','txtMontoClover','cmbCuotaClover','chkQrClover','txtRecargoClover','txtTotalClover','txtPaymentIdClover','btnPagarClover','btnManualClover','btnVolverClover','btnCerrarClover','estadoClover'].map(id=>[id,new Elemento()]));
    const root={querySelector:s=>ids[s.slice(1)],ownerDocument:{createElement:()=>new Elemento()},dataset:{}};
    const win={CloverModelo:modelo,jQuery:()=>({modal(){},on(){}})};
    vm.runInNewContext(fs.readFileSync(__dirname+'/../wwwroot/js/app/cloverPagoVista.js','utf8'),{window:win});
    await caso('Vista: cuotas, recargo, total y controles se actualizan',async()=>{const v=win.CloverPago.crearVista(root,fixtures.crearTransporte());await v.abrir(contexto);assert.equal(ids.cmbCuotaClover.children[1].textContent,'5 cuotas (12 %)');v.sesion.actualizar({plan:'1'});assert.equal(ids.txtRecargoClover.value,'$ 6,000.03');assert.equal(ids.txtTotalClover.value,'$ 56,000.26');assert(!ids.btnPagarClover.disabled);assert(ids.btnManualClover.disabled);v.sesion.actualizar({paymentid:'ABC'});assert(ids.btnManualClover.disabled);v.sesion.actualizar({paymentid:'ABCD'});assert(!ids.btnManualClover.disabled);});
    await caso('Vista: Enter avanza sin iniciar el pago',async()=>{const v=win.CloverPago.crearVista(root,fixtures.crearTransporte());await v.abrir(contexto);let prevented=false;ids.txtMontoClover.events.keydown({key:'Enter',preventDefault(){prevented=true;}});assert(prevented);assert(ids.cmbCuotaClover.focused);});
    await caso('Vista: backend desconectado muestra aviso y no permite pagar',async()=>{const v=win.CloverPago.crearVista(root,{opciones:async()=>({ok:false,mensaje:'Sin conexión configurada'})});await v.abrir(contexto);assert(ids.btnPagarClover.disabled);assert(ids.btnManualClover.disabled);assert.equal(ids.cmbCuotaClover.children[0].textContent,'Sin cuotas disponibles');assert(!ids.btnVolverClover.disabled);});
    const src=fs.readFileSync(__dirname+'/../wwwroot/js/app/pagoFactura.js','utf8');
    const clasificador=fs.readFileSync(__dirname+'/../wwwroot/js/app/siteGen.js','utf8').match(/^function esMedioClover\([^]*?^}/m)[0];
    const extraer=n=>clasificador+'\n'+src.match(new RegExp('^function '+n+'\\([^]*?^}', 'm'))[0];
    check('Selector G00 abre Clover y no Transferencia',()=>{let abierto=0;const c={window:{CloverPago:{abrir:x=>{abierto++;assert.equal(x.instrumento.ins_id,'G00');}},_coTipoActual:'CR'},conceptosPago:{diferencia:100},construirJsonValores:()=>[],construirJsonUnionesNC:()=>[],mostrarMensajeError:()=>{throw Error('Fallo');},esInstrumentoDocumento:()=>false};vm.runInNewContext(extraer('abrirDetalleClover')+'\n'+extraer('abrirModalDetalleSegunTipo'),c);c.abrirModalDetalleSegunTipo({ins_id:'G00'},{tcf_id:'T1'});assert.equal(abierto,1);});
    for(const ins of [null,{ins_id:'DEF',ins_desc:'Sin instrumentos disponibles'},{ins_id:'G0',ins_desc:'POS Clover'}]) check('T1 nunca cae a Transferencia: '+JSON.stringify(ins),()=>{
        let abierto=0;const c={window:{CloverPago:{abrir:x=>{abierto++;assert.equal(x.instrumento.tcf_id,'T1');assert.notEqual(x.instrumento.ins_id,'DEF');}},_coTipoActual:'CR'},conceptosPago:{diferencia:100},construirJsonValores:()=>[],construirJsonUnionesNC:()=>[],mostrarMensajeError:()=>{throw Error('Fallo');},esInstrumentoDocumento:()=>false,console:{log(){},warn(){}},abrirModalDetalleTransferencia:()=>{throw Error('No debe abrir Transferencia');}};
        vm.runInNewContext(extraer('abrirDetalleClover')+'\n'+extraer('abrirModalDetalleSegunTipo')+'\n'+extraer('procesarInstrumentos')+'\n'+extraer('cargarInstrumentos'),c);
        c.cargarInstrumentos({tcf_id:'T1',tcf_desc:'POS Clover'});assert.equal(abierto,1);c.procesarInstrumentos(ins?[ins]:[],{tcf_id:'T1',tcf_desc:'POS Clover'});assert.equal(abierto,2);
    });
    for (const [totalBase, saldoFinal, finalizaciones] of [[100,0,1],[150,50,0]]) await caso('Motor real: canónico, recargo único y finalización para base '+totalBase, async () => {
        let apertura, finales=0, filas=0;
        const c={window:{_coTipoActual:'CR',CloverPago:{abrir:x=>apertura=x}},conceptosPago:{totalPagar:totalBase,recargos:0,descuentos:0,diferencia:totalBase},valoresPago:[],valorIdCounter:0,console:{log(){},warn(){}},formatearNumero:String,mostrarMensajeError:()=>{},construirJsonUnionesNC:()=>[],aCentavosMonto:n=>Math.round(Number(n)*100),agregarFilaValor:()=>filas++,finalizarPago:()=>finales++,setTimeout:f=>f(),actualizarTotalesPago:()=>{c.conceptosPago.diferencia=c.obtenerTotalNetoCentavos()/100-c.valoresPago.reduce((s,v)=>s+v.importe,0);}};
        vm.runInNewContext(['aplicarCloverAPago','obtenerTotalNetoCentavos','construirJsonValores','abrirDetalleClover'].map(extraer).join('\n'),c);
        c.abrirDetalleClover({ins_id:'G00'},{tcf_id:'T1'});
        const v={ins_id:'V01',rb_dato1_valor:'POS001',rb_dato2_valor:'000012',rb_dato3_valor:'001234',rb_importe:112,rb_rec:12,rb_aux:12,id_externo:'POS001'};
        await apertura.alAprobar({valorCanonico:v,datosSP:{ins_id:'V01'},recargo:12,total:112});
        assert.equal(c.conceptosPago.diferencia,saldoFinal);assert.equal(finales,finalizaciones);assert.equal(filas,1);
        c.aplicarCloverAPago({valorCanonico:v,datosSP:{ins_id:'V01'},recargo:12,total:112});assert.equal(c.valoresPago.length,1);assert.equal(c.obtenerTotalNetoCentavos(),(totalBase+12)*100);
        const json=c.construirJsonValores();assert.equal(json[0].rb_importe,112);assert.equal(json[0].rb_rec,12);assert.equal(json[0].rb_aux,12);assert.equal(json[0].rb_dato2_valor,'000012');assert.equal(json[0].id_externo,'POS001');assert.equal(json[0].rb_nro_valor,'001');
    });
    await caso('Motor real: dos Clover parciales con distintos recargos finalizan una sola vez',async()=>{
        let apertura,finales=0;
        const c={window:{_coTipoActual:'CR',CloverPago:{abrir:x=>apertura=x}},conceptosPago:{totalPagar:100,recargos:0,descuentos:0,diferencia:100},valoresPago:[],valorIdCounter:0,formatearNumero:String,mostrarMensajeError:()=>{},construirJsonUnionesNC:()=>[],aCentavosMonto:n=>Math.round(Number(n)*100),agregarFilaValor:()=>{},finalizarPago:()=>finales++,setTimeout:f=>f(),actualizarTotalesPago:()=>{c.conceptosPago.diferencia=c.obtenerTotalNetoCentavos()/100-c.valoresPago.reduce((s,v)=>s+v.importe,0);}};
        vm.runInNewContext(['aplicarCloverAPago','obtenerTotalNetoCentavos','construirJsonValores','abrirDetalleClover'].map(extraer).join('\n'),c);
        const uno={ins_id:'V01',id_externo:'POS001',rb_dato1_valor:'POS001',rb_importe:56,rb_rec:12,rb_aux:6};
        const dos={...uno,id_externo:'POS002',rb_dato1_valor:'POS002',rb_importe:59.59,rb_rec:19.17,rb_aux:9.59};
        c.abrirDetalleClover({ins_id:'G00'},{tcf_id:'T1'});await apertura.alAprobar({valorCanonico:uno,total:56,recargo:6});assert.equal(c.conceptosPago.diferencia,50);assert.equal(finales,0);
        c.abrirDetalleClover({ins_id:'G00'},{tcf_id:'T1'});assert.equal(apertura.saldo,50);await apertura.alAprobar({valorCanonico:dos,total:59.59,recargo:9.59});assert.equal(c.conceptosPago.diferencia,0);assert.equal(finales,1);
        await apertura.alAprobar({valorCanonico:dos,total:59.59,recargo:9.59});assert.equal(finales,1);assert.equal(c.valoresPago.length,2);
        const vals=c.construirJsonValores();assert.equal(vals[0].rb_rec,12);assert.equal(vals[0].rb_aux,6);assert.equal(vals[1].rb_rec,19.17);assert.equal(vals[1].rb_aux,9.59);assert.equal(vals[1].rb_nro_valor,'002');
    });
    await caso('Contrato DBA: porcentaje e importe diferentes, con base correcta en el modelo',async()=>{
        const t=fixtures.crearTransporte();t.opciones=async()=>({ok:true,opciones:fixtures.opcionesAnalista});let resultado;
        const s=new modelo.Sesion(t,()=>{},r=>{resultado=r;return true;});await s.abrir({...contexto,saldo:9954.12});s.actualizar({plan:'2'});assert(await s.enviar('POS'));
        assert.equal(resultado.valorCanonico.rb_importe,11862.32);assert.equal(resultado.valorCanonico.rb_rec,19.17);assert.equal(resultado.valorCanonico.rb_aux,1908.20);assert.equal(resultado.recargo,1908.20);assert.equal(resultado.recargoPorcentaje,19.17);assert.equal(resultado.base,9954.12);
    });
    check('Ingreso, carga y finalización no consultan CloverEstado',()=>{
        for(const p of ['pagoFactura.js','cambioValores.js','cloverPagoVista.js'])assert(!/CloverPago\??\.recuperar|CloverEstado|dataset.estadoUrl/.test(fs.readFileSync(__dirname+'/../wwwroot/js/app/'+p,'utf8')));
    });
    for(const v of ['Inicio','PDiferido' ,'CobranzaCtaCte','CambioValores'])check('Vista '+v+' incluye componente',()=>{const html=fs.readFileSync(__dirname+`/../Areas/Facturacion/Views/${v}/Index.cshtml`,'utf8');assert(html.includes('_detalleClover.cshtml'));assert(html.includes('cloverPago.js'));assert(html.includes('cloverPagoVista.js'));});
    console.log(`PASS: ${casos} escenarios Clover con transporte exclusivamente simulado. Sin SQL, HTTP bancario ni registros de Caja.`);
})().catch(e=>{console.error(e);process.exitCode=1;});
