namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.DTOs;

    public class AccesoValidator : AbstractValidator<AccesoDto>
    {
        public AccesoValidator()
        { 
            RuleFor(p => p.Id).NotNull();
            RuleFor(p => p.UsuarioId).NotNull();
            RuleFor(p => p.Fecha).NotNull();
            RuleFor(p => p.IP).NotNull().Length(4, 15);
            RuleFor(p => p.TipoAcceso).NotNull();

        }
    }
}
