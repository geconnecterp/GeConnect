namespace gc.api.Core.Servicios
{
    using gc.api.core.Dtos;
    using gc.api.core.Entidades;
    using gc.api.core.Interfaces.Datos;
    using gc.api.core.Interfaces.Servicios;
    using gc.api.core.Servicios;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.EntidadesComunes.Options;
    using gc.infraestructura.Core.Enumeraciones;
    using gc.infraestructura.Core.Exceptions;
    using gc.infraestructura.Core.Validaciones;
    using gc.infraestructura.Dtos;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class EmpleadoServicio : Servicio<Persona>, IEmpleadoServicio
    {
        private readonly ISecurityServicio _securityServicio;
        private readonly ILogger<EmpleadoServicio> _logger;
        string? msj;

        public EmpleadoServicio(ILogger<EmpleadoServicio> logger, IUnitOfWork uow, IOptions<PaginationOptions> options, IOptions<ConfigNegocioOption> options1, ISecurityServicio securityServicio) : base(uow, options1, options)
        {
            _securityServicio = securityServicio;
            _logger = logger;
        }

        public EmpleadoDto Find(Guid id)
        {
            var empleados = GetAllIq();
            var _uRep = _uow.GetRepository<Usuario>();
            var usuarios = _uRep.GetAll();
            var _auRep = _uow.GetRepository<Autorizado>();
            var autos = _auRep.GetAll();
            var _rRep = _uow.GetRepository<Role>();
            var roles = _rRep.GetAll();

            var emp = from empl in empleados
                      join u in usuarios on empl.UsuarioId equals u.Id
                      join a in autos on u.Id equals a.UsuarioId
                      join r in roles on a.RoleId equals r.Id
                      where empl.Id == id || u.Id==id
                      select new EmpleadoDto
                      {
                          Id = empl.Id,
                          CodigoInterno = empl.CodigoInterno,
                          Nombre = empl.Nombre,
                          Apellido = empl.Apellido,
                          Direccion = empl.Direccion,
                          Telefono = empl.Telefono,
                          Celular = empl.Celular,
                          LocalidadId = empl.LocalidadId,
                          TipoDocumentoId = empl.TipoDocumentoId,
                          CUIT = empl.CUIT,
                          Email = empl.Email,
                          UsuarioId = empl.UsuarioId,
                          Usuario = u.UserName,
                          Rol = u.Autorizados.First().Role.Nombre,
                          Activo = empl.Activo
                      };

            //var emp = empleados.Join(usuarios,
            //   e => e.UsuarioId,
            //   u => u.Id,
            //   (empl, user) => new EmpleadoDto
            //   {
            //       Id = empl.Id,
            //       CodigoInterno = empl.CodigoInterno,
            //       Nombre = empl.Nombre,
            //       Apellido = empl.Apellido,
            //       Direccion = empl.Direccion,
            //       Telefono = empl.Telefono,
            //       Celular = empl.Celular,
            //       LocalidadId = empl.LocalidadId,
            //       TipoDocumentoId = empl.TipoDocumentoId,
            //       CUIT = empl.CUIT,
            //       Email = empl.Email,
            //       UsuarioId = empl.UsuarioId,
            //       Usuario = user.User,
            //       Rol = user.Autorizados.First().Role.Nombre,
            //       Activo = empl.Activo
            //   });


            return emp.SingleOrDefault();
        }

        public override IQueryable<Persona> GetAllIq()
        {
            //return base.GetAllIq().Where(c => c.EsEmpleado).Include(t => t.TipoDocumento).Include(l => l.Localidad).ThenInclude(p => p.Provincia);
            return base.GetAllIq().Where(c => c.EsEmpleado).Include(t => t.TipoDocumento);
        }

        public new  PagedList<EmpleadoDto> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;


            var empleados = GetAllIq();
            if (string.IsNullOrWhiteSpace(filters.Sort))
            {
                filters.Sort = "Apellido";
            }

            if (string.IsNullOrWhiteSpace(filters.SortDir))
            {
                filters.SortDir = "ASC";
            }
            empleados = empleados.OrderBy($"{filters.Sort} {filters.SortDir}");

            //si indica el Id es que se busca al Usuario que lo cargo.
            if (filters.IdG != default)
            {
                empleados = empleados.Where(c => c.UsuarioId.HasValue && c.UsuarioId.Value == filters.IdG);
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                if (filters.Search.ToLongOrNull() == null)
                {
                    //no es numerico
                    empleados = empleados.Where(c => c.Nombre.Contains(filters.Search));
                }
                else
                {
                    empleados = empleados.Where(c => c.CUIT.Contains(filters.Search));
                }
            }

            var _uRep = _uow.GetRepository<Usuario>();
            var usuarios = _uRep.GetAll().Include(a => a.Autorizados).ThenInclude(r => r.Role);

            var emp = empleados.Join(usuarios,
                e => e.UsuarioId,
                u => u.Id,
                (empl, user) => new EmpleadoDto
                {
                    Id = empl.Id,
                    CodigoInterno = empl.CodigoInterno,
                    Nombre = empl.Nombre,
                    Apellido = empl.Apellido,
                    Direccion = empl.Direccion,
                    Telefono = empl.Telefono,
                    Celular = empl.Celular,
                    LocalidadId = empl.LocalidadId,
                    TipoDocumentoId = empl.TipoDocumentoId,
                    CUIT = empl.CUIT,
                    Email = empl.Email,
                    UsuarioId = empl.UsuarioId,
                    Usuario = user.UserName,
                    Rol = user.Autorizados.First().Role.Nombre,
                    Activo = empl.Activo
                });

            var paginaClientes = PagedList<EmpleadoDto>.Create(emp, filters.PageNumber, filters.PageSize);

            return paginaClientes;
        }

        

        public async Task<bool> Add(Persona empreado, string usuario, string rol, string pass)
        {
            var json = JsonSerializer.Serialize(empreado);
            _logger.LogInformation($"Inicia Alta de empleado y usuario. Empleado: {json} Usuario: {usuario} Rol: {rol}");
            Guid idUsuario = await RegistrarUsuario(usuario, empreado.Email, rol, pass);
            if (idUsuario == default)
            {
                msj = "No se detectó la Registración del Usuario. Verifique.";
                _logger.LogError(msj);
                throw new NegocioException(msj);
            }
            //se agrego el usuario para autenticar. se agrega el usuario "PERSONA".
            empreado.UsuarioId = idUsuario;
            return await AddAsync(empreado);


        }

        private async Task<Guid> RegistrarUsuario(string usuario, string email, string rol, string pass)
        {
            var existe =await _securityServicio.GetAllIq().SingleOrDefaultAsync(e => e.UserName.Equals(usuario));
            if (existe!=null)
            {
                _logger.LogInformation($"Usuario Existente: {JsonSerializer.Serialize(existe)}");
                throw new NegocioException($"Existe el Usuario: {usuario}. Verifique.");
            }
            var res = await _securityServicio.RegistrerUser(new RegistroUserDto
            {
                User = usuario,
                Correo = email,
                Role = rol,
                Password = pass
            });
            if (!res)
            {
                msj = $"No se pudo generar el Usuario de Acceso al Sistema";
                _logger.LogError(msj);
                throw new NegocioException(msj);
            }
            //buscamos el Usuario para conocer su Id
            var usu = _securityServicio.GetAllIq().Where(u => u.UserName.Equals(usuario));
            if (usu.Count() == 0)
            {
                msj = $"No se pudo encontrar al Uusario {usuario}. Verifique.";
                _logger.LogError(msj);
                throw new NegocioException(msj);
            }
            return usu.Single().Id;
        }

        public override void Add(Persona empleado)
        {

            throw new NotImplementedException("Debe utilizar el método asincronico");

        }

        private void NuevoEmpleado(Persona empleado)
        {

            Validar(empleado);


            empleado.Activo = true;
        }

        public override async Task<bool> AddAsync(Persona empleado)
        {
            NuevoEmpleado(empleado);

            ////esta todo bien... le asigno el numero de control interno - esto sera regenerado con el modelo nuevo de GeConnect
            //var _confRep = _uow.GetRepository<ConfigTrade>();
            //var conf = await _confRep.FindAsync(_configTradeOption.Identificador);
            //if (conf == null)
            //{
            //    msj = $"No se ha logrado encontrar la configuración del sistema. Verifique. Lea logs.";
            //    _logger.LogError(msj);
            //    throw new NegocioException(msj);
            //}
            //empleado.CodigoInterno = conf.ContadorPersona++;

            return await base.AddAsync(empleado);
        }

        public override async Task<bool> Update(Persona empleado)
        {
            if (empleado.Id == default)
            {
                var json = JsonSerializer.Serialize(empleado);
                msj = $"Al Empleado no se lo puede identificar. Verifique sus datos. Empleado {json}";
                _logger.LogError(msj);
                throw new NegocioException("Al Empleado no se lo puede identificar. Verifique sus datos.");
            }
            Validar(empleado);

            //dato que no viaja pero debe estar activo
            empleado.EsCliente = false;
            empleado.EsFisica = true;
            empleado.EsEmpleado = true;
            empleado.RealizarPercepcion = false;

            return await base.Update(empleado);
        }

        public override async Task<bool> Delete(object id)
        {
            if (id == default)
            {
                msj = $"Al Empleado no se lo puede identificar. Verifique sus datos.";
                _logger.LogError(msj);
                throw new NegocioException(msj);
            }

            if (SePuedeEliminarEmpleado(id))
            {
                var _usuRep = _uow.GetRepository<Usuario>();
                var _autoRep = _uow.GetRepository<Autorizado>();

                var empleado = Find(id);
                if(!empleado.UsuarioId.HasValue || (empleado.UsuarioId.HasValue && empleado.UsuarioId.Value == default))
                {
                    msj = $"Al Empleado no se lo puede identificar su propio usuario de acceso. Verifique sus datos.";
                    _logger.LogError(msj);
                    throw new NegocioException(msj);
                }

                var usuario = await _usuRep.GetAll().Include(u => u.Autorizados).SingleOrDefaultAsync(x=>x.Id== empleado.UsuarioId.Value); //.FindAsync(empleado.UsuarioId.Value);
                if (usuario == null)
                {
                    msj = $"No se lo puede identificar el usuario de acceso. Verifique sus datos.";
                    _logger.LogError(msj);
                    throw new NegocioException(msj);
                }
                foreach (var auto in usuario.Autorizados)
                {
                    _autoRep.Remove(auto);
                }
                _usuRep.Remove(usuario);

                return await base.Delete(id);
            }
            else
            {
                var empleado = await FindAsync(id);
                empleado.Activo = false;
                var res = await _uow.SaveChangesAsync();
                return res > 0;
            }
        }

        private bool SePuedeEliminarEmpleado(object id)
        {
            //debo verificar las relaciones en el sistema. Con tener alguna Venta al empleado es condisión suficiente
            //para solo marcarlo como NO ACTIVO.
            var empleado = Find(id);
            if (empleado == null)
            {
                msj = $"Al Cliente no se lo puede identificar. Verifique sus datos.";
                _logger.LogError(msj);
                throw new NegocioException(msj);
            }

            //if (empleado.Facturas.Any())//existe algun registro en productos  - es importante que exista este if. Esta comentado para que compile inicialmente
            //{
            //    return false;
            //}
            return true;
        }

        private void Validar(Persona empleado)
        {
            if (empleado.TipoDocumentoId == 1)//CUIT 
            {
                var situacionDocumento = ValidacionCuil.ValidarDocumento(empleado.CUIT);
                switch (situacionDocumento)
                {
                    case ValidezDocumento.DocumentoInvalido:
                        throw new NegocioException("CUIT - El Documento es inválido");
                    case ValidezDocumento.PoseeElementosNoNumericos:
                        throw new NegocioException("CUIT - El Documento posee elementos no numéricos");
                    case ValidezDocumento.DocumentoIgualACeroO99999999:
                        throw new NegocioException("CUIT - El Documento es cero o 99999999");
                    default:
                        var situacionCuil = ValidacionCuil.ValidarCuilCuit(empleado.CUIT);
                        switch (situacionCuil)
                        {
                            case ValidezCuilCuit.PoseeElementosNoNumericos:
                                throw new NegocioException("CUIT - El CUIT posee elementos no numéricos");
                            case ValidezCuilCuit.LongitudDistintaDe11:
                                throw new NegocioException("CUIT - El CUIT no tiene 11 digitos numéricos");
                            case ValidezCuilCuit.CuilInvalido:
                                throw new NegocioException("CUIT - El CUIT es Inválido");
                        }
                        break;
                }
            }

        }

        public async Task<bool> CambioClave(Guid id, CambioClaveDto cambio)
        {
            var _uRep = _uow.GetRepository<Usuario>();
            var usuarios = await _uRep.GetAll().SingleOrDefaultAsync(u=>u.Id== id);
            if (usuarios == null)
            {
                _logger.LogError("No se encontró el Usuario. Verifique");
                return false;
            }

            usuarios.Contrasena = cambio.PassNew;
            var res = await _uow.SaveChangesAsync();
            return res > 0;
        }

        /// <summary>
        /// Se devolvera si esta o no logueado y la IP si estuviera logueado.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<(bool, string)> VerificaSiEstaLogueado(string? userName,string? ip)
        {
            var _uRep = _uow.GetRepository<Usuario>();

            var usuario = await _uRep.GetAll().Include(a => a.Accesos).SingleOrDefaultAsync(u => u.UserName.Equals(userName));

            DateTime ahora = DateTime.Now;
            var tiempoAdmitido = ahora.AddHours(-1);
            var lista = new List<char>() { 'L', 'O' };
            //buscamos los accesos "L"ogueados del usuario en la ultima hora.
            var res = usuario.Accesos.Where(a => a.Fecha >= tiempoAdmitido && a.UsuarioId == usuario.Id && lista.Contains(a.TipoAcceso));

            if (res.Any())
            {
                var ultimo = res.OrderByDescending(s => s.Fecha).First();

                if (ultimo.TipoAcceso.Equals('O')) //logout
                {
                    return (false, string.Empty);
                }
                else
                {
                    RegistrarAcceso(usuario.Id, ip, 'R');
                    return (true, ultimo.IP);
                }
            }
            else
            {
                return (false, string.Empty);
            }
        }

        //se registra el acceso a modo de auditoria
        public void RegistrarAcceso(Guid id,string? ip,char tipoAcceso)
        {
            var rep = _uow.GetRepository<Acceso>();
            var acceso = new Acceso { Fecha = DateTime.Now, IP = ip, TipoAcceso = tipoAcceso, UsuarioId = id };
            rep.Add(acceso);
            _uow.SaveChanges(true);
        }

        public async Task Logoff(string? userName, string? ip)
        {
            var _uRep = _uow.GetRepository<Usuario>();

            var usuario = await _uRep.GetAll().SingleOrDefaultAsync(u => u.UserName.Equals(userName));

            RegistrarAcceso(usuario.Id, ip, 'O');//logoff            
        }
    }
}
