using gc.api.core.Entidades;

namespace gc.api.core.Entidades
{
    public class UserLogin : EntidadBase
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Admid { get; set; }
        public string? Sistema { get; set; }
    }
}
