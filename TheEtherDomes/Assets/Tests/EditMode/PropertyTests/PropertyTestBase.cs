using NUnit.Framework;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Base class for property-based tests.
    /// Provides helper methods and configuration for property testing.
    /// 
    /// Note: Unity doesn't support FsCheck directly via NuGet.
    /// We use NUnit's [Repeat] attribute to simulate property-based testing
    /// with random inputs generated in each iteration.
    /// 
    /// Requirements: 22.4
    /// </summary>
    public abstract class PropertyTestBase
    {
        /// <summary>
        /// Minimum iterations for property tests.
        /// </summary>
        public const int MIN_ITERATIONS = 100;

        /// <summary>
        /// Generate a random float in range.
        /// </summary>
        protected float RandomFloat(float min = 0f, float max = 1f)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// Generate a random int in range.
        /// </summary>
        protected int RandomInt(int min = 0, int max = 100)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// Generate a random ulong.
        /// </summary>
        protected ulong RandomULong(ulong min = 1, ulong max = 1000)
        {
            return (ulong)Random.Range((int)min, (int)max);
        }

        /// <summary>
        /// Generate a random Vector3.
        /// </summary>
        protected Vector3 RandomVector3(float min = -100f, float max = 100f)
        {
            return new Vector3(
                Random.Range(min, max),
                Random.Range(min, max),
                Random.Range(min, max)
            );
        }

        /// <summary>
        /// Generate a random position (Y >= 0).
        /// </summary>
        protected Vector3 RandomPosition(float range = 100f)
        {
            return new Vector3(
                Random.Range(-range, range),
                Random.Range(0f, range / 2f),
                Random.Range(-range, range)
            );
        }

        /// <summary>
        /// Generate a random bool.
        /// </summary>
        protected bool RandomBool()
        {
            return Random.value > 0.5f;
        }

        /// <summary>
        /// Generate a random string.
        /// </summary>
        protected string RandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[Random.Range(0, chars.Length)];
            }
            return new string(result);
        }

        /// <summary>
        /// Assert that a value is within a percentage tolerance of expected.
        /// </summary>
        protected void AssertWithinTolerance(float actual, float expected, float tolerancePercent, string message = "")
        {
            float tolerance = expected * tolerancePercent;
            Assert.That(actual, Is.InRange(expected - tolerance, expected + tolerance), message);
        }

        /// <summary>
        /// Assert that a probability approximates expected over many trials.
        /// </summary>
        protected void AssertProbabilityApproximates(int successes, int trials, float expectedProbability, float tolerance = 0.1f)
        {
            float actualRate = (float)successes / trials;
            Assert.That(actualRate, Is.InRange(expectedProbability - tolerance, expectedProbability + tolerance),
                $"Probability {actualRate:P1} should approximate {expectedProbability:P1} (±{tolerance:P0})");
        }

        /// <summary>
        /// Run a property test with custom generator.
        /// </summary>
        protected void RunPropertyTest<T>(System.Func<T> generator, System.Action<T> test, int iterations = MIN_ITERATIONS)
        {
            for (int i = 0; i < iterations; i++)
            {
                T input = generator();
                test(input);
            }
        }

        /// <summary>
        /// Run a property test with two inputs.
        /// </summary>
        protected void RunPropertyTest<T1, T2>(
            System.Func<T1> generator1, 
            System.Func<T2> generator2, 
            System.Action<T1, T2> test, 
            int iterations = MIN_ITERATIONS)
        {
            for (int i = 0; i < iterations; i++)
            {
                T1 input1 = generator1();
                T2 input2 = generator2();
                test(input1, input2);
            }
        }
    }
}
