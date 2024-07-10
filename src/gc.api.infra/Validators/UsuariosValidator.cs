namespace gc.api.infra.Validators
{
    using FluentValidation;
    using gc.infraestructura.Dtos;

    public class UsuariosValidator : AbstractValidator<UsuarioDto>
    {
        public UsuariosValidator()
        {
            RuleFor(p => p.Usu_Id).NotNull().Length(4, 10);
            RuleFor(p => p.Usu_Password).NotNull().Length(4, 300);
            RuleFor(p => p.Usu_Apellidoynombre).NotNull().Length(4, 50);
            RuleFor(p => p.Usu_Bloqueado).NotNull();
            RuleFor(p => p.Usu_Intentos).NotNull();
            RuleFor(p => p.Usu_Estalogeado).NotNull();
            RuleFor(p => p.Tdo_Codigo).Length(4, 2);
            RuleFor(p => p.Usu_Ducumento).Length(4, 11);
            RuleFor(p => p.Usu_Email).Length(4, 80).EmailAddress();
            RuleFor(p => p.Usu_Celu).Length(4, 80);
            RuleFor(p => p.Usu_Pin).Length(4, 30);
            RuleFor(p => p.Cta_Id).Length(4, 8);

        }
    }
}
