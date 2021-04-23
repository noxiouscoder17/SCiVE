using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestManager : MonoBehaviour
{
    public GeneticManager manager;
    public CarController controller;
    
    public int bestGen = 0;
    public int bestGenome = 0;
    public float bestFitness = 0;

    [Header("UI")]
    public Text GenerationText;
    public Text GenomeText;
    public Text FitnessText;

    private void Update()
    {
        if (bestFitness <= controller.overallFitness)
        {
            if (GenerationText != null)
                GenerationText.text = ("Best Generation: " + (manager.currentGeneration));
            if (GenomeText != null)
                GenomeText.text = ("Best Genome: " + (manager.currentGenome));
            if (FitnessText != null)
                FitnessText.text = ("Best Fitness: " + (controller.overallFitness));
            bestGen = manager.currentGeneration;
            bestGenome = manager.currentGenome;
            bestFitness = controller.overallFitness;
        }
    }
}
