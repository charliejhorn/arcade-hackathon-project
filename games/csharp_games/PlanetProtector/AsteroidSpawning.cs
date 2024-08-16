using System.Collections.Generic;
using SplashKitSDK;
using System;
using System.Linq;

namespace PlanetProtector
{
    class AsteroidSpawning
    {
        // PARAMETERS AND VARIABLES
        private float _initialSpawnRate = 1.0f; // Initial rate at which asteroids start spawning
        private int _maxAsteroids = 50; // Maximum number of asteroids allowed in the game
        private int _initialMaxAsteroids = 50;
        private float _baseInterval = 2000f; // Minimum interval between asteroid spawns (in milliseconds)
        private float _intervalVariance = 1000f; // Range for random intervals
        private float _spawnRateMultiplier = 1.1f; // Multiplier for increasing the spawn rate over time
        private int _maxAsteroidsCapTime = 60; // Time after which we cap the maximum number of asteroids
        private float _nextWaveDelayIncrement = 500f; // Increment in base interval between waves
        private float _waveDifficultyIncreaseFactor = 1.1f; // Factor by which to increase the base interval as waves progress
        private float _asteroidGrowthFactor = 1.2f; // Factor by which to increase the initial spawn rate over time


        // TIMER CONSTANTS
        private float _initialSpawnTime = 10000f; // Time at which spawn rate starts increasing (in milliseconds)
        private float _nextWaveThreshold = 30000f; // Time after which waves become more frequent and asteroids harder to avoid
        private float _waveDelayAccelerationRate = 5000f; // Rate at which the interval between waves increases
        private bool _waveHappening = false;

        // BLANK CONSTRUCTOR
        AsteroidSpawning() { }

        private List<float> GenerateIntervals(int numAsteroids)
        {
            List<float> intervals = new List<float>();
            float lastIntervalEnd = 0; // Start at 0
            for (int i = 0; i < numAsteroids; i++)
            {
                // Generate a random interval based on the base interval and variance
                float intervalDuration = _baseInterval + SplashKit.Rnd((int)-_intervalVariance / 2, (int)_intervalVariance / 2);
                intervals.Add(lastIntervalEnd + intervalDuration); // last interval calculated plus this new interval
                lastIntervalEnd += intervalDuration; // add the most recent duration for next iteration
            }
            return intervals;
        }

        // PUBLIC METHODS

        public List<Asteroid> SpawnAsteroids(List<Asteroid> asteroids, Window gameWindow, Timer gameTimer, Timer asteroidTimer)
        {
            List<Asteroid> newAsteroids = new List<Asteroid>();

            float elapsedTime = (float)gameTimer.Ticks / 1000; // Convert ticks to seconds for easier handling

            // Calculate current spawn rate
            // spawn rate is a function of time and difficulty level
            // it is the minimum of either the max asteroids or the initial spawn rate multiplied by the spawn rate multiplier raised to the power of elapsed time
            // this rate is then the number of asteroids to spawn in this wave
            float currentRate = MathF.Min(_maxAsteroids, _initialSpawnRate * MathF.Pow(_spawnRateMultiplier, elapsedTime));
            int numAsteroidsToSpawn = SplashKit.Rnd(1, (int)currentRate);

            // Generate intervals for spawning these asteroids
            List<float> intervals = GenerateIntervals(numAsteroidsToSpawn);

            // If there's still available asteroids to spawn
            if (asteroids.Count() < _maxAsteroids)
            {
                if
                (
                    // If the asteroid timer has passed the next interval
                    asteroidTimer.Ticks > intervals.Last()
                )
                {
                    // Reset the timer
                    asteroidTimer.Reset();
                    // Spawn a new asteroid
                    int spawnX = SplashKit.Rnd(0, gameWindow.Width * 2); // Spawn randomly within the game window width
                    newAsteroids.Add(new Asteroid(spawnX, -10)); // Adjust the y-coordinate as needed to simulate falling asteroids
                }
            }

            // Adjust parameters for the next wave based on elapsed time and difficulty level
            if (elapsedTime > _nextWaveThreshold)
            {
                _baseInterval *= _waveDifficultyIncreaseFactor;
                _initialSpawnRate *= _asteroidGrowthFactor;
                _nextWaveThreshold += _nextWaveDelayIncrement * (1 + elapsedTime / _waveDelayAccelerationRate);

                // set new max asteroids
                int newMax = (int)((double)_initialMaxAsteroids * Math.Pow(elapsedTime, _maxAsteroidsCapTime));
                _maxAsteroids = (int)MathF.Min(_maxAsteroids, newMax);
            }

            return newAsteroids;
        }
    }
}