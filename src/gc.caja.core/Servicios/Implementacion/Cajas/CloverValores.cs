using gc.infraestructura.Dtos.Cajas;
namespace gc.caja.core.Servicios.Implementacion.Cajas;

public static class CloverValores
{
    public static bool Iguales(Json_Valor a, Json_Valor b) => a.ins_id == b.ins_id && a.rb_dato1_valor == b.rb_dato1_valor && a.rb_dato2_valor == b.rb_dato2_valor && a.rb_dato3_valor == b.rb_dato3_valor && a.rb_opcion_cuota == b.rb_opcion_cuota && a.rb_fecha_valor == b.rb_fecha_valor && a.rb_importe == b.rb_importe && a.rb_rec == b.rb_rec && a.id_externo == b.id_externo && a.rb_cupon_manual == b.rb_cupon_manual && a.rb_ch_dif == b.rb_ch_dif && a.rb_estado == b.rb_estado && a.rb_aux == b.rb_aux;
}
