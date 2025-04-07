using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text.Json;

int count = 1;
#region Block
//string username = "here_its_me_only";
//string password = "RainbowAbhisar@25";
//string filePath = "G:\\Projects\\POCs\\InstagramAutomation\\Assets\\accounts.txt"; // Text file containing Instagram usernames
//string cookiesFile = "G:\\Projects\\POCs\\InstagramAutomation\\Assets\\cookies.json";
#endregion

#region Follow
string username = "c_h_o_k_o_007";
string password = "RainbowAbhisar@25";
string filePath = "G:\\Projects\\POCs\\InstagramAutomation\\Assets\\accounts_follow.txt"; // Text file containing Instagram usernames
string cookiesFile = "G:\\Projects\\POCs\\InstagramAutomation\\Assets\\cookies_follow.json";
#endregion

//BlockAccounts();
FollowAccounts();

void FollowAccounts()
{
    // Initialize WebDriver
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
        driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
        Thread.Sleep(5000);

        if (File.Exists(cookiesFile))
        {
            LoadCookies(driver, cookiesFile);
            driver.Navigate().Refresh();
            Thread.Sleep(3000);
        }

        if (!IsLoggedIn(driver))
        {
            Login(driver);
            SaveCookies(driver, cookiesFile);
        }

        // Read usernames from file
        string[] accounts = File.ReadAllLines(filePath);

        foreach (string account in accounts)
        {
            FollowUser(driver, account);
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
}

void BlockAccounts()
{
    // Initialize WebDriver
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
        driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
        Thread.Sleep(5000);

        if (File.Exists(cookiesFile))
        {
            LoadCookies(driver, cookiesFile);
            driver.Navigate().Refresh();
            Thread.Sleep(3000);
        }

        if (!IsLoggedIn(driver))
        {
            Login(driver);
            SaveCookies(driver, cookiesFile);
        }

        // Read usernames from file
        string[] accounts = File.ReadAllLines(filePath);

        foreach (string account in accounts)
        {
            //if (count > 100)
            //{
            //    break; // Limit to 100 accounts
            //}
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
}

void Login(IWebDriver driver)
{
    driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
    Thread.Sleep(5000);

    driver.FindElement(By.Name("username")).SendKeys(username);
    driver.FindElement(By.Name("password")).SendKeys(password);
    driver.FindElement(By.Name("password")).SendKeys(Keys.Enter);
    Thread.Sleep(7000); // Wait for login

    Console.WriteLine("Logged in and cookies saved.");
}

static bool IsLoggedIn(IWebDriver driver)
{
    return driver.Url.Contains("instagram.com") && !driver.Url.Contains("accounts/login");
}

static void SaveCookies(IWebDriver driver, string cookiesFile)
{
    var cookies = driver.Manage().Cookies.AllCookies;
    var serializableCookies = new List<SerializableCookie>();

    foreach (var cookie in cookies)
    {
        serializableCookies.Add(new SerializableCookie
        {
            Name = cookie.Name,
            Value = cookie.Value,
            Domain = cookie.Domain,
            Path = cookie.Path,
            Expiry = cookie.Expiry,
            Secure = cookie.Secure,
            HttpOnly = cookie.IsHttpOnly
        });
    }

    File.WriteAllText(cookiesFile, JsonSerializer.Serialize(serializableCookies));
}

static void LoadCookies(IWebDriver driver, string cookiesFile)
{
    var json = File.ReadAllText(cookiesFile);
    var serialized = JsonSerializer.Deserialize<List<SerializableCookie>>(json);

    driver.Navigate().GoToUrl("https://www.instagram.com/");

    foreach (var cookie in serialized)
    {
        try
        {
            var seleniumCookie = new Cookie(
                cookie.Name,
                cookie.Value,
                cookie.Domain,
                cookie.Path,
                cookie.Expiry
            );

            driver.Manage().Cookies.AddCookie(seleniumCookie);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding cookie: {ex.Message}");
        }
    }
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

        Console.WriteLine($"Blocked: {count}.{username}");
        count++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to block {username}: {ex.Message}");
    }
}

void FollowUser(IWebDriver driver, string username)
{
    try
    {
        // Navigate to the user's profile
        driver.Navigate().GoToUrl($"https://www.instagram.com/{username}/");
        Thread.Sleep(10000);

        var followButton = driver.FindElement(By.XPath("//button//div[text()='Follow']"));
        followButton.Click();
        Console.WriteLine($"Followed: {count}.{username}");
        count++;

        Thread.Sleep(5000);
    }
    catch (NoSuchElementException)
    {
        Console.WriteLine($"Already following or button not found: {username}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to follow {username}: {ex.Message}");
    }
}

class SerializableCookie
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Domain { get; set; }
    public string Path { get; set; }
    public DateTime? Expiry { get; set; }
    public bool Secure { get; set; }
    public bool HttpOnly { get; set; }
}
