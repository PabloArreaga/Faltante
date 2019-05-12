using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTreeDLL
{
	public class FabricaDeTexto<TKey, T> where TKey : IComparable<TKey> where T : IComparable<T>
	{
		private static FabricaDeTexto<TKey, T> formatoTexto;

		public static FabricaDeTexto<TKey, T> obtenerFormatoTexto()
		{
			return formatoTexto ?? (formatoTexto = new FabricaDeTexto<TKey, T>());
		}
		public string FormatoNumerico(int num)
		{
			return num == -2147483648 ? "-2147483648" : num.ToString("00000000000");
		}

		public string HijoLista(List<int> hijo, int grado)
		{
			int con = 0;
			string formato = "";
			for (int i = 0; i < grado; i++)
			{
				try
				{
					if (hijo.ElementAt(i) > -2147483648)
					{
						con++;
						formato += FormatoNumerico(hijo.ElementAt(i)) + "|";
					}
				}
				catch
				{

				}
			}
			int diferencia = grado - con;
			for (int j = 0; j < diferencia; j++)
			{
				formato += "-2147483648|";
			}
			return formato;
		}

		public string ListadoLlaves(List<TKey> llave, int grado)
		{
			string formato = "";
			for (int i = 0; i < (grado - 1); i++)
			{
				try
				{
					formato += llave.ElementAt(i).ToString() + "|";
				}
				catch (Exception)
				{
					formato += "####################################|";
				}
			}
			return formato;
		}

		public string ListadoAtributos(List<T> objeto, int grado)
		{
			string formato = "";
			for (int i = 0; i < (grado - 1); i++)
			{
				try
				{
					formato += objeto.ElementAt(i).ToString() + "|";
				}
				catch
				{
					formato += "####################################|";
				}
			}
			return formato;
		}

		public int ConteoAtributo(T objeto)
		{
			return objeto.GetType().GetProperties().Length;
		}
	}
}
