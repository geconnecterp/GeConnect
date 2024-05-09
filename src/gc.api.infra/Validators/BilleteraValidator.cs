namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos.Billeteras;

    public class BilleteraValidator : AbstractValidator<BilleteraDto>
    {
        public BilleteraValidator()
        {
            RuleFor(p => p.Bill_id).NotNull().Length(4, 2);
            RuleFor(p => p.Bill_desc).NotNull().Length(4, 20);

        }
    }
}
