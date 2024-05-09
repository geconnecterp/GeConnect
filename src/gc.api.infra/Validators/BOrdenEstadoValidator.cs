namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos.Billeteras;

    public class BOrdenEstadoValidator : AbstractValidator<BOrdenEstadoDto>
    {
        public BOrdenEstadoValidator()
        {
            RuleFor(p => p.Boe_id).NotNull().Length(4, 2);
            RuleFor(p => p.Boe_desc).NotNull().Length(4, 20);

        }
    }
}
