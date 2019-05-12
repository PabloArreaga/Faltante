using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTreeDLL
{
	class FileHandler<TKey, T> where TKey : IComparable<TKey> where T : IComparable<T>
	{
		FileStream fileStream;
		const long EncabezadoTamaño = 65;
		long PosicionLinea = 0;

		public FileHandler(string NombreArchivo, int Grado)
		{
			fileStream = new FileStream(NombreArchivo + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			fileStream.Close();
		}

		public TKey ConocerLlave(T objeto)
		{
			if (typeof(T) == typeof(string))
			{
				return (TKey)(object)objeto.ToString();
			}
			return default(TKey);
		}

		public void ConstruccionEncabezado(string NombreArchivo, int Raiz, int Posi, int Tamaño, int Grado, int Altura)
		{
			fileStream = new FileStream(NombreArchivo + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);

			StreamWriter EncabezadoEscribir = new StreamWriter(fileStream);
			EncabezadoEscribir.WriteLine(FabricaDeTexto<TKey, T>.obtenerFormatoTexto().FormatoNumerico(Raiz));
			EncabezadoEscribir.WriteLine(FabricaDeTexto<TKey, T>.obtenerFormatoTexto().FormatoNumerico(Posi));
			EncabezadoEscribir.WriteLine(FabricaDeTexto<TKey, T>.obtenerFormatoTexto().FormatoNumerico(Tamaño));
			EncabezadoEscribir.WriteLine(FabricaDeTexto<TKey, T>.obtenerFormatoTexto().FormatoNumerico(Grado));
			EncabezadoEscribir.WriteLine(FabricaDeTexto<TKey, T>.obtenerFormatoTexto().FormatoNumerico(Altura));
			EncabezadoEscribir.Close();
		}

		public void InsertarNodo(BNode<TKey, T> nodo, string filename, bool actualizar)
		{
			fileStream = new FileStream(filename + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			// Posición para escribir
			PosicionLinea = EncabezadoTamaño + (nodo.Posi - 1) * (34 + (11 * nodo.Grado + (nodo.Grado - 1)) + (36 * (nodo.Grado - 1) + (nodo.Grado - 2)) + (36 * (nodo.Grado - 1) + (nodo.Grado - 1)));
			// Se proporciona la posición
			fileStream.Seek(PosicionLinea, SeekOrigin.Begin);
			// Escritura
			StreamWriter NodoEscribir = new StreamWriter(fileStream);
			if (actualizar == true)
			{
				NodoEscribir.Write(FabricaDeTexto<TKey, T>.obtenerFormatoTexto().FormatoNumerico(nodo.Posi) + "|" +
					FabricaDeTexto<TKey, T>.obtenerFormatoTexto().FormatoNumerico(nodo.Padre) + "|||" +
					FabricaDeTexto<TKey, T>.obtenerFormatoTexto().HijoLista(nodo.ApuntaHijo, nodo.Grado) + "||" +
					FabricaDeTexto<TKey, T>.obtenerFormatoTexto().ListadoLlaves(nodo.Llave, nodo.Grado) + "||" +
					FabricaDeTexto<TKey, T>.obtenerFormatoTexto().ListadoAtributos(nodo.Datos, nodo.Grado));
			}
			else
			{
				NodoEscribir.WriteLine(FabricaDeTexto<TKey, T>.obtenerFormatoTexto().FormatoNumerico(nodo.Posi) + "|" +
					FabricaDeTexto<TKey, T>.obtenerFormatoTexto().FormatoNumerico(nodo.Padre) + "|||" +
					FabricaDeTexto<TKey, T>.obtenerFormatoTexto().HijoLista(nodo.ApuntaHijo, nodo.Grado) + "||" +
					FabricaDeTexto<TKey, T>.obtenerFormatoTexto().ListadoLlaves(nodo.Llave, nodo.Grado) + "||" +
					FabricaDeTexto<TKey, T>.obtenerFormatoTexto().ListadoAtributos(nodo.Datos, nodo.Grado));
			}
			NodoEscribir.Close();
		}

		public BNode<TKey, T> ConvertirLinea(string nombreArchivo, int posi, int grado)
		{
			if (posi <= -2147483648)
				return null;
			fileStream = new FileStream(nombreArchivo + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			// Posición para escribir
			PosicionLinea = EncabezadoTamaño + (posi - 1) * (34 + (11 * grado + (grado - 1)) + (36 * (grado - 1) + (grado - 2)) + (36 * (grado - 1) + (grado - 1)));
			// Posición para leer
			fileStream.Seek(PosicionLinea, SeekOrigin.Begin);
			// Lectura
			StreamReader leer = new StreamReader(fileStream);
			// De linea a nodo
			string linea = leer.ReadLine();
			string[] ArrayLinea = linea.Split('|');
			leer.Close();
			// Posición
			int nodoPosi = int.Parse(ArrayLinea[0]);
			int nodoPadre = int.Parse(ArrayLinea[1]);
			int arrayHijos = 4;
			int arrayLlaves = arrayHijos + grado + 2;
			int arrayDatos = arrayLlaves + (grado - 1) + 2;
			// Se incluyen los diferentes atributos
			int apendice = arrayDatos + grado - 1;
			// Llenado de los hijos
			List<int> hijo = new List<int>();
			for (int i = arrayHijos; i < arrayLlaves - 2; i++)
			{
				hijo.Add(int.Parse(ArrayLinea[i]));
			}
			hijo.Add(-2147483648);
			// Llenado de las llaves
			List<TKey> llave = new List<TKey>();
			for (int i = arrayLlaves; i < arrayDatos - 2; i++)
			{
				if (ArrayLinea[i].Contains("#"))
				{
					llave.Add((TKey)(object)null);
				}
				else
				{
					llave.Add((TKey)(object)ArrayLinea[i]);
				}
			}
			// Espacio temporal (↓f)
			llave.Add((TKey)(object)null);
			// Llenado de los datos
			List<T> dato = new List<T>();
			for (int i = arrayDatos; i < apendice; i++)
			{
				if (ArrayLinea[i].Contains("#"))
				{
					dato.Add((T)(object)null);
				}
				else
				{
					dato.Add((T)(object)ArrayLinea[i]);
				}
			}
			// Espacio temporal (↓f)
			dato.Add((T)(object)null);
			// Creacion del Nodo ARBOL B
			BNode<TKey, T> nodoNuevo = new BNode<TKey, T>(dato, hijo, llave, nodoPadre, nodoPosi, grado);
			return nodoNuevo;
		}

		public long PosicionArchivo()
		{
			return 0;
		}

		public bool RaizVacia(string nombreArchivo)
		{
			fileStream = new FileStream(nombreArchivo + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			StreamReader lecturaEncabezado = new StreamReader(fileStream);
			// Verificacion de la raiz
			if (lecturaEncabezado.ReadLine() == "00000000000" || lecturaEncabezado.ReadLine() == null)
			{
				lecturaEncabezado.Close();
				return true;
			}
			lecturaEncabezado.Close();
			return false;
		}

		public int UltimaPosicion(string nombreArchivo)
		{
			fileStream = new FileStream(nombreArchivo + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			// Se da posición
			fileStream.Seek(16, SeekOrigin.Begin);
			StreamReader lecturaEncabezado = new StreamReader(fileStream);
			// se obtiene posición
			int ultima = int.Parse(lecturaEncabezado.ReadLine());
			lecturaEncabezado.Close();
			return ultima;
		}

		public int ObtenerRaiz(string nombreArchivo)
		{
			fileStream = new FileStream(fileStream + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			StreamReader lecturaEncabezado = new StreamReader(fileStream);
			// Se obtiene posición
			int raiz = int.Parse(lecturaEncabezado.ReadLine());
			lecturaEncabezado.Close();
			return raiz;
		}

		public int ObtenerTamaño(string nombreArchivo)
		{
			fileStream = new FileStream(nombreArchivo + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			// Se da posición
			fileStream.Seek(29, SeekOrigin.Begin);
			StreamReader lecturaEncabezado = new StreamReader(fileStream);
			// Se obtiene posición
			int tamaño = int.Parse(lecturaEncabezado.ReadLine());
			lecturaEncabezado.Close();
			return tamaño;
		}

		public int ObtenerAltura(string nombreArchivo)
		{
			fileStream = new FileStream(nombreArchivo + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			// Se da posición
			fileStream.Seek(55, SeekOrigin.Begin);
			StreamReader lecturaEncabezado = new StreamReader(fileStream);
			// Se obtiene posición
			int altura = int.Parse(lecturaEncabezado.ReadLine());
			lecturaEncabezado.Close();
			return altura;
		}


	}
}
