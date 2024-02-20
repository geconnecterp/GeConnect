namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos;

    public class TestValidator : AbstractValidator<TestDto>
    {
        public TestValidator()
        {
            RuleFor(p => p.Id).NotNull();
            RuleFor(p => p.DatoStr).NotNull().Length(4, 50);
            RuleFor(p => p.DatoBool).NotNull();

        }
    }
}
