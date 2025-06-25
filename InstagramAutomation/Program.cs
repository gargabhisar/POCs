using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Text.Json;

int count = 1;
#region Block
//string username = "here_its_me_only";
//string password = "RainbowAbhisar@25";
string filePath = "G:\\Projects\\POCs\\InstagramAutomation\\Assets\\accounts.txt"; // Text file containing Instagram usernames
//string cookiesFile = "G:\\Projects\\POCs\\InstagramAutomation\\Assets\\cookies.json";
#endregion

#region Follow
//string username = "c_h_o_k_o_007";
//string password = "RainbowAbhisar@25";
//string filePath = "G:\\Projects\\POCs\\InstagramAutomation\\Assets\\accounts_follow.txt"; // Text file containing Instagram usernames
//string cookiesFile = "G:\\Projects\\POCs\\InstagramAutomation\\Assets\\cookies_follow.json";
#endregion

#region UploadVideos
string username = "here_its_me_only";
string password = "RainbowAbhisar@25";
string videoFolder = "D:\\Reels\\420+ Reeels"; // Text file containing Instagram usernames
string cookiesFile = "G:\\Projects\\POCs\\InstagramAutomation\\Assets\\cookies.json";
int videoUploadCount = 1; // Counter for uploaded videos
#endregion

//BlockAccounts();
//FollowAccounts();
UploadVideos();

