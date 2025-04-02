using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

string username = "here_its_me_only";
string password = "RainbowAbhisar@25";
string filePath = "C:\\Users\\abhis\\Desktop\\accounts.txt"; // Text file containing Instagram usernames

//// Initialize WebDriver
//ChromeOptions options = new ChromeOptions();
//options.AddArgument("--start-maximized"); // Open browser in maximized mode


// Initialize ChromeOptions for headless mode
ChromeOptions options = new ChromeOptions();
options.AddArgument("--headless"); // Run in headless mode (no GUI)
options.AddArgument("--disable-gpu"); // Recommended for headless mode
options.AddArgument("--window-size=1920,1080"); // Set window size
options.AddArgument("--no-sandbox"); // Bypass OS security model
options.AddArgument("--disable-dev-shm-usage"); // Overcome resource limits
options.AddArgument("--disable-blink-features=AutomationControlled"); // Avoid detection
IWebDriver driver = new ChromeDriver(options);

try
{
    // Navigate to Instagram login page
    driver.Navigate().GoToUrl("https://www.instagram.com/");
    Thread.Sleep(5000);

    // Login to Instagram
    driver.FindElement(By.Name("username")).SendKeys(username);
    driver.FindElement(By.Name("password")).SendKeys(password);
    driver.FindElement(By.Name("password")).SendKeys(Keys.Enter);
    Thread.Sleep(7000); // Wait for login to complete

    // Read usernames from file
    string[] accounts = File.ReadAllLines(filePath);

    foreach (string account in accounts)
    {
        BlockUser(driver, account);
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}
finally
{
    driver.Quit();
}

void BlockUser(IWebDriver driver, string username)
{
    try
    {
        // Navigate to the user's profile
        driver.Navigate().GoToUrl($"https://www.instagram.com/{username}/");
        Thread.Sleep(5000);

        // Locate and click on the three-dot menu (Options button)
        IWebElement optionsButton = driver.FindElement(By.XPath("//*[name()='svg' and @aria-label='Options']"));
        optionsButton.Click();
        Thread.Sleep(2000);

        // Click "Block" button
        IWebElement blockButton = driver.FindElement(By.XPath("//button[contains(text(), 'Block')]"));
        blockButton.Click();
        Thread.Sleep(2000);

        // Confirm blocking
        IWebElement confirmButton = driver.FindElement(By.XPath("(//button[contains(text(), 'Block')])[2]"));
        confirmButton.Click();
        Thread.Sleep(2000);

        Console.WriteLine($"Blocked: {username}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to block {username}: {ex.Message}");
    }
}