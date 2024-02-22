namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.DTOs;

    public class UsuarioValidator : AbstractValidator<UsuarioDto>
    {
        public UsuarioValidator()
        { 
            RuleFor(p => p.Id).NotNull();
            RuleFor(p => p.Contrasena).NotNull().Length(4, 200);
            RuleFor(p => p.Correo).NotNull().Length(4, 50);
            RuleFor(p => p.Bloqueado).NotNull();
            RuleFor(p => p.Intentos).NotNull();
            RuleFor(p => p.FechaAlta).NotNull();
            RuleFor(p => p.UserName).NotNull().Length(4, 50);
            RuleFor(p => p.EstaLogueado).NotNull();

        }
    }
}
