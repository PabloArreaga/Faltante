using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTreeDLL
{
	public abstract class AbstractTree<T> where T : IComparable<T>
	{
		public abstract void Agregar(T nuevoDato);
		public abstract void Eliminar(T eliminaDato);
		public abstract void Actualizar(T actualizaDato);
		public abstract bool Vacio();
		public abstract List<Object> ConvertirAObj();
	}
}