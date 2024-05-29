namespace _8PuzzleAStar
{
    using System;
    using System.Collections.Generic;

    public class Node : IComparable<Node?>
    {
        public int CostoFuncion { get; } // Costo total estimado del nodo
        public int CostoOperador { get; } // Costo acumulado de los operadores para llegar a este nodo
        public int PosicionVacio { get; } // Índice del espacio vacío en el rompecabezas
        public List<int> EstadoActual { get; }
        public Node? NodoPadre { get; }

        public Node(int costoOperador, int posicionVacio, List<int> estadoActual, List<int> estadoMeta, Node? nodoPadre)
        {
            CostoOperador = costoOperador;
            PosicionVacio = posicionVacio;
            EstadoActual = new List<int>(estadoActual);
            NodoPadre = nodoPadre;
            CostoFuncion = CostoOperador + CalcularHeuristica(estadoActual, estadoMeta);
        }

        public int CalcularHeuristica(List<int> estadoActual, List<int> estadoMeta)
        {
            int h = 0;
             // Recorre cada posición del 8Puzzle
            for (int i = 0; i < estadoActual.Count; i++)
            {
                 // Si el valor en la posición actual no es cero y no coincide con el estado meta,
                // incrementa la heurística
                if (estadoActual[i] != 0 && estadoActual[i] != estadoMeta[i])
                {
                    h++;
                }
            }
            return h;
        }

        public List<int> PosiblesMovimientos()
        {
            var movimientos = new List<int>();
            // Recuerda que posicion vacio es el indice del 0
            int fila = PosicionVacio / 3;
            int columna = PosicionVacio % 3;

            // Recuerda que esta parte agrega los movimientos posibles partiendo del indice del 0 hacia arriba, abajo, izquierda y derecha y los regresa en una lista
            if (fila > 0) movimientos.Add(PosicionVacio - 3); // arriba
            if (fila < 2) movimientos.Add(PosicionVacio + 3); // abajo
            if (columna > 0) movimientos.Add(PosicionVacio - 1); // izquierda
            if (columna < 2) movimientos.Add(PosicionVacio + 1); // derecha

            return movimientos;
        }

        // Método para comparar dos nodos según su costo total estimado
        public int CompareTo(Node? other)
        {
            if (other == null) return 1;
            // Compara los costos totales estimados de los nodos
            int comparison = CostoFuncion.CompareTo(other.CostoFuncion);
            // Si los costos totales son iguales, compara los costos acumulados de los operadores
            if (comparison == 0)
            {
                comparison = CostoOperador.CompareTo(other.CostoOperador);
            }
            return comparison;
        }
    }

    public class Program
    {
        public static void Main()
        {
            var estadoInicial = LeerEstado("Ingrese el E.I:");
            var estadoMeta = LeerEstado("Ingrese el E.M:");

            // Aquí se crea un conjunto ordenado para almacenar los nodos y se inicializa un conjunto para almacenar los estados visitados.
            var priorityQueue = new SortedSet<Node>();
            var visitedStates = new HashSet<string>();

            // Encuentra la posición del espacio vacío en el estado inicial y crea el nodo raíz
            int vacio = PosicionVacio(estadoInicial);
            var root = new Node(0, vacio, estadoInicial, estadoMeta, null);
            priorityQueue.Add(root);
            visitedStates.Add(EstadoToString(estadoInicial));

            Node? resultado = null; // Nodo que almacenará el resultado de la búsqueda

            // Mientras haya nodos en la cola de prioridad
            while (priorityQueue.Count > 0)
            {
                var nodoActual = priorityQueue.Min; // Obtiene el nodo con el menor costo total estimado
                if (nodoActual == null) continue; // Si el nodo es nulo, continúa con el siguiente
                priorityQueue.Remove(nodoActual); // Elimina el nodo de la cola de prioridad

                int h = nodoActual.CostoFuncion - nodoActual.CostoOperador; // Calcula la heurística

                // Si la heurística es 0, significa que se ha llegado al estado meta
                if (h == 0)
                {
                    resultado = nodoActual;
                    break;
                }

                Console.WriteLine($"Explorando nodo al nivel: {nodoActual.CostoOperador + 1} <---------------------------------------------------------------------------------->");
                ImprimirEstado(nodoActual.EstadoActual);
                Console.WriteLine();

                // Recuerda que aquí se obtienen los números que se pueden mover y se almacenan en la variable posiblesMovimientos.
                // Luego, se crea un nuevo estado basado en el estado actual, y con los índices de los posibles movimientos,
                // se intercambian los valores de las posiciones para simular cada posible movimiento del espacio vacío.
                var posiblesMovimientos = nodoActual.PosiblesMovimientos();
                var nodosHijos = new List<Node>();

                foreach (var index in posiblesMovimientos)
                {
                    // Se crea una copia del estado actual para modificarla.
                    var nuevoEstado = new List<int>(nodoActual.EstadoActual);
                    // Se intercambian los valores entre la posición del espacio vacío y la posición a la que se va a mover.
                    nuevoEstado[nodoActual.PosicionVacio] = nuevoEstado[index];
                    nuevoEstado[index] = 0; // El espacio vacío se mueve a la posición 'index'.

                    // Si el nuevo estado generado no ha sido visitado antes,
                    // se crea un nuevo nodo con ese estado y se agrega a la lista de nodos hijos.
                    if (!visitedStates.Contains(EstadoToString(nuevoEstado)))
                    {
                        var nuevoNodo = new Node(nodoActual.CostoOperador + 1, index, nuevoEstado, estadoMeta, nodoActual);
                        nodosHijos.Add(nuevoNodo);
                        visitedStates.Add(EstadoToString(nuevoEstado)); // Se marca como visitado para evitar ciclos.
                    }
                }

                Console.WriteLine("Posibles estados y sus costos:<---------------------------------------------------------------------------------->");
                foreach (var nodo in nodosHijos)
                {
                    ImprimirEstado(nodo.EstadoActual);
                    Console.WriteLine($"Costo heurístico: {nodo.CostoFuncion}\n");
                }

                // Selección del nodo con el menor costo
                var nodoConMenorCosto = nodosHijos.Count > 0 ? nodosHijos.Min() : null;
                if (nodoConMenorCosto != null)
                {
                    Console.WriteLine("Estado seleccionado con el menor costo:<---------------------------------------------------------------------------------->");
                    ImprimirEstado(nodoConMenorCosto.EstadoActual);
                    Console.WriteLine($"Costo heurístico: {nodoConMenorCosto.CostoFuncion}\n");
                    priorityQueue.Add(nodoConMenorCosto);
                }
            }

            if (resultado != null)
            {
                Console.WriteLine("Resultado <---------------------------------------------------------------------------------->");
                Console.WriteLine($"Función heurística final: {resultado.CostoFuncion}");
                Console.WriteLine($"Pasos hacia la solución: {resultado.CostoOperador}\n");
                ImprimirEstado(resultado.EstadoActual);
            }
            else
            {
                Console.WriteLine("Hubo un error y no se llegó a la solución");
            }
        }

        private static List<int> LeerEstado(string mensaje)
        {
            Console.WriteLine(mensaje);
            var estado = new List<int>();
            var input = Console.ReadLine().Split(' ');

            foreach (var item in input)
            {
                estado.Add(int.Parse(item));
            }

            if (estado.Count < 9)
            {
                throw new Exception("Estado mal formado");
            }

            return estado;
        }

        private static int PosicionVacio(List<int> arr)
        {
            return arr.IndexOf(0);
        }

        private static string EstadoToString(List<int> estado)
        {
            return string.Join(",", estado);
        }

        private static void ImprimirEstado(List<int> estado)
        {
            for (int i = 0; i < estado.Count; i++)
            {
                if (i % 3 == 0) Console.WriteLine();
                Console.Write(estado[i] == 0 ? " " : estado[i].ToString());
                Console.Write(" ");
            }
            Console.WriteLine();
        }
    }
}
