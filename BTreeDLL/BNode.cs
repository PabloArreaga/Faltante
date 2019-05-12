using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTreeDLL
{
	public class BNode<TKey, T>
	{
		public List<T> Datos { get; set; }
		public List<int> ApuntaHijo { get; set; }
		public List<TKey> Llave { get; set; }
		public int Padre { get; set; }
		public int Posi { get; set; }
		public int Grado { get; set; }

		public BNode()
		{
			Datos = new List<T>();
			Llave = new List<TKey>();
			ApuntaHijo = new List<int>();
		}

		public BNode(List<T> datos, List<int> apuntaHijo, List<TKey> llave, int padre, int posicion, int grado)
		{
			this.Datos = datos;
			this.ApuntaHijo = apuntaHijo;
			this.Llave = llave;
			this.Padre = padre;
			this.Posi = posicion;
			this.Grado = grado;
		}

		public bool Hoja()
		{
			int con = 0;
			for (int i = 0; i < Grado; i++)
			{
				if (ApuntaHijo.ElementAt(i) == -2147483648)
				{
					con++;
				}
			}
			return con == Grado ? true : false;
		}

		public bool Under()
		{
			int con = 0;
			for (int i = 0; i < Grado; i++)
			{
				if (Llave.ElementAt(i) == null)
				{
					con++;
				}
			}
			return con == 0;
		}

		public int HijoCon()
		{
			int con = 0;
			for (int i = 0; i < ApuntaHijo.Count; i++)
			{
				if (ApuntaHijo[i] > -2147483648)
				{
					con++;
				}
			}
			return con;
		}

		public int DatoCon()
		{
			int con = 0;
			for (int i = 0; i < Datos.Count; i++)
			{
				if (Datos[i] != null)
				{
					con++;
				}
			}
			return con;
		}

		public int LlaveCon()
		{
			int con = 0;
			for (int i = 0; i < Llave.Count; i++)
			{
				if (Llave[i] != null)
				{
					con++;
				}
			}
			return con;
		}

	}
}
