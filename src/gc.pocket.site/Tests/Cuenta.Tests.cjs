const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const source=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/seguridad/cuenta.js'),'utf8');
const ctx={ $:()=>{} };vm.createContext(ctx);vm.runInContext(source,ctx);
let count=0;function test(name,fn){fn();count++;console.log('OK '+name);}
const strict={obligatoria:false,longitud:true,min:8,max:20,complejidad:true,upper:true,lower:true,number:true,symbol:true,distinta:true};
const validate=(a,n,c,p=strict)=>ctx.validarClaveCuenta(a,n,c,p);
test('Acepta contraseña conforme a política',()=>assert.equal(validate('Anterior1!','Nueva123!','Nueva123!'),null));
for(const [label,n] of [['mayúscula','nueva123!'],['minúscula','NUEVA123!'],['número','Nuevaabc!'],['símbolo','Nueva1234'],['mínimo','Ab1!'],['máximo','Ab1!'.repeat(6)]])test('Rechaza falta de '+label,()=>assert(validate('Anterior1!',n,n)));
test('No acepta actual vacía',()=>assert.equal(validate(' ','Nueva123!','Nueva123!').focus,'ClaveActual'));
test('Requiere confirmación exacta, incluyendo espacios',()=>assert.equal(validate('Anterior1!','Nueva123!',' Nueva123!').focus,'ConfirmacionClave'));
test('Espacios y mayúsculas se conservan',()=>assert.equal(validate(' Anterior1! ',' Nueva123! ',' Nueva123! '),null));
test('Impide reutilizar la actual cuando corresponde',()=>assert(validate('Nueva123!','Nueva123!','Nueva123!')));
test('Cambio forzado no exige actual',()=>assert.equal(validate('','Nueva123!','Nueva123!',{...strict,obligatoria:true}),null));
test('Respeta complejidad desactivada',()=>assert.equal(validate('anterior','nuevaclave','nuevaclave',{...strict,complejidad:false}),null));
test('Respeta validación de longitud desactivada',()=>assert.equal(validate('anterior','a','a',{...strict,longitud:false,complejidad:false}),null));
test('Máximo técnico 128 aunque no valide longitud',()=>assert(validate('anterior','a'.repeat(129),'a'.repeat(129),{...strict,longitud:false,complejidad:false})));
function setup(){
 const nodes=new Map(),requests=[],redirects=[],timers=[];let ready;
 function node(s){if(!nodes.has(s))nodes.set(s,{attrs:{},props:{},value:'',text:'',html:'',classes:new Set(),handlers:{}});return nodes.get(s);}
 function wrap(selectors){const list=selectors.map(node);return {length:list.length,
 attr(k,v){if(v===undefined)return list[0]?.attrs[k];list.forEach(n=>n.attrs[k]=v);return this;},prop(k,v){if(v===undefined)return list[0]?.props[k];list.forEach(n=>n.props[k]=v);return this;},
 val(v){if(v===undefined)return list[0]?.value;list.forEach(n=>n.value=v);return this;},text(v){if(v===undefined)return list[0]?.text;list.forEach(n=>n.text=v);return this;},html(v){if(v===undefined)return list[0]?.html;list.forEach(n=>n.html=v);return this;},
 addClass(c){list.forEach(n=>c.split(' ').forEach(x=>n.classes.add(x)));return this;},removeClass(c){list.forEach(n=>c.split(' ').forEach(x=>n.classes.delete(x)));return this;},toggleClass(c,b){list.forEach(n=>b?n.classes.add(c):n.classes.delete(c));return this;},
 on(e,cb){list.forEach(n=>n.handlers[e]=cb);return this;},trigger(e){list.forEach(n=>{if(e==='focus')n.focused=true;else n.handlers[e]?.({preventDefault(){}});});return this;},
 find(s){if(s.startsWith('input'))return wrap(['#ClaveActual','#ClaveNueva','#ConfirmacionClave',...(s.includes('button')?['#cuentaGuardar']:[])]);return wrap([s]);},
 serialize(){return JSON.stringify({actual:node('#ClaveActual').value,nueva:node('#ClaveNueva').value,confirmacion:node('#ConfirmacionClave').value,antiforgery:'test'});},eq(i){return wrap(selectors.slice(i,i+1));},index(){return 0;}};}
 function $(s){if(typeof s==='function'){ready=s;return;}return wrap([String(s)]);}
 $.ajax=options=>{const req={options,done(fn){this.success=fn;return this;},fail(fn){this.error=fn;return this;},always(fn){this.complete=fn;return this;}};requests.push(req);return req;};
 const attrs=node('#formCuentaClave').attrs;Object.assign(attrs,{'data-longitud-minima':'8','data-longitud-maxima':'20','data-validar-longitud':'true','data-validar-complejidad':'true','data-mayuscula':'true','data-minuscula':'true','data-numero':'true','data-simbolo':'true','data-distinta':'true','data-obligatoria':'false','data-login':'/login',action:'/cambiar'});
 const context={$ ,document:'document',window:{location:{assign:u=>redirects.push(u)}},setTimeout:fn=>timers.push(fn)};vm.createContext(context);vm.runInContext(source,context);ready();
 node('#ClaveActual').value=' Anterior1! ';node('#ClaveNueva').value=node('#ConfirmacionClave').value=' Nueva123! ';
 return {node,requests,redirects,timers,submit:()=>$('#formCuentaClave').trigger('submit'),success:r=>{requests.at(-1).success(r);requests.at(-1).complete();},fail:status=>{requests.at(-1).error({status});requests.at(-1).complete();}};
}
test('Dos envíos simultáneos producen una petición y conservan valores',()=>{const t=setup();t.submit();t.submit();assert.equal(t.requests.length,1);assert.equal(JSON.parse(t.requests[0].options.data).nueva,' Nueva123! ');assert.equal(t.requests[0].options.global,false);});
test('Rechazo del servidor permite corregir sin cerrar sesión',()=>{const t=setup();t.submit();t.success({ok:false,warn:true,msg:'Clave incorrecta',focus:'ClaveActual'});assert(t.node('#ClaveActual').focused);assert.equal(t.node('#cuentaGuardar').props.disabled,false);t.submit();assert.equal(t.requests.length,2);assert.equal(t.redirects.length,0);});
test('Éxito limpia credenciales y bloquea nuevos envíos',()=>{const t=setup();t.submit();t.success({ok:true,msg:'Actualizada',redirect:'/login?ok'});assert.equal(t.node('#ClaveNueva').value,'');t.submit();assert.equal(t.requests.length,1);t.timers[0]();assert.equal(t.redirects[0],'/login?ok');});
for(const status of [0,500,503])test('Fallo incierto '+status+' no permite duplicar ni deja claves',()=>{const t=setup();t.submit();t.fail(status);t.submit();assert.equal(t.requests.length,1);assert.equal(t.node('#ClaveActual').value,'');assert(!t.node('#cuentaReingresar').classes.has('d-none'));});
for(const status of [401,403,440])test('Sesión inválida '+status+' pide reingreso',()=>{const t=setup();t.submit();t.fail(status);assert.equal(t.redirects[0],'/login');});
test('Respuesta malformada no se interpreta como éxito ni permite repetir',()=>{const t=setup();t.submit();t.success(null);t.submit();assert.equal(t.requests.length,1);assert.equal(t.node('#ClaveNueva').value,'');});
console.log(count+' pruebas de cuenta aprobadas, sin red ni contraseñas reales.');