void UploadVideos()
{
    // Initialize WebDriver
    ChromeOptions options = new ChromeOptions();
    options.AddArgument("--start-maximized"); // Open browser in maximized mode

    //// Initialize ChromeOptions for headless mode
    //ChromeOptions options = new ChromeOptions();
    //options.AddArgument("--headless"); // Run in headless mode (no GUI)
    //options.AddArgument("--disable-gpu"); // Recommended for headless mode
    //options.AddArgument("--window-size=1920,1080"); // Set window size
    //options.AddArgument("--no-sandbox"); // Bypass OS security model
    //options.AddArgument("--disable-dev-shm-usage"); // Overcome resource limits
    //options.AddArgument("--disable-blink-features=AutomationControlled"); // Avoid detection
    //options.AddArgument("--enable-unsafe-swiftshader");

    using (var driver = new ChromeDriver(options))
    {
        // Navigate to Instagram login page
        driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
        Thread.Sleep(5000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

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

        var videoFiles = Directory.GetFiles(videoFolder, "*.*")
            .Where(f => f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var videoPath in videoFiles)
        {
            UploadVideo(driver, wait, videoPath);
            videoUploadCount++;
        }

        Console.WriteLine("All done!");
    }
}

void UploadVideo(IWebDriver driver, WebDriverWait wait, string videoPath)
{
    try
    {
        bool isLoaded = false;
        int maxAttempts = 5;
        int attempts = 0;
        Random rnd = new Random();

        do
        {
            try
            {
                WebDriverWait waitForInsta = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                waitForInsta.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[text()='Home']")));
                isLoaded = true;
            }
            catch (WebDriverTimeoutException)
            {
                attempts++;
                if (attempts >= maxAttempts)
                {
                    Console.WriteLine("Page did not load after maximum attempts. Stopping...");
                    break; // or throw new Exception("Page failed to load.");
                }

                driver.Navigate().Refresh();
            }
        }
        while (!isLoaded);

        // Click the “Create” button
        var createBtn = wait.Until(d => d.FindElement(By.XPath("//span[text()='Create']")));
        createBtn.Click();
        Thread.Sleep(rnd.Next(2000, 5000));

        var postBtn = wait.Until(d => d.FindElement(By.XPath("//span[text()='Post']")));
        postBtn.Click();
        Thread.Sleep(rnd.Next(2000, 5000));

        // ✅ Locate hidden file input (not the button)
        var fileInput = wait.Until(d => d.FindElement(By.XPath("//input[@type='file']")));
        fileInput.SendKeys(videoPath);
        Thread.Sleep(rnd.Next(10000, 15000));

        Console.WriteLine("Starting: " + Path.GetFileName(videoPath));

        var okButtons = driver.FindElements(By.XPath("//button[text()='OK']"));
        if (okButtons.Count > 0)
        {
            okButtons[0].Click();
            Thread.Sleep(rnd.Next(2000, 5000));
        }

        Thread.Sleep(rnd.Next(5000, 10000));

        var selectCropBtn = wait.Until(d => d.FindElement(By.XPath("//*[name()='svg' and @aria-label='Select crop']")));
        selectCropBtn.Click();
        Thread.Sleep(rnd.Next(2000, 5000));
        
        var selectRatioBtn = wait.Until(d => d.FindElement(By.XPath("//span[text()='9:16']")));
        selectRatioBtn.Click();
        Thread.Sleep(rnd.Next(2000, 5000));

        // Click Next (possibly twice)
        var nextBtn1 = wait.Until(d => d.FindElements(By.XPath("//div[text()='Next']/..")));
        foreach (var btn in nextBtn1) btn.Click();
        Thread.Sleep(rnd.Next(2000, 5000));

        // Click Next (possibly twice)
        var nextBtn2 = wait.Until(d => d.FindElements(By.XPath("//div[text()='Next']/..")));
        foreach (var btn in nextBtn2) btn.Click();
        Thread.Sleep(rnd.Next(2000, 5000));

        // Add caption
        List<string> hashtags = new List<string>
        {
            "#MotivationDaily", "#NeverGiveUp", "#YouCanDoIt", "#MindsetMatters", "#SuccessMindset",
            "#GrowthMindset", "#InspireOthers", "#DreamBigWorkHard", "#SelfBelief", "#DisciplineEqualsFreedom",
            "#KeepPushing", "#LevelUpMindset", "#MotivatedMindset", "#BetterEveryday", "#FocusOnYou",
            "#BuildYourDream", "#PositiveVibesOnly", "#QuoteOfTheDay", "#WakeUpWithPurpose", "#WordsToLiveBy", 
            "#ReelMotivation", "#ReelItFeelIt", "#ExplorePage", "#ViralReels", "#DailyInspiration", 
            "#FuelYourFire", "#InnerStrength", "#IGMotivation", "#MotivationReels", "#OwnYourStory", 
            "#PushThrough", "#StayHungry", "#RiseAndGrind", "#HustleHard", "#DailyGrind", 
            "#YouGotThis", "#SuccessDriven", "#ChaseYourDreams", "#MotivationNation", "#MindOverMatter", 
            "#GrindMode", "#PowerOfPositivity", "#NoExcuses", "#BelieveInYourself", "#WorkHardStayHumble", 
            "#HardWorkPaysOff", "#StayMotivated", "#MakeItHappen", "#StayInspired", "#BeUnstoppable", 
            "#AmbitionOnFleek", "#KeepGrinding", "#LimitlessMindset", "#DrivenToSucceed", "#FearlessPursuit", 
            "#RelentlessEffort", "#InnerDrive", "#SuccessFuel", "#WinnersMindset", "#ThinkPositive"
        };

        // Shuffle and take 15 randomly
        Random rng = new Random();
        List<string> selected = hashtags.OrderBy(x => rng.Next()).Take(15).ToList();

        // Join as space-separated string
        string result = string.Join(" ", selected);
        
        var captionArea = wait.Until(d => d.FindElement(By.XPath("//div[@aria-label='Write a caption...' and @contenteditable='true']")));
        captionArea.SendKeys("Motivation: " + Path.GetFileNameWithoutExtension(videoPath) + " " + result);
        Thread.Sleep(rnd.Next(2000, 5000));

        // Click Share
        var shareBtn = wait.Until(d => d.FindElement(By.XPath("//div[text()='Share']/..")));
        shareBtn.Click();

        // wait for upload
        WebDriverWait waitForUpload = new WebDriverWait(driver, TimeSpan.FromSeconds(100000));

        waitForUpload.Until(driver =>
        {
            try
            {
                waitForUpload.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath("//div[@aria-label='Sharing']")));
                return true;
            }
            catch
            {
                waitForUpload.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h3[text()='Your reel has been shared.']")));
                return true;
            }
        });

        //waitForUpload.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath("//div[@aria-label='Sharing']")));
        //waitForUpload.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h3[text()='Your reel has been shared.']")));
        Console.WriteLine("Uploaded: " + Path.GetFileName(videoPath));

        // ✅ Move uploaded file to 'Uploaded' subfolder
        string uploadedFolder = Path.Combine(Path.GetDirectoryName(videoPath), "Uploaded");
        if (!Directory.Exists(uploadedFolder))
        {
            Directory.CreateDirectory(uploadedFolder);
        }

        string destFilePath = Path.Combine(uploadedFolder, Path.GetFileName(videoPath));

        // Optional: avoid overwrite
        if (File.Exists(destFilePath))
        {
            File.Delete(destFilePath); // or rename with timestamp
        }

        File.Move(videoPath, destFilePath);
        Console.WriteLine(Path.GetFileName(videoPath) + ": Moved to Uploaded folder.");
        Console.WriteLine("Total Upload count: " + videoUploadCount); 
        Console.WriteLine("===========================================================");

        // Random delay
        int number = rnd.Next(20000, 50000);
        Thread.Sleep(number);

        // Refresh page
        driver.Navigate().Refresh();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to upload " + Path.GetFileName(videoPath) + ": " + ex.Message);
    }
}

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

    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
    DismissDialogs(driver, wait);

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

static void DismissDialogs(IWebDriver driver, WebDriverWait wait)
{
    try
    {
        wait.Until(d => d.FindElement(By.XPath("//button[text()='Not Now']"))).Click();
        Thread.Sleep(2000);
    }
    catch { }

    try
    {
        wait.Until(d => d.FindElement(By.XPath("//button[text()='Not Now']"))).Click();
        Thread.Sleep(2000);
    }
    catch { }
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
