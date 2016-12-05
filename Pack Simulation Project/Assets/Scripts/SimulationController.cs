﻿using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

static class Config {

    public const int NUMBER_OF_RUNS = 15;
    public const int SIMULATION_SPEED_MULTIPLIER = 1; // 1-100
    public const string FILE_PATH = "c:\\temp\\MyTest.txt";

    /* Predator Config values */
    public const float PREDATOR_SPREAD = 45.0f;
    public const float PREDATOR_DISTANCE = -50.0f;
    public const int PREDATOR_COUNT = 5;
    public const float PREDATOR_WALK_SPEED = 2.0f;
    public const float PREDATOR_RUN_SPEED = 16.0f;
    public const float PREDATOR_VISION_RADIUS = 150.0f;
    public const bool PREDATOR_DIFFERENT_STARTING_DIRECTION = false;
    public const int PREDATOR_VARIANT_STARTING_DIRECTION = 8;
    public const int PREDATOR_STARTING_DIRECTION = 1;

    /* Prey Config values */
    public const float PREY_SPREAD = 100.0f;
    public const float PREY_DISTANCE = 75.0f;
    public const int PREY_COUNT = 15;
    public const float PREY_WALK_SPEED = 1.25f;
    public const float PREY_RUN_SPEED = 20.0f;
    public const float PREY_MIN_RAND_SPEED = 15.0f; // minimum run speed possible when using random speed generation
    public const float PREY_MAX_RAND_SPEED = 20.0f; // maximum run speed possible when using random speed generation
    public const float PREY_VISION_RADIUS = 75.0f;
    public const bool PREY_USE_RANDOM_SPEEDS = false; // when true, uses random speed generation for each prey
    public const bool PREY_DIFFERENT_STARTING_DIRECTION = true;
    public const int PREY_VARIANT_STARTING_DIRECTION = 12;
    public const int PREY_STARTING_DIRECTION = 0;

    /* Values for control over weaknesses in the prey group
     * NOT IMPLEMENTED YET
     */
    public const int NUMBER_OF_VULNERABLE_PREY = 0;
    public const double WEAKNESS_PERCENT = 0.75;

    /* Controls if the simulation will be initialized
     * with random seed, or a provided seed.
     */
    public const bool GEN_RANDOM_SEED = true;
    public const int SEED = 38293423; /* If GEN_RANDOM_SEED is true, this value is irrelevant.         */
                                      /* Will be used to control all random aspects of initialization. */

    /* Constants. Do not change unless there is a good reason. */
    public const float HEIGHT_PLANE = -25.8267f; // The y-axis coordinate

}

public class SimulationController : MonoBehaviour {

    public GameObject prey;
    public GameObject predator;

    //public PredatorAgent[] predators;
    //public PreyAgent[] preys;
    private GameObject[] predators;
    private GameObject[] preys;

    private int runCount;
    private int mySuccesses;
    private int myFailures;

	// Use this for initialization
	void Start () {
        runCount = 0;
        Time.timeScale = 1.0f * (float) Config.SIMULATION_SPEED_MULTIPLIER;
        //Time.fixedDeltaTime = 1.0f * (float) Config.SIMULATION_SPEED_MULTIPLIER;

        initEntities();
    }

    /**
     * @return Returns the list of prey currently in the simulation.
     */ 
    public GameObject[] getPreyList() { 
        return preys;
    }


    private void initEntities()
    {
        preys = initGroup(prey,
                          Config.PREY_COUNT,
                          Config.PREY_SPREAD,
                          Config.PREY_DISTANCE,
                          Config.GEN_RANDOM_SEED,
                          Config.PREY_VARIANT_STARTING_DIRECTION,
                          Config.PREY_DIFFERENT_STARTING_DIRECTION,
                          Config.PREY_STARTING_DIRECTION);

        predators = initGroup(predator,
                          Config.PREDATOR_COUNT,
                          Config.PREDATOR_SPREAD,
                          Config.PREDATOR_DISTANCE,
                          Config.GEN_RANDOM_SEED,
                          Config.PREDATOR_VARIANT_STARTING_DIRECTION,
                          Config.PREDATOR_DIFFERENT_STARTING_DIRECTION,
                          Config.PREDATOR_STARTING_DIRECTION);
    }
    
