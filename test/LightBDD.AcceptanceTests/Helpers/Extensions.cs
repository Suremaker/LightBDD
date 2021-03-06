using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace LightBDD.AcceptanceTests.Helpers
{
    public static class Extensions
    {
        public static IWebElement FindFeature(this ChromeDriver driver, int featureNo)
        {
            return FindFeatures(driver).ElementAt(featureNo - 1);
        }

        public static IEnumerable<IWebElement> FindFeatures(this ChromeDriver driver)
        {
            return driver.FindElementsByClassName("feature");
        }

        public static IEnumerable<IWebElement> FindAllScenarios(this ChromeDriver driver)
        {
            return driver.FindElementsByClassName("scenario");
        }

        public static IEnumerable<IWebElement> FindAllSteps(this ChromeDriver driver)
        {
            return driver.FindElementsByClassName("step");
        }

        public static IWebElement FindScenario(this IWebElement element, int scenarioNo)
        {
            return FindScenarios(element).ElementAt(scenarioNo - 1);
        }

        public static IEnumerable<IWebElement> FindScenarios(this IWebElement element)
        {
            return element.FindElements(By.ClassName("scenario"));
        }

        public static IEnumerable<IWebElement> FindSteps(this IWebElement element)
        {
            return element.FindElements(By.ClassName("step"));
        }

        public static string[] GetClassNames(this IWebElement element)
        {
            return element.GetAttribute("class").Split(' ');
        }

        public static bool HasClassName(this IWebElement element, string className)
        {
            return element.GetClassNames().Any(c => c == className);
        }

        public static IWebElement FindLabelByText(this ChromeDriver driver, string text)
        {
            return driver.FindElementsByTagName("label").Single(l => l.Text == text);
        }

        public static IWebElement FindLabelTarget(this ChromeDriver driver, IWebElement label)
        {
            return driver.FindElementById(label.GetAttribute("for"));
        }

        public static void ClickSync(this IWebElement element, ChromeDriver driver)
        {
            ExecuteSync(driver, element.Click);
        }

        private static void ExecuteSync(ChromeDriver driver, Action action)
        {
            var counter = driver.Synchronize();
            action();
            Repeat.Until(() => driver.Synchronize() > counter, "The synchronization counter has not been increased!");
        }

        /// <summary>
        /// Executes javascript in the browser (that would wait for pending operation to finish) and returns synchronization counter (that is incremented after each long operation is finished).
        /// </summary>
        public static long Synchronize(this ChromeDriver driver)
        {
            return (long)driver.ExecuteScript("return synchronizationCounter;");
        }

        public static void EnsurePageIsLoaded(this ChromeDriver driver)
        {
            Repeat.Until(() => driver.Synchronize() == 0, () => $"The synchronization counter should be 0, but was: {driver.Synchronize()}");
            Repeat.Until(() => (bool)driver.ExecuteScript("return initialized;"), "The page is not initialized.");
        }
    }
}