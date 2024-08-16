using System.Collections.Generic;
using SplashKitSDK;
using System;
using System.Linq;

namespace PlanetProtector
{
    class AsteroidSpawningCopy
    {
        // CONSTANTS
        private const float _INITIALSPAWNRATE = 1.0f; // Initial rate at which asteroids start spawning
        private const int _INITIALMAXASTEROIDS = 50;


        // SPAWN VARIABLES
        private float _maxAsteroids = _INITIALMAXASTEROIDS; // Maximum number of asteroids allowed in the game
        private float _maxAsteroidsGrowthRate = 1.2f; // Rate at which the maximum number of asteroids increases
        private float _spawnRateMultiplier = 1.1f; // Multiplier for increasing the spawn rate over time
        private float _spawnRate = _INITIALSPAWNRATE; // Current rate at which asteroids are spawning
        private Timer _asteroidTimer;


        // WAVE VARIABLES
        private Timer _waveTimer;
        private float _waveDifficultyIncreaseFactor = 1.1f; // Factor by which to increase the base interval as waves progress
        private float _nextWaveDelayIncrement = 500f; // Increment in base interval between waves
        private bool _waveHappening = false;
        // private float _waveDelayAccelerationRate = 5000f; // Rate at which the interval between waves increases
        private float _nextWaveTime; // Time at which the next wave will start
        private float _waveIntervalVariance = 1000f; // Range for random intervals
        private float _waveInterval = 5000f; // Base interval between waves


        // ASTEROID VARIABLES
        private float _asteroidIntervalVariance = 1000f; // Range for random intervals
        private float _asteroidInterval = 500f; // Minimum interval between asteroid spawns (in milliseconds)
        private float _asteroidIntervalDecreaseFactor = 0.9f; // Factor by which to decrease the interval between asteroids
        private List<float> _intervals = new List<float>(); // List of intervals for each asteroid spawn
        private float _currentWaveAsteroidsToSpawn = 0; // Number of asteroids to spawn in the current wave
        private int _currentIntervalIndex = 0; // Index of the current interval


        // CONSTRUCTOR
        AsteroidSpawningCopy() 
        {
            _waveTimer = new Timer("WaveTimer");
            _waveTimer.Start();

            _asteroidTimer = new Timer("AsteroidTimer");
            _asteroidTimer.Start();

            _nextWaveTime = SplashKit.Rnd((int)(_waveInterval - _waveIntervalVariance/2), (int)(_waveInterval + _waveIntervalVariance/2));
        }


        // PUBLIC METHODS
        public void Update(List<Asteroid> asteroids, Window gameWindow, Timer gameTimer)
        {
            if(
                !_waveHappening 
                && _waveTimer.Ticks > _nextWaveTime
            )
            {
                _waveHappening = true;
                GenerateNewWave(gameTimer);
            }

            if(_waveHappening)
            {
                SpawnAsteroids(asteroids, gameWindow, gameTimer);
            }

            // If the wave is over (all intervals have been 'used')
            if (_currentIntervalIndex >= _intervals.Count)
            {
                _currentIntervalIndex = 0; // Reset the interval index
                _asteroidInterval *= _asteroidIntervalDecreaseFactor; // Decrease the interval between asteroids
                _maxAsteroids *= _maxAsteroidsGrowthRate; // set new max asteroids
            }
        }


        // PRIVATE METHODS
        private void GenerateNewWave(Timer gameTimer)
        {
            _waveTimer.Reset();
            _waveTimer.Stop(); // set timer to 0 and stop

            float gameElapsedTime = (float)gameTimer.Ticks / 1000; // Convert ticks to seconds for easier handling

            // Calculate current spawn rate
            // spawn rate is a function of time and difficulty level
            // it is the minimum of either the max asteroids or the initial spawn rate multiplied by the spawn rate multiplier raised to the power of elapsed time
            // this rate is then the number of asteroids to spawn in this wave
            _spawnRate = MathF.Min(_maxAsteroids, _INITIALSPAWNRATE * MathF.Pow(_spawnRateMultiplier, gameElapsedTime));
            _currentWaveAsteroidsToSpawn =  SplashKit.Rnd( 10, (int)(_spawnRate*10) ) / 10; // multiply and divide by 10 to increase accuracy
            
            GenerateIntervals();
        }


        private List<Asteroid> SpawnAsteroids(List<Asteroid> asteroids, Window gameWindow, Timer gameTimer)
        {
            List<Asteroid> newAsteroids = new List<Asteroid>();

            
            if 
            (
                asteroids.Count() < _maxAsteroids // If there's still available asteroids to spawn
                && _asteroidTimer.Ticks > _intervals[_currentIntervalIndex] // If the asteroid timer has passed the next interval
            )
            {
                _asteroidTimer.Reset(); // Reset the timer
                _currentIntervalIndex++; // Move to the next interval

                // Spawn a new asteroid
                int spawnX = SplashKit.Rnd(0, gameWindow.Width * 2); // Spawn randomly within the game window width
                newAsteroids.Add(new Asteroid(spawnX, -10)); // Adjust the y-coordinate as needed to simulate falling asteroids
            }

            return newAsteroids;
        }


        private List<float> GenerateIntervals()
        {
            List<float> intervals = new List<float>();
            for (int i = 0; i < _currentWaveAsteroidsToSpawn; i++)
            {
                // Generate a random interval based on the base interval and variance
                float intervalDuration = _asteroidInterval + SplashKit.Rnd((int)(-_asteroidIntervalVariance / 2), (int)(_asteroidIntervalVariance / 2));
                intervals.Add(intervalDuration); // add new interval to list
            }
            return intervals;
        }


        private void ResetWave()
        {
            _waveTimer.Reset();
            _waveHappening = false;
            // Set the next wave time. Random time between the base interval and +/- variance
            _nextWaveTime = _waveInterval + SplashKit.Rnd((int)(-_waveIntervalVariance / 2), (int)(_waveIntervalVariance / 2));
        }
    }
}