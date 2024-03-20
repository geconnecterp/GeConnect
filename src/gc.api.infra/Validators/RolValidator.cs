namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos;

    public class RolValidator : AbstractValidator<RolDto>
    {
        public RolValidator()
        { 
            RuleFor(p => p.Id).NotNull();
            RuleFor(p => p.Nombre).NotNull().Length(4, 20);

        }
    }
}
