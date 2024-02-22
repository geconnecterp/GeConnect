namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.DTOs;

    public class AuditoriaUsuarioValidator : AbstractValidator<AuditoriaUsuarioDto>
    {
        public AuditoriaUsuarioValidator()
        { 
            RuleFor(p => p.Id).NotNull();
            RuleFor(p => p.UsuarioId).NotNull();
            RuleFor(p => p.FechaAuditoria).NotNull();
            RuleFor(p => p.IP).NotNull().Length(4, 15);
            RuleFor(p => p.MetodoAccedido).NotNull().Length(4, 100);

        }
    }
}
