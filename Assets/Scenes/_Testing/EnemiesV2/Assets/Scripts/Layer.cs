using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase que representa una capa en la red neuronal
public class Layer
{
    // Número de neuronas en la capa
    public int numNeurons;

    // Lista que contiene todas las neuronas de la capa
    public List<Neuron> neurons = new List<Neuron>();

    // Constructor de la clase Layer, que inicializa las neuronas en la capa
    public Layer(int nNeurons, int nInputs)
    {
        numNeurons = nNeurons;  // Asigna el número de neuronas a la capa

        // Por cada neurona, se instancia una nueva neurona con un número de entradas igual a 'nInputs'
        for (int i = 0; i < numNeurons; i++)
        {
            neurons.Add(new Neuron(nInputs));  // Añadir neurona a la lista de neuronas
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
