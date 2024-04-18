namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos;

    public class UsuariosValidator : AbstractValidator<UsuariosDto>
    {
        public UsuariosValidator()
        {
            RuleFor(p => p.usu_id).NotNull().Length(4, 10);
            RuleFor(p => p.usu_password).NotNull().Length(4, 300);
            RuleFor(p => p.usu_apellidoynombre).NotNull().Length(4, 50);
            RuleFor(p => p.usu_bloqueado).NotNull();
            RuleFor(p => p.usu_intentos).NotNull();
            RuleFor(p => p.usu_estalogeado).NotNull();
            RuleFor(p => p.tdo_codigo).Length(4, 2);
            RuleFor(p => p.usu_ducumento).Length(4, 11);
            RuleFor(p => p.usu_email).Length(4, 80).EmailAddress();
            RuleFor(p => p.usu_celu).Length(4, 80);
            RuleFor(p => p.usu_pin).Length(4, 30);
            RuleFor(p => p.cta_id).Length(4, 8);

        }
    }
}
