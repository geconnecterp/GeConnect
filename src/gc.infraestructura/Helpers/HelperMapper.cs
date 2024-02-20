using System;

namespace gc.infraestructura.Core.Helpers
{
    public class HelperMapper<T,Y1> where T:class where Y1:class
    {
       public T Map(Y1 ent)
        {
            var entidad = MapAutomatico<T>(ent);
            return entidad;
        }

        private T1 MapAutomatico<T1>(Y1 ent) where T1:class 
        {
            var t = typeof(T1);
            T1 result = Activator.CreateInstance<T1>();
            var properties = t.GetProperties();

            foreach (var p in properties)
            {
                if (p.PropertyType == typeof(int))
                {
                    p.SetValue(result, this.MapInt(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(int?))
                {
                    p.SetValue(result, this.MapIntNulleable(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(short))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(short?))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(string))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(bool))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(bool?))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(char))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                //if (p.PropertyType == typeof(Nullable<char>))
                //{
                //    p.SetValue(result, GetValue(ent, p.Name));
                //    continue;
                //}

                if (p.PropertyType == typeof(DateTime))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(DateTime?))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(long))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(long?))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(decimal))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(decimal?))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(Guid))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }

                if (p.PropertyType == typeof(Guid?))
                {
                    p.SetValue(result, GetValue(ent, p.Name));
                    continue;
                }
            }

            return result;
        }
       
        private bool ExistePropiedad(Y1 ent,string name)
        {
            return ent.GetType().GetProperty(name) != null;
        }

        private object MapInt(Y1 ent, string name)
        {
            if (ExistePropiedad(ent, name))
            {
                return ent.GetType().GetProperty(name).GetValue(ent, null);
            }
            return 0;
        }

        private object MapIntNulleable(Y1 ent, string name)
        {
            if (ExistePropiedad(ent, name))
            {
                return ent.GetType().GetProperty(name).GetValue(ent, null);
            }
            return 0;
        }

        private object GetValue(Y1 ent, string name)
        {
            if (ExistePropiedad(ent, name))
            {
                return ent.GetType().GetProperty(name).GetValue(ent, null);
            }
            return default;
        }

    }
}