    /*
     * Generates a group of entities with the given parameters
     * to determine the amount of entities in the group, the
     * individual starting positions, and the starting directions.
     *
     * @theObject             The type of entity being generated.
     * @theCount              The amount of the entity being generated.
     * @theSpread             The distance factor for how far apart entities are positioned.
     * @isRand                Determines if a new seed will be generated.
     * @theDirectionVariance  How much the starting direction angle will vary from other entities.
     * @isRandDirections      Determines if random directions will be generated.
     * @theStartingDirection  The starting direction of the entity if random ones aren't generated.
     *
     * @return                An array of GameObjects for the type of entity being generated.
     */
    private static GameObject[] initGroup(GameObject theObject, 
                                          int theCount, 
                                          float theSpread,
                                          float theDistance, 
                                          bool isRand,
                                          int theDirectionVariance, 
                                          bool isRandDirections,
                                          int theStartingDirection) {
        GameObject[] objects;
        objects = new GameObject[theCount];

        if (!isRand) {
            UnityEngine.Random.InitState(Config.SEED);
        }

        for (int i = 0; i < theCount; i++) {
            Vector3 position = getRandomPosition(theSpread, theDistance);
            Quaternion direction = getDirection(isRandDirections, theDirectionVariance, theStartingDirection);

            objects[i] = (GameObject) Instantiate(theObject, position, direction);
        }

        /* OLD CODE
        for (int i = 0; i < theCount; i++) {
            xPos = UnityEngine.Random.Range(-1 * theSpread / 2, theSpread / 2);
            zPos = UnityEngine.Random.Range(-1 * theSpread / 2, theSpread / 2) + theDistance;
            objects[i] = (GameObject) Instantiate(theObject,
                                                 new Vector3(xPos, Config.HEIGHT_PLANE, zPos),
                                                 Quaternion.identity);
        }
        */

        return objects;
    }



    /*
     * Helper function for calculating the starting positions.
     * Starting positions are randomized via polar coordinates.
     *
     */
    private static Vector3 getRandomPosition(float theSpread, float theDistance)
    {
        float xPos = 0;
        float zPos = 0 + theDistance;
        float ranAngle = UnityEngine.Random.Range(0, 359);
        float ranMagnitude = UnityEngine.Random.Range(0, theSpread / 2);

        xPos = xPos + (Mathf.Cos(ranAngle) * ranMagnitude);
        zPos = zPos + (Mathf.Sin(ranAngle) * ranMagnitude);
        
        return new Vector3(xPos, Config.HEIGHT_PLANE, zPos);
    }

    /*
     * Helper function for calculating the starting directions.
     * 
     */
    private static Quaternion getDirection(bool isRandDirections, int theDirectionVariance, int theStartingDirection)
    {
        float ranDirection;

        if (isRandDirections)
        {
            ranDirection = UnityEngine.Random.Range(0, 359);
        }
        else
        {
            ranDirection = theStartingDirection;
        }
        float ranVariance = UnityEngine.Random.Range(-1 * theDirectionVariance, theDirectionVariance);
        ranDirection += ranVariance;

        return Quaternion.Euler(0, ranDirection, 0);
    }


