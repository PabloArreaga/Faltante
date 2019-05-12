using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTreeDLL
{
	public class BTree<TKey, T> : AbstractTree<T> where TKey : IComparable<TKey> where T : IComparable<T>
	{
		private const int GradoMax = 99;
		private const int GradoMin = 3;

		private Queue<int> EliminarPadre = new Queue<int>();
		private Queue<int> EliminarIndex = new Queue<int>();

		private BNode<TKey, T> raiz;
		private string nombreArchivo;
		private int grado;
		private int tamaño;
		private FileHandler<TKey, T> archivoArbol;
		private int factor;

		public BTree(string archivoNombre, int grado)
		{
			if (grado <= GradoMax && grado >= GradoMin)
			{
				this.grado = grado;
				this.nombreArchivo = archivoNombre;
				raiz = null;
				archivoArbol = new FileHandler<TKey, T>(archivoNombre, grado);
				tamaño = 0;
			}
		}

		public override void Agregar(T nuevoDato)
		{
			if (archivoArbol.RaizVacia(nombreArchivo))
			{
				List<TKey> lista = new List<TKey>();
				List<T> listaT = new List<T>();
				// Se ingresan datos
				lista.Add(archivoArbol.ConocerLlave(nuevoDato));
				listaT.Add(nuevoDato);
				// Creación del primer encabezado (por default)
				archivoArbol.ConstruccionEncabezado(nombreArchivo, 0, 0, 0, grado, 0);
				// Inserción de Nodo raiz
				BNode<TKey, T> nodoRaiz = new BNode<TKey, T>(listaT, new List<int>(), lista, -2147483648, 1, grado);
				archivoArbol.InsertarNodo(nodoRaiz, nombreArchivo, false);
				// Creación de encabezado por inserción de raiz
				archivoArbol.ConstruccionEncabezado(nombreArchivo, 1, 2, 1, grado, 0);
			}
			else
			{
				// Se agregan datos cuando la raiz no es null
				tamaño++;
				AgregarArbol(archivoArbol.ObtenerRaiz(nombreArchivo), nuevoDato, archivoArbol.ConocerLlave(nuevoDato));
			}
		}
		private void AgregarArbol(int posiNodo, T dato, TKey compara)
		{
			// Segun la posición se obtiene 
			BNode<TKey, T> posiNodoB = archivoArbol.ConvertirLinea(nombreArchivo, posiNodo, grado);
			// Verificamos que sea hoja
			if (posiNodoB.Hoja())
			{
				for (int i = 0; i < grado; i++)
				{
					// Se inserta si la llave es menor al parametro del nodo
					if (compara.CompareTo(posiNodoB.Llave.ElementAt(i)) == -1 || posiNodoB.Llave.ElementAt(i) == null)
					{
						posiNodoB.Llave.Insert(i, compara);
						posiNodoB.Datos.Insert(i, dato);
						break;
					}
				}
				// Comprobacion de underflow al insertar
				UnderF(posiNodoB);
			}
			else
			{
				// Se observa si debe seguir la recursividad
				for (int i = 0; i < grado; i++)
				{
					// Se sigue aplicando recursividad si la llave es menor al parametro del nodo
					if (compara.CompareTo(posiNodoB.Llave.ElementAt(i)) == -1 || posiNodoB.Llave.ElementAt(i) == null)
					{
						AgregarArbol(posiNodoB.ApuntaHijo.ElementAt(i), dato, compara);
						break;
					}
				}
			}

		}

		private void UnderF(BNode<TKey, T> nuevoB)
		{
			// Se inserta inmediatamente por no ser underflow
			if (nuevoB.Under())
			{
				archivoArbol.InsertarNodo(nuevoB, nombreArchivo, true);
				archivoArbol.ConstruccionEncabezado(nombreArchivo, archivoArbol.ObtenerRaiz(nombreArchivo), archivoArbol.UltimaPosicion(nombreArchivo), tamaño, grado, 0);
				return;
			}
			// Se determina quien sube
			if (grado % 2 == 0)
			{
				// Sube el de en medio si es impar
				factor = (grado / 2) - 1;
			}
			else
			{
				// Sube el de en medio +1 si es impar
				factor = (grado / 2);
			}
			// Se obtien la ultima posicion disponible
			int posicion = archivoArbol.UltimaPosicion(nombreArchivo);
			// Si el padre es nulo, es raiz
			if (nuevoB.Padre == -2147483648)
			{
				// Hijo Derecho
				BNode<TKey, T> derecho = new BNode<TKey, T>();
				// Atributos
				derecho.Posi = posicion;
				derecho.Padre = posicion + 1;
				derecho.Grado = grado;
				for (int i = factor + 1; i <= grado; i++)
				{
					derecho.ApuntaHijo.Add(nuevoB.ApuntaHijo.ElementAt(i));
					derecho.Datos.Add(nuevoB.Datos.ElementAt(i));
					derecho.Llave.Add(nuevoB.Llave.ElementAt(i));
				}
				// Actualización de los padres en el nuevo nodo
				for (int i = 0; i < derecho.ApuntaHijo.Count; i++)
				{
					if (derecho.ApuntaHijo.ElementAt(i) != -2147483648)
					{
						BNode<TKey, T> padresNuevos = archivoArbol.ConvertirLinea(nombreArchivo, derecho.ApuntaHijo.ElementAt(i), grado);
						padresNuevos.Padre = derecho.Posi;
						archivoArbol.InsertarNodo(padresNuevos, nombreArchivo, true);

					}
				}
				// Creacion de nueva raiz
				BNode<TKey, T> raiznueva = new BNode<TKey, T>();
				// Atributos
				raiznueva.Posi = posicion + 1;
				raiznueva.Padre = -2147483648;
				raiznueva.Grado = grado;
				raiznueva.ApuntaHijo.Add(nuevoB.Posi);
				raiznueva.ApuntaHijo.Add(posicion);
				raiznueva.Datos.Add(nuevoB.Datos.ElementAt(factor));
				raiznueva.Llave.Add(nuevoB.Llave.ElementAt(factor));
				// Hijo Izquierdo
				nuevoB.Padre = posicion + 1;
				nuevoB.ApuntaHijo.RemoveRange(factor + 1, nuevoB.ApuntaHijo.Count - (factor + 1));
				nuevoB.Datos.RemoveRange(factor, nuevoB.Datos.Count - factor);
				nuevoB.Llave.RemoveRange(factor, nuevoB.Llave.Count - factor);
				// Se realizan cambios en el archivo
				archivoArbol.InsertarNodo(nuevoB, nombreArchivo, true);
				archivoArbol.InsertarNodo(derecho, nombreArchivo, false);
				archivoArbol.InsertarNodo(raiznueva, nombreArchivo, false);
				// Actualización de encabezado
				archivoArbol.ConstruccionEncabezado(nombreArchivo, raiznueva.Posi, posicion + 2, tamaño, grado, 0);
			}
			else
			{
				// Llamar al padre
				BNode<TKey, T> padre = archivoArbol.ConvertirLinea(nombreArchivo, nuevoB.Padre, grado);
				for (int i = 0; i < grado; i++)
				{
					try
					{
						if (padre.Llave.ElementAt(i).CompareTo(nuevoB.Llave.ElementAt(factor)) == 1 || padre.Llave.ElementAt(i) == null)
						{
							padre.Llave.Insert(i, nuevoB.Llave.ElementAt(factor));
							padre.Datos.Insert(i, nuevoB.Datos.ElementAt(factor));
							padre.ApuntaHijo.Insert(i + 1, posicion);
							break;
						}
					}
					catch (NullReferenceException)
					{
						padre.Llave.Insert(i, nuevoB.Llave.ElementAt(factor));
						padre.Datos.Insert(i, nuevoB.Datos.ElementAt(factor));
						padre.ApuntaHijo.Insert(i + 1, posicion);
						break;
					}
				}
				// Hijo derecho
				BNode<TKey, T> derecho = new BNode<TKey, T>();
				// Atributos
				derecho.Posi = posicion;
				derecho.Padre = posicion + 1;
				derecho.Grado = grado;
				for (int i = factor + 1; i <= grado; i++)
				{
					derecho.ApuntaHijo.Add(nuevoB.ApuntaHijo.ElementAt(i));
					derecho.Datos.Add(nuevoB.Datos.ElementAt(i));
					derecho.Llave.Add(nuevoB.Llave.ElementAt(i));
				}
				// Actualización de los padres en el nuevo nodo
				for (int i = 0; i < derecho.ApuntaHijo.Count; i++)
				{
					if (derecho.ApuntaHijo.ElementAt(i) != -2147483648)
					{
						BNode<TKey, T> padresNuevos = archivoArbol.ConvertirLinea(nombreArchivo, derecho.ApuntaHijo.ElementAt(i), grado);
						padresNuevos.Padre = derecho.Posi;
						archivoArbol.InsertarNodo(padresNuevos, nombreArchivo, true);
					}
				}
				// Hijo izquierdo
				nuevoB.ApuntaHijo.RemoveRange(factor + 1, nuevoB.ApuntaHijo.Count - (factor + 1));
				nuevoB.Datos.RemoveRange(factor, nuevoB.Datos.Count - factor);
				nuevoB.Llave.RemoveRange(factor, nuevoB.Llave.Count - factor);
				// Se realizan cambios en el archivo
				archivoArbol.InsertarNodo(nuevoB, nombreArchivo, true);
				archivoArbol.InsertarNodo(derecho, nombreArchivo, false);
				archivoArbol.InsertarNodo(padre, nombreArchivo, true);
				// Actualización de encabezado
				archivoArbol.ConstruccionEncabezado(nombreArchivo, archivoArbol.ObtenerRaiz(nombreArchivo), posicion + 1, tamaño, grado, 0);
				// Llamar recursivamente al padre
				UnderF(padre);
			}
		}

		public override bool Vacio()
		{
			if (this.raiz == null)
			{
				return true;
			}
			return false;
		}

		private bool Hoja(BNode<TKey, T> CorrienteB)
		{
			try
			{
				if (CorrienteB.HijoCon() == 0)
				{
					return true;
				}
				return false;
			}
			catch
			{
				return true;
			}
		}

		private BNode<TKey, T> Buscar(T buscarDato)
		{
			return BuscarDato(buscarDato, archivoArbol.ConvertirLinea(nombreArchivo, archivoArbol.ObtenerRaiz(nombreArchivo), grado));
		}
		private BNode<TKey, T> BuscarDato(T buscarDato, BNode<TKey, T> corriente)
		{
			int p = 0;
			while (p < corriente.DatoCon() && corriente.Datos[p].CompareTo(buscarDato) < 0)
				p = p++;
			if (corriente.Datos[p].CompareTo(buscarDato) == 0)
				return corriente;
			else if (corriente.Datos[p].CompareTo(buscarDato) < 0 && !Hoja(corriente))
				return BuscarDato(buscarDato, archivoArbol.ConvertirLinea(nombreArchivo, corriente.ApuntaHijo[grado - 1], grado));
			else if (corriente.Datos[p].CompareTo(buscarDato) > 0 && !Hoja(corriente))
				return BuscarDato(buscarDato, archivoArbol.ConvertirLinea(nombreArchivo, corriente.ApuntaHijo[p], grado));
			else
				return null;
		}

		public override void Eliminar(T eliminaDato)
		{
			BNode<TKey, T> escogido = Buscar(eliminaDato);
			int eliminarPosi = ObtenerPosicionEliminada(escogido, eliminaDato);
			if (!Hoja(escogido))
			{
				CambiarValores(escogido, eliminarPosi, DerEnIz(escogido, eliminarPosi));
				escogido = DerEnIz(escogido, eliminarPosi);
				eliminarPosi = escogido.LlaveCon() - 1;
			}
			escogido.Llave.RemoveAt(eliminarPosi);
			escogido.Datos.RemoveAt(eliminarPosi);
			archivoArbol.InsertarNodo(escogido, nombreArchivo, true);
			if (UnderFlow(escogido))
			{
				ResolverUF(escogido, eliminarPosi);
			}
			while (EliminarPadre.Count != 0)
			{
				int posiPadre = EliminarPadre.Dequeue();
				int posiIndex = EliminarIndex.Dequeue();
				BNode<TKey, T> PadreAfectado = archivoArbol.ConvertirLinea(nombreArchivo, posiPadre, grado);
				if (UnderFlow(PadreAfectado))
				{
					ResolverUF(PadreAfectado, posiIndex);
				}
			}
		}

		private void EliminarNodo(T eliminarValor, BNode<TKey, T> corriente)
		{
			BNode<TKey, T> escogido = corriente;
			int eliminarPosi = ObtenerPosicionEliminada(escogido, eliminarValor);
			if (!Hoja(escogido))
			{
				CambiarValores(escogido, eliminarPosi, DerEnIz(escogido, eliminarPosi));
				escogido = DerEnIz(escogido, eliminarPosi);
				eliminarPosi = escogido.LlaveCon() - 1;
			}
			escogido.Llave.RemoveAt(eliminarPosi);
			escogido.Datos.RemoveAt(eliminarPosi);
			if (UnderFlow(escogido))
			{
				ResolverUF(escogido, eliminarPosi);
			}
		}

		private int ObtenerPosicionEliminada(BNode<TKey, T> escogido, T eliminarValor)
		{
			int pos = -1;
			try
			{
				for (int i = 0; i < escogido.LlaveCon(); i++)
				{
					if (escogido.Datos[i].CompareTo(eliminarValor) == 0)
					{
						return i;
					}
				}
				return pos;
			}
			catch
			{
				return pos;
			}
		}

		private BNode<TKey, T> DerEnIz(BNode<TKey, T> escogido, int eliminarPosi)
		{
			BNode<TKey, T> nodoApunta = archivoArbol.ConvertirLinea(nombreArchivo, escogido.ApuntaHijo[eliminarPosi], grado);
			while (!Hoja(nodoApunta))
				nodoApunta = archivoArbol.ConvertirLinea(nombreArchivo, nodoApunta.ApuntaHijo[nodoApunta.HijoCon() - 1], grado);
			return nodoApunta;
		}

		private void CambiarValores(BNode<TKey, T> NodoHojaNo, int eliminarPosi, BNode<TKey, T> HojaNodo)
		{
			TKey pivoteTKey = NodoHojaNo.Llave[eliminarPosi];
			T pivoteT = NodoHojaNo.Datos[eliminarPosi];
			NodoHojaNo.Llave[eliminarPosi] = HojaNodo.Llave[HojaNodo.LlaveCon() - 1];
			NodoHojaNo.Datos[eliminarPosi] = HojaNodo.Datos[HojaNodo.DatoCon() - 1];
			HojaNodo.Llave[HojaNodo.LlaveCon() - 1] = pivoteTKey;
			HojaNodo.Datos[HojaNodo.DatoCon() - 1] = pivoteT;
			archivoArbol.InsertarNodo(HojaNodo, nombreArchivo, true);
			archivoArbol.InsertarNodo(NodoHojaNo, nombreArchivo, true);
		}

		private bool UnderFlow(BNode<TKey, T> escogido)
		{
			if (escogido.LlaveCon() <= (this.grado - 1) / 2 - 1)
			{
				return true;
			}
			return false;
		}

		private void ResolverUF(BNode<TKey, T> corriente, int eliminarPosi)
		{
			BNode<TKey, T> HermanoNodo = CambiarHemano(corriente);
			if (HermanoNodo.LlaveCon() <= (this.grado - 1) / 2)
			{
				Mezcla(corriente, HermanoNodo);
			}
			else
			{
				Prestamo(corriente, HermanoNodo, eliminarPosi);
			}
		}

		private void Mezcla(BNode<TKey, T> corriente, BNode<TKey, T> HermanoNodo)
		{
			int corrienteIndex = ObtenerNumeroHijosPadres(corriente.Posi, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado));
			int hermanoIndex = ObtenerNumeroHijosPadres(HermanoNodo.Posi, archivoArbol.ConvertirLinea(nombreArchivo, HermanoNodo.Padre, grado));
			int espacioIndex = ObtenerNumeroEspacioPadres(corriente, HermanoNodo);
			corriente = CrearMezcla(corrienteIndex, hermanoIndex, espacioIndex, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado));
			// Convertir nulo al hermano
			for (int i = 0; i < HermanoNodo.Datos.Count; i++)
			{
				HermanoNodo.Datos.RemoveAt(i);
				HermanoNodo.Llave.RemoveAt(i);
			}
			for (int i = 0; i < HermanoNodo.HijoCon(); i++)
			{
				HermanoNodo.ApuntaHijo[i] = -2147483648;
			}
			BNode<TKey, T> padre = archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado);
			archivoArbol.InsertarNodo(corriente, nombreArchivo, true);
			archivoArbol.InsertarNodo(padre, nombreArchivo, true);
			int hermanoPadre = ObtenerNumeroHijosPadres(HermanoNodo.Posi, padre);
			padre.ApuntaHijo[hermanoPadre] = -2147483648;
			archivoArbol.InsertarNodo(padre, nombreArchivo, true);
		}

		private void Prestamo(BNode<TKey, T> corriente, BNode<TKey, T> hermanoNodo, int elimonarPosi)
		{
			int prestamoHermano = ObtenerHermanoExtremo(corriente, hermanoNodo);
			int espacioIndex = ObtenerNumeroEspacioPadres(corriente, hermanoNodo);
			TKey espacioTKey = archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado).Llave[espacioIndex];
			T espacioT = archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado).Datos[espacioIndex];
			BNode<TKey, T> padre = archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado);
			padre.Llave[espacioIndex] = hermanoNodo.Llave[prestamoHermano];
			padre.Datos[espacioIndex] = hermanoNodo.Datos[prestamoHermano];
			corriente.Llave[elimonarPosi] = espacioTKey;
			corriente.Datos[elimonarPosi] = espacioT;
			hermanoNodo.Llave.RemoveAt(prestamoHermano);
			hermanoNodo.Datos.RemoveAt(prestamoHermano);
			if (prestamoHermano == 0 && hermanoNodo.HijoCon() > 0)
			{
				int hijo = hermanoNodo.ApuntaHijo[prestamoHermano];
				corriente.ApuntaHijo[corriente.HijoCon()] = hijo;
				hermanoNodo.ApuntaHijo[prestamoHermano] = -2147483648;
			}
			else if (hermanoNodo.HijoCon() > 0)
			{
				corriente.ApuntaHijo[0] = hermanoNodo.ApuntaHijo[prestamoHermano];
				hermanoNodo.ApuntaHijo[prestamoHermano] = -2147483648;
			}
			archivoArbol.InsertarNodo(padre, nombreArchivo, true);
			archivoArbol.InsertarNodo(corriente, nombreArchivo, true);
			archivoArbol.InsertarNodo(hermanoNodo, nombreArchivo, true);

		}

		private BNode<TKey, T> CambiarHemano(BNode<TKey, T> corriente)
		{
			int posiCorriente = ObtenerNumeroHijosPadres(corriente.Posi, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado));
			int izquierda = -1;
			int derecha = -1;
			if (posiCorriente > 0)
			{
				izquierda = posiCorriente - 1;
			}
			if (posiCorriente < archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado).HijoCon() - 1)
			{
				derecha = posiCorriente + 1;
			}
			if (izquierda > 0 && derecha > 0)
			{
				if (archivoArbol.ConvertirLinea(nombreArchivo, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado).ApuntaHijo[derecha], grado).LlaveCon() > archivoArbol.ConvertirLinea(nombreArchivo, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado).ApuntaHijo[izquierda], grado).LlaveCon())
				{
					return archivoArbol.ConvertirLinea(nombreArchivo, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado).ApuntaHijo[derecha], grado);
				}
				else
				{
					return archivoArbol.ConvertirLinea(nombreArchivo, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado).ApuntaHijo[izquierda], grado);
				}
			}
			else
			{
				if (izquierda < 0)
				{
					return archivoArbol.ConvertirLinea(nombreArchivo, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado).ApuntaHijo[derecha], grado);
				}
				else
				{
					return archivoArbol.ConvertirLinea(nombreArchivo, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado).ApuntaHijo[izquierda], grado);
				}
			}

		}

		private int ObtenerNumeroHijosPadres(int posiHijo, BNode<TKey, T> nodoPadre)
		{
			int index = 0;
			for (int i = 0; i < nodoPadre.HijoCon(); i++)
			{
				if (archivoArbol.ConvertirLinea(nombreArchivo, nodoPadre.ApuntaHijo[i], grado).Posi == posiHijo)
				{
					index = i;
					return index;
				}
			}
			return index;
		}

		private int ObtenerNumeroEspacioPadres(BNode<TKey, T> corriente, BNode<TKey, T> hermanoNodo)
		{
			int corrienteIndex = ObtenerNumeroHijosPadres(corriente.Posi, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado));
			int hermanoIndex = ObtenerNumeroHijosPadres(hermanoNodo.Posi, archivoArbol.ConvertirLinea(nombreArchivo, hermanoNodo.Padre, grado));
			int indexEspacioPadre = Math.Max(corrienteIndex, hermanoIndex) - 1;
			return indexEspacioPadre;
		}

		private void EliminarUnder(BNode<TKey, T> corrienete, int valorIndex)
		{
			corrienete.Llave.RemoveAt(valorIndex);
			corrienete.Datos.RemoveAt(valorIndex);
			archivoArbol.InsertarNodo(corrienete, nombreArchivo, true);
		}

		private BNode<TKey, T> CrearMezcla(int corriente, int hermanoNodo, int espacioPadre, BNode<TKey, T> padrel)
		{
			BNode<TKey, T> nuevoNodo;
			// Eliminar al padre del que ya se eliminio
			if (corriente < hermanoNodo)
			{
				nuevoNodo = archivoArbol.ConvertirLinea(nombreArchivo, padrel.ApuntaHijo[corriente], grado);
				nuevoNodo.Llave[nuevoNodo.LlaveCon()] = padrel.Llave[espacioPadre];
				nuevoNodo.Datos[nuevoNodo.DatoCon()] = padrel.Datos[espacioPadre];
				EliminarIndex.Enqueue(espacioPadre);
				EliminarPadre.Enqueue(padrel.Posi);
				EliminarUnder(padrel, espacioPadre);
				BNode<TKey, T> hermano = archivoArbol.ConvertirLinea(nombreArchivo, padrel.ApuntaHijo[hermanoNodo], grado);
				for (int i = 0; i < hermano.LlaveCon(); i++)
				{
					nuevoNodo.Llave[nuevoNodo.LlaveCon()] = hermano.Llave[i];
					nuevoNodo.Datos[nuevoNodo.DatoCon()] = hermano.Datos[i];
				}
				for (int j = 0; j < hermano.HijoCon(); j++)
				{
					nuevoNodo.ApuntaHijo[nuevoNodo.HijoCon()] = hermano.ApuntaHijo[j];
				}
				BNode<TKey, T> padre = archivoArbol.ConvertirLinea(nombreArchivo, hermano.Padre, grado);
				padre.ApuntaHijo.Remove(hermano.Posi);
				padre.ApuntaHijo[padre.HijoCon()] = nuevoNodo.Posi;
				archivoArbol.InsertarNodo(padre, nombreArchivo, true);
				hermano.Padre = -2147483648;
				archivoArbol.InsertarNodo(hermano, nombreArchivo, true);
			}
			else
			{
				nuevoNodo = archivoArbol.ConvertirLinea(nombreArchivo, padrel.ApuntaHijo[hermanoNodo], grado);
				nuevoNodo.Llave[nuevoNodo.LlaveCon()] = (padrel.Llave[espacioPadre]);
				nuevoNodo.Datos[nuevoNodo.LlaveCon()] = (padrel.Datos[espacioPadre]);
				EliminarIndex.Enqueue(espacioPadre);
				EliminarPadre.Enqueue(padrel.Posi);
				EliminarUnder(padrel, espacioPadre);
				BNode<TKey, T> corrient = archivoArbol.ConvertirLinea(nombreArchivo, padrel.ApuntaHijo[corriente], grado);
				for (int i = 0; i < corrient.LlaveCon(); i++)
				{
					nuevoNodo.Llave[nuevoNodo.LlaveCon()] = corrient.Llave[i];
					nuevoNodo.Datos[nuevoNodo.LlaveCon()] = corrient.Datos[i];
				}
				for (int j = 0; j < corrient.HijoCon(); j++)
				{
					nuevoNodo.ApuntaHijo[nuevoNodo.HijoCon()] = corrient.ApuntaHijo[j];
				}
				BNode<TKey, T> padrelo = archivoArbol.ConvertirLinea(nombreArchivo, corrient.Padre, grado);
				padrelo.ApuntaHijo.Remove(corrient.Posi);
				padrelo.ApuntaHijo[padrelo.HijoCon()] = nuevoNodo.Posi;
				archivoArbol.InsertarNodo(padrelo, nombreArchivo, true);
				corrient.Padre = -2147483648;
				archivoArbol.InsertarNodo(corrient, nombreArchivo, true);
			}
			List<int> hijoCorrecto = new List<int>();
			for (int i = 0; i < nuevoNodo.ApuntaHijo.Count; i++)
			{
				if (nuevoNodo.ApuntaHijo[i] > -2147483648)
				{
					hijoCorrecto.Add(nuevoNodo.ApuntaHijo[i]);
				}
			}
			nuevoNodo.ApuntaHijo = hijoCorrecto;
			return nuevoNodo;
		}

		private int ObtenerHermanoExtremo(BNode<TKey, T> corriente, BNode<TKey, T> hermanoNodo)
		{
			int corrient = ObtenerNumeroHijosPadres(corriente.Posi, archivoArbol.ConvertirLinea(nombreArchivo, corriente.Padre, grado));
			int hermano = ObtenerNumeroHijosPadres(hermanoNodo.Posi, archivoArbol.ConvertirLinea(nombreArchivo, hermanoNodo.Padre, grado));
			int extremoApropiado = -1;
			if (corrient > hermano)
			{
				extremoApropiado = hermanoNodo.LlaveCon() - 1;
			}
			else
			{
				extremoApropiado = 0;
			}
			return extremoApropiado;
		}

		public override void Actualizar(T actualizaDato)
		{
			throw new NotImplementedException();
		}

		public override List<object> ConvertirAObj()
		{
			throw new NotImplementedException();
		}

		public int Grado
		{
			get
			{
				return grado;
			}
		}

		public string NombreArchivo
		{
			get
			{
				return nombreArchivo;
			}
		}

	}
}
