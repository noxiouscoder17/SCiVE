using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenManager : MonoBehaviour
{
    public GeneticManager manager;
    public CarController controller;

    [Header("UI")]
    public Text GenerationText;
    public Text GenomeText;
    public Text FitnessText;

    private void Update()
    {
        if (GenerationText != null)
            GenerationText.text = ("Generation: " + (manager.currentGeneration));
        if (GenomeText != null)
            GenomeText.text = ("Genome: " + (manager.currentGenome));
        if (GenerationText != null)
            FitnessText.text = ("Fitness: " + (controller.overallFitness));

    }
}
