using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase que representa una neurona en la red neuronal
public class Neuron
{
    // Número de entradas que recibe la neurona
    public int numInputs;

    // Valor de sesgo (bias) de la neurona, que se ajusta junto con los pesos
    public double bias;

    // Salida de la neurona después de aplicar la función de activación
    public double output;

    // Gradiente del error usado para la retropropagación
    public double errorGradient;

    // Lista de pesos asociados a las entradas de la neurona
    public List<double> weights = new List<double>();

    // Lista de entradas que recibe la neurona
    public List<double> inputs = new List<double>();

    // Constructor de la clase Neuron, recibe el número de entradas que la neurona manejará
    public Neuron(int nInputs)
    {
        // Inicializa el bias con un valor aleatorio entre -1.0 y 1.0
        bias = UnityEngine.Random.Range(-1.0f, 1.0f);

        // Asigna el número de entradas
        numInputs = nInputs;

        // Inicializa los pesos de las conexiones (entradas) con valores aleatorios entre -1.0 y 1.0
        for (int i = 0; i < numInputs; i++)
        {
            weights.Add(UnityEngine.Random.Range(-1.0f, 1.0f));
        }
    }

    // Start es llamado antes de que comience el primer frame (es un método de Unity)
    void Start()
    {

    }

    // Update es llamado en cada frame (es un método de Unity)
    void Update()
    {

    }
}
