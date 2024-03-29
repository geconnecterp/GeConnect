namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.DTOs;

    public class AutorizadoValidator : AbstractValidator<AutorizadoDto>
    {
        public AutorizadoValidator()
        { 
            RuleFor(p => p.UsuarioId).NotNull();
            RuleFor(p => p.RoleId).NotNull();

        }
    }
}