    // Update is called once per frame
    void Update () {

        if (preys != null && predators != null && runCount < Config.NUMBER_OF_RUNS )
        {
            bool isOver = true;
            for (int i = 0; i < predators.Length; i++)
            {
                if (predators[i].gameObject.GetComponent<PredatorAgent>().areTargets)
                {
                    // A predator can still see a prey, simulation isn't over yet.
                    isOver = false;
                    break;
                }
            }

            // Only check prey if simulation wasn't determined to be over from predator checks.
            if (!isOver)
            {
                for (int i = 0; i < preys.Length; i++)
                {
                    if (preys[i].gameObject.GetComponent<PreyAgent>().health <= 0)
                    {
                        // A prey is dead, simulation is over.
                        isOver = true;
                        mySuccesses++;
                        break;
                    }
                }
            }
            else
            {
                // Predators failed hunt, can't see anymore prey.
                myFailures++;
            }

            // If simulation is over, reload the scene.
            if (isOver)
            {
                reloadScene();
            }
        }

        // for built versions of the simulation escape will exit the application
        if (Input.GetKey(KeyCode.Escape)) 
        {
            Application.Quit();
        }

    }

    private void reloadScene()
    {
        runCount++;
        // Transcribe data here.

        // Only reload the scene if there are still runs to do, otherwise freeze simulation.
        if (runCount < Config.NUMBER_OF_RUNS)
        {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            for (int i = 0; i < predators.Length; i++)
            {
                Destroy(predators[i]);
            }
            for (int i = 0; i < preys.Length; i++)
            {
                Destroy(preys[i]);
            }
            initEntities();
        }
        else
        {
            // Out of runs, freeze simulation and write report to file.
            Time.timeScale = 0.0f;
            //Time.fixedDeltaTime = 0.0f;
            string dataReport = generateReport(mySuccesses, myFailures);
            writeToFile(dataReport);
        }
        
    }


    private static string generateReport(int successCount, int failureCount)
    {
        StringBuilder report = new StringBuilder();

        report.Append(String.Format("{0,-20}", "Number of Runs"));
        report.Append(Config.NUMBER_OF_RUNS);
        report.AppendLine();

        report.Append(String.Format("{0,-20}", "Starting Distance"));
        float distance = Mathf.Abs(Config.PREDATOR_DISTANCE) + Mathf.Abs(Config.PREY_DISTANCE);
        report.Append(distance);
        report.AppendLine();
        report.AppendLine();



        report.Append(String.Format("{0,-15}{1,10}{2,10}", "", "Predator", "Prey"));
        report.AppendLine();

        report.Append(String.Format("{0,15}{1,10}{2,10}", "Count", Config.PREDATOR_COUNT, Config.PREY_COUNT));
        report.AppendLine();

        report.Append(String.Format("{0,15}{1,10}{2,10}", "Walk Speed", Config.PREDATOR_WALK_SPEED, Config.PREY_WALK_SPEED));
        report.AppendLine();

        report.Append(String.Format("{0,15}{1,10}{2,10}", "Run Speed", Config.PREDATOR_RUN_SPEED, Config.PREY_RUN_SPEED));
        report.AppendLine();





        /* Success/Failure */
        report.Append("---------------------------------------");

        report.AppendLine();
        report.Append(String.Format("{0, -15}", "Success"));
        report.Append(String.Format("{0, -5}", successCount));
        float successPercent = successCount / Config.NUMBER_OF_RUNS;
        report.Append(String.Format("{0,-5:P1}", successPercent));
        report.AppendLine();



        report.Append(String.Format("{0, -15}", "Failure"));
        report.Append(String.Format("{0, -5}", failureCount));
        float failurePercent = failureCount / Config.NUMBER_OF_RUNS;
        report.Append(String.Format("{0,-5:P1}", failurePercent));
        report.AppendLine();

        report.AppendLine();
        report.Append("*************************************************");
        report.AppendLine();

        return report.ToString();
    }



    private static void writeToFile(string theData)
    {
        if (!File.Exists(Config.FILE_PATH))
        {
            string createText = "Simulation Data" + Environment.NewLine + Environment.NewLine;
            File.WriteAllText(Config.FILE_PATH, createText);
        }

        File.AppendAllText(Config.FILE_PATH, theData + Environment.NewLine);

    }
}
