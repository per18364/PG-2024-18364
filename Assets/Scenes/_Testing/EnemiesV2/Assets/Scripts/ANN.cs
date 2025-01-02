using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase que define la estructura de una red neuronal artificial
public class ANN
{
    // Número de entradas, salidas, capas ocultas, neuronas por capa oculta y el valor del factor de aprendizaje (alpha)
    public int numInputs;
    public int numOutputs;
    public int numHidden;
    public int numNPerHidden;
    public double alpha;

    // Lista de capas (cada capa contendrá varias neuronas)
    List<Layer> layers = new List<Layer>();

    // Constructor de la red neuronal, inicializa los parámetros de la red
    public ANN(int nI, int nO, int nH, int nPH, double a)
    {
        numInputs = nI;  // Número de entradas
        numOutputs = nO;  // Número de salidas
        numHidden = nH;  // Número de capas ocultas
        numNPerHidden = nPH;  // Número de neuronas por capa oculta
        alpha = a;  // Tasa de aprendizaje

        // Si la red tiene capas ocultas, se crean las capas ocultas y la de salida
        if (numHidden > 0)
        {
            // Crear la primera capa oculta conectada a las entradas
            layers.Add(new Layer(numNPerHidden, numInputs));

            // Crear las demás capas ocultas conectadas entre sí
            for (int i = 0; i < numHidden - 1; i++)
            {
                layers.Add(new Layer(numNPerHidden, numNPerHidden));
            }

            // Crear la capa de salida conectada a la última capa oculta
            layers.Add(new Layer(numOutputs, numNPerHidden));
        }
        else
        {
            // Si no hay capas ocultas, se crea directamente la capa de salida conectada a las entradas
            layers.Add(new Layer(numOutputs, numInputs));
        }
    }

    // Método que procesa las entradas y realiza el feedforward, además de actualizar los pesos
    public List<double> Go(List<double> inputValues, List<double> desiredOutput = null)
    {
        List<double> inputs = new List<double>();
        List<double> outputs = new List<double>();

        // Verificar que el número de entradas sea el correcto
        if (inputValues.Count != numInputs)
        {
            Debug.Log("ERROR: Number of Inputs must be " + numInputs);
            return outputs;  // Devolver lista vacía si hay error
        }

        inputs = new List<double>(inputValues);  // Asignar las entradas

        // Proceso de feedforward a través de las capas
        for (int i = 0; i < numHidden + 1; i++)
        {
            // Si no es la primera capa, las entradas se igualan a las salidas anteriores
            if (i > 0)
            {
                inputs = new List<double>(outputs);
            }

            outputs.Clear();  // Limpiar las salidas anteriores

            // Calcular las salidas de cada neurona en la capa actual
            for (int j = 0; j < layers[i].numNeurons; j++)
            {
                double N = 0;  // Variable que almacena la suma ponderada

                // Limpiar las entradas previas de la neurona
                layers[i].neurons[j].inputs.Clear();

                // Para cada entrada, multiplicar por su peso correspondiente y sumar
                for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
                {
                    layers[i].neurons[j].inputs.Add(inputs[k]);
                    N += layers[i].neurons[j].weights[k] * inputs[k];
                }

                // Restar el valor del sesgo (bias) de la neurona
                N -= layers[i].neurons[j].bias;

                // Aplicar la función de activación y guardar la salida de la neurona
                layers[i].neurons[j].output = ActivationFunction(N);
                outputs.Add(layers[i].neurons[j].output);
            }
        }

        // Actualizar los pesos de las neuronas usando retropropagación
        // UpdateWeights(outputs, desiredOutput);

        if (desiredOutput != null)
        {
            UpdateWeights(outputs, desiredOutput);
        }

        // Devolver las salidas finales después del feedforward
        return outputs;
    }

    // Método para actualizar los pesos de la red usando retropropagación
    public void UpdateWeights(List<double> outputs, List<double> desiredOutput)
    {
        double error;

        // Retropropagación de errores, de la capa de salida hacia atrás
        for (int i = numHidden; i >= 0; i--)
        {
            for (int j = 0; j < layers[i].numNeurons; j++)
            {
                if (i == numHidden)
                {
                    // Para la capa de salida: calcular el gradiente del error
                    error = desiredOutput[j] - outputs[j];
                    layers[i].neurons[j].errorGradient = outputs[j] * (1 - outputs[j]) * error;
                }
                else
                {
                    // Para las capas ocultas: calcular el gradiente basado en los gradientes de la siguiente capa
                    layers[i].neurons[j].errorGradient = layers[i].neurons[j].output * (1 - layers[i].neurons[j].output);

                    double errorGradSum = 0;
                    // Sumar los gradientes de las neuronas de la capa siguiente
                    for (int p = 0; p < layers[i + 1].numNeurons; p++)
                    {
                        errorGradSum += layers[i + 1].neurons[p].errorGradient * layers[i + 1].neurons[p].weights[j];
                    }
                    layers[i].neurons[j].errorGradient *= errorGradSum;
                }

                // Actualizar los pesos de la neurona
                for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
                {
                    if (i == numHidden)
                    {
                        // Actualizar pesos de las neuronas en la capa de salida
                        error = desiredOutput[j] - outputs[j];
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * error;
                    }
                    else
                    {
                        // Actualizar pesos de las neuronas en las capas ocultas
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * layers[i].neurons[j].errorGradient;
                    }
                }

                // Actualizar el bias (sesgo) de la neurona
                layers[i].neurons[j].bias += alpha * -1 * layers[i].neurons[j].errorGradient;
            }
        }
    }



    // Función de activación, en este caso se usa la función sigmoide
    public double ActivationFunction(double value)
    {
        // return Sigmoid(value);
        return ReLU(value);
    }

    // Función sigmoide: calcula la salida de una neurona en función de su suma ponderada
    public double Sigmoid(double value)
    {
        double k = (double)System.Math.Exp(value);
        return k / (1.0f + k);  // Fórmula de la sigmoide
    }

    // Función de activación ReLU
    public double ReLU(double value)
    {
        return Mathf.Max(0, (float)value);  // ReLU retorna 0 si el valor es negativo
    }

    // Start es un método que Unity llama al inicio del juego
    void Start()
    {

    }

    // Update es un método que Unity llama en cada frame
    void Update()
    {

    }
}
