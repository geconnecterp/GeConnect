namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos;

    public class TipoDocumentoValidator : AbstractValidator<TipoDocumentoDto>
    {
        public TipoDocumentoValidator()
        {
            RuleFor(p => p.Tdoc_Id).NotNull().Length(2, 2);
            RuleFor(p => p.Tdoc_Desc).NotNull().Length(4, 15);

        }
    }
}
